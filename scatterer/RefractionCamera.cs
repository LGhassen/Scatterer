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
			
			_refractionCamCamera.farClipPlane=Core.Instance.farCamera.farClipPlane;
			_refractionCamCamera.nearClipPlane=Core.Instance.farCamera.nearClipPlane;
			_refractionCamCamera.depthTextureMode=DepthTextureMode.None;
			
			_refractionCamCamera.transform.parent=Core.Instance.farCamera.transform;			
			_refractionCamCamera.enabled = false;
		}

		//void OnPreRender () 
		public void OnPreCull()
		{
			_refractionCamCamera.CopyFrom(inCamera);
			_refractionCamCamera.enabled = false;

			//disable rendering when away from PQS
			bool renderRefractionBuffer = false;
			if (FlightGlobals.ActiveVessel)
			{
				if (FlightGlobals.ActiveVessel.orbit.referenceBody.pqsController)
					renderRefractionBuffer = FlightGlobals.ActiveVessel.orbit.referenceBody.pqsController.isActive;
			}
			renderRefractionBuffer = renderRefractionBuffer || Core.Instance.pqsEnabled;

			if (renderRefractionBuffer && postProcessingCube)
			{
				//take a random frustum corner and compute the angle to the camera forward direction
				//there is probably a simple formula to do this but I'm feeling lazy today so using the unity methods
				Vector3 topLeft = _refractionCamCamera.ViewportPointToRay(new Vector3(0f,1f,0f)).direction;
				topLeft.Normalize();
				
				float angle = Vector3.Dot (topLeft, _refractionCamCamera.transform.forward);
				
				_refractionCamCamera.nearClipPlane=Mathf.Max(iSkyNode.trueAlt * angle,Core.Instance.nearCamera.nearClipPlane);
				_refractionCamCamera.farClipPlane = Mathf.Max (300f, 200 * _refractionCamCamera.nearClipPlane); //magic

				//for some reason in KSP this camera wouldn't clear the texture before rendering to it, resulting in a trail effect
				//this snippet fixes that. We need the texture cleared to full white to mask the sky
				RenderTexture rt=RenderTexture.active;
				RenderTexture.active= _refractionTex;			
				GL.Clear(false,true,Color.black);
				//here disable the ocean and the postprocessing stuff
				//can we disable EVE clouds here as well?
				//also, disable it over a certain altitude
				bool prev = postProcessingCube.enabled;
				postProcessingCube.enabled = false;

				bool prev2=false;

				if (underwaterPostProcessing)
				{
					prev2 = underwaterPostProcessing.enabled;
					underwaterPostProcessing.enabled =false;
				}

				for (int i=0; i < numGrids; i++)
				{
					waterMeshRenderers[i].enabled=false;
				}

				//render
				_refractionCamCamera.targetTexture = _refractionTex;
				_refractionCamCamera.Render();

				//here re-enable the ocean and the postprocessing stuff
				postProcessingCube.enabled = prev;
				if (underwaterPostProcessing)
					underwaterPostProcessing.enabled = prev2;

				for (int i=0; i < numGrids; i++)
				{
					waterMeshRenderers[i].enabled=true;
				}

				//restore active rendertexture
				RenderTexture.active=rt;
			}
		}
		
		
//		void OnRenderImage (RenderTexture source, RenderTexture destination)
//		{
//			if (Core.Instance.depthbufferEnabled)
//			{
//				Graphics.Blit (_depthTex, destination);
//			}
//		}
		
//		public void clearDepthTexture()
//		{
//			RenderTexture rt=RenderTexture.active;
//			RenderTexture.active= _depthTex;			
//			GL.Clear(false,true,Color.white);
//			RenderTexture.active=rt;
//			
//			depthTextureCleared = true;
//		}
		
		public void OnDestroy ()
		{
			UnityEngine.Object.Destroy (_refractionCam);
		}
		
		
	}
}