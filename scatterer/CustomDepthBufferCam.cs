using System;
using UnityEngine;
using System.Collections;

namespace scatterer
{

public class CustomDepthBufferCam : MonoBehaviour
{
	
		public Camera inCamera;

	public RenderTexture _depthTex;
	private GameObject _depthCam;

		private Shader depthShader;
		
			
	void OnPreRender () 
	{
//		if (!enabled || !gameObject.active) return;

		if (!_depthCam) {
			_depthCam = new GameObject("CustomDepthCamera");
			_depthCam.AddComponent<Camera>();
			_depthCam.camera.enabled = true;
			//_depthCam.hideFlags = HideFlags.HideAndDontSave;
			_depthCam.camera.depthTextureMode = DepthTextureMode.None;

			depthShader = ShaderTool.GetShader2("CompiledDepthTexture.shader");
		}

		_depthCam.camera.CopyFrom(inCamera);
		//_depthCam.camera.backgroundColor = new Color(0,0,0,0);
		//_depthCam.camera.clearFlags = CameraClearFlags.SolidColor;
		//_depthCam.camera.cullingMask = 1 << LayerMask.NameToLayer("Character1") | 
		//	1 << LayerMask.NameToLayer("Character2");
		_depthCam.camera.targetTexture = _depthTex;

		
		_depthCam.camera.SetReplacementShader(depthShader,"");

		_depthCam.camera.RenderWithShader(depthShader,"");
	}
	
	//void OnRenderImage (RenderTexture source, RenderTexture destination) {
		//material.SetTexture("_DepthNormal", _depthTex);
		//ImageEffects.BlitWithMaterial(material, source, destination);
		//CleanUpTextures();
	//}		
	

}
}