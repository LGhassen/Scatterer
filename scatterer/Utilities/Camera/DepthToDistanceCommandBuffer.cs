using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace scatterer
{
	public class DepthToDistanceCommandBuffer : MonoBehaviour
	{

		private CommandBuffer m_Buffer;
		public Camera m_Camera;
		private Material m_Material;

		private void Awake ()
		{
			// after depth texture is rendered on far and near cameras, copy it and merge it as a single distance buffer
			m_Camera = gameObject.GetComponent<Camera>();
			Utils.LogDebug ("Adding DepthToDistanceCommandBuffer to "+m_Camera.name);
			m_Buffer = new CommandBuffer();
			m_Buffer.name = "ScattererDepthToDistanceCommandBuffer";
			m_Material = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthToDistance")]);

			if (m_Camera.name == Scatterer.Instance.ReturnProperCamera(true, false).name)
			{
				m_Buffer.SetRenderTarget(Scatterer.Instance.bufferManager.depthTexture);
				m_Buffer.ClearRenderTarget (false, true, Color.white);
			}


			m_Buffer.Blit (null, Scatterer.Instance.bufferManager.depthTexture, m_Material); //change to shadowmap texture

			m_Camera.AddCommandBuffer (CameraEvent.AfterDepthTexture, m_Buffer);
		}

		public void OnDestroy ()
		{
			Utils.LogDebug ("Removing DepthToDistanceCommandBuffer from "+m_Camera.name);
			if (!ReferenceEquals(m_Buffer, null) && !ReferenceEquals(m_Camera, null))
				m_Camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, m_Buffer);
		}
	}
}

