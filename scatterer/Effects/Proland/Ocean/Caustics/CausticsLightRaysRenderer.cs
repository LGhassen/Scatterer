using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

namespace scatterer
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
		                 Vector2 causticsLayer2Speed, float causticsMultiply, float causticsMinBrightness, float oceanRadius, float blurDepth, OceanNode oceanNodeIn)
		{
			if (string.IsNullOrEmpty (causticsTexturePath) || !System.IO.File.Exists (Utils.GameDataPath+causticsTexturePath))
			{
				Utils.LogInfo("Caustics texture "+ Utils.GameDataPath+causticsTexturePath +" not found, disabling caustics light rays for current planet");
				return false;
			}
			else
			{
				if (ReferenceEquals (CausticsLightRaysMaterial, null)) {
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
				if (!cam || MapView.MapIsEnabled || oceanNode.m_manager.m_skyNode.inScaledSpace)
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
						CameraToLightRaysScript [cam].Init ();
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
		private RenderTexture targetRT;
		private RenderTexture downscaledDepthRT;
		private Material downscaleDepthMaterial;
		bool isInitialized = false;
		bool renderingEnabled = false;
		
		public CausticsLightRaysCameraScript ()
		{
			
		}
		
		public void Init()
		{
			targetCamera = GetComponent<Camera>();
			Utils.LogInfo ("CausticsLightRaysCameraScript::Init for cam " + targetCamera.name);
			
			if (!ReferenceEquals (targetCamera.targetTexture, null))
			{
				targetRT = new RenderTexture (targetCamera.targetTexture.width / 2, targetCamera.targetTexture.height / 2, 0, RenderTextureFormat.ARGB32); //maybe change this to single 8 bit texture? doesn't exist I think lmao
			}
			else
			{
				targetRT = new RenderTexture (Screen.width / 2, Screen.height / 2, 0, RenderTextureFormat.ARGB32); //maybe change this to single 8 bit texture? doesn't exist I think lmao
			}
			targetRT.anisoLevel = 1;
			targetRT.antiAliasing = 1;
			targetRT.volumeDepth = 0;
			targetRT.useMipMap = true;
			targetRT.autoGenerateMips = false;
			targetRT.Create();
			targetRT.filterMode = FilterMode.Point; //might need a way to access both point and bilinear, or try the coord trick
			
			downscaledDepthRT = new RenderTexture(targetRT.width, targetRT.height, 0, RenderTextureFormat.RFloat);
			downscaledDepthRT.anisoLevel = 1;
			downscaledDepthRT.antiAliasing = 1;
			downscaledDepthRT.volumeDepth = 0;
			downscaledDepthRT.useMipMap = false;
			downscaledDepthRT.autoGenerateMips = false;
			downscaledDepthRT.Create();
			
			downscaledDepthRT.filterMode = FilterMode.Point;
			
			downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders [("Scatterer/DownscaleDepth")]); //still need to copy this from EVE
			compositeLightRaysMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/CompositeCausticsGodrays")]);
			compositeLightRaysMaterial.SetTexture ("LightRaysTexture", targetRT);

			commandBuffer = new CommandBuffer(); //might be worth doing this in init? since always the same, yes
			
			//downscale depth
			commandBuffer.Blit(null, downscaledDepthRT, downscaleDepthMaterial);
			commandBuffer.SetGlobalTexture("ScattererDownscaledDepth", downscaledDepthRT);
			
			//render and copy to screen (add upscaling)
			commandBuffer.Blit(null, targetRT, CausticsLightRaysMaterial); //blitting a bunch of garbage so check this
			commandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, compositeLightRaysMaterial);



			isInitialized = true;
			Utils.LogInfo ("CausticsLightRaysCameraScript::Init 2");
		}
		
		public void EnableForThisFrame()
		{
			if (isInitialized) //this still neeeds a check for underwater, or do it in the renderer class
			{
				targetCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
				
				//TODO: move this out of here
				float warpTime = (TimeWarp.CurrentRate > 1) ? (float) Planetarium.GetUniversalTime() : 0f;
				CausticsLightRaysMaterial.SetFloat (ShaderProperties.warpTime_PROPERTY, warpTime);
				
				renderingEnabled = true;
			}
		}
		
		public void OnPostRender()
		{
			if (renderingEnabled)
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
				renderingEnabled = false;
			}
		}

		public void CleanUp()
		{
			renderingEnabled = false;
			if (!ReferenceEquals(commandBuffer,null))
			{
				if (targetCamera)
				{
					targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, commandBuffer);
				}
				commandBuffer.Dispose ();
			}

			targetRT.Release ();
			downscaledDepthRT.Release ();
		}
	}
}


