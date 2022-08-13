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

namespace scatterer
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
		private Camera targetCamera;
		private CommandBuffer screenCopyCommandBuffer;
		private RenderTexture colorCopyRenderTexture;
		
		public ScreenCopyCommandBuffer ()
		{
		}
		
		public void Initialize()
		{
			targetCamera = GetComponent<Camera> ();

			int width, height;

			if (!reflectionProbeMode)
			{
				targetCamera.forceIntoRenderTexture = true;

				if (!ReferenceEquals (targetCamera.activeTexture, null))
				{
					width = targetCamera.activeTexture.width;
					height = targetCamera.activeTexture.height;
				}
				else
				{
					width = Screen.width;
					height = Screen.height;
				}

				colorCopyRenderTexture = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32);
				colorCopyRenderTexture.anisoLevel = 1;
				colorCopyRenderTexture.antiAliasing = 1;
				colorCopyRenderTexture.volumeDepth = 0;
				colorCopyRenderTexture.useMipMap = false;
				colorCopyRenderTexture.autoGenerateMips = false;
				colorCopyRenderTexture.Create ();
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
				screenCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopyBeforeOcean", Texture2D.blackTexture);	//Hack but will stop sky flickering
			}
			
			isInitialized = true;
		}

		public void EnableScreenCopyForFrame()
		{
			if (!isEnabled && isInitialized)
			{
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
				if (isEnabled)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);
					isEnabled = false;
				}
			}
		}
		
		public void OnDestroy ()
		{
			//if (!ReferenceEquals(targetCamera,null))
			if (targetCamera != null)
			{
				//if (!ReferenceEquals(screenCopyCommandBuffer,null))
				if (screenCopyCommandBuffer != null)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterImageEffectsOpaque, screenCopyCommandBuffer);
					colorCopyRenderTexture.Release();
					isEnabled = false;
				}
			}
		}
	}
}

