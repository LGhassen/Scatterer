using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
	public class SubpixelMorphologicalAntialiasing : GenericAntiAliasing
	{
		Camera targetCamera;
		CommandBuffer SMAACommandBuffer;
		Material SMAAMaterial;

		enum Pass { EdgeDetection = 0, BlendWeights = 3, NeighborhoodBlending = 6 }
		public enum Quality { DepthMode = 0, Medium = 1, High = 2 }
		
		Quality quality;

		public void forceDepthBuffermode()
		{
			quality = Quality.DepthMode;
			initialized = false;
		}

		RenderTexture flip, flop;
		static Texture2D areaTex, searchTex;
		bool initialized = false;

		public SubpixelMorphologicalAntialiasing()
		{
			targetCamera = GetComponent<Camera> ();
			
			targetCamera.forceIntoRenderTexture = true;

			int width, height;
			
			if (targetCamera.activeTexture)
			{
				width = targetCamera.activeTexture.width;
				height = targetCamera.activeTexture.height;
			}
			else
			{
				width = Screen.width;
				height = Screen.height;
			}
			
			flip = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32); //TODO: hdr support for these
			flip.anisoLevel = 1;
			flip.antiAliasing = 1;
			flip.volumeDepth = 0;
			flip.useMipMap = false;
			flip.autoGenerateMips = false;
			flip.wrapMode = TextureWrapMode.Clamp;
			flip.filterMode = FilterMode.Bilinear;
			flip.Create ();

			flop = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32);
			flop.anisoLevel = 1;
			flop.antiAliasing = 1;
			flop.volumeDepth = 0;
			flop.useMipMap = false;
			flop.autoGenerateMips = false;
			flip.wrapMode = TextureWrapMode.Clamp;
			flip.filterMode = FilterMode.Bilinear;
			flop.Create ();

			SMAAMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/SubpixelMorphologicalAntialiasing")]);

			if (areaTex == null)
				areaTex = (Texture2D) ShaderReplacer.Instance.LoadedTextures ["AreaTex"];
			if (searchTex == null)
				searchTex = (Texture2D)ShaderReplacer.Instance.LoadedTextures ["SearchTex"];

			SMAAMaterial.SetTexture("_AreaTex"  , areaTex);
			SMAAMaterial.SetTexture("_SearchTex", searchTex);

			quality = (Quality)((Int32) Mathf.Clamp( (float) Scatterer.Instance.mainSettings.smaaQuality,1f,2f));	//limit to 1 and 2, 1 and 2 are image-based. 0 is depth-based and would make performance worse by having to run for each camera's depth
																													//only use depth mode when we force it for the IVA camera

			SMAACommandBuffer = new CommandBuffer ();
		}

		public void OnPreCull()
		{
			if (!initialized)
			{
				SMAACommandBuffer.Clear ();
			
				SMAACommandBuffer.SetRenderTarget (flop);
				SMAACommandBuffer.ClearRenderTarget (false, true, Color.clear);
			
				SMAACommandBuffer.SetRenderTarget (flip);
				SMAACommandBuffer.ClearRenderTarget (false, true, Color.clear);
			
				SMAACommandBuffer.SetGlobalTexture ("_MainTexture", BuiltinRenderTextureType.CameraTarget);

				SMAACommandBuffer.Blit (null, flip, SMAAMaterial, (int)Pass.EdgeDetection + (int)quality);		//screen to flip with edge detection
			
				SMAACommandBuffer.SetGlobalTexture ("_MainTexture", flip);
				SMAACommandBuffer.Blit (null, flop, SMAAMaterial, (int)Pass.BlendWeights + (int)quality);		//flip to flop with blendweights
				SMAACommandBuffer.SetGlobalTexture ("_BlendTex", flop);
				SMAACommandBuffer.SetGlobalTexture ("_MainTexture", BuiltinRenderTextureType.CameraTarget);
				SMAACommandBuffer.Blit (null, flip, SMAAMaterial, (int)Pass.NeighborhoodBlending);				//neighborhood blending to flip
				SMAACommandBuffer.Blit (flip, BuiltinRenderTextureType.CameraTarget);							//blit back to screen
			
				targetCamera.AddCommandBuffer (CameraEvent.AfterForwardAlpha, SMAACommandBuffer); 				// BeforeImageEffects doesn't work well so use this
				initialized = true;
			}
		}
		
		public void OnDestroy()
		{
			SMAAMaterial = null;

			if (SMAACommandBuffer !=null)
			{
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, SMAACommandBuffer);
				SMAACommandBuffer.Clear();
			}
			
			if (flip)
				flip.Release ();

			if (flop)
				flop.Release ();
		}
	}
}
