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
	public partial class Core: MonoBehaviour
	{	
		private static Core instance;
		public static Core Instance {get {return instance;}}

		public MainSettingsReadWrite mainSettings = new MainSettingsReadWrite();
		public PluginDataReadWrite pluginData     = new PluginDataReadWrite();
		public ConfigReader planetsConfigsReader = new ConfigReader ();

		GUIhandler GUItool= new GUIhandler();
		public bool visible = false;

		public EVEReflectionHandler eveReflectionHandler;
		public SunflareManager sunflareManager;
		public PlanetshineManager planetshineManager;

		public BufferRenderingManager bufferRenderingManager;

		//runtime stuff
		DisableAmbientLight ambientLightScript;
		public SunlightModulator sunlightModulatorInstance;
		//		public ShadowMaskModulateCommandBuffer shadowMaskModulate;
		public ShadowRemoveFadeCommandBuffer shadowFadeRemover;
		public TweakFarCameraShadowCascades farCameraShadowCascadeTweaker;

		DepthToDistanceCommandBuffer farDepthCommandbuffer, nearDepthCommandbuffer;

		public CelestialBody[] CelestialBodies;		
		public GameObject sunLight,scaledspaceSunLight, mainMenuLight;
		public Camera farCamera, scaledSpaceCamera, nearCamera;

		bool callCollector=false;

		//means a PQS enabled for the closest celestial body, regardless of whether it uses scatterer effects or not
		bool globalPQSEnabled = false;
		public bool isGlobalPQSEnabled {get{return globalPQSEnabled;}}

		//means a PQS enabled for a celestial body which scatterer effects are active on (is this useless?)
		bool pqsEnabledOnScattererPlanet = false;
		public bool isPQSEnabledOnScattererPlanet{get{return pqsEnabledOnScattererPlanet;}}

		public bool underwater = false;

		bool coreInitiated = false;
		public bool isActive = false;
		public bool mainMenuOptions=false;
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

			GUItool.windowId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

			loadSettings ();

			//find all celestial bodies, used for finding scatterer-enabled bodies and disabling the stock ocean
			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));

			Utils.LogInfo ("Version:"+versionNumber);
			Utils.LogInfo ("Running on " + SystemInfo.graphicsDeviceVersion + " on " +SystemInfo.operatingSystem);
			Utils.LogInfo ("Game resolution " + Screen.width.ToString() + "x" +Screen.height.ToString());
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				isActive = true;
				mainMenuOptions = (HighLogic.LoadedScene == GameScenes.SPACECENTER);
				GUItool.windowRect.x=pluginData.inGameWindowLocation.x;
				GUItool.windowRect.y=pluginData.inGameWindowLocation.y;
			} 
			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				isActive = true;

				if (mainSettings.useOceanShaders)
				{
					OceanUtils.removeStockOceans();
				}

				if (mainSettings.integrateWithEVEClouds)
				{
					ShaderReplacer.Instance.replaceEVEshaders();
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
			findScattererCelestialBodies();

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
			
			//create buffer manager
			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				bufferRenderingManager = (BufferRenderingManager)farCamera.gameObject.AddComponent (typeof(BufferRenderingManager));
				bufferRenderingManager.start();

				//copy stock depth buffers and combine into a single depth buffer
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
				eveReflectionHandler.mapEVEClouds();
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

			//create sunlightModulator
			if (mainSettings.sunlightExtinction || (mainSettings.underwaterLightDimming && mainSettings.useOceanShaders))
			{
				sunlightModulatorInstance = (SunlightModulator) Core.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SunlightModulator));
			}

			coreInitiated = true;
			Utils.LogDebug("Core setup done");
		}

		void Update ()
		{
			//toggle whether GUI is visible or not
			//TODO: move to guihandler
			if ((Input.GetKey (pluginData.guiModifierKey1) || Input.GetKey (pluginData.guiModifierKey2)) && (Input.GetKeyDown (pluginData.guiKey1) || (Input.GetKeyDown (pluginData.guiKey2))))
			{
				if (ToolbarButton.Instance.button!= null)
				{
					if (visible)
						ToolbarButton.Instance.button.SetFalse(false);
					else
						ToolbarButton.Instance.button.SetTrue(false);
				}

				visible = !visible;
			}

			//TODO: get rid of this check, maybe move to coroutine? what happens when coroutine exits?
			if (coreInitiated)
			{
				//TODO: determine if still needed anymore, ie test without
				if (callCollector)
				{
					GC.Collect();
					callCollector=false;
				}

				globalPQSEnabled = false;
				if (FlightGlobals.currentMainBody )
				{
					if (FlightGlobals.currentMainBody.pqsController)
						globalPQSEnabled = FlightGlobals.currentMainBody.pqsController.isActive;
				}
				
				pqsEnabledOnScattererPlanet = false;
				underwater = false;

				//TODO: make into it's own function
				//TODO: definitely refactor this next
				foreach (ScattererCelestialBody _cur in planetsConfigsReader.scattererCelestialBodies)
				{
					float dist, shipDist=0f;
					if (_cur.hasTransform)
					{
						dist = Vector3.Distance (ScaledSpace.ScaledToLocalSpace( scaledSpaceCamera.transform.position),
						                         ScaledSpace.ScaledToLocalSpace (_cur.transform.position));
						
						//don't unload planet the player ship is close to if panning away in map view
						if (FlightGlobals.ActiveVessel)
						{
							shipDist = Vector3.Distance (FlightGlobals.ActiveVessel.transform.position,
							                             ScaledSpace.ScaledToLocalSpace (_cur.transform.position));
						}

						if (_cur.active)
						{
							if (dist > _cur.unloadDistance && (shipDist > _cur.unloadDistance || shipDist == 0f )) {
								
								_cur.m_manager.OnDestroy ();
								UnityEngine.Object.Destroy (_cur.m_manager);
								_cur.m_manager = null;
								_cur.active = false;
								callCollector=true;
								
								Utils.LogDebug ("Effects unloaded for " + _cur.celestialBodyName);
							} else {
								
								_cur.m_manager.Update ();
								{
									if (!_cur.m_manager.m_skyNode.inScaledSpace)
									{
										pqsEnabledOnScattererPlanet = true;
									}
									
									if (!ReferenceEquals(_cur.m_manager.GetOceanNode(),null) && pqsEnabledOnScattererPlanet) 
									{
										underwater = _cur.m_manager.GetOceanNode().isUnderwater;
									}
								}
							}
						} 
						else
						{
							if (dist < _cur.loadDistance && _cur.transform && _cur.celestialBody)
							{
								try
								{

									if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
										_cur.hasOcean=false;

									_cur.m_manager = new Manager ();

									_cur.m_manager.Init(_cur);
									_cur.active = true;
									
									GUItool.selectedConfigPoint = 0;
									GUItool.displayOceanSettings = false;
									GUItool.selectedPlanet = planetsConfigsReader.scattererCelestialBodies.IndexOf (_cur);
									GUItool.getSettingsFromSkynode ();

									if (!ReferenceEquals(_cur.m_manager.GetOceanNode(),null)) {
										GUItool.getSettingsFromOceanNode ();
									}
									callCollector=true;
									Utils.LogDebug ("Effects loaded for " + _cur.celestialBodyName);
								}
								catch(Exception e)
								{
									Utils.LogDebug ("Effects couldn't be loaded for " + _cur.celestialBodyName +" because of exception: "+e.ToString());
									try
									{
										_cur.m_manager.OnDestroy();
									}
									catch(Exception ee)
									{
										Utils.LogDebug ("manager couldn't be removed for " + _cur.celestialBodyName +" because of exception: "+ee.ToString());
									}
									planetsConfigsReader.scattererCelestialBodies.Remove(_cur);
									Utils.LogDebug (""+ _cur.celestialBodyName +" removed from active planets.");
									return;
								}
							}
						}
					}
				}

				//move this out of this update, let it be a one time thing
				if (bufferRenderingManager)
				{
					if (!bufferRenderingManager.depthTextureCleared && (MapView.MapIsEnabled || !pqsEnabledOnScattererPlanet) )
						bufferRenderingManager.clearDepthTexture();
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

				for (int i = 0; i < planetsConfigsReader.scattererCelestialBodies.Count; i++) {
					
					ScattererCelestialBody cur = planetsConfigsReader.scattererCelestialBodies [i];
					if (cur.active) {
						cur.m_manager.OnDestroy ();
						UnityEngine.Object.Destroy (cur.m_manager);
						cur.m_manager = null;
						cur.active = false;
					}
					
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

				if (bufferRenderingManager)
				{
					bufferRenderingManager.OnDestroy();
					Component.Destroy (bufferRenderingManager);
				}

				pluginData.inGameWindowLocation=new Vector2(GUItool.windowRect.x,GUItool.windowRect.y);
				saveSettings();
			}

			UnityEngine.Object.Destroy (GUItool);
			
		}

		void OnGUI ()
		{
			if (visible)
			{
				GUItool.DrawGui();
			}
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

		void findScattererCelestialBodies()
		{
			foreach (ScattererCelestialBody sctBody in planetsConfigsReader.scattererCelestialBodies)
			{
				var _idx = 0;
			
				var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
				
				if (celBody == null)
				{
					celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
				}
				
				Utils.LogDebug ("Celestial Body: " + celBody);
				if (celBody != null)
				{
					_idx = planetsConfigsReader.scattererCelestialBodies.IndexOf (sctBody);
					Utils.LogDebug ("Found: " + sctBody.celestialBodyName + " / " + celBody.GetName ());
				};
				
				sctBody.celestialBody = celBody;

				var sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.transformName);
				if (!sctBodyTransform)
				{
					sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.celestialBodyName);
				}
				else
				{
					sctBody.transform = sctBodyTransform;
					sctBody.hasTransform = true;
				}
				sctBody.active = false;
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

				foreach (CelestialBody _sc in CelestialBodies)
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
