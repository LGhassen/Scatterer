using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	
	public class RefractionCamera : MonoBehaviour
	{
		public Camera inCamera;
		
		private GameObject _refractionCam;
		public Camera _refractionCamCamera;

		public RenderTexture _refractionTex;

		public MeshRenderer[] waterMeshRenderers;
		public MeshRenderer postProcessingCube;
		public MeshRenderer underwaterPostProcessing;
		public int numGrids=0;

		public SkyNode iSkyNode;

		public void start()
		{
			_refractionCam = new GameObject("RefractionCamera");
			_refractionCamCamera = _refractionCam.AddComponent<Camera>();
			
			_refractionCamCamera.CopyFrom(inCamera);
			_refractionCamCamera.transform.parent = inCamera.transform;
			
			_refractionCamCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
			_refractionCamCamera.nearClipPlane=Core.Instance.farCamera.nearClipPlane;
			_refractionCamCamera.depthTextureMode=DepthTextureMode.None;
			
			_refractionCamCamera.transform.parent=Core.Instance.farCamera.transform;			
			_refractionCamCamera.enabled = false;
		}

		//void OnPreRender () 
		public void OnPreCull()
		{
			if (!ReferenceEquals (iSkyNode, null))
			{
				if (postProcessingCube && iSkyNode.m_manager.GetOceanNode ().renderRefractions)
				{
					_refractionCamCamera.fieldOfView = inCamera.fieldOfView;
					_refractionCamCamera.enabled = false;
					
					_refractionCamCamera.cullingMask = 9076737; //essentially the same as farcamera except ignoring transparentFX
					//the idea is to move clouds (and maybe cloud shadow projectors?) and water shaders to transparentFX to improve performance

					//take a random frustum corner and compute the angle to the camera forward direction
					//there is probably a simple formula to do this
					Vector3 topLeft = _refractionCamCamera.ViewportPointToRay (new Vector3 (0f, 1f, 0f)).direction;
					topLeft.Normalize ();

					float angle = Vector3.Dot (topLeft, _refractionCamCamera.transform.forward);
				
					_refractionCamCamera.nearClipPlane = Mathf.Max (iSkyNode.trueAlt * angle, Core.Instance.nearCamera.nearClipPlane);
					_refractionCamCamera.farClipPlane = Mathf.Max (300f, 200 * _refractionCamCamera.nearClipPlane); //magic

					//for some reason in KSP this camera wouldn't clear the texture before rendering to it, resulting in a trail effect
					//this snippet fixes that. We need the texture cleared to full black to mask the sky
					RenderTexture rt = RenderTexture.active;
					RenderTexture.active = _refractionTex;
					GL.Clear (false, true, Color.black);
					//here disable the ocean and the postprocessing stuff
					//can we disable EVE clouds here as well?
					bool prev = postProcessingCube.enabled;
					postProcessingCube.enabled = false;
					bool prev2 = false;
					if (underwaterPostProcessing) {
						prev2 = underwaterPostProcessing.enabled;
						underwaterPostProcessing.enabled = false;
					}

					for (int i=0; i < numGrids; i++) {
						waterMeshRenderers [i].enabled = false;
					}

					//render
					_refractionCamCamera.targetTexture = _refractionTex;
					_refractionCamCamera.Render ();

					//here re-enable the ocean and the postprocessing stuff
					postProcessingCube.enabled = prev;
					if (underwaterPostProcessing)
						underwaterPostProcessing.enabled = prev2;

					for (int i=0; i < numGrids; i++) {
						waterMeshRenderers [i].enabled = true;
					}

					//restore active rendertexture
					RenderTexture.active = rt;
				}
			}
		}
		
		public void OnDestroy ()
		{
			UnityEngine.Object.Destroy (_refractionCam);
		}
		
		
	}
}