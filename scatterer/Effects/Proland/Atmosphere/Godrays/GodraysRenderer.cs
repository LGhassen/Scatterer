using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace scatterer
{
	public class GodraysRenderer : MonoBehaviour
	{
		SkyNode parentSkyNode;
		Camera targetCamera;
		GameObject targetLightGO;

		ComputeBuffer inverseShadowMatricesBuffer;
		ComputeShader inverseShadowMatricesComputeShader;

		CommandBuffer shadowVolumeCB;
		Material volumeDepthMaterial;
		GameObject volumeDepthGO;
		public RenderTexture volumeDepthTexture;

		GameObject cloudShadowGO;
		MeshRenderer cloudShadowMR;
		Dictionary<ShadowMapPass, List<CommandBuffer>> shadowRenderCommandBuffers = new Dictionary<ShadowMapPass, List<CommandBuffer>> ();
		List<Tuple<EVEClouds2d,Material>> cloudsShadowsMaterials = new List<Tuple<EVEClouds2d, Material>>();
		RenderTexture cloudShadowMap;
		CommandBuffer clearShadowMapCB;

		bool commandBufferAdded = false;
		
		public GodraysRenderer ()
		{

		}

		public bool Init(Light inputLight, SkyNode inputParentSkyNode)
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

			targetLightGO = inputLight.gameObject;
			parentSkyNode = inputParentSkyNode;

			Utils.EnableOrDisableShaderKeywords (volumeDepthMaterial, "DUAL_DEPTH_ON", "DUAL_DEPTH_OFF", (Scatterer.Instance.unifiedCameraMode && Scatterer.Instance.mainSettings.terrainShadows && (Scatterer.Instance.mainSettings.unifiedCamShadowsDistance > 8000f)));
			Utils.EnableOrDisableShaderKeywords (volumeDepthMaterial, "OCEAN_INTERSECT_ON", "OCEAN_INTERSECT_OFF", parentSkyNode.m_manager.hasOcean && Scatterer.Instance.mainSettings.useOceanShaders);
			volumeDepthMaterial.SetFloat ("Rt", parentSkyNode.Rt);
			volumeDepthMaterial.SetFloat ("Rg", parentSkyNode.Rg);
			Utils.EnableOrDisableShaderKeywords(volumeDepthMaterial, "CLOUDSMAP_ON", "CLOUDSMAP_OFF", false);

			volumeDepthGO = new GameObject ("GodraysVolumeDepth "+inputParentSkyNode.m_manager.parentCelestialBody.name);
			volumeDepthMaterial.renderQueue = 2999; //for debugging only
			volumeDepthGO.layer = 15;

			MeshFilter _mf = volumeDepthGO.AddComponent<MeshFilter> ();
			_mf.mesh.Clear ();			
			_mf.mesh = MeshFactory.MakePlane32BitIndexFormat (512, 512, MeshFactory.PLANE.XY, false, false); //fixed with 32bit indices
				//_mf.mesh = MeshFactory.MakePlane16BitIndexFormat (256, 256, MeshFactory.PLANE.XY, false, false); //Definitely need to subdivide more here and in the tesselation check edge length and culling			
			_mf.mesh.bounds = new Bounds (Vector4.zero, new Vector3 (1e18f, 1e18f, 1e18f)); //apparently mathf.Infinity is bad, remember this
			
			MeshRenderer _mr = volumeDepthGO.AddComponent<MeshRenderer> ();
			_mr.material = volumeDepthMaterial;
			_mr.receiveShadows = false;
			_mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_mr.enabled = false;

			volumeDepthTexture = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.RHalf); //seems to work if we divide the contents by 100, to keep under half's 65000 limit
			volumeDepthTexture.useMipMap = false;
			volumeDepthTexture.antiAliasing = 1;
			volumeDepthTexture.filterMode = FilterMode.Point;
			volumeDepthTexture.Create ();

			if (Scatterer.Instance.mainSettings.integrateEVECloudsGodrays)
			{
				InitCloudShadowCommandBuffers ();
			}

			//world to shadow matrices aren't exposed in the C# api so we can't compute the shadow to world matrices from them
			//since they are exposed in shaders we use a compute shader to do it
			inverseShadowMatricesBuffer = new ComputeBuffer (4, 16 * sizeof(float));

			shadowVolumeCB = new CommandBuffer();
			shadowVolumeCB.SetComputeBufferParam (inverseShadowMatricesComputeShader, 0, "resultBuffer", inverseShadowMatricesBuffer);
			shadowVolumeCB.DispatchCompute(inverseShadowMatricesComputeShader, 0, 4, 1, 1);

			shadowVolumeCB.SetRenderTarget(volumeDepthTexture);
			shadowVolumeCB.ClearRenderTarget(false, true, Color.black, 1f);
			shadowVolumeCB.DrawRenderer (_mr, volumeDepthMaterial);

			targetCamera = gameObject.GetComponent<Camera> ();

			return true;
		}

		void InitCloudShadowCommandBuffers ()
		{
			if (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey (parentSkyNode.m_manager.parentCelestialBody.name))
			{
				foreach (EVEClouds2d clouds2d in Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [parentSkyNode.m_manager.parentCelestialBody.name])
				{
					if (clouds2d.CloudShadowMaterial != null)
					{
						Material cloudShadowDepthMaterial = new Material (ShaderReplacer.Instance.LoadedShaders ["Scatterer-EVE/CloudShadowMap"]);
						cloudShadowDepthMaterial.CopyKeywordsFrom (clouds2d.CloudShadowMaterial);
						if (cloudShadowGO == null)
						{
							cloudShadowGO = GameObject.CreatePrimitive (PrimitiveType.Sphere);
							GameObject.Destroy (cloudShadowGO.GetComponent<Collider> ());
							MeshFilter cloudShadowMF = cloudShadowGO.GetComponent<MeshFilter> ();
							cloudShadowMF.mesh.Clear ();
							cloudShadowMF.mesh = IcoSphere.CreateIcoSphereMesh ();
							cloudShadowMR = cloudShadowGO.GetComponent<MeshRenderer> ();
							cloudShadowGO.transform.parent = parentSkyNode.m_manager.parentCelestialBody.transform;
							cloudShadowMR.receiveShadows = false;
							cloudShadowMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
							cloudShadowMR.enabled = false;

							cloudShadowMap = new RenderTexture (2048, 2048, 0, RenderTextureFormat.RHalf); //replace size after
							cloudShadowMap.useMipMap = false;
							cloudShadowMap.antiAliasing = 1;
							cloudShadowMap.filterMode = FilterMode.Point;
							cloudShadowMap.Create ();

							clearShadowMapCB = new CommandBuffer();
							clearShadowMapCB.SetRenderTarget(cloudShadowMap);
							clearShadowMapCB.ClearRenderTarget(false,true,Color.black);

							shadowRenderCommandBuffers[ShadowMapPass.DirectionalCascade0] = new List<CommandBuffer>();
							shadowRenderCommandBuffers[ShadowMapPass.DirectionalCascade0].Add(clearShadowMapCB);

							Utils.EnableOrDisableShaderKeywords(volumeDepthMaterial, "CLOUDSMAP_ON", "CLOUDSMAP_OFF", true);
							volumeDepthMaterial.SetTexture("cloudShadowMap", cloudShadowMap);
						}
						RenderObjectInCustomCascade (targetLightGO.GetComponent<Light> (), cloudShadowMap, cloudShadowMR, cloudShadowDepthMaterial, ShadowMapPass.DirectionalCascade0, 0f, 0f, 0.5f, 0.5f);
						RenderObjectInCustomCascade (targetLightGO.GetComponent<Light> (), cloudShadowMap, cloudShadowMR, cloudShadowDepthMaterial, ShadowMapPass.DirectionalCascade1, 0.5f, 0f, 0.5f, 0.5f);
						RenderObjectInCustomCascade (targetLightGO.GetComponent<Light> (), cloudShadowMap, cloudShadowMR, cloudShadowDepthMaterial, ShadowMapPass.DirectionalCascade2, 0f, 0.5f, 0.5f, 0.5f);
						RenderObjectInCustomCascade (targetLightGO.GetComponent<Light> (), cloudShadowMap, cloudShadowMR, cloudShadowDepthMaterial, ShadowMapPass.DirectionalCascade3, 0.5f, 0.5f, 0.5f, 0.5f);

						cloudsShadowsMaterials.Add (new Tuple<EVEClouds2d, Material> (clouds2d, cloudShadowDepthMaterial));
					}
				}
			}
		}
		
		void RenderObjectInCustomCascade(Light targetLight, RenderTexture rt, MeshRenderer mr, Material mat , ShadowMapPass passMask, float startX, float startY, float width, float height)
		{
			CommandBuffer cloudShadowDepthCB = new CommandBuffer();
			Rect cascadeRect = new Rect ((int)(startX * rt.width), (int)(startY * rt.height), (int)(width * rt.width), (int)(height * rt.height));

			cloudShadowDepthCB.SetRenderTarget(rt);
			cloudShadowDepthCB.EnableScissorRect(cascadeRect);
			cloudShadowDepthCB.SetViewport(cascadeRect);
			cloudShadowDepthCB.DrawRenderer(mr, mat);
			cloudShadowDepthCB.DisableScissorRect();

			if (shadowRenderCommandBuffers.ContainsKey (passMask))
			{
				shadowRenderCommandBuffers[passMask].Add(cloudShadowDepthCB);
			}
			else
			{
				shadowRenderCommandBuffers[passMask] = new List<CommandBuffer>();
				shadowRenderCommandBuffers[passMask].Add(cloudShadowDepthCB);
			}
		}

		public void EnableRenderingForFrame()
		{
			if (!commandBufferAdded)
			{
				ShadowMapCopier.Instance.RequestShadowMapCopy();

				targetCamera.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, shadowVolumeCB);

				Light targetLight = targetLightGO.GetComponent<Light> ();

				foreach (ShadowMapPass pass in shadowRenderCommandBuffers.Keys)
				{
					foreach (CommandBuffer cb in shadowRenderCommandBuffers[pass])
					{
						targetLight.AddCommandBuffer (LightEvent.AfterShadowMapPass, cb, pass);
					}
				}

				commandBufferAdded = true;
			}
		}

		public void RenderingDone()
		{
			if (commandBufferAdded)
			{
				targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, shadowVolumeCB);
				
				Light targetLight = targetLightGO.GetComponent<Light> ();
				
				foreach (ShadowMapPass pass in shadowRenderCommandBuffers.Keys) {
					foreach (CommandBuffer cb in shadowRenderCommandBuffers[pass]) {
						targetLight.RemoveCommandBuffer (LightEvent.AfterShadowMapPass, cb);
					}
				}
				commandBufferAdded = false;
			}

		}

		void OnPreCull()
		{
			if (!ReferenceEquals(parentSkyNode,null) && !parentSkyNode.inScaledSpace)
			{
				EnableRenderingForFrame();

				volumeDepthMaterial.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, targetCamera.cameraToWorldMatrix);
				volumeDepthMaterial.SetTexture(ShaderProperties._ShadowMapTextureCopyScatterer_PROPERTY, ShadowMapCopy.RenderTexture);
				volumeDepthMaterial.SetBuffer("inverseShadowMatricesBuffer", inverseShadowMatricesBuffer);

				//Calculate light's bounding Box englobing camera's frustum up to the shadows distance
				//The idea is to create a "projector" from the light's PoV, which is essentially a bounding box cover the whole range from near clip plance to the shadows, so we can project into the scene
				Vector3 lightDirForward = targetLightGO.transform.forward.normalized;
				Vector3 lightDirRight = targetLightGO.transform.right.normalized;
				Vector3 lightDirUp = targetLightGO.transform.up.normalized;
				
				Vector3[] frustumCornersNear = new Vector3[4];
				Vector3[] frustumCornersFar = new Vector3[4];
				
				//the corners appear to be in camera Space even though the documentation says in world Space
				targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), targetCamera.nearClipPlane, Camera.MonoOrStereoscopicEye.Mono, frustumCornersNear);
				targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), QualitySettings.shadowDistance, Camera.MonoOrStereoscopicEye.Mono, frustumCornersFar);
				
				//now, calculate the corners positions in light Space
				List<Vector3> frustumCornersInLightSpace = new List<Vector3>();
				
				foreach(Vector3 corner in frustumCornersNear)
				{
					frustumCornersInLightSpace.Add(targetLightGO.transform.worldToLocalMatrix.MultiplyPoint(targetCamera.transform.localToWorldMatrix.MultiplyPoint(corner)));
				}
				
				foreach(Vector3 corner in frustumCornersFar)
				{
					frustumCornersInLightSpace.Add(targetLightGO.transform.worldToLocalMatrix.MultiplyPoint(targetCamera.transform.localToWorldMatrix.MultiplyPoint(corner)));
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
				Matrix4x4 VP = shadowProjectionMatrix * targetLightGO.transform.worldToLocalMatrix;
				
				//And inverse transformation from "shadow space" into world used to create our mesh
				Matrix4x4 lightToWorld = VP.inverse;

				volumeDepthMaterial.SetMatrix(ShaderProperties.lightToWorld_PROPERTY, lightToWorld);
				volumeDepthMaterial.SetVector(ShaderProperties.lightDirection_PROPERTY, targetLightGO.transform.forward);

				volumeDepthMaterial.SetFloat(ShaderProperties._experimentalAtmoScale_PROPERTY,parentSkyNode.experimentalAtmoScale);
				volumeDepthMaterial.SetVector (ShaderProperties._planetPos_PROPERTY, parentSkyNode.parentLocalTransform.position);

				foreach(Tuple<EVEClouds2d, Material> tuple in cloudsShadowsMaterials)
				{
					tuple.Item2.CopyPropertiesFromMaterial(tuple.Item1.CloudShadowMaterial);
					tuple.Item2.SetVector(ShaderProperties.lightDirection_PROPERTY, targetLightGO.transform.forward);
					tuple.Item2.SetFloat(ShaderProperties._godrayCloudThreshold_PROPERTY, parentSkyNode.godrayCloudAlphaThreshold);
				}
			}
		}

		public void OnPostRender()
		{
			if (!ReferenceEquals(parentSkyNode,null))
			{
				RenderingDone ();
			}
		}
		
		public void Cleanup()
		{
			RenderingDone ();

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

			if (!ReferenceEquals (cloudShadowGO, null))
			{
				DestroyImmediate(cloudShadowGO);
			}
		}
	}
}

