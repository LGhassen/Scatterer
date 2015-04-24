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

		MeshRenderer mr = new MeshRenderer ();

		//bool[] debugSettings= new bool[5];
		
		//postprocessing properties
		//float inscatteringCoeff=85f; //useless, removed from shader too
		float extinctionCoeff=70f;
		float postProcessingalpha=78f;
		float postProcessDepth=150f;
		float postProcessScale=1000f;
		float postProcessExposure=18f;

		int renderQueue=2000;
		int renderQueue2=2010;


		float apparentDistance=1000f;

		//sky properties
		float exposure = 20f;
		float alphaGlobal=100f;
		int layer=15;
		int cam=1;
		
		//other stuff
		float atmosphereGlobalScale=1000f;
		float m_radius;// = 600000.0f;
		String parentPlanet="Kerbin";
		int PlanetId;
		int SunId;				
		CelestialBody[] celestialBodies;	
		Manager m_manager;
		bool depthbufferEnabled=false;
		bool isActive;
		
		
		public Transform GetScaledTransform(string body)
		{
			List<Transform> transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single(n => n.name == body);
		}
		
		internal override void Awake()
		{
			WindowCaption = "Scatterer mod: alt+f11 toggle";
			WindowRect = new Rect(0, 0, 300, 50);
			Visible = true;						
			isActive = false;
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene==GameScenes.SPACECENTER )

			{
				isActive = true;

//				for (int j=0;j<5;j++){
//					debugSettings[j]=false;
//				}
				
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
				
				m_manager=new Manager();
				m_manager.setParentCelestialBody(celestialBodies[PlanetId]);
				m_manager.setSunCelestialBody(celestialBodies[SunId]);
				m_manager.Awake();
				
				m_radius = (float)celestialBodies [PlanetId].Radius;								
			}					
		}
		
		
		
		internal override void Update()
		{			
			//toggle whether its visible or not
			if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
				Visible = !Visible;
			if (isActive)
			{
				m_manager.Update ();
			}
		}
		
		void OnGUI()
		{
			//debugging for rendertextures, not needed anymore but might be when I implement oceans
			//	GUI.DrawTexture(new Rect(250,250,512,512), m_transmit, ScaleMode.StretchToFill, false);
			//	GUI.DrawTexture(new Rect(250,250,512,512), RenderTexture.active, ScaleMode.StretchToFill, false);
		}
		
		internal override void DrawWindow(int id)
		{
			
			
			DragEnabled = true;
			
			//			GUILayout.BeginHorizontal();
			//			GUILayout.Label(String.Format("Drag Enabled:{0}",DragEnabled.ToString()));
			//			if (GUILayout.Button("Toggle Drag"))
			//            DragEnabled = !DragEnabled;
			//			GUILayout.EndHorizontal();
			
			
			GUILayout.Label (String.Format ("In game:{0}", isActive.ToString ()));
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Hide"))
				Visible = !Visible;
			GUILayout.EndHorizontal ();
			
			Camera[] cams = Camera.allCameras;
			int count = Camera.allCameras.Length;
			
			
			if (isActive)
			{
				//setting up lots of properties here, not the most elegant way to do it
				//but since the GUI is just for testing It'll remain here for now
				
				
				//CAM DEBUG OPTIONS								
								GUILayout.Label(String.Format("Number of cams:{0}",count.ToString()));
								GUILayout.Label (String.Format ("cam1pos:{0}", cams [0].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam1Name:{0}", cams [0].name));
								GUILayout.Label (String.Format ("cam2pos:{0}", cams [1].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam2pos:{0}", cams [1].name));
								GUILayout.Label (String.Format ("cam3pos:{0}", cams [2].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam3pos:{0}", cams [2].name));
								GUILayout.Label (String.Format ("cam4pos:{0}", cams [3].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam4pos:{0}", cams [3].name));
								GUILayout.Label (String.Format ("cam5pos:{0}", cams [4].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam5pos:{0}", cams [4].name));
								GUILayout.Label (String.Format ("cam6pos:{0}", cams [5].transform.position.ToString ()));
								GUILayout.Label (String.Format ("cam6pos:{0}", cams [5].name));
				
								if (Camera.allCameras.Length == 7) {
									GUILayout.Label (String.Format ("cam7pos:{0}", cams [6].transform.position.ToString ()));
									GUILayout.Label (String.Format ("cam7pos:{0}", cams [6].name));
								}															
				//END CAM DEBUG OPTIONS
				
				
				DeactivateAtmosphere (parentPlanet);
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Layer");
				layer = Convert.ToInt32 (GUILayout.TextField (layer.ToString ()));
				if (GUILayout.Button ("+"))
					layer = layer + 1;
				
				if (GUILayout.Button ("-"))
					layer = layer - 1;
				
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Cam");
				cam = Convert.ToInt32 (GUILayout.TextField (cam.ToString ()));
				
				if (GUILayout.Button ("+"))
					cam = cam + 1;
				
				if (GUILayout.Button ("-"))
					cam = cam - 1;
				
				GUILayout.EndHorizontal ();
				
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Alpha Global (/100)");
				alphaGlobal = (float)(Convert.ToDouble (GUILayout.TextField (alphaGlobal.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetAlphaGlobal (alphaGlobal / 100);
				}
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Exposure (/100)");
				exposure = (float)(Convert.ToDouble (GUILayout.TextField (exposure.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetExposure (exposure / 100);
				}
				GUILayout.EndHorizontal ();
				
				
				
				m_manager.m_skyNode.setLayernCam (layer, cam);
				
				GUILayout.BeginHorizontal ();
				
				if (GUILayout.Button ("Toggle depth buffer"))
				{
					if (!depthbufferEnabled)
					{
						cams [cam].gameObject.AddComponent (typeof(ViewDepthBuffer));
						depthbufferEnabled = true;
					}
					else
					{
						Component.Destroy (cams [cam].gameObject.GetComponent<ViewDepthBuffer> ());
						depthbufferEnabled = false;
					}
				}
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				
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
				GUILayout.Label ("Post Processing Alpha (/100)");
				postProcessingalpha = (float)(Convert.ToDouble (GUILayout.TextField (postProcessingalpha.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetPostProcessAlpha (postProcessingalpha / 100);
				}
				GUILayout.EndHorizontal ();
				
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Post Processing Depth (/10000)");
				postProcessDepth = (float)(Convert.ToDouble (GUILayout.TextField (postProcessDepth.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetPostProcessDepth (postProcessDepth / 10000);
				}
				GUILayout.EndHorizontal ();
				
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Post Processing Exposure (/100)");
				postProcessExposure = (float)(Convert.ToDouble (GUILayout.TextField (postProcessExposure.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetPostProcessExposure (postProcessExposure / 100);
				}
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Post Processing Scale (/1000)");
				postProcessScale = (float)(Convert.ToDouble (GUILayout.TextField (postProcessScale.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetPostProcessScale (postProcessScale / 1000);
				}
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Planet radius (display only)");
				GUILayout.TextField (celestialBodies [PlanetId].Radius.ToString ());
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("atmosphereGlobalScale (/1000)");
				
				
				atmosphereGlobalScale = (float)(Convert.ToDouble (GUILayout.TextField (atmosphereGlobalScale.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetAtmosphereGlobalScale (atmosphereGlobalScale / 1000);
				}
				GUILayout.EndHorizontal ();
				
				
				
				//				GUILayout.BeginHorizontal ();
				//				GUILayout.Label ("Inscattering Coeff (/100)");
				//				inscatteringCoeff = (float)(Convert.ToDouble (GUILayout.TextField (inscatteringCoeff.ToString ())));
				//				
				//				if (GUILayout.Button ("Set"))
				//				{
				//					m_manager.m_skyNode.SetInscatteringCoeff (inscatteringCoeff / 100);
				//				}
				//				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Extinction Coeff (/100)");
				
				
				extinctionCoeff = (float)(Convert.ToDouble (GUILayout.TextField (extinctionCoeff.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.SetExtinctionCoeff (extinctionCoeff / 100);
				}
				GUILayout.EndHorizontal ();


				for (int j=0;j<7;j++){
				GUILayout.BeginHorizontal ();
				GUILayout.Label (String.Format("Debug setting:{0}", j.ToString()));	
				GUILayout.TextField(m_manager.m_skyNode.debugSettings[j].ToString());
				
				if (GUILayout.Button ("Toggle"))
				{
						m_manager.m_skyNode.debugSettings[j] = !m_manager.m_skyNode.debugSettings[j];
				}
				GUILayout.EndHorizontal ();
				}

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Apparent distance (/10000)");
				apparentDistance = (float)(Convert.ToDouble (GUILayout.TextField (apparentDistance.ToString ())));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.apparentDistance= apparentDistance/1000f;
				}
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("RenderQueue");
				renderQueue = Convert.ToInt32 (GUILayout.TextField (renderQueue.ToString ()));
				
				if (GUILayout.Button ("Set"))
				{
					m_manager.m_skyNode.renderQueue= renderQueue;
				}
				GUILayout.EndHorizontal ();



				if (mr==null){
				//								//Snippet from RbRay's EVE
													Transform transform = GetScaledTransform (parentPlanet);													
													{
														mr = (MeshRenderer)transform.GetComponent (typeof(MeshRenderer));
														if (mr != null)
														{														
															print ("planet shader: " + mr.material.shader);	
															print("RENDER QUEUE"+mr.material.renderQueue);
														}
													}										
				}




				GUILayout.BeginHorizontal ();
				GUILayout.Label ("RenderQueue Kerbin");
				renderQueue2 = Convert.ToInt32 (GUILayout.TextField (renderQueue2.ToString ()));
				
				if (GUILayout.Button ("Set"))
				{
					mr.material.renderQueue = renderQueue2;
				}
				GUILayout.EndHorizontal ();

				print("KERBIN RENDER QUEUE"+mr.material.renderQueue);
				
								
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("ManagerState");
				GUILayout.TextField (m_manager.getManagerState ());
				GUILayout.EndHorizontal ();
				
				
//								GUILayout.BeginHorizontal ();
//								//Snippet from RbRay's EVE
//								if (GUILayout.Button ("Get Planet shader"))
//								{
//									Transform transform = GetScaledTransform (parentPlanet);
//									
//									PQS pqs = celestialBodies [PlanetId].pqsController;
//									
//									if (pqs != null)
//									{
//										MeshRenderer mr = (MeshRenderer)transform.GetComponent (typeof(MeshRenderer));
//										if (mr != null)
//										{														
//											print ("planet shader: " + mr.material.shader);														
//										}
//									}										
//								}
//								GUILayout.EndHorizontal ();
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
					//sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), 0);
					//sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), 0);
					
					// Stop our script
					i = t.childCount + 10;
				}
			}
		}
		
		
	}
}