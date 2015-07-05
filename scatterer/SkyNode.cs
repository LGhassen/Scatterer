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

		public float postRotX=0f,postRotY=0f,postRotZ=180f,postDist=8000f;

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

		string codeBase;
		UriBuilder uri;
		string path;

		float totalscale;
		float totalscale2;


		Transform celestialTransform;

		Vector3 idek;

		float alt;
		
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType<SkyNode>(null);
		
		//bool inScaledSpace=false;
		
		public static Dictionary<string, PSystemBody> prefabs = new Dictionary<string, PSystemBody>();
		
		public bool[] debugSettings= new bool[7];
		public float[] additionalScales=new float[10];
		
		//PSystemBody[] pSystemBodies;
		//ScaledSpaceFader kerbinPsystemBody;
		
		public float apparentDistance=1f;
		
		GameObject tester;
		MeshRenderer MR;
		MeshFilter MF;
		
		CelestialBody parentCelestialBody;
		//Matrix4x4 m_sun_worldToLocalRotation;
		
		bool sunglareEnabled=true;
		bool stocksunglareEnabled=true;
		float sunglareCutoffAlt;
		
		Texture2D sunGlare;
		Texture2D black;

		//atmosphere properties

		[Persistent] public float extinctionCoeff=0.7f;
		[Persistent] public float atmosphereGlobalScale=1f;
		[Persistent] public float postProcessingAlpha=0.78f;
		[Persistent] public float postProcessingScale=1f;
		[Persistent] public float postProcessDepth=0.02f;
		[Persistent] public float postProcessExposure=0.18f;
		//		float inscatteringCoeff=0.8f; //useless, I also removed it from shader
		
		[Persistent] public float m_HDRExposure= 0.2f;
		
		static PQS CurrentPQS=null;
		public bool inScaledSpace { get { return !(CurrentPQS != null && CurrentPQS.isActive);}}
		
		PQS testPQS;
		
		Vector3 position;
		
		bool initiated=false;
		Camera[] cams;
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		public bool postprocessingEnabled=true;
		int waitBeforeReloadCnt=0;
		
		[Persistent] public float alphaCutoff=0.001f;
		[Persistent] public float alphaGlobal=1f;
		
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
		
		//Material idekk;
		
//		[SerializeField]
//		Material m_skyMaterial;
		Material m_skyMaterialScaled;
		
		//		[SerializeField]
		Material m_skyMapMaterial;
		
		//scatter coefficient for rayliegh
		[SerializeField]
		Vector3 m_betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[SerializeField]
		float m_mieG = 0.85f;
		
		string m_filePath = "/Proland/Textures/Atmo";
//		string path;
		
		public Matrix4x4d m_cameraToScreenMatrix;
		
		Mesh m_mesh;
		
		RenderTexture m_transmit, m_inscatter, m_irradiance, m_skyMap;//, m_inscatterGround, m_transmitGround;
		
		Manager m_manager;

//		var cbTransform;
		
		
		//Initialization
		public void Start()
		{
			m_radius=m_manager.GetRadius();
			
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
			
			//			old mesh, causes artifacts with aniso
			//			m_mesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false,false);
			//			m_mesh.bounds = new Bounds(parentCelestialBody.transform.position, new Vector3(1e8f,1e8f, 1e8f));
			
			m_mesh = isoSphere.Create ();
			m_mesh.bounds = new Bounds(parentCelestialBody.transform.position, new Vector3(1e8f,1e8f, 1e8f));
			
			//The sky map is used to create a reflection of the sky for objects that need it (like the ocean)
			m_skyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
			m_skyMap.filterMode = FilterMode.Trilinear;
			m_skyMap.wrapMode = TextureWrapMode.Clamp;
			m_skyMap.anisoLevel = 9;
			m_skyMap.useMipMap = true;
			//m_skyMap.mipMapBias = -0.5f;
			m_skyMap.Create();
			
			
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
//			m_skyMaterial=new Material(ShaderTool.GetMatFromShader2("CompiledSky.shader"));
			m_skyMaterialScaled=new Material(ShaderTool.GetMatFromShader2("CompiledSkyScaled.shader"));
			m_skyMapMaterial=new Material(ShaderTool.GetMatFromShader2("CompiledSkyMap.shader"));
			//m_skyMaterial.renderQueue = 2000;
			m_skyMaterialScaled.renderQueue = 2003;
			
			sunGlare = new Texture2D (512, 512);
			black = new Texture2D (512, 512);
			
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);
			
			sunGlare.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", path + m_filePath, "sunglare.png")));
			black.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", path + m_filePath, "black.png")));
			
			if (sunGlare == null)
			{
				print ("SUNGLARE NULL");
				
			}
			else
			{
				sunGlare.wrapMode = TextureWrapMode.Clamp;
//				m_skyMaterial.SetTexture("_Sun_Glare", sunGlare);
				m_skyMaterialScaled.SetTexture("_Sun_Glare", sunGlare);
			}
			

			
			
//			InitUniforms(m_skyMaterial);
			InitUniforms(m_skyMaterialScaled);
			InitUniforms(m_skyMapMaterial);
			
			m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
			

			if (forceOFFaniso) { 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			}
			
			else
			{ 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			}
			
			CurrentPQS = parentCelestialBody.pqsController;
			testPQS = parentCelestialBody.pqsController;

			
			
			
			for (int j=0; j<7; j++)
			{
				debugSettings[j]=true;
			}
			
			//			idekk = m_skyMaterial;
			//			Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
			//			foreach(Material mat in materials)
			//			{
			//				if(mat.name == "EVE/Diffuse")
			//				{idekk=mat;
			//					print("DIFFUSE MATERIAL FOUND");}
			//
			//
			//			}
			//
			
			for (int j=0; j<10; j++)
			{
				additionalScales[j]=1f;
			}

			tester = new GameObject ();
			MF = tester.AddComponent<MeshFilter>();
			Mesh idmesh = MF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
			//
			tester.layer = layer;
			celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			//			tester.transform.parent = parentCelestialBody.transform;
			tester.transform.parent = celestialTransform;
			
			
			MR = tester.AddComponent<MeshRenderer>();
			
			//			InitUniforms (m_skyMaterialScaled);
			//			SetUniforms(m_skyMaterialScaled);
			
			MR.sharedMaterial = m_skyMaterialScaled;
			MR.material =m_skyMaterialScaled;
			
			MR.castShadows = false;
			MR.receiveShadows = false;
			
			
			//			tester.transform.localPosition = Vector3.zero;
			//			tester.transform.localRotation = Quaternion.identity;
			//			tester.transform.localScale = Vector3.one;
			
			
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

			hp = new SimplePostProcessCube (2000, m_atmosphereMaterial);
			atmosphereMesh = hp.GameObject;
			
		}
		
		public void UpdateNode()
		{



//			print ("m_skyMaterialScaled.renderQueue");
//			print (m_skyMaterialScaled.renderQueue);
			
//			m_skyMaterialScaled.renderQueue = renderQueue;
//			Sun.Instance.sunFlare.enabled = false;
//			var cbTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == CurrentPQS.transform).FirstOrDefault (); 





//			if (!initiated) {
//				cbTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == CurrentPQS.transform).FirstOrDefault (); 
//			}

//			print ("type of cbtransform");
//	//		print ((cbTransform));
//			print (  );

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
			
			
											
			m_radius = m_manager.GetRadius ();
			//m_radius = 600000.0f;
			
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
			sunglareCutoffAlt = (Rt + Rt-Rg) /*  *0.995f*/ * atmosphereGlobalScale;
			
			//			if(inScaledSpace)
			//			{
			//				position=(ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name)).position;
			//			}
			//			else{
			position = parentCelestialBody.transform.position;
			//			}
			
			//			print ("In scaled Space");
			//			print (inScaledSpace);
			
			
			if (!initiated) {   //gets the cameras, this isn't done at start() because the cameras still don't exist then and it crashes the game
				cams = Camera.allCameras;
				
				for (int i=0; i<cams.Length; i++) {
					if (cams [i].name == "Camera ScaledSpace")
						scaledSpaceCamera = cams [i];

					if (cams [i].name == "Camera 01")
						farCamera = cams [i];
					if (cams [i].name == "Camera 00")
						nearCamera = cams [i];
				}

//				print("scaledspace cam near clipplane");
//				print (scaledSpaceCamera.nearClipPlane);
//				print("scaledSpaceCamer far clipplane");
//				print(scaledSpaceCamera.farClipPlane);



				var cbTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> (true).Where (mod => mod.transform.parent == CurrentPQS.transform).FirstOrDefault (); 
				cbTransform.deactivateAltitude = 5000000;

				scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
				
				if (tmp != null) {
					Component.Destroy (tmp);
				}



//				if (scaledSpaceCamera.gameObject.GetComponent<drawSky> () != null)
//				{
//						Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<drawSky> ());
//				}

				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
				{
					Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ());
				}
				
				

				
				//				Transform transform = GetScaledTransform (parentCelestialBody.name);	///duplciate from Core but this is just a placeholder												
				//				{
				//					MeshRenderer planetMR = (MeshRenderer)transform.GetComponent (typeof(MeshRenderer));
				//					if (planetMR != null) {														
				//						planetMR.material.renderQueue = 2002;
				//					}
				//				}	
				
				
				if (postprocessingEnabled) {
					farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
					if (farCamera.gameObject.GetComponent<scatterPostprocess> () != null) {
						initiated = true;
					}
				} else {
					initiated = true;
				}



			}

//			atmosphereMesh.transform.parent = FlightCamera.fetch.transform;
			//atmosphereMesh.transform.position = Vector3.zero;
			atmosphereMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;

			atmosphereMesh.transform.localRotation = farCamera.transform.localRotation;
			atmosphereMesh.transform.rotation = farCamera.transform.rotation;
			atmosphereMesh.transform.Rotate (new Vector3 (postRotX, postRotY, postRotZ), Space.Self);

			//parent = farCamera.transform;
			//atmosphereMesh.transform.localRotation=new Vector3(
			//atmosphereMesh.transform.localPosition = Vector3.zero;
			//atmosphereMesh.transform.localScale = Vector3.one;
//			atmosphereMesh.layer = 10;
			atmosphereMesh.layer = 15;

			var mr = hp.GameObject.GetComponent<MeshRenderer>();
			mr.material = m_atmosphereMaterial;

			
			alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
			if ((sunglareEnabled) ^ (alt < sunglareCutoffAlt)) { //^ is XOR
				toggleSunglare ();
			}

			if ((coronasDisabled) ^ (alt < sunglareCutoffAlt)) { //^ is XOR
				toggleCoronas ();
			}

			if ((!stocksunglareEnabled) ^ (alt < sunglareCutoffAlt-1000)) { //^ is XOR
				toggleStockSunglare();
			}

//			print ("alphacutoff");
//			print (alphaCutoff);
			
//			if (alt<sunglareCutoffAlt){
//				alphaCutoff=0.0001f;}
//			else
//			{
//				if(alt < 7*(sunglareCutoffAlt-m_radius)+m_radius)
//				{					
////					alphaCutoff=0.299f * (alt-sunglareCutoffAlt)/(5*(sunglareCutoffAlt-m_radius)) +0.001f;
//					float factor=((alt-sunglareCutoffAlt) / (7*(sunglareCutoffAlt-m_radius)));
//					alphaCutoff=0.00001f * factor +0.0001f;
//					print ("alphacutoff");
//					print (alphaCutoff);
//				}
//				else
//					alphaCutoff=0.00011f;
//			}
			
			
			
			//			if (alt < (100000 + m_radius)) {
			//				extinctionCoeff = 0.7f;
			//			}
			//				else 
			//			{
			//				extinctionCoeff=0.7f +(1.1f/300000f) * (alt-m_radius-100000f);
			//			}
			//
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
			
			if ((alt > 1000000f +m_radius) && (!inScaledSpace)) 
			{
				farCamera.farClipPlane = 7000000;
				
				//				farCamera.nearClipPlane = alt - m_radius - 350000f;
				farCamera.nearClipPlane = 100000f;
				
				nearCamera.farClipPlane = 099999f;
			}
			
			
			
			else{
				
				if ((alt > 200000f +m_radius) && (!inScaledSpace)) {
					farCamera.farClipPlane = 2000000;
					//				farCamera.nearClipPlane = alt - m_radius - 350000f;
					farCamera.nearClipPlane = 150000f;
					nearCamera.farClipPlane = 149999f;
				}
				
				
				
				else 			
				{
					farCamera.farClipPlane = 750000f;
					farCamera.nearClipPlane = 300f;
					nearCamera.farClipPlane = 300f;
				}
				
			}
			
			
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
			
			//adding post processing to camera
			if ((!inScaledSpace) || (MapView.MapIsEnabled)) {
				if (postprocessingEnabled) {
					InitPostprocessMaterial (m_atmosphereMaterial);
					UpdatePostProcessMaterial (m_atmosphereMaterial);
					
					if (scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> () != null) {
//						print ("ScaledSpaceCamera scatterPostprocess!=null");
						Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> ());
					}
					
					
					if (farCamera.gameObject.GetComponent<scatterPostprocess> () == null) {
//						print ("farCamera scatterPostprocess==null");
						farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
					}
					
					farCamera.gameObject.GetComponent<scatterPostprocess> ().setMaterial (m_atmosphereMaterial);
				}
			}
			//
			//			else 
			//			
			//			{
			//				if (postprocessingEnabled) {
			//					InitPostprocessMaterial (m_atmosphereMaterial);
			//					UpdatePostProcessMaterial (m_atmosphereMaterial);
			//					
			//					if (farCamera.gameObject.GetComponent<scatterPostprocess> () != null) {
			//						Component.Destroy(farCamera.gameObject.GetComponent<scatterPostprocess> ());
			//					}
			//
			//					if (scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> () == null) {
			//						scaledSpaceCamera.gameObject.AddComponent (typeof(scatterPostprocess));
			//					}
			//					
			//					scaledSpaceCamera.gameObject.GetComponent<scatterPostprocess> ().setMaterial (m_atmosphereMaterial);
			//				}						
			//			
			//			}
			
			
			
			
			//adding sky to camera
			
			//			if ((!inScaledSpace)||(MapView.MapIsEnabled)) ///For now I disabled the effect in mapview because it still lags behind
			//if (1==0)
//			{
//				MR.enabled = false;
//
//				if (farCamera.gameObject.GetComponent<drawSky> () == null)
//				{
//					farCamera.gameObject.AddComponent (typeof(drawSky));
//				}
//
//
//				
//				if (scaledSpaceCamera.gameObject.GetComponent<drawSky> () != null)
//				{
//					Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<drawSky> ());
//				}
//				
//				if (farCamera.gameObject.GetComponent<drawSky> () != null)
//				{
//					
//					m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
//					//					InitUniforms (m_skyMaterial);
//					//					SetUniforms (m_skyMaterial);
//					farCamera.gameObject.GetComponent<drawSky> ().settings (m_skyMaterial, position, m_mesh, m_manager, this, farCamera, layer);
//					
//				}												
//			}
			
//			else
			{
				///when in scaledSpace
				
				tester.layer = 10;
				
				
				//			InitUniforms (m_skyMaterialScaled);
				//			SetUniforms(m_skyMaterialScaled);
				
				///				moved to !intialized block
				
				//				m_skyMaterialScaled.renderQueue=2001;
				//
				//				Transform transform = GetScaledTransform (parentCelestialBody.name);	///duplciate from Core but this is just a placeholder												
				//				{
				//					MeshRenderer planetMR = (MeshRenderer)transform.GetComponent (typeof(MeshRenderer));
				//					if (planetMR != null)
				//					{														
				//						planetMR.material.renderQueue = 2002;
				//					}
				//				}										
				
				
				MF.mesh = m_mesh;
				//m_skyMaterial.renderQueue = 2000;
//				MR.sharedMaterial = m_skyMaterial;
				MR.material =m_skyMaterialScaled;
				
				
				MR.castShadows = false;
				MR.receiveShadows = false;
				
				
				
				//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,idekk,layer,cams[cam]);
				
				MR.sharedMaterial = m_skyMaterialScaled;
				
				MR.enabled = true;
				
				
				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () == null)
				{
//					print ("ScaledSpaceCamera updateAtCameraRythm==null");
					scaledSpaceCamera.gameObject.AddComponent(typeof(updateAtCameraRythm));
				}
				
				if (farCamera.gameObject.GetComponent<drawSky> () != null)
				{
//					print ("FarCamera drawsky!=null");
					Component.Destroy(farCamera.gameObject.GetComponent<drawSky> ());
				}
				
				if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
				{
//					print ("ScaledSpaceCamera updateAtCameraRythm!=null");
					scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ().settings (m_skyMaterialScaled, m_manager,this,tester,debugSettings[6],parentCelestialBody);										
				}
				
			}
			
			
			
			//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cams[cam]);
			
			
			//						if (debugSettings[6]){
			//						tester.transform.parent = parentCelestialBody.transform;
			//						}
			//			
			//						else{
			//							Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			//							tester.transform.parent = celestialTransform;
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
			
			SetUniforms(m_skyMapMaterial);
			Graphics.Blit(null, m_skyMap, m_skyMapMaterial);
			
			
		}
		
		
		public void SetUniforms(Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if(mat == null) return;
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			
			mat.SetFloat ("_Alpha_Cutoff", alphaCutoff);
			mat.SetFloat ("_Alpha_Global", alphaGlobal);
			
			mat.SetFloat("scale",atmosphereGlobalScale);
			mat.SetFloat("Rg", Rg*atmosphereGlobalScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale);
			mat.SetFloat("RL", RL*atmosphereGlobalScale);
			
			//			if (debugSettings [5])
			if(!MapView.MapIsEnabled)
			{
				mat.SetFloat ("_Globals_ApparentDistance", apparentDistance);
			}
			else
			{
				mat.SetFloat("_Globals_ApparentDistance", (float)(parentCelestialBody.Radius/100.2f));
			}
			
			
			//			if (debugSettings[1])
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
			mat.SetTexture("_Sky_Map", m_skyMap);
			mat.SetFloat("_Sun_Intensity", 100f);
			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun().normalized);
			//			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun());
			
			
			//			//copied from m_manager's set uniforms
			
			//Matrix4x4 p;
			//			if (debugSettings [2])
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
								
				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
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
			mat.SetTexture("_Sky_Map", m_skyMap);
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
				
				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
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
				
				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
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




//			Matrix4x4 viewMat = farCamera.worldToCameraMatrix;
			viewMat = viewMat.Inverse ();
			Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);
			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4());          
			mat.SetMatrix ("_ViewProjInv", viewProjMat.inverse);



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
			mat.SetFloat("_Alpha_Cutoff", alphaCutoff);
			
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
		
		public void SetExposure(float expo)
		{
			m_HDRExposure=expo;
		}
		
		public void SetExtinctionCoeff(float exCoeff)
		{
			extinctionCoeff = exCoeff;
		}
		
		
		public void SetAlphaCutoff(float cutoff)
		{
			alphaCutoff = cutoff;
		}
		
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
			m_skyMap.Create();
			
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);
			
			
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
			m_skyMap.Release ();
			
			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			
			if(tmp != null)
			{
				Component.Destroy (tmp);
			}
			m_skyMap.Release();

			if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null)
			{
				Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ());
			}


			Component.Destroy (MR);
			Destroy (tester);
		}

		public void destroyTester()
		{
			Destroy (tester);
		}
		
		
		public void toggleSunglare()
		{
			if (sunglareEnabled)
			{
				m_skyMaterialScaled.SetTexture ("_Sun_Glare", black);
				sunglareEnabled = false;
//				alphaCutoff=0.5f;
				m_skyMaterialScaled.renderQueue = 2001;
			}
			else
			{
				m_skyMaterialScaled.SetTexture("_Sun_Glare", sunGlare);
				sunglareEnabled=true;
//				alphaCutoff=0.001f;
				m_skyMaterialScaled.renderQueue = 2003;
				
				
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
//				Sun.Instance.enabled=false;




			} 

			else 
			{
				Sun.Instance.sunFlare.enabled = true;
//				Sun.Instance.enabled=false;


			}
			stocksunglareEnabled = !stocksunglareEnabled;
		}

		public void toggleAniso(){
			if (!forceOFFaniso) { 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			}

			else
			{ 
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			}

			forceOFFaniso = !forceOFFaniso;
		}
		
		public Transform GetScaledTransform(string body)
		{
			List<Transform> transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single(n => n.name == body);
		}
		
		//		public void findPrefabBodies(PSystemBody body)
		//		{
		//			prefabs[((CelestialBody)body.celestialBody).name] = body;
		//			using (List<PSystemBody>.Enumerator enumerator = ((List<PSystemBody>)body.children).GetEnumerator())
		//			{
		//				while (enumerator.MoveNext())
		//					this.findPrefabBodies(enumerator.Current);
		//
		//
		//			}
		//		}
		
		
		
		
		
		//		public RenderTexture getInscatter()
		//		{
		//			return m_inscatter;
		//		}
		
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
		
		public void saveSettings()
		{
			cfg ["Rg"] = Rg.ToString();
			cfg ["Rt"] = Rt.ToString();
			cfg ["RL"] = RL.ToString();
			
			cfg ["BETA_R"] = m_betaR;
			cfg ["BETA_MSca"] = BETA_MSca;
			cfg ["MIE_G"] = m_mieG.ToString();
			cfg ["HR"] = HR.ToString();
			cfg ["HM"] = HM.ToString();
			cfg ["AVERAGE_GROUND_REFLECTANCE"] = AVERAGE_GROUND_REFLECTANCE.ToString();
			
			cfg.save ();
		}

		public void loadFromConfigNode() {
			ConfigNode cnToLoad = ConfigNode.Load(path+"/config/Settings.txt");

			extinctionCoeff = (float)(Convert.ToDouble(cnToLoad.GetValue ("extinctionCoeff")));

			atmosphereGlobalScale = (float)(Convert.ToDouble(cnToLoad.GetValue ("atmosphereGlobalScale")));
			postProcessingAlpha = (float)(Convert.ToDouble(cnToLoad.GetValue ("postProcessingAlpha")));
			postProcessingScale = (float)(Convert.ToDouble(cnToLoad.GetValue ("postProcessingScale")));
			postProcessDepth = (float)(Convert.ToDouble(cnToLoad.GetValue ("postProcessDepth")));


			postProcessExposure = (float)(Convert.ToDouble(cnToLoad.GetValue ("postProcessExposure")));
			m_HDRExposure = (float)(Convert.ToDouble(cnToLoad.GetValue ("m_HDRExposure")));
			alphaCutoff = (float)(Convert.ToDouble(cnToLoad.GetValue ("alphaCutoff")));
			alphaGlobal = (float)(Convert.ToDouble(cnToLoad.GetValue ("alphaGlobal")));
			forceOFFaniso = Convert.ToBoolean(cnToLoad.GetValue ("forceOFFaniso"));

		}


		public void saveToConfigNode() {
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject(this);
			cnTemp.Save(path+"/config/Settings.txt");
		}

		//custom graphicsBlit for the postprocessing,
		//originally this was in scatterpostprocess class but I moved it here
		//because the shader is no longer called in postprocessing
		static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr) 
		{
			RenderTexture.active = dest;
			
			//fxMaterial.SetTexture ("_MainTex", source);	        
			
			GL.PushMatrix ();
			GL.LoadOrtho ();
			
			fxMaterial.SetPass (passNr);	
			
			GL.Begin (GL.QUADS);
			
			//This custom blit is needed as infomation about what corner verts relate to what frustum corners is needed
			//A index to the frustum corner is store in the z pos of vert
			
			GL.MultiTexCoord2 (0, 0.0f, 0.0f); 
			GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
			
			GL.MultiTexCoord2 (0, 1.0f, 0.0f); 
			GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
			
			GL.MultiTexCoord2 (0, 1.0f, 1.0f); 
			GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
			
			GL.MultiTexCoord2 (0, 0.0f, 1.0f); 
			GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
			
			GL.End ();
			GL.PopMatrix ();
			
		}	
	}
}