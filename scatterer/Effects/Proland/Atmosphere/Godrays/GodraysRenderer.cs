using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace scatterer
{
	//Class to initialize the godray settings, commandBuffers, rendertexture and create the mesh
	public class GodraysRenderer : MonoBehaviour
	{
		public Material volumeDepthMaterial;
		public Camera targetCamera;
		public ComputeShader inverseShadowMatricesComputeShader;
		public GameObject targetLight;
		
		public RenderTexture volumeDepthTexture;

		GameObject volumeDepthGO;
		ComputeBuffer inverseShadowMatricesBuffer;
		CommandBuffer shadowVolumeCB;
		ShadowMapCopyCommandBuffer shadowMapCopier;

		//Does it need to be added to a camera? I think so, for OnPreCull to work
		public GodraysRenderer ()
		{

		}

		public bool Init(Light inputLight) //or init or whatever
		{
			if (!SystemInfo.supportsComputeShaders)
			{
				Utils.LogError("Compute shaders not supported, godrays can't be added");
				return false;
			}

			if (ShaderReplacer.Instance.LoadedComputeShaders.ContainsKey ("ComputeInverseShadowMatrices"))
			{
				inverseShadowMatricesComputeShader = ShaderReplacer.Instance.LoadedComputeShaders ["ComputeInverseShadowMatrices"];
			}
			else
			{
				Utils.LogError("Godrays inverse shadow matrices compute shader can't be found, godrays can't be added");
				return false;
			}

			if (ShaderReplacer.Instance.LoadedShaders.ContainsKey ("Scatterer/VolumeDepth"))
			{
				volumeDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders ["Scatterer/VolumeDepth"]);
			}
			else
			{
				Utils.LogError("Godrays depth shader can't be found, godrays can't be added");
				return false;
			}

			if (ReferenceEquals (inputLight, null))
			{
				Utils.LogError("Godrays light is null, godrays can't be added");
				return false;
			}

			targetLight = inputLight.gameObject;

			if (Scatterer.Instance.unifiedCameraMode && Scatterer.Instance.mainSettings.terrainShadows && (Scatterer.Instance.mainSettings.unifiedCamShadowsDistance > 8000f))
			{
				Utils.EnableOrDisableShaderKeywords (volumeDepthMaterial, "DUAL_DEPTH_ON", "DUAL_DEPTH_OFF", true);
				//volumeDepthMaterial.SetTexture("AdditionalDepthBuffer", Scatterer.Instance.partialUnifiedCameraDepthBuffer.depthTexture); //no need it's set as global
			}
			else
			{
				Utils.EnableOrDisableShaderKeywords(volumeDepthMaterial, "DUAL_DEPTH_ON", "DUAL_DEPTH_OFF", false);
			}


			volumeDepthGO = new GameObject ("GodraysVolumeDepth");
			MeshFilter _mf = volumeDepthGO.AddComponent<MeshFilter> ();
			_mf.mesh.Clear ();			
			_mf.mesh = MeshFactory.MakePlane32BitIndexFormat (512, 512, MeshFactory.PLANE.XY, false, false); //fixed with 32bit indices
				//_mf.mesh = MeshFactory.MakePlane16BitIndexFormat (256, 256, MeshFactory.PLANE.XY, false, false); //Definitely need to subdivide more here and in the tesselation check edge length and culling			
			_mf.mesh.bounds = new Bounds (Vector4.zero, new Vector3 (1e18f, 1e18f, 1e18f)); //apparently mathf.Infinity is bad, remember this
			
			MeshRenderer _mr = volumeDepthGO.AddComponent<MeshRenderer> ();
			_mr.sharedMaterial = volumeDepthMaterial;
			_mr.receiveShadows = false;
			_mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_mr.enabled = false;
			
			volumeDepthTexture = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.RFloat); //check if we can do half precision
			//volumeDepthTexture = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.RHalf); //seems to cause issues, seems like the max value a half can be is 65000 (from insight)
			volumeDepthTexture.useMipMap = false;
			volumeDepthTexture.antiAliasing = 1; //no need, the depth makes it naturally soft
			volumeDepthTexture.filterMode = FilterMode.Point;
			volumeDepthTexture.Create ();
			
			inverseShadowMatricesBuffer = new ComputeBuffer (4, 16 * sizeof(float));
			inverseShadowMatricesComputeShader.SetBuffer (0, "resultBuffer", inverseShadowMatricesBuffer);

			shadowVolumeCB = new CommandBuffer();
			shadowVolumeCB.DispatchCompute(inverseShadowMatricesComputeShader, 0, 4, 1, 1);
			
			shadowVolumeCB.SetGlobalBuffer("inverseShadowMatricesBuffer", inverseShadowMatricesBuffer); //here we set the matrices buffer for our runtime shader
			
			shadowVolumeCB.SetRenderTarget(volumeDepthTexture);
			shadowVolumeCB.ClearRenderTarget(false, true, Color.black, 1f);
			shadowVolumeCB.DrawRenderer (_mr, volumeDepthMaterial);

			targetCamera = gameObject.GetComponent<Camera> ();
			targetCamera.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, shadowVolumeCB);

			shadowMapCopier = (ShadowMapCopyCommandBuffer) targetLight.gameObject.AddComponent (typeof(ShadowMapCopyCommandBuffer));

			//Still need to add a parameter to pass the second higher precision depth texture to shader (if needed though)

			return true;
		}


		public void Enable()
		{
			shadowMapCopier.Enable ();
			targetCamera.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, shadowVolumeCB);
		}

		public void Disable() //to disable when in scaledSpace
		{
			shadowMapCopier.Disable ();
			targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, shadowVolumeCB);
		}

		void OnPreCull()
		{
			//if (volumeDepthMaterial != null && targetCamera != null && targetLight != null) //shouldn't be needed right?
			{
				volumeDepthMaterial.SetMatrix("CameraToWorld", targetCamera.cameraToWorldMatrix);
				
				//Calculate light's bounding Box englobing camera's frustum up to the shadows distance
				//The idea is to create a "projector" from the light's PoV, which is essentially a bounding box cover the whole range from near clip plance to the shadows, so we can project into the scene
				Vector3 lightDirForward = targetLight.transform.forward.normalized;
				Vector3 lightDirRight = targetLight.transform.right.normalized;
				Vector3 lightDirUp = targetLight.transform.up.normalized;
				
				Vector3[] frustumCornersNear = new Vector3[4];
				Vector3[] frustumCornersFar = new Vector3[4];
				
				//the corners appear to be in camera Space even though the documentation says in world Space
				targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), targetCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCornersNear);
				targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), QualitySettings.shadowDistance, Camera.MonoOrStereoscopicEye.Mono, frustumCornersFar);
				
				//now, calculate the corners positions in light Space
				List<Vector3> frustumCornersInLightSpace = new List<Vector3>();
				
				foreach(Vector3 corner in frustumCornersNear)
				{
					frustumCornersInLightSpace.Add(targetLight.transform.worldToLocalMatrix.MultiplyPoint(targetCamera.transform.localToWorldMatrix.MultiplyPoint(corner)));
				}
				
				foreach(Vector3 corner in frustumCornersFar)
				{
					frustumCornersInLightSpace.Add(targetLight.transform.worldToLocalMatrix.MultiplyPoint(targetCamera.transform.localToWorldMatrix.MultiplyPoint(corner)));
				}

				Bounds bounds = GeometryUtility.CalculateBounds(frustumCornersInLightSpace.ToArray(), Matrix4x4.identity);
				
				//Create an orthogonal projection matrix from the bounding box
				Matrix4x4 shadowProjectionMatrix = Matrix4x4.Ortho(
					bounds.center.x-bounds.extents.x,
					bounds.center.x+bounds.extents.x,
					
					bounds.center.y-bounds.extents.y,
					bounds.center.y+bounds.extents.y,
					
					bounds.center.z-bounds.extents.z,
					bounds.center.z+bounds.extents.z);
				
				shadowProjectionMatrix = GL.GetGPUProjectionMatrix(shadowProjectionMatrix, false);
				
				//Transformation from world into our "shadow space" matrix
				Matrix4x4 VP = shadowProjectionMatrix * targetLight.transform.worldToLocalMatrix;
				
				//And inverse transformation from "shadow space" into world used to create our mesh
				Matrix4x4 lightToWorld = VP.inverse;

				//make sure to rewrite these with shader properties
				volumeDepthMaterial.SetMatrix("lightToWorld", lightToWorld);
				volumeDepthMaterial.SetVector("lightDirection", targetLight.transform.forward);
				volumeDepthMaterial.SetVector("cameraForwardDir", targetCamera.transform.forward);

			}
		}
		
		public void OnDestroy()
		{
			Disable ();

			if (!ReferenceEquals (inverseShadowMatricesBuffer, null))
			{
				inverseShadowMatricesBuffer.Dispose();
			}

			if (!ReferenceEquals (volumeDepthTexture, null))
			{
				volumeDepthTexture.Release();
			}

			if (!ReferenceEquals (volumeDepthGO, null))
			{
				DestroyImmediate(volumeDepthGO);
			}
		}
	}
}

