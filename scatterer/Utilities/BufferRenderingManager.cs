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
				
		//Shaders
		Shader depthShader;
		//Shader GodrayDepthShader;

		//Camera
		Camera replacementCamera;					//camera that will have shader replacement used on it, will copy it's settings from the farCamera


		//Misc
		public bool depthTextureCleared = false; //clear depth texture when away from PQS, for the sunflare shader/


		//Misc - EVE projectors to disable 
		List<Projector> EVEprojector=new List<Projector> {};
		int projectorCount=0;

		public void start()
		{
			Debug.Log ("[Scatterer] Antialiasing level detected: " + GameSettings.ANTI_ALIASING);

			//create textures
			depthTexture = new RenderTexture ( Screen.width,Screen.height,16, RenderTextureFormat.RFloat);
			depthTexture.useMipMap  = false;
			depthTexture.generateMips = false;
			depthTexture.filterMode = FilterMode.Point; // if this isn't in point filtering artifacts appear
			depthTexture.antiAliasing = GameSettings.ANTI_ALIASING; //fixes some issue with aliased objects in front of water
			depthTexture.Create ();


			//Camera creation and setup
			//replacementCamera = new Camera();
			bufferRenderingManagerGO = new GameObject("ScattererBufferManager");
			replacementCamera = bufferRenderingManagerGO.AddComponent<Camera>();

			replacementCamera.CopyFrom(Core.Instance.farCamera);
			replacementCamera.transform.parent = Core.Instance.farCamera.transform;
			
			replacementCamera.enabled = false;
			replacementCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
			replacementCamera.nearClipPlane=Core.Instance.nearCamera.nearClipPlane;
			replacementCamera.depthTextureMode=DepthTextureMode.None;

			//Shader setup
			depthShader = ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthTexture")];

			mapEVEshadowProjectors ();			
		}

		void OnPreRender () 
		{
			//update Camera
			replacementCamera.fieldOfView = Core.Instance.farCamera.fieldOfView;

			//check buffers are created
			//if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				if (!depthTexture.IsCreated ())
				{
					depthTexture.Create ();
				}
			}

			//Render depth buffer
			if( (Core.Instance.fullLensFlareReplacement && Core.Instance.isGlobalPQSEnabled) || Core.Instance.isPQSEnabledOnScattererPlanet) //disable rendering when away from PQS
			{
				//Clear depth texture to white
				RenderTexture rt=RenderTexture.active;
				RenderTexture.active= depthTexture;			
				GL.Clear(false,true,Color.white);
				
				//disable EVE shadow projector
				disableEVEshadowProjectors();

				//render
				replacementCamera.targetTexture = depthTexture;
				replacementCamera.RenderWithShader (depthShader, "RenderType");				
				depthTextureCleared = false;

				//TODO render godrays?
//				if (Core.Instance.useGodrays)
//				{
//					RenderTexture.active= _godrayDepthTex;			
//					GL.Clear(false,true,Color.white);
//					
//					_depthCamCamera.targetTexture =  _godrayDepthTex;
//					_depthCamCamera.cullingMask=32768; //ignore ships, parts, to avoid black godrays casting from ship
//					_depthCamCamera.RenderWithShader (GodrayDepthShader, "RenderType");
//				}
				
				//re-enable EVE shadow projector
				enableEVEshadowProjectors();
				
				//restore active rendertexture
				RenderTexture.active=rt;
			}
		}
		
		//for debugging purposes
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (Core.Instance.depthbufferEnabled)
			{
				Graphics.Blit (depthTexture, destination);
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
				Debug.Log ("[Scatterer] Custom depth buffer: null EVE shadow projectors, remapping...");
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
			if (bufferRenderingManagerGO)
				UnityEngine.Object.Destroy (bufferRenderingManagerGO);
		}
		
		
	}
}