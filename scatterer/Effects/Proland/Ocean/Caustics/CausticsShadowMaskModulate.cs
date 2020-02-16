// after light's screenspace shadow mask is computed, apply caustics to it

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace scatterer
{
	public class CausticsShadowMaskModulate : MonoBehaviour
	{
		private CommandBuffer m_Buffer;
		private Light sunLight;
		public Material CausticsShadowMaskModulateMaterial;
		Texture2D causticsTexture;
		public bool isEnabled = false;
		public bool commandBufferAdded = false;
		
		public CausticsShadowMaskModulate ()
		{
		}

		public void Init(string causticsTexturePath,Vector2 causticsLayer1Scale,Vector2 causticsLayer1Speed,Vector2 causticsLayer2Scale,
		                 Vector2 causticsLayer2Speed, float causticsMultiply, float causticsMinBrightness, float oceanRadius, float blurDepth)
		{
			if (ReferenceEquals (CausticsShadowMaskModulateMaterial, null))
			{
				CausticsShadowMaskModulateMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/CausticsOcclusion")]);
			}

			causticsTexture = new Texture2D (1, 1);
			causticsTexture.LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath, causticsTexturePath)));
			causticsTexture.wrapMode = TextureWrapMode.Repeat;

			CausticsShadowMaskModulateMaterial.SetTexture("_CausticsTexture",causticsTexture);

			CausticsShadowMaskModulateMaterial.SetVector("layer1Scale",causticsLayer1Scale);
			CausticsShadowMaskModulateMaterial.SetVector("layer1Speed",causticsLayer1Speed);
			CausticsShadowMaskModulateMaterial.SetVector("layer2Scale",causticsLayer2Scale);
			CausticsShadowMaskModulateMaterial.SetVector("layer2Speed",causticsLayer2Speed);

			CausticsShadowMaskModulateMaterial.SetFloat("causticsMultiply",causticsMultiply);
			CausticsShadowMaskModulateMaterial.SetFloat("causticsMinBrightness",causticsMinBrightness);
			CausticsShadowMaskModulateMaterial.SetFloat("oceanRadius",oceanRadius);
			CausticsShadowMaskModulateMaterial.SetFloat("causticsBlurDepth",blurDepth);

			CausticsShadowMaskModulateMaterial.EnableKeyword ("SPHERE_PLANET");
			CausticsShadowMaskModulateMaterial.DisableKeyword ("FLAT_PLANET");

			sunLight = Scatterer.Instance.sunLight.GetComponent < Light > ();

			m_Buffer = new CommandBuffer();
			m_Buffer.name = "CausticsShadowMaskmodulate";			
			m_Buffer.Blit (null, BuiltinRenderTextureType.CurrentActive, CausticsShadowMaskModulateMaterial);

			AddCommandBuffer ();
			isEnabled = true;
		}

		public void AddCommandBuffer()
		{
			sunLight.AddCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);
			commandBufferAdded = true;
		}
		
		public void UpdateCaustics()
		{
			if (commandBufferAdded && !isEnabled)
			{
				RemoveCommandBuffer();
			}
			else if (!commandBufferAdded && isEnabled)
			{
				AddCommandBuffer();
			}
		}
				
		public void RemoveCommandBuffer ()
		{
			sunLight.RemoveCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);
			commandBufferAdded = false;
		}

		public void OnDestroy ()
		{
			RemoveCommandBuffer ();
		}
	}
}
	

