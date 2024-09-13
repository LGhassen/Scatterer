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
        bool renderingEnabled = false;
        bool hdrEnabled = false;

        private MeshRenderer targetRenderer;
        private Material targetMaterial;
        private int vertCountX, vertCountY;
        private Mesh oceanScreenGrid;

        private Camera targetCamera;
        private CommandBuffer rendererCommandBuffer;

        private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
        private Material copyCameraDepthMaterial;
        bool oceanScreenShotModeEnabled = false;

        int width = 0, height = 0;

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

            rendererCommandBuffer = new CommandBuffer();
            rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";

            RecreateCommandBuffer();
        }

        private void RecreateCommandBuffer()
        {
            rendererCommandBuffer.Clear();

            int vertWorldPositionsXYZOceanPositionsXId = Shader.PropertyToID("vertWorldPositionsXYZOceanPositionsXId");
            int vertOceanPositionsYId = Shader.PropertyToID("vertOceanPositionsYId");

            // We need 3 float channels for world position and 2 float channels for ocean position
            // There is no RGBFloat format so split them over RGBA + R
            rendererCommandBuffer.GetTemporaryRT(vertWorldPositionsXYZOceanPositionsXId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat);
            rendererCommandBuffer.GetTemporaryRT(vertOceanPositionsYId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.RFloat);

            RenderTargetIdentifier[] oceanVertexPosRenderTextures = { new RenderTargetIdentifier(vertWorldPositionsXYZOceanPositionsXId), new RenderTargetIdentifier(vertOceanPositionsYId) };

            rendererCommandBuffer.SetRenderTarget(oceanVertexPosRenderTextures, vertWorldPositionsXYZOceanPositionsXId);
            rendererCommandBuffer.ClearRenderTarget(false, true, Color.black);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 0);  // ocean verts pass

            rendererCommandBuffer.SetGlobalTexture("oceanVertWorldPositionsXYZOceanPositionsX", vertWorldPositionsXYZOceanPositionsXId);
            rendererCommandBuffer.SetGlobalTexture("oceanVertOceanPositionsY", vertOceanPositionsYId);

            // Rasterize them and output normals, foam and roughness (all 8-bit apart from depth)
            int oceanGbufferDepthTextureId = Shader.PropertyToID("oceanGbufferDepth");
            int oceanGbufferNormalsTextureId = Shader.PropertyToID("oceanGbufferNormals");
            int oceanGbufferSigmaSqTextureId = Shader.PropertyToID("SigmaSq");
            int oceanGbufferFoamTextureId = Shader.PropertyToID("oceanGbufferFoam");

            rendererCommandBuffer.GetTemporaryRT(oceanGbufferDepthTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferNormalsTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.RG16);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferSigmaSqTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.R8);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferFoamTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.R8);

            rendererCommandBuffer.SetRenderTarget(oceanGbufferDepthTextureId);
            rendererCommandBuffer.ClearRenderTarget(false, true, SystemInfo.usesReversedZBuffer ? Color.black : Color.white);

            RenderTargetIdentifier[] oceanGbufferRenderTextures = { new RenderTargetIdentifier(oceanGbufferDepthTextureId), new RenderTargetIdentifier(oceanGbufferNormalsTextureId),
                                                                    new RenderTargetIdentifier(oceanGbufferSigmaSqTextureId), new RenderTargetIdentifier(oceanGbufferFoamTextureId)};

            rendererCommandBuffer.SetRenderTarget(oceanGbufferRenderTextures, BuiltinRenderTextureType.CameraTarget);   // Setting this as depth works for z-testing
            rendererCommandBuffer.DrawMesh(oceanScreenGrid, Matrix4x4.identity, targetMaterial, 0, 1);                  // Ocean gbuffer pass

            rendererCommandBuffer.SetGlobalTexture("oceanGbufferDepth", oceanGbufferDepthTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferNormals", oceanGbufferNormalsTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferSigmaSq", oceanGbufferSigmaSqTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferFoam", oceanGbufferFoamTextureId);

            // Invoke EVE method to render cloud shadows, returns bool, use that to know if we enable shadows or not
            // (also if EVE not installed)

            // Also render regular shadows on top, but if EVE not installed or not used, create your own texture here and render them
            // also eclipses

            // Then switch keyword or something
            bool shadowsTextureCreated = Scatterer.Instance.EveReflectionHandler != null &&
                Scatterer.Instance.EveReflectionHandler.AddEVEOceanShadowCommands(rendererCommandBuffer, width / 2, height / 2);

            if (true) // check eclipses or regular shadows are needed
            { 
                if (!shadowsTextureCreated)
                {
                    // create 1/4 res texture manually
                }

                // render regular shadows and eclipses here
            }

            // Draw ocean color using gbuffer, color only, we already have ocean depth
            rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 2);  // Main pass, render a quad and read all shading info from gbuffer

            // Blend the resolved camera depth buffer and the ocean depth buffer to get a final depth buffer to use for other effects
            rendererCommandBuffer.Blit(null, depthCopyRenderTexture, copyCameraDepthMaterial, 4);

            // Expose the new depth buffer
            rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.SetGlobalTexture("ScattererDepthCopy", depthCopyRenderTexture);

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