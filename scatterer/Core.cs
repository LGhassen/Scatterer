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

//using Utils;

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

		planetsListReader scattererPlanetsListReader = new planetsListReader ();
		public List<SunFlare> customSunFlares = new List<SunFlare>();
		bool customSunFlareAdded=false;
		
		bool visible = false;

//		bool rtsResized=false;

		bool wireFrame=false;

		[Persistent]
		public bool disableAmbientLight=false;
		
//		[Persistent]
		public string mainSunCelestialBodyName="Sun";
		
		[Persistent]
		public bool integrateWithEVEClouds=false;

		disableAmbientLight ambientLightScript;
		
		List < scattererCelestialBody > scattererCelestialBodies = new List < scattererCelestialBody > {};

		public List<string> sunflaresList=new List<string> {};

		CelestialBody[] CelestialBodies;
		
		Light[] lights;

		//map EVE 2d cloud materials to planet names
		public Dictionary<String, List<Material> > EVEClouds = new Dictionary<String, List<Material> >();

		//map EVE CloudObjects to planet names
		//as far as I understand CloudObjects in EVE contain the 2d clouds and the volumetrics for a given
		//layer on a given planet, however due to the way they are handled in EVE they don't directly reference
		//their parent planet and the volumetrics are only created when the PQS is active
		//I map them here to facilitate accesing the volumetrics later
		public Dictionary<String, List<object>> EVECloudObjects = new Dictionary<String, List<object>>();

		GameObject sunLight,scaledspaceSunLight;
//		public GameObject copiedScaledSunLight, copiedScaledSunLight2;
//		public GameObject copiedSunLight;

		Cubemap planetShineCookieCubeMap;

		[Persistent]
		Vector2 mainMenuWindowLocation=Vector2.zero;

		[Persistent]
		Vector2 inGameWindowLocation=Vector2.zero;

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
		int scrollSectionHeight = 500;
		
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

		List<planetShineLightSource> celestialLightSourcesData=new List<planetShineLightSource> {};
		
		List<planetShineLight> celestialLightSources=new List<planetShineLight> {};

		UrlDir.UrlConfig[] baseConfigs;
		public UrlDir.UrlConfig[] atmoConfigs;
		public UrlDir.UrlConfig[] oceanConfigs;

		public ConfigNode[] sunflareConfigs;

		[Persistent]
		public bool
			terrainShadows = true;

		[Persistent]
		float shadowNormalBias=0.4f;
		
		[Persistent]
		float shadowBias=0.125f;
		
		[Persistent]
		public float
			shadowsDistance=100000;
		
		//[Persistent]
		float godrayResolution = 1f;

		[Persistent]
		string guiModifierKey1String=KeyCode.LeftAlt.ToString();

		[Persistent]
		string guiModifierKey2String=KeyCode.RightAlt.ToString();

		[Persistent]
		string guiKey1String=KeyCode.F10.ToString();
		
		[Persistent]
		string guiKey2String=KeyCode.F11.ToString();

		KeyCode guiKey1, guiKey2, guiModifierKey1, guiModifierKey2 ;

		private Vector2 _scroll;
		private Vector2 _scroll2;
		public bool pqsEnabled = false;
		bool displayOceanSettings = false;
		bool displaySunflareSettings = false;
		CustomDepthBufferCam customDepthBuffer;
		public RenderTexture customDepthBufferTexture;
		public RenderTexture godrayDepthTexture;
		
		bool depthBufferSet = false;

		float experimentalAtmoScale=1f;
		float viewdirOffset=0f;
		
		public CelestialBody sunCelestialBody;
		public CelestialBody munCelestialBody;
		public string path, gameDataPath;
		bool found = false;
		bool showInterpolatedValues = false;
		public bool stockSunglare = false;
		public bool extinctionEnabled = true;
		float rimBlend = 20f;
		float rimpower = 600f;
		float cloudColorMultiplier=1f;
		float cloudScatteringMultiplier=1f;
		float cloudSkyIrradianceMultiplier = 1f;
		float volumetricsColorMultiplier=1f;
//		float volumetricsScatteringMultiplier=1f;
//		float volumetricsSkyIrradianceMultiplier = 1f;
		float mieG = 0.85f;
		float openglThreshold = 10f;
		float _GlobalOceanAlpha = 1f;
		
		float edgeThreshold = 1f;

		float extinctionMultiplier = 1f;
		float extinctionTint = 1f;
		float skyExtinctionRimFade=0f;
		float skyExtinctionGroundFade=0f;
		
		float _extinctionScatterIntensity=1f;
		float _mapExtinctionScatterIntensity=1f;
		
		float mapExtinctionMultiplier = 1f;
		float mapExtinctionTint = 1f;
		float mapSkyExtinctionRimFade=1f;
		float specR = 0f, specG = 0f, specB = 0f, shininess = 0f;
		
		//configPoint variables 		
		float pointAltitude = 0f;
		float newCfgPtAlt = 0f;
		int configPointsCnt;
		int selectedConfigPoint = 0;
		int selectedPlanet = 0;
		Camera[] cams;
		public Camera farCamera, scaledSpaceCamera, nearCamera;

//		cameraHDRTonemapping tonemapper;
		
//		float MapViewScale = 1000f;
		
		[Persistent]
		public int oceanRenderQueue=2001;
		
		float postProcessingalpha = 78f;
		float postProcessDepth = 200f;
		
		float _Post_Extinction_Tint=100f;
		float postExtinctionMultiplier=100f;

		float postProcessExposure = 18f;
		
		//sky properties
		float exposure = 25f;
		float skyRimExposure = 25f;
		float alphaGlobal = 100f;
		float mapExposure = 15f;
		float mapSkyRimeExposure = 15f;
		float mapAlphaGlobal = 100f;

		public Camera chosenCamera;
		public int layer = 15;

		//ocean variables
		public bool stockOcean = false;
		
		float oceanLevel = 0f;
		float oceanAlpha = 1f;
		float oceanAlphaRadius = 3000f;
		float oceanScale = 1f;
		float WAVE_CM = 0.23f;
		float WAVE_KM = 370.0f;
		float AMP = 1.0f;
		float m_windSpeed = 5.0f; //A higher wind speed gives greater swell to the waves
		float m_omega = 0.84f; //A lower number means the waves last longer and will build up larger waves
		
		int m_ansio = 2;
		int m_varianceSize = 4;
		int m_foamAnsio = 9;
		float m_foamMipMapBias = -2.0f;
		float m_whiteCapStr = 0.1f;
		float farWhiteCapStr = 0.1f;
		float choppynessMultiplier = 1f;
		
		//		Vector3 m_oceanUpwellingColor = new Vector3 (0.039f, 0.156f, 0.47f);
		float oceanUpwellingColorR = 0.0039f;
		float oceanUpwellingColorG = 0.0156f;
		float oceanUpwellingColorB = 0.047f;
		
		int m_resolution = 4;
		//		int MAX_VERTS = 65000;
		
		Vector4 m_gridSizes = new Vector4 (5488, 392, 28, 2); //Size in meters (i.e. in spatial domain) of each grid
		Vector4 m_choppyness = new Vector4 (2.3f, 2.1f, 1.3f, 0.9f); //strengh of sideways displacement for each grid

		[Persistent]
		public int m_fourierGridSize = 128; //This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.


		
		//other stuff
		float atmosphereGlobalScale = 1000f;
		
		public bool depthbufferEnabled = false;
		public bool d3d9 = false;
		public bool opengl = false;
		public bool d3d11 = false;
		bool isActive = false;
		bool mainMenu=false;
		
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
					cams = Camera.allCameras;
					for (int i = 0; i < cams.Length; i++)
					{

//						cams [i].hdr=true;


						if (cams [i].name == "Camera ScaledSpace")
						{
							//cams [i].renderingPath=RenderingPath.DeferredShading;
							scaledSpaceCamera = cams [i];
						}
						
						if (cams [i].name == "Camera 01")
						{
							//cams [i].renderingPath=RenderingPath.DeferredShading;
							farCamera = cams [i];
							//cams [i].enabled=false;

						}
						
						if (cams [i].name == "Camera 00")
						{
							//cams [i].renderingPath=RenderingPath.DeferredShading;
							nearCamera = cams [i];
							//cams [i].farClipPlane=700000;
							//cams [i].enabled=false;
//							tonemapper = (cameraHDRTonemapping)nearCamera.gameObject.AddComponent (typeof(cameraHDRTonemapping));
						}

					}
					

					
					//find sunlight
					lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
//					Debug.Log ("number of lights" + lights.Length);
					foreach (Light _light in lights)
					{
//						Debug.Log("name:"+_light.gameObject.name);
//						Debug.Log("intensity:"+_light.intensity.ToString());
//						Debug.Log ("mask:"+_light.cullingMask.ToString());
//						Debug.Log ("type:"+_light.type.ToString());
//						Debug.Log ("Parent:"+_light.transform.parent.gameObject.name);
//						Debug.Log ("range:"+_light.range.ToString());
//
//						Debug.Log ("shadows:"+_light.shadows.ToString ());
//						Debug.Log ("shadowStrength:"+_light.shadowStrength.ToString ());
//						Debug.Log ("shadowNormalBias:"+_light.shadowNormalBias.ToString ());
//						Debug.Log ("shadowBias:"+_light.shadowBias.ToString ());
						
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



//					copiedScaledSunLight=(UnityEngine.GameObject) Instantiate(scaledspaceSunLight);
//					copiedScaledSunLight2=(UnityEngine.GameObject) Instantiate(scaledspaceSunLight);

//					copiedScaledSunLight=(UnityEngine.GameObject) Instantiate(sunLight);

//					Debug.Log ("copied scaledSpaceSunlight");

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

//							ringObject.GetComponent < MeshRenderer > ().material.shader=
//													ShaderReplacer.Instance.LoadedShaders["Scatterer/Rings"];
//
//							Debug.Log("[Scatterer] Replaced ring shader for "+_cb.name);
						}
					}

					//set up planetshine lights
					if(usePlanetShine)
					{
						foreach (planetShineLightSource _aSource in celestialLightSourcesData)
						{
							var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == _aSource.bodyName);
							if (celBody)
							{
								planetShineLight aPsLight= new planetShineLight();
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
						if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
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
								godrayDepthTexture = new RenderTexture ((int)(Screen.width*godrayResolution),(int)(Screen.height*godrayResolution),16, RenderTextureFormat.RFloat);
								
								godrayDepthTexture.filterMode = FilterMode.Bilinear;
								godrayDepthTexture.useMipMap=false;
								customDepthBuffer._godrayDepthTex = godrayDepthTexture;
								godrayDepthTexture.Create ();
							}
							
							customDepthBuffer._depthTex = customDepthBufferTexture;
						}
						depthBufferSet = true;
					}


					//custom lens flares
					if ((fullLensFlareReplacement) && !customSunFlareAdded)
					{
						//dir sunflare directory
						//string sunFlarePath=path + "/sunflare";
						foreach (string sunflareBody in sunflaresList)
						{
							try
							{
								SunFlare customSunFlare =(SunFlare) scaledSpaceCamera.gameObject.AddComponent(typeof(SunFlare));
								customSunFlare.source=CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == sunflareBody);
								customSunFlare.sourceName=sunflareBody;
								customSunFlare.start ();
								customSunFlares.Add(customSunFlare);
							}
							catch (Exception stupid)
							{
								Debug.Log("[Scatterer] Custom sunflare cannot be added to "+sunflareBody+" "+stupid.ToString());
								continue;
							}
						}

						customSunFlareAdded=true;
					}

					if (disableAmbientLight && !ambientLightScript)
					{
						ambientLightScript = (disableAmbientLight) scaledSpaceCamera.gameObject.AddComponent (typeof(disableAmbientLight));
					}

					if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
					{
						if (!customDepthBufferTexture.IsCreated ())
						{
							customDepthBufferTexture.Create ();
						}
					}

					pqsEnabled = false;
					
					foreach (scattererCelestialBody _cur in scattererCelestialBodies)
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
//								if (dist > _cur.unloadDistance && !MapView.MapIsEnabled) {
								if (dist > _cur.unloadDistance && shipDist > _cur.unloadDistance) {
									_cur.m_manager.OnDestroy ();
									UnityEngine.Object.Destroy (_cur.m_manager);
									_cur.m_manager = null;
									//ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
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
									}
								}
							} 
							else
							{
//								if (dist < _cur.loadDistance && !MapView.MapIsEnabled && _cur.transform && _cur.celestialBody) {
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
//												if (cc.name == "Mun")
//												if (cc.name == "Jool")
//												{
//													munCelestialBody=cc;
//
//													copiedScaledSunLight.GetComponent<Light>().type=LightType.Point;
//													copiedScaledSunLight.GetComponent<Light>().cookie=planetShineCookieCubeMap;
//													copiedScaledSunLight.GetComponent<Light>().range=1E9f;
//													copiedScaledSunLight.GetComponent<Light>().color=new Color(0.6f,1f,0.4f);
//
//													copiedScaledSunLight2.GetComponent<Light>().type=LightType.Point;
//													copiedScaledSunLight2.GetComponent<Light>().cookie=planetShineCookieCubeMap;
//													copiedScaledSunLight2.GetComponent<Light>().range=1E9f;
//													copiedScaledSunLight2.GetComponent<Light>().color=new Color(0.6f,1f,0.4f);
//													copiedScaledSunLight2.GetComponent<Light>().cullingMask=557591;
//												}
											}
										}
										_cur.m_manager.eclipseCasters=eclipseCasters;
									}

									List<atmoPlanetShineSource> planetshineSources=new List<atmoPlanetShineSource> {};

									if (usePlanetShine)
									{								
										for (int k=0; k < _cur.planetshineSources.Count; k++)
										{
											var cc = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _cur.planetshineSources[k].bodyName);
											if (cc==null)
												Debug.Log("[Scatterer] planetshine source "+_cur.planetshineSources[k].bodyName+" not found for "+_cur.celestialBodyName);
											else
											{
												atmoPlanetShineSource src=_cur.planetshineSources[k];
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


									selectedConfigPoint = 0;
									displayOceanSettings = false;
									selectedPlanet = scattererCelestialBodies.IndexOf (_cur);
									getSettingsFromSkynode ();
									if (_cur.hasOcean && useOceanShaders) {
										getSettingsFromOceanNode ();
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
						foreach (planetShineLight _aLight in celestialLightSources)
						{
//							Debug.Log("updating "+_aLight.source.name);
							_aLight.updateLight();

						}
					}

//					Debug.Log("near cam renpath "+nearCamera.actualRenderingPath.ToString());
//					//Debug.Log("far cam renpath "+farCamera.actualRenderingPath.ToString());
//					Debug.Log("scaled cam renpath "+scaledSpaceCamera.actualRenderingPath.ToString());


//					if (munCelestialBody)
//					{
//						copiedScaledSunLight.gameObject.transform.position=ScaledSpace.LocalToScaledSpace(munCelestialBody.transform.position);
//						copiedScaledSunLight.gameObject.transform.LookAt(ScaledSpace.LocalToScaledSpace(sunCelestialBody.transform.position));
//
//						copiedScaledSunLight2.gameObject.transform.position=(munCelestialBody.transform.position);
//						copiedScaledSunLight2.gameObject.transform.LookAt(sunCelestialBody.transform.position);
//					}


					//					GameObject[] list = (GameObject[]) GameObject.FindObjectsOfType(typeof(GameObject));
					//					int d=0;
					//					foreach (GameObject _go in list)
					//					{
					//						Debug.Log("object i "+d.ToString()+" "+_go.name);
					//						if (_go.transform.parent)
					//							Debug.Log("object i parent"+d.ToString()+" "+_go.transform.parent.gameObject.name);
					//						MeshRenderer[] list2 = (MeshRenderer[]) _go.GetComponentsInChildren<MeshRenderer>();
					//						foreach (MeshRenderer _mrr in list2)
					//						{
					//							Debug.Log("meshrenderer"+ _mrr.name);
					//							Debug.Log("meshrenderer mat"+ _mrr.material.name);
					//						}
					//						d++;
					//					}
					//					Material[] list = (Material[]) Material.FindObjectsOfType(typeof(Material));
					//					int d=0;
					//					foreach (Material _mtl in list)
					//					{
					//						Debug.Log("material i "+d.ToString()+" "+ _mtl.name);
					//						d++;
					//					}
					//					
					//					MeshRenderer[] list2 = (MeshRenderer[]) MeshRenderer.FindObjectsOfType(typeof(MeshRenderer));
					//					d=0;
					//					foreach (MeshRenderer _mrr in list2)
					//					{
					//						Debug.Log("meshrenderer i "+d.ToString()+" "+ _mrr.name);
					//						d++;
					//					}
					
					//					GameObject kerbinClouds = GameObject.Find("Kerbin-clouds1");
					//					if (kerbinClouds)
					//					{
					//						Debug.Log("Kerbin clouds found");
					//						MeshRenderer[] list = kerbinClouds.GetComponentsInChildren<MeshRenderer>();
					//						int d=0;
					//						foreach (MeshRenderer _mrr in list)
					//						{
					//							Debug.Log("Meshrenderer "+d.ToString()+" "+ _mrr.name);
					////							Debug.Log("Meshrenderer "+d.ToString()+" "+ .GetType().ToString ());
					//							d++;
					//						}
					//					}
					////					Resources.FindObjectsOfTypeAll(typeof(HalfSphere));
					//					HalfSphere[] halfs =  Resources.FindObjectsOfTypeAll(typeof(HalfSphere));  ;
					////					UnityEngine.Object[] halfs =  FindObjectsOfType(typeof(HalfSphere));
					//					int d=0;
					////					foreach (HalfSphere _hlf in halfs)
					//					foreach (UnityEngine.Object _hlf in halfs)
					//					{
					//						HalfSphere _hlff = (HalfSphere) _hlf;
					//						Debug.Log("_hlf "+d.ToString()+" GameObject.name "+_hlf.GameObject.name);
					//						if (_hlf.GameObject.GetComponentInChildren<MeshRenderer>())
					//						{
					//							MeshRenderer idekk =_hlf.GameObject.GetComponentInChildren<MeshRenderer>();
					//							Debug.Log("_hlf "+d.ToString()+" mat.name "+idekk.material.name);
					//						}
					//						d++;
					//
					//					}


//					RenderTexture[] RenderTextureList = Resources.FindObjectsOfTypeAll<RenderTexture> ();
//					Debug.Log ("Start rt list");
//					for (int i=0;i<RenderTextureList.Length;i++)
//					{
//						RenderTexture _rt= RenderTextureList[i];
//						Debug.Log(_rt.name.ToString()+" "+_rt.width.ToString()+" "+ _rt.height.ToString()+" "+_rt.depth.ToString()+" "+_rt.format.ToString()+" ");
////						if (_rt.name == "ImageEffects Temp" && !rtsResized)
////						{
////							_rt = new RenderTexture((int)Screen.width/2,(int)Screen.height/2,_rt.depth, _rt.format);
////							_rt.filterMode=FilterMode.Trilinear;
////							_rt.Create();
////						}
//					}
//					Debug.Log ("End rt list");
////
////					Debug.Log(scaledSpaceCamera.);
////
////					rtsResized=true;
//
//					for (int i = 0; i < cams.Length; i++)
//					{
//					
////						cams [i].hdr
//						
//						Debug.Log(cams[i].name+" "+cams [i].hdr.ToString ());
//					}

//					PQSMod_MeshScatter[] scatters = (PQSMod_MeshScatter[]) PQSMod_MeshScatter.FindObjectsOfType<PQSMod_MeshScatter>();
//
//
//					Debug.Log("begin scatters list");
//					for (int i=0;i<scatters.Length;i++)
//					{
//						PQSMod_MeshScatter _sct = scatters[i];
//						Debug.Log(_sct.name+" "+_sct.scatterName+" "+_sct.enabled.ToString()+" "+_sct.isActiveAndEnabled.ToString()+" "+_sct.maxScatter.ToString()+" "+_sct.sphere.name);
//					}
//
//					PQSMod_MeshScatter_QuadControl[] scatters2 = (PQSMod_MeshScatter_QuadControl[]) PQSMod_MeshScatter_QuadControl.FindObjectsOfType<PQSMod_MeshScatter_QuadControl>();
//
//					Debug.Log(scatters2.Length);
//					PQSMod_LandClassScatterQuad[] scatters3 = (PQSMod_LandClassScatterQuad[]) PQSMod_LandClassScatterQuad.FindObjectsOfType<PQSMod_LandClassScatterQuad>();
//					Debug.Log(scatters3.Length);

//					PQSMod_LandClassScatterQuad[] scatters = (PQSMod_LandClassScatterQuad[]) PQSMod_LandClassScatterQuad.FindObjectsOfType<PQSMod_LandClassScatterQuad>();
//
//					Debug.Log("begin scatters list");
//
//					for (int i=0;i<scatters.Length;i++)
//					{
//						PQSMod_LandClassScatterQuad _sct = scatters[i];
//						Debug.Log(_sct.scatter.scatterName);
//					}

				}
			} 
		}
		

		void OnDestroy ()
		{
			if (isActive) {

//				if (copiedScaledSunLight)
//				{
//					UnityEngine.Object.Destroy(copiedScaledSunLight);
//					UnityEngine.Object.Destroy(copiedScaledSunLight2);
//				}

				if(usePlanetShine)
				{
					foreach (planetShineLight _aLight in celestialLightSources)
					{
						_aLight.OnDestroy();
						UnityEngine.Object.Destroy(_aLight);
					}
				}

				for (int i = 0; i < scattererCelestialBodies.Count; i++) {
					
					scattererCelestialBody cur = scattererCelestialBodies [i];
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
				
//				if (tonemapper)
//					Component.Destroy(tonemapper);
				
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
			
		}

		


		void OnGUI ()
		{
			if (visible)
			{
				windowRect = GUILayout.Window (windowId, windowRect, DrawScattererWindow,"Scatterer v0.0275 preview: "
				                               + guiModifierKey1String+"/"+guiModifierKey2String +"+" +guiKey1String
				                               +"/"+guiKey2String+" toggle");

				//prevent window from going offscreen
				windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
				windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);

//				RenderTexture[] RenderTextureList = Resources.FindObjectsOfTypeAll<RenderTexture> ();
////				Debug.Log ("Start rt list");
//				int j=0;
//				for (int i=0;i<RenderTextureList.Length;i++)
//				{
//					RenderTexture _rt= RenderTextureList[i];
//					if (_rt.name == "ImageEffects Temp")
//					{
//						GUI.DrawTexture(new Rect((float)256*j,0.0f,144f,256f),_rt, ScaleMode.ScaleToFit, false);
//						j++;
//					}
//				}
			}
		}

		//		UI BUTTONS
		//		This isn't the most elegant section due to how many elements are here
		//		I don't care enough to do it in a cleaner way
		//		After all it's a basic UI for tweaking settings and it does it's job
		void DrawScattererWindow (int windowId)
		{
			{
				GUItoggle("Hide",ref visible);
								
				
				if (mainMenu)  //MAIN MENU options
				{ 
					GUILayout.Label (String.Format ("Scatterer: features selector"));
					useOceanShaders = GUILayout.Toggle(useOceanShaders, "Ocean shaders (may require game restart on change)");

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Ocean: fourierGridSize (64:fast,128:normal,256:HQ)");
					m_fourierGridSize = (Int32)(Convert.ToInt32 (GUILayout.TextField (m_fourierGridSize.ToString ())));
					GUILayout.EndHorizontal ();

					oceanSkyReflections = GUILayout.Toggle(oceanSkyReflections, "Ocean: accurate sky reflection");
					oceanPixelLights = GUILayout.Toggle(oceanPixelLights, "Ocean: lights compatibility (huge performance hit when lights on)");

//					usePlanetShine = GUILayout.Toggle(usePlanetShine, "PlanetShine");
					integrateWithEVEClouds = GUILayout.Toggle(integrateWithEVEClouds, "Integrate effects with EVE clouds (may require restart)");

					drawAtmoOnTopOfClouds= GUILayout.Toggle(drawAtmoOnTopOfClouds, "Draw atmo on top of EVE clouds(old cloud shading, use with EVE 7-4)");

					fullLensFlareReplacement=GUILayout.Toggle(fullLensFlareReplacement, "Lens flare shader");
					useEclipses = GUILayout.Toggle(useEclipses, "Eclipses (WIP, sky/orbit only for now)");
					useRingShadows = GUILayout.Toggle(useRingShadows, "Kopernicus ring shadows");
					useGodrays = GUILayout.Toggle(useGodrays, "Godrays (early WIP)");
					
//					GUILayout.BeginHorizontal ();
//					GUILayout.Label ("Godray resolution scale");
//					godrayResolution = float.Parse (GUILayout.TextField (godrayResolution.ToString ("0.000")));
//					GUILayout.EndHorizontal ();
					
					terrainShadows = GUILayout.Toggle(terrainShadows, "Terrain shadows");
					GUILayout.BeginHorizontal ();

					GUILayout.Label ("Shadow bias");
					shadowBias = float.Parse (GUILayout.TextField (shadowBias.ToString ("0.000")));

					GUILayout.Label ("Shadow normal bias");
					shadowNormalBias = float.Parse (GUILayout.TextField (shadowNormalBias.ToString ("0.000")));

					GUILayout.EndHorizontal ();


					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Menu scroll section height");
					scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (scrollSectionHeight.ToString ())));
					GUILayout.EndHorizontal ();

					disableAmbientLight = GUILayout.Toggle(disableAmbientLight, "Disable scaled space ambient light");

					useAlternateShaderSQRT = GUILayout.Toggle(useAlternateShaderSQRT, "Alternate SQRT in shader (potential fix for halo around planets)");

					showMenuOnStart = GUILayout.Toggle(showMenuOnStart, "Show this menu on start-up");

					GUILayout.BeginHorizontal ();
					GUILayout.Label (".cfg file used:");
					GUILayout.TextField(baseConfigs [0].parent.url);
					GUILayout.EndHorizontal ();
				}
				
				
				
				else if (isActive)
				{
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Planet:");
					
					if (GUILayout.Button ("<")) {
						if (selectedPlanet > 0) {
							selectedPlanet -= 1;
							selectedConfigPoint = 0;
							if (scattererCelestialBodies [selectedPlanet].active) {
								loadConfigPoint (selectedConfigPoint);
								getSettingsFromSkynode ();
								if (scattererCelestialBodies [selectedPlanet].hasOcean)
									getSettingsFromOceanNode ();
							}
						}
					}
					
					GUILayout.TextField ((scattererCelestialBodies [selectedPlanet].celestialBodyName).ToString ());
					
					if (GUILayout.Button (">")) {
						if (selectedPlanet < scattererCelestialBodies.Count - 1) {
							selectedPlanet += 1;
							selectedConfigPoint = 0;
							if (scattererCelestialBodies [selectedPlanet].active) {
								loadConfigPoint (selectedConfigPoint);
								getSettingsFromSkynode ();
								if (scattererCelestialBodies [selectedPlanet].hasOcean)
									getSettingsFromOceanNode ();
							}
						}
					}
					GUILayout.EndHorizontal ();
					
					
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Planet loaded:" + scattererCelestialBodies [selectedPlanet].active.ToString ()+
					                 "                                Has ocean:" + scattererCelestialBodies [selectedPlanet].hasOcean.ToString ());
					GUILayout.EndHorizontal ();
					
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Load distance:" + scattererCelestialBodies [selectedPlanet].loadDistance.ToString ()+
					                 "                             Unload distance:" + scattererCelestialBodies [selectedPlanet].unloadDistance.ToString ());
					GUILayout.EndHorizontal ();
					
					
					
					
					if (scattererCelestialBodies [selectedPlanet].active) {
						configPointsCnt = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
						
						
						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button ("Atmosphere settings")) {
							displayOceanSettings = false;
							displaySunflareSettings = false;
						}
						
						if (GUILayout.Button ("Ocean settings")) {
							if (scattererCelestialBodies [selectedPlanet].hasOcean)
								displayOceanSettings = true;

							displaySunflareSettings = false;
						}

//						if (GUILayout.Button ("Sunflare(s) Settings")) {
//							displayOceanSettings = false;
//							displaySunflareSettings = true;
//						}
						
						GUILayout.EndHorizontal ();

						configPoint _cur = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint];


						if (!displayOceanSettings)
						{
							if (!MapView.MapIsEnabled)
							{
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("New point altitude:");
								newCfgPtAlt = Convert.ToSingle (GUILayout.TextField (newCfgPtAlt.ToString ()));
								if (GUILayout.Button ("Add"))
								{
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Insert (selectedConfigPoint + 1,
									                                                                                   new configPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100, skyRimExposure/100,
									                 postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100,
									                 extinctionMultiplier / 100, extinctionTint / 100, skyExtinctionRimFade/100, skyExtinctionGroundFade/100,
									                 openglThreshold, edgeThreshold / 100,viewdirOffset,_Post_Extinction_Tint/100,
									                 postExtinctionMultiplier/100, _GlobalOceanAlpha/100, _extinctionScatterIntensity/100));
									selectedConfigPoint += 1;
									configPointsCnt = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
									loadConfigPoint (selectedConfigPoint);
								}
								GUILayout.EndHorizontal ();
								
								
								
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Config point:");
								
								if (GUILayout.Button ("<")) {
									if (selectedConfigPoint > 0) {
										selectedConfigPoint -= 1;
										loadConfigPoint (selectedConfigPoint);
									}
								}
								
								GUILayout.TextField ((selectedConfigPoint).ToString ());
								
								if (GUILayout.Button (">")) {
									if (selectedConfigPoint < configPointsCnt - 1) {
										selectedConfigPoint += 1;
										loadConfigPoint (selectedConfigPoint);
									}
								}
								
								//GUILayout.Label (String.Format("Total:{0}", configPointsCnt));
								if (GUILayout.Button ("Delete")) {
									if (configPointsCnt <= 1)
										print ("Can't delete config point, one or no points remaining");
									else
									{
										scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.RemoveAt (selectedConfigPoint);
										if (selectedConfigPoint >= configPointsCnt - 1)
										{
											selectedConfigPoint = configPointsCnt - 2;
										}
										configPointsCnt = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
										loadConfigPoint (selectedConfigPoint);
									}
									
								}
								
								GUILayout.EndHorizontal ();

								
								GUIfloat("Point altitude", ref pointAltitude, ref _cur.altitude);
								
								
								_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (scrollSectionHeight));
								
								
								GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
								GUIfloat("AtmosphereGlobalScale", ref atmosphereGlobalScale, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);
								GUIfloat("experimentalViewDirOffset", ref viewdirOffset,ref _cur.viewdirOffset);
								
								
//								GUILayout.Label("Sky/Orbit shader");

								GUIfloat("Sky/orbit Alpha", ref alphaGlobal, ref _cur.skyAlpha);
								GUIfloat("Sky/orbit Exposure", ref exposure, ref _cur.skyExposure);
//								tonemapper.setExposure(exposure);
								GUIfloat ("Sky/orbit Rim Exposure", ref skyRimExposure, ref _cur.skyRimExposure);
								
								GUIfloat("extinctionMultiplier", ref extinctionMultiplier, ref _cur.skyExtinctionMultiplier);
								GUIfloat("extinctionTint", ref extinctionTint, ref _cur.skyExtinctionTint);
								GUIfloat("extinctionRimFade", ref skyExtinctionRimFade ,ref  _cur.skyextinctionRimFade);
								GUIfloat("extinctionGroundFade", ref skyExtinctionGroundFade, ref _cur.skyextinctionGroundFade);
								GUIfloat("extinctionScatterIntensity", ref _extinctionScatterIntensity, ref _cur._extinctionScatterIntensity);
								
//								GUILayout.Label("Post-processing shader");
								
								GUIfloat("Post Processing Alpha", ref postProcessingalpha, ref _cur.postProcessAlpha);
								GUIfloat("Post Processing Depth", ref postProcessDepth,ref _cur.postProcessDepth);
								GUIfloat("Post Processing Extinction Multiplier", ref postExtinctionMultiplier, ref _cur.postExtinctionMultiplier);
								GUIfloat("Post Processing Extinction Tint", ref _Post_Extinction_Tint, ref _cur._Post_Extinction_Tint);
								GUIfloat("Post Processing Exposure", ref postProcessExposure ,ref _cur.postProcessExposure);
									
//								if (!d3d9)
									{
										GUIfloat("Depth buffer Threshold", ref openglThreshold, ref _cur.openglThreshold);
									}
									
								GUIfloat("_GlobalOceanAlpha", ref _GlobalOceanAlpha, ref _cur._GlobalOceanAlpha);
								

							} 

							else
							{
								_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (scrollSectionHeight));

								GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
								GUIfloat("AtmosphereGlobalScale", ref atmosphereGlobalScale, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);

								GUIfloat("Map view alpha", ref mapAlphaGlobal, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapAlphaGlobal);
								GUIfloat("Map view exposure", ref mapExposure, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExposure);
								GUIfloat("Map view rim exposure", ref mapSkyRimeExposure, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyRimExposure);

								GUIfloat("MapExtinctionMultiplier", ref mapExtinctionMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionMultiplier);
								GUIfloat("MapExtinctionTint", ref mapExtinctionTint, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionTint);
								GUIfloat("MapExtinctionRimFade", ref mapSkyExtinctionRimFade, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyExtinctionRimFade);
								GUIfloat("MapExtinctionScatterIntensity", ref _mapExtinctionScatterIntensity, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode._mapExtinctionScatterIntensity);
							}


							GUIfloat("mieG", ref mieG, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_mieG);
								
							if (integrateWithEVEClouds)
							{
								GUIfloat("Cloud Color Multiplier", ref cloudColorMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudColorMultiplier);
								GUIfloat("Cloud Scattering Multiplier", ref cloudScatteringMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudScatteringMultiplier);
								GUIfloat("Cloud Sky irradiance Multiplier", ref cloudSkyIrradianceMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudSkyIrradianceMultiplier);
								
								GUIfloat("Volumetrics Color Multiplier", ref volumetricsColorMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsColorMultiplier);
//								GUIfloat("Volumetrics Scattering Multiplier", ref volumetricsScatteringMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsScatteringMultiplier);
//								GUIfloat("Volumetrics Sky irradiance Multiplier", ref volumetricsSkyIrradianceMultiplier, ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsSkyIrradianceMultiplier);
							}

							GUILayout.BeginHorizontal ();
							GUILayout.Label ("RimBlend");
							rimBlend = Convert.ToSingle (GUILayout.TextField (rimBlend.ToString ()));
							
							GUILayout.Label ("RimPower");
							rimpower = Convert.ToSingle (GUILayout.TextField (rimpower.ToString ()));
							
							if (GUILayout.Button ("Set")) {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
							}
							GUILayout.EndHorizontal ();
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Spec: R");
							specR = (float)(Convert.ToDouble (GUILayout.TextField (specR.ToString ())));
							
							GUILayout.Label ("G");
							specG = (float)(Convert.ToDouble (GUILayout.TextField (specG.ToString ())));
							
							GUILayout.Label ("B");
							specB = (float)(Convert.ToDouble (GUILayout.TextField (specB.ToString ())));
							
							GUILayout.Label ("shine");
							shininess = (float)(Convert.ToDouble (GUILayout.TextField (shininess.ToString ())));
							
							if (GUILayout.Button ("Set")) {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR = specR;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG = specG;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB = specB;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess = shininess;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
							}
							GUILayout.EndHorizontal ();
							
//								if (showInterpolatedValues)
							if (!MapView.MapIsEnabled)
							{
								GUILayout.BeginHorizontal ();
								if (scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0)
									GUILayout.Label ("Current state:Ground, cfgPoint 0");
								else if (scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt)
									GUILayout.Label (String.Format ("Current state:Orbit, cfgPoint{0}", scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1));
								else
									GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage)), scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage), scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
								GUILayout.EndHorizontal ();
							}
							GUILayout.EndScrollView ();
							

							GUILayout.BeginHorizontal ();

							GUItoggle("Toggle depth buffer", ref depthbufferEnabled);

							if (GUILayout.Button ("Toggle PostProcessing"))
							{
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePostProcessing();
							}

							GUILayout.EndHorizontal ();

//							GUILayout.BeginHorizontal ();
//							if (GUILayout.Button ("toggle sky"))
//							{
//								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled = !scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled;
//								if (scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled)
//									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere();
//								else
//									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.RestoreStockAtmosphere();
//							}
//							
//							GUILayout.EndHorizontal ();

						
							GUILayout.BeginHorizontal ();
							if (GUILayout.Button ("Save atmo"))
							{
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.displayInterpolatedVariables = showInterpolatedValues;
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.saveToConfigNode ();
							}
							
							if (GUILayout.Button ("Load atmo"))
							{
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.loadFromConfigNode ();
								getSettingsFromSkynode ();
								loadConfigPoint (selectedConfigPoint);
							}

							GUILayout.EndHorizontal ();

							GUILayout.BeginHorizontal ();
							GUILayout.Label (".cfg file used:");
							GUILayout.TextField(scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configUrl.parent.url);
							GUILayout.EndHorizontal ();

							GUILayout.BeginHorizontal ();
							if (GUILayout.Button ("Map EVE clouds"))
							{
								mapEVEClouds();
								foreach (scattererCelestialBody _cel in scattererCelestialBodies)
								{
									if (_cel.active)
									{
										if (!_cel.m_manager.m_skyNode.inScaledSpace)
											_cel.m_manager.m_skyNode.mapEVEvolumetrics();
									}
								}
							}
							GUILayout.EndHorizontal ();
						}
						else
						{

							OceanWhiteCaps oceanNode = scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();

							GUItoggle("Toggle ocean", ref stockOcean);

							_scroll2 = GUILayout.BeginScrollView (_scroll2, false, true, GUILayout.Width (400), GUILayout.Height (scrollSectionHeight+100));
							{
								
								GUIfloat ("ocean Level", ref oceanLevel, ref oceanNode.m_oceanLevel);
								GUIfloat ("Alpha/WhiteCap Radius", ref oceanAlphaRadius, ref oceanNode.alphaRadius);
								GUIfloat ("ocean Alpha", ref oceanAlpha, ref oceanNode.oceanAlpha);
								GUIfloat ("whiteCapStr (foam)", ref m_whiteCapStr, ref oceanNode.m_whiteCapStr);
								GUIfloat ("far whiteCapStr", ref farWhiteCapStr, ref oceanNode.m_farWhiteCapStr);
								GUIfloat ("choppyness multiplier", ref choppynessMultiplier, ref oceanNode.choppynessMultiplier);

								GUIvector3 ("Ocean Upwelling Color", ref oceanUpwellingColorR, ref oceanUpwellingColorG, ref oceanUpwellingColorB, ref oceanNode.m_oceanUpwellingColor);

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("To apply the next setting press \"rebuild ocean\" and wait");
								GUILayout.EndHorizontal ();
								
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Keep in mind this saves your current settings");
								GUILayout.EndHorizontal ();

//						        GUIfloat("ocean Scale", ref oceanScale, ref oceanNode.oceanScale);
//								GUIfloat ("WAVE_CM (default 0.23)", ref WAVE_CM, ref oceanNode.WAVE_CM);
//								GUIfloat ("WAVE_KM (default 370)", ref WAVE_KM, ref oceanNode.WAVE_KM);
								GUIfloat ("AMP (default 1)", ref AMP, ref oceanNode.AMP);
								
								GUIfloat ("wind Speed", ref m_windSpeed, ref oceanNode.m_windSpeed);
								GUIfloat ("omega: inverse wave age", ref m_omega, ref oceanNode.m_omega);
								
								GUIfloat ("foamMipMapBias", ref m_foamMipMapBias, ref oceanNode.m_foamMipMapBias);
								
								
								GUIint ("m_ansio", ref m_ansio, ref oceanNode.m_ansio, 1);
								GUIint ("m_foamAnsio", ref m_foamAnsio, ref oceanNode.m_foamAnsio, 1);
								
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Performance settings");
								GUILayout.EndHorizontal ();
								
								GUIint ("m_varianceSize (sun reflection, power of 2)", ref m_varianceSize, ref oceanNode.m_varianceSize, 1);
								
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("m_varianceSize increases rebuild time exponentially");
								GUILayout.EndHorizontal ();
								
								GUIint ("Ocean mesh resolution (lower is better)", ref m_resolution, ref oceanNode.m_resolution, 1);

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("current fourierGridSize: "+m_fourierGridSize.ToString());
								GUILayout.EndHorizontal ();

								GUIint("Ocean renderqueue", ref oceanRenderQueue, ref oceanRenderQueue,1);
	
							}	
							GUILayout.EndScrollView ();
							
							GUILayout.BeginHorizontal ();
							if (GUILayout.Button ("Rebuild ocean")) {
								scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
								scattererCelestialBodies [selectedPlanet].m_manager.reBuildOcean ();
							}
							GUILayout.EndHorizontal ();
							
							GUILayout.BeginHorizontal ();
							

							if (GUILayout.Button ("Save ocean")) {
								scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
							}
							
							if (GUILayout.Button ("Load ocean")) {
								scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().loadFromConfigNode ();
								getSettingsFromOceanNode ();
							}
							GUILayout.EndHorizontal ();

							GUILayout.BeginHorizontal ();
							GUILayout.Label (".cfg file used:");
							GUILayout.TextField(scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode().configUrl.parent.url);
							GUILayout.EndHorizontal ();
						}

						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button ("Toggle WireFrame"))
						{
							if (wireFrame)
							{
								if (nearCamera.gameObject.GetComponent (typeof(Wireframe)))
									Component.Destroy(nearCamera.gameObject.GetComponent (typeof(Wireframe)));
								
								if (farCamera.gameObject.GetComponent (typeof(Wireframe)))
									Component.Destroy(farCamera.gameObject.GetComponent (typeof(Wireframe)));
								
								if (scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
									Component.Destroy(scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
								
								wireFrame=false;
							}
							
							else
							{
								nearCamera.gameObject.AddComponent (typeof(Wireframe));
								farCamera.gameObject.AddComponent (typeof(Wireframe));
								scaledSpaceCamera.gameObject.AddComponent (typeof(Wireframe));
								
								wireFrame=true;
							}
						}

//						GUILayout.BeginHorizontal ();
//						if (GUILayout.Button ("Toggle AmbientLight"))
//						{
//							if (ambientLight)
//							{
//								scaledSpaceCamera.gameObject.AddComponent (typeof(disableAmbientLight));
//								ambientLight=false;
//							}
//							
//							else
//							{
//								if (scaledSpaceCamera.GetComponent(typeof(disableAmbientLight)))
//									Component.Destroy(scaledSpaceCamera.GetComponent(typeof(disableAmbientLight)));
//								
//								ambientLight=true;
//							}
//						}
//						
//						GUILayout.EndHorizontal ();
						
						GUILayout.EndHorizontal ();

						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button ("Reload shader bundles"))
						{
							ShaderReplacer.Instance.LoadAssetBundle();
						}

						GUILayout.EndHorizontal ();
						
					}

				}
				
				else
				{
					GUILayout.Label (String.Format ("Inactive in tracking station and VAB/SPH"));
					GUILayout.EndHorizontal ();
				}
				
			}

			GUI.DragWindow();
		}
		
		//		//snippet by Thomas P. from KSPforum
		//		public void DeactivateAtmosphere(string name) {
		//			Transform t = ScaledSpace.Instance.transform.FindChild(name);
		//			
		//			for (int i = 0; i < t.childCount; i++) {
		//				if (t.GetChild(i).gameObject.layer == 9) {
		//					// Deactivate the Athmosphere-renderer
		//					t.GetChild(i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive(false);
		//					g
		//					// Reset the shader parameters
		//					Material sharedMaterial = t.renderer.sharedMaterial;
		//					
		//					//sharedMaterial.SetTexture(Shader.PropertyToID("_rimColorRamp"), null);
		//					//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), 0);
		//					//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), 0);
		//					
		//					// Stop our script
		//					i = t.childCount + 10;
		//				}
		//			}
		//		}
		
		
		
		public void getSettingsFromSkynode ()
		{
			SkyNode skyNode = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode;
			configPoint selected = skyNode.configPoints [selectedConfigPoint];

			postProcessingalpha = selected.postProcessAlpha;
			//			postProcessDepth = 10000 * selected.postProcessDepth;
			postProcessDepth = selected.postProcessDepth;
			
			_Post_Extinction_Tint = selected._Post_Extinction_Tint;
			postExtinctionMultiplier = selected.postExtinctionMultiplier;
			
			postProcessExposure = selected.postProcessExposure;
			exposure = selected.skyExposure;
			skyRimExposure = selected.skyRimExposure;
			alphaGlobal = selected.skyAlpha;
			
			openglThreshold = selected.openglThreshold;
			
			_GlobalOceanAlpha = selected._GlobalOceanAlpha;
			//			edgeThreshold = selected.edgeThreshold * 100;
			
			
			mapAlphaGlobal = skyNode.mapAlphaGlobal;
			mapExposure = skyNode.mapExposure;
			mapSkyRimeExposure = skyNode.mapSkyRimExposure;
			configPointsCnt = skyNode.configPoints.Count;
			
			specR = skyNode.specR;
			specG = skyNode.specG;
			specB = skyNode.specB;
			shininess = skyNode.shininess;
			
			
			rimBlend = skyNode.rimBlend;
			rimpower = skyNode.rimpower;
			
			//MapViewScale = skyNode.MapViewScale;
			extinctionMultiplier = selected.skyExtinctionMultiplier;
			extinctionTint = selected.skyExtinctionTint;
			skyExtinctionRimFade = selected.skyextinctionRimFade;
			skyExtinctionGroundFade = selected.skyextinctionGroundFade;
			_extinctionScatterIntensity = selected._extinctionScatterIntensity;
			
			mapExtinctionMultiplier = skyNode.mapExtinctionMultiplier;
			mapExtinctionTint = skyNode.mapExtinctionTint;
			mapSkyExtinctionRimFade= skyNode.mapSkyExtinctionRimFade;
			_mapExtinctionScatterIntensity = skyNode._mapExtinctionScatterIntensity;
			
			showInterpolatedValues = skyNode.displayInterpolatedVariables;
			
			mieG = skyNode.m_mieG;

			experimentalAtmoScale = skyNode.experimentalAtmoScale;
			viewdirOffset = selected.viewdirOffset;

			cloudColorMultiplier = skyNode.cloudColorMultiplier;
			cloudScatteringMultiplier = skyNode.cloudScatteringMultiplier;
			cloudSkyIrradianceMultiplier = skyNode.cloudSkyIrradianceMultiplier;

			volumetricsColorMultiplier = skyNode.volumetricsColorMultiplier;
//			volumetricsScatteringMultiplier = skyNode.volumetricsScatteringMultiplier;
//			volumetricsSkyIrradianceMultiplier = skyNode.volumetricsSkyIrradianceMultiplier;
		}
		
		public void getSettingsFromOceanNode ()
		{
			OceanWhiteCaps oceanNode = scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();
		
			oceanLevel = oceanNode.m_oceanLevel;
			oceanAlpha = oceanNode.oceanAlpha;
			oceanAlphaRadius = oceanNode.alphaRadius;
			
			oceanUpwellingColorR = oceanNode.m_oceanUpwellingColor.x;
			oceanUpwellingColorG = oceanNode.m_oceanUpwellingColor.y;
			oceanUpwellingColorB = oceanNode.m_oceanUpwellingColor.z;
			
			oceanScale = oceanNode.oceanScale;
			
			choppynessMultiplier = oceanNode.choppynessMultiplier;
			
			WAVE_CM = oceanNode.WAVE_CM;
			WAVE_KM = oceanNode.WAVE_KM;
			AMP = oceanNode.AMP;
			
			m_windSpeed = oceanNode.m_windSpeed;
			m_omega = oceanNode.m_omega;
			
			m_gridSizes = oceanNode.m_gridSizes;
			m_choppyness = oceanNode.m_choppyness;
//			m_fourierGridSize = oceanNode.m_fourierGridSize;
			
			m_ansio = oceanNode.m_ansio;
			
			m_varianceSize = oceanNode.m_varianceSize;
			m_foamAnsio = oceanNode.m_foamAnsio;
			m_foamMipMapBias = oceanNode.m_foamMipMapBias;
			m_whiteCapStr = oceanNode.m_whiteCapStr;
			farWhiteCapStr = oceanNode.m_farWhiteCapStr;
			
			m_resolution = oceanNode.m_resolution;
//			m_fourierGridSize = oceanNode.m_fourierGridSize;
			
		}
		

		
		public void loadConfigPoint (int point)
		{
			configPoint _cur = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point];

			postProcessDepth = _cur.postProcessDepth;
			_Post_Extinction_Tint = _cur._Post_Extinction_Tint;
			postExtinctionMultiplier = _cur.postExtinctionMultiplier;
			postProcessExposure = _cur.postProcessExposure;
			postProcessingalpha = _cur.postProcessAlpha;

			alphaGlobal = _cur.skyAlpha;
			exposure = _cur.skyExposure;
			skyRimExposure = _cur.skyRimExposure;
			extinctionMultiplier = _cur.skyExtinctionMultiplier;
			extinctionTint = _cur.skyExtinctionTint;
			skyExtinctionRimFade = _cur.skyextinctionRimFade;
			skyExtinctionGroundFade = _cur.skyextinctionGroundFade;
			_extinctionScatterIntensity = _cur._extinctionScatterIntensity;

			pointAltitude = _cur.altitude;

			openglThreshold = _cur.openglThreshold;
			_GlobalOceanAlpha = _cur._GlobalOceanAlpha;
			viewdirOffset = _cur.viewdirOffset;
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
		
		public void GUIfloat (string label, ref float local, ref float target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			local = float.Parse (GUILayout.TextField (local.ToString ("00000.0000")));
			if (GUILayout.Button ("Set")) {
				target = local;
			}
			GUILayout.EndHorizontal ();
		}
		
		public void GUIint (string label, ref int local, ref int target, int divisionFactor)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			local = (Int32)(Convert.ToInt32 (GUILayout.TextField (local.ToString ())));
			
			
			if (GUILayout.Button ("Set")) {
				target = local / divisionFactor;
			}
			GUILayout.EndHorizontal ();
		}
		
		public void GUIvector3 (string label, ref float localR, ref float localG, ref float localB, ref Vector3 target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			localR = float.Parse (GUILayout.TextField (localR.ToString ("0000.00000")));
			localG = float.Parse (GUILayout.TextField (localG.ToString ("0000.00000")));
			localB = float.Parse (GUILayout.TextField (localB.ToString ("0000.00000")));
			
			if (GUILayout.Button ("Set")) {
				target = new Vector3 (localR, localG, localB);
			}
			GUILayout.EndHorizontal ();
		}
		
		
		public void GUItoggle (string label, ref bool toToggle)
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (label))
				toToggle = !toToggle;
			GUILayout.EndHorizontal ();
		}

		void removeStockOceans()
		{
			FakeOceanPQS[] fakes = (FakeOceanPQS[])FakeOceanPQS.FindObjectsOfType (typeof(FakeOceanPQS));
			
			if (fakes.Length == 0) { //if stock oceans haven't already been replaced
				foreach (scattererCelestialBody sctBody in scattererCelestialBodies)
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
			foreach (scattererCelestialBody sctBody in scattererCelestialBodies)
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
