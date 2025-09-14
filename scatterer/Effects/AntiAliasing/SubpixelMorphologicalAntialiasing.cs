using System;
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

        private static CameraEvent SMAACameraEvent = CameraEvent.AfterForwardAlpha; // BeforeImageEffects doesn't work well

        Quality quality;

        public void forceDepthBuffermode()
        {
            quality = Quality.DepthMode;
            initialized = false;
        }

		int flipRenderTargetNameID, flopRenderTargetNameID;

		static Texture2D areaTex, searchTex;
		bool initialized = false;
		bool hdrEnabled = false;

        public Quality QualityUsed { get => quality; }

        public void Awake()
        {
            targetCamera = GetComponent<Camera> ();
            
            targetCamera.forceIntoRenderTexture = true;

			hdrEnabled = targetCamera.allowHDR;

			flipRenderTargetNameID = Shader.PropertyToID("_SMAA_Flip");
			flopRenderTargetNameID = Shader.PropertyToID("_SMAA_Flop");

            SMAAMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/SubpixelMorphologicalAntialiasing")]);

            if (areaTex == null)
                areaTex = (Texture2D) ShaderReplacer.Instance.LoadedTextures ["AreaTex"];
            if (searchTex == null)
                searchTex = (Texture2D)ShaderReplacer.Instance.LoadedTextures ["SearchTex"];

            SMAAMaterial.SetTexture("_AreaTex"  , areaTex);
            SMAAMaterial.SetTexture("_SearchTex", searchTex);

            quality = (Quality)((Int32) Mathf.Clamp( (float) Scatterer.Instance.mainSettings.smaaQuality,1f,2f));    //limit to 1 and 2, 1 and 2 are image-based. 0 is depth-based and would make performance worse by having to run for each camera's depth
                                                                                                                    //only use depth mode when we force it for the IVA camera

            SMAACommandBuffer = new CommandBuffer ();
            SMAACommandBuffer.name = $"Scatterer SMAA CommandBuffer for {targetCamera.name}";
        }

        static readonly int MainTextureProperty = Shader.PropertyToID("_MainTexture");
        static readonly int BlendTexproperty = Shader.PropertyToID("_BlendTex");

        public void OnPreCull()
		{
			bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);

			if ((!initialized || targetCamera.allowHDR != hdrEnabled) && !screenShotModeEnabled)
			{
				SMAACommandBuffer.Clear ();

				hdrEnabled = targetCamera.allowHDR;
				var colorFormat = hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32;

				SMAACommandBuffer.GetTemporaryRT(flipRenderTargetNameID, -1, -1, 0, FilterMode.Bilinear, colorFormat);
				// TODO: how do we make sure to match the previous settings?  Do we need to use RenderTextureDescriptor?
				//flip.anisoLevel = 1;
				//flip.antiAliasing = 1;
				//flip.volumeDepth = 0;
				//flip.useMipMap = false;
				//flip.autoGenerateMips = false;
				//flip.wrapMode = TextureWrapMode.Clamp;
				//flip.filterMode = FilterMode.Bilinear;

				SMAACommandBuffer.GetTemporaryRT(flopRenderTargetNameID, -1, -1, 0, FilterMode.Bilinear, colorFormat);

				SMAACommandBuffer.SetRenderTarget (flopRenderTargetNameID);
				SMAACommandBuffer.ClearRenderTarget (false, true, Color.clear);
			
				SMAACommandBuffer.SetRenderTarget (flipRenderTargetNameID);
				SMAACommandBuffer.ClearRenderTarget (false, true, Color.clear);
			
				SMAACommandBuffer.SetGlobalTexture (MainTextureProperty, BuiltinRenderTextureType.CameraTarget);

				SMAACommandBuffer.Blit (null, flipRenderTargetNameID, SMAAMaterial, (int)Pass.EdgeDetection + (int)quality);		//screen to flip with edge detection
			
				SMAACommandBuffer.SetGlobalTexture (MainTextureProperty, flipRenderTargetNameID);
				SMAACommandBuffer.Blit (null, flopRenderTargetNameID, SMAAMaterial, (int)Pass.BlendWeights + (int)quality);		//flip to flop with blendweights
				SMAACommandBuffer.SetGlobalTexture (BlendTexproperty, flopRenderTargetNameID);
				SMAACommandBuffer.SetGlobalTexture (MainTextureProperty, BuiltinRenderTextureType.CameraTarget);
				SMAACommandBuffer.Blit (null, flipRenderTargetNameID, SMAAMaterial, (int)Pass.NeighborhoodBlending);				//neighborhood blending to flip
				SMAACommandBuffer.Blit (flipRenderTargetNameID, BuiltinRenderTextureType.CameraTarget);							//blit back to screen
			
				targetCamera.AddCommandBuffer (SMAACameraEvent, SMAACommandBuffer);
				initialized = true;
			}

            if (initialized && screenShotModeEnabled)
            {
                targetCamera.RemoveCommandBuffer(SMAACameraEvent, SMAACommandBuffer);
                initialized = false;
            }
		}
		
		public void OnDestroy()
		{
			SMAAMaterial = null;

			if (SMAACommandBuffer !=null)
			{
				targetCamera.RemoveCommandBuffer (SMAACameraEvent, SMAACommandBuffer);
				SMAACommandBuffer.Clear();
				SMAACommandBuffer.Release();
				SMAACommandBuffer = null;
				initialized = false;
			}
		}
	}
}
