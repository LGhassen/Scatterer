// after light's screenspace shadow mask is computed, apply caustics to it

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace scatterer
{
	//transform this to general caustics class, pass to it the light and once of the ocean's meshrenderers?
	//how to handle the lightrays? do we need to render a quad to screen? do we set it as texture which the underwaterProjector reads from?


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

		public bool Init(string causticsTexturePath,Vector2 causticsLayer1Scale,Vector2 causticsLayer1Speed,Vector2 causticsLayer2Scale,
		                 Vector2 causticsLayer2Speed, float causticsMultiply, float causticsMinBrightness, float oceanRadius, float blurDepth)
		{

			if (string.IsNullOrEmpty (causticsTexturePath) || !System.IO.File.Exists (Utils.GameDataPath+causticsTexturePath))
			{
				Utils.LogInfo("Caustics texture "+ Utils.GameDataPath+causticsTexturePath +" not found, disabling caustics for current planet");
				return false;
			}
			else
			{
				if (ReferenceEquals (CausticsShadowMaskModulateMaterial, null)) {
					CausticsShadowMaskModulateMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/CausticsOcclusion")]);
				}

				causticsTexture = new Texture2D (1, 1);
				causticsTexture.LoadImage (System.IO.File.ReadAllBytes (Utils.GameDataPath+causticsTexturePath));
				causticsTexture.wrapMode = TextureWrapMode.Repeat;

				CausticsShadowMaskModulateMaterial.SetTexture ("_CausticsTexture", causticsTexture);

				CausticsShadowMaskModulateMaterial.SetVector ("layer1Scale", causticsLayer1Scale);
				CausticsShadowMaskModulateMaterial.SetVector ("layer1Speed", causticsLayer1Speed);
				CausticsShadowMaskModulateMaterial.SetVector ("layer2Scale", causticsLayer2Scale);
				CausticsShadowMaskModulateMaterial.SetVector ("layer2Speed", causticsLayer2Speed);

				CausticsShadowMaskModulateMaterial.SetFloat ("causticsMultiply", causticsMultiply);
				CausticsShadowMaskModulateMaterial.SetFloat ("causticsMinBrightness", causticsMinBrightness);
				CausticsShadowMaskModulateMaterial.SetFloat ("oceanRadius", oceanRadius);
				CausticsShadowMaskModulateMaterial.SetFloat ("causticsBlurDepth", blurDepth);

				CausticsShadowMaskModulateMaterial.EnableKeyword ("SPHERE_PLANET");
				CausticsShadowMaskModulateMaterial.DisableKeyword ("FLAT_PLANET"); //for testing in unity editor only, obviously, Kerbin is not flat I swear

				sunLight = Scatterer.Instance.sunLight;

				m_Buffer = new CommandBuffer ();
				m_Buffer.name = "CausticsShadowMaskmodulate";			
				m_Buffer.Blit (null, BuiltinRenderTextureType.CurrentActive, CausticsShadowMaskModulateMaterial);

				AddCommandBuffer ();
				isEnabled = true;
				return true;
			}
		}

		public void AddCommandBuffer()
		{
			sunLight.AddCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);
			commandBufferAdded = true;
		}
		
		public void UpdateCaustics()
		{
			if (isEnabled)
			{
				float warpTime = (TimeWarp.CurrentRate > 1) ? (float) Planetarium.GetUniversalTime() : 0f;
				CausticsShadowMaskModulateMaterial.SetFloat (ShaderProperties.warpTime_PROPERTY, warpTime);
			}

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
			if (!ReferenceEquals(sunLight,null))
				sunLight.RemoveCommandBuffer (LightEvent.AfterScreenspaceMask, m_Buffer);
			commandBufferAdded = false;
		}

		public void OnDestroy ()
		{
			RemoveCommandBuffer ();
		}
	}
}
	

