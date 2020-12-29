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
	public class OceanRenderingHook : MonoBehaviour
	{
		public bool isEnabled = false;

		public OceanRenderingHook ()
		{
		}
		
		public MeshRenderer targetRenderer;
		public Material targetMaterial;
		
		//Dictionary to check if we added the OceanCommandBuffer to the camera
		private Dictionary<Camera,OceanCommandBuffer> cameraToOceanCommandBuffer = new Dictionary<Camera,OceanCommandBuffer>();
		
		void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			
			if (!cam || !targetRenderer || !targetMaterial)
				return;
			
			// Enable screen copying for this frame
			ScreenCopyCommandBuffer.EnableScreenCopyForFrame (cam);

			// Render ocean MeshRenderer for this frame
			// If projector mode render directly to screen
			// If depth buffer mode render to separate buffer so we can have the ocean's color and depth to be used by the scattering shader
			if (cameraToOceanCommandBuffer.ContainsKey (cam))
			{
				if (cameraToOceanCommandBuffer[cam] != null)
				{
					cameraToOceanCommandBuffer[cam].EnableForThisFrame();
				}
			}
			else
			{
				//we add null to the cameras we don't want to render on so we don't do a string compare every time
				if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera"))
				{
					cameraToOceanCommandBuffer[cam] = null;
				}
				else
				{
					OceanCommandBuffer oceanCommandBuffer = (OceanCommandBuffer) cam.gameObject.AddComponent(typeof(OceanCommandBuffer));
					oceanCommandBuffer.targetRenderer = targetRenderer;
					oceanCommandBuffer.targetMaterial = targetMaterial;
					oceanCommandBuffer.Initialize();
					oceanCommandBuffer.EnableForThisFrame();
					
					cameraToOceanCommandBuffer[cam] = oceanCommandBuffer;
				}
			}
		}
		
		public void OnDestroy ()
		{
			foreach (OceanCommandBuffer oceanCommandBuffer in cameraToOceanCommandBuffer.Values)
			{
				Component.Destroy(oceanCommandBuffer);
			}
		}
	}

	//this stuff now needs to be handled by the renderingcommandbuffer handler
	public class OceanCommandBuffer : MonoBehaviour
	{
		bool renderingEnabled = false;

		public MeshRenderer targetRenderer;
		public Material targetMaterial;
		
		private Camera targetCamera;
		private CommandBuffer rendererCommandBuffer;
		
		// We'll want to add a command buffer on any camera that renders us,
		// so have a dictionary of them.
		private Dictionary<Camera,CommandBuffer> m_Cameras = new Dictionary<Camera,CommandBuffer>();

		private RenderTexture oceanRenderTexture, depthCopyRenderTexture;
		private Material copyCameraDepthMaterial;

		public void Initialize()
		{
			targetCamera = GetComponent<Camera> ();

			rendererCommandBuffer = new CommandBuffer();
			rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";

			if (!Scatterer.Instance.mainSettings.useDepthBufferMode)
			{
				rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
				rendererCommandBuffer.DrawRenderer (targetRenderer, targetMaterial, 0, 0);
			}
			else
			{
				// If depth buffer mode render to separate buffer so we can have the ocean's color and depth to be used by the scattering shader
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

				targetCamera.forceIntoRenderTexture = true; //do this to force the camera target orientation to always match depth orientation
															//that way we don't have to worry about flipping them separately

				CreateRenderTextures (width, height);

				//initialize color and depth render textures
				rendererCommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, oceanRenderTexture);
				
				//blit by itself draws a quad with zwite off, use a material with zwrite on and outputs to depth
				//source: support.unity.com/hc/en-us/articles/115000229323-Graphics-Blit-does-not-copy-RenderTexture-depth
				rendererCommandBuffer.Blit (null, depthCopyRenderTexture, copyCameraDepthMaterial, 0);

				//!!!!!!! //with this there is a line at night on the ocean around the KSC? really weird

				//draw ocean renderer
				rendererCommandBuffer.SetRenderTarget(new RenderTargetIdentifier(oceanRenderTexture), new RenderTargetIdentifier(depthCopyRenderTexture));
				rendererCommandBuffer.DrawRenderer (targetRenderer, targetMaterial,0, 0); 	//this doesn't work with pixel lights so render only the main pass here and render pixel lights the regular way
																							//they will render on top of depth buffer scattering but that's not a noticeable issue, especially since ocean lights are soft additive

				
				//then set the textures for the scattering shader
				rendererCommandBuffer.SetGlobalTexture ("ScattererScreenCopy", oceanRenderTexture);
				rendererCommandBuffer.SetGlobalTexture ("ScattererDepthCopy",  depthCopyRenderTexture);
			}
		}

		void CreateRenderTextures (int width, int height)
		{
			oceanRenderTexture = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32);
			oceanRenderTexture.anisoLevel = 1;
			oceanRenderTexture.antiAliasing = 1;
			oceanRenderTexture.volumeDepth = 0;
			oceanRenderTexture.useMipMap = false;
			oceanRenderTexture.autoGenerateMips = false;
			oceanRenderTexture.Create ();

			depthCopyRenderTexture = new RenderTexture (width, height, 32, RenderTextureFormat.Depth);
			depthCopyRenderTexture.anisoLevel = 1;
			depthCopyRenderTexture.antiAliasing = 1;
			depthCopyRenderTexture.volumeDepth = 0;
			depthCopyRenderTexture.useMipMap = false;
			depthCopyRenderTexture.autoGenerateMips = false;
			depthCopyRenderTexture.filterMode = FilterMode.Point;
			depthCopyRenderTexture.depth = 32;
			depthCopyRenderTexture.Create ();
		}
		
		public void EnableForThisFrame()
		{
			if (!renderingEnabled)
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, rendererCommandBuffer);
				renderingEnabled = true;
			}
		}
		
		void OnPostRender()
		{
			if (renderingEnabled)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, rendererCommandBuffer);
				renderingEnabled = false;
			}
		}
		
		public void OnDestroy ()
		{
			if (!ReferenceEquals(targetCamera,null) && !ReferenceEquals(rendererCommandBuffer,null))
			{
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, rendererCommandBuffer);
				depthCopyRenderTexture.Release();
				oceanRenderTexture.Release();
				renderingEnabled = false;
			}
		}
	}
}