//rendering steps
//scaledSpaceCamera.OnPrecull -> skynodes update the extinction texture one after one
//camerahook on the relevant scaledSpace or farCamera -> clear the extinction texture before the next frame
//while taking care of keeping it around until the rendering has finished (either nearCamera or scaledCamera)

using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	public class SunFlare : MonoBehaviour
	{
		public ConfigNode configNodeToLoad;

		public Material sunglareMaterial;

		public CelestialBody source;
		public string sourceName;
		public Transform sourceScaledTransform;

		//Size is loaded automatically from the files
		Texture2D sunSpikes = new Texture2D (1, 1);
		Texture2D sunFlare  = new Texture2D (1, 1);
		Texture2D sunGhost1 = new Texture2D (1, 1);
		Texture2D sunGhost2 = new Texture2D (1, 1);
		Texture2D sunGhost3 = new Texture2D (1, 1);

		public RenderTexture extinctionTexture;
		int waitBeforeReloadCnt = 0;
		SunflareCameraHook nearCameraHook, scaledCameraHook;

		Vector3 sunViewPortPos=Vector3.zero;

		RaycastHit hit;
		bool hitStatus=false;
		bool eclipse=false;

		float sunGlareScale=1;
		float sunGlareFade=1;
		float ghostFade=1;

		Mesh screenMesh;
		GameObject sunflareGameObject;
		
		public int syntaxVersion = 1;

		//Syntax V1 settings
		SunflareSettingsV1 settingsV1;

		//Syntax V2 settings
		SunflareSettingsV2 settingsV2;


		public void start()
		{
			LoadSettings ();

			sunglareMaterial = new Material (ShaderReplacer.Instance.LoadedShaders["Scatterer/sunFlare"]);

			sunglareMaterial.SetOverrideTag ("IGNOREPROJECTOR", "True");
			sunglareMaterial.SetOverrideTag ("IgnoreProjector", "True");

			Utils.EnableOrDisableShaderKeywords (sunglareMaterial, "SCATTERER_MERGED_DEPTH_OFF", "SCATTERER_MERGED_DEPTH_ON", Scatterer.Instance.unifiedCameraMode);

			if (!Scatterer.Instance.unifiedCameraMode)
			{
				if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
					sunglareMaterial.SetTexture ("_customDepthTexture", Scatterer.Instance.bufferManager.depthTexture);
				else
					sunglareMaterial.SetTexture ("_customDepthTexture", Texture2D.whiteTexture);	//keep this in mind for when doing multiple points check and ditching raycast
			}

			sunglareMaterial.renderQueue = 3100;

			screenMesh = MeshFactory.MakePlane (2, 2, MeshFactory.PLANE.XY, false, false);
			screenMesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));

			sunflareGameObject = new GameObject ();
			MeshFilter sunflareGameObjectMeshFilter;

			if (sunflareGameObject.GetComponent<MeshFilter> ())
				sunflareGameObjectMeshFilter = sunflareGameObject.GetComponent<MeshFilter> ();
			else
				sunflareGameObjectMeshFilter = sunflareGameObject.AddComponent<MeshFilter>();
			
			sunflareGameObjectMeshFilter.mesh.Clear ();
			sunflareGameObjectMeshFilter.mesh = screenMesh;
			
			MeshRenderer sunflareGameObjectMeshRenderer;

			if (sunflareGameObject.GetComponent<MeshRenderer> ())
				sunflareGameObjectMeshRenderer = sunflareGameObject.GetComponent<MeshRenderer> ();
			else
				sunflareGameObjectMeshRenderer = sunflareGameObject.AddComponent<MeshRenderer>();
			
			sunflareGameObjectMeshRenderer.sharedMaterial = sunglareMaterial;
			sunflareGameObjectMeshRenderer.material = sunglareMaterial;
			
			sunflareGameObjectMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			sunflareGameObjectMeshRenderer.receiveShadows = false;
			sunflareGameObjectMeshRenderer.enabled = true;
			
			sunflareGameObject.layer = 10; //start in scaledspace

			scaledCameraHook = (SunflareCameraHook) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SunflareCameraHook));
			scaledCameraHook.flare = this;
			scaledCameraHook.useDbufferOnCamera = 0f;

			if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				nearCameraHook = (SunflareCameraHook)Scatterer.Instance.nearCamera.gameObject.AddComponent (typeof(SunflareCameraHook));
				nearCameraHook.flare = this;
				nearCameraHook.useDbufferOnCamera = 1f;
			}

			ApplySunflareConfig ();

			sunglareMaterial.SetFloat (ShaderProperties.aspectRatio_PROPERTY, Scatterer.Instance.scaledSpaceCamera.aspect);

			Utils.LogDebug ("Added custom sun flare for "+sourceName);
		}

		public void updateProperties()
		{
			sunViewPortPos = Scatterer.Instance.scaledSpaceCamera.WorldToViewportPoint (sourceScaledTransform.position);
			hitStatus=false;

			if (sunViewPortPos.z > 0)
			{
				if (syntaxVersion == 1)
				{
					float dist = (float) (Scatterer.Instance.scaledSpaceCamera.transform.position - sourceScaledTransform.position)
						.magnitude;

					sunGlareScale = dist / 2266660f * Scatterer.Instance.scaledSpaceCamera.fieldOfView / 60f;

					//if dist > 1.25*sunglareFadeDistance -->1
					//if dist < 0.25*sunglareFadeDistance -->0
					//else values smoothstepped in between
					sunGlareFade = Mathf.SmoothStep (0, 1, (dist / settingsV1.sunGlareFadeDistance) - 0.25f);

					//if dist < 0.5 * ghostFadeDistance -->1
					//if dist > 1.5 * ghostFadeDistance -->0
					//else values smoothstepped in between
					ghostFade = Mathf.SmoothStep (0, 1, (dist - 0.5f * settingsV1.ghostFadeDistance) / (settingsV1.ghostFadeDistance));
					ghostFade = 1 - ghostFade;

					sunglareMaterial.SetFloat (ShaderProperties.sunGlareScale_PROPERTY, sunGlareScale);
					sunglareMaterial.SetFloat (ShaderProperties.sunGlareFade_PROPERTY, sunGlareFade);
					sunglareMaterial.SetFloat (ShaderProperties.ghost1Fade_PROPERTY, ghostFade);
					sunglareMaterial.SetFloat (ShaderProperties.ghost2Fade_PROPERTY, ghostFade);
					sunglareMaterial.SetFloat (ShaderProperties.ghost3Fade_PROPERTY, ghostFade);
				}
				else if (syntaxVersion == 2)
				{
					float dist = (float) (Scatterer.Instance.scaledSpaceCamera.transform.position - sourceScaledTransform.position).magnitude / ((float) source.Radius / ScaledSpace.ScaleFactor); //distance measured in stellar radius

					//Utils.LogInfo("dist "+dist.ToString());

					if (settingsV2.flares.Count > 0)
					{
						float intensity = settingsV2.flares[0].intensityCurve.Curve.Evaluate(dist);
						float scale = settingsV2.flares[0].scaleCurve.Curve.Evaluate(dist);

						sunglareMaterial.SetVector ("flareSettings", new Vector3(intensity, settingsV2.flares[0].displayAspectRatio,1f/scale));
					}
					if (settingsV2.flares.Count > 1)
					{
						float intensity = settingsV2.flares[1].intensityCurve.Evaluate(dist);
						float scale = settingsV2.flares[1].scaleCurve.Evaluate(dist);
						sunglareMaterial.SetVector ("spikesSettings", new Vector3(intensity, settingsV2.flares[1].displayAspectRatio,1f/scale));
					}

					//ghostFade is now per ghost
					if (settingsV2.ghosts.Count > 0)
					{
						ghostFade = settingsV2.ghosts[0].intensityCurve.Evaluate(dist);
						sunglareMaterial.SetFloat (ShaderProperties.ghost1Fade_PROPERTY, ghostFade);
					}
					if (settingsV2.ghosts.Count > 1)
					{
						ghostFade = settingsV2.ghosts[1].intensityCurve.Evaluate(dist);
						sunglareMaterial.SetFloat (ShaderProperties.ghost2Fade_PROPERTY, ghostFade);
					}
					if (settingsV2.ghosts.Count > 2)
					{
						ghostFade = settingsV2.ghosts[2].intensityCurve.Evaluate(dist);
						sunglareMaterial.SetFloat (ShaderProperties.ghost3Fade_PROPERTY, ghostFade);
					}
				}

				if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				{
					
					hitStatus = Physics.Raycast (Scatterer.Instance.nearCamera.transform.position,
					                             (source.transform.position - Scatterer.Instance.nearCamera.transform.position).normalized,
					                             out hit, Mathf.Infinity, (int)((1 << 15) + (1 << 0)));
					if (!hitStatus) {
						hitStatus = Physics.Raycast (Scatterer.Instance.scaledSpaceCamera.transform.position,
						                             (sourceScaledTransform.position - Scatterer.Instance.scaledSpaceCamera.transform.position)
						                             .normalized, out hit, Mathf.Infinity, (int)((1 << 10)));
					}
				}
				else
				{
					hitStatus = Physics.Raycast (Scatterer.Instance.scaledSpaceCamera.transform.position, (sourceScaledTransform.position
					                                                                                       - Scatterer.Instance.transform.position).normalized, out hit, Mathf.Infinity, (int)((1 << 10)));
				}

				if(hitStatus)
				{
					//if sun visible, draw sunflare
					if(hit.transform == sourceScaledTransform)
						hitStatus=false;
				}

				sunglareMaterial.SetVector (ShaderProperties.sunViewPortPos_PROPERTY, sunViewPortPos);

				if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
					sunglareMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Scatterer.Instance.bufferManager.depthTexture);
			}

			eclipse = hitStatus;
			sunglareMaterial.SetFloat(ShaderProperties.renderSunFlare_PROPERTY, (!eclipse && (sunViewPortPos.z > 0) && !Scatterer.Instance.scattererCelestialBodiesManager.underwater ) ? 1.0f : 0.0f);
		}	

		public void Update()
		{
			//if rendertexture is lost, wait a bit before re-creating it
			if (!extinctionTexture.IsCreated())
			{
				waitBeforeReloadCnt++;
				if (waitBeforeReloadCnt >= 2)
				{
					extinctionTexture.Create();
					waitBeforeReloadCnt = 0;
				}
			}

			//enable or disable scaled or near script depending on trackstation or mapview
			if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				nearCameraHook.enabled = true;
				scaledCameraHook.enabled = false;
				sunflareGameObject.layer = 15;
			}
			else
			{
				if (nearCameraHook)
					nearCameraHook.enabled=false;

				scaledCameraHook.enabled=true;
				sunflareGameObject.layer = 10;
			}
		}

		public void ClearExtinction()
		{
			RenderTexture rt=RenderTexture.active;
			RenderTexture.active= extinctionTexture;			

			GL.Clear(false,true,Color.white);

			//restore active rendertexture
			RenderTexture.active=rt;
		}	

		public void CleanUp()
		{
			if (nearCameraHook)
			{
				Component.Destroy (nearCameraHook);
				UnityEngine.Object.Destroy (nearCameraHook);
			}
			if (scaledCameraHook)
			{
				Component.Destroy (scaledCameraHook);
				UnityEngine.Object.Destroy (scaledCameraHook);
			}

			if (extinctionTexture)
			{
				extinctionTexture.Release();
				UnityEngine.Object.Destroy (extinctionTexture);
			}
		}

		public void Configure(CelestialBody source, string sourceName, Transform sourceScaledTransform, ConfigNode configNodeToLoad)
		{
			this.source = source;
			this.sourceName = sourceName;
			this.sourceScaledTransform = sourceScaledTransform;
			this.configNodeToLoad = configNodeToLoad;
		}

		public void LoadSettings ()
		{
			LoadSettingsFromConfigNode (configNodeToLoad);
		}

		void LoadSettingsFromConfigNode (ConfigNode node)
		{
			if (node.HasValue ("syntaxVersion"))
			{
				if (node.TryGetValue ("syntaxVersion", ref syntaxVersion) && ((syntaxVersion == 1) || (syntaxVersion == 2)))
				{
					Utils.LogDebug ("Sunflare syntax version: " + syntaxVersion.ToString ());
				}
				else
				{
					Utils.LogDebug ("Invalid sunflare syntax version found: " + node.GetValue ("syntaxVersion") + ", defaulting to version 1 for retro-compatibility");
					syntaxVersion = 1;
				}
			}
			else
			{
				Utils.LogDebug ("No sunflare syntax version found, defaulting to version 1 for retro-compatibility");
				syntaxVersion = 1;
			}
			if (syntaxVersion == 1)
			{
				settingsV1 = new SunflareSettingsV1 ();
				ConfigNode.LoadObjectFromConfig (settingsV1, node);

			}
			else
			{
				settingsV2 = new SunflareSettingsV2 ();
				settingsV2.Load(node);
			}
		}

		
		public void ApplyFromUI(ConfigNode node)
		{
			LoadSettingsFromConfigNode (node);
			configNodeToLoad = node;
			ApplySunflareConfig ();
		}

		void ApplySunflareConfig ()
		{
			if (syntaxVersion == 1)
			{
				ApplySyntaxV1FlareConfig ();
			}
			else if (syntaxVersion == 2)
			{
				ApplySyntaxV2FlareConfig ();
			}

			extinctionTexture = new RenderTexture (4, 4, 0, RenderTextureFormat.ARGB32);
			extinctionTexture.antiAliasing = 1;
			extinctionTexture.filterMode = FilterMode.Point;
			extinctionTexture.Create ();
			sunglareMaterial.SetTexture ("extinctionTexture", extinctionTexture);
		}

		void ApplySyntaxV1FlareConfig ()
		{
			LoadAndSetTexture ("sunFlare" , sunFlare , (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "sunFlare.png")));
			LoadAndSetTexture ("sunSpikes", sunSpikes, (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "sunSpikes.png")));

			LoadAndSetTexture ("sunGhost1", sunGhost1, (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost1.png")));
			LoadAndSetTexture ("sunGhost2", sunGhost2, (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost2.png")));
			LoadAndSetTexture ("sunGhost3", sunGhost3, (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost3.png")));

			//didn't want to serialize the matrices directly as the result is pretty unreadable
			//sorry about the mess, syntax v2 is cleaner
			Matrix4x4 ghost1Settings1 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost1SettingsList1.Count; i++)
			{
				ghost1Settings1.SetRow (i, settingsV1.ghost1SettingsList1 [i]);
			}
			Matrix4x4 ghost1Settings2 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost1SettingsList2.Count; i++)
			{
				ghost1Settings2.SetRow (i, settingsV1.ghost1SettingsList2 [i]);
			}
			//ghost 2
			Matrix4x4 ghost2Settings1 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost2SettingsList1.Count; i++)
			{
				ghost2Settings1.SetRow (i, settingsV1.ghost2SettingsList1 [i]);
			}
			Matrix4x4 ghost2Settings2 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost2SettingsList2.Count; i++)
			{
				ghost2Settings2.SetRow (i, settingsV1.ghost2SettingsList2 [i]);
			}
			//ghost 3
			Matrix4x4 ghost3Settings1 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost3SettingsList1.Count; i++)
			{
				ghost3Settings1.SetRow (i, settingsV1.ghost3SettingsList1 [i]);
			}
			Matrix4x4 ghost3Settings2 = Matrix4x4.zero;
			for (int i = 0; i < settingsV1.ghost3SettingsList2.Count; i++)
			{
				ghost3Settings2.SetRow (i, settingsV1.ghost3SettingsList2 [i]);
			}

			sunglareMaterial.SetVector ("flareSettings", settingsV1.flareSettings);
			sunglareMaterial.SetVector ("spikesSettings", settingsV1.spikesSettings);
			sunglareMaterial.SetMatrix ("ghost1Settings1", ghost1Settings1);
			sunglareMaterial.SetMatrix ("ghost1Settings2", ghost1Settings2);
			sunglareMaterial.SetMatrix ("ghost2Settings1", ghost2Settings1);
			sunglareMaterial.SetMatrix ("ghost2Settings2", ghost2Settings2);
			sunglareMaterial.SetMatrix ("ghost3Settings1", ghost3Settings1);
			sunglareMaterial.SetMatrix ("ghost3Settings2", ghost3Settings2);
			sunglareMaterial.SetVector ("flareColor", settingsV1.flareColor);
		}

		void ApplySyntaxV2FlareConfig ()
		{
			sunglareMaterial.SetVector ("flareSettings", Vector3.zero);
			sunglareMaterial.SetVector ("spikesSettings", Vector3.zero);
			sunglareMaterial.SetMatrix ("ghost1Settings1", Matrix4x4.zero);
			sunglareMaterial.SetMatrix ("ghost1Settings2", Matrix4x4.zero);
			sunglareMaterial.SetMatrix ("ghost2Settings1", Matrix4x4.zero);
			sunglareMaterial.SetMatrix ("ghost2Settings2", Matrix4x4.zero);
			sunglareMaterial.SetMatrix ("ghost3Settings1", Matrix4x4.zero);
			sunglareMaterial.SetMatrix ("ghost3Settings2", Matrix4x4.zero);
			sunglareMaterial.SetFloat (ShaderProperties.sunGlareScale_PROPERTY, 1f);
			sunglareMaterial.SetFloat (ShaderProperties.sunGlareFade_PROPERTY, 1f);

			//For the 2 flares the scale and intensity have to be overriden from curve I think, or just leave intensity at 1 here and override fade from curve?
			//TODO: replace only the second 1f with the 1/value from the scaleCurve

			if (settingsV2.flares.Count > 0)
			{
				LoadAndSetTexture ("sunFlare", sunFlare, Utils.GameDataPath + settingsV2.flares[0].texture);
				sunglareMaterial.SetVector ("flareSettings", new Vector3(1f, settingsV2.flares[0].displayAspectRatio,1f));
			}
			if (settingsV2.flares.Count > 1)
			{
				LoadAndSetTexture ("sunSpikes", sunSpikes, Utils.GameDataPath + settingsV2.flares[1].texture);
				sunglareMaterial.SetVector ("spikesSettings", new Vector3(1f, settingsV2.flares[1].displayAspectRatio,1f));
			}
			if (settingsV2.flares.Count > 2)
			{
				Utils.LogError ("More than 2 flares used on sunflare " + sourceName + ", only 2 are supported, additional flares will not be used");
			}

			//All static settings are set for ghosts, all that's left to do is configure the fade from ghost intensity curves
			if (settingsV2.ghosts.Count > 0)
			{
				LoadAndSetTexture ("sunGhost1", sunGhost1, Utils.GameDataPath + settingsV2.ghosts[0].texture);
				SetGhostParameters ("ghost1Settings1", "ghost1Settings2", settingsV2.ghosts[0]);
			}
			if (settingsV2.ghosts.Count > 1)
			{
				LoadAndSetTexture ("sunGhost2", sunGhost2, Utils.GameDataPath + settingsV2.ghosts[1].texture);
				SetGhostParameters ("ghost2Settings1", "ghost2Settings2", settingsV2.ghosts[1]);
			}
			if (settingsV2.ghosts.Count > 2)
			{
				LoadAndSetTexture ("sunGhost3", sunGhost3, Utils.GameDataPath + settingsV2.ghosts[2].texture);
				SetGhostParameters ("ghost3Settings1", "ghost3Settings2", settingsV2.ghosts[2]);
			}
			if (settingsV2.ghosts.Count > 3)
			{
				Utils.LogError ("More than 3 ghosts used on sunflare " + sourceName + ", only 3 are supported, additional ghosts will not be used");
			}

			sunglareMaterial.SetVector ("flareColor", settingsV2.flareColor);
		}

		void SetGhostParameters (string shaderParam1, string shaderParam2, GhostSettings ghostSettings )
		{
			Matrix4x4 ghostSettings1 = Matrix4x4.zero;
			for (int i = 0; (i < ghostSettings.instances.Count) && (i < 4); i++)
			{
				ghostSettings1.SetRow (i, new Vector4 (ghostSettings.instances [i].intensityMultiplier, ghostSettings.instances [i].displayAspectRatio, 1f / ghostSettings.instances [i].scale, ghostSettings.instances [i].sunToScreenCenterPosition));
			}
			Matrix4x4 ghostSettings2 = Matrix4x4.zero;
			for (int i = 4; (i < ghostSettings.instances.Count) && (i < 8); i++)
			{
				ghostSettings2.SetRow (i, new Vector4 (ghostSettings.instances [i].intensityMultiplier, ghostSettings.instances [i].displayAspectRatio, 1f / ghostSettings.instances [i].scale, ghostSettings.instances [i].sunToScreenCenterPosition));
			}
			sunglareMaterial.SetMatrix (shaderParam1, ghostSettings1);
			sunglareMaterial.SetMatrix (shaderParam2, ghostSettings2);
		}

		void LoadAndSetTexture (string textureName, Texture2D texture, string path)
		{
			if (Path.GetExtension(path) == ".dds")
			{
				texture = Utils.LoadDDSTexture(System.IO.File.ReadAllBytes (path),path);
			}
			else
			{
				texture.LoadImage (System.IO.File.ReadAllBytes (path));
			}

			texture.wrapMode = TextureWrapMode.Clamp;
			sunglareMaterial.SetTexture (textureName, texture);
		}
	}
}