using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace scatterer
{
	public class ShadowRemoveFadeCommandBuffer : MonoBehaviour
	{

		private CommandBuffer m_Buffer;
		private Camera m_Camera;

		public ShadowRemoveFadeCommandBuffer ()
		{
			m_Buffer = new CommandBuffer();
			m_Buffer.name = "ScattererShadowRemoveFade";

			//"fix" the shadows fading near far clip plane

			//works for the fade but doesn't fix breaks in squares/axis-aligned lines near farclipPlane of nearCamera, limitation of what? idk
			//could still be ok for SSAO, maybe passable for eclipses but not sure
			m_Buffer.SetGlobalVector ("unity_ShadowFadeCenterAndType",new Vector4(float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity,-1f));

			m_Camera = GetComponent<Camera>();
			m_Camera.AddCommandBuffer (CameraEvent.BeforeForwardOpaque, m_Buffer);
		}

		public void OnDestroy ()
		{
			m_Camera.RemoveCommandBuffer (CameraEvent.BeforeForwardOpaque, m_Buffer);
		}
	}
}

