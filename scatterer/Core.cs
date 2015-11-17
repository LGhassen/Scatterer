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


namespace scatterer {
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class Core: MonoBehaviourWindow {
		[Persistent] List < scattererCelestialBody > scattererCelestialBodies = new List < scattererCelestialBody >
		{
			new scattererCelestialBody("Kerbin","Kerbin",5000,10000),
			new scattererCelestialBody("Duna","Duna",5000,10000),
			new scattererCelestialBody("Laythe","Laythe",5000,1000),
			new scattererCelestialBody("Eve","Eve",5000,1000)
		};

		[Persistent] int delayLoading = 50;

		CelestialBody[] CelestialBodies;

		List < celestialBodySortableByDistance > celestialBodiesWithDistance= new List<celestialBodySortableByDistance>();
		
		//List < Manager > Managers = new List < Manager >();
		
		
		/*
		[Persistent] List < String > scattererCelestialBodyNames = new List < String >
		{
			"Kerbin", "Duna", "Laythe", "Eve"
		};

		[Persistent] List < String > scattererTransformNames = new List < String >
		{
			"Kerbin", "Duna", "Laythe", "Eve"
		};

		public List < CelestialBody > scattererCelestialBodies = new List < CelestialBody > ();
		public List < Transform > scattererCelestialBodyTransforms = new List < Transform > ();

		*/
		
		CelestialBody sunCelestialBody;
		
		MeshRenderer mr = new MeshRenderer();
		
		public string path;
		
		int updateCnt = 0;
		
		bool found = false;
		bool showInterpolatedValues = false;
		public bool stockSunglare = false;
		public bool extinctionEnabled = true;
		
		float rimBlend = 20f;
		float rimpower = 600f;
		
		float extinctionMultiplier = 100f;
		float extinctionTint = 100f;
		
		float mapExtinctionMultiplier = 100f;
		float mapExtinctionTint = 100f;
		
		float specR = 0f, specG = 0f, specB = 0f, shininess = 0f;
		
		//configPoint variables 		
		float pointAltitude = 0f;
		float newCfgPtAlt = 0f;
		int configPointsCnt;
		int selectedConfigPoint = 0;
		int selectedPlanet = 0;
		
		Camera[] cams;
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		
		//int count;
		
		float MapViewScale = 1000f;
		
		//ReflectedLight variables
		//		float terrainReflectance=100;
		//		float sunIntensity=100;
		//		float irradianceFactor=100;
		
		
		//Debug variables
		//		public float[] additionalScales=new float[10];
		public bool[] debugSettings = new bool[10];
		//		public int renderQueue=2000;
		//		int renderQueue2=2010;
		
		//postprocessing properties
		//		float inscatteringCoeff=85f; //useless, removed from shader
		//		float extinctionCoeff=70f;   //obsolete
		//		float postProcessScale=1000f;
		float postProcessingalpha = 78f;
		float postProcessDepth = 200f;
		float postProcessExposure = 18f;
		//		float MapViewScale=1000f;
		
		
		//sky properties
		float exposure = 25f;
		float alphaGlobal = 100f;
		float mapExposure = 15f;
		float mapAlphaGlobal = 100f;
		
		//Transform ParentPlanetTransform;
		
		public Camera chosenCamera;
		public int layer = 15;
		int cam = 1;
		
		//other stuff
		//float atmosphereGlobalScale = 1000f;
		//float m_radius; // = 600000.0f;
		//String parentPlanet="Kerbin";
		
		//[Persistent] String ParentPlanetCelestialBodyName = "Kerbin";
		//[Persistent] String ParentPlanetTransformName = "Kerbin";
		
		
		//		int PlanetId;
		//		int SunId;
		
		//Manager m_manager;
		bool depthbufferEnabled = false;
		bool isActive;
		
		//Material originalMaterial;
		
		
		public Transform GetScaledTransform(string body) {

			//RSS quick fix
			if (body == "Earth")
			{
				body="Kerbin";
			}

			List < Transform > transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single(n => n.name == body);
		}
		
		internal override void Awake() {
			WindowCaption = "Scatterer v0.0191: alt+f10/f11 toggle";
			WindowRect = new Rect(0, 0, 300, 50);
			Visible = false;
			isActive = false;
			
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			path = Uri.UnescapeDataString(uri.Path);
			path = Path.GetDirectoryName(path);
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER|| HighLogic.LoadedScene == GameScenes.TRACKSTATION )
				
			{
				isActive = true;
			}
		}
		
		
		internal override void Update() {
			//toggle whether GUI is visible or not
			if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && (Input.GetKeyDown(KeyCode.F11) || (Input.GetKeyDown(KeyCode.F10)))) Visible = !Visible;
			
			if (isActive) {
				updateCnt++;
				if (updateCnt > delayLoading) {
					
					if (!found) {
						
						loadPlanets(); //loads the planets list
						
						//find sun and celestialbodies
						CelestialBodies = (CelestialBody[]) CelestialBody.FindObjectsOfType(typeof(CelestialBody));
						/*
						for (int i=0;i<CelestialBodies.Length;i++)
						{
							celestialBodiesWithDistance.Add(new celestialBodySortableByDistance() 
							                                {CelestialBody = CelestialBodies[i], Distance = 0}); 
						}
						
*/

						for (int k = 0; k < CelestialBodies.Length; k++)
						{
							bool inScatterer=false;
							for (int i=0;i<scattererCelestialBodies.Count;i++)
							{
								if (CelestialBodies[k].GetName() == scattererCelestialBodies[i].celestialBodyName)
								{
									celestialBodiesWithDistance.Add(new celestialBodySortableByDistance() 
									                                {CelestialBody = CelestialBodies[k], Distance = 0, usesScatterer=true, scattererIndex=i });
									inScatterer=true;
									i+=100;
								}
							}
							if(!inScatterer)
							{
								celestialBodiesWithDistance.Add(new celestialBodySortableByDistance() 
								{CelestialBody = CelestialBodies[k], Distance = 0, usesScatterer=false, scattererIndex=0 });
							}
						}


						for (int i=0;i<scattererCelestialBodies.Count;i++)
						{
							scattererCelestialBody cur = scattererCelestialBodies[i];
							cur.transform= ScaledSpace.Instance.transform.FindChild(cur.transformName);
							
							for (int k = 0; k < CelestialBodies.Length; k++)
							{
								if (CelestialBodies[k].GetName() == cur.celestialBodyName)
								{
									cur.celestialBody=CelestialBodies[k];
									//celestialBodiesWithDistance.Add(new celestialBodySortableByDistance() 
									                               // {CelestialBodyName = CelestialBodies[i].name, Distance = 0, usesScatterer=true, scattererIndex=i });
								}
								else
								{
									//celestialBodiesWithDistance.Add(new celestialBodySortableByDistance() 
									                                //{CelestialBodyName = CelestialBodies[i].name, Distance = 0, usesScatterer=false, scattererIndex=i });
									if (CelestialBodies[k].GetName() == "Sun")
										sunCelestialBody=CelestialBodies[k];
								}
							}
							cur.active=false;
						}
						
						
						
						cams = Camera.allCameras;
						
						for (int i=0; i<cams.Length; i++) {
							if (cams [i].name == "Camera ScaledSpace")
								scaledSpaceCamera = cams [i];
							
							if (cams [i].name == "Camera 01")
								farCamera = cams [i];
							if (cams [i].name == "Camera 00")
								nearCamera = cams [i];
						}
						
						found = true;
					}
					
					
					if ( found && ScaledSpace.Instance && farCamera) {
						
						for (int i=0;i<scattererCelestialBodies.Count;i++)
						{
							float dist;

							scattererCelestialBody cur = scattererCelestialBodies[i];
							if (FlightGlobals.ActiveVessel)
							{
								dist = Vector3.Distance (FlightGlobals.ActiveVessel.transform.position, ScaledSpace.ScaledToLocalSpace(cur.transform.position));
//								print (cur.celestialBody.name);
//								print ("dist "+dist.ToString());
//								print("dist2 "+ Vector3.Distance(farCamera.transform.position, ScaledSpace.ScaledToLocalSpace(cur.transform.position)).ToString());
							}
							else
							{
								dist = Vector3.Distance(farCamera.transform.position, ScaledSpace.ScaledToLocalSpace(cur.transform.position));
							}
							//print ("dist to ="+cur.celestialBodyName+" "+dist);
							if(cur.active)
							{
								if (dist>cur.unloadDistance && !MapView.MapIsEnabled)
								{
									cur.m_manager.OnDestroy();
									Destroy (cur.m_manager);
									cur.m_manager=null;
									//ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
									cur.active=false;

									Resources.UnloadUnusedAssets();
									System.GC.Collect();
									
									print("scatterer effects unloaded for "+cur.celestialBodyName);
								}
								else
								{
									//print ("updating manager for "+cur.celestialBodyName);
									cur.m_manager.Update();
								}
							}
							else
							{
								if (dist<cur.loadDistance && !MapView.MapIsEnabled)
								{
									//print("loading scatterer effects for "+cur.celestialBodyName);
									//create and configure manager
									cur.m_manager = new Manager();
									cur.m_manager.setParentCelestialBody(cur.celestialBody);
									cur.m_manager.setParentPlanetTransform(cur.transform);
									cur.m_manager.setSunCelestialBody(sunCelestialBody);
									cur.m_manager.SetCore(this);
									cur.m_manager.Awake();
									//cur.originalPlanetMaterialBackup=backupAtmosphereMaterial(cur.transformName);
									//tweakStockAtmosphere(cur.transformName, rimBlend, rimpower);
									
									//getSettingsFromSkynode();
									//loadConfigPoint(selectedConfigPoint);
									//m_radius = (float) celestialBodies[PlanetId].Radius;
									//backupAtmosphereMaterial(ParentPlanetTransformName);
									//tweakStockAtmosphere(ParentPlanetTransformName, rimBlend, rimpower);
									
									cur.active=true;
									selectedPlanet=i;
									getSettingsFromSkynode();
									print("scatterer effects loaded for "+cur.celestialBodyName);
								}
							}
						}

						fixDrawOrders();
					
					}
					
					/*
						//create and configure manager
						m_manager = new Manager();
						m_manager.setParentCelestialBody(celestialBodies[PlanetId]);
						m_manager.setParentPlanetTransform(ParentPlanetTransform);
						m_manager.setSunCelestialBody(celestialBodies[SunId]);
						m_manager.SetCore(this);
						m_manager.Awake();

						
						getSettingsFromSkynode();
						loadConfigPoint(selectedConfigPoint);
						
						m_radius = (float) celestialBodies[PlanetId].Radius;


						backupAtmosphereMaterial(ParentPlanetTransformName);
						tweakStockAtmosphere(ParentPlanetTransformName, rimBlend, rimpower);

						cams = Camera.allCameras;
						count = Camera.allCameras.Length;
						initiated = true;
						*/
					
					
					
					/*
					if (initiated) {
						m_manager.Update();
					}
					*/
					
				}
			}
		}
		
		void OnGUI() {
			//debugging for rendertextures, not needed anymore but might be when I implement oceans
			//	GUI.DrawTexture(new Rect(250,250,512,512), m_transmit, ScaleMode.StretchToFill, false);
			//	GUI.DrawTexture(new Rect(250,250,512,512), RenderTexture.active, ScaleMode.StretchToFill, false);
		}
		
		internal override void OnDestroy() {
			if (isActive)
			{
				//m_manager.OnDestroy ();
				//Destroy (m_manager);
				
				for (int i=0;i<scattererCelestialBodies.Count;i++)
				{
					scattererCelestialBody cur = scattererCelestialBodies[i];
					if (cur.active)
					{
						cur.m_manager.OnDestroy();
						Destroy(cur.m_manager);
						cur.m_manager=null;
						//ReactivateAtmosphere(cur.transformName,cur.originalPlanetMaterialBackup);
						cur.active=false;
					}
					
				}
			}
		}
		
		
		//		UI BUTTONS
		//		This isn't the most elegant section due to how much code is necessary for each element
		internal override void DrawWindow(int id) {
			DragEnabled = true;
			
			if (!isActive) GUILayout.Label(String.Format("Mod will activate in KSC view or in flight."));
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Hide")) Visible = !Visible;
			GUILayout.EndHorizontal();
			
			if (isActive) {
				

				GUILayout.BeginHorizontal();
				GUILayout.Label("Planet:");
				
				if (GUILayout.Button("<")) {
					if (selectedPlanet > 0) {
						selectedPlanet -= 1;
						selectedConfigPoint=0;
						if (scattererCelestialBodies[selectedPlanet].active)
						{
							loadConfigPoint(selectedConfigPoint);
							getSettingsFromSkynode();
						}
					}
				}
				
				GUILayout.TextField((scattererCelestialBodies[selectedPlanet].celestialBodyName).ToString());
				
				if (GUILayout.Button(">")) {
					if (selectedPlanet < scattererCelestialBodies.Count - 1) {
						selectedPlanet += 1;
						selectedConfigPoint=0;
						if (scattererCelestialBodies[selectedPlanet].active)
						{
							loadConfigPoint(selectedConfigPoint);
							getSettingsFromSkynode();
						}
					}
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Label("Planet loaded:"+scattererCelestialBodies[selectedPlanet].active.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Load distance:"+scattererCelestialBodies[selectedPlanet].loadDistance.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Unload distance:"+scattererCelestialBodies[selectedPlanet].unloadDistance.ToString());
				GUILayout.EndHorizontal();


				if (scattererCelestialBodies[selectedPlanet].active)
				{
					configPointsCnt=scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.Count;

					GUILayout.BeginHorizontal();
					
					if (GUILayout.Button("Toggle depth buffer")) {
						if (!depthbufferEnabled) {
							cams[2].gameObject.AddComponent(typeof(ViewDepthBuffer));
							depthbufferEnabled = true;
						} else {
							Component.Destroy(cams[2].gameObject.GetComponent < ViewDepthBuffer > ());
							depthbufferEnabled = false;
						}
					}
					
					if (GUILayout.Button("Toggle PostProcessing")) {
						
						if (!scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.postprocessingEnabled) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.enablePostprocess();
						} else {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.disablePostprocess();
						}
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("New point altitude:");
					newCfgPtAlt = (float)(Convert.ToDouble(GUILayout.TextField(newCfgPtAlt.ToString())));
					if (GUILayout.Button("Add")) {
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.Insert(selectedConfigPoint + 1, new configPoint(newCfgPtAlt, alphaGlobal / 100, exposure / 100, postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100, extinctionMultiplier / 100, extinctionTint / 100));
						selectedConfigPoint += 1;
						configPointsCnt = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.Count;
						loadConfigPoint(selectedConfigPoint);
					}
					GUILayout.EndHorizontal();
					
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Config point:");
					
					if (GUILayout.Button("<")) {
						if (selectedConfigPoint > 0) {
							selectedConfigPoint -= 1;
							loadConfigPoint(selectedConfigPoint);
						}
					}
					
					GUILayout.TextField((selectedConfigPoint).ToString());
					
					if (GUILayout.Button(">")) {
						if (selectedConfigPoint < configPointsCnt - 1) {
							selectedConfigPoint += 1;
							loadConfigPoint(selectedConfigPoint);
						}
					}
					
					//GUILayout.Label (String.Format("Total:{0}", configPointsCnt));
					if (GUILayout.Button("Delete")) {
						if (configPointsCnt <= 1) print("Can't delete config point, one or no points remaining");
						else {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.RemoveAt(selectedConfigPoint);
							if (selectedConfigPoint >= configPointsCnt - 1) {
								selectedConfigPoint = configPointsCnt - 2;
							}
							configPointsCnt = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.Count;
							loadConfigPoint(selectedConfigPoint);
						}
						
					}
					
					GUILayout.EndHorizontal();
					
					
					
					if (!MapView.MapIsEnabled) {
						
						//				GUILayout.BeginHorizontal ();
						//				GUILayout.Label ("Sky Settings");
						//				GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Point altitude");
						pointAltitude = (float)(Convert.ToDouble(GUILayout.TextField(pointAltitude.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].altitude = pointAltitude;
						}
						GUILayout.EndHorizontal();
						
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Sky/orbit Alpha (/100)");
						alphaGlobal = (float)(Convert.ToDouble(GUILayout.TextField(alphaGlobal.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyAlpha = alphaGlobal / 100f;
						}
						GUILayout.EndHorizontal();
						
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Sky/orbit Exposure (/100)");
						exposure = (float)(Convert.ToDouble(GUILayout.TextField(exposure.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExposure = exposure / 100f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Post Processing Alpha (/100)");
						postProcessingalpha = (float)(Convert.ToDouble(GUILayout.TextField(postProcessingalpha.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessAlpha = postProcessingalpha / 100f;
						}
						GUILayout.EndHorizontal();
						
						
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Post Processing Depth (/10000)");
						postProcessDepth = (float)(Convert.ToDouble(GUILayout.TextField(postProcessDepth.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessDepth = postProcessDepth / 10000f;
						}
						GUILayout.EndHorizontal();
						
						
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Post Processing Exposure (/100)");
						postProcessExposure = (float)(Convert.ToDouble(GUILayout.TextField(postProcessExposure.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessExposure = postProcessExposure / 100f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("extinctionMultiplier (/100)");
						extinctionMultiplier = (float)(Convert.ToDouble(GUILayout.TextField(extinctionMultiplier.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExtinctionMultiplier = extinctionMultiplier / 100f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("extinctionTint (/100)");
						extinctionTint = (float)(Convert.ToDouble(GUILayout.TextField(extinctionTint.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExtinctionTint = extinctionTint / 100f;
						}
						GUILayout.EndHorizontal();
						
					} else {
						
						//					GUILayout.BeginHorizontal ();
						//					GUILayout.Label ("Sky Settings (map view)");
						//					GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Map view alpha (/100)");
						mapAlphaGlobal = (float)(Convert.ToDouble(GUILayout.TextField(mapAlphaGlobal.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapAlphaGlobal = mapAlphaGlobal / 100f;
						}
						GUILayout.EndHorizontal();
						
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Map view exposure (/100)");
						mapExposure = (float)(Convert.ToDouble(GUILayout.TextField(mapExposure.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExposure = mapExposure / 100f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("Map view scale (/1000)");
						MapViewScale = (float)(Convert.ToDouble(GUILayout.TextField(MapViewScale.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.MapViewScale = MapViewScale / 1000f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("MapExtinctionMultiplier (/100)");
						mapExtinctionMultiplier = (float)(Convert.ToDouble(GUILayout.TextField(mapExtinctionMultiplier.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExtinctionMultiplier = mapExtinctionMultiplier / 100f;
						}
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label("MapExtinctionTint (/100)");
						mapExtinctionTint = (float)(Convert.ToDouble(GUILayout.TextField(mapExtinctionTint.ToString())));
						
						if (GUILayout.Button("Set")) {
							scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExtinctionTint = mapExtinctionTint / 100f;
						}
						GUILayout.EndHorizontal();
						
					}
					
					GUILayout.BeginHorizontal();
					
					GUILayout.Label(String.Format("ForceOFF aniso"));
					GUILayout.TextField(scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.forceOFFaniso.ToString());
					
					if (GUILayout.Button("Toggle")) {
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.toggleAniso();
					}
					GUILayout.EndHorizontal();
					
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("RimBlend");
					rimBlend = (float)(Convert.ToDouble(GUILayout.TextField(rimBlend.ToString())));
					
					GUILayout.Label("RimPower");
					rimpower = (float)(Convert.ToDouble(GUILayout.TextField(rimpower.ToString())));
					
					if (GUILayout.Button("Set")) {
						//					tweakStockAtmosphere(parentPlanet,rimBlend,rimpower);
						//tweakStockAtmosphere(ParentPlanetTransformName, rimBlend, rimpower);
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimBlend=rimBlend;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimpower=rimpower;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("Spec: R");
					specR = (float)(Convert.ToDouble(GUILayout.TextField(specR.ToString())));
					
					GUILayout.Label("G");
					specG = (float)(Convert.ToDouble(GUILayout.TextField(specG.ToString())));
					
					GUILayout.Label("B");
					specB = (float)(Convert.ToDouble(GUILayout.TextField(specB.ToString())));
					
					GUILayout.Label("shine");
					shininess = (float)(Convert.ToDouble(GUILayout.TextField(shininess.ToString())));
					
					if (GUILayout.Button("Set")) {
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR=specR;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG=specG;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB=specB;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess=shininess;
						scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere();
					}
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Save settings")) {
//						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
//						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.displayInterpolatedVariables = showInterpolatedValues;
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.saveToConfigNode();
					}
					
					if (GUILayout.Button("Load settings")) {
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.loadFromConfigNode();
						getSettingsFromSkynode();
						loadConfigPoint(selectedConfigPoint);
					}
					GUILayout.EndHorizontal();
					
					
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Display interpolated values")) {
						showInterpolatedValues = !showInterpolatedValues;
					}
					GUILayout.EndHorizontal();
					
					
					if (showInterpolatedValues)
					{
						GUILayout.BeginHorizontal();
						if (scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0) GUILayout.Label("Current state:Ground, cfgPoint 0");
						else if (scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt - 1) GUILayout.Label(String.Format("Current state:Orbit, cfgPoint{0}", scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
						else GUILayout.Label(String.Format("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.percentage)), scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.percentage), scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label(String.Format("SkyAlpha: {0} ", (int)(100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.alphaGlobal)));
						GUILayout.Label(String.Format("SkyExposure: {0}", (int)(100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.m_HDRExposure)));
						GUILayout.EndHorizontal();
						
						GUILayout.BeginHorizontal();
						GUILayout.Label(String.Format("PostAlpha: {0}", (int)(100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.postProcessingAlpha)));
						GUILayout.Label(String.Format("PostDepth: {0}", (int)(10000 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.postProcessDepth)));
						GUILayout.Label(String.Format("PostExposure: {0}", (int)(100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.postProcessExposure)));
						GUILayout.EndHorizontal();
					}
					
					
					GUILayout.BeginHorizontal();
					GUILayout.Label("ManagerState");
					GUILayout.TextField(scattererCelestialBodies[selectedPlanet].m_manager.getManagerState());
					GUILayout.EndHorizontal();
					
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Disable stock atmo")) {
						//					DeactivateAtmosphere(parentPlanet);
						//DeactivateAtmosphere(ParentPlanetTransformName);
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.DeactivateAtmosphere();
					}
					
					if (GUILayout.Button("Enable stock atmo")) {
						//					ReactivateAtmosphere(parentPlanet);
						//ReactivateAtmosphere(ParentPlanetTransformName);
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.RestoreStockAtmosphere();
					}
					GUILayout.EndHorizontal();
					
					
					GUILayout.BeginHorizontal();
					
					
					//				if (GUILayout.Button ("Toggle stock sunglare"))
					//				{
					//					stockSunglare =!stockSunglare;
					//				}
					
					if (GUILayout.Button("Toggle extinction")) {
						scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.extinctionEnabled =
							!scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.extinctionEnabled;
					}
					
					GUILayout.EndHorizontal();
					
					chosenCamera = cams[cam];
					
				}	
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
		
		
		
		public void getSettingsFromSkynode() {
			
			postProcessingalpha = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessAlpha;
			postProcessDepth = 10000 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessDepth;
			postProcessExposure = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessExposure;
			exposure = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExposure;
			alphaGlobal = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyAlpha;
			
			mapAlphaGlobal = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapAlphaGlobal;
			mapExposure = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExposure;
			configPointsCnt = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints.Count;

			specR = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR;
			specG = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG;
			specB = scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB;
			shininess=scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess;
			
			
			rimBlend = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimBlend;
			rimpower = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.rimpower;
			
			MapViewScale = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.MapViewScale * 1000f;
			extinctionMultiplier = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExtinctionMultiplier;
			extinctionTint = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExtinctionTint;
			
			mapExtinctionMultiplier = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExtinctionMultiplier;
			mapExtinctionTint = 100 * scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.mapExtinctionTint;
			
			showInterpolatedValues = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.displayInterpolatedVariables;
		}
		
		
//		public Material backupAtmosphereMaterial(string name) {
//			Transform t = ScaledSpace.Instance.transform.FindChild(name);
//			Material originalMaterial=null;
//			
//			for (int i = 0; i < t.childCount; i++) {
//				if (t.GetChild(i).gameObject.layer == 9) {
//					t.GetChild(i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive(true);
//					originalMaterial = (Material) Material.Instantiate(t.renderer.sharedMaterial);
//					i = t.childCount + 10;
//				}
//			}
//			return(originalMaterial);
//		}
//		
//		
//		public void ReactivateAtmosphere(string name, Material originalMaterial) {
//			Transform t = ScaledSpace.Instance.transform.FindChild(name);
//			for (int i = 0; i < t.childCount; i++) {
//				if (t.GetChild(i).gameObject.layer == 9) {
//					t.GetChild(i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive(true);
//					t.renderer.sharedMaterial = originalMaterial;
//					i = t.childCount + 10;
//				}
//			}
//		}
//		
//		
//		public void tweakStockAtmosphere(string name, float inRimBlend, float inRimPower) {
//			Transform t = ScaledSpace.Instance.transform.FindChild(name);
//			
//			for (int i = 0; i < t.childCount; i++) {
//				if (t.GetChild(i).gameObject.layer == 9) {
//					t.GetChild(i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive(false);
//					Material sharedMaterial = t.renderer.sharedMaterial;
//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), inRimBlend / 100f);
//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), inRimPower / 100f);
//					sharedMaterial.SetColor("_SpecColor", new Color(specR / 100f, specG / 100f, specB / 100f));
//					sharedMaterial.SetFloat("_Shininess", shininess / 100);
//					
//					i = t.childCount + 10;
//				}
//			}
//		}

		

		public void fixDrawOrders(){

			for (int k = 0; k < celestialBodiesWithDistance.Count; k++)
			{
				celestialBodiesWithDistance[k].Distance = Vector3.Distance (farCamera.transform.position,
				                                                            ScaledSpace.ScaledToLocalSpace(GetScaledTransform(celestialBodiesWithDistance[k].CelestialBody.name).position));
			}

			celestialBodiesWithDistance.Sort ();


			int currentRenderQueue = 2001;

			for (int k = 0; k < celestialBodiesWithDistance.Count; k++)
			{
				celestialBodySortableByDistance current=celestialBodiesWithDistance[celestialBodiesWithDistance.Count-1-k];

				Transform tmpTransform = GetScaledTransform(current.CelestialBody.name);

				MeshRenderer mr2 = (MeshRenderer) tmpTransform.GetComponent(typeof(MeshRenderer));

				if (mr2 != null) {
					mr2.material.renderQueue = currentRenderQueue;
//					print (current.CelestialBody.name+current.Distance.ToString());
//					print ("base queue:"+currentRenderQueue.ToString());
//					print (current.Distance);
//					
					currentRenderQueue+=1;
				}

				if (current.usesScatterer)
				{
					if(scattererCelestialBodies[current.scattererIndex].active)
					{
						scattererCelestialBodies[current.scattererIndex].m_manager.m_skyNode.m_skyExtinction.renderQueue
							=currentRenderQueue;
//						print ("extinction queue:"+currentRenderQueue.ToString());

						scattererCelestialBodies[current.scattererIndex].m_manager.m_skyNode.m_skyMaterialScaled.renderQueue
							=currentRenderQueue+1;

						currentRenderQueue+=2;
					}
				}
			}
		}




		public void loadConfigPoint(int point) {
			postProcessDepth = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].postProcessDepth * 10000f;
			postProcessExposure = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].postProcessExposure * 100f;
			postProcessingalpha = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].postProcessAlpha * 100f;
			
			alphaGlobal = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].skyAlpha * 100f;
			exposure = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].skyExposure * 100f;
			
			extinctionMultiplier = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].skyExtinctionMultiplier * 100f;
			extinctionTint = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].skyExtinctionTint * 100f;
			
			pointAltitude = scattererCelestialBodies[selectedPlanet].m_manager.m_skyNode.configPoints[point].altitude;
		}
		
		public void loadPlanets() {
			ConfigNode cnToLoad = ConfigNode.Load(path + "/config/PlanetsList.cfg");
			ConfigNode.LoadObjectFromConfig(this, cnToLoad);	
		}
		
		public void savePlanets() {
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this);
			cnTemp.Save(path + "/config/PlanetsList.cfg");
		}
	}
}