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

		Vector3d previousParentPosition = Vector3d.zero;

		bool renderingEnabled = false;

		bool hasOcean = false;

		public RaymarchedGodraysRenderer()
		{

		}

		public bool Init(Light inputLight, SkyNode inputParentSkyNode)
		{
			if (ShaderReplacer.Instance.LoadedShaders.ContainsKey ("Scatterer/RaymarchScatteringOcclusion")) // TODO: change this to not duplicate the key
			{
				scatteringOcclusionMaterial = new Material(ShaderReplacer.Instance.LoadedShaders ["Scatterer/RaymarchScatteringOcclusion"]);
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

			scatteringOcclusionMaterial.SetTexture("StbnBlueNoise", ShaderReplacer.stbn);
			scatteringOcclusionMaterial.SetVector("stbnDimensions", new Vector3(ShaderReplacer.stbnDimensions.x, ShaderReplacer.stbnDimensions.y, ShaderReplacer.stbnDimensions.z));

			targetCamera = gameObject.GetComponent<Camera> ();

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

			renderWidth  = screenWidth  / 4;
			renderHeight = screenHeight / 4;

			godraysRT = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
			depthRT   = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.RHalf, FilterMode.Bilinear);

			downscaledDepth = RenderTextureUtils.CreateRenderTexture(renderWidth, renderHeight, RenderTextureFormat.RFloat, false, FilterMode.Point);

			godraysCommandBuffer = new FlipFlop<CommandBuffer>(VRUtils.VREnabled() ? new CommandBuffer() : null, new CommandBuffer());

			GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Quad);

			mesh = Instantiate(tempGO.GetComponent<MeshFilter>().mesh);

			GameObject.DestroyImmediate(tempGO);

			return true;
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
				commandBuffer.GetTemporaryRT(Shader.PropertyToID("tempGodraysDepthDownscale"), renderWidth * 2, renderHeight * 2, 0, FilterMode.Point, RenderTextureFormat.RFloat, RenderTextureReadWrite.Default);
				
				if (hasOcean)
					commandBuffer.Blit(null, Shader.PropertyToID("tempGodraysDepthDownscale"), downscaleDepthMaterial, 3);      //ocean depth buffer downsample
				else
					commandBuffer.Blit(null, Shader.PropertyToID("tempGodraysDepthDownscale"), downscaleDepthMaterial, 0);      //default depth buffer downsample
				

				// then downscale again to 1/16 (or 1/8)
				commandBuffer.Blit(Shader.PropertyToID("tempGodraysDepthDownscale"), downscaledDepth, downscaleDepthMaterial, 4); // TODO: do min/max downscaling, although current system works well enough

				commandBuffer.ReleaseTemporaryRT(Shader.PropertyToID("tempGodraysDepthDownscale"));

				scatteringOcclusionMaterial.SetTexture("downscaledDepth", downscaledDepth);
				scatteringOcclusionMaterial.SetTexture("historyGodrayOcclusionBuffer", godraysRT[isRightEye][!useFlipBuffer]);
				scatteringOcclusionMaterial.SetTexture("historyGodrayDepthBuffer", depthRT[isRightEye][!useFlipBuffer]);

				var prevV = previousV[isRightEye];

				// TODO: inject the parent/origin movement offset here, we will be lacking the planet's rotation but it won't be a big deal I think

				var prevP = previousP[isRightEye];

				scatteringOcclusionMaterial.SetMatrix("previousVP", prevP * prevV);

				RenderTargetIdentifier[] RenderTargets = { new RenderTargetIdentifier(godraysRT[isRightEye][useFlipBuffer]), new RenderTargetIdentifier(depthRT[isRightEye][useFlipBuffer]) };

				commandBuffer.SetRenderTarget(RenderTargets, godraysRT[isRightEye][useFlipBuffer].depthBuffer);
				commandBuffer.DrawMesh(mesh, Matrix4x4.identity, scatteringOcclusionMaterial);

				commandBuffer.SetGlobalTexture(ShaderProperties._godrayDepthTexture_PROPERTY, RenderTargets[0]);
				commandBuffer.SetGlobalTexture("downscaledGodrayDepth", downscaledDepth);

				targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);
			}
		}

		void OnPostRender()
		{
			if (renderingEnabled)
			{
				bool isRightEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right;
				var commandBuffer = godraysCommandBuffer[isRightEye];

				targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, commandBuffer);

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

