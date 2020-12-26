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
		
		public static void EnableOceanScreenCopyForFrame(Camera cam)
		{
			if (CameraToScreenCopyCommandBuffer.ContainsKey (cam))
			{
				if(CameraToScreenCopyCommandBuffer[cam])
					CameraToScreenCopyCommandBuffer[cam].EnableOceanScreenCopyForFrame();
			}
			else
			{
				if (cam.name.Contains("Reflection Probes Camera"))
					CameraToScreenCopyCommandBuffer[cam] = null;
				else
					CameraToScreenCopyCommandBuffer[cam] = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));
			}
		}

		public static void EnableScatteringScreenAndDepthCopyForFrame(Camera cam)
		{
			if (CameraToScreenCopyCommandBuffer.ContainsKey (cam))
			{
				if(CameraToScreenCopyCommandBuffer[cam])
					CameraToScreenCopyCommandBuffer[cam].EnableScatteringScreenAndDepthCopyForFrame();
			}
			else
			{
				//reflection probe should be fine here?
				CameraToScreenCopyCommandBuffer[cam] = (ScreenCopyCommandBuffer) cam.gameObject.AddComponent(typeof(ScreenCopyCommandBuffer));
			}
		}
		
		bool oceanRenderingEnabled = false;
		bool scatteringRenderingEnabled = false;
		bool isInitialized = false;
		private Camera targetCamera;
		private CommandBuffer colorCopyCommandBuffer, colorAndDepthCopyCommandBuffer;
		private RenderTexture colorCopyRenderTexture, depthCopyRenderTexture;
		private Material copyCameraDepthMaterial;
		
		public ScreenCopyCommandBuffer ()
		{
		}
		
		public void Initialize()
		{
			copyCameraDepthMaterial = new Material (ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
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
			
			colorCopyCommandBuffer = new CommandBuffer();
			colorCopyCommandBuffer.name = "Scatterer Screen Color Copy CommandBuffer";
			colorCopyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, colorCopyRenderTexture);
			colorCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopy", colorCopyRenderTexture);

			depthCopyRenderTexture = new RenderTexture(width, height, 32, RenderTextureFormat.Depth);
			depthCopyRenderTexture.anisoLevel = 1;
			depthCopyRenderTexture.antiAliasing = 1;
			depthCopyRenderTexture.volumeDepth = 0;
			depthCopyRenderTexture.useMipMap = false;
			depthCopyRenderTexture.autoGenerateMips = false;
			depthCopyRenderTexture.filterMode = FilterMode.Point;
			depthCopyRenderTexture.depth = 32;
			depthCopyRenderTexture.Create();		
			
			colorAndDepthCopyCommandBuffer = new CommandBuffer();
			colorAndDepthCopyCommandBuffer.name = "Scatterer Screen Color and Depth Copy CommandBuffer";

			colorAndDepthCopyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, colorCopyRenderTexture);

			//blit by itself draws a quad with zwite off, here I use a material which has zwrite on and outputs to depth
			//source: support.unity.com/hc/en-us/articles/115000229323-Graphics-Blit-does-not-copy-RenderTexture-depth
			colorAndDepthCopyCommandBuffer.Blit (null, depthCopyRenderTexture, copyCameraDepthMaterial, 0); //alright this works, so you're supposed to do this, then render the ocean to this color buffer and depth buffer
																											//then use depth buffer for scattering and render to the screen using the same color buffer the ocean rendered into, and also write out the new depth, done!
																											//when scattering renders alone it gets simplified, these steps get removed and there is no depth to write

			//colorAndDepthCopyCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, depthCopyRenderTexture, copyCameraDepthMaterial, 1); 	//try this which takes a maintex, nope does fuckall
//			colorAndDepthCopyCommandBuffer.Blit (BuiltinRenderTextureType.Depth, depthCopyRenderTexture, copyCameraDepthMaterial, 1); 			//try again with this, doesn't work either

			colorAndDepthCopyCommandBuffer.SetGlobalTexture ("ScattererScreenCopy", colorCopyRenderTexture);
			colorAndDepthCopyCommandBuffer.SetGlobalTexture ("ScattererDepthCopy",  depthCopyRenderTexture);
			
			isInitialized = true;
		}
		
		public void EnableOceanScreenCopyForFrame()
		{
			if (!oceanRenderingEnabled && isInitialized)
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, colorCopyCommandBuffer);	//both afterForwardOpaque and beforeForwardAlpha happen between 2500 and 2501							
				oceanRenderingEnabled = true;															//will have to make the ocean add it's commandbuffer to afterForwardOpaque, but after requesting enable
																										//and then have the scattering request a screen+depth copy after the ocean, using afterForwardAlpha, then scattering can render at renderqueue 2501,
																										//some of their stupid elements like kerbals, flags and whatnot I have to manually modify the renderqueue I guess
			}
		}

		public void EnableScatteringScreenAndDepthCopyForFrame()
		{
			if (!scatteringRenderingEnabled && isInitialized)
			{
				targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
				scatteringRenderingEnabled = true;
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
				if (oceanRenderingEnabled)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, colorCopyCommandBuffer);
					oceanRenderingEnabled = false;
				}
				if (scatteringRenderingEnabled)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
					scatteringRenderingEnabled = false;
				}
			}
		}
		
		public void OnDestroy ()
		{
			if (!ReferenceEquals(targetCamera,null))
			{
				if (!ReferenceEquals(colorCopyCommandBuffer,null))
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, colorCopyCommandBuffer);
					colorCopyRenderTexture.Release();
					oceanRenderingEnabled = false;
				}
				if (!ReferenceEquals(colorAndDepthCopyCommandBuffer,null))
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, colorAndDepthCopyCommandBuffer);
					depthCopyRenderTexture.Release();
					scatteringRenderingEnabled = false;
				}
			}
		}
	}
}

