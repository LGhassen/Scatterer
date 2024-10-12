using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
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
        int width, height;

        private Camera targetCamera;
        private CommandBuffer screenCopyCommandBuffer;
        private RenderTexture colorCopyRenderTexture;

        private static CameraEvent ScreenCopyCameraEvent = CameraEvent.AfterImageEffectsOpaque;

        public ScreenCopyCommandBuffer ()
        {
        }
        
        public void Initialize()
        {
            targetCamera = GetComponent<Camera> ();

            if (!reflectionProbeMode)
            {
                targetCamera.forceIntoRenderTexture = true;

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

                InitRT();
            }

            screenCopyCommandBuffer = new CommandBuffer();
            screenCopyCommandBuffer.name = "Scatterer screen copy CommandBuffer";

            if (!reflectionProbeMode)
            {
                screenCopyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, colorCopyRenderTexture);
                screenCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopyBeforeOcean", colorCopyRenderTexture);
            }
            else
            {
                screenCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopyBeforeOcean", Texture2D.blackTexture);    //Hack but will stop sky flickering
            }
            
            isInitialized = true;
        }

        private void InitRT()
        {
            if (colorCopyRenderTexture!=null)
                colorCopyRenderTexture.Release();

            hdrEnabled = targetCamera.allowHDR;

            colorCopyRenderTexture = new RenderTexture(width, height, 0, hdrEnabled? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
            colorCopyRenderTexture.anisoLevel = 1;
            colorCopyRenderTexture.antiAliasing = 1;
            colorCopyRenderTexture.volumeDepth = 0;
            colorCopyRenderTexture.useMipMap = false;
            colorCopyRenderTexture.autoGenerateMips = false;
            colorCopyRenderTexture.Create();
        }

        private void Reinit()
        {
            InitRT();

            targetCamera.RemoveCommandBuffer(ScreenCopyCameraEvent, screenCopyCommandBuffer);

            if (!reflectionProbeMode)
            {
                screenCopyCommandBuffer.Clear();
                screenCopyCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, colorCopyRenderTexture);
                screenCopyCommandBuffer.SetGlobalTexture("ScattererScreenCopyBeforeOcean", colorCopyRenderTexture);
            }
        }

        public void EnableScreenCopyForFrame()
        {
            if (!isEnabled && isInitialized)
            {
                if (hdrEnabled != targetCamera.allowHDR)
                    Reinit();

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
                    colorCopyRenderTexture.Release();
                    isEnabled = false;
                }
            }
        }
    }
}

