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
	public class Core: MonoBehaviour
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

		PlanetsListReader scattererPlanetsListReader = new PlanetsListReader ();
		public List<SunFlare> customSunFlares = new List<SunFlare>();
		bool customSunFlareAdded=false;
		
		public bool visible = false;

		[Persistent]
		public bool disableAmbientLight=false;
		
//		[Persistent]
		public string mainSunCelestialBodyName="Sun";
		
		[Persistent]
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

		GameObject sunLight,scaledspaceSunLight;
//		public GameObject copiedScaledSunLight, copiedScaledSunLight2;
//		public GameObject copiedSunLight;

		Cubemap planetShineCookieCubeMap;

		[Persistent]
		Vector2 mainMenuWindowLocation=Vector2.zero;

		[Persistent]
		Vector2 inGameWindowLocation=Vector2.zero;

		[Persistent]
		float nearClipPlane=0.5f;

//		[Persistent]
//		public bool
//			render24bitDepthBuffer = true;

		[Persistent]
		public bool
			useAlternateShaderSQRT = false;

		[Persistent]
		public bool
			forceDisableDefaultDepthBuffer = false;

		[Persistent]
		public bool
			useOceanShaders = true;

		[Persistent]
		public bool
			oceanSkyReflections = true;

		[Persistent]
		public bool
			oceanRefraction = true;

		[Persistent]
		public bool
			oceanPixelLights = false;
		
		[Persistent]
		public bool
			drawAtmoOnTopOfClouds = true;
		
		[Persistent]
		public bool
			oceanCloudShadows = false;
		
		[Persistent]
		public bool
			fullLensFlareReplacement = true;
		
		[Persistent]
		public bool
			showMenuOnStart = true;

		[Persistent]
		public int scrollSectionHeight = 500;
		
		bool callCollector=false;
		

//		[Persistent]
		public bool craft_WaveInteractions = false;
		
		[Persistent]
		public bool
			useGodrays = true;
		
		[Persistent]
		public bool
			useEclipses = true;

		[Persistent]
		public bool
			useRingShadows = true;

//		[Persistent]
		public bool
			usePlanetShine = false;

		List<PlanetShineLightSource> celestialLightSourcesData=new List<PlanetShineLightSource> {};
		
		List<PlanetShineLight> celestialLightSources=new List<PlanetShineLight> {};

		public UrlDir.UrlConfig[] baseConfigs;
		public UrlDir.UrlConfig[] atmoConfigs;
		public UrlDir.UrlConfig[] oceanConfigs;

		public ConfigNode[] sunflareConfigs;

		[Persistent]
		public bool
			terrainShadows = true;

		[Persistent]
		public float shadowNormalBias=0.4f;
		
		[Persistent]
		public float shadowBias=0.125f;
		
		[Persistent]
		public float
			shadowsDistance=100000;
		
		//[Persistent]
		//float godrayResolution = 1f;

		[Persistent]
		string guiModifierKey1String=KeyCode.LeftAlt.ToString();

		[Persistent]
		string guiModifierKey2String=KeyCode.RightAlt.ToString();

		[Persistent]
		string guiKey1String=KeyCode.F10.ToString();
		
		[Persistent]
		string guiKey2String=KeyCode.F11.ToString();

		KeyCode guiKey1, guiKey2, guiModifierKey1, guiModifierKey2 ;
			
		public bool pqsEnabled = false;
		public bool underwater = false;
		public CustomDepthBufferCam customDepthBuffer;
		public RenderTexture customDepthBufferTexture;
		public RenderTexture godrayDepthTexture;
		public RefractionCamera refractionCam;
		public RenderTexture refractionTexture;
		
		bool depthBufferSet = false;


		
		public CelestialBody sunCelestialBody;
		public CelestialBody munCelestialBody;
		public string path, gameDataPath;
		bool found = false;
		public bool stockSunglare = false;
		public bool extinctionEnabled = true;
		
		public Camera farCamera, scaledSpaceCamera, nearCamera;
	
		public Camera chosenCamera;
		public int layer = 15;

//		//ocean variables
//		public bool stockOcean = false;

		[Persistent]
		public int m_fourierGridSize = 128; //This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.


		

		
		public bool depthbufferEnabled = false;
		public bool d3d9 = false;
		public bool opengl = false;
		public bool d3d11 = false;
		public bool isActive = false;
		public bool mainMenu=false;
		
		//Material originalMaterial;
		
		public Transform GetScaledTransform (string body)
		{
			return (ScaledSpace.Instance.transform.FindChild (body));	
		}
		

		void Awake ()
		{
			string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			path = Uri.UnescapeDataString (uri.Path);
			path = Path.GetDirectoryName (path);

			int index = path.LastIndexOf ("GameData");
			gameDataPath= path.Remove(index+9, path.Length-index-9);

			//load the planets list and the settings
			loadSettings ();

			//find all celestial bodies, used for finding scatterer-enabled bodies and disabling the stock ocean
			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));
			
			if (SystemInfo.graphicsDeviceVersion.StartsWith ("Direct3D 9"))
			{
				d3d9 = true;
			}
			else if (SystemInfo.graphicsDeviceVersion.StartsWith ("OpenGL"))
			{
				opengl = true;
			}
			else if (SystemInfo.graphicsDeviceVersion.StartsWith ("Direct3D 11"))
			{
				d3d11 = true;
			}

			Debug.Log ("[Scatterer] Detected " + SystemInfo.graphicsDeviceVersion);
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION)
			{
				isActive = true;
				windowRect.x=inGameWindowLocation.x;
				windowRect.y=inGameWindowLocation.y;
			} 

			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				mainMenu = true;
				visible = showMenuOnStart;
				windowRect.x=mainMenuWindowLocation.x;
				windowRect.y=mainMenuWindowLocation.y;
				
				//find and remove stock oceans
				if (useOceanShaders)
				{
					removeStockOceans();
				}

			}
		}

		void Update ()
		{	
			//toggle whether GUI is visible or not
			if ((Input.GetKey (guiModifierKey1) || Input.GetKey (guiModifierKey2)) && (Input.GetKeyDown (guiKey1) || (Input.GetKeyDown (guiKey2))))
				visible = !visible;

			if (isActive && ScaledSpace.Instance) {
				if (!found)
				{
					//set shadows
					setShadows();

					//find scatterer celestial bodies
					findScattererCelestialBodies();

					//find sun
					sunCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == mainSunCelestialBodyName);

					//find main cameras
					Camera[] cams = Camera.allCameras;
					for (int i = 0; i < cams.Length; i++)
					{
						if (cams [i].name == "Camera ScaledSpace")
						{
							scaledSpaceCamera = cams [i];
						}
						
						if (cams [i].name == "Camera 01")
						{
							farCamera = cams [i];

						}
						
						if (cams [i].name == "Camera 00")
						{
							nearCamera = cams [i];
							nearCamera.nearClipPlane = nearClipPlane;
						}
					}
					

					
					//find sunlight and set shadow bias
					lights = (Light[]) Light.FindObjectsOfType(typeof( Light));

					foreach (Light _light in lights)
					{
						if (_light.gameObject.name == "Scaledspace SunLight")
						{
							scaledspaceSunLight=_light.gameObject;
							Debug.Log("Found scaled sunlight");

							_light.shadowNormalBias =shadowNormalBias;
							_light.shadowBias=shadowBias;
						}
						
						if (_light.gameObject.name == "SunLight")
						{
							sunLight=_light.gameObject;
							Debug.Log("Found Sunlight");
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

//					//find and fix renderqueue of sun corona
//					Transform scaledSunTransform=GetScaledTransform(mainSunCelestialBodyName);
//					foreach (Transform child in scaledSunTransform)
//					{
//						MeshRenderer temp = child.gameObject.GetComponent<MeshRenderer>();
//						temp.material.renderQueue = 3000;
//					}

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
								
								ScaledPlanetShineLight.GetComponent<Light>().range=1E9f;
								ScaledPlanetShineLight.GetComponent<Light>().color=new Color(_aSource.color.x,_aSource.color.y,_aSource.color.z);
								ScaledPlanetShineLight.name=celBody.name+"PlanetShineLight(ScaledSpace)";
								
								
								LocalPlanetShineLight.GetComponent<Light>().type=LightType.Point;
								if (!_aSource.isSun)
									LocalPlanetShineLight.GetComponent<Light>().cookie=planetShineCookieCubeMap;
								LocalPlanetShineLight.GetComponent<Light>().range=1E9f;
								LocalPlanetShineLight.GetComponent<Light>().color=new Color(_aSource.color.x,_aSource.color.y,_aSource.color.z);
								LocalPlanetShineLight.GetComponent<Light>().cullingMask=557591;
								LocalPlanetShineLight.name=celBody.name+"PlanetShineLight(LocalSpace)";
								
								aPsLight.scaledLight=ScaledPlanetShineLight;
								aPsLight.localLight=LocalPlanetShineLight;
								
								celestialLightSources.Add(aPsLight);
								Debug.Log ("[Scatterer] Added celestialLightSource "+aPsLight.source.name);
							}
						}
					}


					//find EVE clouds
					if (integrateWithEVEClouds)
					{
						mapEVEClouds();
					}
					
					found = true;
				}
				

				if (ScaledSpace.Instance && scaledSpaceCamera)
				{
					if (callCollector)
					{
						GC.Collect();
						callCollector=false;
					}


					if (!depthBufferSet)
					{
						if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
						{
							customDepthBuffer = (CustomDepthBufferCam)farCamera.gameObject.AddComponent (typeof(CustomDepthBufferCam));
							customDepthBuffer.inCamera = farCamera;
							customDepthBuffer.start();

							customDepthBufferTexture = new RenderTexture ( Screen.width,Screen.height,16, RenderTextureFormat.RFloat);
							customDepthBufferTexture.useMipMap=false;
							customDepthBufferTexture.filterMode = FilterMode.Point; // if this isn't in point filtering artifacts appear
							customDepthBufferTexture.Create ();
							
							if (useGodrays)
							{

								godrayDepthTexture = new RenderTexture (Screen.width,Screen.height,16, RenderTextureFormat.RFloat);
								godrayDepthTexture.filterMode = FilterMode.Point;
								godrayDepthTexture.useMipMap=false;
								customDepthBuffer._godrayDepthTex = godrayDepthTexture;
								godrayDepthTexture.Create ();
							}

							customDepthBuffer._depthTex = customDepthBufferTexture;

							//refraction stuff
							if (useOceanShaders && oceanRefraction)
							{
								refractionCam = (RefractionCamera) farCamera.gameObject.AddComponent (typeof(RefractionCamera));
								refractionCam.inCamera = farCamera;
								refractionCam.start();

								refractionTexture = new RenderTexture ( Screen.width,Screen.height,16, RenderTextureFormat.ARGB32);
								refractionTexture.useMipMap=false;
								refractionTexture.filterMode = FilterMode.Bilinear;
								refractionTexture.Create ();

								refractionCam._refractionTex = refractionTexture;
							}

						}
						depthBufferSet = true;
					}


					//custom lens flares
					if ((fullLensFlareReplacement) && !customSunFlareAdded)
					{
						foreach (string sunflareBody in sunflaresList)
						{
							SunFlare customSunFlare =(SunFlare) scaledSpaceCamera.gameObject.AddComponent(typeof(SunFlare));

							try
							{
								customSunFlare.source=CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == sunflareBody);
								customSunFlare.sourceName=sunflareBody;
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

					if (disableAmbientLight && !ambientLightScript)
					{
						ambientLightScript = (DisableAmbientLight) scaledSpaceCamera.gameObject.AddComponent (typeof(DisableAmbientLight));
					}

					if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
					{
						if (!customDepthBufferTexture.IsCreated ())
						{
							customDepthBufferTexture.Create ();
						}

						if (useOceanShaders && oceanRefraction)
						{
							if (!refractionTexture.IsCreated ())
							{
								refractionTexture.Create ();
							}
						}
					}

					pqsEnabled = false;
					underwater = false;
					
					foreach (ScattererCelestialBody _cur in scattererCelestialBodies)
					{
						float dist, shipDist=0f;
						if (_cur.hasTransform)
						{
							dist = Vector3.Distance (ScaledSpace.ScaledToLocalSpace( scaledSpaceCamera.transform.position),
													 ScaledSpace.ScaledToLocalSpace (_cur.transform.position));

							//don't unload planet the player ship is close to if panning away in map view
							if (MapView.MapIsEnabled && FlightGlobals.ActiveVessel)
								shipDist = Vector3.Distance (FlightGlobals.ActiveVessel.transform.position,
							                         ScaledSpace.ScaledToLocalSpace (_cur.transform.position));

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
											pqsEnabled = true;
										}

										if (_cur.m_manager.hasOcean && useOceanShaders && pqsEnabled)
										{
											underwater = _cur.m_manager.GetOceanNode().isUnderwater;
										}
									}
								}
							} 
							else
							{
							if (dist < _cur.loadDistance && _cur.transform && _cur.celestialBody) {
									_cur.m_manager = new Manager ();
									_cur.m_manager.setParentCelestialBody (_cur.celestialBody);
									_cur.m_manager.setParentPlanetTransform (_cur.transform);

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


									_cur.m_manager.hasOcean = _cur.hasOcean;
									_cur.m_manager.Awake ();
									_cur.active = true;


									GUItool.selectedConfigPoint = 0;
									GUItool.displayOceanSettings = false;
									GUItool.selectedPlanet = scattererCelestialBodies.IndexOf (_cur);
									GUItool.getSettingsFromSkynode ();
									if (_cur.hasOcean && useOceanShaders) {
										GUItool.getSettingsFromOceanNode ();
									}
									
									callCollector=true;
									Debug.Log ("[Scatterer] Effects loaded for " + _cur.celestialBodyName);
								}
							}
						}
					}

					//fixDrawOrders ();
					
					//if in mapView check that depth texture is clear for the sunflare shader
					if (customDepthBuffer)
					{
						if (!customDepthBuffer.depthTextureCleared && (MapView.MapIsEnabled || !pqsEnabled) )
							customDepthBuffer.clearDepthTexture();
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
//							Debug.Log("updating "+_aLight.source.name);
							_aLight.updateLight();

						}
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
//						ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
						cur.active = false;
					}
					
				}

				if (ambientLightScript)
				{
					ambientLightScript.restoreLight();
					Component.Destroy(ambientLightScript);
				}

				if(customDepthBuffer)
				{
					customDepthBuffer.OnDestroy();
					Component.Destroy (customDepthBuffer);
					UnityEngine.Object.Destroy (customDepthBuffer);
					customDepthBufferTexture.Release();
					UnityEngine.Object.Destroy (customDepthBufferTexture);
				}

				if(refractionCam)
				{
					refractionCam.OnDestroy();
					Component.Destroy (refractionCam);
					UnityEngine.Object.Destroy (refractionCam);
					refractionTexture.Release();
					UnityEngine.Object.Destroy (refractionTexture);
				}
				
				if(useGodrays)
				{
					if (godrayDepthTexture)
					{
						if (godrayDepthTexture.IsCreated())
							godrayDepthTexture.Release();
						UnityEngine.Object.Destroy (godrayDepthTexture);
					}
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

				}

				inGameWindowLocation=new Vector2(windowRect.x,windowRect.y);
				saveSettings();
			}
			
			else if (mainMenu)	
			{
				//replace EVE cloud shaders when leaving main menu to game
				if (integrateWithEVEClouds)
				{
					ShaderReplacer.Instance.replaceEVEshaders();
				}

				mainMenuWindowLocation=new Vector2(windowRect.x,windowRect.y);
				saveSettings();
			}


			UnityEngine.Object.Destroy (GUItool);
			
		}

		void OnGUI ()
		{
			if (visible)
			{
				windowRect = GUILayout.Window (windowId, windowRect, GUItool.DrawScattererWindow,"Scatterer v0.0310 preview: "
				                               + guiModifierKey1String+"/"+guiModifierKey2String +"+" +guiKey1String
				                               +"/"+guiKey2String+" toggle");

				//prevent window from going offscreen
				windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
				windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);
			}
		}
		
		public void loadSettings ()
		{
			//load scatterer config
			baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");
			if (baseConfigs.Length == 0)
			{
				Debug.Log ("[Scatterer] No config file found, check your install");
				return;
			}

			if (baseConfigs.Length > 1)
			{
				Debug.Log ("[Scatterer] Multiple config files detected, check your install");
			}

			ConfigNode.LoadObjectFromConfig (this, (baseConfigs [0]).config);


			guiKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey1String);
			guiKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey2String);

			guiModifierKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey1String);
			guiModifierKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey2String);

			//load planetsList, light sources list and sunflares list
			scattererPlanetsListReader.loadPlanetsList ();
			scattererCelestialBodies = scattererPlanetsListReader.scattererCelestialBodies;
			celestialLightSourcesData = scattererPlanetsListReader.celestialLightSourcesData;
			sunflaresList = scattererPlanetsListReader.sunflares;
			//mainSunCelestialBodyName = scattererPlanetsListReader.mainSunCelestialBodyName;

			//load atmo and ocean configs
			atmoConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_atmosphere");
			oceanConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_ocean");

			//load sunflare configs
			sunflareConfigs = GameDatabase.Instance.GetConfigNodes ("Scatterer_sunflare");
		}
		
		public void saveSettings ()
		{
			baseConfigs [0].config = ConfigNode.CreateConfigFromObject (this);
			baseConfigs [0].config.name = "Scatterer_config";
			Debug.Log ("[Scatterer] Saving settings to: " + baseConfigs [0].parent.url+".cfg");
			baseConfigs [0].parent.SaveConfigs ();
		}

		void removeStockOceans()
		{
			FakeOceanPQS[] fakes = (FakeOceanPQS[])FakeOceanPQS.FindObjectsOfType (typeof(FakeOceanPQS));
			
			if (fakes.Length == 0) { //if stock oceans haven't already been replaced
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
							if (pqs != null) {
								PQS ocean = pqs.ChildSpheres [0];
								if (ocean != null) {
									GameObject go = new GameObject ();
									FakeOceanPQS fakeOcean = go.AddComponent<FakeOceanPQS> ();
									fakeOcean.Apply (ocean);
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
			else
			{
				Debug.Log ("[Scatterer] Stock oceans already removed");
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
			if (terrainShadows)
			{
				foreach (CelestialBody _sc in CelestialBodies)
				{
					if (_sc.pqsController)
					{
						_sc.pqsController.meshCastShadows = true;
						_sc.pqsController.meshRecieveShadows = true;

//						Debug.Log("[Scatterer] PQS material of "+_sc.name+": "
//						          +_sc.pqsController.surfaceMaterial.shader.name);

						QualitySettings.shadowDistance = shadowsDistance;

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
							}
						}
					}
				}
			}
		}
	
		internal static Type getType(string name)
		{
			Type type = null;
			AssemblyLoader.loadedAssemblies.TypeOperation(t =>
			                                              
			                                              {
				if (t.FullName == name)
					type = t;
			}
			);
			
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
			//Type EVEType = getType("Utils.HalfSphere"); 


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

			object EVEinstance;

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

				object cloud2dObj = _obj.GetType().GetField("layer2D", flags).GetValue(_obj) as object;
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
	}
}
