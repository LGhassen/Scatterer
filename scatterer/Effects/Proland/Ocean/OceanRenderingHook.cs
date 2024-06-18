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

			// Render ocean MeshRenderer for this frame
			// If projector mode render directly to screen
			// If depth buffer mode render to separate buffer so we can have the ocean's color and depth to be used by the scattering shader
			if (cameraToOceanCommandBuffer.ContainsKey (cam))
			{
				if (cameraToOceanCommandBuffer[cam] != null)
				{
					cameraToOceanCommandBuffer[cam].EnableForThisFrame();

                    // Enable screen copying for this frame
                    if (Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions && (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera))
                        ScreenCopyCommandBuffer.EnableScreenCopyForFrame(cam);
                }
			}
			else
			{
				//we add null to the cameras we don't want to render on so we don't do a string compare every time
				if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera") || (cam.name == "DepthCamera") || (cam.name == "NearCamera"))
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
		bool hdrEnabled = false;

		public MeshRenderer targetRenderer;
		public Material targetMaterial;
		public string celestialBodyName;

		private Camera targetCamera;
		private CommandBuffer rendererCommandBuffer;

		private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
		private Material copyCameraDepthMaterial;
		bool oceanScreenShotModeEnabled = false;

		int width = 0, height = 0;

		public void Initialize()
        {
            targetCamera = GetComponent<Camera>();

            copyCameraDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
            CreateRenderTextures();

            rendererCommandBuffer = new CommandBuffer();
            rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";

            RecreateCommandBuffer();
        }

        private void RecreateCommandBuffer()
        {
            rendererCommandBuffer.Clear();

            // Init depth render texture
            rendererCommandBuffer.Blit(null, depthCopyRenderTexture, copyCameraDepthMaterial, 3);

            // Draw ocean renderer, output as normal to the screen, and ocean depth to the depth texture
            RenderTargetIdentifier[] oceanRenderTargets = { new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), new RenderTargetIdentifier(depthCopyRenderTexture) };

            rendererCommandBuffer.SetRenderTarget(oceanRenderTargets, BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 0);   //this doesn't work with pixel lights so render only the main pass here and render pixel lights the regular way
                                                                                        //they will render on top of depth buffer scattering but that's not a noticeable issue, especially since ocean lights are soft additive

            // expose the new depth buffer
            rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.SetGlobalTexture("ScattererDepthCopy", depthCopyRenderTexture);

            // enable cloud shadows
            rendererCommandBuffer.SetGlobalFloat(ShaderProperties.render_ocean_cloud_shadow_PROPERTY, 1f);

            // draw cloud shadows
            if (Scatterer.Instance.eveReflectionHandler.EVECloudLayers.ContainsKey(celestialBodyName))
            {
                foreach (var clouds2d in Scatterer.Instance.eveReflectionHandler.EVECloudLayers[celestialBodyName])
                {
                    if (clouds2d.CloudShadowMaterial != null)
                    {
                        rendererCommandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, clouds2d.CloudShadowMaterial);
                    }
                }
            }

            // disable it for regular cloud shadows
            rendererCommandBuffer.SetGlobalFloat(ShaderProperties.render_ocean_cloud_shadow_PROPERTY, 0f);

            // recopy the screen and expose it as input for the scattering
            rendererCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, new RenderTargetIdentifier(oceanRenderTexture));

            // then set the textures for the scattering shader
            rendererCommandBuffer.SetGlobalTexture("ScattererScreenCopy", oceanRenderTexture);
        }

        private void GetTargetDimensions(out int screenWidth, out int screenHeight)
        {
            if (targetCamera.activeTexture)
            {
                screenWidth = targetCamera.activeTexture.width;
                screenHeight = targetCamera.activeTexture.height;
            }
            else
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }
        }

        void CreateRenderTextures ()
        {
            targetCamera.forceIntoRenderTexture = true; //do this to force the camera target orientation to always match depth orientation
                                                        //that way we don't have to worry about flipping them separately
            hdrEnabled = targetCamera.allowHDR;

            GetTargetDimensions(out width, out height);
            CreateTextures(width, height);
        }

        private void CreateTextures(int targetWidth, int targetHeight)
        {
			if (depthCopyRenderTexture)
				depthCopyRenderTexture.Release();

			if (oceanRenderTexture)
				oceanRenderTexture.Release();

			oceanRenderTexture = new RenderTexture(targetWidth, targetHeight, 0, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
            oceanRenderTexture.useMipMap = false;
            oceanRenderTexture.autoGenerateMips = false;
            oceanRenderTexture.Create();

            depthCopyRenderTexture = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.RFloat);
            depthCopyRenderTexture.useMipMap = false;
            depthCopyRenderTexture.autoGenerateMips = false;
            depthCopyRenderTexture.filterMode = FilterMode.Point;
            depthCopyRenderTexture.Create();
        }

        public void EnableForThisFrame()
		{
			if (!renderingEnabled)
            {
                int targetWidth, targetHeight;
                GetTargetDimensions(out targetWidth, out targetHeight);

                if (hdrEnabled != targetCamera.allowHDR || width != targetWidth || height != targetHeight || !oceanRenderTexture.IsCreated() || !depthCopyRenderTexture.IsCreated())
                {
                    CreateRenderTextures();
                    RecreateCommandBuffer();
                }

				bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);
				if (oceanScreenShotModeEnabled != screenShotModeEnabled)
                {
					// Resize textures
					int superSizingFactor = screenShotModeEnabled ? Mathf.Max(GameSettings.SCREENSHOT_SUPERSIZE, 1) : 1;
					CreateTextures(width * superSizingFactor, height * superSizingFactor);

					oceanScreenShotModeEnabled = screenShotModeEnabled;
				}

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