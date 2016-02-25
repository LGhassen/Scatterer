using System;
using UnityEngine;
using System.Collections;

namespace scatterer
{
	
	public class CustomDepthBufferCam : MonoBehaviour
	{
		
		public Camera inCamera;
		
		public Core incore;
		
		public RenderTexture _depthTex;
		public RenderTexture _godrayDepthTex;
		private GameObject _depthCam;
		
		public bool depthTextureCleared = false; //clear depth texture when away from PQS, for the sunflare shader
		//Later I'll make the shader stop checking the depth buffer instead
		
		//private Material viewCustomBufferShader;
		
		private Shader depthShader;
		private Shader GodrayDepthShader;
		
		
		void OnPreRender () 
		{
			//		if (!enabled || !gameObject.active) return;
			
			if (!_depthCam) {
				_depthCam = new GameObject("CustomDepthCamera");
				_depthCam.AddComponent<Camera>();

				_depthCam.camera.CopyFrom(inCamera);
				
				_depthCam.camera.farClipPlane=incore.farCamera.farClipPlane;
				_depthCam.camera.nearClipPlane=incore.farCamera.nearClipPlane;
				_depthCam.camera.depthTextureMode=DepthTextureMode.None;

				_depthCam.camera.transform.parent=incore.farCamera.transform;

//				_depthCam.camera.enabled = true;
				_depthCam.camera.enabled = false;



				depthShader = ShaderTool.GetShader2("CompiledDepthTexture.shader");


				if (incore.useGodrays)
					GodrayDepthShader=ShaderTool.GetShader2("CompiledGodrayDepthTexture.shader");

			}

			_depthCam.camera.CopyFrom(inCamera);
			_depthCam.camera.enabled = false;

			//_depthCam.camera.backgroundColor = new Color(0,0,0,0);
			//_depthCam.camera.clearFlags = CameraClearFlags.SolidColor;
			
			//inCamera.camera.SetReplacementShader(depthShader,"RenderType");
			
			//disable rendering of the custom depth buffer when away from PQS
			if (incore.pqsEnabled)   //change this to render at any PQS
			{


				//for some reason in KSP this camera wouldn't clear the texture before rendering to it, resulting in a trail effect
				//this snippet fixes that. We need the texture cleared to full white to mask the sky
				RenderTexture rt=RenderTexture.active;
				RenderTexture.active= _depthTex;			
				GL.Clear(false,true,Color.white);
				//needed only with Rfloat rendertexture (for godray)
				//not needed for built-in depth
				
				
				_depthCam.camera.targetTexture = _depthTex;
//				_depthCam.camera.SetReplacementShader (depthShader, "RenderType");
				_depthCam.camera.RenderWithShader (depthShader, "RenderType");
				depthTextureCleared = false;
				
				if (incore.useGodrays)
				{
					RenderTexture.active= _godrayDepthTex;			
					GL.Clear(false,true,Color.white);

					
					_depthCam.camera.targetTexture =  _godrayDepthTex;
//					_depthCam.camera.SetReplacementShader (GodrayDepthShader, "RenderType");
					_depthCam.camera.RenderWithShader (GodrayDepthShader, "RenderType");
				}
				
				RenderTexture.active=rt;
			}
		}
		
		
		void OnRenderImage (RenderTexture source, RenderTexture destination) {
			//		material.SetTexture("_DepthNormal", _depthTex);
			//		ImageEffects.BlitWithMaterial(material, source, destination);
			//		CleanUpTextures();
			//			RenderTexture.active = _depthCam;
			//			Texture2D dest = new Texture2D (Screen.width, Screen.height);
			//			_depthCam.camera.targetTexture = destination;
			//			
			//			
			//			_depthCam.camera.SetReplacementShader(depthShader,"RenderType");
			//			
			//			
			//			_depthCam.camera.RenderWithShader(depthShader,"RenderType");
			
			if (incore.depthbufferEnabled)
			{
				//				Graphics.Blit (_depthTex, destination, viewCustomBufferShader, 0);
				Graphics.Blit (_depthTex, destination);
				//				Graphics.Blit (_godrayDepthTex, destination);
				
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