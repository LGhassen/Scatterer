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
		//maybe these don't need to be static, or at least need to be thrown out on scene changes, in any case try KSC screen, because after screen changes this stuff becomes invalid
		//yep, that was it! add event to clean them up or clean up the design, put everything back how it was but expose the target color and depth textures
		//I vote to put everything back, I can't think straight, I'm tired
		private static Dictionary<Camera,ScreenCopyCommandBuffer> CameraToCommandBufferHandler = new Dictionary<Camera,ScreenCopyCommandBuffer>();

		//and also this will only work for one ocean, if you go to another planet the mr and mat don't change, so really extract this and move it back to ocean hook ok?
		public static void EnableScreenCopyForFrame(Camera cam)
		{
			if (CameraToCommandBufferHandler.ContainsKey (cam))
			{
				if(CameraToCommandBufferHandler[cam])
					CameraToCommandBufferHandler[cam].EnableScreenCopyForFrame();
			}
			else
			{
				if ((cam.name=="Reflection Probes Camera"))  //in depth buffer mode screen still needs to be copied for reflection probes so think about it
					CameraToCommandBufferHandler[cam] = null;
				else
				{
					ScreenCopyCommandBuffer handler = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));
					handler.Initialize();
					CameraToCommandBufferHandler[cam] = handler;
				}
			}
		}

		public static void EnableScatteringScreenAndDepthCopyForFrame(Camera cam)
		{
//			if (CameraToCommandBufferHandler.ContainsKey (cam))
//			{
//				if(CameraToCommandBufferHandler[cam])
//					CameraToCommandBufferHandler[cam].EnableScatteringScreenAndDepthCopyForFrame();
//			}
//			else
//			{
//				//reflection probe should be fine here?
//				CameraToCommandBufferHandler[cam] = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));
//			}
		}
		
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

			screenCopyCommandBuffer = new CommandBuffer();
			screenCopyCommandBuffer.name = "Scatterer screen copy CommandBuffer";

			screenCopyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, colorCopyRenderTexture);
			screenCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopyBeforeOcean", colorCopyRenderTexture);
			
			isInitialized = true;
		}

		public void EnableScreenCopyForFrame()
		{
			if (!isEnabled && isInitialized)
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, screenCopyCommandBuffer);
				isEnabled = true;
			}
		}

//		//what's the point of this if we have the ocean?
//		public void EnableScatteringScreenAndDepthCopyForFrame()
//		{
//			if (!scatteringRenderingEnabled && isInitialized)
//			{
//				targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
//				scatteringRenderingEnabled = true;
//			}
//		}
		
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
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, screenCopyCommandBuffer);
					isEnabled = false;
				}
//				if (scatteringRenderingEnabled)
//				{
//					targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
//					scatteringRenderingEnabled = false;
//				}
			}
		}
		
		public void OnDestroy ()
		{
			if (!ReferenceEquals(targetCamera,null))
			{
				if (!ReferenceEquals(screenCopyCommandBuffer,null))
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, screenCopyCommandBuffer);
					colorCopyRenderTexture.Release();
					isEnabled = false;
				}
//				if (!ReferenceEquals(colorAndDepthCopyCommandBuffer,null))
//				{
//					targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
//					depthCopyRenderTexture.Release();
//					scatteringRenderingEnabled = false;
//				}
			}
		}
	}
}

