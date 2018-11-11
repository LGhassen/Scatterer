using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	
	public class BufferRenderingManager : MonoBehaviour
	{
		GameObject bufferRenderingManagerGO; 		//needed for the cameras for some reason

		//RenderTextures
		public RenderTexture depthTexture; 			//custom depth buffer texture
		public RenderTexture godrayDepthTexture; 			//custom depth buffer texture
		public RenderTexture refractionTexture; 	//textures for the refractions, created once and accessed from here, but rendered to from oceanNode
				
		//Shaders
		Shader depthShader;
		Shader GodrayDepthShader;

		//Camera
		Camera replacementCamera;					//camera that will have shader replacement used on it, will copy it's settings from the farCamera
		
		//Misc
		public bool depthTextureCleared = false; //clear depth texture when away from PQS, for the sunflare shader/


		//Misc - EVE projectors to disable 
		List<Projector> EVEprojector=new List<Projector> {};
		int projectorCount=0;

		public void start()
		{					
			//Camera creation and setup
			bufferRenderingManagerGO = new GameObject("ScattererBufferManager");
			replacementCamera = bufferRenderingManagerGO.AddComponent<Camera>();

			replacementCamera.CopyFrom(Core.Instance.farCamera);
			replacementCamera.transform.parent = Core.Instance.farCamera.transform;
			
			replacementCamera.enabled = false;
			replacementCamera.clearFlags = CameraClearFlags.SolidColor;
			replacementCamera.backgroundColor = Color.white;
			replacementCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
			replacementCamera.nearClipPlane=Core.Instance.nearCamera.nearClipPlane;
			replacementCamera.depthTextureMode=DepthTextureMode.None;

			//Shader setup
			depthShader = ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthTexture")];

			if (Core.Instance.useGodrays)
				GodrayDepthShader=ShaderReplacer.Instance.LoadedShaders[("Scatterer/GodrayDepthTexture")];

			createTextures();

			mapEVEshadowProjectors ();			
		}

		void OnPreRender () 
		{
			//update Camera
			replacementCamera.fieldOfView = Core.Instance.farCamera.fieldOfView;

			//check buffers are created
			if (!depthTexture || !depthTexture.IsCreated())
			{
				Debug.Log("[Scatterer] BufferRenderingManager: Recreating textures");
				createTextures();
				Core.Instance.onRenderTexturesLost();
			}

			//Render depth buffer
			if( (Core.Instance.fullLensFlareReplacement && Core.Instance.isGlobalPQSEnabled) || Core.Instance.isPQSEnabledOnScattererPlanet) //disable rendering when away from PQS
			{
				//disable EVE shadow projector
				disableEVEshadowProjectors();

				//render
				replacementCamera.targetTexture = depthTexture;
				replacementCamera.RenderWithShader (depthShader, "RenderType");				
				depthTextureCleared = false;

				//render godrays
				if (Core.Instance.useGodrays)
				{
					replacementCamera.targetTexture =  godrayDepthTexture;
					replacementCamera.cullingMask=32768; //ignore ships, parts, to avoid black godrays casting from ship
					replacementCamera.RenderWithShader (GodrayDepthShader, "RenderType");
					replacementCamera.cullingMask = Core.Instance.farCamera.cullingMask; //restore normal culling mask after rendering godrays
				}
				
				//re-enable EVE shadow projector
				enableEVEshadowProjectors();
			}
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
			EVEprojector.Clear ();
			Projector[] list = (Projector[]) Projector.FindObjectsOfType(typeof(Projector));
			for(int i=0;i<list.Length;i++)
			{
				if (list[i].material.name == "EVE/CloudShadow")
					EVEprojector.Add(list[i]);
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
				Debug.Log ("[Scatterer] BufferRenderingManager: null EVE shadow projectors, remapping...");
				mapEVEshadowProjectors ();   //remap
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

		public void createTextures()
		{
			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				//create textures
				depthTexture = new RenderTexture ( Screen.width, Screen.height,16, RenderTextureFormat.RFloat);
				depthTexture.name = "scattererDepthTexture";
				depthTexture.useMipMap  = false;
				depthTexture.filterMode = FilterMode.Point; // if this isn't in point filtering artifacts appear
				//depthTexture.antiAliasing = GameSettings.ANTI_ALIASING; //fixes some issue with aliased objects in front of water, creates halo on edge of objects however
				depthTexture.antiAliasing = 0;
				depthTexture.Create ();
				
				//godray stuff
				if (Core.Instance.useGodrays)
				{
					godrayDepthTexture = new RenderTexture (Screen.width, Screen.height, 16, RenderTextureFormat.RFloat);
					godrayDepthTexture.name = "scattererGodrayDepthTexture";
					godrayDepthTexture.filterMode = FilterMode.Point;
					godrayDepthTexture.antiAliasing = 0;
					godrayDepthTexture.useMipMap = false;
					godrayDepthTexture.Create ();
				}

				if (Core.Instance.useOceanShaders && Core.Instance.oceanRefraction)
				{
					refractionTexture = new RenderTexture ( Screen.width,Screen.height,16, RenderTextureFormat.ARGB32);
					refractionTexture.name = "scattererRefractionTexture";
					refractionTexture.useMipMap=false;
					refractionTexture.filterMode = FilterMode.Bilinear;
					refractionTexture.antiAliasing = 0;
					refractionTexture.Create ();
				}
			}
		}
		
		
	}
}