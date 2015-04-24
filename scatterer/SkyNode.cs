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

		public int renderQueue=2000;
		public int renderQueue2=2010;

		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType<SkyNode>(null);
		
		//bool inScaledSpace=false;

		public bool[] debugSettings= new bool[7];

		public float apparentDistance=1f;

		GameObject tester;
		MeshRenderer MR;
		MeshFilter MF;

		CelestialBody parentCelestialBody;
		Matrix4x4 m_sun_worldToLocalRotation;
		
		bool sunglareEnabled=true;
		float sunglareCutoffAlt;
		
		Texture2D sunGlare;
		Texture2D black;
		
		//atmosphere properties
		float extinctionCoeff=0.3f;
		float atmosphereGlobalScale=1f;
		float postProcessingAlpha=0.78f;
		float postProcessingScale=1f;
		float postProcessDepth=0.02f;
		float postProcessExposure=0.18f;
		//		float inscatteringCoeff=0.8f; //useless, I also removed it from shader
		
		float m_HDRExposure= 0.2f;
		
		static PQS CurrentPQS=null;
		static bool inScaledSpace { get { return !(CurrentPQS != null && CurrentPQS.isActive);} }
		
		Vector3 position;
		
		bool initiated=false;
		Camera[] cams;
		Camera farCamera, scaledSpaceCamera;
		public bool postprocessingEnabled=true;
		int waitBeforeReloadCnt=0;
		
		float alphaCutoff=0f;
		float alphaGlobal=1f;
		
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

		Material idekk;
		
		[SerializeField]
		Material m_skyMaterial;
		Material m_skyMaterialScaled;
		
		//		[SerializeField]
		//		Material m_skyMapMaterial;
		
		//scatter coefficient for rayliegh
		[SerializeField]
		Vector3 m_betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[SerializeField]
		float m_mieG = 0.85f;
		
		string m_filePath = "/Proland/Textures/Atmo";
		
		Mesh m_mesh;
		
		RenderTexture m_transmit, m_inscatter, m_irradiance;//, m_skyMap;//, m_inscatterGround, m_transmitGround;
		
		Manager m_manager;
								
		
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
			m_skyMaterial=new Material(ShaderTool.GetMatFromShader2("CompiledSky.shader"));
			m_skyMaterialScaled=new Material(ShaderTool.GetMatFromShader2("CompiledSkyScaled.shader"));
			//m_skyMaterial.renderQueue = 2000;
			
			sunGlare = new Texture2D (512, 512);
			black = new Texture2D (512, 512);
			
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
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
				m_skyMaterial.SetTexture("_Sun_Glare", sunGlare);
			}

			m_skyMaterialScaled.SetTexture("_Sun_Glare", black);
			
			
			InitUniforms(m_skyMaterial);
			InitUniforms(m_skyMaterialScaled);
			m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
			
			//aniso defaults to to forceEnable on higher visual settings and causes artifacts
			//no longer needed since I switched to the new mesh
			//QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
			
			CurrentPQS = parentCelestialBody.pqsController;

			for (int j=0;j<7;j++){
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
			tester = new GameObject ();
			MF = tester.AddComponent<MeshFilter>();
			Mesh idmesh = MF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
//
			tester.layer = layer;
			Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
//			tester.transform.parent = parentCelestialBody.transform;
			tester.transform.parent = celestialTransform;


			MR = tester.AddComponent<MeshRenderer>();

//			InitUniforms (m_skyMaterialScaled);
//			SetUniforms(m_skyMaterialScaled);

			MR.sharedMaterial = m_skyMaterialScaled;
			MR.material =m_skyMaterialScaled;

			MR.castShadows = false;
			MR.receiveShadows = false;
			MR.enabled = true;


		



//			tester.transform.localPosition = Vector3.zero;
//			tester.transform.localRotation = Quaternion.identity;
//			tester.transform.localScale = Vector3.one;


//			MR.enabled = true;

		}
		
		public void UpdateNode()
		{
			
			
			m_radius=m_manager.GetRadius();
			//m_radius = 600000.0f;
			
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
			sunglareCutoffAlt = Rt * 0.995f * atmosphereGlobalScale;
			
			//			if(inScaledSpace)
			//			{
			//				position=(ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name)).position;
			//			}
			//			else{
			position= parentCelestialBody.transform.position;
			//			}
			
			//			print ("In scaled Space");
			//			print (inScaledSpace);
			
			
			if (!initiated)   //gets the cameras, this isn't done at start() because the cameras still don't exist then and it crashes the game
			{
				cams = Camera.allCameras;
				
				for (int i=0; i<cams.Length; i++)
				{
					if (cams [i].name == "Camera ScaledSpace")
						scaledSpaceCamera = cams [i];
					if (cams [i].name == "Camera 01")
						farCamera = cams [i];
				}
				scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
				
				if(tmp!= null)
				{
					Component.Destroy (tmp);
				}
				
				if (postprocessingEnabled)
				{
					farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
					if(farCamera.gameObject.GetComponent<scatterPostprocess> () != null)
					{
						initiated =true;
					}
				}
				
				else
				{
					initiated =true;
				}
			}
			
			


			
			
			
			float alt = Vector3.Distance(farCamera.transform.position, parentCelestialBody.transform.position);
			if ((sunglareEnabled)^(alt < sunglareCutoffAlt)) //^ is XOR
			{
				toggleSunglare();
			}
			
			
			//if alt-tabbing/windowing and rendertextures are lost
			//this loads them back up
			//you have to wait for a frame of two because if you do it immediately they don't get loaded
			if (!m_inscatter.IsCreated ())
			{
				waitBeforeReloadCnt++;
				if (waitBeforeReloadCnt>=2)
				{
					
					initiateOrRestart ();
					print ("Scatterer: reloaded scattering tables");
					waitBeforeReloadCnt=0;
				}
			}
			
			//adding post processing to camera
			if (postprocessingEnabled)
			{
				InitPostprocessMaterial(m_atmosphereMaterial);
				UpdatePostProcessMaterial (m_atmosphereMaterial);
				
				if(farCamera.gameObject.GetComponent<scatterPostprocess> () == null)
				{
					farCamera.gameObject.AddComponent (typeof(scatterPostprocess));
				}
				
				farCamera.gameObject.GetComponent<scatterPostprocess>().setMaterial(m_atmosphereMaterial);
			}
			
			//adding sky to camera

//			if (!inScaledSpace) {
//
//				if (farCamera.gameObject.GetComponent<drawSky> () == null) {
//					farCamera.gameObject.AddComponent (typeof(drawSky));
//				}
//
//				if (scaledSpaceCamera.gameObject.GetComponent<drawSky> () != null) {
//					Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<drawSky> ());
//				}
//			
//				if (farCamera.gameObject.GetComponent<drawSky> () != null) {
//
//					m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
////					InitUniforms (m_skyMaterial);
////					SetUniforms (m_skyMaterial);
//					farCamera.gameObject.GetComponent<drawSky> ().settings (m_skyMaterial, position, m_mesh, m_manager, this, farCamera, layer);
//
//				}
//
//			}
//
//			if (inScaledSpace) {
//				
//				if (scaledSpaceCamera.gameObject.GetComponent<drawSky> () == null) {
//					scaledSpaceCamera.gameObject.AddComponent (typeof(drawSky));
//				}
//				
//				if (farCamera.gameObject.GetComponent<drawSky> () != null) {
//					Component.Destroy(farCamera.gameObject.GetComponent<drawSky> ());
//				}
//				
//				if (scaledSpaceCamera.gameObject.GetComponent<drawSky> () != null) {
//					m_skyMaterialScaled.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
//					m_skyMaterialScaled.renderQueue=renderQueue;
//					print ("RENDERQUEUE");
//					print (renderQueue);
//					scaledSpaceCamera.gameObject.GetComponent<drawSky> ().settings (m_skyMaterialScaled, position, m_mesh, m_manager, this, scaledSpaceCamera, 10);
////					InitUniforms (m_skyMaterialScaled);
////					SetUniforms (m_skyMaterialScaled);
//
//				}
//				
//			}

//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cams[cam]);


//			if (debugSettings[6]){
//			tester.transform.parent = parentCelestialBody.transform;
//			}
//
//			else{
//				Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
//				tester.transform.parent = celestialTransform;
//			}



							if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () == null) {
							scaledSpaceCamera.gameObject.AddComponent(typeof(updateAtCameraRythm));
							}
							
							
								if (scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> () != null) {
//								m_skyMaterialScaled.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
//								m_skyMaterialScaled.renderQueue=renderQueue;
//								print ("RENDERQUEUE");
//								print (renderQueue);
								scaledSpaceCamera.gameObject.GetComponent<updateAtCameraRythm> ().settings (m_skyMaterialScaled, m_manager,this);
			//					InitUniforms (m_skyMaterialScaled);
			//					SetUniforms (m_skyMaterialScaled);
			
							}




			tester.layer = layer;


//			InitUniforms (m_skyMaterialScaled);
//			SetUniforms(m_skyMaterialScaled);

			m_skyMaterialScaled.renderQueue=renderQueue;
			print ("RENDERQUEUE");
			print (renderQueue);
			MF.mesh = m_mesh;
			//m_skyMaterial.renderQueue = 2000;
			MR.sharedMaterial = m_skyMaterial;
			MR.material =m_skyMaterialScaled;

			
			MR.castShadows = false;
			MR.receiveShadows = false;
			MR.enabled = true;


//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,idekk,layer,cams[cam]);

			MR.sharedMaterial = m_skyMaterialScaled;
			





			
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

			if (debugSettings [5]) {
				mat.SetFloat ("_Globals_ApparentDistance", apparentDistance);
			} else {
				mat.SetFloat("_Globals_ApparentDistance", (float)(parentCelestialBody.Radius/1002f));
			}
			

			if (debugSettings[1]){

			mat.SetMatrix ("_Globals_WorldToCamera", farCamera.worldToCameraMatrix);
			mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);}

			else{
			mat.SetMatrix ("_Globals_WorldToCamera", cams[cam].worldToCameraMatrix);
			mat.SetMatrix ("_Globals_CameraToWorld", cams[cam].worldToCameraMatrix.inverse);}

			
						
			mat.SetVector("betaR", m_betaR / 1000.0f);
			mat.SetFloat("mieG", Mathf.Clamp(m_mieG, 0.0f, 0.99f));
			mat.SetTexture("_Sky_Transmittance", m_transmit);
			mat.SetTexture("_Sky_Inscatter", m_inscatter);
			mat.SetTexture("_Sky_Irradiance", m_irradiance);
			//mat.SetTexture("_Sky_Map", m_skyMap);
			mat.SetFloat("_Sun_Intensity", 100f);
			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun().normalized);
			//			mat.SetVector("_Sun_WorldSunDir", m_manager.getDirectionToSun());
			
			
			//			//copied from m_manager's set uniforms
						
			Matrix4x4 p;
			if (debugSettings [2]) {
				p = farCamera.projectionMatrix;
			} else {
				p = scaledSpaceCamera.projectionMatrix;
			}


			Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());

			if (debugSettings [3]) {
				mat.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position);
			} else {
				Vector3 newpos= ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position);
//				m_skyMaterial.SetVector ("_Globals_WorldCameraPos", scaledSpaceCamera.transform.position);
				mat.SetVector ("_Globals_WorldCameraPos", newpos);
			}

			if (debugSettings [4]) {
				mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
			} else {
			

				Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				Vector3 idek =celestialTransform.position;
				mat.SetVector ("_Globals_Origin", idek);
			
			}
			
			mat.SetFloat ("_Exposure", m_HDRExposure);
		}
		
		void InitPostprocessMaterial(Material mat)
		{
			mat.SetTexture("_Transmittance", m_transmit);
			mat.SetTexture("_Inscatter", m_inscatter);
			
			//Consts, best leave these alone
			mat.SetFloat("M_PI", Mathf.PI);
			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*postProcessingScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*postProcessingScale);
			mat.SetFloat("Rl", RL*atmosphereGlobalScale*postProcessingScale);
			mat.SetFloat("RES_R", RES_R);
			mat.SetFloat("RES_MU", RES_MU);
			mat.SetFloat("RES_MU_S", RES_MU_S);
			mat.SetFloat("RES_NU", RES_NU);
			mat.SetFloat("SUN_INTENSITY", 100f);//
			mat.SetVector("_inCamPos", cams[cam].transform.position);
			mat.SetVector("SUN_DIR", m_manager.GetSunNodeDirection());
		}
		
		
		void UpdatePostProcessMaterial(Material mat)
		{
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*postProcessingScale);
			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*postProcessingScale);
			mat.SetFloat("Rl", RL*atmosphereGlobalScale*postProcessingScale);
			
			//mat.SetFloat("_inscatteringCoeff", inscatteringCoeff);
			mat.SetFloat("_extinctionCoeff", extinctionCoeff);
			mat.SetFloat("_global_alpha", postProcessingAlpha);
			mat.SetFloat("_Exposure", postProcessExposure);
			mat.SetFloat("_global_depth", postProcessDepth);
			
			mat.SetFloat("_Scale", postProcessingScale);
			//			mat.SetFloat("_Scale", 1);
			
			mat.SetVector ("_Globals_Origin", /*Vector3.zero-*/parentCelestialBody.transform.position);
			
			//			mat.SetMatrix ("_Globals_CameraToWorld", cams [0].worldToCameraMatrix.inverse);
			mat.SetMatrix ("_Globals_CameraToWorld", cams [0].worldToCameraMatrix.inverse);
			mat.SetVector ("_CameraForwardDirection", cams [cam].transform.forward);
			//mat.SetVector("betaR", m_betaR / (Rg / m_radius));
			//			mat.SetVector("betaR", m_betaR / (postProcessDepth));
			mat.SetVector("betaR", new Vector4(2.9e-3f, 0.675e-2f, 1.655e-2f, 0.0f));
			mat.SetFloat("mieG", 0.4f);
			mat.SetVector("SUN_DIR", /*Vector3.zero-*/m_manager.GetSunNodeDirection());
			mat.SetFloat("SUN_INTENSITY", 100f);
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
		
		public void SetNearPlane(int NR)
		{
			farCamera.gameObject.GetComponent<scatterPostprocess>().setNearPlane(NR);
		}
		
		public void SetFarPlane(int FR)
		{
			farCamera.gameObject.GetComponent<scatterPostprocess>().setFarPlane(FR);
		}
		
		public void setManager(Manager manager)
		{
			m_manager=manager;
		}
		
		public void enablePostprocess()
		{
			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			if (tmp == null)
			{
				farCamera.gameObject.AddComponent(typeof(scatterPostprocess));
			}
			postprocessingEnabled = true;
		}
		
		public void disablePostprocess()
		{
			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			if (tmp != null)
			{
				Component.Destroy (tmp);
				
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
			
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
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
			
			m_transmit.Release();
			m_irradiance.Release();
			m_inscatter.Release();
			
			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			
			if(tmp != null)
			{
				Component.Destroy (tmp);
			}
			//m_skyMap.Release();
			Destroy (tester);
		}
		
		
		public void toggleSunglare()
		{
			if (sunglareEnabled)
			{
				m_skyMaterial.SetTexture ("_Sun_Glare", black);
				sunglareEnabled = false;
				alphaCutoff=0f;
			}
			else
			{
				m_skyMaterial.SetTexture("_Sun_Glare", sunGlare);
				sunglareEnabled=true;
				alphaCutoff=0f;
				
				
			}
		}
		
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
		
	}
}