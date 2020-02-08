using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	
	public class BufferRenderingManager : MonoBehaviour
	{
		public RenderTexture depthTexture;			//full-scene depth texture, from merged built-in depth textures of the two local cameras
		public RenderTexture refractionTexture;		//textures for the refractions, created once and accessed from here, written to from oceanNode
		//public RenderTexture occlusionTexture;	//for SSAO and eclipses, for now will just contain a copy of the screenspace shadowmask

		public bool depthTextureCleared = false; 	//clear depth texture when away from PQS, for the sunflare shader

		public void start()
		{
			createTextures();
		}

		void OnPreRender () 
		{
			if (!depthTexture || !depthTexture.IsCreated())
			{
				Utils.LogDebug("BufferRenderingManager: Recreating textures");
				createTextures();
				Core.Instance.onRenderTexturesLost();
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