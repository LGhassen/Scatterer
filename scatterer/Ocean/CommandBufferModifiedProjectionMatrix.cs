using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace scatterer
{
	public class CommandBufferModifiedProjectionMatrix : MonoBehaviour
	{
//		private Camera m_Cam;
		public Core m_core;
		
		// We'll want to add a command buffer on any camera that renders us,
		// so have a dictionary of them.
		private Dictionary<Camera,CommandBuffer> m_Cameras = new Dictionary<Camera,CommandBuffer>();


		// Whenever any camera will render us, add a command buffer to do the work on it
		public void OnWillRenderObject()
		{
			var act = gameObject.activeInHierarchy && enabled;
			if (!act)
			{
				Cleanup();
				return;
			}
			
			var cam = Camera.current;
			if (!cam)
				return;
			
			CommandBuffer buf = null;
			// Did we already add the command buffer on this camera? Nothing to do then.
			if (m_Cameras.ContainsKey(cam))
				return;
			
			buf = new CommandBuffer();
			buf.name = "Scatterer - Modified projection matrix for the ocean";
			m_Cameras[cam] = buf;


			Matrix4x4 ctos = ModifiedProjectionMatrix (cam);
			Matrix4x4 stoc = ctos.inverse;

			buf.SetGlobalMatrix ("_Globals_CameraToScreen", ctos);
			buf.SetGlobalMatrix ("_Globals_ScreenToCamera", stoc);
			
			cam.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, buf);
		}	

		//if OpenGL isn't detected
		// Scale and bias depth range
		public Matrix4x4 ModifiedProjectionMatrix (Camera inCam)
		{
			Matrix4x4 p;
			
			p = inCam.projectionMatrix;

			if (!m_core.opengl)
				for (int i = 0; i < 4; i++)
			{
				p [2, i] = p [2, i] * 0.5f + p [3, i] * 0.5f;
			}

			return p;
		}

		// Remove command buffers from all cameras we added into
		public void Cleanup()
		{
			foreach (var cam in m_Cameras)
			{
				if (cam.Key)
				{
					cam.Key.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, cam.Value);
				}
			}
			m_Cameras.Clear();
		}
		
		public void OnEnable()
		{
			Cleanup();
		}
		
		public void OnDisable()
		{
			Cleanup();
		}
	}
}