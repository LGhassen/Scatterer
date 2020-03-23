using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;


namespace scatterer
{
	public class ShadowMapCopyCommandBuffer : MonoBehaviour
	{

		private CommandBuffer m_Buffer;
		private Light m_Light;

		public ShadowMapCopyCommandBuffer ()
		{
			// after light's shadow map is computed, copy it
			RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
			m_Buffer = new CommandBuffer();
			m_Buffer.name = "ScattererShadowMapCopy";
			
			// Change shadow sampling mode for shadowmap.
			m_Buffer.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);

			//copy shadowmap
//			m_Buffer.Blit (shadowmap, Scatterer.Instance.bufferRenderingManager.occlusionTexture); //change to shadowmap texture
			
			m_Light = GetComponent<Light>();
			m_Light.AddCommandBuffer (LightEvent.AfterShadowMap, m_Buffer);
			
			// Sampling mode is restored automatically after this command buffer completes, so shadows will render normally.
		}

		public void OnDestroy ()
		{
			m_Light.RemoveCommandBuffer (LightEvent.AfterShadowMap, m_Buffer);
		}
	}
}

