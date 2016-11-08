using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	
	public class CustomDepthBufferCam : MonoBehaviour
	{
		List<Projector> EVEprojector=new List<Projector> {};
		int projectorCount=0;

		public Camera inCamera;

		public RenderTexture _depthTex;
		public RenderTexture _godrayDepthTex;
		private GameObject _depthCam;
		private Camera _depthCamCamera;
		
		public bool depthTextureCleared = false; //clear depth texture when away from PQS, for the sunflare shader
		//Later I'll make the shader stop checking the depth buffer instead
		
		private Shader depthShader;
		private Shader GodrayDepthShader;
		
		public void start()
		{
			_depthCam = new GameObject("CustomDepthCamera");
			_depthCamCamera = _depthCam.AddComponent<Camera>();
			
			_depthCamCamera.CopyFrom(inCamera);
			
			_depthCamCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
			_depthCamCamera.nearClipPlane=Core.Instance.farCamera.nearClipPlane;
			_depthCamCamera.depthTextureMode=DepthTextureMode.None;
			
			_depthCamCamera.transform.parent=Core.Instance.farCamera.transform;			
			_depthCamCamera.enabled = false;
			
			depthShader = ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthTexture")];
			
			if (Core.Instance.useGodrays)
				GodrayDepthShader=ShaderReplacer.Instance.LoadedShaders[("Scatterer/GodrayDepthTexture")];


			mapEVEshadowProjectors ();
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

		void OnPreRender () 
		{
			_depthCamCamera.CopyFrom(inCamera);
			_depthCamCamera.enabled = false;

			//disable rendering of the custom depth buffer when away from PQS
			bool renderDepthBuffer = false;
			if (FlightGlobals.ActiveVessel)
			{
				if (FlightGlobals.ActiveVessel.orbit.referenceBody.pqsController)
					renderDepthBuffer = FlightGlobals.ActiveVessel.orbit.referenceBody.pqsController.isActive;
			}

			renderDepthBuffer = renderDepthBuffer || Core.Instance.pqsEnabled;

			if (renderDepthBuffer)
			{
				//for some reason in KSP this camera wouldn't clear the texture before rendering to it, resulting in a trail effect
				//this snippet fixes that. We need the texture cleared to full white to mask the sky
				RenderTexture rt=RenderTexture.active;
				RenderTexture.active= _depthTex;			
				GL.Clear(false,true,Color.white);
							
				//disable EVE shadow projector
				int i=0;
				try
				{
					for(i=0;i<projectorCount;i++)
					{
						EVEprojector[i].enabled=false;
					}
				}
				catch(Exception)
				{
					Debug.Log("[Scatterer] Custom depth buffer: null EVE shadow projectors, remapping...");
					mapEVEshadowProjectors();
				}

				_depthCamCamera.targetTexture = _depthTex;
				_depthCamCamera.RenderWithShader (depthShader, "RenderType");
							
				depthTextureCleared = false;
				
				if (Core.Instance.useGodrays)
				{
					RenderTexture.active= _godrayDepthTex;			
					GL.Clear(false,true,Color.white);

					_depthCamCamera.targetTexture =  _godrayDepthTex;
					_depthCamCamera.cullingMask=32768; //ignore ships, parts, to avoid black godrays casting from ship
					_depthCamCamera.RenderWithShader (GodrayDepthShader, "RenderType");
				}

				//re-enable EVE shadow projector
				for(i=0;i<projectorCount;i++)
				{
					EVEprojector[i].enabled=true;
				}

				//restore active rendertexture
				RenderTexture.active=rt;
			}
		}
		
		
		void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
			if (Core.Instance.depthbufferEnabled)
			{
				Graphics.Blit (_depthTex, destination);
			}
		}
		
		public void clearDepthTexture()
		{
			RenderTexture rt=RenderTexture.active;
			RenderTexture.active= _depthTex;			
			GL.Clear(false,true,Color.white);
			RenderTexture.active=rt;
			
			depthTextureCleared = true;
		}
		
		public void OnDestroy ()
		{
			UnityEngine.Object.Destroy (_depthCam);
		}
		
		
	}
}