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

        //Dictionary to check if we added the OceanCommandBuffer to the camera
        private Dictionary<Camera,OceanCommandBuffer> cameraToOceanCommandBuffer = new Dictionary<Camera,OceanCommandBuffer>();
        
        public void Init(Material targetMaterial, MeshRenderer targetRenderer, int vertCountX, int vertCountY)
        {
            this.targetMaterial = targetMaterial;
            this.targetRenderer = targetRenderer;
            this.vertCountX = vertCountX;
            this.vertCountY = vertCountY;
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

                    oceanCommandBuffer.Init(targetRenderer, targetMaterial, vertCountX, vertCountY);
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

        private Camera targetCamera;
        private CommandBuffer rendererCommandBuffer;

        private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
        private Material copyCameraDepthMaterial;
        bool oceanScreenShotModeEnabled = false;

        int width = 0, height = 0;

        MeshRenderer dummyRenderer;

        public void Init(MeshRenderer targetRenderer, Material targetMaterial, int vertCountX, int vertCountY)
        {
            this.targetRenderer = targetRenderer;
            this.targetMaterial = targetMaterial;
            this.vertCountX = vertCountX;
            this.vertCountY = vertCountY;

            targetCamera = GetComponent<Camera>();

            copyCameraDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
            CreateRenderTextures();

            rendererCommandBuffer = new CommandBuffer();
            rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";

            var dummyGO = GameObject.CreatePrimitive(PrimitiveType.Quad);

            var collider = dummyGO.GetComponent<Collider>();

            if (collider != null )
            {
                Component.Destroy(collider);
            }

            dummyGO.SetActive(false);
            dummyRenderer = dummyGO.GetComponent<MeshRenderer>();

            RecreateCommandBuffer();
        }

        private void RecreateCommandBuffer()
        {
            rendererCommandBuffer.Clear();

            int vertWorldPositionsTextureId = Shader.PropertyToID("oceanVertWorldPositionsTextureId");
            int vertOceanPositionsTextureId = Shader.PropertyToID("oceanVertOceanPositionsTextureId");

            rendererCommandBuffer.GetTemporaryRT(vertWorldPositionsTextureId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.ARGBFloat); // TODO: reuse the last channel
            rendererCommandBuffer.GetTemporaryRT(vertOceanPositionsTextureId, vertCountX, vertCountY, 0, FilterMode.Point, RenderTextureFormat.RGFloat);

            RenderTargetIdentifier[] oceanVertexPosRenderTextures = { new RenderTargetIdentifier(vertWorldPositionsTextureId), new RenderTargetIdentifier(vertOceanPositionsTextureId) };


            rendererCommandBuffer.SetRenderTarget(oceanVertexPosRenderTextures, vertWorldPositionsTextureId); // ideally you don't set this depth texture
            rendererCommandBuffer.ClearRenderTarget(false, true, Color.black);
            //rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 3);  // ocean verts pass // Need a quad though
            rendererCommandBuffer.DrawRenderer(dummyRenderer, targetMaterial, 0, 3);  // ocean verts pass
            //rendererCommandBuffer.Blit(null, oceanVertexPosRenderTextures, targetMaterial, 3, 0);

            rendererCommandBuffer.SetGlobalTexture("oceanVertWorldPositions", vertWorldPositionsTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanVertOceanPositions", vertOceanPositionsTextureId);

            // Start by blitting quad to multiple RT to calculate these, using that renderer's material

            // Next rasterize them and output normals (try 10-bit), foam and sigmaSQ (8-bit each)
            int oceanGbufferDepthTextureId = Shader.PropertyToID("oceanGbufferDepth");
            int oceanGbufferNormalsTextureId = Shader.PropertyToID("oceanGbufferNormals");
            int oceanGbufferSigmaSqTextureId = Shader.PropertyToID("SigmaSq");
            int oceanGbufferFoamTextureId = Shader.PropertyToID("oceanGbufferFoam");

            rendererCommandBuffer.GetTemporaryRT(oceanGbufferDepthTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            //rendererCommandBuffer.GetTemporaryRT(oceanGbufferNormalsTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferNormalsTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.RG16);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferSigmaSqTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.R8);
            rendererCommandBuffer.GetTemporaryRT(oceanGbufferFoamTextureId, width, height, 0, FilterMode.Point, RenderTextureFormat.R8);

            RenderTargetIdentifier[] oceanGbufferRenderTextures = { new RenderTargetIdentifier(oceanGbufferDepthTextureId), new RenderTargetIdentifier(oceanGbufferNormalsTextureId),
                                                                    new RenderTargetIdentifier(oceanGbufferSigmaSqTextureId), new RenderTargetIdentifier(oceanGbufferFoamTextureId)};

            rendererCommandBuffer.SetRenderTarget(oceanGbufferRenderTextures, BuiltinRenderTextureType.CameraTarget); // setting this as depth works for culling
            rendererCommandBuffer.ClearRenderTarget(false, true, Color.black);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 2);  // ocean gbuffer pass

            rendererCommandBuffer.SetGlobalTexture("oceanGbufferDepth", oceanGbufferDepthTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferNormals", oceanGbufferNormalsTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferSigmaSq", oceanGbufferSigmaSqTextureId);
            rendererCommandBuffer.SetGlobalTexture("oceanGbufferFoam", oceanGbufferFoamTextureId);


            int oceanDownscaledReflectionsTextureId = Shader.PropertyToID("oceanDownscaledReflections");
            //rendererCommandBuffer.GetTemporaryRT(oceanDownscaledReflectionsTextureId, width / 2, height / 2, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            rendererCommandBuffer.GetTemporaryRT(oceanDownscaledReflectionsTextureId, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            //rendererCommandBuffer.GetTemporaryRT(oceanDownscaledReflectionsTextureId, width / 2, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            rendererCommandBuffer.SetRenderTarget(oceanDownscaledReflectionsTextureId);
            rendererCommandBuffer.ClearRenderTarget(false, true, Color.black);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 5);  // downscaled reflections pass

            rendererCommandBuffer.SetGlobalTexture("oceanDownscaledReflections", oceanDownscaledReflectionsTextureId);

            // rasterize all that shit to gbuffer

            // Invoke EVE method to render cloud shadows, returns bool, use that to know if we enable shadows or not
            // (also if EVE not installed)

            // Also render regular shadows on top, but if EVE not installed not used, create your own texture here and render them
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

            // Init depth render texture
            rendererCommandBuffer.Blit(null, depthCopyRenderTexture, copyCameraDepthMaterial, 3);

            // Draw ocean renderer, output as normal to the screen, and ocean depth to the depth texture
            RenderTargetIdentifier[] oceanRenderTargets = { new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), new RenderTargetIdentifier(depthCopyRenderTexture) };

            rendererCommandBuffer.SetRenderTarget(oceanRenderTargets, BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 0);   //this doesn't work with pixel lights so render only the main pass here and render pixel lights the regular way
                                                                                        //they will render on top of depth buffer scattering but that's not a noticeable issue, especially since ocean lights are soft additive


            rendererCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.DrawRenderer(dummyRenderer, targetMaterial, 0, 4);  // main pass but from gbuffer

            // expose the new depth buffer
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

            if (dummyRenderer != null)
            {
                GameObject.Destroy(dummyRenderer.gameObject);
            }
        }
    }
}