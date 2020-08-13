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
		private static Dictionary<Camera,ScreenCopyCommandBuffer> CameraToScreenCopyCommandBuffer = new Dictionary<Camera,ScreenCopyCommandBuffer>();
		
		public static void EnableForThisFrame(Camera cam)
		{
			if (CameraToScreenCopyCommandBuffer.ContainsKey (cam))
			{
				if(CameraToScreenCopyCommandBuffer[cam])
					CameraToScreenCopyCommandBuffer[cam].EnableForThisFrame();
			}
			else
			{
				if (cam.name.Contains("Reflection Probes Camera"))
					CameraToScreenCopyCommandBuffer[cam] = null;
				else
					CameraToScreenCopyCommandBuffer[cam] = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));
			}
		}
		
		bool renderingEnabled = false;
		bool isInitialized = false;
		private Camera targetCamera;
		private CommandBuffer copyCommandBuffer;
		private RenderTexture copyRenderTexture;
		
		public ScreenCopyCommandBuffer ()
		{
		}
		
		public void Initialize()
		{
			targetCamera = GetComponent<Camera> ();
			
			if (!ReferenceEquals(targetCamera.activeTexture ,null))
			{
				copyRenderTexture = new RenderTexture (targetCamera.activeTexture.width, targetCamera.activeTexture.height, 0, RenderTextureFormat.ARGB32);
				copyRenderTexture.anisoLevel = 1;
				copyRenderTexture.antiAliasing = 1;
				copyRenderTexture.volumeDepth = 0;
				copyRenderTexture.useMipMap = false;
				copyRenderTexture.autoGenerateMips = false;
				copyRenderTexture.Create ();

				copyCommandBuffer = new CommandBuffer();
				copyCommandBuffer.name = "Scatterer Screen Copy CommandBuffer";
				copyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, copyRenderTexture);
				
				copyCommandBuffer.SetGlobalTexture ("ScattererScreenCopy", copyRenderTexture);
				
				isInitialized = true;
				//Utils.Log ("ScreenCopyCommandBuffer initialized successfully!!!");
			}
		}
		
		public void EnableForThisFrame()
		{
			if (!renderingEnabled && isInitialized)
			{
				//Utils.LogInfo ("ScreenCopyCommandBuffer rendering for this frame on camera "+targetCamera.name);
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, copyCommandBuffer);
				renderingEnabled = true;
			}
		}
		
		void OnPostRender()
		{
			if (!isInitialized)
			{
				Initialize ();
			}
			else if (renderingEnabled)
			{
				//Utils.LogInfo ("ScreenCopyCommandBuffer rendering finished, disabling on Camera "+targetCamera.name);
				targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, copyCommandBuffer);
				renderingEnabled = false;
			}
		}
		
		public void OnDestroy ()
		{
			//Utils.Log ("OnDestroy called on ScreenCopyCommandBuffer");
			if (!ReferenceEquals(targetCamera,null) && !ReferenceEquals(copyCommandBuffer,null))
			{
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, copyCommandBuffer);
				copyRenderTexture.Release();
				renderingEnabled = false;
			}
		}
	}
}

