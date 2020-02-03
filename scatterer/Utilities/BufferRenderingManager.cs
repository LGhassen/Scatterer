using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	
	public class BufferRenderingManager : MonoBehaviour
	{
		GameObject bufferRenderingManagerGO;
		
		public RenderTexture depthTexture;			//full-scene depth texture, from merged built-in depth textures of the two local cameras
		public RenderTexture refractionTexture; 	//textures for the refractions, created once and accessed from here, written to from oceanNode
		//public RenderTexture occlusionTexture; 		//for SSAO and eclipses, for now will just contain a copy of the screenspace shadowmask

		public bool depthTextureCleared = false; 	//clear depth texture when away from PQS, for the sunflare shader

		List<Projector> EVEprojector=new List<Projector> {}; int projectorCount=0; //EVE projectors to disable, move to EVEUtils

		public void start()
		{					
			//Camera creation and setup
			bufferRenderingManagerGO = new GameObject("ScattererBufferManager");
//			replacementCamera = bufferRenderingManagerGO.AddComponent<Camera>();
//
//			replacementCamera.CopyFrom(Core.Instance.farCamera);
//			replacementCamera.transform.parent = Core.Instance.farCamera.transform;
//			
//			replacementCamera.enabled = false;
//			replacementCamera.clearFlags = CameraClearFlags.SolidColor;
//			replacementCamera.backgroundColor = Color.white;
//			replacementCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
//			replacementCamera.nearClipPlane=Core.Instance.nearCamera.nearClipPlane;
//			replacementCamera.depthTextureMode=DepthTextureMode.None;
//
//			//Shader setup
//			depthShader = ShaderReplacer.Instance.LoadedShaders["Scatterer/DepthTexture"];
//			scatteringReplacementShader = ShaderReplacer.Instance.LoadedShaders ["Scatterer/AtmosphericScatter (replacement shader)"];
//
//			if (Core.Instance.useGodrays)
//				godrayDepthShader=ShaderReplacer.Instance.LoadedShaders["Scatterer/GodrayDepthTexture"];

			createTextures();

			mapEVEshadowProjectors ();			
		}

		void OnPreRender () 
		{
			//update Camera
			//replacementCamera.fieldOfView = Core.Instance.farCamera.fieldOfView;

			//check buffers are created
			if (!depthTexture || !depthTexture.IsCreated())
			{
				Utils.Log("BufferRenderingManager: Recreating textures");
				createTextures();
				Core.Instance.onRenderTexturesLost();
			}

//			//Render depth buffer
//			if( (Core.Instance.fullLensFlareReplacement && Core.Instance.isGlobalPQSEnabled) || Core.Instance.isPQSEnabledOnScattererPlanet) //disable rendering when away from PQS
//			{
//				disableEVEshadowProjectors();
//				replacementCamera.backgroundColor = Color.white;
//				//render
//				replacementCamera.targetTexture = depthTexture;
//				replacementCamera.RenderWithShader (depthShader, "RenderType"); 
//
//				//render godrays
//				if (Core.Instance.useGodrays && Core.Instance.isPQSEnabledOnScattererPlanet)
//				{
//					replacementCamera.targetTexture =  godrayDepthTexture;
//					replacementCamera.cullingMask=32768; //ignore ships, parts, to avoid black godrays casting from ship
//					replacementCamera.RenderWithShader (godrayDepthShader, "RenderType");
//					replacementCamera.cullingMask = Core.Instance.farCamera.cullingMask; //restore normal culling mask after rendering godrays
//				}
//
//				enableEVEshadowProjectors();
//				depthTextureCleared = false;
//			}
		}
		
		public void clearDepthTexture()
		{
			RenderTexture rt=RenderTexture.active;
			RenderTexture.active= depthTexture;			
			GL.Clear(false,true,Color.white);
			RenderTexture.active=rt;			
			depthTextureCleared = true;
		}

		
		void mapEVEshadowProjectors()
		{
            if (EVEprojector == null)
                return;
			EVEprojector.Clear ();
			//Material atmosphereMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);
			Projector[] list = (Projector[]) Projector.FindObjectsOfType(typeof(Projector));
            if (list == null)
                return;
			for(int i=0;i<list.Length;i++)
			{
				if (list[i].material != null && list[i].material.name != null && list[i].material.name == "EVE/CloudShadow")
				{
					EVEprojector.Add(list[i]);
					//list[i].material = atmosphereMaterial;
				}
			}
			projectorCount = EVEprojector.Count;
		}

		void disableEVEshadowProjectors()
		{
			try
			{
				for (int i=0; i<projectorCount; i++) {
					EVEprojector [i].enabled = false;
				}
			}
			catch (Exception)
			{
				Utils.Log ("BufferRenderingManager: null EVE shadow projectors, remapping...");
				mapEVEshadowProjectors ();
			}
		}

		void enableEVEshadowProjectors()
		{
			for(int i=0;i<projectorCount;i++)
			{
				EVEprojector[i].enabled=true;
			}
		}

		public void OnDestroy ()
		{
			if (depthTexture)
			{
				depthTexture.Release ();
				UnityEngine.Object.Destroy (depthTexture);
			}
			if (refractionTexture)
			{
				refractionTexture.Release ();
				UnityEngine.Object.Destroy (refractionTexture);
			}
			if (bufferRenderingManagerGO)
				UnityEngine.Object.Destroy (bufferRenderingManagerGO);
		}	

		public void createTextures() //create simpler method createTexture with params, call it multiple times, make it static and move it to utils, reuse in skynode as well
		{
			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				//create textures
				depthTexture = new RenderTexture ( Screen.width, Screen.height,0, RenderTextureFormat.RFloat);
				depthTexture.name = "scattererDepthTexture";
				depthTexture.useMipMap  = false;
				depthTexture.filterMode = FilterMode.Point; // if this isn't in point filtering artifacts appear
				depthTexture.antiAliasing = 1;
				depthTexture.Create ();

//				//godray stuff
//				if (Core.Instance.useGodrays)
//				{
//					godrayDepthTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.RFloat);
//					godrayDepthTexture.name = "scattererGodrayDepthTexture";
//					godrayDepthTexture.filterMode = FilterMode.Point;
//					godrayDepthTexture.antiAliasing = 1;
//					godrayDepthTexture.useMipMap = false;
//					godrayDepthTexture.Create ();
//				}

				if (Core.Instance.mainSettings.useOceanShaders && Core.Instance.mainSettings.oceanRefraction)
				{
					refractionTexture = new RenderTexture ( Screen.width,Screen.height,0, RenderTextureFormat.ARGB32);
					refractionTexture.name = "scattererRefractionTexture";
					refractionTexture.useMipMap=false;
					refractionTexture.filterMode = FilterMode.Point;
					refractionTexture.antiAliasing = 1;
					refractionTexture.Create ();
				}
			}
		}
		
		
	}
}