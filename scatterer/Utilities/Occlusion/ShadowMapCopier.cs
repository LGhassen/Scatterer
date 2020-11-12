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


namespace scatterer
{
	public class ShadowMapCopier : MonoBehaviour
	{
		private CommandBuffer copyCascadeCB0, copyCascadeCB1, copyCascadeCB2, copyCascadeCB3;
		private Light m_Light;
		bool commandBufferAdded = false;

		private static ShadowMapCopier instance;

		public static ShadowMapCopier Instance
		{
			get
			{
				if (instance==null)
				{
					instance = ScaledCamera.Instance.galaxyCamera.gameObject.AddComponent<ShadowMapCopier>();
				}
				return instance;
			}
		}
		
		public ShadowMapCopier ()
		{
			m_Light = Scatterer.Instance.sunLight;
			CreateCopyCascadeCBs ();
			GameEvents.onGameSceneLoadRequested.Add(RecreateForSceneChange);
		}

		public void RecreateForSceneChange(GameScenes scene)
		{
			StartCoroutine (DelayedRecreateForSceneChange());
		}

		//When scene changes, the resolution of the shadowMap can change so recreate the commandBuffers
		IEnumerator DelayedRecreateForSceneChange()
		{
			for (int i=0; i<3; i++)
				yield return new WaitForFixedUpdate ();

			Disable ();
			CreateCopyCascadeCBs();
		}

		private void CreateCopyCascadeCBs()
		{
			copyCascadeCB0 = CreateCopyCascadeCB (ShadowMapCopy.RenderTexture,   0f,   0f, 0.5f, 0.5f);
			copyCascadeCB1 = CreateCopyCascadeCB (ShadowMapCopy.RenderTexture, 0.5f,   0f, 0.5f, 0.5f);
			copyCascadeCB2 = CreateCopyCascadeCB (ShadowMapCopy.RenderTexture,   0f, 0.5f, 0.5f, 0.5f);
			copyCascadeCB3 = CreateCopyCascadeCB (ShadowMapCopy.RenderTexture, 0.5f, 0.5f, 0.5f, 0.5f);
		}

		private CommandBuffer CreateCopyCascadeCB(RenderTexture targetRt, float startX, float startY, float width, float height)
		{
			CommandBuffer cascadeCopyCB = new CommandBuffer();
			Rect cascadeRect = new Rect ((int)(startX * targetRt.width), (int)(startY * targetRt.height), (int)(width * targetRt.width), (int)(height * targetRt.height));

			cascadeCopyCB.EnableScissorRect(cascadeRect);
			cascadeCopyCB.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
			cascadeCopyCB.Blit (BuiltinRenderTextureType.CurrentActive,targetRt);
			cascadeCopyCB.DisableScissorRect();

			return cascadeCopyCB;
		}

		public void Disable()
		{
			m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB0);
			m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB1);
			m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB2);
			m_Light.RemoveCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB3);

			commandBufferAdded = false;
		}

		public void Enable ()
		{
			if (!commandBufferAdded)
			{
				m_Light.AddCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB0, ShadowMapPass.DirectionalCascade0);
				m_Light.AddCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB1, ShadowMapPass.DirectionalCascade1);
				m_Light.AddCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB2, ShadowMapPass.DirectionalCascade2);
				m_Light.AddCommandBuffer(LightEvent.AfterShadowMapPass, copyCascadeCB3, ShadowMapPass.DirectionalCascade3);

				commandBufferAdded = true;
			}
		}

		public void OnPreRender()
		{
			if (commandBufferAdded)
				Disable ();
		}

		public void RequestShadowMapCopy()
		{
			if (!commandBufferAdded)
				Enable ();
		}

		public void OnDestroy ()
		{
			Disable ();
		}
	}

	public static class ShadowMapCopy
	{
		private static RenderTexture renderTexture;

		public static RenderTexture RenderTexture
		{
			get 
			{
				if (ReferenceEquals(renderTexture,null))
				{
					CreateTexture();
				}
				else
				{
					//If the size of the shadowMap changed from the last time we created a copy
					if (Scatterer.Instance.sunLight.shadowCustomResolution != renderTexture.width)
					{
						renderTexture.Release();
						CreateTexture();
					}
				}

				return renderTexture;
			}
		}

		private static void CreateTexture()
		{
			renderTexture = new RenderTexture ((int) Scatterer.Instance.sunLight.shadowCustomResolution, (int) Scatterer.Instance.sunLight.shadowCustomResolution, 0, RenderTextureFormat.RHalf); //QualitySettings return 2x2? Try anyway, RFloat may not be right, shadowMaps may use half? check with nsight
			renderTexture.useMipMap = false;
			renderTexture.antiAliasing = 1;
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.Create ();
		}
	}
}

