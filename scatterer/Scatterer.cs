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

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public partial class Scatterer : MonoBehaviour
	{
		private static Scatterer instance;
		public static Scatterer Instance { get { return instance; } }

		public MainSettingsReadWrite mainSettings = new MainSettingsReadWrite();
		public PluginDataReadWrite pluginData = new PluginDataReadWrite();
		public ConfigReader planetsConfigsReader = new ConfigReader();

		public GUIhandler guiHandler = new GUIhandler();

		public ScattererCelestialBodiesManager scattererCelestialBodiesManager = new ScattererCelestialBodiesManager();
		public BufferManager bufferManager;
		public SunflareManager sunflareManager;
		public EVEReflectionHandler eveReflectionHandler;
		public PlanetshineManager planetshineManager;

		//runtime stuff
		//TODO: merge all into lightAndShadowManager?
		DisableAmbientLight ambientLightScript;
		public SunlightModulator sunlightModulatorInstance;
		//		public ShadowMaskModulateCommandBuffer shadowMaskModulate;
		public ShadowRemoveFadeCommandBuffer shadowFadeRemover;
		public TweakFarCameraShadowCascades farCameraShadowCascadeTweaker;

		//probably move these to buffer rendering manager
		DepthToDistanceCommandBuffer farDepthCommandbuffer, nearDepthCommandbuffer;

		public GameObject sunLight, scaledspaceSunLight, mainMenuLight;
		public Camera scaledSpaceCamera, unifiedCamera, farCamera, nearCamera;
		public Boolean unifiedCameraEnabled;

		bool coreInitiated = false;
		public bool isActive = false;
		public string versionNumber = "0.055_UFCRTBDEV_UR1_2";

		void Awake()
		{
			if (instance == null)
			{
				instance = this;
				Utils.LogDebug("Core instance created");
			}
			else
			{
				//destroy any duplicate instances that may be created by a duplicate install
				Utils.LogError("Destroying duplicate instance, check your install for duplicate mod folders");
				UnityEngine.Object.Destroy(this);
			}

			Utils.LogInfo("Version:" + versionNumber);
			Utils.LogInfo("Running on " + SystemInfo.graphicsDeviceVersion + " on " + SystemInfo.operatingSystem);
			Utils.LogInfo("Game resolution " + Screen.width.ToString() + "x" + Screen.height.ToString());

			loadSettings();
			scattererCelestialBodiesManager.Init();

			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				isActive = true;
				guiHandler.Init();

				if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				{
					if (mainSettings.useOceanShaders)
					{
						OceanUtils.removeStockOceans();
					}

					if (mainSettings.integrateWithEVEClouds)
					{
						ShaderReplacer.Instance.replaceEVEshaders();
					}
				}
			}

			if (isActive)
			{
				StartCoroutine(DelayedInit());
			}
		}

		//wait for 5 frames for EVE and the game to finish setting up
		IEnumerator DelayedInit()
		{
			int delayFrames = (HighLogic.LoadedScene == GameScenes.MAINMENU) ? 5 : 1;
			for (int i = 0; i < delayFrames; i++)
				yield return new WaitForFixedUpdate();

			Init();
		}

		void Init()
		{
			SetupMainCameras();

			SetShadows();

			FindSunlights();

			Utils.FixKopernicusRingsRenderQueue();
			Utils.FixSunsCoronaRenderQueue();

			if (mainSettings.usePlanetShine)
			{
				planetshineManager = new PlanetshineManager();
				planetshineManager.Init();
			}

			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				bufferManager = (BufferManager)ReturnProperCamera(true, false).gameObject.AddComponent(typeof(BufferManager));
				bufferManager.start();

				//copy stock depth buffers and combine into a single depth buffer
				//TODO: shouldn't this be moved to bufferManager?
				if (mainSettings.useOceanShaders || mainSettings.fullLensFlareReplacement)
				{
					Camera farCam = ReturnProperCamera(true, true);
					if (!(farCam is null))
					{
						farDepthCommandbuffer = farCam.gameObject.AddComponent<DepthToDistanceCommandBuffer>();
					}
					nearDepthCommandbuffer = ReturnProperCamera(false, false).gameObject.AddComponent<DepthToDistanceCommandBuffer>();
				}
			}

			if ((mainSettings.fullLensFlareReplacement) && (HighLogic.LoadedScene != GameScenes.MAINMENU))
			{
				sunflareManager = new SunflareManager();
				sunflareManager.Init();
			}

			if (mainSettings.integrateWithEVEClouds)
			{
				eveReflectionHandler = new EVEReflectionHandler();
				eveReflectionHandler.Start();
			}

			if (mainSettings.disableAmbientLight && !ambientLightScript)
			{
				ambientLightScript = (DisableAmbientLight)scaledSpaceCamera.gameObject.AddComponent(typeof(DisableAmbientLight));
			}

			//			//add shadowmask modulator (adds occlusion to shadows)
			//			shadowMaskModulate = (ShadowMaskModulateCommandBuffer)sunLight.AddComponent (typeof(ShadowMaskModulateCommandBuffer));
			//
			//add shadow far plane fixer
			shadowFadeRemover = (ShadowRemoveFadeCommandBuffer)ReturnProperCamera(false, false).gameObject.AddComponent(typeof(ShadowRemoveFadeCommandBuffer));

			//magically fix stupid issues when reverting to space center from map view
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				MapView.MapIsEnabled = false;
			}

			if (mainSettings.sunlightExtinction || (mainSettings.underwaterLightDimming && mainSettings.useOceanShaders))
			{
				sunlightModulatorInstance = (SunlightModulator)Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SunlightModulator));
			}

			coreInitiated = true;
			Utils.LogDebug("Core setup done");
		}

		void Update()
		{
			guiHandler.UpdateGUIvisible();

			//TODO: get rid of this check, maybe move to coroutine? what happens when coroutine exits?
			if (coreInitiated)
			{
				scattererCelestialBodiesManager.Update();

				//move this out of this update, let it be a one time thing
				//TODO: check what this means
				if (bufferManager)
				{
					if (!bufferManager.depthTextureCleared && (MapView.MapIsEnabled || !scattererCelestialBodiesManager.isPQSEnabledOnScattererPlanet))
						bufferManager.ClearDepthTexture();
				}

				if (!ReferenceEquals(sunflareManager, null))
				{
					sunflareManager.UpdateFlares();
				}

				if (!ReferenceEquals(planetshineManager, null))
				{
					planetshineManager.UpdatePlanetshine();
				}
				if (!CheckClipPlanes())
				{
					setupClipPlanes();
				}
			}
		}


		void OnDestroy()
		{
			if (isActive)
			{
				if (!ReferenceEquals(planetshineManager, null))
				{
					planetshineManager.CleanUp();
					Component.Destroy(planetshineManager);
				}

				if (!ReferenceEquals(scattererCelestialBodiesManager, null))
				{
					scattererCelestialBodiesManager.Cleanup();
				}

				if (ambientLightScript)
				{
					ambientLightScript.restoreLight();
					Component.Destroy(ambientLightScript);
				}


				if (ReturnProperCamera(true, false))
				{
					if (ReturnProperCamera(false, false).gameObject.GetComponent(typeof(Wireframe)))
						Component.Destroy(ReturnProperCamera(false, false).gameObject.GetComponent(typeof(Wireframe)));
					if (scaledSpaceCamera.gameObject.GetComponent(typeof(Wireframe)))
						Component.Destroy(scaledSpaceCamera.gameObject.GetComponent(typeof(Wireframe)));
				}


				if (!ReferenceEquals(sunflareManager, null))
				{
					sunflareManager.Cleanup();
					UnityEngine.Component.Destroy(sunflareManager);
				}

				if (!ReferenceEquals(sunlightModulatorInstance, null))
				{
					sunlightModulatorInstance.OnDestroy();
					Component.Destroy(sunlightModulatorInstance);
				}

				//				if (shadowMaskModulate)
				//				{
				//					shadowMaskModulate.OnDestroy();
				//					Component.Destroy(shadowMaskModulate);
				//				}

				if (shadowFadeRemover)
				{
					shadowFadeRemover.OnDestroy();
					Component.Destroy(shadowFadeRemover);
				}

				if (farCameraShadowCascadeTweaker)
				{
					Component.Destroy(farCameraShadowCascadeTweaker);
				}

				if (farDepthCommandbuffer)
					Component.Destroy(farDepthCommandbuffer);

				if (nearDepthCommandbuffer)
					Component.Destroy(nearDepthCommandbuffer);

				if (bufferManager)
				{
					bufferManager.OnDestroy();
					Component.Destroy(bufferManager);
				}

				pluginData.inGameWindowLocation = new Vector2(guiHandler.windowRect.x, guiHandler.windowRect.y);
				saveSettings();
			}

			UnityEngine.Object.Destroy(guiHandler);

		}

		void OnGUI()
		{
			guiHandler.DrawGui();
		}

		public void loadSettings()
		{
			mainSettings.loadMainSettings();
			pluginData.loadPluginData();
			planetsConfigsReader.loadConfigs();
		}

		public void saveSettings()
		{
			pluginData.savePluginData();
			mainSettings.saveMainSettingsIfChanged();
		}
		public Camera ReturnProperCamera(Boolean requestingFarCamera, Boolean disableIfUnified)
		{
			if (disableIfUnified)
			{
				//They have explicitly said to disable this camera if in unified mode. 
				if (unifiedCameraEnabled)
				{
					//It is in unified mode, so we return a null camera, signaling this camera is disabled as requested.
					return null;
				}
				else
				{
					//this implies we are not in unified mode.  Return the requested camera no matter what.
					if (requestingFarCamera)
					{
						//the request is for the far camera
						return farCamera;
					}
					else
					{
						//logically, the request must be for the near camera.
						return nearCamera;
					}
				}
			}
			else
			{
				//this section implies the request wants the camera whether unified or not.  Just give them the closest camera.
				if (unifiedCameraEnabled)
				{
					//we return the unifiedCamera in this case.
					return unifiedCamera;
				}
				else if (requestingFarCamera)
				{
					//they are requesting the far camera and we are not in unified mode.  Give them it.
					return farCamera;
				}
				else
				{
					//logically, the only thing left to do is return the nearCamera.
					return nearCamera;
				}
			}

		}
		void SetupMainCameras()
		{
			Camera[] cams = Camera.allCameras;
			scaledSpaceCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera ScaledSpace");
			if (SystemInfo.graphicsDeviceVersion.Contains("Direct3D 11.0"))
			{
				unifiedCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera 00");
				unifiedCameraEnabled = true;
				Utils.LogDebug("Using Unified Camera.");
			}
			else
			{
				farCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera 01");
				nearCamera = Camera.allCameras.FirstOrDefault(_cam => _cam.name == "Camera 00");
				unifiedCameraEnabled = false;
				Utils.LogDebug("Not using Unified Camera.");
			}

			if (((scaledSpaceCamera && unifiedCamera) && unifiedCameraEnabled) || (!unifiedCameraEnabled && (scaledSpaceCamera && farCamera && nearCamera)))
			{
				farCameraShadowCascadeTweaker = (TweakFarCameraShadowCascades)ReturnProperCamera(false, false).gameObject.AddComponent(typeof(TweakFarCameraShadowCascades));
				setupClipPlanes();

			}
			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				//if are in main menu, where there is only 1 camera, affect all cameras to Landscape camera
				scaledSpaceCamera = Camera.allCameras.Single(_cam => _cam.name == "Landscape Camera");
				if (unifiedCameraEnabled)
				{
					unifiedCamera = scaledSpaceCamera;
				}
				else
				{
					scaledSpaceCamera = Camera.allCameras.Single(_cam => _cam.name == "Landscape Camera");
					farCamera = scaledSpaceCamera;
					nearCamera = scaledSpaceCamera;
				}
				// decide near clipplane depth, main menu needs like none
				if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				{
					ReturnProperCamera(false, false).nearClipPlane = 0.5f;
				}
			}
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				//if in trackstation, just to get rid of some nullrefs
				if (unifiedCameraEnabled)
				{
					unifiedCamera = scaledSpaceCamera;
				}
				else
				{
					farCamera = scaledSpaceCamera;
					nearCamera = scaledSpaceCamera;
				}
			}
		}
		public bool CheckClipPlanes()
		{
			float farClip = ReturnProperCamera(false, false).farClipPlane;
			float nearClip = ReturnProperCamera(false, false).nearClipPlane;
			if (mainSettings.overrideNearClipPlane)
			{
				return true;
			}
			else if ((!mainSettings.RSSMode) && ((nearClip < 0.52f) && (farClip > 820000f) && (farClip < 850000f)))
			{
				return true;
			}
			else if ((mainSettings.RSSMode) && ((nearClip > 0.52f) && (farClip > 1700000f) && (farClip < 1800000f)))
			{
				return true;
			}
			return false;	
		}

		void setupClipPlanes()
		{
			if (mainSettings.overrideNearClipPlane)
			{
				Utils.LogDebug("Override near clip plane from:" + ReturnProperCamera(false, false).nearClipPlane.ToString() + " to:" + mainSettings.nearClipPlane.ToString());
				ReturnProperCamera(false, false).nearClipPlane = mainSettings.nearClipPlane;
			}
			else if (!mainSettings.RSSMode)
			{
				ReturnProperCamera(false, false).nearClipPlane = 0.5f;
			}
			else
			{
				ReturnProperCamera(false, false).nearClipPlane = 1f;
			}
			//then set the farclip
			if (!mainSettings.RSSMode)
			{
				ReturnProperCamera(false, false).farClipPlane = 825000f;
			}
			else
			{
				ReturnProperCamera(false, false).farClipPlane = 1750000f;
			}
		}

		void SetShadows()
		{
			if ((mainSettings.terrainShadows && (!mainSettings.RSSMode)) && (HighLogic.LoadedScene != GameScenes.MAINMENU ) )
			{
				QualitySettings.shadowDistance = mainSettings.shadowsDistance;
				Utils.LogDebug("Number of shadow cascades detected "+QualitySettings.shadowCascades.ToString());
				//StableFit always.  It prevents flicker.
				QualitySettings.shadowProjection = ShadowProjection.StableFit;
				//QualitySettings.shadowResolution = ShadowResolution.VeryHigh;


				//set shadow bias
				//fixes checkerboard artifacts aka shadow acne
				Light[] lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
				foreach (Light _light in lights)
				{
					if ((_light.gameObject.name == "Scaledspace SunLight")
						|| (_light.gameObject.name == "SunLight"))
					{
						_light.shadowNormalBias = mainSettings.shadowNormalBias;
						_light.shadowBias = mainSettings.shadowBias;
						//_light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
						//_light.shadows=LightShadows.Soft;
						//_light.shadowCustomResolution=8192;
					}
				}

				foreach (CelestialBody _sc in scattererCelestialBodiesManager.CelestialBodies)
				{
					if (_sc.pqsController)
					{
						_sc.pqsController.meshCastShadows = true;
						_sc.pqsController.meshRecieveShadows = true;
					}
				}
			}
		}

		void FindSunlights ()
		{
			Light[] lights = (Light[])Light.FindObjectsOfType (typeof(Light));
			foreach (Light _light in lights) {
				if (_light.gameObject.name == "SunLight") {
					sunLight = _light.gameObject;
				}
				if (_light.gameObject.name.Contains ("PlanetLight") || _light.gameObject.name.Contains ("Directional light")) {
					mainMenuLight = _light.gameObject;
					Utils.LogDebug ("Found main menu light");
				}
			}
		}

		public void onRenderTexturesLost()
		{
			foreach (ScattererCelestialBody _cur in planetsConfigsReader.scattererCelestialBodies)
			{
				if (_cur.active)
				{
					_cur.m_manager.m_skyNode.reInitMaterialUniformsOnRenderTexturesLoss ();
					if (_cur.m_manager.hasOcean && mainSettings.useOceanShaders && !_cur.m_manager.m_skyNode.inScaledSpace)
					{
						_cur.m_manager.reBuildOcean ();
					}
				} 
			}
		}
	
	}
}
