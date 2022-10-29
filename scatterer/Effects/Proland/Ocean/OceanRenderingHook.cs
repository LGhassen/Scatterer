using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
	public class OceanRenderingHook : MonoBehaviour
	{
		public bool isEnabled = false;

		public OceanRenderingHook ()
		{
		}
		
		public MeshRenderer targetRenderer;
		public Material targetMaterial;
		public string celestialBodyName;

		//Dictionary to check if we added the OceanCommandBuffer to the camera
		private Dictionary<Camera,OceanCommandBuffer> cameraToOceanCommandBuffer = new Dictionary<Camera,OceanCommandBuffer>();
		
		void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			
			if (!cam || !targetRenderer || !targetMaterial)
				return;
			
			// Enable screen copying for this frame
			if (Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions && (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera))
				ScreenCopyCommandBuffer.EnableScreenCopyForFrame (cam);

			// Render ocean MeshRenderer for this frame
			// If projector mode render directly to screen
			// If depth buffer mode render to separate buffer so we can have the ocean's color and depth to be used by the scattering shader
			if (cameraToOceanCommandBuffer.ContainsKey (cam))
			{
				if (cameraToOceanCommandBuffer[cam] != null)
				{
					cameraToOceanCommandBuffer[cam].EnableForThisFrame();
				}
			}
			else
			{
				//we add null to the cameras we don't want to render on so we don't do a string compare every time
				if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera"))
				{
					cameraToOceanCommandBuffer[cam] = null;
				}
				else
				{
					OceanCommandBuffer oceanCommandBuffer = (OceanCommandBuffer) cam.gameObject.AddComponent(typeof(OceanCommandBuffer));
					oceanCommandBuffer.targetRenderer = targetRenderer;
					oceanCommandBuffer.targetMaterial = targetMaterial;
					oceanCommandBuffer.celestialBodyName = celestialBodyName;
					oceanCommandBuffer.Initialize();
					oceanCommandBuffer.EnableForThisFrame();
					
					cameraToOceanCommandBuffer[cam] = oceanCommandBuffer;
				}
			}
		}
		
		public void OnDestroy ()
		{
			foreach (OceanCommandBuffer oceanCommandBuffer in cameraToOceanCommandBuffer.Values)
			{
				if (oceanCommandBuffer)
					Component.DestroyImmediate(oceanCommandBuffer);
			}
		}
	}

	public class OceanCommandBuffer : MonoBehaviour
	{
		bool renderingEnabled = false;

		public MeshRenderer targetRenderer;
		public Material targetMaterial;
		public string celestialBodyName;

		private Camera targetCamera;
		private CommandBuffer rendererCommandBuffer;

		private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
		private Material copyCameraDepthMaterial;

		public void Initialize()
		{
			targetCamera = GetComponent<Camera> ();

			rendererCommandBuffer = new CommandBuffer();
			rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";

			if (!Scatterer.Instance.mainSettings.useDepthBufferMode)
			{
				rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				rendererCommandBuffer.DrawRenderer (targetRenderer, targetMaterial, 0, 0);
			}
			else
			{
				// If depth buffer mode render to separate buffer so we can have the ocean's color and depth to be used by the scattering shader
				copyCameraDepthMaterial = new Material (ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
				
				int width, height;
				
				if (targetCamera.activeTexture)
				{
					width = targetCamera.activeTexture.width;
					height = targetCamera.activeTexture.height;
				}
				else
				{
					width = Screen.width;
					height = Screen.height;
				}

				targetCamera.forceIntoRenderTexture = true; //do this to force the camera target orientation to always match depth orientation
															//that way we don't have to worry about flipping them separately

				CreateRenderTextures (width, height);

				//initialize color and depth render textures
				rendererCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, oceanRenderTexture);
				
				//blit by itself draws a quad with zwite off, use a material with zwrite on and outputs to depth
				//source: support.unity.com/hc/en-us/articles/115000229323-Graphics-Blit-does-not-copy-RenderTexture-depth
				rendererCommandBuffer.Blit (null, depthCopyRenderTexture, copyCameraDepthMaterial, 0);

				//draw ocean renderer
				rendererCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(oceanRenderTexture), new RenderTargetIdentifier(depthCopyRenderTexture));
				rendererCommandBuffer.DrawRenderer (targetRenderer, targetMaterial,0, 0);   //this doesn't work with pixel lights so render only the main pass here and render pixel lights the regular way
																							//they will render on top of depth buffer scattering but that's not a noticeable issue, especially since ocean lights are soft additive

				//expose the new depth buffer
				rendererCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(oceanRenderTexture), new RenderTargetIdentifier(oceanRenderTexture.depthBuffer));
				rendererCommandBuffer.SetGlobalTexture("ScattererDepthCopy", depthCopyRenderTexture);

				//enable cloud shadows
				rendererCommandBuffer.SetGlobalFloat(ShaderProperties.render_ocean_cloud_shadow_PROPERTY, 1f);

				//draw cloud shadows
				if (Scatterer.Instance.eveReflectionHandler.EVECloudLayers.ContainsKey(celestialBodyName))
				{ 
					foreach (var clouds2d in Scatterer.Instance.eveReflectionHandler.EVECloudLayers[celestialBodyName])
					{
						if (clouds2d.CloudShadowMaterial != null)
						{
							rendererCommandBuffer.Blit(null, oceanRenderTexture, clouds2d.CloudShadowMaterial);
						}
					}
				}

				//disable regular cloud shadows
				rendererCommandBuffer.SetGlobalFloat(ShaderProperties.render_ocean_cloud_shadow_PROPERTY, 0f);

				//then set the textures for the scattering shader
				rendererCommandBuffer.SetGlobalTexture ("ScattererScreenCopy", oceanRenderTexture);
			}
		}

		void CreateRenderTextures (int width, int height)
		{
			oceanRenderTexture = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32);
			oceanRenderTexture.anisoLevel = 1;
			oceanRenderTexture.antiAliasing = 1;
			oceanRenderTexture.volumeDepth = 0;
			oceanRenderTexture.useMipMap = false;
			oceanRenderTexture.autoGenerateMips = false;
			oceanRenderTexture.Create ();

			depthCopyRenderTexture = new RenderTexture (width, height, 32, RenderTextureFormat.Depth);
			depthCopyRenderTexture.anisoLevel = 1;
			depthCopyRenderTexture.antiAliasing = 1;
			depthCopyRenderTexture.volumeDepth = 0;
			depthCopyRenderTexture.useMipMap = false;
			depthCopyRenderTexture.autoGenerateMips = false;
			depthCopyRenderTexture.filterMode = FilterMode.Point;
			depthCopyRenderTexture.depth = 32;
			depthCopyRenderTexture.Create ();
		}
		
		public void EnableForThisFrame()
		{
			if (!renderingEnabled)
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, rendererCommandBuffer); //ocean renders on AfterImageEffectsOpaque, local scattering (with it's depth downscale) can render and copy to screen on afterForwardAlpha
				renderingEnabled = true;
			}
		}
		
		void OnPostRender()
		{
			if (renderingEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, rendererCommandBuffer);
				renderingEnabled = false;
			}
		}
		
		public void OnDestroy ()
		{
			if (targetCamera && rendererCommandBuffer != null)
			{
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterImageEffectsOpaque, rendererCommandBuffer);

				if (depthCopyRenderTexture)
					depthCopyRenderTexture.Release();
				
				if (oceanRenderTexture)
					oceanRenderTexture.Release();

				renderingEnabled = false;
			}
		}
	}
}