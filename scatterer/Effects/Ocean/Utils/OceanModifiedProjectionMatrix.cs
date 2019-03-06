using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace scatterer
{
	public class OceanModifiedProjectionMatrix : MonoBehaviour
	{
		public OceanNode oceanNode;

		Matrix4x4 ctos;
		Matrix4x4 stoc;
		
		// Whenever any camera will render us, update the material with the right projection params
		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			ctos = GL.GetGPUProjectionMatrix (cam.projectionMatrix,false);
			stoc = ctos.inverse;

			oceanNode.m_oceanMaterial.SetMatrix ("_Globals_CameraToScreen", ctos);
			oceanNode.m_oceanMaterial.SetMatrix ("_Globals_ScreenToCamera", stoc);
		}

		//not needed to be a separate method anymore
		//TODO: cleanup and refactore
		public Matrix4x4 ModifiedProjectionMatrix (Camera inCam)
		{
			Matrix4x4 p;
			
			p = inCam.projectionMatrix;
			p = GL.GetGPUProjectionMatrix (p, false);
			return p;
		}

		public void OnDestroy()
		{
		}
	}
}