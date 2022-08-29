using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace Scatterer
{
	
	// First class inits the material and OnWillRender adds a script to relevant cameras, the given script adds and removes commandbuffers before and after rendering? yes, also enables only OnWillRender and if underwater
	public class CausticsLightRaysRenderer : MonoBehaviour
	{
		public Material CausticsLightRaysMaterial;
		Texture2D causticsTexture;
		public bool isEnabled = false;
		public bool commandBufferAdded = false;
		
		public OceanNode oceanNode;
		
		private Dictionary<Camera, CausticsLightRaysCameraScript> CameraToLightRaysScript = new Dictionary<Camera, CausticsLightRaysCameraScript>();
		
		public CausticsLightRaysRenderer ()
		{
		}
		
		//Todo refactor and make caustics class which handles the common code of lightrays and shadowmask?
		public bool Init(string causticsTexturePath,Vector2 causticsLayer1Scale,Vector2 causticsLayer1Speed,Vector2 causticsLayer2Scale,
		                 Vector2 causticsLayer2Speed, float causticsMultiply, float causticsMinBrightness, float oceanRadius, float blurDepth, OceanNode oceanNodeIn, float lightRaysStrength)
		{
			if (string.IsNullOrEmpty (causticsTexturePath) || !System.IO.File.Exists (Utils.GameDataPath+causticsTexturePath))
			{
				Utils.LogInfo("Caustics texture "+ Utils.GameDataPath+causticsTexturePath +" not found, disabling caustics light rays for current planet");
				return false;
			}
			else
			{
				if (CausticsLightRaysMaterial == null) {
					CausticsLightRaysMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/CausticsGodraysRaymarch")]);
				}
				
				causticsTexture = new Texture2D (1, 1);
				causticsTexture.LoadImage (System.IO.File.ReadAllBytes (Utils.GameDataPath+causticsTexturePath));
				causticsTexture.wrapMode = TextureWrapMode.Repeat;
				
				CausticsLightRaysMaterial.SetTexture ("_CausticsTexture", causticsTexture);
				
				CausticsLightRaysMaterial.SetVector ("layer1Scale", causticsLayer1Scale);
				CausticsLightRaysMaterial.SetVector ("layer1Speed", causticsLayer1Speed);
				CausticsLightRaysMaterial.SetVector ("layer2Scale", causticsLayer2Scale);
				CausticsLightRaysMaterial.SetVector ("layer2Speed", causticsLayer2Speed);
				
				CausticsLightRaysMaterial.SetFloat ("causticsMultiply", causticsMultiply);
				CausticsLightRaysMaterial.SetFloat ("causticsMinBrightness", causticsMinBrightness);
				CausticsLightRaysMaterial.SetFloat ("oceanRadius", oceanRadius);
				CausticsLightRaysMaterial.SetFloat ("causticsBlurDepth", blurDepth);
				CausticsLightRaysMaterial.SetFloat ("transparencyDepth", oceanNodeIn.transparencyDepth);
				CausticsLightRaysMaterial.SetFloat ("lightRaysStrength", lightRaysStrength);


				CausticsLightRaysMaterial.EnableKeyword ("SPHERE_PLANET");
				CausticsLightRaysMaterial.DisableKeyword ("FLAT_PLANET"); //for testing in unity editor only, obviously, Kerbin is not flat I swear

				oceanNode = oceanNodeIn;
				
				isEnabled = true;
				return true;
			}
		}
		
		public void OnWillRenderObject()
		{
			if (isEnabled && oceanNode.isUnderwater)
			{
				Camera cam = Camera.current;
				if (!cam || MapView.MapIsEnabled || oceanNode.prolandManager.skyNode.inScaledSpace)
					return;
				
				if (!CameraToLightRaysScript.ContainsKey (cam))
				{
					if ((cam.name == "Reflection Probes Camera") || (cam.name == "ScattererPartialDepthBuffer"))
					{
						//we add it anyway to avoid doing a string compare
						CameraToLightRaysScript [cam] = null;
					}
					else
					{
						CameraToLightRaysScript [cam] = (CausticsLightRaysCameraScript)cam.gameObject.AddComponent (typeof(CausticsLightRaysCameraScript));
						CameraToLightRaysScript [cam].CausticsLightRaysMaterial = CausticsLightRaysMaterial;
						CameraToLightRaysScript [cam].Init (oceanNode);
					}
				}
				else
				{
					if (CameraToLightRaysScript [cam] != null)
					{
						CameraToLightRaysScript [cam].EnableForThisFrame ();
					}
				}
			}
		}
		
		public void OnDestroy ()
		{
			foreach (CausticsLightRaysCameraScript script in CameraToLightRaysScript.Values)
			{
				if (script != null)
				{
					script.CleanUp();
					Component.Destroy(script);
				}
			}
		}
	}
	
	public class CausticsLightRaysCameraScript : MonoBehaviour
	{
		public Material CausticsLightRaysMaterial, compositeLightRaysMaterial;
		
		CommandBuffer commandBuffer;
		Camera targetCamera;
		private RenderTexture targetRT, targetRT2;
		private RenderTexture downscaledDepthRT,downscaledDepthRT2;
		private Material downscaleDepthMaterial;
		bool isInitialized = false;
		bool renderingEnabled = false;
		Light targetLight;
		private OceanNode oceanNode;
		
		public CausticsLightRaysCameraScript ()
		{
			
		}
		
		public void Init(OceanNode oceanNodeIn)
		{
			targetCamera = GetComponent<Camera>();
			
			if (targetCamera.targetTexture)
			{
				targetRT = new RenderTexture (targetCamera.targetTexture.width / 4, targetCamera.targetTexture.height / 4, 0, RenderTextureFormat.R8);
			}
			else
			{
				targetRT = new RenderTexture (Screen.width / 4, Screen.height / 4, 0, RenderTextureFormat.R8);
			}
			targetRT.anisoLevel = 1;
			targetRT.antiAliasing = 1;
			targetRT.volumeDepth = 0;
			targetRT.useMipMap = true;
			targetRT.autoGenerateMips = false;
			targetRT.filterMode = FilterMode.Bilinear;
			targetRT.Create();

			targetRT2 = new RenderTexture (targetRT.width, targetRT.height, 0, RenderTextureFormat.R8);
			targetRT2.anisoLevel = 1;
			targetRT2.antiAliasing = 1;
			targetRT2.volumeDepth = 0;
			targetRT2.useMipMap = true;
			targetRT2.autoGenerateMips = false;
			targetRT2.filterMode = FilterMode.Bilinear;
			targetRT2.Create();
			
			downscaledDepthRT = new RenderTexture(targetRT.width * 2, targetRT.height * 2, 0, RenderTextureFormat.RFloat);
			downscaledDepthRT.anisoLevel = 1;
			downscaledDepthRT.antiAliasing = 1;
			downscaledDepthRT.volumeDepth = 0;
			downscaledDepthRT.useMipMap = false;
			downscaledDepthRT.autoGenerateMips = false;
			downscaledDepthRT.filterMode = FilterMode.Point;
			downscaledDepthRT.Create();			

			downscaledDepthRT2 = new RenderTexture(downscaledDepthRT.width / 2, downscaledDepthRT.height / 2, 0, RenderTextureFormat.RFloat);
			downscaledDepthRT2.anisoLevel = 1;
			downscaledDepthRT2.antiAliasing = 1;
			downscaledDepthRT2.volumeDepth = 0;
			downscaledDepthRT2.useMipMap = false;
			downscaledDepthRT2.autoGenerateMips = false;
			downscaledDepthRT2.filterMode = FilterMode.Point;
			downscaledDepthRT2.Create();
			
			downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders [("Scatterer/DownscaleDepth")]);
			compositeLightRaysMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/CompositeCausticsGodrays")]);
			compositeLightRaysMaterial.SetTexture ("LightRaysTexture", targetRT);

			compositeLightRaysMaterial.SetColor(ShaderProperties._sunColor_PROPERTY, oceanNodeIn.prolandManager.getIntensityModulatedSunColor());
			compositeLightRaysMaterial.SetVector ("_Underwater_Color", oceanNodeIn.m_UnderwaterColor);

			commandBuffer = new CommandBuffer();
			
			//downscale depth to 1/4
			commandBuffer.Blit(null, downscaledDepthRT, downscaleDepthMaterial, 0);
			commandBuffer.SetGlobalTexture("ScattererDownscaledDepthIntermediate", downscaledDepthRT);
			//further downscale depth to 1/16
			commandBuffer.Blit(null, downscaledDepthRT2, downscaleDepthMaterial, 1);
			commandBuffer.SetGlobalTexture("ScattererDownscaledDepth", downscaledDepthRT2);
			
			//render
			commandBuffer.Blit(null, targetRT, CausticsLightRaysMaterial);

			//bilateral blur, 2 taps seems enough
			commandBuffer.SetGlobalVector ("BlurDir", new Vector2(0,1));
			commandBuffer.SetGlobalTexture ("TextureToBlur", targetRT);
			commandBuffer.Blit(null, targetRT2, downscaleDepthMaterial, 2);

			commandBuffer.SetGlobalVector ("BlurDir", new Vector2(1,0));
			commandBuffer.SetGlobalTexture ("TextureToBlur", targetRT2);
			commandBuffer.Blit(null, targetRT, downscaleDepthMaterial, 2);

			//copy to screen
			commandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, compositeLightRaysMaterial);

			targetLight = oceanNodeIn.prolandManager.mainSunLight;
			oceanNode = oceanNodeIn;

			isInitialized = true;
		}
		
		public void EnableForThisFrame()
		{
			if (isInitialized)
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
				
				//TODO: move this out of here
				float warpTime = (TimeWarp.CurrentRate > 1) ? (float) Planetarium.GetUniversalTime() : 0f;
				CausticsLightRaysMaterial.SetFloat (ShaderProperties.warpTime_PROPERTY, warpTime);

				// If we use sunlightExtinction, reuse already computed extinction color
				if (Scatterer.Instance.mainSettings.sunlightExtinction)
				{
					compositeLightRaysMaterial.SetColor(ShaderProperties._sunColor_PROPERTY, oceanNode.prolandManager.getIntensityModulatedSunColor() * Scatterer.Instance.sunlightModulatorsManagerInstance.GetLastModulateColor(targetLight));
				}

				renderingEnabled = true;
			}
		}
		
		public void OnPostRender()
		{
			if (renderingEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
				renderingEnabled = false;
			}
		}

		public void CleanUp()
		{
			renderingEnabled = false;
			if (commandBuffer != null)
			{
				if (targetCamera)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, commandBuffer);
				}
				commandBuffer.Dispose ();
			}

			targetRT.Release ();
			targetRT2.Release ();
			downscaledDepthRT.Release ();
			downscaledDepthRT2.Release ();
		}
	}
}


