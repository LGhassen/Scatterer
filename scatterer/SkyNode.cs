using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	/*
 * Loads the tables required for the atmospheric scattering and sets any uniforms for shaders
 * that need them. If you create new tables using the PreprocessAtmo.cs script and changed some of
 * the settings (like the tables dimensions) you need to make sure the settings match here.
 * You can adjust some of these settings (mieG, betaR) to change the look of the scattering but
 * as precomputed tables are used there is a limit to how much the scattering will change.
 * For large changes you will need to create new table with the settings you want.
 * NOTE - all scenes must contain a skyNode
 */
	public class SkyNode : MonoBehaviour
	{
		
		[Persistent] public bool forceOFFaniso;
		
		SimplePostProcessCube hp;
		GameObject atmosphereMesh;
		MeshRenderer atmosphereMeshrenderer;
		
		//		[Persistent] public bool UIvisible = false;
		[Persistent] public bool displayInterpolatedVariables = false;
		
		public CelestialBody[] celestialBodies;	
		
		//		public float postRotX=0f,postRotY=0f,postRotZ=0f,
		//		public float postScaleX=1f,postScaleY=1f,postScaleZ=1f;
		
		public float postDist=-4500f;
		
		public float percentage;
		public int currentConfigPoint;
		
		bool coronasDisabled=false;
		
		public int renderQueue=2000;
		public int renderQueue2=2010;
		Matrix4x4 p;
		
		public float oceanNearPlane=0.01f;
		public float oceanFarPlane=750000f;
		
		public float terrainReflectance=1f;
		public float sunIntensity=100f;
		public float irradianceFactor =1f;
		public float oceanSigma = 0.04156494f;
		public float _Ocean_Threshold = 25f;
		[Persistent] public float extinctionMultiplier=1f;
		[Persistent] public float extinctionTint=100f;
		
		//		string codeBase;
		//		UriBuilder uri;
		string path;
		
		float totalscale;
		float totalscale2;
		
		int newRenderQueue;
		
		
		Transform celestialTransform;
		
		Vector3 idek;
		
		float alt,trueAlt;
		
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType<SkyNode>(null);
		
		//bool inScaledSpace=false;
		
		//		public static Dictionary<string, PSystemBody> prefabs = new Dictionary<string, PSystemBody>();
		
		public bool[] debugSettings= new bool[10];
		public float[] additionalScales=new float[10];
		
		//PSystemBody[] pSystemBodies;
		//ScaledSpaceFader kerbinPsystemBody;
		
		[Persistent] public float MapViewScale=1f;
		
		GameObject skyObject, skyExtinctObject;
		MeshRenderer skyMR, skyExtinctMR;
		MeshFilter skyMF, skyExtinctMF;
		
		CelestialBody parentCelestialBody;
		Transform ParentPlanetTransform;

		//Matrix4x4 m_sun_worldToLocalRotation;
		
		bool sunglareEnabled=true;
		bool stocksunglareEnabled=true;
		float sunglareCutoffAlt;
		
		Texture2D sunGlare;
		Texture2D black;
		
		//atmosphere properties
		
		/*[Persistent]*/ public float extinctionCoeff=0.7f;
		/*[Persistent]*/ public float atmosphereGlobalScale=1f;
		/*[Persistent]*/ public float postProcessingAlpha=0.78f;
		/*[Persistent]*/ public float postProcessingScale=1f;
		/*[Persistent]*/ public float postProcessDepth=0.02f;
		/*[Persistent]*/ public float postProcessExposure=0.18f;
		//		float inscatteringCoeff=0.8f; //useless, I also removed it from shader
		
		/*[Persistent]*/ public float m_HDRExposure= 0.2f;
		[Persistent] public float mapExposure= 0.15f;
		
		static PQS CurrentPQS=null;
		public bool inScaledSpace { get { return !(CurrentPQS != null && CurrentPQS.isActive);}}
		
		PQS testPQS;
		
		Vector3 position;
		
		bool initiated=false;
		Camera[] cams;
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		public bool postprocessingEnabled=true;
		int waitBeforeReloadCnt=0;
		
		//		[Persistent] public float alphaCutoff=0.001f;
		/*[Persistent]*/ public float alphaGlobal=1f;
		[Persistent] public float mapAlphaGlobal=1f;
		
		float m_radius;// = 600000.0f;
		//The radius of the planet (Rg), radius of the atmosphere (Rt)
		float Rg;// = 600000.0f;
		float Rt;// = (64200f/63600f) * 600000.0f;
		float RL;// = (64210.0f/63600f) * 600000.0f;
		
		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;
		const int RES_R = 32;
		const int RES_MU = 128;
		const int RES_MU_S = 32;
		const int RES_NU = 8;
		
		int layer=15;
		int cam=1;
		
		float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		//Half heights for the atmosphere air density (HR) and particle density (HM)
		//This is the height in km that half the particles are found below
		float HR = 8.0f;
		float HM = 1.2f;
		//scatter coefficient for mie
		Vector3 BETA_MSca = new Vector3(4e-3f,4e-3f,4e-3f);
		
		public Material m_atmosphereMaterial;
		Material m_skyMaterialScaled;
		Material m_skyExtinction;
						
		Vector3 m_betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[SerializeField]
		float m_mieG = 0.85f;
		
		string m_filePath = "/Proland/Textures/Atmo";
		
		
		public Matrix4x4d m_cameraToScreenMatrix;
		
		Mesh m_mesh;
		
		RenderTexture m_transmit, m_inscatter, m_irradiance;//, m_inscatterGround, m_transmitGround;
		
		Manager m_manager;
		
		//		var cbTransform;
		
		[Persistent] public float rimBlend=20f;
		[Persistent] public float rimpower=600f;
		
		
		[Persistent] public List<configPoint> configPoints= new List<configPoint> {new configPoint(5000f,1f,0.25f,1f,0.4f,0.23f)
			,new configPoint(15000f,1f,0.15f,1f,8f,0.23f)};
		
		//Initialization
		public void Start()
		{
			m_radius=m_manager.GetRadius();
			
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;

			m_mesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false,false);
			m_mesh.bounds = new Bounds(parentCelestialBody.transform.position, new Vector3(1e8f,1e8f, 1e8f));

			//Inscatter is responsible for the change in the sky color as the sun moves
			//The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture
			//and the shader manually performs the sample for the 4th dimension
			m_inscatter = new RenderTexture(RES_MU_S * RES_NU, RES_MU * RES_R, 0, RenderTextureFormat.ARGBHalf);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			
			//Transmittance is responsible for the change in the sun color as it moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_transmit = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBHalf);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;
			
			//Irradiance is responsible for the change in the sky color as the sun moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_irradiance = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBHalf);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;
			
			
			initiateOrRestart ();
			m_skyMaterialScaled=new Material(ShaderTool.GetMatFromShader2("CompiledSkyScaled.shader"));
			m_skyMaterialScaled.renderQueue = 2004;



			m_skyExtinction=new Material(ShaderTool.GetMatFromShader2("CompiledSkyExtinction.shader"));
			m_skyExtinction.renderQueue = 2002;
			
			sunGlare = new Texture2D (512, 512);
			black = new Texture2D (512, 512);
			
//			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
//			UriBuilder uri = new UriBuilder(codeBase);
//			path = Uri.UnescapeDataString(uri.Path);
//			path=Path.GetDirectoryName (path);
			path = m_manager.GetCore ().path;
			
			sunGlare.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", path + m_filePath, "sunglare.png")));
			black.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", path + m_filePath, "black.png")));
			
			sunGlare.wrapMode = TextureWrapMode.Clamp;
			m_skyMaterialScaled.SetTexture("_Sun_Glare", sunGlare);
			
			InitUniforms(m_skyMaterialScaled);
			InitUniforms(m_skyExtinction);

			
			m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
			
			
			CurrentPQS = parentCelestialBody.pqsController;
			testPQS = parentCelestialBody.pqsController;
			
			
			for (int j=0; j<10; j++)
			{
				debugSettings[j]=true;
			}
			
			for (int j=0; j<10; j++)
			{
				additionalScales[j]=1f;
			}
			
			skyObject = new GameObject ();
			skyMF = skyObject.AddComponent<MeshFilter>();
			Mesh idmesh = skyMF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
			//
			skyObject.layer = layer;
//			celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			celestialTransform = ParentPlanetTransform;
			//			skyObject.transform.parent = parentCelestialBody.transform;
			skyObject.transform.parent = celestialTransform;
			
			skyMR = skyObject.AddComponent<MeshRenderer>();
			skyMR.sharedMaterial = m_skyMaterialScaled;
			skyMR.material =m_skyMaterialScaled;
			skyMR.castShadows = false;
			skyMR.receiveShadows = false;

			///same for skyextinct
			skyExtinctObject = new GameObject ();
			skyExtinctMF = skyExtinctObject.AddComponent<MeshFilter>();
			idmesh = skyExtinctMF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
			//
			skyExtinctObject.layer = layer;
			skyExtinctObject.transform.parent = celestialTransform;
			
			skyExtinctMR = skyExtinctObject.AddComponent<MeshRenderer>();
			skyExtinctMR.sharedMaterial = m_skyExtinction;
			skyExtinctMR.material =m_skyExtinction;
			skyExtinctMR.castShadows = false;
			skyExtinctMR.receiveShadows = false;
			
			
			//			skyObject.transform.localPosition = Vector3.zero;
			//			skyObject.transform.localRotation = Quaternion.identity;
			//			skyObject.transform.localScale = Vector3.one;
			
			
			//			MR.enabled = true;
			
			
			//			pSystemBodies = (PSystemBody[])UnityEngine.Object.FindObjectsOfType(typeof(PSystemBody));
			//			print ("NUMBER FOUND");
			//			print (pSystemBodies.Length);
			//
			//			kerbinPsystemBody=ScaledSpace.Instance.transform.FindChild("Kerbin").gameObject.GetComponentInChildren<ScaledSpaceFader>();
			//
			//			if (kerbinPsystemBody == null) {
			//				print ("NULL");
			//			}
			//				else{
			//					print ("NOT NULL");
			//				print("fadeStart");
			//
			//				print(kerbinPsystemBody.fadeStart);
			//
			//				print("fadeEnd");
			//
			//				print(kerbinPsystemBody.fadeEnd);
			//
			//			
			//			}
			
			hp = new SimplePostProcessCube (10000, m_atmosphereMaterial);
			atmosphereMesh = hp.GameObject;
			atmosphereMesh.layer = 15;
			atmosphereMeshrenderer = hp.GameObject.GetComponent<MeshRenderer>();
			atmosphereMeshrenderer.material = m_atmosphereMaterial;
			
			celestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType(typeof(CelestialBody));	
		}
		
		
		public void UpdateStuff()  //to be called by update at camera rythm for some graphical stuff
		{
			
			atmosphereMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
			atmosphereMesh.transform.localRotation = farCamera.transform.localRotation;
			atmosphereMesh.transform.rotation = farCamera.transform.rotation;
			
			//adding post processing to camera
			if ((!inScaledSpace) && (!MapView.MapIsEnabled)) {
				if (postprocessingEnabled) {
					InitPostprocessMaterial (m_atmosphereMaterial);
					UpdatePostProcessMaterial (m_atmosphereMaterial);
					
					//					if (scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> () != null) {
					//						//						print ("ScaledSpaceCamera scatterPostprocess!=null");
					//						Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> ());
					//					}
					//					
					//					
					//					if (farCamera.gameObject.GetComponent<scatterPostprocess> () == null) {
					//						//						print ("farCamera scatterPostprocess==null");
					//						farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
					//					}
					
					farCamera.gameObject.GetComponent<scatterPostprocess> ().setMaterial (m_atmosphereMaterial);
				}
			}
			
		}
		
		public void UpdateNode()
		{
			
			//			if (!initiated) {
			//				cbTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == CurrentPQS.transform).FirstOrDefault (); 
			//			}
			//			cbTransform.deactivateAltitude = 5000000;
			
			//print ("Deactivate altitude");
			//print(cbTransform.deactivateAltitude);
			
			//			testPQS = parentCelestialBody.pqsController;
			//			print ("MAX PQS DETAIL DISTANCE");
			//			print (testPQS.maxDetailDistance);
			//
			//			print ("PQS visible radius");
			//			print (testPQS.visibleRadius);
			//
			//
			//			print ("PQS visible altitude");
			//			print (testPQS.visibleAltitude);
			//
			//			testPQS.isActive = true;
			//			print ("fade start");
			//			print (parentCelestialBody.scaledBody.GetComponent<ScaledSpaceFader> ().fadeStart);
			//
			//			print ("fade end");
			//			print (parentCelestialBody.scaledBody.GetComponent<ScaledSpaceFader>().fadeEnd);
			
			
			position = parentCelestialBody.transform.position;
			
			if (!initiated) {   
				m_radius = m_manager.GetRadius ();
				//m_radius = 600000.0f;
				
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;
				sunglareCutoffAlt = (Rt + Rt-Rg) /*  *0.995f*/;
				
				
				//gets the cameras, this isn't done at start() because the cameras still don't exist then and it crashes the game
				cams = Camera.allCameras;
				
				for (int i=0; i<cams.Length; i++) {
					if (cams [i].name == "Camera ScaledSpace")
						scaledSpaceCamera = cams [i];
					
					if (cams [i].name == "Camera 01")
						farCamera = cams [i];
					if (cams [i].name == "Camera 00")
						nearCamera = cams [i];
				}
				
				//				var cbTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == CurrentPQS.transform).FirstOrDefault (); 
				//				cbTransform.deactivateAltitude = 5000000;
				
				scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
				
				if (tmp != null) {
					Component.Destroy (tmp);
				}
				
				
				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
				{
					Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ());
				}
				
				
				//				if (postprocessingEnabled) {
				farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
				//					if (farCamera.gameObject.GetComponent<scatterPostprocess> () != null) {
				initiated = true;
				//					}
				//				} else {
				//					initiated = true;
				//				}
				
				
				if (forceOFFaniso) { 
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				}
				
				else
				{ 
					QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				}
				
			}
			
			
			alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
			trueAlt = alt - m_radius;
			
			if ((sunglareEnabled) ^ ((alt < sunglareCutoffAlt) && !MapView.MapIsEnabled)) { //^ is XOR
				toggleSunglare ();
			}
			
			if ((coronasDisabled) ^ (alt < sunglareCutoffAlt)) { //^ is XOR
				toggleCoronas ();
			}
			
//			if ((!stocksunglareEnabled) ^ ((alt < sunglareCutoffAlt-1000) && !MapView.MapIsEnabled)) { //^ is XOR
//				toggleStockSunglare();
//			}

			if ((!stocksunglareEnabled) ^ (m_manager.GetCore().stockSunglare)) { //^ is XOR
				toggleStockSunglare();
			}
			
			interpolateVariables ();
			
			//			if (alt < (80000 + m_radius)) 
			//			{
			//				postProcessDepth=(150f + (alt-m_radius)*(1350f/ 80000f)) /10000f;
			//				//postProcessDepth=(150 + (alt-m_radius)*(3350/ 80000))/1000f;
			//			}
			//
			//			else
			//			{
			//				postProcessDepth=(2500f + (alt-m_radius-80000f)*(4000f/ 3200000f)) /10000f;
			//			}
			//
			//			print ("POSTPROCESS DEPTH");
			//			print (postProcessDepth*10000f);
			
			
			//if alt-tabbing/windowing and rendertextures are lost
			//this loads them back up
			//you have to wait for a frame of two because if you do it immediately they don't get loaded
			if (!m_inscatter.IsCreated ()) {
				waitBeforeReloadCnt++;
				if (waitBeforeReloadCnt >= 2) {
					
					initiateOrRestart ();
					print ("Scatterer: reloaded scattering tables");
					waitBeforeReloadCnt = 0;
				}
			}
			
			{
				
				skyObject.layer = 10;
				skyExtinctObject.layer=10;
				
				
				skyMF.mesh = m_mesh;
				skyMR.material =m_skyMaterialScaled;
				skyMR.castShadows = false;
				skyMR.receiveShadows = false;
				skyMR.sharedMaterial = m_skyMaterialScaled;
				skyMR.enabled = true;

				skyExtinctMF.mesh = m_mesh;
				skyExtinctMR.material =m_skyExtinction;
				skyExtinctMR.castShadows = false;
				skyExtinctMR.receiveShadows = false;
				skyExtinctMR.sharedMaterial = m_skyExtinction;
				skyExtinctMR.enabled = m_manager.GetCore().extinctionEnabled;
//				skyExtinctMR.enabled = false;
				
				
				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () == null)
				{
					//					print ("ScaledSpaceCamera updateAtCameraRythm==null");
					scaledSpaceCamera.gameObject.AddComponent(typeof(updateAtCameraRythm));
				}
				

				
				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
				{
					//					print ("ScaledSpaceCamera updateAtCameraRythm!=null");
					scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ().settings (m_mesh,m_skyMaterialScaled,m_skyExtinction, m_manager,this,skyObject,skyExtinctObject,debugSettings[6],parentCelestialBody.transform,celestialTransform);										
					//					scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ().settings (m_skyMaterialScaled, m_manager,this,skyObject,debugSettings[6],farCamera.transform);
				}
				
			}
			
			
			
			//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cams[cam]);
			
			
			//						if (debugSettings[6]){
			//						skyObject.transform.parent = parentCelestialBody.transform;
			//						}
			//			
			//						else{
			//							Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			//							skyObject.transform.parent = celestialTransform;
			//						}
			
			
			//			kerbinPsystemBody=ScaledSpace.Instance.transform.FindChild("Kerbin").gameObject.GetComponentInChildren<ScaledSpaceFader>();
			//			
			//			if (kerbinPsystemBody == null) {
			//				print ("NULL");
			//			}
			//			else{
			//				print ("NOT NULL");
			//				print("fadeStart");
			//				kerbinPsystemBody.fadeStart=110000;
			//				print(kerbinPsystemBody.fadeStart);
			//				
			//				print("fadeEnd");
			//				kerbinPsystemBody.fadeEnd=200000;
			//				print(kerbinPsystemBody.fadeEnd);
			//				
			//				
			//			}
			
			//			if (inScaledSpace) {
			//				print ("In scaled space");
			//			}
			//				else{
			//
			//					print ("In normal space");
			//				}

			
			
			atmosphereMeshrenderer.enabled = (!inScaledSpace) && (postprocessingEnabled);
			
			
			//this snippet fixes the problem with the moon rendering over the atmosphere but behind the planet
			
			for (int k=0; (k< celestialBodies.Length) ; k++)
			{
				Transform tmpTransform;
				//				Transform tmpTransform =celestialBodies[k].transform;
				{
					newRenderQueue=2001;
					
					if (celestialBodies[k].name != parentCelestialBody.name)
					{
						tmpTransform = GetScaledTransform (celestialBodies[k].name);												;
						if(!MapView.MapIsEnabled){
							if ((celestialBodies[k].transform.position-farCamera.transform.position).magnitude < (parentCelestialBody.transform.position-farCamera.transform.position).magnitude)
							{
								newRenderQueue=2005;
							}
						}
						else
						{
//							if ((ScaledSpace.LocalToScaledSpace(celestialBodies[k].transform.position)-scaledSpaceCamera.transform.position).magnitude < (ScaledSpace.LocalToScaledSpace(parentCelestialBody.transform.position)-scaledSpaceCamera.transform.position).magnitude)
							if ((ParentPlanetTransform.position-scaledSpaceCamera.transform.position).magnitude < (ParentPlanetTransform.position-scaledSpaceCamera.transform.position).magnitude)
							{
								newRenderQueue=2005;
							}
							
						}
						
					}
					else
					{
						tmpTransform=ParentPlanetTransform;
					}
					
					//					print (celestialBodies[k].name);
					MeshRenderer mr2 = (MeshRenderer)tmpTransform.GetComponent (typeof(MeshRenderer));
					if (mr2 != null)
					{															
						mr2.material.renderQueue=newRenderQueue;
						//						print ("mech null");
					}
					//					else print ("ey null");
				}										
			}
			
			
		}
		
		
		public void SetUniforms(Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if(mat == null) return;
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			
			//			mat.SetFloat ("_Alpha_Cutoff", alphaCutoff);
			
			mat.SetFloat ("_Extinction_Tint", extinctionTint);
			mat.SetFloat ("extinctionMultiplier", extinctionMultiplier);

			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat ("_Alpha_Global", alphaGlobal);
			}
			
			else
			{
				mat.SetFloat ("_Alpha_Global", mapAlphaGlobal);
			}
			
			mat.SetFloat("scale",atmosphereGlobalScale);
			mat.SetFloat("Rg", Rg*atmosphereGlobalScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale);
			mat.SetFloat("RL", RL*atmosphereGlobalScale);
			
			//						if (debugSettings [5])
			if(!MapView.MapIsEnabled)
			{
				mat.SetFloat ("_Globals_ApparentDistance", 1f);
			}
			else
			{
				mat.SetFloat("_Globals_ApparentDistance", (float)((parentCelestialBody.Radius/100.2f)/ MapViewScale ));
//				mat.SetFloat ("_Globals_ApparentDistance", MapViewScale);
			}
			
			
			//						if (debugSettings[1])
			if(!MapView.MapIsEnabled)
			{
				
				mat.SetMatrix ("_Globals_WorldToCamera", farCamera.worldToCameraMatrix);
				mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			}
			
			else
			{
				mat.SetMatrix ("_Globals_WorldToCamera", scaledSpaceCamera.worldToCameraMatrix);
				mat.SetMatrix ("_Globals_CameraToWorld", scaledSpaceCamera.worldToCameraMatrix.inverse);
			}
			
			
			
			mat.SetVector("betaR", m_betaR / 1000.0f);
			mat.SetFloat("mieG", Mathf.Clamp(m_mieG, 0.0f, 0.99f));
			mat.SetTexture("_Sky_Transmittance", m_transmit);
			mat.SetTexture("_Sky_Inscatter", m_inscatter);
			mat.SetTexture("_Sky_Irradiance", m_irradiance);
			mat.SetFloat("_Sun_Intensity", 100f);
			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun().normalized);
			//			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun());
			
			
			//			//copied from m_manager's set uniforms
			
			//Matrix4x4 p;
			//						if (debugSettings [2])
			if(!MapView.MapIsEnabled)
			{
				p = farCamera.projectionMatrix;
			}
			else
			{
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			//						if (debugSettings [3])
			if(!MapView.MapIsEnabled)
			{
				mat.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position);
			}
			else
			{
				mat.SetVector ("_Globals_WorldCameraPos", scaledSpaceCamera.transform.position);
			}
			//			else
			//			{
			//				Vector3 newpos= ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position);
			//				//				m_skyMaterial.SetVector ("_Globals_WorldCameraPos", scaledSpaceCamera.transform.position);
			//				mat.SetVector ("_Globals_WorldCameraPos", newpos);
			//			}
			//			
			//						if (debugSettings [4])
			if(!MapView.MapIsEnabled)
			{
				mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
				
			}
			else
			{
				
//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				celestialTransform = ParentPlanetTransform;
				//				idek =celestialTransform.position;
				idek =celestialTransform.position-scaledSpaceCamera.transform.position;
				mat.SetVector ("_Globals_Origin", idek);
				
			}
			
			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat ("_Exposure", m_HDRExposure);
			}
			else
			{
				mat.SetFloat ("_Exposure", mapExposure);
			}
			
			
			//			int childCnt = 0;
			//			Transform scaledSunTransform=ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == "Sun");
			//			foreach (Transform child in scaledSunTransform)
			//			{
			//				print(childCnt);
			//				print(child.gameObject.name);
			//				childCnt++;
			//				MeshRenderer temp;
			//				temp = child.gameObject.GetComponent<MeshRenderer>();
			////				temp.enabled=false;
			//				print(temp.enabled);
			//			}
		}
		
		
		public void SetOceanUniforms(Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if(mat == null) return;
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			
			//			mat.SetFloat ("_Alpha_Cutoff", alphaCutoff);
			//			mat.SetFloat ("_Alpha_Global", alphaGlobal);
			
			mat.SetFloat("scale",atmosphereGlobalScale);
			mat.SetFloat("Rg", Rg*atmosphereGlobalScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale);
			mat.SetFloat("RL", RL*atmosphereGlobalScale);
			
			
			
			//			//			if (debugSettings[1])
			//			if(!MapView.MapIsEnabled)
			//			{
			//				
			//				mat.SetMatrix ("_Globals_WorldToCamera", farCamera.worldToCameraMatrix);
			//				mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			//			}
			//			
			//			else
			//			{
			//				mat.SetMatrix ("_Globals_WorldToCamera", scaledSpaceCamera.worldToCameraMatrix);
			//				mat.SetMatrix ("_Globals_CameraToWorld", scaledSpaceCamera.worldToCameraMatrix.inverse);
			//			}
			
			
			
			mat.SetVector("betaR", m_betaR / 1000.0f);
			mat.SetFloat("mieG", Mathf.Clamp(m_mieG, 0.0f, 0.99f));
			mat.SetTexture("_Sky_Transmittance", m_transmit);
			mat.SetTexture("_Sky_Inscatter", m_inscatter);
			mat.SetTexture("_Sky_Irradiance", m_irradiance);
			mat.SetFloat("_Sun_Intensity", 100f);
			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun().normalized);
			//			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun());
			
			
			//			//copied from m_manager's set uniforms
			
			//			Matrix4x4 p;
			//			//			if (debugSettings [2])
			////			if(!MapView.MapIsEnabled)
			////			{
			//			float tmpNearclip = m_manager.GetCore ().chosenCamera.nearClipPlane;
			//			float tmpFarclip = m_manager.GetCore ().chosenCamera.farClipPlane;
			//
			//			m_manager.GetCore ().chosenCamera.nearClipPlane = oceanNearPlane;
			//			m_manager.GetCore ().chosenCamera.farClipPlane = oceanFarPlane;
			//
			////			float h = (float)(GetHeight() - m_groundHeight);
			////			m_manager.GetCore ().chosenCamera.nearClipPlane = 0.1f * (alt - m_radius);
			////			m_manager.GetCore ().chosenCamera.farClipPlane = 1e6f * (alt - m_radius);
			//
			//				p = m_manager.GetCore().chosenCamera.projectionMatrix;
			//
			////			p = scaledSpaceCamera.projectionMatrix;
			//
			//			m_manager.GetCore ().chosenCamera.nearClipPlane=tmpNearclip;
			//			m_manager.GetCore ().chosenCamera.farClipPlane=tmpFarclip;
			//
			////			p = scaledSpaceCamera.projectionMatrix;
			////			}
			////			else
			////			{
			////				p = scaledSpaceCamera.projectionMatrix;
			////			}
			//			
			//			
			//			m_cameraToScreenMatrix = new Matrix4x4d (p);
			//			mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
			//			mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			//			if (debugSettings [3])
			{
				mat.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position);
			}
			//			else
			//			{
			//				Vector3 newpos= ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position);
			//				//				m_skyMaterial.SetVector ("_Globals_WorldCameraPos", scaledSpaceCamera.transform.position);
			//				mat.SetVector ("_Globals_WorldCameraPos", newpos);
			//			}
			
			//			if (debugSettings [4])
			if(!MapView.MapIsEnabled)
			{
				mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
			}
			else
			{
				
//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				celestialTransform = ParentPlanetTransform;
				idek =celestialTransform.position;
				mat.SetVector ("_Globals_Origin", idek);
				
			}
			
			mat.SetFloat ("_Exposure", m_HDRExposure);
			
			//			int childCnt = 0;
			//			Transform scaledSunTransform=ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == "Sun");
			//			foreach (Transform child in scaledSunTransform)
			//			{
			//				print(childCnt);
			//				print(child.gameObject.name);
			//				childCnt++;
			//				MeshRenderer temp;
			//				temp = child.gameObject.GetComponent<MeshRenderer>();
			////				temp.enabled=false;
			//				print(temp.enabled);
			//			}
		}
		
		void InitPostprocessMaterial(Material mat)
		{
			
			totalscale = 1;
			for (int j=0; j<10; j++)
			{
				totalscale=totalscale*additionalScales[j];
			}
			
			mat.SetTexture("_Transmittance", m_transmit);
			mat.SetTexture("_Inscatter", m_inscatter);
			mat.SetTexture("_Irradiance", m_irradiance);
			
			
			
			//Consts, best leave these alone
			mat.SetFloat("M_PI", Mathf.PI);
			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*postProcessingScale*totalscale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*postProcessingScale*totalscale);
			mat.SetFloat("Rl", RL*atmosphereGlobalScale*postProcessingScale*totalscale);
			mat.SetFloat("RES_R", RES_R);
			mat.SetFloat("RES_MU", RES_MU);
			mat.SetFloat("RES_MU_S", RES_MU_S);
			mat.SetFloat("RES_NU", RES_NU);
			
			mat.SetFloat("SKY_W", SKY_W);
			mat.SetFloat("SKY_H", SKY_H);
			
			mat.SetFloat("_Ocean_Sigma", oceanSigma);//
			
			mat.SetFloat("SUN_INTENSITY", sunIntensity);//
			//			mat.SetVector("_inCamPos", cams[cam].transform.position);
			
			if (debugSettings [0]) {
				mat.SetVector("_inCamPos", farCamera.transform.position);
			}
			
			else 
			{	if (!debugSettings [4]) 
				
				{
					Vector3 pos= ScaledSpace.LocalToScaledSpace(scaledSpaceCamera.transform.position);
					mat.SetVector("_inCamPos", pos);
				}
				
				else    //if (!debugSettings [0])
				{
					mat.SetVector("_inCamPos", scaledSpaceCamera.transform.position);
				}
				
				
			}
			
			mat.SetVector("SUN_DIR", m_manager.GetSunNodeDirection());
		}
		
		
		void UpdatePostProcessMaterial(Material mat)
		{
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			//			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*postProcessingScale);
			//			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*postProcessingScale);
			//			mat.SetFloat("Rl", RL*atmosphereGlobalScale*postProcessingScale);
			




			totalscale  = 1;
			totalscale2 = 1;
			for (int j=0; j<5; j++)
			{
				totalscale=totalscale*additionalScales[j];
			}
			
			for (int j=6; j<10; j++)
			{
				totalscale2=totalscale2*additionalScales[j];
			}
			
			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*totalscale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*totalscale);
			mat.SetFloat("Rl", RL*atmosphereGlobalScale*totalscale);
			
			//mat.SetFloat("_inscatteringCoeff", inscatteringCoeff);
			mat.SetFloat("_extinctionCoeff", extinctionCoeff);
			mat.SetFloat("_global_alpha", postProcessingAlpha);
			mat.SetFloat("_Exposure", postProcessExposure);
			mat.SetFloat("_global_depth", postProcessDepth);
			mat.SetFloat("_global_depth2", totalscale2);
			
			mat.SetFloat("terrain_reflectance", terrainReflectance);
			mat.SetFloat("_irradianceFactor", irradianceFactor);
			
			mat.SetFloat("_Scale", postProcessingScale);
			//			mat.SetFloat("_Scale", 1);
			
			mat.SetFloat ("_Ocean_Sigma", oceanSigma);
			mat.SetFloat ("_Ocean_Threshold", _Ocean_Threshold);
			
			
			//			mat.SetMatrix ("_Globals_CameraToWorld", cams [0].worldToCameraMatrix.inverse);
			if (debugSettings [1]) {
				mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			}
			else
			{
				mat.SetMatrix ("_Globals_CameraToWorld", scaledSpaceCamera.worldToCameraMatrix.inverse);
			}
			
			if (debugSettings [2]) {
				mat.SetVector ("_CameraForwardDirection", farCamera.transform.forward);
			} else {
				mat.SetVector ("_CameraForwardDirection", scaledSpaceCamera.transform.forward);
			}
			
			if (debugSettings [3]) {
				mat.SetVector ("_Globals_Origin", /*Vector3.zero-*/parentCelestialBody.transform.position);
			} else
				
			{
				
//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				celestialTransform = ParentPlanetTransform;
				idek =celestialTransform.position;
				mat.SetVector ("_Globals_Origin", /*Vector3.zero-*/ idek);
			}
			
			//mat.SetVector("betaR", m_betaR / (Rg / m_radius));
			//			mat.SetVector("betaR", m_betaR / (postProcessDepth));
			mat.SetVector("betaR", new Vector4(2.9e-3f, 0.675e-2f, 1.655e-2f, 0.0f));
			mat.SetFloat("mieG", 0.4f);
			mat.SetVector("SUN_DIR", m_manager.GetSunNodeDirection());
			mat.SetFloat("SUN_INTENSITY", sunIntensity);
			
			
			Matrix4x4 ctol1 = farCamera.cameraToWorldMatrix;
			Vector3d tmp = (farCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			
			Matrix4x4d viewMat = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
			                                     ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
			                                     ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
			                                     ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);
			
//			print ("viewmat");
//			print (viewMat.ToMatrix4x4());
			
			
			//			Matrix4x4 viewMat = farCamera.worldToCameraMatrix;
			viewMat = viewMat.Inverse ();
			Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);

//			projMat.m23 = projMat.m23 * 0.5f;

			//			print ("projmat");
//			print (projMat);

			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4());          
			mat.SetMatrix ("_ViewProjInv", viewProjMat.inverse);
			

//			mat.SetMatrix("_ViewToWorld", viewMat.ToMatrix4x4());
//			
//			var lpoints = RecalculateFrustrumPoints(farCamera);
//			mat.SetVector("_FrustrumPoints", new Vector4(
//				lpoints[4].x,lpoints[5].x,lpoints[5].y,lpoints[6].y));
//			
//			mat.SetFloat("_CameraFar", farCamera.farClipPlane);

			
		}
		
		
		public void InitUniforms(Material mat)
		{
			//Init uniforms that this or other gameobjects may need
			if(mat == null) return;
			
			mat.SetFloat("scale",Rg*atmosphereGlobalScale /  m_radius);
			mat.SetFloat("Rg", Rg*atmosphereGlobalScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale);
			mat.SetFloat("RL", RL*atmosphereGlobalScale);
			mat.SetFloat("TRANSMITTANCE_W", TRANSMITTANCE_W);
			mat.SetFloat("TRANSMITTANCE_H", TRANSMITTANCE_H);
			mat.SetFloat("SKY_W", SKY_W);
			mat.SetFloat("SKY_H", SKY_H);
			mat.SetFloat("RES_R", RES_R);
			mat.SetFloat("RES_MU", RES_MU);
			mat.SetFloat("RES_MU_S", RES_MU_S);
			mat.SetFloat("RES_NU", RES_NU);
			mat.SetFloat("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
			mat.SetFloat("HR", HR * 1000.0f);
			mat.SetFloat("HM", HM * 1000.0f);
			mat.SetVector("betaMSca", BETA_MSca / 1000.0f);
			mat.SetVector("betaMEx", (BETA_MSca / 1000.0f) / 0.9f);
			//			mat.SetFloat("_Alpha_Cutoff", alphaCutoff);
			
		}
		
		/*void OnGUI(){
    GUI.DrawTexture(new Rect(0,0,512, 512), m_skyMap);
        }*/
		
		//		public void SetNearPlane(int NR)
		//		{
		//			farCamera.gameObject.GetComponent<scatterPostprocess>().setNearPlane(NR);
		//		}
		//		
		//		public void SetFarPlane(int FR)
		//		{
		//			farCamera.gameObject.GetComponent<scatterPostprocess>().setFarPlane(FR);
		//		}
		
		public void setManager(Manager manager)
		{
			m_manager=manager;
		}
		
		public void enablePostprocess()
		{
			//			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			if ((farCamera.gameObject.GetComponent<scatterPostprocess> ()) == null)
			{
				farCamera.gameObject.AddComponent(typeof(scatterPostprocess));
			}
			//			atmosphereMeshrenderer.enabled = true;
			postprocessingEnabled = true;
		}
		
		public void disablePostprocess()
		{
			//			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			//			if (tmp != null)
			if ((farCamera.gameObject.GetComponent<scatterPostprocess> ()) != null)
			{
				//				Component.Destroy (tmp);
				
				Component.Destroy (farCamera.gameObject.GetComponent<scatterPostprocess> ());
				
			}
			
			
			//Component.Destroy(cams[cam+1].gameObject.GetComponent<scatterPostprocess>());
			//			atmosphereMeshrenderer.enabled = false;
			postprocessingEnabled = false;
		}
		
		
		public void SetPostProcessExposure(float postExposure)
		{
			postProcessExposure=postExposure;
		}
		
		public void SetPostProcessDepth(float postDepth)
		{
			postProcessDepth=postDepth;
		}
		
		public void SetPostProcessAlpha(float postAlpha)
		{
			postProcessingAlpha=postAlpha;
		}
		
		public void SetPostProcessScale(float postScale)
		{
			postProcessingScale=postScale;
		}
		
		public void SetAtmosphereGlobalScale(float gScale)
		{
			atmosphereGlobalScale=gScale;
		}
		
		public void SetParentCelestialBody(CelestialBody inPlanet)
		{
			parentCelestialBody=inPlanet;
		}

		public void setParentPlanetTransform (Transform parentTransform)
		{
			ParentPlanetTransform = parentTransform;
		}
		
		public void SetExposure(float expo)
		{
			m_HDRExposure=expo;
		}
		
		public void SetExtinctionCoeff(float exCoeff)
		{
			extinctionCoeff = exCoeff;
		}
		
		
		//		public void SetAlphaCutoff(float cutoff)
		//		{
		//			alphaCutoff = cutoff;
		//		}
		
		public void SetAlphaGlobal(float glob)
		{
			alphaGlobal = glob;
		}
		
		public void setLayernCam(int inLayer, int inCam)
		{
			layer = inLayer;
			cam = inCam;
		}
		
		
		public void initiateOrRestart()
		{
			
			m_inscatter.Create ();
			m_transmit.Create ();
			m_irradiance.Create ();
			
//			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
//			UriBuilder uri = new UriBuilder(codeBase);
//			path = Uri.UnescapeDataString(uri.Path);
//			path=Path.GetDirectoryName (path);
			path = m_manager.GetCore ().path;
			
			
			string path1 = path + m_filePath + "/transmittance.raw";
			EncodeFloat.WriteIntoRenderTexture (m_transmit, 3, path1,null);
			
			path1 = path + m_filePath + "/irradiance.raw";
			EncodeFloat.WriteIntoRenderTexture (m_irradiance, 3, path1,null);
			
			path1 = path + m_filePath + "/inscatter.raw";
			EncodeFloat.WriteIntoRenderTexture (m_inscatter, 4, path1,null);
		}
		
		
		
		public void OnDestroy()
		{
			//base.OnDestroy();
			saveToConfigNode ();
			
			m_transmit.Release();
			m_irradiance.Release();
			m_inscatter.Release();
			
			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			
			if(tmp != null)
			{
				Component.Destroy (tmp);
			}

			
			if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
			{
				Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ());
			}
			
			
			Component.Destroy (skyMR);
			Destroy (skyObject);

			Component.Destroy (skyExtinctMR);
			Destroy (skyExtinctObject);

			Component.Destroy (atmosphereMeshrenderer);
			Destroy (atmosphereMesh);
			//Destroy (hp);
		}
		
		public void destroyskyObject()
		{
			Destroy (skyObject);
		}
		
		
		public void toggleSunglare()
		{
			if (sunglareEnabled)
			{
				m_skyMaterialScaled.SetTexture ("_Sun_Glare", black);
				sunglareEnabled = false;
			}
			else
			{
				m_skyMaterialScaled.SetTexture("_Sun_Glare", sunGlare);
				sunglareEnabled=true;
			}
		}
		
		public void toggleCoronas(){
			
			Transform scaledSunTransform=ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == "Sun");
			foreach (Transform child in scaledSunTransform)
			{
				MeshRenderer temp;
				temp = child.gameObject.GetComponent<MeshRenderer>();
				temp.enabled=coronasDisabled;
			}
			coronasDisabled=!coronasDisabled;
		}
		
		public void toggleStockSunglare()
		{
			if (stocksunglareEnabled) 
			{
				Sun.Instance.sunFlare.enabled = false;
			} 
			
			else 
			{
				Sun.Instance.sunFlare.enabled = true;
			}
			stocksunglareEnabled = !stocksunglareEnabled;
		}
		
		public void toggleAniso(){
			if (!forceOFFaniso) { 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			}
			
			else
			{ 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			}
			
			forceOFFaniso = !forceOFFaniso;
		}
		
		public Transform GetScaledTransform(string body)
		{
			List<Transform> transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single(n => n.name == body);
		}
		
		public void loadSettings()
		{
			cfg.load ();
			Rg =float.Parse(cfg.GetValue<string>("Rg"));
			Rt =float.Parse(cfg.GetValue<string>("Rt"));
			RL =float.Parse(cfg.GetValue<string>("RL"));
			
			m_betaR = cfg.GetValue<Vector3>("BETA_R");
			BETA_MSca = cfg.GetValue<Vector3>("BETA_MSca");
			m_mieG =float.Parse(cfg.GetValue<string>("MIE_G"));
			
			HR =float.Parse( cfg.GetValue<string>("HR"));
			HM =float.Parse( cfg.GetValue<string>("HM"));
			AVERAGE_GROUND_REFLECTANCE =float.Parse(cfg.GetValue<string>("AVERAGE_GROUND_REFLECTANCE"));
			atmosphereGlobalScale=float.Parse(cfg.GetValue<string>("atmosphereGlobalScale"));	
		}
		
		//		public void saveSettings()
		//		{
		//			cfg ["Rg"] = Rg.ToString();
		//			cfg ["Rt"] = Rt.ToString();
		//			cfg ["RL"] = RL.ToString();
		//			
		//			cfg ["BETA_R"] = m_betaR;
		//			cfg ["BETA_MSca"] = BETA_MSca;
		//			cfg ["MIE_G"] = m_mieG.ToString();
		//			cfg ["HR"] = HR.ToString();
		//			cfg ["HM"] = HM.ToString();
		//			cfg ["AVERAGE_GROUND_REFLECTANCE"] = AVERAGE_GROUND_REFLECTANCE.ToString();
		//			
		//			cfg.save ();
		//		}
		
		public void loadFromConfigNode() {
			ConfigNode cnToLoad = ConfigNode.Load(path+"/config/Settings.txt");
			ConfigNode.LoadObjectFromConfig(this, cnToLoad);
		}
		
		
		public void saveToConfigNode() {
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this);
			cnTemp.Save(path+"/config/Settings.txt");
		}
		
		public void interpolateVariables()
		{
			if (trueAlt <= configPoints [0].altitude) {
				alphaGlobal = configPoints [0].skyAlpha;
				m_HDRExposure = configPoints [0].skyExposure;
				postProcessingAlpha = configPoints [0].postProcessAlpha;
				postProcessDepth = configPoints [0].postProcessDepth;
				postProcessExposure = configPoints [0].postProcessExposure;
				currentConfigPoint=0;
			} 
			else if (trueAlt > configPoints [configPoints.Count-1].altitude) {
				alphaGlobal = configPoints [configPoints.Count-1].skyAlpha;
				m_HDRExposure = configPoints [configPoints.Count-1].skyExposure;
				postProcessingAlpha = configPoints [configPoints.Count-1].postProcessAlpha;
				postProcessDepth = configPoints [configPoints.Count-1].postProcessDepth;
				postProcessExposure = configPoints [configPoints.Count-1].postProcessExposure;
				currentConfigPoint=configPoints.Count-1;
			} 
			else
			{
				for (int j=1;j<configPoints.Count;j++)
				{
					if ((trueAlt > configPoints [j-1].altitude) && (trueAlt <= configPoints [j].altitude))
					{
						percentage=(trueAlt-configPoints [j-1].altitude)/(configPoints [j].altitude-configPoints [j-1].altitude);
						
						alphaGlobal = percentage*configPoints [j].skyAlpha+(1-percentage)*configPoints [j-1].skyAlpha;
						m_HDRExposure = percentage*configPoints [j].skyExposure+(1-percentage)*configPoints [j-1].skyExposure;
						postProcessingAlpha = percentage*configPoints [j].postProcessAlpha+(1-percentage)*configPoints [j-1].postProcessAlpha;
						postProcessDepth = percentage*configPoints [j].postProcessDepth+(1-percentage)*configPoints [j-1].postProcessDepth;
						postProcessExposure = percentage*configPoints [j].postProcessExposure+(1-percentage)*configPoints [j-1].postProcessExposure;
						currentConfigPoint=j;
					}
				}
			}
			
			//			print ("aphaGlobal:"+alphaGlobal+"m_HDRExposure:"+m_HDRExposure+
			//			       "postProcessingAlpha:"+postProcessingAlpha+"postProcessDepth:"+postProcessDepth+"postProcessExposure"+postProcessExposure);
			//			print (m_HDRExposure);
			//			print (postProcessingAlpha);
			//			print (postProcessDepth);
			//			print (postProcessExposure);
			
		}

		// Code to calculate all the corners near and far of a camera
		private Vector3[] RecalculateFrustrumPoints(Camera cam)
		{
			var frustrumPoints = new Vector3[8];
			var far = cam.farClipPlane;
			var near = cam.nearClipPlane;
			var aspectRatio = cam.pixelWidth / cam.pixelHeight;
			
			if(cam.isOrthoGraphic)
			{
				var orthoSize = cam.orthographicSize;
				
				frustrumPoints[0] = new Vector3(orthoSize, orthoSize, near);
				frustrumPoints[1] = new Vector3(-orthoSize, orthoSize, near);
				frustrumPoints[2] = new Vector3(-orthoSize, -orthoSize, near);
				frustrumPoints[3] = new Vector3(orthoSize, -orthoSize, near);
				frustrumPoints[4] = new Vector3(orthoSize, orthoSize, far);
				frustrumPoints[5] = new Vector3(-orthoSize, orthoSize, far);
				frustrumPoints[6] = new Vector3(-orthoSize, -orthoSize, far);
				frustrumPoints[7] = new Vector3(orthoSize, -orthoSize, far);
			}
			else
			{
				var hNear = 2 * Mathf.Tan((cam.fieldOfView  * 0.5f) * Mathf.Deg2Rad) * near;
				var wNear = hNear * aspectRatio;
				
				var hFar = 2 * Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Deg2Rad) * far;
				var wFar = hFar * aspectRatio;
				
				var fc = Vector3.forward * far;
				var ftl = fc + (Vector3.up * hFar / 2) - (Vector3.right * wFar / 2);
				var ftr = fc + (Vector3.up * hFar / 2) + (Vector3.right * wFar / 2);
				var fbl = fc - (Vector3.up * hFar / 2) - (Vector3.right * wFar / 2);
				var fbr = fc - (Vector3.up * hFar / 2) + (Vector3.right * wFar / 2);
				
				var nc = Vector3.forward * near;
				var ntl = nc + (Vector3.up * hNear / 2) - (Vector3.right * wNear / 2);
				var ntr = nc + (Vector3.up * hNear / 2) + (Vector3.right * wNear / 2);
				var nbl = nc - (Vector3.up * hNear / 2) - (Vector3.right * wNear / 2);
				var nbr = nc - (Vector3.up * hNear / 2) + (Vector3.right * wNear / 2);
				
				frustrumPoints[0] = ntr;
				frustrumPoints[1] = ntl;
				frustrumPoints[2] = nbr;
				frustrumPoints[3] = nbl;
				frustrumPoints[4] = ftr;
				frustrumPoints[5] = ftl;
				frustrumPoints[6] = fbl;
				frustrumPoints[7] = fbr;    
			}
			
			return frustrumPoints;
		}	
	}
}