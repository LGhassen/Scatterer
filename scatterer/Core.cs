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
		public List <ScattererCelestialBody > scattererCelestialBodies = new List <ScattererCelestialBody> {};

		public Rect windowRect = new Rect (0, 0, 400, 50);
		int windowId;

		GUIhandler GUItool= new GUIhandler();

		//EVE shit
		//
		//map EVE 2d cloud materials to planet names
		public Dictionary<String, List<Material> > EVEClouds = new Dictionary<String, List<Material> >();
		//map EVE CloudObjects to planet names
		//as far as I understand CloudObjects in EVE contain the 2d clouds and the volumetrics for a given
		//layer on a given planet, however due to the way they are handled in EVE they don't directly reference
		//their parent planet and the volumetrics are only created when the PQS is active
		//I map them here to facilitate accessing the volumetrics later
		public Dictionary<String, List<object>> EVECloudObjects = new Dictionary<String, List<object>>();


		//planetsList Stuff
		public List<PlanetShineLightSource> celestialLightSourcesData=new List<PlanetShineLightSource> {};	


		//sunflares stuff
		public ConfigNode[] sunflareConfigs;
		public List<string> sunflaresList=new List<string> {};
		public List<SunFlare> customSunFlares = new List<SunFlare>();

		//runtime shit
		DisableAmbientLight ambientLightScript;
		public CelestialBody[] CelestialBodies;		
		Light[] lights;
		public GameObject sunLight,scaledspaceSunLight, mainMenuLight;
		bool callCollector=false;
		List<PlanetShineLight> celestialLightSources=new List<PlanetShineLight> {};
		Cubemap planetShineCookieCubeMap;
		public UrlDir.UrlConfig[] baseConfigs,atmoConfigs,oceanConfigs;
		public bool visible = false;
		bool customSunFlareAdded=false;


		//means a PQS enabled for the closest celestial body, regardless of whether it uses scatterer effects or not
		bool globalPQSEnabled = false;

		public bool isGlobalPQSEnabled {get{return globalPQSEnabled;}}

		//means a PQS enabled for a celestial body which scatterer effects are active on (is this useless?)
		bool pqsEnabledOnScattererPlanet = false;

		public bool isPQSEnabledOnScattererPlanet{get{return pqsEnabledOnScattererPlanet;}}

		public bool underwater = false;

		public BufferRenderingManager bufferRenderingManager;

		public string path, gameDataPath;
		bool coreInitiated = false;
				
		public Camera farCamera, scaledSpaceCamera, nearCamera;

		public bool isActive = false;
		public bool mainMenuOptions=false;
		string versionNumber = "0.0542dev";

		public object EVEinstance;
		public SunlightModulator sunlightModulatorInstance;
		
//		public ShadowMaskModulateCommandBuffer shadowMaskModulate;
		public ShadowRemoveFadeCommandBuffer shadowFadeRemover;
		DepthToDistanceCommandBuffer farDepthCommandbuffer, nearDepthCommandbuffer;

		public TweakFarCameraShadowCascades farCameraShadowCascadeTweaker;

		void Awake ()
		{
            if (instance == null)
            {
                instance = this;
                Utils.Log("Core instance created");
            }
            else
            {
                //destroy any duplicate instances that may be created by a duplicate install
                Utils.Log("Destroying duplicate instance, check your install for duplicate mod folders");
                UnityEngine.Object.Destroy(this);
            }
            string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			path = Uri.UnescapeDataString (uri.Path);
			path = Path.GetDirectoryName (path);

			//this doesn't look nice, do it properly
			int index = path.LastIndexOf ("GameData");
			gameDataPath= path.Remove(index+9, path.Length-index-9);

			windowId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

			//load the planets list and the settings
			loadSettings ();

			//find all celestial bodies, used for finding scatterer-enabled bodies and disabling the stock ocean
			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));

			Utils.Log ("Version:"+versionNumber);
			Utils.Log ("Running on " + SystemInfo.graphicsDeviceVersion + " on " +SystemInfo.operatingSystem);
			Utils.Log ("Game resolution " + Screen.width.ToString() + "x" +Screen.height.ToString());
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				isActive = true;
				mainMenuOptions = (HighLogic.LoadedScene == GameScenes.SPACECENTER);
				windowRect.x=pluginData.inGameWindowLocation.x;
				windowRect.y=pluginData.inGameWindowLocation.y;
			} 
			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				isActive = true;

				//find and remove stock oceans
				if (mainSettings.useOceanShaders)
				{
					OceanUtils.removeStockOceans();
				}

				//replace EVE cloud shaders if main menu (ie on startup only)
				if (mainSettings.integrateWithEVEClouds)
				{
					ShaderReplacer.Instance.replaceEVEshaders();
				}
			}

			if (isActive)
				StartCoroutine(DelayedInit());
		}

		IEnumerator DelayedInit()
		{
			//wait for 5 frames for EVE and the game to finish setting up
			for (int i=0; i<5;i++)
				yield return new WaitForFixedUpdate();

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
				
			SetupPlanetshine ();
			
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

//			//add shadowmask modulator (adds occlusion to shadows)
//			shadowMaskModulate = (ShadowMaskModulateCommandBuffer)sunLight.AddComponent (typeof(ShadowMaskModulateCommandBuffer));
//
			//add shadow far plane fixer
			shadowFadeRemover = (ShadowRemoveFadeCommandBuffer)nearCamera.gameObject.AddComponent (typeof(ShadowRemoveFadeCommandBuffer));

			//find EVE clouds
			if (mainSettings.integrateWithEVEClouds)
			{
				mapEVEClouds();
			}

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
			Utils.Log("Core setup done");
		}

		void Update ()
		{
			//toggle whether GUI is visible or not
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

			if (coreInitiated)
			{
				if (callCollector)
				{
					GC.Collect();
					callCollector=false;
				}
				
				//custom lens flares
				//TODO: move to init
				if ((mainSettings.fullLensFlareReplacement) && !customSunFlareAdded && (HighLogic.LoadedScene != GameScenes.MAINMENU))
				{
					//disable stock sun flares
					global::SunFlare[] stockFlares = (global::SunFlare[]) global::SunFlare.FindObjectsOfType(typeof( global::SunFlare));
					foreach(global::SunFlare _flare in stockFlares)
					{
						if (sunflaresList.Contains(_flare.sun.name))
						{
							Utils.Log("Disabling stock sunflare for "+_flare.sun.name);
							_flare.sunFlare.enabled=false;
							_flare.enabled=false;
							_flare.gameObject.SetActive(false);
						}
					}
					
					foreach (string sunflareBody in sunflaresList)
					{
						SunFlare customSunFlare =(SunFlare) scaledSpaceCamera.gameObject.AddComponent(typeof(SunFlare));
						
						try
						{
							customSunFlare.source=CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == sunflareBody);
							customSunFlare.sourceName=sunflareBody;
							customSunFlare.sourceScaledTransform = Utils.GetScaledTransform(customSunFlare.source.name);
							customSunFlare.start ();
							customSunFlares.Add(customSunFlare);
						}
						catch (Exception stupid)
						{
							Utils.Log("Custom sunflare cannot be added to "+sunflareBody+" "+stupid.ToString());
							
							Component.Destroy(customSunFlare);
							UnityEngine.Object.Destroy(customSunFlare);
							
							if (customSunFlares.Contains(customSunFlare))
							{
								customSunFlares.Remove(customSunFlare);
							}
							
							continue;
						}
					}
					customSunFlareAdded=true;
				}

				//TODO: move to init
				if (mainSettings.disableAmbientLight && !ambientLightScript)
				{
					ambientLightScript = (DisableAmbientLight) scaledSpaceCamera.gameObject.AddComponent (typeof(DisableAmbientLight));
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
				foreach (ScattererCelestialBody _cur in scattererCelestialBodies)
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
								
								Utils.Log ("Effects unloaded for " + _cur.celestialBodyName);
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
									GUItool.selectedPlanet = scattererCelestialBodies.IndexOf (_cur);
									GUItool.getSettingsFromSkynode ();

									if (!ReferenceEquals(_cur.m_manager.GetOceanNode(),null)) {
										GUItool.getSettingsFromOceanNode ();
									}
									callCollector=true;
									Utils.Log ("Effects loaded for " + _cur.celestialBodyName);
								}
								catch(Exception e)
								{
									Utils.Log ("Effects couldn't be loaded for " + _cur.celestialBodyName +" because of exception: "+e.ToString());
									try
									{
										_cur.m_manager.OnDestroy();
									}
									catch(Exception ee)
									{
										Utils.Log ("manager couldn't be removed for " + _cur.celestialBodyName +" because of exception: "+ee.ToString());
									}
									scattererCelestialBodies.Remove(_cur);
									Utils.Log (""+ _cur.celestialBodyName +" removed from active planets.");
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
				//update sun flare
				if (mainSettings.fullLensFlareReplacement)
				{
					foreach (SunFlare customSunFlare in customSunFlares)
					{
						customSunFlare.updateNode();
					}
				}
				//update planetshine lights
				if(mainSettings.usePlanetShine)
				{
					foreach (PlanetShineLight _aLight in celestialLightSources)
					{
						_aLight.updateLight();
						
					}
				}
			}
		} 


		void OnDestroy ()
		{
			if (isActive)
			{
				if(mainSettings.usePlanetShine)
				{
					foreach (PlanetShineLight _aLight in celestialLightSources)
					{
						_aLight.OnDestroy();
						UnityEngine.Object.Destroy(_aLight);
					}
				}

				for (int i = 0; i < scattererCelestialBodies.Count; i++) {
					
					ScattererCelestialBody cur = scattererCelestialBodies [i];
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


				if (mainSettings.fullLensFlareReplacement && customSunFlareAdded)
				{
					foreach (SunFlare customSunFlare in customSunFlares)
					{
						customSunFlare.cleanUp();
						Component.Destroy (customSunFlare);
					}

					//re-enable stock sun flares
					global::SunFlare[] stockFlares = (global::SunFlare[]) global::SunFlare.FindObjectsOfType(typeof( global::SunFlare));
					foreach(global::SunFlare _flare in stockFlares)
					{						
						if (sunflaresList.Contains(_flare.sun.name))
						{
							_flare.sunFlare.enabled=true;
						}
					}
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

				pluginData.inGameWindowLocation=new Vector2(windowRect.x,windowRect.y);
				saveSettings();
			}

			UnityEngine.Object.Destroy (GUItool);
			
		}

		void OnGUI ()
		{

			//why not move this shit to guiHandler?
			if (visible)
			{
				windowRect = GUILayout.Window (windowId, windowRect, GUItool.DrawScattererWindow,"Scatterer v"+versionNumber+": "
				                               + pluginData.guiModifierKey1String+"/"+pluginData.guiModifierKey2String +"+" +pluginData.guiKey1String
				                               +"/"+pluginData.guiKey2String+" toggle");

				//prevent window from going offscreen
				windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
				windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);

				//for debugging
//				if (bufferRenderingManager.depthTexture)
//				{
//					GUI.DrawTexture(new Rect(0,0,1280, 720), bufferRenderingManager.depthTexture);
//				}
			}
		}
		
		public void loadSettings ()
		{
			baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config"); //only used for displaying filepath

			mainSettings.loadMainSettings ();
			pluginData.loadPluginData ();

			PlanetsListReader scattererPlanetsListReader = new PlanetsListReader ();
			scattererPlanetsListReader.loadPlanetsListToCore (); //remove ToCore

			atmoConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_atmosphere");
			oceanConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_ocean");
			sunflareConfigs = GameDatabase.Instance.GetConfigNodes ("Scatterer_sunflare");
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
					Utils.Log("Override near clip plane from:"+nearCamera.nearClipPlane.ToString()+" to:"+mainSettings.nearClipPlane.ToString());
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
			foreach (ScattererCelestialBody sctBody in scattererCelestialBodies)
			{
				var _idx = 0;
			
				var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
				
				if (celBody == null)
				{
					celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
				}
				
				Utils.Log ("Celestial Body: " + celBody);
				if (celBody != null)
				{
					_idx = scattererCelestialBodies.IndexOf (sctBody);
					Utils.Log ("Found: " + sctBody.celestialBodyName + " / " + celBody.GetName ());
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
				Utils.Log("Number of shadow cascades detected "+QualitySettings.shadowCascades.ToString());


				if (mainSettings.shadowsOnOcean)
					QualitySettings.shadowProjection = ShadowProjection.CloseFit; //with ocean shadows
				else
					QualitySettings.shadowProjection = ShadowProjection.StableFit; //without ocean shadows

				//set shadow bias
				//fixes checkerboard artifacts aka shadow acne
				lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
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
			lights = (Light[])Light.FindObjectsOfType (typeof(Light));
			foreach (Light _light in lights) {
				if (_light.gameObject.name == "SunLight") {
					sunLight = _light.gameObject;
				}
				if (_light.gameObject.name.Contains ("PlanetLight") || _light.gameObject.name.Contains ("Directional light")) {
					mainMenuLight = _light.gameObject;
					Utils.Log ("Found main menu light");
				}
			}
		}

		//move to its own class
		void SetupPlanetshine ()
		{
			if (mainSettings.usePlanetShine)
			{
				//load planetshine "cookie" cubemap
				planetShineCookieCubeMap = new Cubemap (512, TextureFormat.ARGB32, true);
				Texture2D[] cubeMapFaces = new Texture2D[6];
				for (int i = 0; i < 6; i++) {
					cubeMapFaces [i] = new Texture2D (512, 512);
				}
				cubeMapFaces [0].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_NegativeX.png")));
				cubeMapFaces [1].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_PositiveX.png")));
				cubeMapFaces [2].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_NegativeY.png")));
				cubeMapFaces [3].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_PositiveY.png")));
				cubeMapFaces [4].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_NegativeZ.png")));
				cubeMapFaces [5].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path + "/planetShineCubemap", "_PositiveZ.png")));
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [0].GetPixels (), CubemapFace.NegativeX);
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [1].GetPixels (), CubemapFace.PositiveX);
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [2].GetPixels (), CubemapFace.NegativeY);
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [3].GetPixels (), CubemapFace.PositiveY);
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [4].GetPixels (), CubemapFace.NegativeZ);
				planetShineCookieCubeMap.SetPixels (cubeMapFaces [5].GetPixels (), CubemapFace.PositiveZ);
				planetShineCookieCubeMap.Apply ();


				foreach (PlanetShineLightSource _aSource in celestialLightSourcesData) {
					var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == _aSource.bodyName);
					if (celBody) {
						PlanetShineLight aPsLight = new PlanetShineLight ();
						aPsLight.isSun = _aSource.isSun;
						aPsLight.source = celBody;
						if (!_aSource.isSun)
							aPsLight.sunCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _aSource.mainSunCelestialBody);
						GameObject ScaledPlanetShineLight = (UnityEngine.GameObject)Instantiate (scaledspaceSunLight);
						GameObject LocalPlanetShineLight = (UnityEngine.GameObject)Instantiate (scaledspaceSunLight);
						ScaledPlanetShineLight.GetComponent<Light> ().type = LightType.Point;
						if (!_aSource.isSun)
							ScaledPlanetShineLight.GetComponent<Light> ().cookie = planetShineCookieCubeMap;
						//ScaledPlanetShineLight.GetComponent<Light>().range=1E9f;
						ScaledPlanetShineLight.GetComponent<Light> ().range = _aSource.scaledRange;
						ScaledPlanetShineLight.GetComponent<Light> ().color = new Color (_aSource.color.x, _aSource.color.y, _aSource.color.z);
						ScaledPlanetShineLight.name = celBody.name + "PlanetShineLight(ScaledSpace)";
						LocalPlanetShineLight.GetComponent<Light> ().type = LightType.Point;
						if (!_aSource.isSun)
							LocalPlanetShineLight.GetComponent<Light> ().cookie = planetShineCookieCubeMap;
						//LocalPlanetShineLight.GetComponent<Light>().range=1E9f;
						LocalPlanetShineLight.GetComponent<Light> ().range = _aSource.scaledRange * 6000;
						LocalPlanetShineLight.GetComponent<Light> ().color = new Color (_aSource.color.x, _aSource.color.y, _aSource.color.z);
						LocalPlanetShineLight.GetComponent<Light> ().cullingMask = 557591;
						LocalPlanetShineLight.GetComponent<Light> ().shadows = LightShadows.Soft;
						LocalPlanetShineLight.GetComponent<Light> ().shadowCustomResolution = 2048;
						LocalPlanetShineLight.name = celBody.name + "PlanetShineLight(LocalSpace)";
						aPsLight.scaledLight = ScaledPlanetShineLight;
						aPsLight.localLight = LocalPlanetShineLight;
						celestialLightSources.Add (aPsLight);
						Utils.Log ("Added celestialLightSource " + aPsLight.source.name);
					}
				}
			}
		}
		


		//map EVE clouds to planet names
		//move to own class
		public void mapEVEClouds()
		{
			Utils.Log ("mapping EVE clouds");
			EVEClouds.Clear();
			EVECloudObjects.Clear ();

			//find EVE base type
			Type EVEType = EVEReflectionUtils.getType("Atmosphere.CloudsManager"); 

			if (EVEType == null)
			{
				Utils.Log("Eve assembly type not found");
				return;
			}
			else
			{
				Utils.Log("Eve assembly type found");
			}

			Utils.Log("Eve assembly version: " + EVEType.Assembly.GetName().ToString());

			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;

			try
			{
//				EVEinstance = EVEType.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
				EVEinstance = EVEType.GetField("instance", flags).GetValue(null) ;
			}
			catch (Exception)
			{
				Utils.Log("No EVE Instance found");
				return;
			}
			if (EVEinstance == null)
			{
				Utils.Log("Failed grabbing EVE Instance");
				return;
			}
			else
			{
				Utils.Log("Successfully grabbed EVE Instance");
			}

			IList objectList = EVEType.GetField ("ObjectList", flags).GetValue (EVEinstance) as IList;

			foreach (object _obj in objectList)
			{
				String body = _obj.GetType().GetField("body", flags).GetValue(_obj) as String;

				if (EVECloudObjects.ContainsKey(body))
				{
					EVECloudObjects[body].Add(_obj);
				}
				else
				{
					List<object> objectsList = new List<object>();
					objectsList.Add(_obj);
					EVECloudObjects.Add(body,objectsList);
				}

				object cloud2dObj;
				if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				{
					object cloudsPQS = _obj.GetType().GetField("cloudsPQS", flags).GetValue(_obj) as object;

					if (cloudsPQS==null)
					{
						Utils.Log("cloudsPQS not found for layer on planet :"+body);
						continue;
					}
					cloud2dObj = cloudsPQS.GetType().GetField("mainMenuLayer", flags).GetValue(cloudsPQS) as object;
				}
				else
				{
					cloud2dObj = _obj.GetType().GetField("layer2D", flags).GetValue(_obj) as object;
				}

				if (cloud2dObj==null)
				{
					Utils.Log("layer2d not found for layer on planet :"+body);
					continue;
				}

				GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
				if (cloudmesh==null)
				{
					Utils.Log("cloudmesh null");
					return;
				}

				if (EVEClouds.ContainsKey(body))
				{
					EVEClouds[body].Add(cloudmesh.GetComponent < MeshRenderer > ().material);
				}
				else
				{
					List<Material> cloudsList = new List<Material>();
					cloudsList.Add(cloudmesh.GetComponent < MeshRenderer > ().material);
					EVEClouds.Add(body,cloudsList);
				}
				Utils.Log("Detected EVE 2d cloud layer for planet: "+body);
			}
		}

		public void onRenderTexturesLost()
		{
			foreach (ScattererCelestialBody _cur in scattererCelestialBodies)
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
