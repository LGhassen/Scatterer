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

		public ShadowMapRetrieveCommandBuffer shadowMapRetriever;
		public ShadowRemoveFadeCommandBuffer shadowFadeRemover;
		public TweakShadowCascades shadowCascadeTweaker;
		public PartialDepthBuffer partialUnifiedCameraDepthBuffer;

		//probably move these to buffer rendering manager
		DepthToDistanceCommandBuffer farDepthCommandbuffer, nearDepthCommandbuffer;
		
		public Light sunLight,scaledSpaceSunLight, mainMenuLight;
		public Camera farCamera, scaledSpaceCamera, nearCamera;

		//classic SQUAD
		ReflectionProbeChecker reflectionProbeChecker;
		GameObject ReflectionProbeCheckerGO;
		
		bool coreInitiated = false;
		public bool isActive = false;
		public bool unifiedCameraMode = false;
		public string versionNumber = "0.0610 Dev";

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

			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				isActive = true;

				loadSettings ();
				scattererCelestialBodiesManager.Init ();

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

			FindSunlights ();

			SetShadows();
			
			Utils.FixKopernicusRingsRenderQueue ();			
			Utils.FixSunsCoronaRenderQueue ();

			addReflectionProbeFixer ();

			if (mainSettings.usePlanetShine)
			{
				planetshineManager = new PlanetshineManager();
				planetshineManager.Init();
			}

			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				// Note: Stock KSP dragCubes make a copy of components and removes them rom far/near cameras when rendering
				// This can cause issues with renderTextures and commandBuffers, to keep in mind for when implementing godrays
				bufferManager = (BufferManager)scaledSpaceCamera.gameObject.AddComponent (typeof(BufferManager));	// This doesn't need to be added to any camera anymore
																													// TODO: move to appropriate gameObject

				//copy stock depth buffers and combine into a single depth buffer
				//TODO: shouldn't this be moved to bufferManager?
				if (!unifiedCameraMode && (mainSettings.useOceanShaders || mainSettings.fullLensFlareReplacement))
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
			if (!unifiedCameraMode)
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
				

				if (nearCamera)
				{
					if (nearCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (nearCamera.gameObject.GetComponent (typeof(Wireframe)));
					
					
					if (farCamera && farCamera.gameObject.GetComponent (typeof(Wireframe)))
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

				if (shadowMapRetriever)
				{
					shadowMapRetriever.OnDestroy();
					Component.Destroy(shadowMapRetriever);
				}

				if (shadowFadeRemover)
				{
					shadowFadeRemover.OnDestroy();
					Component.Destroy(shadowFadeRemover);
				}

				if (shadowCascadeTweaker)
				{
					Component.Destroy(shadowCascadeTweaker);
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

				if (partialUnifiedCameraDepthBuffer)
				{
					partialUnifiedCameraDepthBuffer.OnDestroy();
					Component.Destroy(partialUnifiedCameraDepthBuffer);
				}

				if (reflectionProbeChecker)
				{
					reflectionProbeChecker.OnDestroy ();
					Component.Destroy (reflectionProbeChecker);
				}

				if (ReflectionProbeCheckerGO)
				{
					UnityEngine.GameObject.Destroy (ReflectionProbeCheckerGO);
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

			// Hack: for mainMenu everything is jumbled, so just attempt to load every planet always
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				foreach (ScattererCelestialBody _SCB in planetsConfigsReader.scattererCelestialBodies)
				{
					_SCB.loadDistance = Mathf.Infinity;
					_SCB.unloadDistance = Mathf.Infinity;
				}
			}
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

			if (nearCamera && !farCamera) 
			{
				Utils.LogInfo("Running in unified camera mode");
				unifiedCameraMode = true;
			}

			if (scaledSpaceCamera && nearCamera)
			{
				//move these to be used only with long-distance shadows?
				if (!unifiedCameraMode && (mainSettings.dualCamShadowCascadeSplitsOverride != Vector3.zero))
				{
					shadowCascadeTweaker = (TweakShadowCascades) Utils.getEarliestLocalCamera().gameObject.AddComponent(typeof(TweakShadowCascades));
					shadowCascadeTweaker.Init(mainSettings.dualCamShadowCascadeSplitsOverride);
				}
				else if (unifiedCameraMode && (mainSettings.unifiedCamShadowCascadeSplitsOverride != Vector3.zero))
				{
					shadowCascadeTweaker = (TweakShadowCascades) Utils.getEarliestLocalCamera().gameObject.AddComponent(typeof(TweakShadowCascades));
					shadowCascadeTweaker.Init(mainSettings.unifiedCamShadowCascadeSplitsOverride);
				}
				
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
			if (HighLogic.LoadedScene != GameScenes.MAINMENU)
			{
				if (unifiedCameraMode && (mainSettings.d3d11ShadowFix || mainSettings.terrainShadows))
				{
					QualitySettings.shadowProjection = ShadowProjection.StableFit; //way more resistant to jittering
					GraphicsSettings.SetShaderMode (BuiltinShaderType.ScreenSpaceShadows, BuiltinShaderMode.UseCustom);
					if ((mainSettings.terrainShadows) && (mainSettings.unifiedCamShadowsDistance > 8000f))
					{
						GraphicsSettings.SetCustomShader (BuiltinShaderType.ScreenSpaceShadows, ShaderReplacer.Instance.LoadedShaders [("Scatterer/longDistanceScreenSpaceShadows")]);
						partialUnifiedCameraDepthBuffer = (PartialDepthBuffer) nearCamera.gameObject.AddComponent(typeof(PartialDepthBuffer));
						partialUnifiedCameraDepthBuffer.Init(nearCamera);
					}
					else
					{
						GraphicsSettings.SetCustomShader (BuiltinShaderType.ScreenSpaceShadows, ShaderReplacer.Instance.LoadedShaders [("Scatterer/fixedScreenSpaceShadows")]);
					}
				}

				if (mainSettings.shadowsOnOcean)
				{
					if (unifiedCameraMode || SystemInfo.graphicsDeviceVersion.Contains("Direct3D 11.0"))
					{
						QualitySettings.shadowProjection = ShadowProjection.StableFit;	//StableFit + splitSpheres is the only thing that works Correctly for unified camera (dx11) ocean shadows
																					  	//Otherwise we get artifacts near shadow cascade edges
					}
					else
					{
						QualitySettings.shadowProjection = ShadowProjection.CloseFit;	//CloseFit without SplitSpheres seems to be the only setting that works for OpenGL for ocean shadows
																						//Seems like I lack the correct variables to determine which shadow path to take
																						//also try without the transparent tag
					}

					shadowMapRetriever = (ShadowMapRetrieveCommandBuffer) sunLight.gameObject.AddComponent (typeof(ShadowMapRetrieveCommandBuffer));
				}

				if (mainSettings.terrainShadows)
				{
					QualitySettings.shadowDistance = unifiedCameraMode ? mainSettings.unifiedCamShadowsDistance : mainSettings.dualCamShadowsDistance;
					Utils.LogDebug ("Set shadow distance: " + QualitySettings.shadowDistance.ToString ());
					Utils.LogDebug ("Number of shadow cascades detected " + QualitySettings.shadowCascades.ToString ());

					if (sunLight)
					{
						//fixes checkerboard artifacts aka shadow acne
						float bias = unifiedCameraMode ? mainSettings.unifiedCamShadowNormalBiasOverride : mainSettings.dualCamShadowNormalBiasOverride;
						float normalBias = unifiedCameraMode ? mainSettings.unifiedCamShadowBiasOverride : mainSettings.dualCamShadowBiasOverride;

						if (bias > 0f)
							sunLight.shadowBias = bias;

						if (normalBias > 0f)
							sunLight.shadowNormalBias = normalBias;

						int customRes = unifiedCameraMode ? mainSettings.unifiedCamShadowResolutionOverride : mainSettings.dualCamShadowResolutionOverride;
						if (customRes!=0)
						{
							if (Utils.IsPowerOfTwo(customRes))
							{
								Utils.LogDebug("Setting shadowmap resolution to: "+customRes.ToString());
								sunLight.shadowCustomResolution = customRes;
							}
							else
							{
								Utils.LogError("Selected shadowmap resolution not a power of 2: "+customRes.ToString());
							}
						}
					}

					//and finally force shadow Casting and receiving on celestial bodies if not already set
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
		}

		void FindSunlights ()
		{
			Light[] lights = (Light[])Light.FindObjectsOfType (typeof(Light));
			foreach (Light _light in lights)
			{
				if (_light.gameObject.name == "SunLight")
				{
					sunLight = _light;
					Utils.LogDebug ("Found SunLight");
				}
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledSpaceSunLight = _light;
					Utils.LogDebug ("Found Scaledspace SunLight");
				}

				if (_light.gameObject.name.Contains ("PlanetLight") || _light.gameObject.name.Contains ("Directional light"))
				{
					mainMenuLight = _light;
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

		// Just a dummy gameObject so the reflectionProbeChecker can capture the reflection Camera
		public void addReflectionProbeFixer()
		{
			ReflectionProbeCheckerGO = new GameObject ("Scatterer ReflectionProbeCheckerGO");
			//ReflectionProbeCheckerGO.transform.parent = nearCamera.transform; //VesselViewer doesn't like this for some reason
			ReflectionProbeCheckerGO.layer = 15;

			reflectionProbeChecker = ReflectionProbeCheckerGO.AddComponent<ReflectionProbeChecker> ();

			MeshFilter _mf = ReflectionProbeCheckerGO.AddComponent<MeshFilter> ();
			_mf.mesh.Clear ();
			_mf.mesh = MeshFactory.MakePlane (2, 2, MeshFactory.PLANE.XY, false, false);
			_mf.mesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));

			MeshRenderer _mr = ReflectionProbeCheckerGO.AddComponent<MeshRenderer> ();
			_mr.sharedMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
			_mr.material = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
			_mr.receiveShadows = false;
			_mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			_mr.enabled = true;
		}

	}
}
