using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace scatterer
{
	public class CommandBufferModifiedProjectionMatrix : MonoBehaviour
	{
//		private Camera m_Cam;

		public OceanNode oceanNode;

		Matrix4x4 ctos;
		Matrix4x4 stoc;

		CommandBuffer buf = null;

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


			Camera cam = Camera.current;
			if (!cam)
				return;

			bool alreadyAdded = m_Cameras.ContainsKey (cam);
			// Did we already add the command buffer on this camera
			if (alreadyAdded)
			{
				buf = m_Cameras [cam];
			}
			else
			{
				buf = new CommandBuffer ();
				buf.name = "Scatterer - Modified projection matrix for the ocean";

				m_Cameras [cam] = buf;
			}

			ctos = ModifiedProjectionMatrix (cam);
			stoc = ctos.inverse;

			buf.SetGlobalMatrix ("_Globals_CameraToScreen", ctos);
			buf.SetGlobalMatrix ("_Globals_ScreenToCamera", stoc);

			if (!alreadyAdded)
				cam.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, buf);
		}	

		//if OpenGL isn't detected
		public Matrix4x4 ModifiedProjectionMatrix (Camera inCam)
		{
			Matrix4x4 p;
			
			p = inCam.projectionMatrix;
			p = GL.GetGPUProjectionMatrix (p, false);

			if (!Core.Instance.opengl && (GameSettings.ANTI_ALIASING > 0))
			{
				p = p * Matrix4x4.Scale(new Vector3(1, -1, 1)); //not a perfect fix, flips front and back faces
																//to fix this faces are flipped in OceanNode when the mesh is created
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

		public void OnDestroy()
		{
		}
	}
}