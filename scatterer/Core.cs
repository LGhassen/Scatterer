using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;
using KSPPluginFramework;

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	
	public class Core: MonoBehaviourWindow
	{

		bool wireFrame=false;

		[Persistent]
		List < scattererCelestialBody >
			scattererCelestialBodies = new List < scattererCelestialBody > {};
		CelestialBody[] CelestialBodies;

		Light[] lights;

		List < celestialBodySortableByDistance > celestialBodiesWithDistance = new List < celestialBodySortableByDistance > ();
		[Persistent]
		public bool
			render24bitDepthBuffer = true;
		[Persistent]
		public bool
			forceDisableDefaultDepthBuffer = false;

		[Persistent]
		public bool
			useOceanShaders = true;

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

		GameObject sunLight,scaledspaceSunLight;
		public GameObject copiedScaledSunLight;
		public GameObject copiedSunLight;
		

//		[Persistent]
		public bool craft_WaveInteractions = false;

//		[Persistent]
//		public bool
//			useGodrays = false;

//		[Persistent]
//		float godrayResolution = 0.5f;


//		[Persistent] float depthBufferSupersampling=1f;

		private Vector2 _scroll;
		private Vector2 _scroll2;
		public bool pqsEnabled = false;
		bool displayOceanSettings = false;
		CustomDepthBufferCam customDepthBuffer;
		public RenderTexture customDepthBufferTexture;
		public RenderTexture godrayDepthTexture;
		bool depthBufferSet = false;
//		public float windAngle=90;
//		public float anglex=1;
//		public float angley=1;
//		public float anglez=1;

		float experimentalAtmoScale=1f;
		float viewdirOffset=0f;
		



		CelestialBody sunCelestialBody;
		CelestialBody munCelestialBody;
		public string path;
		bool found = false;
		bool showInterpolatedValues = false;
		public bool stockSunglare = false;
		public bool extinctionEnabled = true;
		float rimBlend = 20f;
		float rimpower = 600f;
		float mieG = 85f;
		float openglThreshold = 250f;
		//		float globalThreshold = 250f;
		float edgeThreshold = 100f;
		float sunglareScale = 100f;
		float extinctionMultiplier = 100f;
		float extinctionTint = 100f;
		float skyExtinctionRimFade=0f;
		float mapExtinctionMultiplier = 100f;
		float mapExtinctionTint = 100f;
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

		
		float MapViewScale = 1000f;

		[Persistent]	public int oceanRenderQueue=2001;

		float postProcessingalpha = 78f;
		float postProcessDepth = 200f;
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

		int m_fourierGridSize = 128; //This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.


//		float sunReflectionMultiplier = 1f;
//		float skyReflectionMultiplier = 1f;
//		float seaRefractionMultiplier = 1f;

		
		
		//other stuff
		//float atmosphereGlobalScale = 1000f;
		
		//Manager m_manager;
		public bool depthbufferEnabled = false;
		public bool d3d9 = false;
		public bool opengl = false;
		bool isActive = false;
		bool mainMenu=false;
		
		//Material originalMaterial;
		
		public Transform GetScaledTransform (string body)
		{
			List < Transform > transforms = ScaledSpace.Instance.scaledSpaceTransforms;
//			return transforms.Single (n => n.name == body);
			return transforms.SingleOrDefault (n => n.name == body);

		}
		
		internal override void Awake ()
		{
			WindowCaption = "Scatterer v0.022: alt+f10/f11 toggle ";
			WindowRect = new Rect (0, 0, 400, 50);
			
			string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			path = Uri.UnescapeDataString (uri.Path);
			path = Path.GetDirectoryName (path);
			
			// Only load the planets once
			loadPlanetsList ();

			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));

			Visible = false;
			
			if (SystemInfo.graphicsDeviceVersion.StartsWith ("Direct3D 9")) {
//				d3d9 = true;
			} else if (SystemInfo.graphicsDeviceVersion.StartsWith ("OpenGL")) {
				opengl = true;
			}
			
			
			Debug.Log ("[Scatterer] Detected " + SystemInfo.graphicsDeviceVersion);
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				isActive = true;
			} 

			else if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				mainMenu=true;
				Visible=showMenuOnStart;
			}
		}
		
		internal override void Update ()
		{
			//toggle whether GUI is visible or not
			if ((Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) && (Input.GetKeyDown (KeyCode.F11) || (Input.GetKeyDown (KeyCode.F10))))
				Visible = !Visible;
			if (isActive && ScaledSpace.Instance) {
//			if ((HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER) && ScaledSpace.Instance) {

				if (!found) {

//					CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));


					foreach (scattererCelestialBody sctBody in scattererCelestialBodies) {
						var _sct = false;
						var _idx = 0;
//						var celBody = CelestialBodies.Single (_cb => _cb.bodyName == sctBody.celestialBodyName);
						var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);

						if (celBody == null) {
//							celBody = CelestialBodies.Single (_cb => _cb.bodyName == sctBody.transformName);
							celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);

						}

						Debug.Log ("[Scatterer] Celestial Body: " + celBody);
						if (celBody != null) {
							_sct = true;
							_idx = scattererCelestialBodies.IndexOf (sctBody);
							Debug.Log ("[Scatterer] Found: " + sctBody.celestialBodyName + " / " + celBody.GetName ());
						}
						;

						sctBody.celestialBody = celBody;
						celestialBodiesWithDistance.Add (new celestialBodySortableByDistance () {
							CelestialBody = sctBody.celestialBody,
							Distance = 0,
							usesScatterer = _sct,
							scattererIndex = _idx
						});
						
						var sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.transformName);
						if (!sctBodyTransform) {
							sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.celestialBodyName);
						}
						if (sctBodyTransform) {
							sctBody.transform = sctBodyTransform;
							sctBody.hasTransform = true;
						}
						sctBody.active = false;
					}

					
//					sunCelestialBody = CelestialBodies.Single (_cb => _cb.GetName () == "Sun");
					sunCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == "Sun");
					munCelestialBody = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == "Mun");


					//we need to load all of the celestial bodies in celestialBodiesWithDistance regardless
					//of whether they are loaded ins catterer or not
					//this is because we need to sort everything by distance to fix their draw orders
					//and avoid kerbin's atmo being visible from behind the mun for example
					for (int k = 0; k < CelestialBodies.Length; k++) {
						bool inScatterer = false;
						for (int i=0; i<scattererCelestialBodies.Count; i++) {
							if (CelestialBodies [k].GetName () == scattererCelestialBodies [i].celestialBodyName) {
								inScatterer = true;
								i += 100;
							}
						}

						if (!inScatterer) {
							celestialBodiesWithDistance.Add (new celestialBodySortableByDistance () 
							                                {CelestialBody = CelestialBodies[k], Distance = 0, usesScatterer=false, scattererIndex=0 });
						}
					}


					cams = Camera.allCameras;

					for (int i = 0; i < cams.Length; i++) {
						if (cams [i].name == "Camera ScaledSpace")
							scaledSpaceCamera = cams [i];
						
						if (cams [i].name == "Camera 01") {
							farCamera = cams [i];
							Debug.Log ("orig farCamera.nearClipPlane" + farCamera.nearClipPlane.ToString ());
							farCamera.nearClipPlane = 1000f;
						}

						if (cams [i].name == "Camera 00") {
							nearCamera = cams [i];
//							Debug.Log("orig nearCamera.nearClipPlane"+nearCamera.nearClipPlane.ToString());
//							Debug.Log("orig nearCamera.farClipPlane"+nearCamera.farClipPlane.ToString());
							nearCamera.nearClipPlane = 0.15f;
							nearCamera.farClipPlane = 1002f;
						}
					}


					//find sunlight
					lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
					Debug.Log ("number of lights" + lights.Length);
					foreach (Light _light in lights)
					{
						Debug.Log("name:"+_light.gameObject.name);
						Debug.Log("intensity:"+_light.intensity.ToString());
						Debug.Log ("mask:"+_light.cullingMask.ToString());
						Debug.Log ("type:"+_light.type.ToString());
						Debug.Log ("Parent:"+_light.transform.parent.gameObject.name);
						Debug.Log ("range:"+_light.range.ToString());

						if (_light.gameObject.name == "Scaledspace SunLight")
						{
							scaledspaceSunLight=_light.gameObject;
							Debug.Log("Found scaled sunlight");
//							scaledspaceSunLight.light.intensity=3;
						}

						if (_light.gameObject.name == "SunLight")
						{
							sunLight=_light.gameObject;
							_light.intensity=3;
							Debug.Log("Found sunlight");
							//							scaledspaceSunLight.light.intensity=3;
						}

					}
					//

					copiedScaledSunLight=(UnityEngine.GameObject) Instantiate(scaledspaceSunLight);
//					copiedScaledSunLight.name="copiedScaledSunLight";

//					copiedSunLight=(UnityEngine.GameObject) Instantiate(sunLight);
////					copiedSunLight.name="copiedSunLight";
					found = true;
				}
				

			
			
				if (ScaledSpace.Instance && farCamera) {
					if (callCollector)
						{
//							if (collectorCnt==3)
							{
								GC.Collect();
								callCollector=false;
							}
						}

					if (!depthBufferSet) {
						if (!render24bitDepthBuffer || d3d9) {

							farCamera.depthTextureMode = DepthTextureMode.Depth;
							Debug.Log ("[Scatterer] Using default depth buffer");
						} else {


						

							customDepthBuffer = (CustomDepthBufferCam)farCamera.gameObject.AddComponent (typeof(CustomDepthBufferCam));
							customDepthBuffer.inCamera = farCamera;
							customDepthBuffer.incore = this;
							
//								customDepthBufferTexture = new RenderTexture ((int) (Screen.width*depthBufferSupersampling),(int)( Screen.height*depthBufferSupersampling), 24, RenderTextureFormat.Depth);
							customDepthBufferTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
//								customDepthBufferTexture = new RenderTexture ( Screen.width,Screen.height,16, RenderTextureFormat.RFloat);//seems useless in the end  as oceans move around and the edges around the geometry can be taken care of elegantly in the fragment shader
							//might only be useful for godrays
							customDepthBufferTexture.filterMode = FilterMode.Trilinear;

							//introduce supersampling or built-in AA?

//								customDepthBufferTexture = new RenderTexture (Screen.width/2, Screen.height/2, 24, RenderTextureFormat.Depth);
								
							customDepthBufferTexture.Create ();

//							if (useGodrays)
//							{
//								godrayDepthTexture = new RenderTexture ((int)(Screen.width*godrayResolution),(int)(Screen.height*godrayResolution), 16, RenderTextureFormat.RFloat);
//								godrayDepthTexture.filterMode = FilterMode.Trilinear;
//								godrayDepthTexture.Create ();
//							}

							customDepthBuffer._depthTex = customDepthBufferTexture;
							
							Debug.Log ("[Scatterer] Running custom depth buffer");
								
							if (forceDisableDefaultDepthBuffer) {
								Debug.Log ("[Scatterer] Forcing default depth buffer off");
							}
						}
						depthBufferSet = true;

					}


					if (render24bitDepthBuffer && !d3d9 && !customDepthBufferTexture.IsCreated ())
					{
						customDepthBufferTexture.Create ();
					}


				
					if (forceDisableDefaultDepthBuffer) { //want this to be forced off every frame, in case some other mod is reenabling it
						farCamera.depthTextureMode = DepthTextureMode.None;

					}
				

				
					pqsEnabled = false;
					foreach (scattererCelestialBody _cur in scattererCelestialBodies) {
						float dist;
						if (_cur.hasTransform) {
							if (FlightGlobals.ActiveVessel) {
								dist = Vector3.Distance (
								FlightGlobals.ActiveVessel.transform.position,
								ScaledSpace.ScaledToLocalSpace (_cur.transform.position));
							} else {
								dist = Vector3.Distance (
								farCamera.transform.position,
								ScaledSpace.ScaledToLocalSpace (_cur.transform.position));
							}
							if (_cur.active) {
								if (dist > _cur.unloadDistance && !MapView.MapIsEnabled) {
									_cur.m_manager.OnDestroy ();
									UnityEngine.Object.Destroy (_cur.m_manager);
									_cur.m_manager = null;
									//ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
									_cur.active = false;
								
									callCollector=true;

									Debug.Log ("[Scatterer] Effects unloaded for " + _cur.celestialBodyName);
								} else {
									_cur.m_manager.Update ();
									pqsEnabled = pqsEnabled || !_cur.m_manager.m_skyNode.inScaledSpace;
								}
							} else {
								if (dist < _cur.loadDistance && !MapView.MapIsEnabled && _cur.transform && _cur.celestialBody) {
									_cur.m_manager = new Manager ();
									_cur.m_manager.setParentCelestialBody (_cur.celestialBody);
									_cur.m_manager.setParentPlanetTransform (_cur.transform);
									_cur.m_manager.setSunCelestialBody (sunCelestialBody);

									//Find eclipse casters
									List<CelestialBody> eclipseCasters=new List<CelestialBody> {};

									for (int k=0; k < _cur.eclipseCasters.Count; k++)
									{
										var cc = CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == _cur.eclipseCasters[k]);
										if (cc==null)
										    Debug.Log("[Scatterer] Eclipse caster "+_cur.eclipseCasters[k]+" not found for "+_cur.celestialBodyName);
										else
										{
											eclipseCasters.Add(cc);
											Debug.Log("[Scatterer] Added eclipse caster "+_cur.eclipseCasters[k]+" for "+_cur.celestialBodyName);
//											copiedScaledSunLight.light.type=LightType.Point;
//											copiedScaledSunLight.light.range=1E9f;
//											copiedScaledSunLight.light.cullingMask=sunLight.light.cullingMask;
//											copiedSunLight.transform.parent=cc.transform;
										}
									}



									_cur.m_manager.eclipseCasters=eclipseCasters;
										
									_cur.m_manager.SetCore (this);
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

//					copiedScaledSunLight.transform.position=ScaledSpace.LocalToScaledSpace(munCelestialBody.transform.position);

					fixDrawOrders ();
				}
			} 
		}
		
		void OnGUI ()
		{
			//debugging for rendertextures, not needed anymore but might be when I implement oceans
			//	GUI.DrawTexture(new Rect(250,250,512,512), m_transmit, ScaleMode.StretchToFill, false);
			//	GUI.DrawTexture(new Rect(250,250,512,512), RenderTexture.active, ScaleMode.StretchToFill, false);
		}
		
		internal override void OnDestroy ()
		{
						Debug.Log ("[Scatterer] Core.OnDestroy() called");


			if (isActive) {
				//m_manager.OnDestroy ();
				//Destroy (m_manager);
				
				for (int i = 0; i < scattererCelestialBodies.Count; i++) {
					scattererCelestialBody cur = scattererCelestialBodies [i];
					if (cur.active) {
						cur.m_manager.OnDestroy ();
						UnityEngine.Object.Destroy (cur.m_manager);
						cur.m_manager = null;
						//ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
						cur.active = false;
					}
					
				}

				customDepthBuffer.OnDestroy();
				Component.Destroy (customDepthBuffer);
				UnityEngine.Object.Destroy (customDepthBuffer);
				UnityEngine.Object.Destroy (customDepthBufferTexture);

				if (nearCamera.gameObject.GetComponent (typeof(Wireframe)))
					Component.Destroy (nearCamera.gameObject.GetComponent (typeof(Wireframe)));
				
				if (farCamera.gameObject.GetComponent (typeof(Wireframe)))
					Component.Destroy (farCamera.gameObject.GetComponent (typeof(Wireframe)));
				
				if (scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
					Component.Destroy (scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
			
			
			}

			else if (mainMenu)
			
			{
				Debug.Log("[Scatterer] saving user settings");
				savePlanetsList(); //save user preferences //originally this was created only for the planets list, I'll change it later
			}

		}
		
		
		//		UI BUTTONS
		//		This isn't the most elegant section due to how much code is necessary for each element
		internal override void DrawWindow (int id)
		{
			DragEnabled = true;

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Hide")) Visible = !Visible;
			GUILayout.EndHorizontal();


			if (mainMenu)  //MAIN MENU options
			{ 

				GUILayout.Label (String.Format ("Scatterer: features selector"));
				useOceanShaders = GUILayout.Toggle(useOceanShaders, "Ocean shaders");
				oceanCloudShadows = GUILayout.Toggle(oceanCloudShadows, "EVE cloud shadows on ocean, may cause artifacts");
				render24bitDepthBuffer=GUILayout.Toggle(render24bitDepthBuffer, "Use 24 bit depth buffer (dx11/Ogl, removes artifacts)");
				fullLensFlareReplacement=GUILayout.Toggle(render24bitDepthBuffer, "Full lens flare replacement (forces 24bit dbuffer, even in dx9)");

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Menu scroll section height");
				scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (scrollSectionHeight.ToString ())));
				GUILayout.EndHorizontal ();

//				useGodrays=GUILayout.Toggle(useGodrays, "Godrays (WIP, use for screens only)");
//
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Godray resolution");
//				godrayResolution = float.Parse (GUILayout.TextField (godrayResolution.ToString ("00000.0000")));
//				GUILayout.EndHorizontal ();

				showMenuOnStart = GUILayout.Toggle(showMenuOnStart, "Show this menu on start-up");
			}



			else if (isActive)
			{
				
//								_scroll = GUILayout.BeginScrollView (_scroll);
				
				
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
					}
					
					if (GUILayout.Button ("Ocean settings")) {
						if (scattererCelestialBodies [selectedPlanet].hasOcean)
							displayOceanSettings = true;
					}
					
					GUILayout.EndHorizontal ();
					
					
					if (!displayOceanSettings) {
						configPoint _selectedCfgPt = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint];
						
//						_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (500));
						_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (scrollSectionHeight));
						{
						
							if (!MapView.MapIsEnabled) {
							
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("New point altitude:");
								newCfgPtAlt = (float)(Convert.ToDouble (GUILayout.TextField (newCfgPtAlt.ToString ())));
								if (GUILayout.Button ("Add")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Insert (selectedConfigPoint + 1, new configPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100, skyRimExposure/100, postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100, extinctionMultiplier / 100, extinctionTint / 100, skyExtinctionRimFade/100, openglThreshold, edgeThreshold / 100,viewdirOffset));
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
									else {
										scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.RemoveAt (selectedConfigPoint);
										if (selectedConfigPoint >= configPointsCnt - 1) {
											selectedConfigPoint = configPointsCnt - 2;
										}
										configPointsCnt = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
										loadConfigPoint (selectedConfigPoint);
									}
								
								}
							
								GUILayout.EndHorizontal ();
							
							
							
							
							
								//				GUILayout.BeginHorizontal ();
								//				GUILayout.Label ("Sky Settings");
								//				GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Point altitude");
								pointAltitude = (float)(Convert.ToDouble (GUILayout.TextField (pointAltitude.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].altitude = pointAltitude;
								}
								GUILayout.EndHorizontal ();

//								GUIfloat("Point altitude",ref pointAltitude, ref _selectedCfgPt.altitude);
							
								GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,
								         ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);

								GUIfloat("experimentalViewDirOffset", ref viewdirOffset,
								         ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].viewdirOffset);
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Sky/orbit Alpha (/100)");
								alphaGlobal = (float)(Convert.ToDouble (GUILayout.TextField (alphaGlobal.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyAlpha = alphaGlobal / 100f;
								}
								GUILayout.EndHorizontal ();
							
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Sky/orbit Exposure (/100)");
								exposure = (float)(Convert.ToDouble (GUILayout.TextField (exposure.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyExposure = exposure / 100f;
								}
								GUILayout.EndHorizontal ();

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Sky/orbit Rim Exposure (/100)");
								skyRimExposure = (float)(Convert.ToDouble (GUILayout.TextField (skyRimExposure.ToString ())));
								
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyRimExposure = skyRimExposure / 100f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Post Processing Alpha (/100)");
								postProcessingalpha = (float)(Convert.ToDouble (GUILayout.TextField (postProcessingalpha.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].postProcessAlpha = postProcessingalpha / 100f;
								}
								GUILayout.EndHorizontal ();
							
							
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Post Processing Depth (/10000)");
								postProcessDepth = (float)(Convert.ToDouble (GUILayout.TextField (postProcessDepth.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].postProcessDepth = postProcessDepth / 10000f;
								}
								GUILayout.EndHorizontal ();
							
							
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Post Processing Exposure (/100)");
								postProcessExposure = (float)(Convert.ToDouble (GUILayout.TextField (postProcessExposure.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].postProcessExposure = postProcessExposure / 100f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("extinctionMultiplier (/100)");
								extinctionMultiplier = (float)(Convert.ToDouble (GUILayout.TextField (extinctionMultiplier.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyExtinctionMultiplier = extinctionMultiplier / 100f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("extinctionTint (/100)");
								extinctionTint = (float)(Convert.ToDouble (GUILayout.TextField (extinctionTint.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyExtinctionTint = extinctionTint / 100f;
								}
								GUILayout.EndHorizontal ();

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("extinctionRimFade (/100)");
								skyExtinctionRimFade = (float)(Convert.ToDouble (GUILayout.TextField (skyExtinctionRimFade.ToString ())));
								
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].skyextinctionRimFade = skyExtinctionRimFade / 100f;
								}
								GUILayout.EndHorizontal ();
							
								if (!d3d9) {
								
									GUILayout.BeginHorizontal ();
									GUILayout.Label ("OpenGL/dx11 Threshold");
									openglThreshold = (float)(Convert.ToDouble (GUILayout.TextField (openglThreshold.ToString ())));
								
								
									if (GUILayout.Button ("Set")) {
										scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint].openglThreshold = openglThreshold;
									}
								
									GUILayout.EndHorizontal ();
								
//									GUILayout.BeginHorizontal ();
//									GUILayout.Label ("24bit dbuffer Edge Tshld");
//									edgeThreshold = (float)(Convert.ToDouble (GUILayout.TextField (edgeThreshold.ToString ())));

								}
							

							} 
							else
							{

								GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,
								         ref scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);


								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Map view alpha (/100)");
								mapAlphaGlobal = (float)(Convert.ToDouble (GUILayout.TextField (mapAlphaGlobal.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapAlphaGlobal = mapAlphaGlobal / 100f;
								}
								GUILayout.EndHorizontal ();
							
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Map view exposure (/100)");
								mapExposure = (float)(Convert.ToDouble (GUILayout.TextField (mapExposure.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExposure = mapExposure / 100f;
								}
								GUILayout.EndHorizontal ();

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Map view rim exposure (/100)");
								mapSkyRimeExposure = (float)(Convert.ToDouble (GUILayout.TextField (mapSkyRimeExposure.ToString ())));
								
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyRimExposure = mapSkyRimeExposure / 100f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Map view scale (/1000)");
								MapViewScale = (float)(Convert.ToDouble (GUILayout.TextField (MapViewScale.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.MapViewScale = MapViewScale / 1000f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("MapExtinctionMultiplier (/100)");
								mapExtinctionMultiplier = (float)(Convert.ToDouble (GUILayout.TextField (mapExtinctionMultiplier.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionMultiplier = mapExtinctionMultiplier / 100f;
								}
								GUILayout.EndHorizontal ();
							
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("MapExtinctionTint (/100)");
								mapExtinctionTint = (float)(Convert.ToDouble (GUILayout.TextField (mapExtinctionTint.ToString ())));
							
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionTint = mapExtinctionTint / 100f;
								}
								GUILayout.EndHorizontal ();

								GUILayout.BeginHorizontal ();
								GUILayout.Label ("MapExtinctionRimFade (/100)");
								mapSkyExtinctionRimFade = (float)(Convert.ToDouble (GUILayout.TextField (mapSkyExtinctionRimFade.ToString ())));
								
								if (GUILayout.Button ("Set")) {
									scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyExtinctionRimFade = mapSkyExtinctionRimFade / 100f;
								}
								GUILayout.EndHorizontal ();
							}
						
							//					GUILayout.BeginHorizontal();
							//					
							//					GUILayout.Label(String.Format("ForceOFF aniso"));
							//					GUILayout.TextField(scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.forceOFFaniso.ToString());
							//					
							//					if (GUILayout.Button("Toggle")) {
							//						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.toggleAniso();
							//					}
							//					GUILayout.EndHorizontal();
						
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("mieG (/100)");
							mieG = (float)(Convert.ToDouble (GUILayout.TextField (mieG.ToString ())));
						
							if (GUILayout.Button ("Set")) {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_mieG = mieG / 100f;
							}
							GUILayout.EndHorizontal ();
						
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Sunglare scale (/100)");
							sunglareScale = (float)(Convert.ToDouble (GUILayout.TextField (sunglareScale.ToString ())));
						
							if (GUILayout.Button ("Set")) {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.sunglareScale = sunglareScale / 100f;
							}
							GUILayout.EndHorizontal ();
						
						
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("RimBlend");
							rimBlend = (float)(Convert.ToDouble (GUILayout.TextField (rimBlend.ToString ())));
						
							GUILayout.Label ("RimPower");
							rimpower = (float)(Convert.ToDouble (GUILayout.TextField (rimpower.ToString ())));
						
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

						


						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Display interpolated values")) {
							showInterpolatedValues = !showInterpolatedValues;
						}
						GUILayout.EndHorizontal ();


						


						
						
						if (showInterpolatedValues) {
							GUILayout.BeginHorizontal ();
							if (scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0)
								GUILayout.Label ("Current state:Ground, cfgPoint 0");
							else if (scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt)
								GUILayout.Label (String.Format ("Current state:Orbit, cfgPoint{0}", scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1));
							else
								GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage)), scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage), scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
							GUILayout.EndHorizontal ();
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label (String.Format ("SkyAlpha: {0} ", (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.alphaGlobal)));
							GUILayout.Label (String.Format ("SkyExposure: {0}", (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_HDRExposure)));
							GUILayout.EndHorizontal ();
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label (String.Format ("PostAlpha: {0}", (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.postProcessingAlpha)));
							GUILayout.Label (String.Format ("PostDepth: {0}", (int)(10000 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.postProcessDepth)));
							GUILayout.Label (String.Format ("PostExposure: {0}", (int)(100 * scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.postProcessExposure)));
							GUILayout.EndHorizontal ();
						}

						}
						GUILayout.EndScrollView ();

						
						
						GUILayout.BeginHorizontal ();
						
						
						//				if (GUILayout.Button ("Toggle stock sunglare"))
						//				{
						//					stockSunglare =!stockSunglare;
						//				}

						
													if (GUILayout.Button ("Toggle depth buffer")) {
														if (!depthbufferEnabled) {
						//							cams[2].gameObject.AddComponent(typeof(ViewDepthBuffer));
															depthbufferEnabled = true;
														} else {
						//							Component.Destroy(cams[2].gameObject.GetComponent < ViewDepthBuffer > ());
															depthbufferEnabled = false;
														}
													}
						
						if (GUILayout.Button ("Toggle PostProcessing")) {
							
							if (!scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.postprocessingEnabled) {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.enablePostprocess ();
							} else {
								scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.disablePostprocess ();
							}
						}

						
						GUILayout.EndHorizontal ();


						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Save atmo")) {
							//						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
							//						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
							scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.displayInterpolatedVariables = showInterpolatedValues;
							scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.saveToConfigNode ();
						}
						
						if (GUILayout.Button ("Load atmo")) {
							scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.loadFromConfigNode (false);
							getSettingsFromSkynode ();
							loadConfigPoint (selectedConfigPoint);
						}
						
						if (GUILayout.Button ("Load backup")) {
							scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.loadFromConfigNode (true);
							getSettingsFromSkynode ();
							loadConfigPoint (selectedConfigPoint);
							
						}
						GUILayout.EndHorizontal ();




						//									for (int j=0;j<10;j++){
						//									GUILayout.BeginHorizontal ();
						//									GUILayout.Label (String.Format("Debug setting:{0}", j.ToString()));	
						//									GUILayout.TextField(debugSettings[j].ToString());
						//									GUILayout.EndHorizontal ();
						//					}
						
						//						chosenCamera = cams [cam];
						

					} else {

						OceanWhiteCaps oceanNode = scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();




						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button ("Toggle ocean")) {
							stockOcean = !stockOcean;
						}
						GUILayout.EndHorizontal ();

//						_scroll2 = GUILayout.BeginScrollView (_scroll2, false, true, GUILayout.Width (400), GUILayout.Height (500));
						_scroll2 = GUILayout.BeginScrollView (_scroll2, false, true, GUILayout.Width (400), GUILayout.Height (scrollSectionHeight));
						{

							GUIfloat ("ocean Level", ref oceanLevel, ref oceanNode.m_oceanLevel);
							GUIfloat ("Alpha/WhiteCap Radius", ref oceanAlphaRadius, ref oceanNode.alphaRadius);
							GUIfloat ("ocean Alpha", ref oceanAlpha, ref oceanNode.oceanAlpha);
							GUIfloat ("whiteCapStr (foam)", ref m_whiteCapStr, ref oceanNode.m_whiteCapStr);
							GUIfloat ("far whiteCapStr", ref farWhiteCapStr, ref oceanNode.m_farWhiteCapStr);
							GUIfloat ("choppyness multiplier", ref choppynessMultiplier, ref oceanNode.choppynessMultiplier);

//							GUILayout.BeginHorizontal ();
//							GUILayout.Label ("Color settings");
//							GUILayout.EndHorizontal ();

							GUIvector3 ("Ocean Upwelling Color", ref oceanUpwellingColorR, ref oceanUpwellingColorG, ref oceanUpwellingColorB, ref oceanNode.m_oceanUpwellingColor);

//							GUIfloat ("sunReflectionMultiplier", ref sunReflectionMultiplier, ref oceanNode.sunReflectionMultiplier);
//							GUIfloat ("skyReflectionMultiplier", ref skyReflectionMultiplier, ref oceanNode.skyReflectionMultiplier);
//							GUIfloat ("seaRefractionMultiplier", ref seaRefractionMultiplier, ref oceanNode.seaRefractionMultiplier);



							GUILayout.BeginHorizontal ();
							GUILayout.Label ("To apply the next setting press \"rebuild ocean\" and wait");
							GUILayout.EndHorizontal ();
						
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Keep in mind this saves your current settings");
							GUILayout.EndHorizontal ();



//						    GUIfloat("ocean Scale", ref oceanScale, ref oceanNode.oceanScale);
							GUIfloat ("WAVE_CM (default 0.23)", ref WAVE_CM, ref oceanNode.WAVE_CM);
							GUIfloat ("WAVE_KM (default 370)", ref WAVE_KM, ref oceanNode.WAVE_KM);
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

							GUIint ("m_resolution (sea detail, lower is better)", ref m_resolution, ref oceanNode.m_resolution, 1);
//							GUIint ("MAX_VERTS", ref MAX_VERTS, ref oceanNode.MAX_VERTS, 1);

							GUIint ("m_fourierGridSize(power of 2, 256 max)", ref m_fourierGridSize, ref oceanNode.m_fourierGridSize, 1);
//							GUIfloat("windDir",ref windAngle,ref windAngle);
//							GUIfloat("x",ref anglex,ref anglex);
//							GUIfloat("y",ref angley,ref angley);
//							GUIfloat("z",ref anglez,ref anglez);
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
							scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().loadFromConfigNode (false);
							getSettingsFromOceanNode ();
						}
						
						if (GUILayout.Button ("Load backup")) {
							scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().loadFromConfigNode (true);
							getSettingsFromOceanNode ();
						}
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
					
					GUILayout.EndHorizontal ();
					
				}
				
			}

			else
			{
				GUILayout.Label (String.Format ("Inactive in tracking station and VAB/SPH"));
			}

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

			postProcessingalpha = 100 * selected.postProcessAlpha;
			postProcessDepth = 10000 * selected.postProcessDepth;
			postProcessExposure = 100 * selected.postProcessExposure;
			exposure = 100 * selected.skyExposure;
			skyRimExposure = 100 * selected.skyRimExposure;
			alphaGlobal = 100 * selected.skyAlpha;
			
			openglThreshold = selected.openglThreshold;
//			edgeThreshold = selected.edgeThreshold * 100;
			
			
			mapAlphaGlobal = 100 * skyNode.mapAlphaGlobal;
			mapExposure = 100 * skyNode.mapExposure;
			mapSkyRimeExposure = 100 * skyNode.mapSkyRimExposure;
			configPointsCnt = skyNode.configPoints.Count;
			
			specR = skyNode.specR;
			specG = skyNode.specG;
			specB = skyNode.specB;
			shininess = skyNode.shininess;
			
			
			rimBlend = skyNode.rimBlend;
			rimpower = skyNode.rimpower;
			
			MapViewScale = skyNode.MapViewScale * 1000f;
			extinctionMultiplier = 100 * selected.skyExtinctionMultiplier;
			extinctionTint = 100 * selected.skyExtinctionTint;
			skyExtinctionRimFade = 100 * selected.skyextinctionRimFade;
			
			mapExtinctionMultiplier = 100 * skyNode.mapExtinctionMultiplier;
			mapExtinctionTint = 100 * skyNode.mapExtinctionTint;
			mapSkyExtinctionRimFade= 100 * skyNode.mapSkyExtinctionRimFade;
			
			showInterpolatedValues = skyNode.displayInterpolatedVariables;
			
			mieG = skyNode.m_mieG * 100f;
			
			sunglareScale = skyNode.sunglareScale * 100f;

			experimentalAtmoScale = skyNode.experimentalAtmoScale;

			
			
			
			//			globalThreshold = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.globalThreshold;
			//			horizonDepth = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.horizonDepth * 10000f; ;
			
			
		}

		public void getSettingsFromOceanNode ()
		{
//			Debug.Log ("getSettingsFromOceanNode ()");
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
			m_fourierGridSize = oceanNode.m_fourierGridSize;
			
			m_ansio = oceanNode.m_ansio;
			
			m_varianceSize = oceanNode.m_varianceSize;
			m_foamAnsio = oceanNode.m_foamAnsio;
			m_foamMipMapBias = oceanNode.m_foamMipMapBias;
			m_whiteCapStr = oceanNode.m_whiteCapStr;
			farWhiteCapStr = oceanNode.m_farWhiteCapStr;

			m_resolution = oceanNode.m_resolution;
//			MAX_VERTS = oceanNode.MAX_VERTS;
			m_fourierGridSize = oceanNode.m_fourierGridSize;

		}
		

		
		public void fixDrawOrders ()
		{
			
			for (int k = 0; k < celestialBodiesWithDistance.Count; k++) {
				if (celestialBodiesWithDistance [k].CelestialBody) {
					float dist;

//					if (!MapView.MapIsEnabled)
//					{
//						dist=  Vector3.Distance (
//							farCamera.transform.position,
//							ScaledSpace.ScaledToLocalSpace (
//							GetScaledTransform (celestialBodiesWithDistance [k].CelestialBody.name).position));
//					}
//					else
					{
						dist= Vector3.Distance (
							ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position),
							ScaledSpace.ScaledToLocalSpace (
							GetScaledTransform (celestialBodiesWithDistance [k].CelestialBody.name).position));

					}

					celestialBodiesWithDistance [k].Distance = dist;
				}
			}
			celestialBodiesWithDistance.Sort ();
			int currentRenderQueue = 2001;
			
			for (int k = 0; k < celestialBodiesWithDistance.Count; k++) {
				celestialBodySortableByDistance current = celestialBodiesWithDistance [celestialBodiesWithDistance.Count - 1 - k];
				
				if (current.CelestialBody != null) {
					Transform tmpTransform = GetScaledTransform (current.CelestialBody.name);
					
					MeshRenderer mr2 = (MeshRenderer)tmpTransform.GetComponent (typeof(MeshRenderer));
					
					if (mr2 != null) {
						mr2.material.renderQueue = currentRenderQueue;
//											print (current.CelestialBody.name+current.Distance.ToString());
//											print ("base queue:"+currentRenderQueue.ToString());
//											print (current.Distance);
											
						currentRenderQueue += 1;
					}
					
					if (current.usesScatterer) {
						if (scattererCelestialBodies [current.scattererIndex].active) {
							
							scattererCelestialBodies [current.scattererIndex].m_manager.m_skyNode.m_skyMaterialScaled.renderQueue = currentRenderQueue;
							
							currentRenderQueue += 1;
						}
					}
				}
			}
		}
		
		public void loadConfigPoint (int point)
		{
			postProcessDepth = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].postProcessDepth * 10000f;
			postProcessExposure = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].postProcessExposure * 100f;
			postProcessingalpha = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].postProcessAlpha * 100f;
			
			alphaGlobal = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyAlpha * 100f;
			exposure = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyExposure * 100f;
			skyRimExposure = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyRimExposure * 100f;
			
			extinctionMultiplier = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyExtinctionMultiplier * 100f;
			extinctionTint = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyExtinctionTint * 100f;
			skyExtinctionRimFade = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].skyextinctionRimFade * 100f;
			
			pointAltitude = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].altitude;
			
			openglThreshold = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].openglThreshold;
//			edgeThreshold = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].edgeThreshold * 100;
			viewdirOffset = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point].viewdirOffset;
			
		}
		
		public void loadPlanetsList ()
		{
			ConfigNode cnToLoad = ConfigNode.Load (path + "/config/PlanetsList.cfg");
			ConfigNode.LoadObjectFromConfig (this, cnToLoad);
		}
		
		public void savePlanetsList ()
		{
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
			cnTemp.Save (path + "/config/PlanetsList.cfg");
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





		
		
	}
}