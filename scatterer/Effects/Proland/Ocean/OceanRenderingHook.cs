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
			
			if (!cam)
				return;
			
			// Enable screen copying for this frame
			ScreenCopyCommandBuffer.EnableOceanScreenCopyForFrame (cam);
			
			// Render ocean MeshRenderer for this frame
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
		
		public void Initialize()
		{
			targetCamera = GetComponent<Camera> ();
			
			rendererCommandBuffer = new CommandBuffer();
			rendererCommandBuffer.name = "Ocean MeshRenderer CommandBuffer";
			rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
			rendererCommandBuffer.DrawRenderer (targetRenderer, targetMaterial);
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
				renderingEnabled = false;
			}
		}
	}
}