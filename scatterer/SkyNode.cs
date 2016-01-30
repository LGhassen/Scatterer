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
	public class SkyNode: MonoBehaviour
	{
		
		[Persistent]
		public bool forceOFFaniso;
		SimplePostProcessCube hp;
		GameObject atmosphereMesh;
		MeshRenderer atmosphereMeshrenderer;
		
		//		[Persistent] public bool UIvisible = false;
		[Persistent]
		public bool displayInterpolatedVariables = false;
		
		//public CelestialBody[] celestialBodies;
		
		//		public float postRotX=0f,postRotY=0f,postRotZ=0f,
		//		public float postScaleX=1f,postScaleY=1f,postScaleZ=1f;
		
		updateAtCameraRythm updater;
		bool updaterAdded = false;
		
		//		CustomDepthBufferCam customDepthBuffer;
		//		RenderTexture customDepthBufferTexture;
		
		public float postDist = -3500f;
		public float percentage;
		public int currentConfigPoint;
		float ExtinctionCutoff = 2.0f;
		bool coronasDisabled = false;


		[Persistent] public float experimentalAtmoScale=1f;
		float viewdirOffset=0f;

		//public int renderQueue = 2000;
		//public int renderQueue2 = 2010;
		Matrix4x4 p;
//		public float oceanNearPlane = 0.01f;
//		public float oceanFarPlane = 750000f;
//		public float terrainReflectance = 1f;
		public float sunIntensity = 100f;
//		public float irradianceFactor = 1f;
		public float oceanSigma = 0.04156494f;
		public float _Ocean_Threshold = 25f;
		[Persistent]
		public float sunglareScale = 1f;
//		[Persistent]
		public float extinctionMultiplier = 1f;
//		[Persistent]
		public float extinctionTint = 100f;
		public float skyExtinctionRimFade = 0f;
		[Persistent]
		public float mapExtinctionMultiplier = 1f;
		[Persistent]
		public float mapExtinctionTint = 1f;
		[Persistent]
		public float mapSkyExtinctionRimFade = 1f;

		public float openglThreshold = 250f;
		//		[Persistent] public float globalThreshold = 250f;
		public float edgeThreshold = 0.5f;
		//		[Persistent] public float horizonDepth=0.02f;
		
		public bool extinctionEnabled = true;
		
		//		string codeBase;
		//		UriBuilder uri;
		//		string path;
		
		float totalscale;
		//		float totalscale2;
		
		//int newRenderQueue;
		
		bool eclipse=false;

		Transform celestialTransform;
		Vector3 idek;
		float alt;
		public float trueAlt;
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType < SkyNode > (null);
		
		//bool inScaledSpace=false;
		
		//		public static Dictionary<string, PSystemBody> prefabs = new Dictionary<string, PSystemBody>();
		
		//		public bool[] debugSettings = new bool[10];
		public float[] additionalScales = new float[10];
		
		//PSystemBody[] pSystemBodies;
		//ScaledSpaceFader kerbinPsystemBody;
		
		[Persistent]
		public float MapViewScale = 1f;
		[Persistent]
		float postProcessMaxAltitude=160000;

		GameObject skyObject, skyExtinctObject;
		MeshRenderer skyMR, skyExtinctMR;
		MeshFilter skyMF, skyExtinctMF;
		CelestialBody parentCelestialBody;
		Transform ParentPlanetTransform;
		
		//Matrix4x4 m_sun_worldToLocalRotation;
		
		bool sunglareEnabled = true;
		bool stocksunglareEnabled = true;
		float sunglareCutoffAlt;
		Texture2D sunGlare;
		Texture2D black;
		
		//atmosphere properties
		
		/*[Persistent]*/
		public float extinctionCoeff = 0.7f;
		
		/*[Persistent]*/
		public float postProcessingAlpha = 0.78f;
		/*[Persistent]*/
		public float postProcessingScale = 1f;
		/*[Persistent]*/
		public float postProcessDepth = 0.02f;
		/*[Persistent]*/
		public float postProcessExposure = 0.18f;
		//		float inscatteringCoeff=0.8f; //useless, I also removed it from shader
		
		/*[Persistent]*/
		public float m_HDRExposure = 0.2f;
		public float m_rimHDRExposure = 0.2f;
		[Persistent]
		public float mapExposure = 0.15f;
		[Persistent]
		public float mapSkyRimExposure = 0.15f;

		PQS CurrentPQS = null;

//		ScaledSpaceFader currentScaledSpaceFader=null;

		PQSMod_CelestialBodyTransform currentPQSMod_CelestialBodyTransform=null;


		public bool inScaledSpace {
			get {
				return !(CurrentPQS != null && CurrentPQS.isActive);
			}
		}
		
		//PQS testPQS;
		
		Vector3 position;
		bool initiated = false;
		Camera[] cams;
		public Camera farCamera, scaledSpaceCamera, nearCamera;
		public bool postprocessingEnabled = true;
		int waitBeforeReloadCnt = 0;
		
		//		[Persistent] public float alphaCutoff=0.001f;
		/*[Persistent]*/
		public float alphaGlobal = 1f;
		[Persistent]
		public float mapAlphaGlobal = 1f;
		float m_radius; // = 600000.0f;
		//The radius of the planet (Rg), radius of the atmosphere (Rt)
		[Persistent]
		float Rg; // = 600000.0f;
		[Persistent]
		float Rt; // = (64200f/63600f) * 600000.0f;
		[Persistent]
		float RL; // = (64210.0f/63600f) * 600000.0f;
		[Persistent]
		public float atmosphereGlobalScale = 1f;
		
		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;
		const int RES_R = 32;
		const int RES_MU = 128;
		const int RES_MU_S = 32;
		const int RES_NU = 8;
		int layer = 15;
		int cam = 1;
		[Persistent]
		float AVERAGE_GROUND_REFLECTANCE = 0.1f;
		//Half heights for the atmosphere air density (HR) and particle density (HM)
		//This is the height in km that half the particles are found below
		[Persistent]
		float HR = 8.0f;
		[Persistent]
		float HM = 1.2f;
		//scatter coefficient for mie
		[Persistent]
		Vector3 BETA_MSca = new Vector3 (4e-3f, 4e-3f, 4e-3f);
		public Material m_atmosphereMaterial;
		public Material m_skyMaterialScaled;
		public Material m_skyExtinction;
		Material originalMaterial;
		Material alteredMaterial;
		[Persistent]
		Vector3 m_betaR = new Vector3 (5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[SerializeField]
		[Persistent]
		public float
			m_mieG = 0.85f;
		string m_filePath = "/Proland/Textures/Atmo";
		public Matrix4x4d m_cameraToScreenMatrix;
		Mesh m_mesh;
		RenderTexture m_transmit, m_inscatter, m_irradiance; //, m_inscatterGround, m_transmitGround;
		
		Manager m_manager;
		
		//		var cbTransform;
		
		[Persistent]
		public float rimBlend = 20f;
		[Persistent]
		public float rimpower = 600f;
		[Persistent]
		public float specR = 0f;
		[Persistent]
		public float specG = 0f;
		[Persistent]
		public float specB = 0f;
		[Persistent]
		public float shininess = 0f;
		[Persistent]
		public List < configPoint > configPoints = new List < configPoint > {
			new configPoint(5000f, 1f, 0.25f,0.25f, 1f, 0.4f, 0.23f, 1f, 100f,0f, 250f, 0.5f,0f), new configPoint(15000f, 1f, 0.15f,0.15f, 1f, 8f, 0.23f, 1f, 100f,0f, 250f, 0.5f,0f)
		};
		public string assetDir;
		
		//Initialization
		public void Start ()
		{
			m_radius = m_manager.GetRadius ();
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
			
			m_mesh = MeshFactory.MakePlane (2, 2, MeshFactory.PLANE.XY, false, false);
			m_mesh.bounds = new Bounds (parentCelestialBody.transform.position, new Vector3 (1e8f, 1e8f, 1e8f));
			
			//Inscatter is responsible for the change in the sky color as the sun moves
			//The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture
			//and the shader manually performs the sample for the 4th dimension
			m_inscatter = new RenderTexture (RES_MU_S * RES_NU, RES_MU * RES_R, 0, RenderTextureFormat.ARGBHalf);

			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			
			//Transmittance is responsible for the change in the sun color as it moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_transmit = new RenderTexture (TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBHalf);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;
			
			//Irradiance is responsible for the change in the sky color as the sun moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_irradiance = new RenderTexture (SKY_W, SKY_H, 0, RenderTextureFormat.ARGBHalf);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;
			
			
			initiateOrRestart ();
			m_skyMaterialScaled = new Material (ShaderTool.GetMatFromShader2 ("CompiledSkyScaled.shader"));
			//m_skyMaterialScaled.renderQueue = 2004;
			
			
			
			m_skyExtinction = new Material (ShaderTool.GetMatFromShader2 ("CompiledSkyExtinction.shader"));
			//m_skyExtinction.renderQueue = 2002;
			
			sunGlare = new Texture2D (512, 512);
			black = new Texture2D (512, 512);
			
			sunGlare.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", assetDir + m_filePath, "sunglare.png")));
			black.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", assetDir + m_filePath, "black.png")));
			
			sunGlare.wrapMode = TextureWrapMode.Clamp;
			m_skyMaterialScaled.SetTexture ("_Sun_Glare", sunGlare);
			
			InitUniforms (m_skyMaterialScaled);
			InitUniforms (m_skyExtinction);
			
			if (m_manager.GetCore ().render24bitDepthBuffer && !m_manager.GetCore ().d3d9) {
				m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter24bitdepth.shader");
			} else {
				m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
			}

			CurrentPQS = parentCelestialBody.pqsController;


			if (CurrentPQS)
			{
				currentPQSMod_CelestialBodyTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> () [0];
			}



			//testPQS = parentCelestialBody.pqsController;
			
			
			//			for (int j = 0; j < 10; j++) {
			//				debugSettings[j] = true;
			//			}
			
			for (int j = 0; j < 10; j++) {
				additionalScales [j] = 1f;
			}
			
			skyObject = new GameObject ();
			skyMF = skyObject.AddComponent < MeshFilter > ();
			Mesh idmesh = skyMF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
			//
			skyObject.layer = layer;
			//			celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			celestialTransform = ParentPlanetTransform;
			//			skyObject.transform.parent = parentCelestialBody.transform;
			skyObject.transform.parent = celestialTransform;
			
			skyMR = skyObject.AddComponent < MeshRenderer > ();
			skyMR.sharedMaterial = m_skyMaterialScaled;
			skyMR.material = m_skyMaterialScaled;
			skyMR.castShadows = false;
			skyMR.receiveShadows = false;
			
			///same for skyextinct
			skyExtinctObject = new GameObject ();
			skyExtinctMF = skyExtinctObject.AddComponent < MeshFilter > ();
			idmesh = skyExtinctMF.mesh;
			idmesh.Clear ();
			idmesh = m_mesh;
			//
			skyExtinctObject.layer = layer;
			skyExtinctObject.transform.parent = celestialTransform;
			
			skyExtinctMR = skyExtinctObject.AddComponent < MeshRenderer > ();
			skyExtinctMR.sharedMaterial = m_skyExtinction;
			skyExtinctMR.material = m_skyExtinction;
			skyExtinctMR.castShadows = false;
			skyExtinctMR.receiveShadows = false;
			
			hp = new SimplePostProcessCube (10000, m_atmosphereMaterial);
			atmosphereMesh = hp.GameObject;
			atmosphereMesh.layer = 15;
			atmosphereMeshrenderer = hp.GameObject.GetComponent < MeshRenderer > ();
			atmosphereMeshrenderer.material = m_atmosphereMaterial;
			
			//celestialBodies = (CelestialBody[]) CelestialBody.FindObjectsOfType(typeof(CelestialBody));
			
		}
		
		public void UpdateStuff () //to be called by update at camera rythm for some graphical stuff
		{
			RaycastHit hit;
			if (Physics.Raycast (scaledSpaceCamera.transform.position,
			                     ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position)
								 -scaledSpaceCamera.transform.position, out hit, Mathf.Infinity, (int)(1 << 10))) 
			{
//				print ("hit");
//				if(hit.collider)
//					print ("hit.collider"+hit.collider.name);
//				if (hit.rigidbody)
//					print ("hit.rigidbody"+hit.rigidbody.name );
//				if (hit.transform)
//					print ("layer"+hit.transform.gameObject.layer);
				eclipse=true;

			}
			else
			{
//				print ("no hit");
				eclipse=false;
			}
			
			atmosphereMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
			atmosphereMesh.transform.localRotation = farCamera.transform.localRotation;
			atmosphereMesh.transform.rotation = farCamera.transform.rotation;
			
			//adding post processing to camera
			if ((!inScaledSpace) && (!MapView.MapIsEnabled)) {
				if (postprocessingEnabled) {
					InitPostprocessMaterial (m_atmosphereMaterial);
					UpdatePostProcessMaterial (m_atmosphereMaterial);

					if (m_manager.hasOcean)
					{
						m_manager.GetOceanNode().SetUniforms(m_atmosphereMaterial);
					}
					
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
					
					//farCamera.gameObject.GetComponent<scatterPostprocess> ().setMaterial (m_atmosphereMaterial);
				}
			}
			
		}
		
		public void UpdateNode ()
		{
			position = parentCelestialBody.transform.position;
			
			if (!initiated) {
				m_radius = m_manager.GetRadius ();
				//m_radius = 600000.0f;
				
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;
//				sunglareCutoffAlt = (experimentalAtmoScale*(Rt + Rt) - Rg) /*  *0.995f*/;$
				sunglareCutoffAlt = experimentalAtmoScale*(Rt - Rg);
				cams = Camera.allCameras;
				
				for (int i = 0; i < cams.Length; i++) {
					if (cams [i].name == "Camera ScaledSpace")
						scaledSpaceCamera = cams [i];
					
					if (cams [i].name == "Camera 01")
						farCamera = cams [i];
					if (cams [i].name == "Camera 00")
						nearCamera = cams [i];
				}

				
				initiated = true;
				
				//				if (forceOFFaniso) {
				//					QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
				//				} else {
				//					QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
				//				}
				
				backupAtmosphereMaterial ();
				tweakStockAtmosphere ();
				
			} else {
				
				alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
				trueAlt = alt - m_radius;
				
				if ((sunglareEnabled) ^ ((trueAlt < sunglareCutoffAlt) && !MapView.MapIsEnabled && !eclipse)) { //^ is XOR
					toggleSunglare ();
				}
				
				if ((coronasDisabled) ^ (trueAlt < sunglareCutoffAlt)) { //^ is XOR
					toggleCoronas ();
				}
				
				if ((!stocksunglareEnabled) ^ ((trueAlt < sunglareCutoffAlt - 1000) && !MapView.MapIsEnabled)) { //^ is XOR
					toggleStockSunglare ();
				}
				
				interpolateVariables ();
				
				//if alt-tabbing/windowing and rendertextures are lost
				//this loads them back up
				//you have to wait for a frame of two because if you do it immediately they don't get loaded
				if (!m_inscatter.IsCreated ()) {
					waitBeforeReloadCnt++;
					if (waitBeforeReloadCnt >= 2) {


						m_inscatter.Release ();
						m_transmit.Release ();
						m_irradiance.Release ();

						initiateOrRestart ();
						Debug.Log ("[Scatterer] Reloaded scattering tables");
						m_manager.reBuildOcean ();
						waitBeforeReloadCnt = 0;
						Resources.UnloadUnusedAssets ();
						System.GC.Collect ();
					}
				}
				
				//farCamera.hdr = true;
				//nearCamera.hdr = true;
				//scaledSpaceCamera.hdr = true;
				
				{
					skyObject.layer = 10;
					skyExtinctObject.layer = 10;
					
					//					skyObject.layer = 15;
					//					skyExtinctObject.layer = 15;
					
					skyMF.mesh = m_mesh;
					skyMR.material = m_skyMaterialScaled;
					skyMR.castShadows = false;
					skyMR.receiveShadows = false;
					skyMR.sharedMaterial = m_skyMaterialScaled;
					skyMR.enabled = true;
					
					skyExtinctMF.mesh = m_mesh;
					skyExtinctMR.material = m_skyExtinction;
					skyExtinctMR.castShadows = false;
					skyExtinctMR.receiveShadows = false;
					skyExtinctMR.sharedMaterial = m_skyExtinction;
					skyExtinctMR.enabled = extinctionEnabled;
					
					if (scaledSpaceCamera && !updaterAdded) {
						
						/*
						if (!scaledSpaceCamera.gameObject.GetComponent < updateAtCameraRythm > ()) {
							scaledSpaceCamera.gameObject.AddComponent(typeof(updateAtCameraRythm));
							
						}
						*/
						
						updater = (updateAtCameraRythm)scaledSpaceCamera.gameObject.AddComponent (typeof(updateAtCameraRythm));
						updaterAdded = true;
						
						/*
						if (scaledSpaceCamera.gameObject.GetComponent < updateAtCameraRythm > ()) {
							scaledSpaceCamera.gameObject.GetComponent < updateAtCameraRythm > ().settings(m_mesh, m_skyMaterialScaled, m_skyExtinction, m_manager, this, skyObject, skyExtinctObject, debugSettings[6], parentCelestialBody.transform, celestialTransform);
						}
						*/
						
						updater.settings (m_mesh, m_skyMaterialScaled, m_skyExtinction, m_manager, this, skyObject,
						                 skyExtinctObject, true, parentCelestialBody.transform, celestialTransform);
					}
					
					//				if (scaledSpaceCamera.gameObject.GetComponent<cameraHDR> () == null)
					//				{
					//					//					print ("ScaledSpaceCamera updateAtCameraRythm==null");
					//					scaledSpaceCamera.gameObject.AddComponent(typeof(cameraHDR));
					//					scaledSpaceCamera.gameObject.GetComponent<cameraHDR> ().settings (this);
					
					//				if (farCamera.gameObject.GetComponent<cameraHDR> () == null)
					//				{
					//					//					print ("ScaledSpaceCamera updateAtCameraRythm==null");
					//					farCamera.gameObject.AddComponent(typeof(cameraHDR));
					//					farCamera.gameObject.GetComponent<cameraHDR> ().settings (this);
					//				}
					//
					//				if (farCamera.gameObject.GetComponent<cameraHDR> () != null)
					//				{
					//					farCamera.gameObject.GetComponent<cameraHDR> ().settings (this);
					//				}
					
				}
				
				
				atmosphereMeshrenderer.enabled = (!inScaledSpace) &&(postprocessingEnabled)&&(trueAlt<postProcessMaxAltitude);
				//				print (parentCelestialBody.name.ToString());
				//				print ("inscaled space"+inScaledSpace.ToString());
				//				print ("postprocess enabled"+postprocessingEnabled.ToString());
				
				
				//this snippet fixes the problem with the moon rendering over the atmosphere but behind the planet
				/* for (int k = 0;k < celestialBodies.Length; k++) {
					Transform tmpTransform;
					//				Transform tmpTransform =celestialBodies[k].transform;
					{
						newRenderQueue = 2001;
						
						if (celestialBodies[k].name != parentCelestialBody.name) {
							
							//quick RSS fix, change this to take the transform names from the core list later
							//otherwise this is hardcoded to Earth/Kerbin and won't work with other mods
							if (celestialBodies[k].name == "Earth") {
								tmpTransform = GetScaledTransform("Kerbin");
							} else {
								tmpTransform = GetScaledTransform(celestialBodies[k].name);
							}

							if (!MapView.MapIsEnabled) {
								if ((celestialBodies[k].transform.position - farCamera.transform.position).magnitude < (parentCelestialBody.transform.position - farCamera.transform.position).magnitude) {
									newRenderQueue = 2005;
								}
							}
						 else {
							//							if ((ScaledSpace.LocalToScaledSpace(celestialBodies[k].transform.position)-scaledSpaceCamera.transform.position).magnitude < (ScaledSpace.LocalToScaledSpace(parentCelestialBody.transform.position)-scaledSpaceCamera.transform.position).magnitude)
							if ((ParentPlanetTransform.position - scaledSpaceCamera.transform.position).magnitude < (ParentPlanetTransform.position - scaledSpaceCamera.transform.position).magnitude) {
								newRenderQueue = 2005;
							}
							
						}
						
					} else {
						tmpTransform = ParentPlanetTransform;
					}
					
					MeshRenderer mr2 = (MeshRenderer) tmpTransform.GetComponent(typeof(MeshRenderer));
					if (mr2 != null) {
						mr2.material.renderQueue = newRenderQueue;
					}
				}
			}*/
			}



//			Debug.Log ("scaledspace start:" + currentScaledSpaceFader.fadeStart.ToString ());
//			Debug.Log ("scaledspace end:" + currentScaledSpaceFader.fadeEnd.ToString ());


//			Debug.Log ("fade start:" + currentPQSMod_CelestialBodyTransform.planetFade.fadeStart.ToString ());
//			Debug.Log ("fade end:" + currentPQSMod_CelestialBodyTransform.planetFade.fadeEnd.ToString ());




		}
		
		public void SetUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if (mat == null)
				return;

			mat.SetFloat ("_experimentalAtmoScale", experimentalAtmoScale);
			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat ("_viewdirOffset", viewdirOffset);
			}
			else
			{
				mat.SetFloat ("_viewdirOffset", 0f);
			}

			mat.SetFloat ("_Extinction_Cutoff", ExtinctionCutoff);
			if (!MapView.MapIsEnabled) {
				mat.SetFloat ("_Alpha_Global", alphaGlobal);
				mat.SetFloat ("_Extinction_Tint", extinctionTint);
				mat.SetFloat ("extinctionMultiplier", extinctionMultiplier);
				mat.SetFloat ("extinctionRimFade", skyExtinctionRimFade);
			} else {
				mat.SetFloat ("_Alpha_Global", mapAlphaGlobal);
				mat.SetFloat ("_Extinction_Tint", mapExtinctionTint);
				mat.SetFloat ("extinctionMultiplier", mapExtinctionMultiplier);
				mat.SetFloat ("extinctionRimFade", mapSkyExtinctionRimFade);
			}
			
			mat.SetFloat ("scale", atmosphereGlobalScale);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("RL", RL * atmosphereGlobalScale);
			
			//						if (debugSettings [5])
			if (!MapView.MapIsEnabled) {
				mat.SetFloat ("_Globals_ApparentDistance", 1f);
				mat.SetFloat ("_rimQuickFixMultiplier", 2f);
			} else {
				mat.SetFloat ("_Globals_ApparentDistance", (float)((parentCelestialBody.Radius / 100.2f) / MapViewScale));
				//				mat.SetFloat ("_Globals_ApparentDistance", MapViewScale);
				mat.SetFloat ("_rimQuickFixMultiplier", 1f);
			}
			
			
			//						if (debugSettings[1])
			if (!MapView.MapIsEnabled) {
				
				mat.SetMatrix ("_Globals_WorldToCamera", farCamera.worldToCameraMatrix);
				mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			} else {
				mat.SetMatrix ("_Globals_WorldToCamera", scaledSpaceCamera.worldToCameraMatrix);
				mat.SetMatrix ("_Globals_CameraToWorld", scaledSpaceCamera.worldToCameraMatrix.inverse);
			}
			
			
			
			
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetFloat ("_Sun_Intensity", 100f);
			
			mat.SetFloat ("_sunglareScale", sunglareScale);
			
			mat.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToSun ().normalized);

			Shader.SetGlobalVector ("_Godray_WorldSunDir", m_manager.getDirectionToSun ().normalized);

//			Shader.SetGlobalFloat ("_Godray_WorldSunDirX", -1f);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirY", 5f);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirZ", 0f);

//			Shader.SetGlobalFloat ("_Godray_WorldSunDirX", m_manager.getDirectionToSun ().normalized.x);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirY", m_manager.getDirectionToSun ().normalized.y);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirZ", m_manager.getDirectionToSun ().normalized.z);

			Shader.SetGlobalFloat ("_Godray_WorldSunDirX", m_manager.sunCelestialBody.transform.position.x
			                       -parentCelestialBody.transform.position.x);
			Shader.SetGlobalFloat ("_Godray_WorldSunDirY", m_manager.sunCelestialBody.transform.position.y
			                       -parentCelestialBody.transform.position.y);
			Shader.SetGlobalFloat ("_Godray_WorldSunDirZ", m_manager.sunCelestialBody.transform.position.z
			                       -parentCelestialBody.transform.position.z);

			
			
			//			//copied from m_manager's set uniforms
			
			//Matrix4x4 p;
			//						if (debugSettings [2])
			if (!MapView.MapIsEnabled) {
				p = farCamera.projectionMatrix;
			} else {
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			//						if (debugSettings [3])
			if (!MapView.MapIsEnabled) {
				mat.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position);
			} else {
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
			if (!MapView.MapIsEnabled) {
				mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
				
			} else {
				
				//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				celestialTransform = ParentPlanetTransform;
				//				idek =celestialTransform.position;
				idek = celestialTransform.position - scaledSpaceCamera.transform.position;
				mat.SetVector ("_Globals_Origin", idek);
				
			}
			
			if (!MapView.MapIsEnabled) {
				mat.SetFloat ("_Exposure", m_HDRExposure);
				mat.SetFloat ("_RimExposure", m_rimHDRExposure);
			} else {
				mat.SetFloat ("_Exposure", mapExposure);
				mat.SetFloat ("_RimExposure", mapSkyRimExposure);
			}
			

//			//Eclipse stuff
//			mat.SetFloat ("LightRadius",(float) m_manager.sunCelestialBody.Radius*0.7f);
//			mat.SetVector ("Light0Pos", m_manager.sunCelestialBody.transform.position-parentCelestialBody.transform.position);
//			//sun position relative to planet
//			mat.SetVector ("LightCasterPosXYZ",m_manager.munCelestialBody.transform.position-parentCelestialBody.transform.position);
//			//occluder position relative to planet
//			mat.SetFloat ("LightCasterW", (float)m_manager.munCelestialBody.Radius);
//			//occluder radius


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
		
		public void SetOceanUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			
			//			mat.SetFloat ("_Alpha_Cutoff", alphaCutoff);
			//			mat.SetFloat ("_Alpha_Global", alphaGlobal);
			
			mat.SetFloat ("scale", atmosphereGlobalScale);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("RL", RL * atmosphereGlobalScale);
			
			
			
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
			
			
			
			mat.SetVector ("betaR", m_betaR / 1000.0f);
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetTexture ("_Sky_Transmittance", m_transmit);
			mat.SetTexture ("_Sky_Inscatter", m_inscatter);
			mat.SetTexture ("_Sky_Irradiance", m_irradiance);
			mat.SetFloat ("_Sun_Intensity", 100f);
			mat.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToSun ().normalized);

			//couldn't add parameters to a replacement shader so did it via a global shader propriety


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
			if (!MapView.MapIsEnabled) {
				mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
			} else {
				
				//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
				celestialTransform = ParentPlanetTransform;
				idek = celestialTransform.position;
				mat.SetVector ("_Globals_Origin", idek);
				
			}
			
//			mat.SetFloat ("_Exposure", m_HDRExposure);
			mat.SetFloat ("_Exposure", m_rimHDRExposure);
			
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
		
		public void InitPostprocessMaterial (Material mat)
		{
			
			totalscale = 1;
			
			/*
			for (int j = 0; j < 10; j++) {
			totalscale = totalscale * additionalScales[j];
		}
		*/
			
			mat.SetTexture ("_Transmittance", m_transmit);
			mat.SetTexture ("_Inscatter", m_inscatter);
			mat.SetTexture ("_Irradiance", m_irradiance);
			
			if (m_manager.GetCore ().render24bitDepthBuffer && !m_manager.GetCore ().d3d9)
				mat.SetTexture ("_customDepthTexture", m_manager.GetCore ().customDepthBufferTexture);
			
			
			//Consts, best leave these alone
			mat.SetFloat ("M_PI", Mathf.PI);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale * postProcessingScale * totalscale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale * postProcessingScale * totalscale);
			mat.SetFloat ("Rl", RL * atmosphereGlobalScale * postProcessingScale * totalscale);
			mat.SetFloat ("RES_R", RES_R);
			mat.SetFloat ("RES_MU", RES_MU);
			mat.SetFloat ("RES_MU_S", RES_MU_S);
			mat.SetFloat ("RES_NU", RES_NU);
			
			mat.SetFloat ("SKY_W", SKY_W);
			mat.SetFloat ("SKY_H", SKY_H);
			
			mat.SetVector ("betaR", m_betaR / 1000.0f);
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector ("betaMSca", BETA_MSca / 1000.0f);
			mat.SetVector ("betaMEx", (BETA_MSca / 1000.0f) / 0.9f);
			
			//		mat.SetFloat("_Ocean_Sigma", oceanSigma); //
			
			//		mat.SetFloat("SUN_INTENSITY", sunIntensity); //
			//			mat.SetVector("_inCamPos", cams[cam].transform.position);
			
			//		if (debugSettings[0]) {
			mat.SetVector ("_camPos", farCamera.transform.position-parentCelestialBody.transform.position);  //better do this small calculation here than have it redone 1000 times per frame in the shader
			//		} else {
			//			if (!debugSettings[4])
			//				
			//			{
			//				Vector3 pos = ScaledSpace.LocalToScaledSpace(scaledSpaceCamera.transform.position);
			//				mat.SetVector("_inCamPos", pos);
			//			} else //if (!debugSettings [0])
			//			{
			//				mat.SetVector("_inCamPos", scaledSpaceCamera.transform.position);
			//			}
			//			
			//			
			//		}
			
			mat.SetVector ("SUN_DIR", m_manager.GetSunNodeDirection ());
		}
		
		public void UpdatePostProcessMaterial (Material mat)
		{
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			//			mat.SetFloat ("Rg", Rg*atmosphereGlobalScale*postProcessingScale);
			//			mat.SetFloat("Rt", Rt*atmosphereGlobalScale*postProcessingScale);
			//			mat.SetFloat("Rl", RL*atmosphereGlobalScale*postProcessingScale);
			
			
			
			
			
			//		totalscale = 1;
			//		totalscale2 = 1;
			/*
			for (int j = 0; j < 5; j++) {
			totalscale = totalscale * additionalScales[j];
		}
		
		
		for (int j = 6; j < 10; j++) {
			totalscale2 = totalscale2 * additionalScales[j];
		}

*/

			mat.SetFloat ("_experimentalAtmoScale", experimentalAtmoScale);
//			mat.SetFloat ("_viewdirOffset", viewdirOffset);

//			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
//			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
//			mat.SetFloat ("Rl", RL * atmosphereGlobalScale);
			
			//mat.SetFloat("_inscatteringCoeff", inscatteringCoeff);
//			mat.SetFloat ("_extinctionCoeff", extinctionCoeff);
			mat.SetFloat ("_global_alpha", postProcessingAlpha);
			mat.SetFloat ("_Exposure", postProcessExposure);
			mat.SetFloat ("_global_depth", postProcessDepth*1000000);
			//		mat.SetFloat("_global_depth2", totalscale2);
			
			mat.SetFloat ("_openglThreshold", openglThreshold);
			//		mat.SetFloat("_horizonDepth", horizonDepth);
			//		mat.SetFloat("_globalThreshold", globalThreshold);
			mat.SetFloat ("_edgeThreshold", edgeThreshold);
			
			//		mat.SetFloat("terrain_reflectance", terrainReflectance);
			//		mat.SetFloat("_irradianceFactor", irradianceFactor);
			
			mat.SetFloat ("_Scale", postProcessingScale);
			//			mat.SetFloat("_Scale", 1);
			
//			mat.SetFloat ("_Ocean_Sigma", oceanSigma);
//			mat.SetFloat ("_Ocean_Threshold", _Ocean_Threshold);
			
			
			//			mat.SetMatrix ("_Globals_CameraToWorld", cams [0].worldToCameraMatrix.inverse);
			//		if (debugSettings[1]) {
			mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			//		} else {
			//			mat.SetMatrix("_Globals_CameraToWorld", scaledSpaceCamera.worldToCameraMatrix.inverse);
			//		}
			
			//		if (debugSettings[2]) {
			//			mat.SetVector("_CameraForwardDirection", farCamera.transform.forward);
			//		} else {
			//			mat.SetVector("_CameraForwardDirection", scaledSpaceCamera.transform.forward);
			//		}
			
			//		if (debugSettings[3]) {
//			mat.SetVector ("_Globals_Origin", /*Vector3.zero-*/parentCelestialBody.transform.position);
			//		} else
			//			
			//		{
			//			
			//			//				celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
			//			celestialTransform = ParentPlanetTransform;
			//			idek = celestialTransform.position;
			//			mat.SetVector("_Globals_Origin", /*Vector3.zero-*/ idek);
			//		}
			
			//mat.SetVector("betaR", m_betaR / (Rg / m_radius));
			//			mat.SetVector("betaR", m_betaR / (postProcessDepth));
			//		mat.SetVector("betaR", new Vector4(2.9e-3f, 0.675e-2f, 1.655e-2f, 0.0f));
			//		mat.SetFloat("mieG", 0.4f);
			mat.SetVector ("SUN_DIR", m_manager.GetSunNodeDirection ());
			mat.SetFloat ("SUN_INTENSITY", sunIntensity);
			
			
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
			
			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4 ());
			mat.SetMatrix ("_ViewProjInv", viewProjMat.inverse);
			
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			//			mat.SetMatrix("_ViewToWorld", viewMat.ToMatrix4x4());
			//			
			//			var lpoints = RecalculateFrustrumPoints(farCamera);
			//			mat.SetVector("_FrustrumPoints", new Vector4(
			//				lpoints[4].x,lpoints[5].x,lpoints[5].y,lpoints[6].y));
			//			
			//			mat.SetFloat("_CameraFar", farCamera.farClipPlane);

			if (currentPQSMod_CelestialBodyTransform)
			{
				float fadeStart = currentPQSMod_CelestialBodyTransform.planetFade.fadeStart;
				float fadeEnd = currentPQSMod_CelestialBodyTransform.planetFade.fadeEnd;
				mat.SetFloat ("_fade", Mathf.Lerp (1f, 0f, (trueAlt - fadeStart) / (fadeEnd - fadeStart)));
			}

		}
		
		public void InitUniforms (Material mat)
		{
			//Init uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			
			mat.SetVector ("betaR", m_betaR / 1000.0f);
			mat.SetTexture ("_Sky_Transmittance", m_transmit);
			mat.SetTexture ("_Sky_Inscatter", m_inscatter);
			mat.SetTexture ("_Sky_Irradiance", m_irradiance);
			mat.SetFloat ("scale", Rg * atmosphereGlobalScale / m_radius);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("RL", RL * atmosphereGlobalScale);
			
			mat.SetFloat ("TRANSMITTANCE_W", TRANSMITTANCE_W);
			mat.SetFloat ("TRANSMITTANCE_H", TRANSMITTANCE_H);
			mat.SetFloat ("SKY_W", SKY_W);
			mat.SetFloat ("SKY_H", SKY_H);
			mat.SetFloat ("RES_R", RES_R);
			mat.SetFloat ("RES_MU", RES_MU);
			mat.SetFloat ("RES_MU_S", RES_MU_S);
			mat.SetFloat ("RES_NU", RES_NU);
			mat.SetFloat ("AVERAGE_GROUND_REFLECTANCE", AVERAGE_GROUND_REFLECTANCE);
			mat.SetFloat ("HR", HR * 1000.0f);
			mat.SetFloat ("HM", HM * 1000.0f);
			mat.SetVector ("betaMSca", BETA_MSca / 1000.0f);
			mat.SetVector ("betaMEx", (BETA_MSca / 1000.0f) / 0.9f);
			mat.SetFloat ("_sunglareScale", sunglareScale);
			//			mat.SetFloat("_Alpha_Cutoff", alphaCutoff);
			
		}
		
		public void setManager (Manager manager)
		{
			m_manager = manager;
		}
		
		public void enablePostprocess ()
		{
			//			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			//			if ((farCamera.gameObject.GetComponent<scatterPostprocess> ()) == null)
			//			{
			//				farCamera.gameObject.AddComponent(typeof(scatterPostprocess));
			//			}
			//			atmosphereMeshrenderer.enabled = true;
			postprocessingEnabled = true;
		}
		
		public void disablePostprocess ()
		{
			//			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			//			if (tmp != null)
			//			if ((farCamera.gameObject.GetComponent<scatterPostprocess> ()) != null)
			//			{
			//				Component.Destroy (farCamera.gameObject.GetComponent<scatterPostprocess> ());	
			//			}
			
			postprocessingEnabled = false;
		}
		
		public void SetPostProcessExposure (float postExposure)
		{
			postProcessExposure = postExposure;
		}
		
		//		public void SetPostProcessDepth(float postDepth)
		//		{
		//			postProcessDepth=postDepth;
		//		}
		//		
		//		public void SetPostProcessAlpha(float postAlpha)
		//		{
		//			postProcessingAlpha=postAlpha;
		//		}
		//		
		//		public void SetPostProcessScale(float postScale)
		//		{
		//			postProcessingScale=postScale;
		//		}
		//		
		//		public void SetAtmosphereGlobalScale(float gScale)
		//		{
		//			atmosphereGlobalScale=gScale;
		//		}
		
		public void SetParentCelestialBody (CelestialBody inPlanet)
		{
			parentCelestialBody = inPlanet;
			var _celBodyName = parentCelestialBody.name;
			var _celTransformName = parentCelestialBody.name;
			var _basePath = m_manager.GetCore ().path + "/config";
			if (parentCelestialBody.GetTransform () != null) {
				_celTransformName = parentCelestialBody.GetTransform ().name;
			}
			string[] _possiblePaths = {
				_basePath + "/" + _celBodyName,
				_basePath + "/" + _celTransformName
			};
			
			foreach (string _dir in _possiblePaths) {
				if (Directory.Exists (_dir)) {
					assetDir = _dir;
				}
			}
		}
		
		public void setParentPlanetTransform (Transform parentTransform)
		{
			ParentPlanetTransform = parentTransform;
		}
		
		//		public void SetExposure(float expo)
		//		{
		//			m_HDRExposure=expo;
		//		}
		//		
		//		public void SetExtinctionCoeff(float exCoeff)
		//		{
		//			extinctionCoeff = exCoeff;
		//		}
		//		
		//		public void SetAlphaGlobal(float glob)
		//		{
		//			alphaGlobal = glob;
		//		}
		
		public void setLayernCam (int inLayer, int inCam)
		{
			layer = inLayer;
			cam = inCam;
		}
		
		public void initiateOrRestart ()
		{
			string _file;


			m_inscatter.Create ();
			m_transmit.Create ();
			m_irradiance.Create ();
			
			_file = assetDir + m_filePath + "/transmittance.raw";
			EncodeFloat.WriteIntoRenderTexture (m_transmit, 3, _file, null);
			
			_file = assetDir + m_filePath + "/irradiance.raw";
			EncodeFloat.WriteIntoRenderTexture (m_irradiance, 3, _file, null);
			
			_file = assetDir + m_filePath + "/inscatter.raw";
			EncodeFloat.WriteIntoRenderTexture (m_inscatter, 4, _file, null);


			
			Resources.UnloadUnusedAssets ();
			System.GC.Collect ();
		}
		
		public void OnDestroy ()
		{
			//base.OnDestroy();
			saveToConfigNode ();
			if (m_transmit) {
				m_transmit.Release ();
				UnityEngine.Object.Destroy (m_transmit);
			}
			if (m_irradiance) {
				m_irradiance.Release ();
				UnityEngine.Object.Destroy (m_irradiance);
			}
			if (m_inscatter) {
				m_inscatter.Release ();
				UnityEngine.Object.Destroy (m_inscatter);
			}

			UnityEngine.Object.Destroy (black);
			UnityEngine.Object.Destroy (sunGlare);
			
			//			scatterPostprocess tmp = farCamera.gameObject.GetComponent<scatterPostprocess> ();
			//			
			//			if(tmp != null)
			//			{
			//				Component.Destroy (tmp);
			//			}
			
			/*
		if (scaledSpaceCamera.gameObject.GetComponent < updateAtCameraRythm > ()) {
			Component.Destroy(scaledSpaceCamera.gameObject.GetComponent < updateAtCameraRythm > ());
		}
		*/
			
			Component.Destroy (updater);
			UnityEngine.Object.Destroy (updater);
			
			//			if (scaledSpaceCamera.gameObject.GetComponent<cameraHDR> () != null)
			//			{
			//				Component.Destroy(scaledSpaceCamera.gameObject.GetComponent<cameraHDR> ());
			//			}
			
			
			Component.Destroy (skyMR);
			UnityEngine.Object.Destroy (skyObject);
			
			Component.Destroy (skyExtinctMR);
			UnityEngine.Object.Destroy (skyExtinctObject);
			
			Component.Destroy (atmosphereMeshrenderer);
			UnityEngine.Object.Destroy (atmosphereMesh);
			//Destroy (hp);
			
			RestoreStockAtmosphere ();
			UnityEngine.Object.Destroy (alteredMaterial);
			UnityEngine.Object.Destroy (originalMaterial);
			
			//Destroy (originalMaterial);
			
			Resources.UnloadUnusedAssets ();
			System.GC.Collect ();
		}
		
		public void destroyskyObject ()
		{
			UnityEngine.Object.Destroy (skyObject);
		}
		
		public void toggleSunglare ()
		{
			if (sunglareEnabled) {
				m_skyMaterialScaled.SetTexture ("_Sun_Glare", black);
				sunglareEnabled = false;
				ExtinctionCutoff = 0.99f;
			} else {
				m_skyMaterialScaled.SetTexture ("_Sun_Glare", sunGlare);
				sunglareEnabled = true;
				ExtinctionCutoff = 2.0f;
			}
		}
		
		public void toggleCoronas ()
		{
			Transform scaledSunTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single (t => t.name == "Sun");
			foreach (Transform child in scaledSunTransform) {
				MeshRenderer temp;
				temp = child.gameObject.GetComponent < MeshRenderer > ();
				if (temp != null) {
					temp.enabled = coronasDisabled;
				}
			}
			coronasDisabled = !coronasDisabled;
		}
		
		public void toggleStockSunglare ()
		{
			if (stocksunglareEnabled) {
				Sun.Instance.sunFlare.enabled = false;
			} else {
				Sun.Instance.sunFlare.enabled = true;
			}
			stocksunglareEnabled = !stocksunglareEnabled;
		}
		
		public void toggleAniso ()
		{
			if (!forceOFFaniso) {
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
			} else {
				QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
			}
			
			forceOFFaniso = !forceOFFaniso;
		}
		
		public Transform GetScaledTransform (string body)
		{
			List < Transform > transforms = ScaledSpace.Instance.scaledSpaceTransforms;
			return transforms.Single (n => n.name == body);
		}
		
		//		public void loadSettings()
		//		{
		//			cfg.load ();
		//			Rg =float.Parse(cfg.GetValue<string>("Rg"));
		//			Rt =float.Parse(cfg.GetValue<string>("Rt"));
		//			RL =float.Parse(cfg.GetValue<string>("RL"));
		//			
		//			m_betaR = cfg.GetValue<Vector3>("BETA_R");
		//			BETA_MSca = cfg.GetValue<Vector3>("BETA_MSca");
		//			m_mieG =float.Parse(cfg.GetValue<string>("MIE_G"));
		//			
		//			HR =float.Parse( cfg.GetValue<string>("HR"));
		//			HM =float.Parse( cfg.GetValue<string>("HM"));
		//			AVERAGE_GROUND_REFLECTANCE =float.Parse(cfg.GetValue<string>("AVERAGE_GROUND_REFLECTANCE"));
		//			atmosphereGlobalScale=float.Parse(cfg.GetValue<string>("atmosphereGlobalScale"));	
		//		}
		
		
		public void loadFromConfigNode (bool loadbackup)
		{
			ConfigNode cnToLoad;

			if (loadbackup) 
			{
				cnToLoad = ConfigNode.Load (assetDir + "/SettingsBackup.cfg");
			}

			else
			{
				cnToLoad = ConfigNode.Load (assetDir + "/Settings.cfg");
			}

			ConfigNode.LoadObjectFromConfig (this, cnToLoad);
		}
		
		public void saveToConfigNode ()
		{
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
			cnTemp.Save (assetDir + "/Settings.cfg");
		}
		
		public void backupAtmosphereMaterial ()
		{
			//Transform t = ParentPlanetTransform;
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (true);
					originalMaterial = (Material)Material.Instantiate (t.renderer.sharedMaterial);
					alteredMaterial = (Material)Material.Instantiate (t.renderer.sharedMaterial);
					t.renderer.sharedMaterial = alteredMaterial;
					i = t.childCount + 10;
				}
			}
		}
		
		public void RestoreStockAtmosphere ()
		{
			//Transform t = ParentPlanetTransform;
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (true);
					if (originalMaterial)
						t.renderer.sharedMaterial = (Material)Material.Instantiate (originalMaterial);
					i = t.childCount + 10;
				}
			}
		}
		
		
		//		public void tweakStockAtmosphere(string name, float inRimBlend, float inRimPower) {
		public void tweakStockAtmosphere ()
		{
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			//			Transform t = ParentPlanetTransform;
			
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (false);
					Material sharedMaterial = t.renderer.sharedMaterial;
					
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimBlend"), rimBlend / 100f);
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimPower"), rimpower / 100f);
					sharedMaterial.SetColor ("_SpecColor", new Color (specR / 100f, specG / 100f, specB / 100f));
					sharedMaterial.SetFloat ("_Shininess", shininess / 100);
					
					//			alteredMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), rimBlend / 100f);
					//			alteredMaterial.SetFloat(Shader.PropertyToID("_rimPower"), rimpower / 100f);
					//			alteredMaterial.SetColor("_SpecColor", new Color(specR / 100f, specG / 100f, specB / 100f));
					//			alteredMaterial.SetFloat("_Shininess", shininess / 100);
					
					i = t.childCount + 10;
				}
			}
		}
		
		//snippet by Thomas P. from KSPforum
		public void DeactivateAtmosphere ()
		{
			//Transform t = ParentPlanetTransform;
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					// Deactivate the Athmosphere-renderer
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (false);
					
					// Reset the shader parameters
					//				Material sharedMaterial = t.renderer.sharedMaterial;
					
					//sharedMaterial.SetTexture(Shader.PropertyToID("_rimColorRamp"), null);
					//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimBlend"), 0);
					//					sharedMaterial.SetFloat(Shader.PropertyToID("_rimPower"), 0);
					
					// Stop our script
					i = t.childCount + 10;
				}
			}
		}
		
		public void interpolateVariables ()
		{
			if (trueAlt <= configPoints [0].altitude) {
				alphaGlobal = configPoints [0].skyAlpha;
				m_HDRExposure = configPoints [0].skyExposure;
				m_rimHDRExposure = configPoints [0].skyRimExposure;
				postProcessingAlpha = configPoints [0].postProcessAlpha;
				postProcessDepth = configPoints [0].postProcessDepth;
				postProcessExposure = configPoints [0].postProcessExposure;
				extinctionMultiplier = configPoints [0].skyExtinctionMultiplier;
				extinctionTint = configPoints [0].skyExtinctionTint;
				skyExtinctionRimFade = configPoints [0].skyextinctionRimFade;
				edgeThreshold = configPoints [0].edgeThreshold;
				openglThreshold = configPoints [0].openglThreshold;
				viewdirOffset = configPoints [0].viewdirOffset;
				currentConfigPoint = 0;
				
			} else if (trueAlt > configPoints [configPoints.Count - 1].altitude) {
				alphaGlobal = configPoints [configPoints.Count - 1].skyAlpha;
				m_HDRExposure = configPoints [configPoints.Count - 1].skyExposure;
				m_rimHDRExposure = configPoints [configPoints.Count - 1].skyRimExposure;
				postProcessingAlpha = configPoints [configPoints.Count - 1].postProcessAlpha;
				postProcessDepth = configPoints [configPoints.Count - 1].postProcessDepth;
				postProcessExposure = configPoints [configPoints.Count - 1].postProcessExposure;
				extinctionMultiplier = configPoints [configPoints.Count - 1].skyExtinctionMultiplier;
				extinctionTint = configPoints [configPoints.Count - 1].skyExtinctionTint;
				skyExtinctionRimFade = configPoints [configPoints.Count - 1].skyextinctionRimFade;
				edgeThreshold = configPoints [configPoints.Count - 1].edgeThreshold;
				openglThreshold = configPoints [configPoints.Count - 1].openglThreshold;
				viewdirOffset = configPoints [configPoints.Count - 1].viewdirOffset;
				currentConfigPoint = configPoints.Count;
			} else {
				for (int j = 1; j < configPoints.Count; j++) {
					if ((trueAlt > configPoints [j - 1].altitude) && (trueAlt <= configPoints [j].altitude)) {
						percentage = (trueAlt - configPoints [j - 1].altitude) / (configPoints [j].altitude - configPoints [j - 1].altitude);
						
						alphaGlobal = percentage * configPoints [j].skyAlpha + (1 - percentage) * configPoints [j - 1].skyAlpha;
						m_HDRExposure = percentage * configPoints [j].skyExposure + (1 - percentage) * configPoints [j - 1].skyExposure;
						m_rimHDRExposure = percentage * configPoints [j].skyRimExposure + (1 - percentage) * configPoints [j - 1].skyRimExposure;
						postProcessingAlpha = percentage * configPoints [j].postProcessAlpha + (1 - percentage) * configPoints [j - 1].postProcessAlpha;
						postProcessDepth = percentage * configPoints [j].postProcessDepth + (1 - percentage) * configPoints [j - 1].postProcessDepth;
						postProcessExposure = percentage * configPoints [j].postProcessExposure + (1 - percentage) * configPoints [j - 1].postProcessExposure;
						extinctionMultiplier = percentage * configPoints [j].skyExtinctionMultiplier + (1 - percentage) * configPoints [j - 1].skyExtinctionMultiplier;
						extinctionTint = percentage * configPoints [j].skyExtinctionTint + (1 - percentage) * configPoints [j - 1].skyExtinctionTint;
						skyExtinctionRimFade = percentage * configPoints [j].skyextinctionRimFade + (1 - percentage) * configPoints [j - 1].skyextinctionRimFade;
						edgeThreshold = percentage * configPoints [j].edgeThreshold + (1 - percentage) * configPoints [j - 1].edgeThreshold;
						openglThreshold = percentage * configPoints [j].openglThreshold + (1 - percentage) * configPoints [j - 1].openglThreshold;
						viewdirOffset = percentage * configPoints [j].viewdirOffset + (1 - percentage) * configPoints [j - 1].viewdirOffset;
						currentConfigPoint = j;
					}
				}
			}
		}
	}
}