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
		bool commandBufferAdded = false;

		public ShadowMapCopyCommandBuffer ()
		{
			// After light's shadow map is computed, copy it
			RenderTargetIdentifier shadowmap = BuiltinRenderTextureType.CurrentActive;
			m_Buffer = new CommandBuffer();
			m_Buffer.name = "ScattererShadowMapCopy";
			
			// Change shadow sampling mode for shadowmap.
			m_Buffer.SetShadowSamplingMode(shadowmap, ShadowSamplingMode.RawDepth);

			m_Buffer.Blit (shadowmap, ShadowMapCopy.RenderTexture);
			m_Buffer.SetGlobalTexture (ShaderProperties._ShadowMapTextureCopyScatterer_PROPERTY, ShadowMapCopy.RenderTexture);

			m_Light = GetComponent<Light>();
			m_Light.AddCommandBuffer (LightEvent.AfterShadowMap, m_Buffer);			// Sampling mode is restored automatically after this command buffer completes, so shadows will render normally.
			commandBufferAdded = true;
		}

		public void Disable()
		{
			m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMap, m_Buffer);
			commandBufferAdded = false;
		}

		public void Enable ()
		{
			if (!commandBufferAdded)
			{
				m_Light.AddCommandBuffer (LightEvent.AfterShadowMap, m_Buffer);
				commandBufferAdded = true;
			}
		}

		public void OnDestroy ()
		{
			Disable ();
		}
	}

	public static class ShadowMapCopy
	{
		private static RenderTexture renderTexture;

		public static RenderTexture RenderTexture
		{
			get 
			{
				if (ReferenceEquals(renderTexture,null))
				{
					CreateTexture();
				}
				else
				{
					//If the size of the shadowMap changed from the last time we created a copy
					//if (QualitySettings.shadowResolution != renderTexture.width)
					if (Scatterer.Instance.sunLight.shadowCustomResolution != renderTexture.width)
					{
						renderTexture.Release();
						CreateTexture();
					}
				}

				return renderTexture;
			}
		}

		private static void CreateTexture()
		{
			//renderTexture = new RenderTexture ((int) QualitySettings.shadowResolution, (int) QualitySettings.shadowResolution, 0, RenderTextureFormat.RFloat); //QualitySettings return 2x2? Try anyway, RFloat may not be right, shadowMaps may use half? check with nsight
			renderTexture = new RenderTexture ((int) Scatterer.Instance.sunLight.shadowCustomResolution, (int) Scatterer.Instance.sunLight.shadowCustomResolution, 0, RenderTextureFormat.RHalf); //QualitySettings return 2x2? Try anyway, RFloat may not be right, shadowMaps may use half? check with nsight
			Utils.LogInfo("Ghassen QualitySettings.shadowResolution "+Scatterer.Instance.sunLight.shadowCustomResolution.ToString());
			//renderTexture = new RenderTexture (4096, 4096, 0, RenderTextureFormat.RFloat); //RFloat may not be right, shadowMaps may use half?
			renderTexture.useMipMap = false;
			renderTexture.antiAliasing = 1;
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.Create ();
		}
	}
}

