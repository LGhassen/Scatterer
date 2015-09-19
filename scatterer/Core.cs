using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using KSP;
using KSP.IO;
using UnityEngine;
using KSPPluginFramework;


namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class Core : MonoBehaviourWindow
	{
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType<SkyNode>(null);

		[Persistent] List<String> scattererPlanets= new List<String> {};// { "Kerbin", "Duna", "Eeloo" };

		MeshRenderer mr = new MeshRenderer ();

		int updateCnt=0;

		bool initiated=false;
		bool found=false;
		bool showInterpolatedValues=false;
		public bool stockOcean=false;
		public bool stockSunglare=false;
		public bool extinctionEnabled=true;

		float rimBlend=20f;
		float rimpower=600f;

		float extinctionMultiplier=100f;

		//configPoint variables 		
		float pointAltitude=0f;
		float newCfgPtAlt=0f;
		int configPointsCnt;
		int selectedConfigPoint=0;
		
		Camera[] cams;
		int count;
		
//		float apparentDistance=1f;
		
		//ReflectedLight variables
//		float terrainReflectance=100;
//		float sunIntensity=100;
//		float irradianceFactor=100;

		//Ocean variables
		float oceanSigma = 0.04156494f;
		float oceanThreshold=25f;
		float theta =1.0f;
		float phi=1.0f;
		float oceanLevel=0f;
		float oceanNearPlane=0.01f;
		float oceanFarPlane=750000f;


		//Debug variables
//		public float[] additionalScales=new float[10];
		public bool[] debugSettings= new bool[10];
//		public int renderQueue=2000;
//		int renderQueue2=2010;
		
		//postprocessing properties
//		float inscatteringCoeff=85f; //useless, removed from shader
//		float extinctionCoeff=70f;   //obsolete
//		float postProcessScale=1000f;
		float postProcessingalpha=78f;
		float postProcessDepth=200f;
		float postProcessExposure=18f;
//		float apparentDistance=1000f;


		//sky properties
		float exposure = 25f;
		float alphaGlobal=100f;
		float mapExposure = 15f;
		float mapAlphaGlobal=100f;


		public Camera chosenCamera;
		public int layer=15;
		int cam=1;
		
		//other stuff
		float atmosphereGlobalScale=1000f;
		float m_radius;// = 600000.0f;
		String parentPlanet="Kerbin";
		int PlanetId;
		int SunId;				
		public CelestialBody[] celestialBodies;	
		Manager m_manager;
		bool depthbufferEnabled=false;
		bool isActive;

		Material originalMaterial;
		
		
		public Transform GetScaledTransform(string body)
		{
			List<Transform> transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single(n => n.name == body);
		}
		
		internal override void Awake()
		{
			WindowCaption = "Scatterer v0.0175: alt+f10/f11 toggle";
			WindowRect = new Rect(0, 0, 300, 50);
			Visible = true;						
			isActive = false;

			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene==GameScenes.SPACECENTER )

			{
				isActive = true;
			
//				cfg.load ();
//				parentPlanet =cfg.GetValue<string>("Planet");
//				atmosphereGlobalScale=float.Parse(cfg.GetValue<string>("atmosphereGlobalScale"))*1000f;
//				
//				//find sun and parent planet
//				celestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType(typeof(CelestialBody));
//				PlanetId =0;
//				SunId =0;
//				
//				//Transform transform; // this next bit finds kerbin and the sun, and sets all the plaents to renderqueue 2002
//				//so they don't get clipped over by the amospheres (renderqueue 2001)
//				
//				for (int k=0; k< celestialBodies.Length ; k++)
//				{
//					//						transform = GetScaledTransform (celestialBodies[k].name);													
//					//						{
//					//							mr = (MeshRenderer)transform.GetComponent (typeof(MeshRenderer));
//					//							if (mr != null)
//					//							{															
//					//								mr.material.renderQueue=2002;
//					//							}
//					//						}										
//					
//					if (celestialBodies[k].GetName() == parentPlanet)
//						PlanetId=k;
//					
//					if (celestialBodies[k].GetName() == "Sun")
//						SunId=k;
//					//					mr.enabled=false;
//				}
//				
//				m_manager=new Manager();
//				m_manager.setParentCelestialBody(celestialBodies[PlanetId]);
//				m_manager.setSunCelestialBody(celestialBodies[SunId]);
//				m_manager.SetCore(this);
//				m_manager.Awake();
//				getSettingsFromSkynode();
//				loadConfigPoint(selectedConfigPoint);
//				
//				
//				
//				m_radius = (float)celestialBodies [PlanetId].Radius;
//				backupAtmosphereMaterial(parentPlanet);

			}					
		}
		
		
		
		internal override void Update()
		{			
			//toggle whether GUI is visible or not
			if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			    && (Input.GetKeyDown(KeyCode.F11)||(Input.GetKeyDown(KeyCode.F10))))
				Visible = !Visible;

			if (isActive)
			{
				updateCnt++;
				if (updateCnt>5){

				if (!found) {

					
					//debug settings
//									for (int j=0;j<10;j++){
//										debugSettings[j]=false;
//									}
					
					//read parent planet from config
					cfg.load ();
					parentPlanet =cfg.GetValue<string>("Planet");
					atmosphereGlobalScale=float.Parse(cfg.GetValue<string>("atmosphereGlobalScale"))*1000f;
					
					//find sun and parent planet
					celestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType(typeof(CelestialBody));
					PlanetId =0;
					SunId =0;
					
					for (int k=0; k< celestialBodies.Length ; k++)
					{									
						if (celestialBodies[k].GetName() == parentPlanet)
							PlanetId=k;
						
						if (celestialBodies[k].GetName() == "Sun")
							SunId=k;
					}
					
					if (PlanetId==0)
						print("parentPlanet not found");
					else
						found=true;
				}

					if (!initiated && found) {
					
					//create and configure manager
					m_manager=new Manager();
					m_manager.setParentCelestialBody(celestialBodies[PlanetId]);
					m_manager.setSunCelestialBody(celestialBodies[SunId]);
					m_manager.SetCore(this);
					m_manager.Awake();
					getSettingsFromSkynode();
					loadConfigPoint(selectedConfigPoint);
					
					m_radius = (float)celestialBodies [PlanetId].Radius;
					backupAtmosphereMaterial(parentPlanet);
					tweakStockAtmosphere(parentPlanet,rimBlend,rimpower);
//
//				if (!initiated){
					cams = Camera.allCameras;
					count = Camera.allCameras.Length;
					initiated=true;
				}
					
				if(initiated)
				m_manager.Update ();

			}
		}
		}
			
		void OnGUI()
		{
			//debugging for rendertextures, not needed anymore but might be when I implement oceans
			//	GUI.DrawTexture(new Rect(250,250,512,512), m_transmit, ScaleMode.StretchToFill, false);
			//	GUI.DrawTexture(new Rect(250,250,512,512), RenderTexture.active, ScaleMode.StretchToFill, false);
		}

		internal override void OnDestroy()
		{
//			base.OnDestroy ();
//			ReactivateAtmosphere(parentPlanet,rimBlend,rimpower);
//			m_manager.m_skyNode.OnDestroy ();
//			Destroy(m_manager.m_skyNode);
			m_manager.OnDestroy ();
			Destroy (m_manager);
			ReactivateAtmosphere (parentPlanet);

		}
		

//		UI BUTTONS
//		This isn't the most elegant section due to how much code is necessary for each element
		internal override void DrawWindow(int id)
		{
			DragEnabled = true;
			
			if (!isActive)
			GUILayout.Label (String.Format ("Mod will activate in KSC view or in flight."));

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Hide"))
				Visible = !Visible;
			GUILayout.EndHorizontal ();

			if (isActive)
			{
			
				GUILayout.BeginHorizontal ();
				
				if (GUILayout.Button ("Toggle depth buffer"))
				{
					if (!depthbufferEnabled)
					{
						cams[2].gameObject.AddComponent (typeof(ViewDepthBuffer));
						depthbufferEnabled = true;
					}
					else
					{
						Component.Destroy (cams [2].gameObject.GetComponent<ViewDepthBuffer> ());
						depthbufferEnabled = false;
					}
				}
				
				if (GUILayout.Button ("Toggle PostProcessing"))
				{
					
					if (!m_manager.m_skyNode.postprocessingEnabled)
					{
						m_manager.m_skyNode.enablePostprocess ();
					}
					else
					{
						m_manager.m_skyNode.disablePostprocess ();
					}
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("New point altitude:");
				newCfgPtAlt = (float)(Convert.ToDouble (GUILayout.TextField (newCfgPtAlt.ToString ())));
				if (GUILayout.Button ("Add"))
				{
					m_manager.m_skyNode.configPoints.Insert(selectedConfigPoint+1,new configPoint(newCfgPtAlt,alphaGlobal/100,exposure/100,postProcessingalpha/100,postProcessDepth/10000,postProcessExposure/100));
					selectedConfigPoint+=1;
					configPointsCnt=m_manager.m_skyNode.configPoints.Count;
					loadConfigPoint(selectedConfigPoint);
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Config point:");

				if (GUILayout.Button ("<"))
				{
					if (selectedConfigPoint>0){
						selectedConfigPoint-=1;
						loadConfigPoint(selectedConfigPoint);
					}
				}

				GUILayout.TextField ((selectedConfigPoint).ToString ());

				if (GUILayout.Button (">"))
				{
					if (selectedConfigPoint<configPointsCnt-1)
					{
						selectedConfigPoint+=1;
						loadConfigPoint(selectedConfigPoint);
					}
				}

				//GUILayout.Label (String.Format("Total:{0}", configPointsCnt));
				if (GUILayout.Button ("Delete"))
				{
					if (configPointsCnt<=1)
						print ("Can't delete config point, one or no points remaining");
					else
					{
						m_manager.m_skyNode.configPoints.RemoveAt(selectedConfigPoint);
						if (selectedConfigPoint>=configPointsCnt-1)
						{
							selectedConfigPoint=configPointsCnt-2;
						}
						configPointsCnt=m_manager.m_skyNode.configPoints.Count;
						loadConfigPoint(selectedConfigPoint);
					}
					
				}

				GUILayout.EndHorizontal ();


				
				if (!MapView.MapIsEnabled){
				
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Sky Settings");
//				GUILayout.EndHorizontal ();
									
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Point altitude");
				pointAltitude = (float)(Convert.ToDouble (GUILayout.TextField (pointAltitude.ToString ())));
					
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.configPoints[selectedConfigPoint].altitude=pointAltitude;
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Sky/orbit Alpha (/100)");
				alphaGlobal = (float)(Convert.ToDouble (GUILayout.TextField (alphaGlobal.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
						m_manager.m_skyNode.configPoints[selectedConfigPoint].skyAlpha=alphaGlobal/100f;
				}
				GUILayout.EndHorizontal ();
												
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Sky/orbit Exposure (/100)");
				exposure = (float)(Convert.ToDouble (GUILayout.TextField (exposure.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
						m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExposure=exposure / 100f;
				}
				GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Post Processing Alpha (/100)");
					postProcessingalpha = (float)(Convert.ToDouble (GUILayout.TextField (postProcessingalpha.ToString ())));
					
					if (GUILayout.Button ("Set"))
					{
						m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessAlpha=postProcessingalpha/100f;
					}
					GUILayout.EndHorizontal ();
					
					
					
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Post Processing Depth (/10000)");
					postProcessDepth = (float)(Convert.ToDouble (GUILayout.TextField (postProcessDepth.ToString ())));
					
					if (GUILayout.Button ("Set"))
					{
						m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessDepth=postProcessDepth /10000f;
					}
					GUILayout.EndHorizontal ();
					
					
					
					GUILayout.BeginHorizontal ();
					GUILayout.Label ("Post Processing Exposure (/100)");
					postProcessExposure = (float)(Convert.ToDouble (GUILayout.TextField (postProcessExposure.ToString ())));
					
					if (GUILayout.Button ("Set"))
					{
						m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessExposure = postProcessExposure /100f;
					}
					GUILayout.EndHorizontal ();
				
				}

				else
				{

//					GUILayout.BeginHorizontal ();
//					GUILayout.Label ("Sky Settings (map view)");
//					GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Map view alpha (/100)");
				mapAlphaGlobal = (float)(Convert.ToDouble (GUILayout.TextField (mapAlphaGlobal.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.mapAlphaGlobal = mapAlphaGlobal / 100f;
				}
				GUILayout.EndHorizontal ();
												
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Map view exposure (/100)");
				mapExposure = (float)(Convert.ToDouble (GUILayout.TextField (mapExposure.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.mapExposure = mapExposure / 100f;
				}
				GUILayout.EndHorizontal ();
				}

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Post Processing Settings");
//				GUILayout.EndHorizontal ();
			
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("atmosphereGlobalScale (/1000)");
//				
//				
//				atmosphereGlobalScale = (float)(Convert.ToDouble (GUILayout.TextField (atmosphereGlobalScale.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.SetAtmosphereGlobalScale (atmosphereGlobalScale / 1000);
//				}
//				GUILayout.EndHorizontal ();



//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Terrain reflectance (/100)");
//				
//				
//				terrainReflectance = (float)(Convert.ToDouble (GUILayout.TextField (terrainReflectance.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.terrainReflectance=terrainReflectance/100f;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Sun Intensity");
//				
//				
//				sunIntensity = (float)(Convert.ToDouble (GUILayout.TextField (sunIntensity.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.sunIntensity = sunIntensity;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Irradiance Factor /100");
//				
//				
//				irradianceFactor = (float)(Convert.ToDouble (GUILayout.TextField (irradianceFactor.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.irradianceFactor = irradianceFactor / 100f;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Ocean Threshold");
//								
//				oceanThreshold = (float)(Convert.ToDouble (GUILayout.TextField (oceanThreshold.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode._Ocean_Threshold = oceanThreshold;
//				}
//				GUILayout.EndHorizontal ();
//
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Ocean sigma");
//				
//				
//				oceanSigma = float.Parse(GUILayout.TextField(oceanSigma.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.oceanSigma = oceanSigma;
//				}
//				GUILayout.EndHorizontal ();


//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Ocean Near plane");
//				
//				
//				oceanNearPlane = float.Parse(GUILayout.TextField(oceanNearPlane.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.oceanNearPlane = oceanNearPlane;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Ocean far plane");
////				
////				
//				oceanFarPlane = float.Parse(GUILayout.TextField(oceanFarPlane.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.oceanFarPlane = oceanFarPlane;
//				}
//				GUILayout.EndHorizontal ();
//
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("THETA");
//				
//				
//				theta = float.Parse(GUILayout.TextField(theta.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.GetOceanNode().theta = theta;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("PHI");
//				
//				
//				phi = float.Parse(GUILayout.TextField(phi.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.GetOceanNode().phi = phi;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("ocean Level");
//				
//				
//				oceanLevel = float.Parse(GUILayout.TextField(oceanLevel.ToString("00000000.0000000")));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.GetOceanNode().m_oceanLevel = oceanLevel;
//				}
//				GUILayout.EndHorizontal ();




//				for (int j=0;j<10;j++){
//				GUILayout.BeginHorizontal ();
//				GUILayout.Label (String.Format("Debug setting:{0}", j.ToString()));	
//				GUILayout.TextField(debugSettings[j].ToString());
//				
//				if (GUILayout.Button ("Toggle"))
//				{
//						debugSettings[j] = !debugSettings[j];
//				}
////					m_manager.m_skyNode.debugSettings[j]=debugSettings[j];
//				GUILayout.EndHorizontal ();
//				}

//				GUILayout.BeginHorizontal ();
//				GUILayout.Label ("Apparent distance (/10000)");
//				apparentDistance = (float)(Convert.ToDouble (GUILayout.TextField (apparentDistance.ToString ())));
//				
//				if (GUILayout.Button ("Set"))
//				{
//					m_manager.m_skyNode.apparentDistance= apparentDistance/1000f;
//				}
//				GUILayout.EndHorizontal ();

//				GUILayout.BeginHorizontal ();
//
//				if (GUILayout.Button ("Destroy tester"))
//				{
//					m_manager.m_skyNode.destroyTester();
//				}
//				GUILayout.EndHorizontal ();



				GUILayout.BeginHorizontal ();

				GUILayout.Label (String.Format("ForceOFF aniso"));	
				GUILayout.TextField(m_manager.m_skyNode.forceOFFaniso.ToString());
						
				if (GUILayout.Button ("Toggle"))
				{
					m_manager.m_skyNode.toggleAniso();
				}
				GUILayout.EndHorizontal ();

				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("RimBlend");
				rimBlend = (float)(Convert.ToDouble (GUILayout.TextField (rimBlend.ToString ())));
				
				GUILayout.Label ("RimPower");
				rimpower = (float)(Convert.ToDouble (GUILayout.TextField (rimpower.ToString ())));

				if (GUILayout.Button ("Set"))
				{
					tweakStockAtmosphere(parentPlanet,rimBlend,rimpower);
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				GUILayout.Label ("extinctionMultiplier (/100)");
				extinctionMultiplier = (float)(Convert.ToDouble (GUILayout.TextField (extinctionMultiplier.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.extinctionMultiplier=extinctionMultiplier /100f;
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Save settings"))
				{
					m_manager.m_skyNode.rimBlend=rimBlend;
					m_manager.m_skyNode.rimpower=rimpower;
					m_manager.m_skyNode.displayInterpolatedVariables=showInterpolatedValues;
					m_manager.m_skyNode.saveToConfigNode();
				}
				
				if (GUILayout.Button ("Load settings"))
				{
					m_manager.m_skyNode.loadFromConfigNode();
					getSettingsFromSkynode();
					loadConfigPoint(selectedConfigPoint);
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Display interpolated values"))
				{
					showInterpolatedValues=!showInterpolatedValues;
				}
				GUILayout.EndHorizontal ();


				if (showInterpolatedValues)
				{
					GUILayout.BeginHorizontal ();
					if (m_manager.m_skyNode.currentConfigPoint == 0)
						GUILayout.Label ("Current state:Ground, cfgPoint 0");
					else if (m_manager.m_skyNode.currentConfigPoint >= configPointsCnt-1)
						GUILayout.Label (String.Format ("Current state:Orbit, cfgPoint{0}", m_manager.m_skyNode.currentConfigPoint));
					else
						GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100*(1-m_manager.m_skyNode.percentage)),m_manager.m_skyNode.currentConfigPoint-1,(int)(100*m_manager.m_skyNode.percentage),m_manager.m_skyNode.currentConfigPoint));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label (String.Format ("SkyAlpha: {0} ", (int) (100*m_manager.m_skyNode.alphaGlobal)));
					GUILayout.Label (String.Format ("SkyExposure: {0}",(int) (100*m_manager.m_skyNode.m_HDRExposure)));
					GUILayout.EndHorizontal ();

					GUILayout.BeginHorizontal ();
					GUILayout.Label (String.Format ("PostAlpha: {0}", (int) (100*m_manager.m_skyNode.postProcessingAlpha)));
					GUILayout.Label (String.Format ("PostDepth: {0}", (int) (10000*m_manager.m_skyNode.postProcessDepth)));
					GUILayout.Label (String.Format ("PostExposure: {0}", (int) (100*m_manager.m_skyNode.postProcessExposure)));
					GUILayout.EndHorizontal ();
				}


				GUILayout.BeginHorizontal ();
				GUILayout.Label ("ManagerState");
				GUILayout.TextField (m_manager.getManagerState ());
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Disable stock atmo"))
				{					
					DeactivateAtmosphere(parentPlanet);
				}

				if (GUILayout.Button ("Enable stock atmo"))
				{					
					ReactivateAtmosphere(parentPlanet);
				}
				GUILayout.EndHorizontal ();


				GUILayout.BeginHorizontal ();
				
//				if (GUILayout.Button ("Toggle stock ocean"))
//				{
//					stockOcean=!stockOcean;
//				}

				if (GUILayout.Button ("Toggle stock sunglare"))
				{
					stockSunglare =!stockSunglare;
				}

				if (GUILayout.Button ("Toggle extinction"))
				{
					extinctionEnabled =!extinctionEnabled;
				}

				GUILayout.EndHorizontal ();

				chosenCamera=cams[cam];
			
			
			}
	}


		//snippet by Thomas P. from KSPforum
		public void DeactivateAtmosphere(string name)
		{
			Transform t = ScaledSpace.Instance.transform.FindChild(name);
			
			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i).gameObject.layer == 9)
				{
					// Deactivate the Athmosphere-renderer
					t.GetChild(i).gameObject.GetComponent<MeshRenderer>().gameObject.SetActive(false);
					
					// Reset the shader parameters
					Material sharedMaterial = t.renderer.sharedMaterial;

					//sharedMaterial.SetTexture(Shader.PropertyToID("_rimColorRamp"), null);
//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), 0);
//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), 0);
					
					// Stop our script
					i = t.childCount + 10;
				}
			}
		}



			public void getSettingsFromSkynode() {

			postProcessingalpha = 100 * m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessAlpha;
			postProcessDepth = 10000 * m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessDepth;
			postProcessExposure = 100* m_manager.m_skyNode.configPoints[selectedConfigPoint].postProcessExposure;
			exposure = 100* m_manager.m_skyNode.configPoints[selectedConfigPoint].skyExposure;
			alphaGlobal = 100 * m_manager.m_skyNode.configPoints[selectedConfigPoint].skyAlpha;

			mapAlphaGlobal = 100 * m_manager.m_skyNode.mapAlphaGlobal;
			mapExposure = 100* m_manager.m_skyNode.mapExposure;
			configPointsCnt = m_manager.m_skyNode.configPoints.Count;

			rimBlend = m_manager.m_skyNode.rimBlend;
			rimpower = m_manager.m_skyNode.rimpower;

			showInterpolatedValues = m_manager.m_skyNode.displayInterpolatedVariables;
		}


		public void backupAtmosphereMaterial(string name)
		{
			Transform t = ScaledSpace.Instance.transform.FindChild(name);
			
			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i).gameObject.layer == 9)
				{
					t.GetChild(i).gameObject.GetComponent<MeshRenderer>().gameObject.SetActive(true);
					originalMaterial = (Material) Material.Instantiate(t.renderer.sharedMaterial);
					i = t.childCount + 10;
				}
			}
		}


		public void ReactivateAtmosphere(string name)
		{
			Transform t = ScaledSpace.Instance.transform.FindChild(name);
			
			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i).gameObject.layer == 9)
				{
					t.GetChild(i).gameObject.GetComponent<MeshRenderer>().gameObject.SetActive(true);
					t.renderer.sharedMaterial = originalMaterial;
					i = t.childCount + 10;
				}
			}
		}


		public void tweakStockAtmosphere(string name, float inRimBlend, float inRimPower)
		{
			Transform t = ScaledSpace.Instance.transform.FindChild(name);
			
			for (int i = 0; i < t.childCount; i++)
			{
				if (t.GetChild(i).gameObject.layer == 9)
				{
					t.GetChild(i).gameObject.GetComponent<MeshRenderer>().gameObject.SetActive(false);
					Material sharedMaterial = t.renderer.sharedMaterial;
					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), inRimBlend/100f);
					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), inRimPower/100f);
					i = t.childCount + 10;
				}
			}
		}

		public void savePlanetsList() {

			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);

			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this);
			cnTemp.Save(path+"/config/PlanetsList.txt");
		}

		public void loadPlanetsList() {
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);

			ConfigNode cnToLoad = ConfigNode.Load(path+"/config/PlanetsList.txt");
			ConfigNode.LoadObjectFromConfig(this, cnToLoad);
		}

		public void loadConfigPoint(int point)
		{
			postProcessDepth = m_manager.m_skyNode.configPoints [point].postProcessDepth * 10000f;
			postProcessExposure = m_manager.m_skyNode.configPoints [point].postProcessExposure*100f;
			postProcessingalpha = m_manager.m_skyNode.configPoints [point].postProcessAlpha * 100f;

			alphaGlobal = m_manager.m_skyNode.configPoints [point].skyAlpha * 100f;
			exposure = m_manager.m_skyNode.configPoints [point].skyExposure*100f;

			pointAltitude = m_manager.m_skyNode.configPoints [point].altitude;
		}
	}
}