using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;





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
		float postProcessingAlpha=0.95f;
		float postProcessingScale=1f;
		float postProcessDepth=0.02f;
		float postProcessExposure=0.18f;



		Camera[] cams;
		bool postprocessingEnabled=false;
		int waitBeforeReloadCnt=0;
		GameObject idek=new GameObject();
		MeshFilter MF;
		MeshRenderer mr;
		float alphaCutoff=0.00f;
		float alphaGlobal=1f;
		//float m_radius = 63600.0f*4;
		float m_radius;// = 600000.0f;
		//The radius of the planet (Rg), radius of the atmosphere (Rt)
		//const float Rg = 63600.0f*4;
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
		
		const float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		//Half heights for the atmosphere air density (HR) and particle density (HM)
		//This is the height in km that half the particles are found below
		const float HR = 8.0f;
		const float HM = 1.2f;
		//scatter coefficient for mie
		readonly Vector3 BETA_MSca = new Vector3(4e-3f,4e-3f,4e-3f);

		public Material m_atmosphereMaterial;

		[SerializeField]
		Material m_skyMaterial;
		
		[SerializeField]
		Material m_skyMapMaterial;
		//scatter coefficient for rayliegh
		[SerializeField]
		Vector3 m_betaR = new Vector3(5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[SerializeField]
		float m_mieG = 0.85f;
		
		string m_filePath = "/Proland/Textures/Atmo";
		
		Mesh m_mesh;
		
		RenderTexture m_transmit, m_inscatter, m_irradiance, m_skyMap;//, m_inscatterGround, m_transmitGround;

		Manager m_manager;



		
		// Use this for initialization
		public void Start() 
		{

			//Component.Destroy(cams[cam].gameObject.GetComponent<scatterPostprocess>());
			m_radius=m_manager.GetRadius();
			//m_radius = 600000.0f;
			Rg = 600000;
			Rt = (64200f / 63600f) * m_radius;
			RL = (64210.0f/63600f) * m_radius;

			m_mesh = MeshFactory.MakePlane(2, 2, MeshFactory.PLANE.XY, false);
			m_mesh.bounds = new Bounds(m_manager.getParentCelestialBody().transform.position, new Vector3(1e8f,1e8f, 1e8f));
			

			//The sky map is used to create a reflection of the sky for objects that need it (like the ocean)
			//We don't need the skymap here, this is from the proland code
		/*	m_skyMap = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGBHalf);
			m_skyMap.filterMode = FilterMode.Trilinear;
			m_skyMap.wrapMode = TextureWrapMode.Clamp;
			m_skyMap.anisoLevel = 9;
			m_skyMap.useMipMap = true;
			//m_skyMap.mipMapBias = -0.5f;
			m_skyMap.Create();*/
			



						
			//Inscatter is responsible for the change in the sky color as the sun moves
			//The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture 
			//and the shader manually performs the sample for the 4th dimension
			//path = Application.dataPath + m_filePath + "/inscatter.raw";
			//path = Path.GetDirectoryName(path1)+m_filePath + "/inscatter.raw";

			m_inscatter = new RenderTexture(RES_MU_S * RES_NU, RES_MU * RES_R, 0, RenderTextureFormat.ARGBHalf);
			//m_inscatter = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBHalf);
			//m_inscatter.volumeDepth = RES_R;
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
//			m_inscatter.anisoLevel = 1;
			//m_inscatter.filterMode = FilterMode.Point;
			//m_inscatter.useMipMap = true;
//			m_inscatter.antiAliasing = 1;
			//m_inscatter.mipMapBias = -4f;
			//m_inscatter.isVolume = true;
			//m_inscatter.enableRandomWrite = true;

//			m_inscatterGround = new RenderTexture(RES_MU_S * RES_NU, RES_MU * RES_R, 0, RenderTextureFormat.ARGBHalf);
//			//m_inscatter = new RenderTexture(RES_MU_S * RES_NU, RES_MU, 0, RenderTextureFormat.ARGBHalf);
//			//m_inscatter.volumeDepth = RES_R;
//			m_inscatterGround.wrapMode = TextureWrapMode.Clamp;
//			m_inscatterGround.filterMode = FilterMode.Bilinear;





			//Transmittance is responsible for the change in the sun color as it moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1


			
			m_transmit = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBHalf);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;
//			m_transmit.anisoLevel = 1;
//			m_transmit.antiAliasing = 1;
			//m_transmit.filterMode =	FilterMode.Point;
			//m_transmit.useMipMap = true;
			//m_transmit.mipMapBias = -4f;
			//m_transmit.enableRandomWrite = true;

//			m_transmitGround = new RenderTexture(TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBHalf);
//			m_transmitGround.wrapMode = TextureWrapMode.Clamp;
//			m_transmitGround.filterMode = FilterMode.Bilinear;



									
			//Iirradiance is responsible for the change in the sky color as the sun moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1

			
			m_irradiance = new RenderTexture(SKY_W, SKY_H, 0, RenderTextureFormat.ARGBHalf);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;
//			m_irradiance.anisoLevel = 1;
//			m_irradiance.antiAliasing = 1;
			//m_irradiance.filterMode = FilterMode.Point;
			//m_irradiance.useMipMap = true;
			//m_irradiance.mipMapBias = -4f;
			//m_irradiance.enableRandomWrite = true;




			/*m_transmit.Create();
			m_inscatter.Create();
			m_irradiance.Create();
			
			
			string path = Application.dataPath + m_filePath + "/transmittance.raw";
			//path = Path.GetDirectoryName(path1)+m_filePath + "/transmit.raw";			
			EncodeFloat.WriteIntoRenderTexture (m_transmit, 3, path);
			
			path = Application.dataPath + m_filePath + "/irradiance.raw";
			EncodeFloat.WriteIntoRenderTexture (m_irradiance, 3, path);
			
			/*string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path1 = Uri.UnescapeDataString(uri.Path);
			string path = Path.GetDirectoryName(path1)+m_filePath + "/inscatter.raw";*/
			/*path = Application.dataPath + m_filePath + "/inscatter.raw";
			EncodeFloat.WriteIntoRenderTexture (m_inscatter, 4, path);*/

			initiateOrRestart ();			




			//m_skyMapMaterial=new Material(ShaderTool.GetMatFromShader("CompiledSkyMap.shader"));
			m_skyMaterial=new Material(ShaderTool.GetMatFromShader2("CompiledSky.shader"));



			//Texture2D sunGlare = Resources.Load ("sunglare") as Texture2D;
			Texture2D sunGlare = new Texture2D (512, 512);

			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);

			sunGlare.LoadImage(System.IO.File.ReadAllBytes(String.Format("{0}/{1}", path + m_filePath, "sunglare.png")));

			if (sunGlare == null) {
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");
				print ("SUNGLARE NULL");

			} else {
				sunGlare.wrapMode = TextureWrapMode.Clamp;
				m_skyMaterial.SetTexture("_Sun_Glare", sunGlare);

			}
			
			InitUniforms(m_skyMaterial);
			m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
//			if (postprocessingEnabled) {	
//				InitPostprocessMaterial(m_atmosphereMaterial);			 	
//				(cams[cam].gameObject.GetComponent<scatterPostprocess>()).setMaterial(m_atmosphereMaterial);
//			}



			//InitUniforms(m_skyMapMaterial);

			//aniso defaults to to forceEnable on higher visual settings and causes artifacts
			QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;

			
		}




		public void initiateOrRestart(){

			m_inscatter.Create ();
			m_transmit.Create ();
//			m_inscatterGround.Create ();
//			m_transmitGround.Create ();
			m_irradiance.Create ();

			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			path=Path.GetDirectoryName (path);

			
			string path1 = path + m_filePath + "/transmittance.raw";
			//path = Path.GetDirectoryName(path1)+m_filePath + "/transmit.raw";			


			EncodeFloat.WriteIntoRenderTexture (m_transmit, 3, path1);

//			path = Application.dataPath + m_filePath + "/transmittanceGround.raw";
//			//path = Path.GetDirectoryName(path1)+m_filePath + "/transmit.raw";			
//			EncodeFloat.WriteIntoRenderTexture (m_transmitGround, 3, path);
			
			path1 = path + m_filePath + "/irradiance.raw";

			EncodeFloat.WriteIntoRenderTexture (m_irradiance, 3, path1);
			
			/*string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path1 = Uri.UnescapeDataString(uri.Path);
			string path = Path.GetDirectoryName(path1)+m_filePath + "/inscatter.raw";*/
			path1 = path + m_filePath + "/inscatter.raw";

			EncodeFloat.WriteIntoRenderTexture (m_inscatter, 4, path1);

//			path = Application.dataPath + m_filePath + "/inscatterGround.raw";
//			EncodeFloat.WriteIntoRenderTexture (m_inscatterGround, 4, path);

		}

		public void SetRadius(float rad) {
			m_radius=rad;
		}

		public void SetAlphaCutoff(float cutoff) {
			alphaCutoff = cutoff;
		}

		public void SetAlphaGlobal(float glob) {
			alphaGlobal = glob;
		}
		
		public void OnDestroy()
		{
			//base.OnDestroy();
			
			m_transmit.Release();
			m_irradiance.Release();
			m_inscatter.Release();

//			m_inscatterGround.Release();
//			m_transmitGround.Release();
			//m_skyMap.Release();
		}
		

		public void setLayernCam(int inLayer, int inCam)
		{
			layer = inLayer;
			cam = inCam;
		}

		// Update is called once per frame
		public void UpdateNode() 
		{

			//if alt-tabbing/windowing and textures are lost

				if (!m_inscatter.IsCreated ()) {
				waitBeforeReloadCnt++;
				if (waitBeforeReloadCnt>=2){

					initiateOrRestart ();														
					print ("Scatterer: reloaded scattering tables");
					waitBeforeReloadCnt=0;
				}
			}

				m_mesh.bounds = new Bounds (m_manager.getParentCelestialBody ().transform.position, new Vector3 (1e8f, 1e8f, 1e8f));
				SetUniforms (m_skyMaterial);
				m_skyMaterial.SetFloat ("_Alpha_Cutoff", alphaCutoff);
				m_skyMaterial.SetFloat ("_Alpha_Global", alphaGlobal);

			if (postprocessingEnabled) {	

				InitPostprocessMaterial(m_atmosphereMaterial);			 	
				UpdatePostProcessMaterial (m_atmosphereMaterial);
				(cams[cam].gameObject.GetComponent<scatterPostprocess>()).setMaterial(m_atmosphereMaterial);
			}


				//SetUniforms (m_skyMapMaterial);			
				m_manager.SetUniforms (m_skyMaterial);
				m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ());

				cams = Camera.allCameras;
				Graphics.DrawMesh (m_mesh, m_manager.getParentCelestialBody ().transform.position, new Quaternion (0, 1, 0, 0), m_skyMaterial, layer, cams [cam]);






				//skyObject.layer = layer;



				//GameObject idek = new GameObject();
				//idek.la		yer=
				//Vessel playerVessel=FlightGlobals.ActiveVessel;
				//idek.transform.parent = m_manager.getParentCelestialBody ().transform;
				//idek.layer = layer;
				//mr.material = m_skyMaterial;
				//mr.enabled = true;
				//idek.collider.enabled = false;
				//mr.collider.enabled = false;


			
				//Update the sky map if...
				//The sun has moved
				//Or if this is first frame
				//And if this is not a deformed terrain (ie a planet). Planet sky map not supported
				//if((!m_manager.IsDeformed() && m_manager.GetSunNode().GetHasMoved()) || Time.frameCount == 1)
				//Graphics.Blit(null, m_skyMap, m_skyMapMaterial);

		}
		
		public void SetUniforms(Material mat)
		{	
			//Sets uniforms that this or other gameobjects may need
			if(mat == null) return;

			mat.SetFloat("scale",Rg /  m_radius);
			mat.SetVector("betaR", m_betaR / 1000.0f);
			mat.SetFloat("mieG", Mathf.Clamp(m_mieG, 0.0f, 0.99f));
			mat.SetTexture("_Sky_Transmittance", m_transmit);
			mat.SetTexture("_Sky_Inscatter", m_inscatter);
			mat.SetTexture("_Sky_Irradiance", m_irradiance);
			//mat.SetTexture("_Sky_Map", m_skyMap);
			mat.SetFloat("_Sun_Intensity", 100f);		
			mat.SetVector("_Sun_WorldSunDir", m_manager.GetSunNodeDirection());

		}

		void InitPostprocessMaterial(Material mat)
		{
			

			mat.SetTexture("_Transmittance", m_transmit);
			mat.SetTexture("_Inscatter", m_inscatter);
			
			//Consts, best leave these alone
			mat.SetFloat("M_PI", Mathf.PI);

			float SCALE = Rg / m_radius; //
			//float SCALE = 1 / 100;

			//mat.SetFloat("SCALE", SCALE);
			mat.SetFloat ("Rg", Rg/postProcessingScale);
			mat.SetFloat("Rt", Rt/postProcessingScale);
			mat.SetFloat("Rl", RL/postProcessingScale);
//			mat.SetFloat("RES_R", 32.0f);
//			mat.SetFloat("RES_MU", 128.0f);
//			mat.SetFloat("RES_MU_S", 32.0f);
//			mat.SetFloat("RES_NU", 8.0f);

			mat.SetFloat("RES_R", RES_R);
			mat.SetFloat("RES_MU", RES_MU);
			mat.SetFloat("RES_MU_S", RES_MU_S);
			mat.SetFloat("RES_NU", RES_NU);

			mat.SetFloat("SUN_INTENSITY", 100f);//

//			mat.SetVector("EARTH_POS", new Vector3(0.0f, 6360010.0f, 0.0f));
//			mat.SetVector("SUN_DIR", m_sun.transform.forward*-1.0f);


			//mat.SetVector("EARTH_POS", cams[cam].transform.position-m_manager.getParentCelestialBody().transform.position);
			//mat.SetVector("EARTH_POS", new Vector3(0.0f, 0.0f, 0.0f));
			mat.SetVector("_inCamPos", cams[cam].transform.position);
			//mat.SetVector("SUN_DIR", new Vector3(0.5f,0.5f,0.5f));
			mat.SetVector("SUN_DIR", m_manager.GetSunNodeDirection());

			
		}

		void UpdatePostProcessMaterial(Material mat)
		{	
			mat.SetFloat ("Rg", Rg/postProcessingScale);
			mat.SetFloat("Rt", Rt/postProcessingScale);
			mat.SetFloat("Rl", RL/postProcessingScale);

			mat.SetFloat("_global_alpha", postProcessingAlpha);
			mat.SetFloat("_Exposure", postProcessExposure);
			mat.SetFloat("_global_depth", postProcessDepth);
//			mat.SetFloat("_global_depth", 1);
//			mat.SetFloat("_Scale", postProcessingScale);
			mat.SetFloat("_Scale", 1);

//			print ("SCALE");
//			print (postProcessingScale);





			mat.SetVector ("_Globals_Origin", /*Vector3.zero-*/m_manager.getParentCelestialBody().transform.position);	
			//uniform float3 _Globals_Origin;
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
			
			mat.SetFloat("scale",Rg /  m_radius);
			mat.SetFloat("Rg", Rg);
			mat.SetFloat("Rt", Rt);
			mat.SetFloat("RL", RL);

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
			m_skyMaterial.SetFloat("_Alpha_Cutoff", alphaCutoff);
			
		}
		
		/*void OnGUI(){
			GUI.DrawTexture(new Rect(0,0,512, 512), m_skyMap);
		}*/
		
	
		public void SetNearPlane(int NR) {
			cams[cam].gameObject.GetComponent<scatterPostprocess>().setNearPlane(NR);
		}
		
		
		
		public void SetFarPlane(int FR) {
			cams[cam].gameObject.GetComponent<scatterPostprocess>().setFarPlane(FR);
		}


		public void setManager(Manager manager)
		{
			m_manager=manager;
		}

		public void enablePostprocess(){

			//Component.Destroy(cams[cam].gameObject.GetComponent<scatterPostprocess>());
				cams[cam].gameObject.AddComponent(typeof(scatterPostprocess));
				//cams[cam+1].gameObject.AddComponent(typeof(scatterPostprocess));
			postprocessingEnabled = true;
		}
		
		public void disablePostprocess(){
			Component.Destroy(cams[cam].gameObject.GetComponent<scatterPostprocess>());
			//Component.Destroy(cams[cam+1].gameObject.GetComponent<scatterPostprocess>());
			postprocessingEnabled = false;
		}


		public void SetPostProcessExposure(float postExposure) {
			postProcessExposure=postExposure;
		}
		
		public void SetPostProcessDepth(float postDepth) {
			postProcessDepth=postDepth;
		}
		
		public void SetPostProcessAlpha(float postAlpha) {
			postProcessingAlpha=postAlpha;
		}

		public void SetPostProcessScale(float postScale) {
			postProcessingScale=postScale;
		}

		

		public RenderTexture getInscatter()
		{
			return m_inscatter;
		}
	
	}
}
