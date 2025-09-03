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

		private Camera targetCamera;
		private CommandBuffer screenCopyCommandBuffer;
		private int colorCopyRenderTextureNameID;
		
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

			colorCopyRenderTextureNameID = Shader.PropertyToID(nameof(colorCopyRenderTextureNameID));

            screenCopyCommandBuffer = new CommandBuffer();
			screenCopyCommandBuffer.name = "Scatterer screen copy CommandBuffer";

			Reinit();
			
			isInitialized = true;
		}

		private void Reinit()
        {
			targetCamera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);

			screenCopyCommandBuffer.Clear();

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
				if (hdrEnabled != targetCamera.allowHDR)
					Reinit();

				targetCamera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);
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
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);
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
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);
					screenCopyCommandBuffer.Release();
					screenCopyCommandBuffer = null;
					isEnabled = false;
				}
			}
		}
	}
}

