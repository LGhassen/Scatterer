using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace Scatterer
{
	public class RaymarchedGodraysRenderer : MonoBehaviour
	{
		private SkyNode parentSkyNode;
		private Camera targetCamera;
		private Light targetLight;

		private Material scatteringOcclusionMaterial, downscaleDepthMaterial;
		Mesh mesh;

		private int screenWidth, screenHeight;
		private int renderWidth, renderHeight;

		// This is indexed by [isRightEye][flip]
		private FlipFlop<FlipFlop<RenderTexture>> godraysRT, depthRT;
		private bool useFlipBuffer = true;

		private RenderTexture downscaledDepth;

		// Indexed by isRightEye
		private FlipFlop<CommandBuffer> godraysCommandBuffer;
		private FlipFlop<Matrix4x4> previousV;
		private FlipFlop<Matrix4x4> previousP;

		private Vector3d previousParentPosition = Vector3d.zero;

		private bool renderingEnabled = false;

		private bool hasOcean = false;
		private bool useCloudGodrays = true;
		private bool useTerrainGodrays = false;
		private int stepCount = 50;

		public RaymarchedGodraysRenderer()
		{

		}

		public bool Init(Light inputLight, SkyNode inputParentSkyNode, bool useCloudGodrays, bool useTerrainGodrays, int stepCount)
        {
            if (ShaderReplacer.Instance.LoadedShaders.ContainsKey("Scatterer/RaymarchScatteringOcclusion")) // TODO: change this to not duplicate the key
            {
                scatteringOcclusionMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/RaymarchScatteringOcclusion"]);
            }
            else
            {
                Utils.LogError("Godrays Scattering Occlusion shader can't be found, godrays can't be added");
                return false;
            }

            downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/DownscaleDepth")]);

            if (!inputLight)
            {
                Utils.LogError("Godrays light is null, godrays can't be added");
                return false;
            }

            targetLight = inputLight;
            parentSkyNode = inputParentSkyNode;

            hasOcean = parentSkyNode.prolandManager.hasOcean && Scatterer.Instance.mainSettings.useOceanShaders;

            this.useCloudGodrays = useCloudGodrays;
            this.useTerrainGodrays = useTerrainGodrays;
            this.stepCount = stepCount;

            SetStepCountAndKeywords(scatteringOcclusionMaterial);

            scatteringOcclusionMaterial.SetTexture("StbnBlueNoise", ShaderReplacer.stbn);
            scatteringOcclusionMaterial.SetVector("stbnDimensions", new Vector3(ShaderReplacer.stbnDimensions.x, ShaderReplacer.stbnDimensions.y, ShaderReplacer.stbnDimensions.z));

            targetCamera = gameObject.GetComponent<Camera>();

            bool supportVR = VRUtils.VREnabled();

            if (supportVR)
            {
                VRUtils.GetEyeTextureResolution(out screenWidth, out screenHeight);
            }
            else
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }

			// Terrain godrays are higher frequency and need higher resolution to avoid artifacts and aliasing, cloud godrays are fine with lower
			if (useTerrainGodrays)
			{
				renderWidth = screenWidth   / 4;
				renderHeight = screenHeight / 2;
			}
			else
            {
				renderWidth  = screenWidth  / 4;
				renderHeight = screenHeight / 4;
			}

			godraysRT = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
            depthRT = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.RFloat, FilterMode.Point); // not sure if float helps here?

            downscaledDepth = RenderTextureUtils.CreateRenderTexture(renderWidth, renderHeight, RenderTextureFormat.RFloat, false, FilterMode.Point);

            godraysCommandBuffer = new FlipFlop<CommandBuffer>(VRUtils.VREnabled() ? new CommandBuffer() : null, new CommandBuffer());

            GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Quad);

            mesh = Instantiate(tempGO.GetComponent<MeshFilter>().mesh);

            GameObject.DestroyImmediate(tempGO);

            return true;
        }

        public void SetStepCountAndKeywords(Material mat)
        {
			mat.SetFloat("godraysStepCount", stepCount);

			mat.EnableKeyword("GODRAYS_RAYMARCHED");
			mat.DisableKeyword("GODRAYS_OFF");
			mat.DisableKeyword("RAYMARCHED_GODRAYS_OFF");
			mat.DisableKeyword("GODRAYS_LEGACY");

			if (useCloudGodrays && useTerrainGodrays)
            {
				mat.EnableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
				mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
				mat.DisableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
			}
            else if (useCloudGodrays)
            {
				mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
				mat.EnableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
				mat.DisableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
			}
            else if (useTerrainGodrays)
            {
				mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
				mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
				mat.EnableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
			}
		}

        void OnPreRender()
        {
			if (parentSkyNode && !parentSkyNode.inScaledSpace)
			{
				renderingEnabled = true;

				// volumeDepthMaterial.SetTexture(ShaderProperties._ShadowMapTextureCopyScatterer_PROPERTY, ShadowMapCopy.RenderTexture);

				// TODO: remove unneded calls?
				parentSkyNode.InitUniforms(scatteringOcclusionMaterial);
				parentSkyNode.SetUniforms(scatteringOcclusionMaterial);
				parentSkyNode.UpdatePostProcessMaterialUniforms(scatteringOcclusionMaterial);

				int frame = Time.frameCount % ShaderReplacer.stbnDimensions.z;
				scatteringOcclusionMaterial.SetFloat("frameNumber", frame);

				scatteringOcclusionMaterial.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, targetCamera.cameraToWorldMatrix);

				scatteringOcclusionMaterial.SetVector(ShaderProperties._planetPos_PROPERTY, parentSkyNode.parentLocalTransform.position); // check if needed

				bool isRightEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right;

				CommandBuffer commandBuffer = godraysCommandBuffer[isRightEye];

				commandBuffer.Clear();

				// first downscale depth to 1/4
				// TODO: shader property
				commandBuffer.GetTemporaryRT(Shader.PropertyToID("tempGodraysDepthDownscale"), screenWidth / 2, screenHeight / 2, 0, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Default);

				if (hasOcean)
					commandBuffer.Blit(null, Shader.PropertyToID("tempGodraysDepthDownscale"), downscaleDepthMaterial, 3);      //ocean depth buffer downsample
				else
					commandBuffer.Blit(null, Shader.PropertyToID("tempGodraysDepthDownscale"), downscaleDepthMaterial, 0);      //default depth buffer downsample

				// then downscale again to 1/16 or 1/8
				commandBuffer.SetGlobalTexture("tempGodraysDepthDownscale", Shader.PropertyToID("tempGodraysDepthDownscale"));

				if (downscaledDepth.height == screenHeight / 4)
					commandBuffer.Blit(null, downscaledDepth, downscaleDepthMaterial, 4);
				else
					commandBuffer.Blit(null, downscaledDepth, downscaleDepthMaterial, 5);

				commandBuffer.ReleaseTemporaryRT(Shader.PropertyToID("tempGodraysDepthDownscale"));

				scatteringOcclusionMaterial.SetTexture("downscaledDepth", downscaledDepth);
				scatteringOcclusionMaterial.SetTexture("historyGodrayOcclusionBuffer", godraysRT[isRightEye][!useFlipBuffer]);
				scatteringOcclusionMaterial.SetTexture("historyGodrayDepthBuffer", depthRT[isRightEye][!useFlipBuffer]);

				var prevV = previousV[isRightEye];

				// Add the frame to frame offset of the parent body, this contains both the movement of the body and the floating origin
				Vector3d currentOffset = parentSkyNode.parentLocalTransform.position - previousParentPosition;
				previousParentPosition = parentSkyNode.parentLocalTransform.position;

				//transform to camera space
				var currentV = VRUtils.GetViewMatrixForCamera(targetCamera);
				Vector3 floatOffset = currentV.MultiplyVector(-currentOffset);

				//inject in the previous view matrix
				prevV.m03 += floatOffset.x;
				prevV.m13 += floatOffset.y;
				prevV.m23 += floatOffset.z;

				var prevP = previousP[isRightEye];

				scatteringOcclusionMaterial.SetMatrix("previousVP", prevP * prevV);

				var currentP = VRUtils.GetNonJitteredProjectionMatrixForCamera(targetCamera); // Note: This isn't the GPU projection matrix (GL.GetGPUprojection matrix) equivalent to UNITY_MATRIX_P, but the code that uses this is adapted from code originally using unity_CameraInvProjection

				scatteringOcclusionMaterial.SetMatrix("inverseProjection", currentP.inverse);

				RenderTargetIdentifier[] RenderTargets = { new RenderTargetIdentifier(godraysRT[isRightEye][useFlipBuffer]), new RenderTargetIdentifier(depthRT[isRightEye][useFlipBuffer]) };

				commandBuffer.SetRenderTarget(RenderTargets, godraysRT[isRightEye][useFlipBuffer].depthBuffer);
				commandBuffer.DrawMesh(mesh, Matrix4x4.identity, scatteringOcclusionMaterial);

				commandBuffer.SetGlobalTexture(ShaderProperties._godrayDepthTexture_PROPERTY, RenderTargets[0]);
				commandBuffer.SetGlobalTexture("downscaledGodrayDepth", downscaledDepth);

				targetCamera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer); // This renders after the ocean even though they are on the same event because it gets added later (OnPreRender vs OnWillRenderObject)
			}
		}

		void OnPostRender()
		{
			if (renderingEnabled)
			{
				bool isRightEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right;
				var commandBuffer = godraysCommandBuffer[isRightEye];

				targetCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, commandBuffer);

				previousP[isRightEye] = GL.GetGPUProjectionMatrix(VRUtils.GetNonJitteredProjectionMatrixForCamera(targetCamera), false);
				previousV[isRightEye] = VRUtils.GetViewMatrixForCamera(targetCamera);

				bool doneRendering = targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;

				if (doneRendering)
				{
					renderingEnabled = false;
					useFlipBuffer = !useFlipBuffer;
				}
			}
		}
		
		public void OnDestroy()
		{
			// TODO: release, remove commandBuffers etc

		}
	}
}

