using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class ScreenCopyCommandBuffer : MonoBehaviour
    {
        private static Dictionary<Camera,ScreenCopyCommandBuffer> CameraToCommandBufferHandler = new Dictionary<Camera,ScreenCopyCommandBuffer>();
        
        public static void EnableScreenCopyForFrame(Camera cam)
        {
            if (CameraToCommandBufferHandler.ContainsKey (cam))
            {
                if(CameraToCommandBufferHandler[cam])
                    CameraToCommandBufferHandler[cam].EnableScreenCopyForFrame();
            }
            else
            {
                ScreenCopyCommandBuffer handler = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));

                if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera"))
                    handler.reflectionProbeMode = true;
                
                handler.Initialize();
                CameraToCommandBufferHandler[cam] = handler;
            }
        }

        public bool reflectionProbeMode = false;
        bool isEnabled = false;
        bool isInitialized = false;
        bool hdrEnabled = false;

        private Camera targetCamera;
        private Material depthCopyMaterial;
        private CommandBuffer screenCopyCommandBuffer;
        private CommandBuffer scaledDeferredDepthCopyCommandBuffer;
        private CommandBuffer scaledDeferredDepthCleanupCommandBuffer;
        private int colorCopyRenderTextureNameID;
        private int scaledDeferredDepthCopyRenderTextureNameID;
        private int scaledDeferredDepthSourceTextureNameID;
        private bool useScaledDeferredDepthCopy;

        private static CameraEvent ScreenCopyCameraEvent = CameraEvent.BeforeImageEffectsOpaque;
        private static CameraEvent ScaledDeferredDepthCopyCameraEvent = CameraEvent.AfterForwardOpaque;
        private static CameraEvent ScaledDeferredDepthCleanupCameraEvent = CameraEvent.AfterEverything;

        public ScreenCopyCommandBuffer ()
        {
        }
        
        public void Initialize()
        {
            targetCamera = GetComponent<Camera> ();

            if (!reflectionProbeMode)
            {
                targetCamera.forceIntoRenderTexture = true;
            }

            depthCopyMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);

			colorCopyRenderTextureNameID = Shader.PropertyToID(nameof(colorCopyRenderTextureNameID));
            scaledDeferredDepthCopyRenderTextureNameID = Shader.PropertyToID("ScattererScaledDeferredDepthCopy");
            scaledDeferredDepthSourceTextureNameID = Shader.PropertyToID("ScattererScaledDeferredDepthSource");

            screenCopyCommandBuffer = new CommandBuffer();
            screenCopyCommandBuffer.name = "Scatterer screen copy CommandBuffer";

            scaledDeferredDepthCopyCommandBuffer = new CommandBuffer();
            scaledDeferredDepthCopyCommandBuffer.name = "Scatterer scaled deferred depth copy CommandBuffer";

            scaledDeferredDepthCleanupCommandBuffer = new CommandBuffer();
            scaledDeferredDepthCleanupCommandBuffer.name = "Scatterer scaled deferred depth cleanup CommandBuffer";

            Reinit();

            isInitialized = true;
        }

        private void Reinit()
        {
            targetCamera.RemoveCommandBuffer(ScreenCopyCameraEvent, screenCopyCommandBuffer);
            targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCopyCameraEvent, scaledDeferredDepthCopyCommandBuffer);
            targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCleanupCameraEvent, scaledDeferredDepthCleanupCommandBuffer);

            screenCopyCommandBuffer.Clear();
            scaledDeferredDepthCopyCommandBuffer.Clear();
            scaledDeferredDepthCleanupCommandBuffer.Clear();

            useScaledDeferredDepthCopy = targetCamera == Scatterer.Instance.scaledSpaceCamera
                && targetCamera.actualRenderingPath == RenderingPath.DeferredShading;

            // When using deferred and some mods like Mirage/Scaled still render in forward without a dedicated depth pass
            // The unity resolved deferred+forward depth is insufficient
            // Unity seems to do the following:
            //  1. Render deferred objects depth+ g-buffer colors and other properties
            //  2. render forward-only depths to the depth buffers if they have a depth pass
            //  3. use depth + g-buffers for lighting
            //  4. use depth to generate motion vectors for TAA and stuff
            //  5. render forward-only shaders to the main screen color, depth is rendered to a temp texture here then the output depth is ignored
            //  6. transparencies and other effect render using the initial depth from step 2 so they don't see depth from step 5
            // What I do here is manually copy the depth from step 2 to fix this, I assume this is also a similar issue with Parallax Scaled
            // Some additional work here is needed to fix motion vectors and TAA with the new depth
            if (useScaledDeferredDepthCopy)
            {
                scaledDeferredDepthCopyCommandBuffer.GetTemporaryRT(scaledDeferredDepthCopyRenderTextureNameID, -1, -1, 0,
                    FilterMode.Point, RenderTextureFormat.RFloat);

                // Setting a global texture with the BuiltinRenderTextureType.CurrentActive and RenderTextureSubElement.Depth seems
                // to be the only way to target the temporary depth texture used by Unity here internally
                // TODO: also try to just copy these back into the resolved depth texture if possible, would be more robust
                scaledDeferredDepthCopyCommandBuffer.SetGlobalTexture(scaledDeferredDepthSourceTextureNameID,
                    BuiltinRenderTextureType.CurrentActive, RenderTextureSubElement.Depth);

                scaledDeferredDepthCopyCommandBuffer.Blit(null, scaledDeferredDepthCopyRenderTextureNameID,
                    depthCopyMaterial, 5);

                scaledDeferredDepthCopyCommandBuffer.SetGlobalTexture(ShaderProperties.ScattererCameraDepthTexture_PROPERTY,
                    scaledDeferredDepthCopyRenderTextureNameID);
                scaledDeferredDepthCopyCommandBuffer.SetGlobalInt(ShaderProperties.ScattererUseCustomDepthTexture_PROPERTY, 1);

                scaledDeferredDepthCleanupCommandBuffer.ReleaseTemporaryRT(scaledDeferredDepthCopyRenderTextureNameID);
            }
            else
            {
                screenCopyCommandBuffer.SetGlobalTexture(ShaderProperties.ScattererCameraDepthTexture_PROPERTY,
                    targetCamera.actualRenderingPath == RenderingPath.DeferredShading ? BuiltinRenderTextureType.ResolvedDepth : BuiltinRenderTextureType.Depth);
                screenCopyCommandBuffer.SetGlobalInt(ShaderProperties.ScattererUseCustomDepthTexture_PROPERTY, 0);
            }

            if (!reflectionProbeMode)
            {
                screenCopyCommandBuffer.GetTemporaryRT(colorCopyRenderTextureNameID, -1, -1, 0, FilterMode.Point, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
                screenCopyCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, colorCopyRenderTextureNameID);
                screenCopyCommandBuffer.SetGlobalTexture("ScattererScreenCopyBeforeOcean", colorCopyRenderTextureNameID);
            }
            else
            {
                screenCopyCommandBuffer.SetGlobalTexture("ScattererScreenCopyBeforeOcean", Texture2D.blackTexture); //Hack but will stop sky flickering
            }
        }

        public void EnableScreenCopyForFrame()
        {
            if (!isEnabled && isInitialized)
            {
                bool shouldUseScaledDeferredDepthCopy = targetCamera == Scatterer.Instance.scaledSpaceCamera
                    && targetCamera.actualRenderingPath == RenderingPath.DeferredShading;

                if (hdrEnabled != targetCamera.allowHDR || useScaledDeferredDepthCopy != shouldUseScaledDeferredDepthCopy)
                    Reinit();

                if (useScaledDeferredDepthCopy)
                {
                    targetCamera.AddCommandBuffer(ScaledDeferredDepthCopyCameraEvent, scaledDeferredDepthCopyCommandBuffer);
                    targetCamera.AddCommandBuffer(ScaledDeferredDepthCleanupCameraEvent, scaledDeferredDepthCleanupCommandBuffer);
                }

                targetCamera.AddCommandBuffer(ScreenCopyCameraEvent, screenCopyCommandBuffer);
                isEnabled = true;
            }
        }

        void OnPostRender()
        {
            if (!isInitialized)
            {
                Initialize ();
            }
            else
            {
                if (isEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
                {
                    targetCamera.RemoveCommandBuffer (ScreenCopyCameraEvent, screenCopyCommandBuffer);
                    targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCopyCameraEvent, scaledDeferredDepthCopyCommandBuffer);
                    targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCleanupCameraEvent, scaledDeferredDepthCleanupCommandBuffer);
                    isEnabled = false;
                }
            }
        }
        
        public void OnDestroy ()
        {
            if (targetCamera != null)
            {
                if (screenCopyCommandBuffer != null)
                {
                    targetCamera.RemoveCommandBuffer (ScreenCopyCameraEvent, screenCopyCommandBuffer);
                    targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCopyCameraEvent, scaledDeferredDepthCopyCommandBuffer);
                    targetCamera.RemoveCommandBuffer(ScaledDeferredDepthCleanupCameraEvent, scaledDeferredDepthCleanupCommandBuffer);
                    isEnabled = false;
                }
            }

            if (depthCopyMaterial != null)
            {
                UnityEngine.Object.Destroy(depthCopyMaterial);
            }
        }
    }
}

