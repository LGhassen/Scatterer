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
	private GameObject _depthCam;

		private Material viewCustomBufferShader;

		private Shader depthShader;
		
			
	void OnPreRender () 
	{
//		if (!enabled || !gameObject.active) return;

		if (!_depthCam) {
			_depthCam = new GameObject("CustomDepthCamera");
			_depthCam.AddComponent<Camera>();
			_depthCam.camera.enabled = true;
			//_depthCam.hideFlags = HideFlags.HideAndDontSave;
//			_depthCam.camera.depthTextureMode = DepthTextureMode.None;

			depthShader = ShaderTool.GetShader2("CompiledDepthTexture.shader");

			viewCustomBufferShader = ShaderTool.GetMatFromShader2 ("CompiledviewCustomDepthTexture.shader");
			viewCustomBufferShader.SetTexture ("_DepthTex", _depthTex);
		}

		_depthCam.camera.CopyFrom(inCamera);
		//_depthCam.camera.backgroundColor = new Color(0,0,0,0);
		//_depthCam.camera.clearFlags = CameraClearFlags.SolidColor;
		//_depthCam.camera.cullingMask = 1 << LayerMask.NameToLayer("Character1") | 
		//	1 << LayerMask.NameToLayer("Character2");


		


			//inCamera.camera.SetReplacementShader(depthShader,"RenderType");

			//disable rendering of the custom depth buffer when away from PQS
			if (incore.pqsEnabled)
			{
				_depthCam.camera.targetTexture = _depthTex;
			
				_depthCam.camera.SetReplacementShader (depthShader, "RenderType");

				_depthCam.camera.RenderWithShader (depthShader, "RenderType");
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

//			if (incore.depthbufferEnabled)
//			{
//				Graphics.Blit (_depthTex, destination, viewCustomBufferShader, 0);
//			}

	}		
	

}
}