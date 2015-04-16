using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using KSP;
using UnityEngine;

using KSPPluginFramework;


namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class Core : MonoBehaviourWindow
    {
		float postProcessingalpha=95f;
		float postProcessDepth=100f;
		float postProcessScale=100f;
		float postProcessExposure=18f;
		int nearPlane=299;
		int farPlane=75000;

		bool postprocessingEnabled=false;
		bool depthbufferEnabled=false;
		float exposure = 20f;
		int atmosphereToDisable=0;
		float alphaCutoff=0f;
		float alphaGlobal=100f;
		int layer=15;
		int cam=1;
		//float m_radius = 63600.0f*4;
		float m_radius;// = 600000.0f;
//float m_radius = 127200.0f;
		RenderTexture m_transmit;
		int bodyNumber;
		int KerbinId;
		int SunId;
		bool isActive;
		CelestialBody[] celestialBodies;
		Vessel playerVessel;
		SunNode idek;

		Manager m_manager;

        internal override void Awake()
        {
            WindowCaption = "Scatterer mod: alt+f11 toggle";
            WindowRect = new Rect(0, 0, 300, 50);
            Visible = true;
			bodyNumber=0;

			isActive = false;
			
			if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene==GameScenes.SPACECENTER ) {
				isActive = true;

				celestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType(typeof(CelestialBody));
				KerbinId =0;
				SunId =0;
				for (int k=0; k< celestialBodies.Length ;k++){
					if (celestialBodies[k].GetName() == "Kerbin")
						KerbinId=k;
					
					if (celestialBodies[k].GetName() == "Sun")
						SunId=k;
				}
								
				playerVessel=FlightGlobals.ActiveVessel;
								


				m_manager=new Manager();
				m_manager.setParentCelestialBody(celestialBodies[KerbinId]);
				m_manager.setSunCelestialBody(celestialBodies[SunId]);
				m_manager.Awake();

				m_radius = (float)celestialBodies [KerbinId].Radius;
			}


		}

        internal override void Update()
        {

            //toggle whether its visible or not
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetKeyDown(KeyCode.F11))
                Visible = !Visible;
			if (isActive) {
				m_manager.Update ();
				//m_transmit=m_manager.getInscatter();

				/*Shader[] shaderList =Resources.FindObjectsOfTypeAll<Shader> ();
				
				//Log.Normal("{0} loaded shaders", shaders.Count);
				//List<string> sorted = new List<string>(shaders); sorted.Sort();
				
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/shaders.txt"))
					//foreach (var sh in sorted)
					for (int i=0;i<shaderList.Length;i++)
				{
					file.WriteLine(shaderList[i].ToString());
					file.WriteLine(shaderList[i].isSupported.ToString());
				}*/

			}
        }

		void OnGUI(){
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
			

			GUILayout.Label(String.Format("In game:{0}",isActive.ToString()));
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Hide"))
			Visible = !Visible;
			GUILayout.EndHorizontal();

			Camera[] cams = Camera.allCameras;
			//int count = Camera.allCameras.Length;


			if (isActive) {

				//CAM DEBUG OPTIONS


				/*GUILayout.Label(String.Format("Number of cams:{0}",count.ToString()));
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

				 */


				//END CAM DEBUG OPTIONS




				
				/*GUILayout.Label (String.Format ("Kerbin position:{0}", getKerbinTransform().position));
			GUILayout.Label (String.Format ("Kerbin position to scaled space:{0}", ScaledSpace.LocalToScaledSpace(getKerbinTransform().position)));

			Vessel playerVessel=FlightGlobals.ActiveVessel;

			GUILayout.Label (String.Format ("Playervesselposition:{0}", playerVessel.transform.position));
			GUILayout.Label (String.Format ("Player vessel to scaled space:{0}", ScaledSpace.LocalToScaledSpace(playerVessel.transform.position)));*/
				
				
				



				/*GUILayout.Label (String.Format ("Number of bodies:{0}", getBodiesNumber()));
				GUILayout.Label (String.Format ("body 1:{0}", getBodyName (0)));
				GUILayout.Label (String.Format ("body 2:{0}", getBodyName (1)));
				GUILayout.BeginHorizontal();
				GUILayout.Label("Body number to display name");

				bodyNumber=Convert.ToInt32(GUILayout.TextField(bodyNumber.ToString()));
				GUILayout.EndHorizontal();
				GUILayout.Label (String.Format ("body name:{0}", getBodyName (bodyNumber)));*/

				DeactivateAtmosphere("Kerbin");

				GUILayout.BeginHorizontal();
				GUILayout.Label("Layer");				
				layer=Convert.ToInt32(GUILayout.TextField(layer.ToString()));
				if(GUILayout.Button("+"))
					layer = layer+1;

				if(GUILayout.Button("-"))
					layer = layer-1;

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Cam");				
				cam=Convert.ToInt32(GUILayout.TextField(cam.ToString()));

				if(GUILayout.Button("+"))
					cam = cam+1;
				
				if(GUILayout.Button("-"))
					cam = cam-1;

				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

							
				//m_radius=(float)(Convert.ToDouble(GUILayout.TextField(m_radius.ToString())));

				if(GUILayout.Button("transform from cam 0 to cam 1"))
				{
					m_manager.toggleCamTransform();
				}
				if(GUILayout.Button("CamToggle1"))
				{
					m_manager.toggleCam(1);
				}
				
				if(GUILayout.Button("CamToggle2"))
				{
					m_manager.toggleCam(2);
				}

				GUILayout.EndHorizontal();



				GUILayout.BeginHorizontal();

				if(GUILayout.Button("CamToggle3"))
				{
					m_manager.toggleCam(3);
				}
				if(GUILayout.Button("CamToggle4"))
				{
					m_manager.toggleCam(4);
				}

				if(GUILayout.Button("CamToggle5"))
				{
					m_manager.toggleCam(5);
				}

				if(GUILayout.Button("CamToggle6"))
				{
					m_manager.toggleCam(6);
				}




				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Alpha Cutoff (/100)");		
				alphaCutoff=(float)(Convert.ToDouble(GUILayout.TextField(alphaCutoff.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetAlphaCutoff(alphaCutoff/100);
				}							

				if(GUILayout.Button("Disable Atmo"))
				{
					DeactivateAtmosphere("Kerbin");
				}
				GUILayout.EndHorizontal();

			
				GUILayout.BeginHorizontal();

				if(GUILayout.Button("+"))
					atmosphereToDisable++;
				if(GUILayout.Button("-"))
					atmosphereToDisable=atmosphereToDisable-1;
				
				GUILayout.TextField(atmosphereToDisable.ToString());
				
				if(GUILayout.Button("Disable atmo layer"))
					deactivateSpecificAtmosphere("Kerbin", atmosphereToDisable);
				


				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Alpha Global (/100)");		
				alphaGlobal=(float)(Convert.ToDouble(GUILayout.TextField(alphaGlobal.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetAlphaGlobal(alphaGlobal/100);
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Label("Exposure (/100)");		
				exposure=(float)(Convert.ToDouble(GUILayout.TextField(exposure.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetExposure(exposure/100);
				}
				GUILayout.EndHorizontal();


				//m_manager.SetRadius(m_radius);
				m_manager.setLayernCam(layer,cam);

				GUILayout.BeginHorizontal();
								
				if(GUILayout.Button("Toggle depth buffer")){
					if (!depthbufferEnabled){
						cams[cam].gameObject.AddComponent(typeof(ViewDepthBuffer));
						depthbufferEnabled=true;
								}
					else{
						Component.Destroy(cams[cam].gameObject.GetComponent<ViewDepthBuffer>());
						depthbufferEnabled=false;
					}}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				
				if(GUILayout.Button("Toggle PostProcessing")){
					if (!postprocessingEnabled){
						m_manager.enablePostprocess();
						postprocessingEnabled=true;
					}
					else{
						m_manager.disablePostprocess();
						postprocessingEnabled=false;
					}}
				GUILayout.EndHorizontal();


				
				GUILayout.BeginHorizontal();
				GUILayout.Label("Post Processing Alpha (/100)");		
				postProcessingalpha=(float)(Convert.ToDouble(GUILayout.TextField(postProcessingalpha.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetPostProcessAlpha(postProcessingalpha/100);
				}
				GUILayout.EndHorizontal();



				GUILayout.BeginHorizontal();
				GUILayout.Label("Post Processing Depth (/100)");		
				postProcessDepth=(float)(Convert.ToDouble(GUILayout.TextField(postProcessDepth.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetPostProcessDepth(postProcessDepth/100);
				}
				GUILayout.EndHorizontal();



				GUILayout.BeginHorizontal();
				GUILayout.Label("Post Processing Exposure (/100)");		
				postProcessExposure=(float)(Convert.ToDouble(GUILayout.TextField(postProcessExposure.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetPostProcessExposure(postProcessExposure/100);
				}
				GUILayout.EndHorizontal();


				GUILayout.BeginHorizontal();
				GUILayout.Label("Post Processing Scale (/100)");		
				postProcessScale=(float)(Convert.ToDouble(GUILayout.TextField(postProcessScale.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetPostProcessScale(postProcessScale/100);
				}
				GUILayout.EndHorizontal();



				GUILayout.BeginHorizontal();
				GUILayout.Label("Far plane");		
				farPlane=(Convert.ToInt32(GUILayout.TextField(farPlane.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetFarPlane(farPlane);
				}
				GUILayout.EndHorizontal();



				GUILayout.BeginHorizontal();
				GUILayout.Label("Near plane");		
				nearPlane=(Convert.ToInt32(GUILayout.TextField(nearPlane.ToString())));
				
				if(GUILayout.Button("Set"))
				{
					m_manager.SetNearPlane(nearPlane);
				}
				GUILayout.EndHorizontal();




				GUILayout.BeginHorizontal();
				GUILayout.Label("ManagerState");				
				GUILayout.TextField(m_manager.getManagerState());
				GUILayout.EndHorizontal();





//				//DIRECTIONS DEBUG

//				GUILayout.BeginHorizontal();
//				GUILayout.Label("Direction from kerbin to sun");				
//				Vector3 tmp=getSunTransform().position-getKerbinTransform().position;
//				tmp=tmp.normalized;
//				GUILayout.TextField(tmp.ToString());
//				GUILayout.EndHorizontal();
//
//
//				GUILayout.BeginHorizontal();
//				GUILayout.Label("SunNode Direction");	
//				//SunNode idek=m_manager.GetSunNode();
//				//if (idek!=null){
//						//GUILayout.TextField(idek.getSunNodeDirection().ToString());
//				GUILayout.TextField(m_manager.GetSunNodeDirection().ToString());
//				//}
//				//	else{
//				//		GUILayout.TextField("SunNode is null");
//				//}
//
//				GUILayout.EndHorizontal();
//
//
//
//				GUILayout.BeginHorizontal();
//				GUILayout.Label("Active vessel position");				
//				tmp=getActiveVesselTransform().position;
//				GUILayout.TextField(tmp.ToString());
//				GUILayout.EndHorizontal();
//
//				GUILayout.BeginHorizontal();
//				GUILayout.Label("Kerbin radius");				
//				GUILayout.TextField(celestialBodies[KerbinId].Radius.ToString());
//				GUILayout.EndHorizontal();


//				//DIRECTIONS DEBUG



				/*HashSet<string> shaders = new HashSet<string>();
				
				FindObjectsOfType<Shader>().ToList().ForEach(sh => shaders.Add(sh.name));
				Resources.FindObjectsOfTypeAll<Shader>().ToList().ForEach(sh => shaders.Add(sh.name));
				
				Log.Normal("{0} loaded shaders", shaders.Count);
				List<string> sorted = new List<string>(shaders); sorted.Sort();
				
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/shaders.txt"))
					foreach (var sh in sorted)
						file.WriteLine(sh);*/



				/*GUILayout.BeginHorizontal();
				GUILayout.Label("Cube position");				
				tmp=getCubeTransform().position;
				GUILayout.TextField(tmp.ToString());
				GUILayout.EndHorizontal();*/


				///VECTOR3D version, more accurate??
				/*GUILayout.BeginHorizontal();
				GUILayout.Label("Direction from kerbin to sun");				
				Vector3d tmp3d=VariablesInst.getSunWorldPos()-VariablesInst.getKerbinWorldPos()
				tmp3d=tmp3d.normalized;
				GUILayout.TextField(tmp3d.ToString());
				GUILayout.EndHorizontal();*/






				/*GUILayout.BeginHorizontal();
				GUILayout.Label("Kerbin transform forward");				
				GUILayout.TextField(VariablesInst.getKerbinTransform().forward.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Kerbin position");				
				GUILayout.TextField(VariablesInst.getKerbinTransform().position.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Kerbin world position");				
				GUILayout.TextField(VariablesInst.getKerbinWorldPos().ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Sun transform forward");				
				GUILayout.TextField(VariablesInst.getSunTransform().forward.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Sun position");				
				GUILayout.TextField(VariablesInst.getSunTransform().position.ToString());
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();
				GUILayout.Label("Sun World position");				
				GUILayout.TextField(VariablesInst.getSunWorldPos().ToString());
				GUILayout.EndHorizontal();
*/
			}


          /*  if (GUILayout.Button("Toggle Drag"))
                DragEnabled = !DragEnabled;



			            
            GUILayout.Label("Alt+F11 - shows/hides window");*/

        }

		/*public Transform getCubeTransform()
		{
			return cube.transform;
		}*/
		
		
		public Transform getActiveVesselTransform()
		{
			return FlightGlobals.ActiveVessel.GetTransform();
		}
		
		public bool getActiveState()
		{
			return isActive;
		}
		
		public int getBodiesNumber()
		{
			return celestialBodies.Length;
		}
		
		public Transform getKerbinTransform()
		{
			return celestialBodies[KerbinId].GetTransform();
		}
		
		public Vector3d getKerbinWorldPos()
		{
			return celestialBodies[KerbinId].position;
		}
		
		public Vector3d getSunWorldPos()
		{
			return celestialBodies[SunId].position;
		}
		
		public Transform getSunTransform()
		{
			return celestialBodies[SunId].GetTransform();
		}
		
		public string getBodyName(int i)
		{
			return celestialBodies[i].GetName();
		}


		public void deactivateSpecificAtmosphere(string name, int i)
		{
			Transform t = ScaledSpace.Instance.transform.FindChild(name);

					// Deactivate the Athmosphere-renderer
					t.GetChild(i).gameObject.GetComponent<MeshRenderer>().gameObject.SetActive(false);
					
					// Reset the shader parameters
					Material sharedMaterial = t.renderer.sharedMaterial;
					sharedMaterial.SetTexture(Shader.PropertyToID("_rimColorRamp"), null);
					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), 0);
					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), 0);
									
		}


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
