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
	public partial class Scatterer: MonoBehaviour
	{	
		private static Scatterer instance;
		public static Scatterer Instance {get {return instance;}}

		public MainSettingsReadWrite mainSettings = new MainSettingsReadWrite();
		public PluginDataReadWrite pluginData     = new PluginDataReadWrite();
		public ConfigReader planetsConfigsReader  = new ConfigReader ();

		public GUIhandler guiHandler = new GUIhandler();
		
		public ScattererCelestialBodiesManager scattererCelestialBodiesManager = new ScattererCelestialBodiesManager ();
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
		
		public GameObject sunLight,scaledspaceSunLight, mainMenuLight;
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		
		bool coreInitiated = false;
		public bool isActive = false;
		public string versionNumber = "0.0543dev";

		void Awake ()
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

			Utils.LogInfo ("Version:"+versionNumber);
			Utils.LogInfo ("Running on " + SystemInfo.graphicsDeviceVersion + " on " +SystemInfo.operatingSystem);
			Utils.LogInfo ("Game resolution " + Screen.width.ToString() + "x" +Screen.height.ToString());

			loadSettings ();
			scattererCelestialBodiesManager.Init ();

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
				StartCoroutine (DelayedInit ());
			}
		}

		//wait for 5 frames for EVE and the game to finish setting up
		IEnumerator DelayedInit()
		{
			int delayFrames = (HighLogic.LoadedScene == GameScenes.MAINMENU) ? 5 : 1;
			for (int i=0; i<delayFrames; i++)
				yield return new WaitForFixedUpdate ();

			Init();
		}

		void Init()
		{
			SetupMainCameras ();

			SetShadows();

			FindSunlights ();
			
			Utils.FixKopernicusRingsRenderQueue ();			
			Utils.FixSunsCoronaRenderQueue ();
				
			if (mainSettings.usePlanetShine)
			{
				planetshineManager = new PlanetshineManager();
				planetshineManager.Init();
			}

			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				bufferManager = (BufferManager)farCamera.gameObject.AddComponent (typeof(BufferManager));
				bufferManager.start();

				//copy stock depth buffers and combine into a single depth buffer
				//TODO: shouldn't this be moved to bufferManager?
				if (mainSettings.useOceanShaders || mainSettings.fullLensFlareReplacement)
				{
					farDepthCommandbuffer = farCamera.gameObject.AddComponent<DepthToDistanceCommandBuffer>();
					nearDepthCommandbuffer = nearCamera.gameObject.AddComponent<DepthToDistanceCommandBuffer>();
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
				ambientLightScript = (DisableAmbientLight) scaledSpaceCamera.gameObject.AddComponent (typeof(DisableAmbientLight));
			}

//			//add shadowmask modulator (adds occlusion to shadows)
//			shadowMaskModulate = (ShadowMaskModulateCommandBuffer)sunLight.AddComponent (typeof(ShadowMaskModulateCommandBuffer));
//
			//add shadow far plane fixer
			shadowFadeRemover = (ShadowRemoveFadeCommandBuffer)nearCamera.gameObject.AddComponent (typeof(ShadowRemoveFadeCommandBuffer));

			//magically fix stupid issues when reverting to space center from map view
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				MapView.MapIsEnabled = false;
			}

			if (mainSettings.sunlightExtinction || (mainSettings.underwaterLightDimming && mainSettings.useOceanShaders))
			{
				sunlightModulatorInstance = (SunlightModulator) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SunlightModulator));
			}

			coreInitiated = true;
			Utils.LogDebug("Core setup done");
		}

		void Update ()
		{
			guiHandler.UpdateGUIvisible ();

			//TODO: get rid of this check, maybe move to coroutine? what happens when coroutine exits?
			if (coreInitiated)
			{
				scattererCelestialBodiesManager.Update ();

				//move this out of this update, let it be a one time thing
				//TODO: check what this means
				if (bufferManager)
				{
					if (!bufferManager.depthTextureCleared && (MapView.MapIsEnabled || !scattererCelestialBodiesManager.isPQSEnabledOnScattererPlanet) )
						bufferManager.ClearDepthTexture();
				}

				if (!ReferenceEquals(sunflareManager,null))
				{
					sunflareManager.UpdateFlares();
				}

				if(!ReferenceEquals(planetshineManager,null))
				{
					planetshineManager.UpdatePlanetshine();
				}
			}
		} 


		void OnDestroy ()
		{
			if (isActive)
			{
				if(!ReferenceEquals(planetshineManager,null))
				{
					planetshineManager.CleanUp();
					Component.Destroy(planetshineManager);
				}

				if (!ReferenceEquals(scattererCelestialBodiesManager,null))
				{
					scattererCelestialBodiesManager.Cleanup();
				}

				if (ambientLightScript)
				{
					ambientLightScript.restoreLight();
					Component.Destroy(ambientLightScript);
				}
				

				if (farCamera)
				{
					if (nearCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (nearCamera.gameObject.GetComponent (typeof(Wireframe)));
					
					
					if (farCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (farCamera.gameObject.GetComponent (typeof(Wireframe)));
					
					
					if (scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
				}


				if (!ReferenceEquals(sunflareManager,null))
				{
					sunflareManager.Cleanup();
					UnityEngine.Component.Destroy(sunflareManager);
				}

				if (!ReferenceEquals(sunlightModulatorInstance,null))
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
					Component.Destroy (farDepthCommandbuffer);
				
				if (nearDepthCommandbuffer)
					Component.Destroy (nearDepthCommandbuffer);

				if (bufferManager)
				{
					bufferManager.OnDestroy();
					Component.Destroy (bufferManager);
				}

				pluginData.inGameWindowLocation=new Vector2(guiHandler.windowRect.x,guiHandler.windowRect.y);
				saveSettings();
			}

			UnityEngine.Object.Destroy (guiHandler);
			
		}

		void OnGUI ()
		{
			guiHandler.DrawGui ();
		}
		
		public void loadSettings ()
		{
			mainSettings.loadMainSettings ();
			pluginData.loadPluginData ();
			planetsConfigsReader.loadConfigs ();
		}
		
		public void saveSettings ()
		{
			pluginData.savePluginData ();
			mainSettings.saveMainSettingsIfChanged ();
		}

		void SetupMainCameras()
		{
			Camera[] cams = Camera.allCameras;
			scaledSpaceCamera = Camera.allCameras.FirstOrDefault (_cam => _cam.name == "Camera ScaledSpace");
			farCamera = Camera.allCameras.FirstOrDefault (_cam => _cam.name == "Camera 01");
			nearCamera = Camera.allCameras.FirstOrDefault (_cam => _cam.name == "Camera 00");

			if (scaledSpaceCamera && farCamera && nearCamera)
			{
				farCameraShadowCascadeTweaker = (TweakFarCameraShadowCascades) farCamera.gameObject.AddComponent(typeof(TweakFarCameraShadowCascades));
				
				if (mainSettings.overrideNearClipPlane)
				{
					Utils.LogDebug("Override near clip plane from:"+nearCamera.nearClipPlane.ToString()+" to:"+mainSettings.nearClipPlane.ToString());
					nearCamera.nearClipPlane = mainSettings.nearClipPlane;
				}
			}
			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				//if are in main menu, where there is only 1 camera, affect all cameras to Landscape camera
				scaledSpaceCamera = Camera.allCameras.Single(_cam  => _cam.name == "Landscape Camera");
				farCamera = scaledSpaceCamera;
				nearCamera = scaledSpaceCamera;
			}
			else if (HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				//if in trackstation, just to get rid of some nullrefs
				farCamera = scaledSpaceCamera;
				nearCamera = scaledSpaceCamera;
			}
		}

		void SetShadows()
		{
			if (mainSettings.terrainShadows && (HighLogic.LoadedScene != GameScenes.MAINMENU ) )
			{
				QualitySettings.shadowDistance = mainSettings.shadowsDistance;
				Utils.LogDebug("Number of shadow cascades detected "+QualitySettings.shadowCascades.ToString());


				if (mainSettings.shadowsOnOcean)
					QualitySettings.shadowProjection = ShadowProjection.CloseFit; //with ocean shadows
				else
					QualitySettings.shadowProjection = ShadowProjection.StableFit; //without ocean shadows

				//set shadow bias
				//fixes checkerboard artifacts aka shadow acne
				Light[] lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
				foreach (Light _light in lights)
				{
					if ((_light.gameObject.name == "Scaledspace SunLight") 
					    || (_light.gameObject.name == "SunLight"))
					{
						_light.shadowNormalBias=mainSettings.shadowNormalBias;
						_light.shadowBias=mainSettings.shadowBias;
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
