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
		
		private Core()
		{
			if (instance == null)
			{
				instance = this;
				Debug.Log("[Scatterer] Core instance created");
			}
			else
			{
				//destroy any duplicate instances that may be created by a duplicate install
				Debug.Log("[Scatterer] Destroying duplicate instance, check your install for duplicate mod folders");
				UnityEngine.Object.Destroy (this);
			}
		}
		
		public static Core Instance
		{
			get 
			{
				return instance;
			}
		}

		public Rect windowRect = new Rect (0, 0, 400, 50);
		int windowId = UnityEngine.Random.Range(int.MinValue,int.MaxValue);

		GUIhandler GUItool= new GUIhandler();


		public List<SunFlare> customSunFlares = new List<SunFlare>();
		bool customSunFlareAdded=false;
		
		public bool visible = false;
		
		public bool autosavePlanetSettingsOnSceneChange=true;
		
		public bool disableAmbientLight=false;

		public string mainSunCelestialBodyName="Sun";

		public bool integrateWithEVEClouds=false;

		DisableAmbientLight ambientLightScript;
		
		public List <ScattererCelestialBody > scattererCelestialBodies = new List <ScattererCelestialBody> {};

		public List<string> sunflaresList=new List<string> {};

		CelestialBody[] CelestialBodies;
		
		Light[] lights;

		//map EVE 2d cloud materials to planet names
		public Dictionary<String, List<Material> > EVEClouds = new Dictionary<String, List<Material> >();

		//map EVE CloudObjects to planet names
		//as far as I understand CloudObjects in EVE contain the 2d clouds and the volumetrics for a given
		//layer on a given planet, however due to the way they are handled in EVE they don't directly reference
		//their parent planet and the volumetrics are only created when the PQS is active
		//I map them here to facilitate accessing the volumetrics later
		public Dictionary<String, List<object>> EVECloudObjects = new Dictionary<String, List<object>>();

		public GameObject sunLight,scaledspaceSunLight, mainMenuLight;

		public bool overrideNearClipPlane=false;
		public float nearClipPlane=0.5f;
		
		public bool useOceanShaders = true;
		public bool shadowsOnOcean = true;
		public bool oceanSkyReflections = true;
		public bool oceanRefraction = true;
		public bool oceanPixelLights = false;
		public bool fullLensFlareReplacement = true;
		public bool sunlightExtinction = true;
		public bool underwaterLightDimming = true;
		
		bool callCollector=false;

		public bool craft_WaveInteractions = false;
		public bool useEclipses = true;		
		public bool useRingShadows = true;		
		public bool usePlanetShine = false;

		public List<PlanetShineLightSource> celestialLightSourcesData=new List<PlanetShineLightSource> {};	
		List<PlanetShineLight> celestialLightSources=new List<PlanetShineLight> {};
		Cubemap planetShineCookieCubeMap;
		public UrlDir.UrlConfig[] baseConfigs,atmoConfigs,oceanConfigs;
		public ConfigNode[] sunflareConfigs;
		
		public bool terrainShadows = true;
		public float shadowNormalBias=0.4f;
		public float shadowBias=0.125f;
		public float shadowsDistance=100000;

		public bool showMenuOnStart = true;
		public int scrollSectionHeight = 500;
		Vector2 inGameWindowLocation=Vector2.zero;
		string guiModifierKey1String=KeyCode.LeftAlt.ToString();
		string guiModifierKey2String=KeyCode.RightAlt.ToString();
		string guiKey1String=KeyCode.F10.ToString();
		string guiKey2String=KeyCode.F11.ToString();
		KeyCode guiKey1, guiKey2, guiModifierKey1, guiModifierKey2 ;

		//means a PQS enabled for the closest celestial body, regardless of whether it uses scatterer effects or not
		bool globalPQSEnabled = false;

		public bool isGlobalPQSEnabled
		{
			get
			{
				return globalPQSEnabled;
			}
		}

		//means a PQS enabled for a celestial body which scatterer effects are active on (is this useless?)
		bool pqsEnabledOnScattererPlanet = false;

		public bool isPQSEnabledOnScattererPlanet
		{
			get
			{
				return pqsEnabledOnScattererPlanet;
			}
		}

		public bool underwater = false;

		public BufferRenderingManager bufferRenderingManager;

		public CelestialBody sunCelestialBody;
		public CelestialBody munCelestialBody;
		public string path, gameDataPath;
		bool coreInitiated = false;
		public bool extinctionEnabled = true;
		
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		
		public int m_fourierGridSize = 128; //This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.

		public bool isActive = false;
		public bool mainMenuOptions=false;
		string versionNumber = "0.052";

		public object EVEinstance;
		public SunlightModulator sunlightModulatorInstance;
		
//		public ShadowMaskModulateCommandBuffer shadowMaskModulate;
		public ShadowRemoveFadeCommandBuffer shadowFadeRemover;

		public TweakFarCameraShadowCascades farCameraShadowCascadeTweaker;
		
		public Transform GetScaledTransform (string body)
		{
			return (ScaledSpace.Instance.transform.FindChild (body));	
		}
		

		public static GameObject GetMainMenuObject(string name)
		{
			GameObject kopernicusMainMenuObject = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault
					(b => b.name == (name+"(Clone)") && b.transform.parent.name.Contains("Scene"));

			if (kopernicusMainMenuObject != null)
				return kopernicusMainMenuObject;

			GameObject kspMainMenuObject = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault(b => b.name == name && b.transform.parent.name.Contains("Scene"));

			if (kspMainMenuObject == null)
			{
				throw new Exception("No correct main menu object found for "+name);
			}

			return kspMainMenuObject;
		}

		void Awake ()
		{
			string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			path = Uri.UnescapeDataString (uri.Path);
			path = Path.GetDirectoryName (path);

			//this doesn't look nice, do it properly
			int index = path.LastIndexOf ("GameData");
			gameDataPath= path.Remove(index+9, path.Length-index-9);

			//load the planets list and the settings
			loadSettings ();

			//find all celestial bodies, used for finding scatterer-enabled bodies and disabling the stock ocean
			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));

			Debug.Log ("[Scatterer] Version:"+versionNumber);
			Debug.Log ("[Scatterer] Running on " + SystemInfo.graphicsDeviceVersion + " on " +SystemInfo.operatingSystem);
			Debug.Log ("[Scatterer] Game resolution " + Screen.width.ToString() + "x" +Screen.height.ToString());
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				isActive = true;
				mainMenuOptions = (HighLogic.LoadedScene == GameScenes.SPACECENTER);
				windowRect.x=inGameWindowLocation.x;
				windowRect.y=inGameWindowLocation.y;
			} 
			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				isActive = true;

				//find and remove stock oceans
				if (useOceanShaders)
				{
					removeStockOceans();
				}

				//replace EVE cloud shaders if main menu (ie on startup only)
				if (integrateWithEVEClouds)
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
			//set shadows
			setShadows();
			
			//find scatterer celestial bodies
			findScattererCelestialBodies();
			
			//find sun
			sunCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == mainSunCelestialBodyName);
			
			//find main cameras
			Camera[] cams = Camera.allCameras;

			scaledSpaceCamera = Camera.allCameras.FirstOrDefault(_cam  => _cam.name == "Camera ScaledSpace");
			farCamera = Camera.allCameras.FirstOrDefault(_cam  => _cam.name == "Camera 01");
			nearCamera = Camera.allCameras.FirstOrDefault(_cam  => _cam.name == "Camera 00");

			if (scaledSpaceCamera && farCamera && nearCamera)
			{
				farCameraShadowCascadeTweaker = (TweakFarCameraShadowCascades) farCamera.gameObject.AddComponent(typeof(TweakFarCameraShadowCascades));

				if (overrideNearClipPlane)
				{
					Debug.Log("[Scatterer] Override near clip plane from:"+nearCamera.nearClipPlane.ToString()+" to:"+nearClipPlane.ToString());
					nearCamera.nearClipPlane = nearClipPlane;
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

			//find sunlight and set shadow bias
			lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
			
			foreach (Light _light in lights)
			{
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledspaceSunLight=_light.gameObject;
					
					_light.shadowNormalBias =shadowNormalBias;
					_light.shadowBias=shadowBias;
				}
				
				if (_light.gameObject.name == "SunLight")
				{
					sunLight=_light.gameObject;
				}	

				
				if (_light.gameObject.name.Contains ("PlanetLight") || _light.gameObject.name.Contains ("Directional light"))
				{
					mainMenuLight = _light.gameObject;
					Debug.Log("[Scatterer] Found main menu light");
				}
			}
			
			//load planetshine "cookie" cubemap
			if(usePlanetShine)
			{
				planetShineCookieCubeMap=new Cubemap(512,TextureFormat.ARGB32,true);
				
				Texture2D[] cubeMapFaces=new Texture2D[6];
				for (int i=0;i<6;i++)
				{
					cubeMapFaces[i]=new Texture2D(512,512);
				}
				
				cubeMapFaces[0].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_NegativeX.png")));
				cubeMapFaces[1].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_PositiveX.png")));
				cubeMapFaces[2].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_NegativeY.png")));
				cubeMapFaces[3].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_PositiveY.png")));
				cubeMapFaces[4].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_NegativeZ.png")));
				cubeMapFaces[5].LoadImage(System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", path+"/planetShineCubemap", "_PositiveZ.png")));
				
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[0].GetPixels(),CubemapFace.NegativeX);
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[1].GetPixels(),CubemapFace.PositiveX);
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[2].GetPixels(),CubemapFace.NegativeY);
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[3].GetPixels(),CubemapFace.PositiveY);
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[4].GetPixels(),CubemapFace.NegativeZ);
				planetShineCookieCubeMap.SetPixels(cubeMapFaces[5].GetPixels(),CubemapFace.PositiveZ);
				planetShineCookieCubeMap.Apply();
			}
			
			//find and fix renderQueues of kopernicus rings
			foreach (CelestialBody _cb in CelestialBodies)
			{
				GameObject ringObject;
				ringObject=GameObject.Find(_cb.name+"Ring");
				if (ringObject)
				{
					ringObject.GetComponent < MeshRenderer > ().material.renderQueue = 3005;
					Debug.Log("[Scatterer] Found rings for "+_cb.name);
				}
			}
			
			//find and fix renderqueue of sun corona
			Transform scaledSunTransform=GetScaledTransform(mainSunCelestialBodyName);
			foreach (Transform child in scaledSunTransform)
			{
				MeshRenderer temp = child.gameObject.GetComponent<MeshRenderer>();
				if (temp!=null)
					temp.material.renderQueue = 2998;
			}
			
			//set up planetshine lights
			if(usePlanetShine)
			{
				foreach (PlanetShineLightSource _aSource in celestialLightSourcesData)
				{
					var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == _aSource.bodyName);
					if (celBody)
					{
						PlanetShineLight aPsLight= new PlanetShineLight();
						aPsLight.isSun=_aSource.isSun;
						aPsLight.source=celBody;
						aPsLight.sunCelestialBody=sunCelestialBody;
						
						GameObject ScaledPlanetShineLight=(UnityEngine.GameObject) Instantiate(scaledspaceSunLight);
						GameObject LocalPlanetShineLight=(UnityEngine.GameObject) Instantiate(scaledspaceSunLight);
						
						ScaledPlanetShineLight.GetComponent<Light>().type=LightType.Point;
						if (!_aSource.isSun)
							ScaledPlanetShineLight.GetComponent<Light>().cookie=planetShineCookieCubeMap;
						
						//ScaledPlanetShineLight.GetComponent<Light>().range=1E9f;
						ScaledPlanetShineLight.GetComponent<Light>().range=_aSource.scaledRange;
						ScaledPlanetShineLight.GetComponent<Light>().color=new Color(_aSource.color.x,_aSource.color.y,_aSource.color.z);
						ScaledPlanetShineLight.name=celBody.name+"PlanetShineLight(ScaledSpace)";
						
						
						LocalPlanetShineLight.GetComponent<Light>().type=LightType.Point;
						if (!_aSource.isSun)
							LocalPlanetShineLight.GetComponent<Light>().cookie=planetShineCookieCubeMap;
						//LocalPlanetShineLight.GetComponent<Light>().range=1E9f;
						LocalPlanetShineLight.GetComponent<Light>().range=_aSource.scaledRange*6000;
						LocalPlanetShineLight.GetComponent<Light>().color=new Color(_aSource.color.x,_aSource.color.y,_aSource.color.z);
						LocalPlanetShineLight.GetComponent<Light>().cullingMask=557591;
						LocalPlanetShineLight.GetComponent<Light>().shadows=LightShadows.Soft;
						LocalPlanetShineLight.GetComponent<Light>().shadowCustomResolution=2048;
						LocalPlanetShineLight.name=celBody.name+"PlanetShineLight(LocalSpace)";
						
						aPsLight.scaledLight=ScaledPlanetShineLight;
						aPsLight.localLight=LocalPlanetShineLight;
						
						celestialLightSources.Add(aPsLight);
						Debug.Log ("[Scatterer] Added celestialLightSource "+aPsLight.source.name);
					}
				}
			}
			
			//create buffer manager
			if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
			{
				bufferRenderingManager = (BufferRenderingManager)farCamera.gameObject.AddComponent (typeof(BufferRenderingManager));
				bufferRenderingManager.start();

				//copy stock depth buffers and combine into a single depth buffer
				if (useOceanShaders || fullLensFlareReplacement)
				{
					farCamera.gameObject.AddComponent<DepthToDistanceCommandBuffer>();
					nearCamera.gameObject.AddComponent<DepthToDistanceCommandBuffer>();
				}
			}

//			//add shadowmask modulator (adds occlusion to shadows)
//			shadowMaskModulate = (ShadowMaskModulateCommandBuffer)sunLight.AddComponent (typeof(ShadowMaskModulateCommandBuffer));
//
			//add shadow far plane fixer
			shadowFadeRemover = (ShadowRemoveFadeCommandBuffer)nearCamera.gameObject.AddComponent (typeof(ShadowRemoveFadeCommandBuffer));

			//find EVE clouds
			if (integrateWithEVEClouds)
			{
				mapEVEClouds();
			}

			//magically fix stupid issues when reverting to space center from map view
			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				MapView.MapIsEnabled = false;
			}

			//create sunlightModulator
			if (sunlightExtinction || (underwaterLightDimming && useOceanShaders))
			{
				sunlightModulatorInstance = (SunlightModulator) Core.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SunlightModulator));
			}

			coreInitiated = true;
			Debug.Log("[Scatterer] Core setup done");
		}

		void Update ()
		{
			//toggle whether GUI is visible or not
			if ((Input.GetKey (guiModifierKey1) || Input.GetKey (guiModifierKey2)) && (Input.GetKeyDown (guiKey1) || (Input.GetKeyDown (guiKey2))))
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
				if ((fullLensFlareReplacement) && !customSunFlareAdded && (HighLogic.LoadedScene != GameScenes.MAINMENU))
				{
					//disable stock sun flares
					global::SunFlare[] stockFlares = (global::SunFlare[]) global::SunFlare.FindObjectsOfType(typeof( global::SunFlare));
					foreach(global::SunFlare _flare in stockFlares)
					{
						if (sunflaresList.Contains(_flare.sun.name))
						{
							Debug.Log("[Scatterer] Disabling stock sunflare for "+_flare.sun.name);
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
							customSunFlare.sourceScaledTransform = GetScaledTransform(customSunFlare.source.name);
							customSunFlare.start ();
							customSunFlares.Add(customSunFlare);
						}
						catch (Exception stupid)
						{
							Debug.Log("[Scatterer] Custom sunflare cannot be added to "+sunflareBody+" "+stupid.ToString());
							
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
				if (disableAmbientLight && !ambientLightScript)
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
								
								Debug.Log ("[Scatterer] Effects unloaded for " + _cur.celestialBodyName);
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
									_cur.m_manager = new Manager ();
									_cur.m_manager.setParentCelestialBody (_cur.celestialBody);
									if (HighLogic.LoadedScene == GameScenes.MAINMENU)
									{
										_cur.m_manager.setParentScaledTransform (GetMainMenuObject(_cur.celestialBodyName).transform); //doesn't look right but let's see
										_cur.m_manager.setParentLocalTransform  (GetMainMenuObject(_cur.celestialBodyName).transform);
									}
									else
									{
										_cur.m_manager.setParentScaledTransform (_cur.transform);
										_cur.m_manager.setParentLocalTransform (_cur.celestialBody.transform);
									}
									CelestialBody currentSunCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _cur.mainSunCelestialBody);
									_cur.m_manager.setSunCelestialBody (currentSunCelestialBody);
									
									//Find eclipse casters
									List<CelestialBody> eclipseCasters=new List<CelestialBody> {};

									if (useEclipses)
									{
										for (int k=0; k < _cur.eclipseCasters.Count; k++)
										{
											var cc = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _cur.eclipseCasters[k]);
											if (cc==null)
												Debug.Log("[Scatterer] Eclipse caster "+_cur.eclipseCasters[k]+" not found for "+_cur.celestialBodyName);
											else
											{
												eclipseCasters.Add(cc);
												Debug.Log("[Scatterer] Added eclipse caster "+_cur.eclipseCasters[k]+" for "+_cur.celestialBodyName);
											}
										}
										_cur.m_manager.eclipseCasters=eclipseCasters;
									}
									List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};

									if (usePlanetShine)
									{								
										for (int k=0; k < _cur.planetshineSources.Count; k++)
										{
											var cc = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _cur.planetshineSources[k].bodyName);
											if (cc==null)
												Debug.Log("[Scatterer] planetshine source "+_cur.planetshineSources[k].bodyName+" not found for "+_cur.celestialBodyName);
											else
											{
												AtmoPlanetShineSource src=_cur.planetshineSources[k];
												src.body=cc;
												_cur.planetshineSources[k].body=cc;
												planetshineSources.Add (src);
												Debug.Log("[Scatterer] Added planetshine source"+_cur.planetshineSources[k].bodyName+" for "+_cur.celestialBodyName);
											}
										}
										_cur.m_manager.planetshineSources = planetshineSources;
									}
									if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
										_cur.hasOcean=false;

									_cur.m_manager.hasOcean = _cur.hasOcean;
									_cur.m_manager.flatScaledSpaceModel = _cur.flatScaledSpaceModel;
									_cur.m_manager.usesCloudIntegration = _cur.usesCloudIntegration;
									_cur.m_manager.Awake ();
									_cur.active = true;
									
									GUItool.selectedConfigPoint = 0;
									GUItool.displayOceanSettings = false;
									GUItool.selectedPlanet = scattererCelestialBodies.IndexOf (_cur);
									GUItool.getSettingsFromSkynode ();

									if (!ReferenceEquals(_cur.m_manager.GetOceanNode(),null)) {
										GUItool.getSettingsFromOceanNode ();
									}
									callCollector=true;
									Debug.Log ("[Scatterer] Effects loaded for " + _cur.celestialBodyName);
								}
								catch(Exception e)
								{
									Debug.Log ("[Scatterer] Effects couldn't be loaded for " + _cur.celestialBodyName +" because of exception: "+e.ToString());
									try
									{
										_cur.m_manager.OnDestroy();
									}
									catch(Exception ee)
									{
										Debug.Log ("[Scatterer] manager couldn't be removed for " + _cur.celestialBodyName +" because of exception: "+ee.ToString());
									}
									scattererCelestialBodies.Remove(_cur);
									Debug.Log ("[Scatterer] "+ _cur.celestialBodyName +" removed from active planets.");
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
				if (fullLensFlareReplacement)
				{
					foreach (SunFlare customSunFlare in customSunFlares)
					{
						customSunFlare.updateNode();
					}
				}
				//update planetshine lights
				if(usePlanetShine)
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
				if(usePlanetShine)
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


				if (bufferRenderingManager)
				{
					bufferRenderingManager.OnDestroy();
					Component.Destroy (bufferRenderingManager);
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


				if (fullLensFlareReplacement && customSunFlareAdded)
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

				if (farCamera && farCamera.gameObject.GetComponent (typeof(DepthToDistanceCommandBuffer)))
					Component.Destroy (farCamera.gameObject.GetComponent (typeof(DepthToDistanceCommandBuffer)));

				if (nearCamera && nearCamera.gameObject.GetComponent (typeof(DepthToDistanceCommandBuffer)))
					Component.Destroy (nearCamera.gameObject.GetComponent (typeof(DepthToDistanceCommandBuffer)));

				inGameWindowLocation=new Vector2(windowRect.x,windowRect.y);
				saveSettings();
			}

			UnityEngine.Object.Destroy (GUItool);
			
		}

		void OnGUI ()
		{
			if (visible)
			{
				windowRect = GUILayout.Window (windowId, windowRect, GUItool.DrawScattererWindow,"Scatterer v"+versionNumber+": "
				                               + guiModifierKey1String+"/"+guiModifierKey2String +"+" +guiKey1String
				                               +"/"+guiKey2String+" toggle");

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
			//only used for displaying filepath
			baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");

			//load mod settings
			MainSettingsReadWrite mainSettings = new MainSettingsReadWrite();
			mainSettings.loadPluginMainSettingsToCore ();

			//load pluginData
			PluginDataReadWrite pluginData = new PluginDataReadWrite();
			pluginData.loadPluginDataToCore ();

			guiKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey1String);
			guiKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey2String);

			guiModifierKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey1String);
			guiModifierKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey2String);

			//load planetsList, light sources list and sunflares list
			PlanetsListReader scattererPlanetsListReader = new PlanetsListReader ();
			scattererPlanetsListReader.loadPlanetsListToCore ();

			//load atmo and ocean configs
			atmoConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_atmosphere");
			oceanConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_ocean");

			//load sunflare configs
			sunflareConfigs = GameDatabase.Instance.GetConfigNodes ("Scatterer_sunflare");
		}
		
		public void saveSettings ()
		{
			//save pluginData
			PluginDataReadWrite pluginData = new PluginDataReadWrite();
			pluginData.saveCorePluginData ();

			//save mod settings
			MainSettingsReadWrite mainSettings = new MainSettingsReadWrite();
			mainSettings.saveCoreMainSettingsIfChanged ();
		}

		void removeStockOceans()
		{
			Material invisibleOcean = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
			foreach (ScattererCelestialBody sctBody in scattererCelestialBodies)
			{
				if (sctBody.hasOcean)
				{
					bool removed = false;
					var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
					if (celBody == null)
					{
						celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
					}
					
					if (celBody != null)
					{
						//Thanks to rbray89 for this snippet and the FakeOcean class which disable the stock ocean in a clean way
						PQS pqs = celBody.pqsController;
						if ((pqs != null) && (pqs.ChildSpheres!= null) && (pqs.ChildSpheres.Count() != 0))
						{

							PQS ocean = pqs.ChildSpheres [0];
							if (ocean != null)
							{
								ocean.surfaceMaterial = invisibleOcean;
								ocean.surfaceMaterial.SetOverrideTag("IgnoreProjector","True");
								ocean.surfaceMaterial.SetOverrideTag("ForceNoShadowCasting","True");

								removed = true;
							}
						}
					}
					if (!removed) {
						Debug.Log ("[Scatterer] Couldn't remove stock ocean for " + sctBody.celestialBodyName);
					}
				}
			}
			Debug.Log ("[Scatterer] Removed stock oceans");
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
				
				Debug.Log ("[Scatterer] Celestial Body: " + celBody);
				if (celBody != null)
				{
					_idx = scattererCelestialBodies.IndexOf (sctBody);
					Debug.Log ("[Scatterer] Found: " + sctBody.celestialBodyName + " / " + celBody.GetName ());
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

		void setShadows()
		{
			if (terrainShadows && (HighLogic.LoadedScene != GameScenes.MAINMENU ) )
			{
				QualitySettings.shadowDistance = shadowsDistance;
				Debug.Log("[Scatterer] Number of shadow cascades detected "+QualitySettings.shadowCascades.ToString());


				if (shadowsOnOcean)
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
						_light.shadowNormalBias =shadowNormalBias;
						_light.shadowBias=shadowBias;
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
	
		internal static Type getType(string name)
		{
			Type type = null;
			AssemblyLoader.loadedAssemblies.TypeOperation(t =>{if (t.FullName == name)type = t;});
			
			if (type != null)
			{
				return type;
			}
			return null;
		}

		//map EVE clouds to planet names
		public void mapEVEClouds()
		{
			Debug.Log ("[Scatterer] mapping EVE clouds");
			EVEClouds.Clear();
			EVECloudObjects.Clear ();

			//find EVE base type
			Type EVEType = getType("Atmosphere.CloudsManager"); 

			if (EVEType == null)
			{
				Debug.Log("[Scatterer] Eve assembly type not found");
				return;
			}
			else
			{
				Debug.Log("[Scatterer] Eve assembly type found");
			}

			Debug.Log("[Scatterer] Eve assembly version: " + EVEType.Assembly.GetName().ToString());

			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;

			try
			{
//				EVEinstance = EVEType.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
				EVEinstance = EVEType.GetField("instance", flags).GetValue(null) ;
			}
			catch (Exception)
			{
				Debug.Log("[Scatterer] No EVE Instance found");
				return;
			}
			if (EVEinstance == null)
			{
				Debug.Log("[Scatterer] Failed grabbing EVE Instance");
				return;
			}
			else
			{
				Debug.Log("[Scatterer] Successfully grabbed EVE Instance");
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
						Debug.Log("[Scatterer] cloudsPQS not found for layer on planet :"+body);
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
					Debug.Log("[Scatterer] layer2d not found for layer on planet :"+body);
					continue;
				}

				GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
				if (cloudmesh==null)
				{
					Debug.Log("[Scatterer] cloudmesh null");
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
				Debug.Log("[Scatterer] Detected EVE 2d cloud layer for planet: "+body);
			}
		}

		public void onRenderTexturesLost()
		{
			foreach (ScattererCelestialBody _cur in scattererCelestialBodies)
			{
				if (_cur.active)
				{
					_cur.m_manager.m_skyNode.reInitMaterialUniformsOnRenderTexturesLoss ();
					if (_cur.m_manager.hasOcean && useOceanShaders && !_cur.m_manager.m_skyNode.inScaledSpace)
					{
						_cur.m_manager.reBuildOcean ();
					}
				} 
			}
		}
	
	}
}
