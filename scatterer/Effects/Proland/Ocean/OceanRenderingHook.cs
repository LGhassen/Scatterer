using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class OceanRenderingHook : MonoBehaviour
    {
        private MeshRenderer targetRenderer;
        private Material targetMaterial;
        private int vertCountX, vertCountY;
        private Mesh oceanScreenGrid;

        //Dictionary to check if we added the OceanCommandBuffer to the camera
        private Dictionary<Camera,OceanCommandBuffer> cameraToOceanCommandBuffer = new Dictionary<Camera,OceanCommandBuffer>();
        
        public void Init(Material targetMaterial, MeshRenderer targetRenderer, int vertCountX, int vertCountY, Mesh oceanScreenGrid)
        {
            this.targetMaterial = targetMaterial;
            this.targetRenderer = targetRenderer;
            this.vertCountX = vertCountX;
            this.vertCountY = vertCountY;
            this.oceanScreenGrid = oceanScreenGrid;
        }

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

                    oceanCommandBuffer.Init(targetRenderer, targetMaterial, vertCountX, vertCountY, oceanScreenGrid);
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
                    Component.Destroy(oceanCommandBuffer);
            }
        }
    }

    public class OceanCommandBuffer : MonoBehaviour
    {
        public class OceanShaderPasses
        {
            public const int VertexPositions = 0;
            public const int GbufferWrite = 1;
            public const int GbufferMainLightingPass = 2;
            public const int GbufferForwardAdd = 3;
            public const int DeferredShadows = 4;
        }

        bool renderingEnabled = false;
        bool hdrEnabled = false;

        private MeshRenderer targetRenderer;
        private Material targetMaterial;
        private Material downscaleDepthMaterial;
        private int vertCountX, vertCountY;
        private Mesh oceanScreenGrid;

        private Camera targetCamera;
        private CommandBuffer oceanGbufferCommandBuffer, oceanShadingCommandBuffer;

        private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
        private Material copyCameraDepthMaterial;
        bool oceanScreenShotModeEnabled = false;

        int width = 0, height = 0;

        private static CameraEvent OceanGBufferCameraEvent = CameraEvent.AfterForwardOpaque;
        private static CameraEvent OceanShadingCameraEvent = CameraEvent.AfterImageEffectsOpaque;  //ocean renders on AfterImageEffectsOpaque, local scattering (with it's depth downscale) can render and copy to screen on afterForwardAlpha

        public void Init(MeshRenderer targetRenderer, Material targetMaterial, int vertCountX, int vertCountY, Mesh oceanScreenGrid)
        {
            this.targetRenderer = targetRenderer;
            this.targetMaterial = targetMaterial;
            this.vertCountX = vertCountX;
            this.vertCountY = vertCountY;
            this.oceanScreenGrid = oceanScreenGrid;

            targetCamera = GetComponent<Camera>();

            copyCameraDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
            CreateRenderTextures();

            oceanGbufferCommandBuffer = new CommandBuffer();
            oceanGbufferCommandBuffer.name = "Ocean Geometry CommandBuffer";

            oceanShadingCommandBuffer = new CommandBuffer();
            oceanShadingCommandBuffer.name = "Ocean Shading CommandBuffer";

            downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/DownscaleDepth")]);

            RecreateCommandBuffers();
        }

        private void RecreateCommandBuffers()
        {
            oceanGbufferCommandBuffer.Clear();

            int vertWorldPositionsXYZOceanPositionsXId = Shader.PropertyToID("vertWorldPositionsXYZOceanPositionsXId");
            int vertOceanPositionsYId = Shader.PropertyToID("vertOceanPositionsYId");

            // We need 3 float channels for world position and 2 float channels for ocean position
            // There is no RGBFloat format so split them over RGBA + R
            oceanGbufferCommandBuffer.GetTemporaryRT(vertWorldPositionsXYZOceanPositionsXId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
            oceanGbufferCommandBuffer.GetTemporaryRT(vertOceanPositionsYId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.RFloat);

            RenderTargetIdentifier[] oceanVertexPosRenderTextures = { new RenderTargetIdentifier(vertWorldPositionsXYZOceanPositionsXId), new RenderTargetIdentifier(vertOceanPositionsYId) };

            oceanGbufferCommandBuffer.SetRenderTarget(oceanVertexPosRenderTextures, vertWorldPositionsXYZOceanPositionsXId);
            oceanGbufferCommandBuffer.ClearRenderTarget(false, true, Color.black);
            oceanGbufferCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, OceanShaderPasses.VertexPositions);

            oceanGbufferCommandBuffer.SetGlobalTexture("oceanVertWorldPositionsXYZOceanPositionsX", vertWorldPositionsXYZOceanPositionsXId);
            oceanGbufferCommandBuffer.SetGlobalTexture("oceanVertOceanPositionsY", vertOceanPositionsYId);

            // Rasterize them and output normals, foam and roughness
            int oceanGbufferDepthTextureId = Shader.PropertyToID("oceanGbufferDepth");
            int oceanGbufferNormalsAndSigmaTextureId = Shader.PropertyToID("oceanGbufferNormalsAndSigma");
            int oceanGbufferFoamTextureId = Shader.PropertyToID("oceanGbufferFoam");

            oceanGbufferCommandBuffer.GetTemporaryRT(oceanGbufferDepthTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            oceanGbufferCommandBuffer.GetTemporaryRT(oceanGbufferNormalsAndSigmaTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010); // pack normals x and y in RG, sigma in B
                                                                                                                                                             // and sign of normal z in A.
                                                                                                                                                             // since these are signed worldNormals, we need the sign of z
                                                                                                                                                             // which can't be recovered by the formula z = sqrt(1-x^2-y^2)

            oceanGbufferCommandBuffer.GetTemporaryRT(oceanGbufferFoamTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.R8);

            oceanGbufferCommandBuffer.SetRenderTarget(oceanGbufferDepthTextureId);
            oceanGbufferCommandBuffer.ClearRenderTarget(false, true, SystemInfo.usesReversedZBuffer ? Color.black : Color.white);

            RenderTargetIdentifier[] oceanGbufferRenderTextures = { new RenderTargetIdentifier(oceanGbufferDepthTextureId),
                                                                    new RenderTargetIdentifier(oceanGbufferNormalsAndSigmaTextureId),
                                                                    new RenderTargetIdentifier(oceanGbufferFoamTextureId)};

            oceanGbufferCommandBuffer.SetRenderTarget(oceanGbufferRenderTextures, BuiltinRenderTextureType.CameraTarget);                       // Setting this as depth works for z-testing
            oceanGbufferCommandBuffer.DrawMesh(oceanScreenGrid, Matrix4x4.identity, targetMaterial, 0, OceanShaderPasses.GbufferWrite);         // Ocean gbuffer pass

            oceanGbufferCommandBuffer.SetGlobalTexture("oceanGbufferDepth", oceanGbufferDepthTextureId);
            oceanGbufferCommandBuffer.SetGlobalTexture("oceanGbufferNormalsAndSigma", oceanGbufferNormalsAndSigmaTextureId);
            oceanGbufferCommandBuffer.SetGlobalTexture("oceanGbufferFoam", oceanGbufferFoamTextureId);


            // If EVE has shadows to render it will provided a low-res ocean shadows texture and downscaled ocean depth "ScattererDownscaledOceanDepthTexture" for upscaling
            bool shadowsTextureCreated = Scatterer.Instance.EveReflectionHandler != null &&
                Scatterer.Instance.EveReflectionHandler.AddEVEOceanShadowCommands(oceanGbufferCommandBuffer, width, height, oceanGbufferDepthTextureId);

            // check eclipses or regular shadows are needed
            if (Scatterer.Instance.mainSettings.useEclipses || (Scatterer.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable)))
            { 
                if (!shadowsTextureCreated)
                {
                    // Get temporary RTs
                    int downscaledOceanDepthIdentifier = Shader.PropertyToID("ScattererDownscaledOceanDepthTexture");
                    int shadowsTextureId = Shader.PropertyToID("ScattererOceanShadowsTexture");

                    oceanGbufferCommandBuffer.GetTemporaryRT(downscaledOceanDepthIdentifier, width / 2, height / 2, 0, FilterMode.Point, RenderTextureFormat.RFloat);
                    oceanGbufferCommandBuffer.GetTemporaryRT(shadowsTextureId, width / 2, height / 2, 0, FilterMode.Bilinear, RenderTextureFormat.R8);

                    // Downscale depth
                    oceanGbufferCommandBuffer.SetGlobalTexture("ScattererDepthCopy", oceanGbufferDepthTextureId);
                    oceanGbufferCommandBuffer.Blit(null, downscaledOceanDepthIdentifier, downscaleDepthMaterial, 1);
                    oceanGbufferCommandBuffer.SetGlobalTexture("ScattererDownscaledOceanDepthTexture", downscaledOceanDepthIdentifier);

                    // Clear shadows RT
                    oceanGbufferCommandBuffer.SetRenderTarget(shadowsTextureId);
                    oceanGbufferCommandBuffer.ClearRenderTarget(false, true, Color.white);
                }

                // Rendertarget is already set by EVE or the above code
                oceanGbufferCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, OceanShaderPasses.DeferredShadows);  // Shadows and eclipses pass
            }

            // Draw ocean color using gbuffer, color only, we already have ocean depth
            oceanShadingCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            oceanShadingCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, OceanShaderPasses.GbufferMainLightingPass);  // Main pass, render a quad and read all shading info from gbuffer

            // Blend the resolved camera depth buffer and the ocean depth buffer to get a final depth buffer to use for other effects
            oceanShadingCommandBuffer.Blit(null, depthCopyRenderTexture, copyCameraDepthMaterial, 4);

            // Expose the new depth buffer
            oceanShadingCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            oceanShadingCommandBuffer.SetGlobalTexture("ScattererDepthCopy", depthCopyRenderTexture);

            // recopy the screen and expose it as input for the scattering
            oceanShadingCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, new RenderTargetIdentifier(oceanRenderTexture));

            // then set the textures for the scattering shader
            oceanShadingCommandBuffer.SetGlobalTexture("ScattererScreenCopy", oceanRenderTexture);
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
                    RecreateCommandBuffers();
                }

                bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);
                if (oceanScreenShotModeEnabled != screenShotModeEnabled)
                {
                    // Resize textures
                    int superSizingFactor = screenShotModeEnabled ? Mathf.Max(GameSettings.SCREENSHOT_SUPERSIZE, 1) : 1;
                    CreateTextures(width * superSizingFactor, height * superSizingFactor);

                    oceanScreenShotModeEnabled = screenShotModeEnabled;
                }

                targetCamera.AddCommandBuffer(OceanGBufferCameraEvent, oceanGbufferCommandBuffer);
                targetCamera.AddCommandBuffer(OceanShadingCameraEvent, oceanShadingCommandBuffer);
                renderingEnabled = true;
            }
        }

        void OnPostRender()
        {
            if (renderingEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
            {
                targetCamera.RemoveCommandBuffer(OceanGBufferCameraEvent, oceanGbufferCommandBuffer);
                targetCamera.RemoveCommandBuffer(OceanShadingCameraEvent, oceanShadingCommandBuffer);
                renderingEnabled = false;
            }
        }
        
        public void OnDestroy ()
        {
            if (targetCamera && oceanGbufferCommandBuffer != null)
            {
                targetCamera.RemoveCommandBuffer(OceanGBufferCameraEvent, oceanGbufferCommandBuffer);
                targetCamera.RemoveCommandBuffer(OceanShadingCameraEvent, oceanShadingCommandBuffer);

                oceanGbufferCommandBuffer.Release();
                oceanShadingCommandBuffer.Release();

                if (depthCopyRenderTexture)
                    depthCopyRenderTexture.Release();
                
                if (oceanRenderTexture)
                    oceanRenderTexture.Release();

                renderingEnabled = false;
            }
        }
    }
}