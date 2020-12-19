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

		Texture2D sunSpikes, sunFlare;
		Texture2D sunGhost1,sunGhost2,sunGhost3;

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
					sunglareMaterial.SetTexture ("_customDepthTexture", Texture2D.whiteTexture);
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

			if (syntaxVersion == 1)
			{

				//Size is loaded automatically from the files
				sunSpikes = new Texture2D (1, 1);
				sunFlare = new Texture2D (1, 1);
				sunGhost1 = new Texture2D (1, 1);
				sunGhost2 = new Texture2D (1, 1);
				sunGhost3 = new Texture2D (1, 1);
			
				sunSpikes.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "sunSpikes.png")));
				sunSpikes.wrapMode = TextureWrapMode.Clamp;
				sunFlare.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "sunFlare.png")));
				sunFlare.wrapMode = TextureWrapMode.Clamp;
				sunGhost1.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost1.png")));
				sunGhost1.wrapMode = TextureWrapMode.Clamp;
				sunGhost2.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost2.png")));
				sunGhost2.wrapMode = TextureWrapMode.Clamp;
				sunGhost3.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.GameDataPath + settingsV1.assetPath, "Ghost3.png")));
				sunGhost3.wrapMode = TextureWrapMode.Clamp;
			
				//			sunglareMaterial.SetTexture ("_Sun_Glare", sunGlare);
				sunglareMaterial.SetTexture ("sunSpikes", sunSpikes);
				sunglareMaterial.SetTexture ("sunFlare", sunFlare);
				sunglareMaterial.SetTexture ("sunGhost1", sunGhost1);
				sunglareMaterial.SetTexture ("sunGhost2", sunGhost2);
				sunglareMaterial.SetTexture ("sunGhost3", sunGhost3);
			
				//didn't want to serialize the matrices directly as the result is pretty unreadable
				//sorry about the mess, I'll make a cleaner way later
				//ghost 1
				Matrix4x4 ghost1Settings1 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost1SettingsList1.Count; i++) {
					ghost1Settings1.SetRow (i, settingsV1.ghost1SettingsList1 [i]);
				}
				Matrix4x4 ghost1Settings2 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost1SettingsList2.Count; i++) {
					ghost1Settings2.SetRow (i, settingsV1.ghost1SettingsList2 [i]);
				}
			
				//ghost 2
				Matrix4x4 ghost2Settings1 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost2SettingsList1.Count; i++) {
					ghost2Settings1.SetRow (i, settingsV1.ghost2SettingsList1 [i]);
				}
				Matrix4x4 ghost2Settings2 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost2SettingsList2.Count; i++) {
					ghost2Settings2.SetRow (i, settingsV1.ghost2SettingsList2 [i]);
				}
			
				//ghost 3
				Matrix4x4 ghost3Settings1 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost3SettingsList1.Count; i++) {
					ghost3Settings1.SetRow (i, settingsV1.ghost3SettingsList1 [i]);
				}
				Matrix4x4 ghost3Settings2 = Matrix4x4.zero;
				for (int i=0; i<settingsV1.ghost3SettingsList2.Count; i++) {
					ghost3Settings2.SetRow (i, settingsV1.ghost3SettingsList2 [i]);
				}
			
				extinctionTexture = new RenderTexture (4, 4, 0, RenderTextureFormat.ARGB32);
				extinctionTexture.antiAliasing = 1;
				extinctionTexture.filterMode = FilterMode.Point;
				extinctionTexture.Create ();
			
				sunglareMaterial.SetVector ("flareSettings", settingsV1.flareSettings);
				sunglareMaterial.SetVector ("spikesSettings", settingsV1.spikesSettings);
			
				sunglareMaterial.SetMatrix ("ghost1Settings1", ghost1Settings1);
				sunglareMaterial.SetMatrix ("ghost1Settings2", ghost1Settings2);
			
				sunglareMaterial.SetMatrix ("ghost2Settings1", ghost2Settings1);
				sunglareMaterial.SetMatrix ("ghost2Settings2", ghost2Settings2);
			
				sunglareMaterial.SetMatrix ("ghost3Settings1", ghost3Settings1);
				sunglareMaterial.SetMatrix ("ghost3Settings2", ghost3Settings2);
			
				sunglareMaterial.SetTexture ("extinctionTexture", extinctionTexture);
			
				sunglareMaterial.SetVector ("flareColor", settingsV1.flareColor);

			}

			Utils.LogDebug ("Added custom sun flare for "+sourceName);

		}

		public void updateProperties()
		{
			sunViewPortPos = Scatterer.Instance.scaledSpaceCamera.WorldToViewportPoint (sourceScaledTransform.position);

			float dist = (float) (Scatterer.Instance.scaledSpaceCamera.transform.position - sourceScaledTransform.position)
				.magnitude;

			if (syntaxVersion == 1)
			{
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
			}

			hitStatus=false;
			if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			//if (!MapView.MapIsEnabled)
			{
	
				hitStatus = Physics.Raycast (Scatterer.Instance.nearCamera.transform.position,
				                             (source.transform.position- Scatterer.Instance.nearCamera.transform.position).normalized,
				                             out hit, Mathf.Infinity, (int)((1 << 15) + (1 << 0)));
				if(!hitStatus)
				{
					hitStatus = Physics.Raycast (Scatterer.Instance.scaledSpaceCamera.transform.position,
					                             (sourceScaledTransform.position - Scatterer.Instance.scaledSpaceCamera.transform.position)
					                             .normalized,out hit, Mathf.Infinity, (int)((1 << 10)));
				}
			}
			else
			{
				hitStatus = Physics.Raycast (Scatterer.Instance.scaledSpaceCamera.transform.position, (sourceScaledTransform.position
				                                                                           - Scatterer.Instance.transform.position).normalized,out hit, Mathf.Infinity, (int)((1 << 10)));
			}

			if(hitStatus)
			{
				//if sun visible, draw sunflare
				if(hit.transform == sourceScaledTransform)
					hitStatus=false;
			}

			eclipse = hitStatus;
			sunglareMaterial.SetFloat(ShaderProperties.renderSunFlare_PROPERTY, (!eclipse && (sunViewPortPos.z > 0) && !Scatterer.Instance.scattererCelestialBodiesManager.underwater ) ? 1.0f : 0.0f);

			sunglareMaterial.SetVector (ShaderProperties.sunViewPortPos_PROPERTY, sunViewPortPos);
			sunglareMaterial.SetFloat (ShaderProperties.aspectRatio_PROPERTY, Scatterer.Instance.scaledSpaceCamera.aspect);
			sunglareMaterial.SetFloat (ShaderProperties.sunGlareScale_PROPERTY, sunGlareScale);
			sunglareMaterial.SetFloat (ShaderProperties.sunGlareFade_PROPERTY, sunGlareFade);
			sunglareMaterial.SetFloat (ShaderProperties.ghostFade_PROPERTY, ghostFade);

			if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				sunglareMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Scatterer.Instance.bufferManager.depthTexture);
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

		public void LoadSettings ()
		{
			configNodeToLoad = new ConfigNode ();
			foreach (ConfigNode _cn in Scatterer.Instance.planetsConfigsReader.sunflareConfigs)
			{
				if (_cn.TryGetNode(sourceName, ref configNodeToLoad))
				{
					Utils.LogDebug("Sunflare config found for "+sourceName);
					break;
				}
			}

			if (configNodeToLoad.HasValue ("syntaxVersion"))
			{
				if (configNodeToLoad.TryGetValue("syntaxVersion", ref syntaxVersion) && ((syntaxVersion == 1) || (syntaxVersion == 2)))
				{
					Utils.LogDebug ("Sunflare syntax version: " + syntaxVersion.ToString ());
				}
				else
				{
					Utils.LogDebug ("Invalid sunflare syntax version found: "+ configNodeToLoad.GetValue ("syntaxVersion") +", defaulting to version 1 for retro-compatibility");
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
				settingsV1 = new SunflareSettingsV1();
				ConfigNode.LoadObjectFromConfig (settingsV1, configNodeToLoad);
			}
			else
			{
				settingsV2 = new SunflareSettingsV2();
				ConfigNode.LoadObjectFromConfig (settingsV2, configNodeToLoad);
			}


		}

		public void Configure(CelestialBody source, string sourceName, Transform sourceScaledTransform)
		{
			this.source = source;
			this.sourceName = sourceName;
			this.sourceScaledTransform = sourceScaledTransform;
		}

//		public void ApplyFromUI()
//		{
//
//		}
	}
}