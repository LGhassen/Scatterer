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
	 *This whole class is overdue one big-ass cleanup
	 */
	public class SkyNode: MonoBehaviour
	{
		
		[Persistent]
		public bool forceOFFaniso;
		SimplePostProcessCube hp;
		GameObject atmosphereMesh;
		MeshRenderer atmosphereMeshrenderer;
		
		[Persistent]
		public bool displayInterpolatedVariables = false;

		updateAtCameraRythm updater;
		bool updaterAdded = false;
		
		public float postDist = -3500f;
		public float percentage;
		public int currentConfigPoint;
		bool coronasDisabled = false;

		[Persistent] public float experimentalAtmoScale=1f;
		float viewdirOffset=0f;

		Matrix4x4 p;

		public float sunIntensity = 100f;

		public float oceanSigma = 0.04156494f;
		public float _Ocean_Threshold = 25f;
		[Persistent]
		public float sunglareScale = 1f;

		public float extinctionMultiplier = 1f;

		public float extinctionTint = 100f;
		public float skyExtinctionRimFade = 0f;
		[Persistent]
		public float mapExtinctionMultiplier = 1f;
		[Persistent]
		public float mapExtinctionTint = 1f;
		[Persistent]
		public float mapSkyExtinctionRimFade = 1f;

		public float openglThreshold = 250f;
		public float edgeThreshold = 0.5f;


		Vector3 sunViewPortPos=Vector3.zero;

		bool eclipse=false;

		Transform celestialTransform;
		float alt;
		public float trueAlt;
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType < SkyNode > (null);

		
		[Persistent]
		public float MapViewScale = 1f;
		[Persistent]
		float postProcessMaxAltitude=160000;

		GameObject skyObject;
		MeshRenderer skyMR;
		MeshFilter skyMF;
		CelestialBody parentCelestialBody;
		Transform ParentPlanetTransform;

		
		bool sunglareEnabled = true;
		bool stocksunglareEnabled = true;
		float sunglareCutoffAlt;
		Texture2D sunGlare;
		Texture2D black;


		Texture2D sunSpikes;
		Texture2D sunFlare;
		Texture2D sunGhost1;
		Texture2D sunGhost2;

		
		//atmosphere properties
		
		/*[Persistent]*/
		public float extinctionCoeff = 0.7f;
		
		/*[Persistent]*/
		public float postProcessingAlpha = 0.78f;
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


		PQSMod_CelestialBodyTransform currentPQSMod_CelestialBodyTransform=null;


		public bool inScaledSpace {
			get {
				return !(CurrentPQS != null && CurrentPQS.isActive);
			}
		}
		
		Vector3 position;
		bool initiated = false;
		Camera[] cams;


		public Camera farCamera , nearCamera, scaledSpaceCamera;

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

		public Material sunglareMaterial;

		
		Material originalMaterial;
		Material alteredMaterial;
		[Persistent]
		Vector3 m_betaR = new Vector3 (5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number meands more light is scattered in the forward direction
		[Persistent]
		public float
			m_mieG = 0.85f;
		string m_filePath = "/Proland/Textures/Atmo";
		public Matrix4x4d m_cameraToScreenMatrix;
		Mesh m_mesh;
		RenderTexture m_transmit, m_inscatter, m_irradiance;
		
		Manager m_manager;
		
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

			sunglareMaterial = new Material (ShaderTool.GetMatFromShader2 ("CompiledSunGlare.shader"));
			
			sunGlare = new Texture2D (512, 512);
			black = new Texture2D (512, 512);
			
			sunGlare.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares" , "sunglare.png")));
			black.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares", "black.png")));

			sunGlare.wrapMode = TextureWrapMode.Clamp;
			m_skyMaterialScaled.SetTexture ("_Sun_Glare", sunGlare);


			sunSpikes = new Texture2D (2048, 2048);
			sunFlare = new Texture2D (1024, 1024);
			sunGhost1 = new Texture2D (128, 128);
			sunGhost2 = new Texture2D (128, 128);


			sunSpikes.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares", "star_glow.png")));
			sunSpikes.wrapMode = TextureWrapMode.Clamp;
			sunFlare.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares", "AstroNiki_flare.png")));
			sunFlare.wrapMode = TextureWrapMode.Clamp;
			sunGhost1.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares", "EdenGhost1.png")));
			sunGhost1.wrapMode = TextureWrapMode.Clamp;
			sunGhost2.LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", m_manager.GetCore().path+"/flares", "EdenGhost2.png")));
			sunGhost2.wrapMode = TextureWrapMode.Clamp;


			sunglareMaterial.SetTexture ("_Sun_Glare", sunGlare);
			sunglareMaterial.SetTexture ("sunSpikes", sunSpikes);
			sunglareMaterial.SetTexture ("sunFlare", sunFlare);
			sunglareMaterial.SetTexture ("sunGhost1", sunGhost1);
			sunglareMaterial.SetTexture ("sunGhost2", sunGhost2);
			sunglareMaterial.SetTexture ("_customDepthTexture", m_manager.GetCore ().customDepthBufferTexture);


			
			InitUniforms (m_skyMaterialScaled);
			
			if (m_manager.GetCore ().render24bitDepthBuffer && !m_manager.GetCore ().d3d9)
			{
				m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter24bitdepth.shader");
			} 

			else

			{
				m_atmosphereMaterial = ShaderTool.GetMatFromShader2 ("CompiledAtmosphericScatter.shader");
			}

			CurrentPQS = parentCelestialBody.pqsController;

			if (CurrentPQS)
			{
				currentPQSMod_CelestialBodyTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> () [0];
			}

			skyObject = new GameObject ();
			skyMF = skyObject.AddComponent < MeshFilter > ();
			skyMF.mesh.Clear ();
			skyMF.mesh = m_mesh;

			skyObject.layer = 10;
			celestialTransform = ParentPlanetTransform;
			skyObject.transform.parent = celestialTransform;
			
			skyMR = skyObject.AddComponent < MeshRenderer > ();
			skyMR.sharedMaterial = m_skyMaterialScaled;
			skyMR.material = m_skyMaterialScaled;


			skyMR.castShadows = false;
			skyMR.receiveShadows = false;

			skyMR.enabled = true;
			
			hp = new SimplePostProcessCube (10000, m_atmosphereMaterial);
			atmosphereMesh = hp.GameObject;
			atmosphereMesh.layer = 15;
			atmosphereMeshrenderer = hp.GameObject.GetComponent < MeshRenderer > ();
			atmosphereMeshrenderer.material = m_atmosphereMaterial;

		}
		
		public void UpdateStuff () //to be called by update at camera rythm for some graphical stuff
		{
			RaycastHit hit;
			bool hitStatus=false;

			if (!MapView.MapIsEnabled)
			{
//				hitStatus = Physics.Raycast (farCamera.transform.position, (m_manager.sunCelestialBody.position - farCamera.transform.position).normalized,
//				                             out hit, Mathf.Infinity, (int)((1 << 10) + (1 << 15) + (1 << 0))); //doing both like this causes the scaledSpace detection to be off
																												//raycasting in scaledspace has to be done in scaled coordinates
																												//so a separate raycast is required

				hitStatus = Physics.Raycast (farCamera.transform.position, (m_manager.sunCelestialBody.transform.position - farCamera.transform.position).normalized,
				                             out hit, Mathf.Infinity, (int)((1 << 15) + (1 << 0)));

				if(!hitStatus)
					hitStatus = Physics.Raycast (scaledSpaceCamera.transform.position, (ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position)
					                                                                    - scaledSpaceCamera.transform.position).normalized,out hit, Mathf.Infinity, (int)((1 << 10)));


			}

			else
			{
				hitStatus = Physics.Raycast (scaledSpaceCamera.transform.position, (ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position)
				                                                                    - scaledSpaceCamera.transform.position).normalized,out hit, Mathf.Infinity, (int)((1 << 10)));
			}


			if (hitStatus)
			{
				eclipse=true;
				Debug.Log (hit.collider.gameObject.name);
				Debug.Log (hit.collider.gameObject.layer);
			}

			else
			{
				eclipse=false;
			}

			atmosphereMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
			atmosphereMesh.transform.localRotation = farCamera.transform.localRotation;
			atmosphereMesh.transform.rotation = farCamera.transform.rotation;
			//adding post processing to camera
			//??????????????
			if ((!inScaledSpace) && (!MapView.MapIsEnabled))
			{
				if (postprocessingEnabled) {
					InitPostprocessMaterial (m_atmosphereMaterial);
					UpdatePostProcessMaterial (m_atmosphereMaterial);

					if (m_manager.hasOcean && m_manager.GetCore ().useOceanShaders)
					{
						m_manager.GetOceanNode().SetUniforms(m_atmosphereMaterial);
					}

				}
			}


			sunViewPortPos = scaledSpaceCamera.WorldToViewportPoint (ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position));
			sunglareMaterial.SetVector ("sunViewPortPos", sunViewPortPos);
			sunglareMaterial.SetFloat ("aspectRatio", nearCamera.aspect);

			sunglareMaterial.renderQueue = 3000;

			sunglareMaterial.SetFloat ("Rg", Rg);
			sunglareMaterial.SetFloat ("Rt", Rt);
			sunglareMaterial.SetTexture ("_Sky_Transmittance", m_transmit);

			if (!MapView.MapIsEnabled)
				sunglareMaterial.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position - parentCelestialBody.transform.position);
			else
				sunglareMaterial.SetVector ("_Globals_WorldCameraPos", (Vector3) ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position) - parentCelestialBody.transform.position);

			sunglareMaterial.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToSun ().normalized);
		}

		
		public void UpdateNode ()
		{
			position = parentCelestialBody.transform.position;
			
			if (!initiated)
			{
				m_radius = m_manager.GetRadius ();
				
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;
				sunglareCutoffAlt = experimentalAtmoScale*(Rt - Rg);
				cams = Camera.allCameras;

				scaledSpaceCamera=m_manager.GetCore().scaledSpaceCamera;
				farCamera=m_manager.GetCore().farCamera;
				nearCamera=m_manager.GetCore().nearCamera;
				
				initiated = true;
				backupAtmosphereMaterial ();
				tweakStockAtmosphere ();
				
			}
			else
			{
				
				alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
				trueAlt = alt - m_radius;
				
//				if ((sunglareEnabled) ^ ((trueAlt < sunglareCutoffAlt) && !MapView.MapIsEnabled && !eclipse)) { //^ is XOR
//					toggleSunglare ();
//				}

				if ((sunglareEnabled)) { //^ is XOR
					toggleSunglare ();
				}
				

//				if ((coronasDisabled) ^ ((trueAlt < sunglareCutoffAlt) && !MapView.MapIsEnabled && !eclipse)) { //^ is XOR
//				if ((coronasDisabled) ^ ((trueAlt < sunglareCutoffAlt) && !MapView.MapIsEnabled)) { //don't really like how the coronas look on eclipses
//					toggleCoronas ();
//				}

				if ((coronasDisabled)) { 
					toggleCoronas ();
				}
				
//				if ((!stocksunglareEnabled) ^ ((trueAlt < sunglareCutoffAlt - 1000) && !MapView.MapIsEnabled)) { //^ is XOR
//					toggleStockSunglare ();
//				}

				if (stocksunglareEnabled) { //^ is XOR
					toggleStockSunglare ();
				}
				
				interpolateVariables ();


				//if alt-tabbing/windowing and rendertextures are lost
				//this loads them back up
				//you have to wait for a frame of two because if you do it immediately they don't get loaded
				if (!m_inscatter.IsCreated ())
				{
					waitBeforeReloadCnt++;
					if (waitBeforeReloadCnt >= 2)
					{
						m_inscatter.Release ();
						m_transmit.Release ();
						m_irradiance.Release ();

						initiateOrRestart ();
						Debug.Log ("[Scatterer] Reloaded scattering tables for"+parentCelestialBody.name);
						m_manager.reBuildOcean ();
						waitBeforeReloadCnt = 0;
					}
				}


					
					if (scaledSpaceCamera && !updaterAdded)
					{

						updater = (updateAtCameraRythm)scaledSpaceCamera.gameObject.AddComponent (typeof(updateAtCameraRythm));
						updaterAdded = true;
						
						updater.settings (m_mesh, m_skyMaterialScaled, m_manager, this, skyObject,
						                  parentCelestialBody.transform, celestialTransform);
					}
				
				atmosphereMeshrenderer.enabled = (!inScaledSpace) &&(postprocessingEnabled)&&(trueAlt<postProcessMaxAltitude);

				if (!MapView.MapIsEnabled && !eclipse && (sunViewPortPos.z > 0))
				{
					Graphics.DrawMesh (m_mesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 15,
					                   nearCamera, 0, null, false, false);

				}

				else if (!eclipse && (sunViewPortPos.z > 0))
				{
					Graphics.DrawMesh (m_mesh, Vector3.zero, Quaternion.identity, sunglareMaterial, 10,
					                   scaledSpaceCamera, 0, null, false, false);
						
				}
			}

//			Transform scaledSunTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single (t => t.name == "Sun");
//			foreach (Transform child in scaledSunTransform) {
//				MeshRenderer temp;
//				temp = child.gameObject.GetComponent < MeshRenderer > ();
//				Debug.Log("temp.material.renderQueue"+child.name+" "+temp.material.renderQueue);
//				Debug.Log(child.gameObject.layer);
//				temp.material.renderQueue=2000;
//
//				temp = child.gameObject.GetComponent < MeshRenderer > ();
//				if (temp != null) {
//					temp.enabled = coronasDisabled;
//				}
//			}

			Debug.Log ("length "+(ScaledSpace.LocalToScaledSpace (m_manager.sunCelestialBody.transform.position
				- scaledSpaceCamera.transform.position)).magnitude.ToString());

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

//			Shader.SetGlobalVector ("_Godray_WorldSunDir", m_manager.getDirectionToSun ().normalized);
//
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirX", m_manager.sunCelestialBody.transform.position.x
//			                       -parentCelestialBody.transform.position.x);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirY", m_manager.sunCelestialBody.transform.position.y
//			                       -parentCelestialBody.transform.position.y);
//			Shader.SetGlobalFloat ("_Godray_WorldSunDirZ", m_manager.sunCelestialBody.transform.position.z
//			                       -parentCelestialBody.transform.position.z);


			if (!MapView.MapIsEnabled) {
				p = farCamera.projectionMatrix;
			} else {
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix ("_Globals_CameraToScreen", m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix ("_Globals_ScreenToCamera", m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());

			Vector3 temp = ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position);
			mat.SetVector ("_Globals_WorldCameraPos", temp);
			mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
				

			
			if (!MapView.MapIsEnabled) {
				mat.SetFloat ("_Exposure", m_HDRExposure);
				mat.SetFloat ("_RimExposure", m_rimHDRExposure);
			} else {
				mat.SetFloat ("_Exposure", mapExposure);
				mat.SetFloat ("_RimExposure", mapSkyRimExposure);
			}
			

			//Eclipse stuff
			Vector3 sunPosRelPlanet = m_manager.sunCelestialBody.transform.position;
			mat.SetVector("sunPosAndRadius", new Vector4(sunPosRelPlanet.x,sunPosRelPlanet.y,
			                                             sunPosRelPlanet.z,(float)m_manager.sunCelestialBody.Radius));

			//build and set casters matrix
			Matrix4x4 castersMatrix= Matrix4x4.zero;

			for (int i=0;i< Mathf.Min(4, m_manager.eclipseCasters.Count);i++)
			{
				Vector3 casterPosRelPlanet = m_manager.eclipseCasters[i].transform.position;
				castersMatrix.SetRow(i,new Vector4(casterPosRelPlanet.x, casterPosRelPlanet.y,
				                                    casterPosRelPlanet.z,(float)m_manager.eclipseCasters[i].Radius));

			}
			mat.SetMatrix ("lightOccluders", castersMatrix);
		}



		public void SetOceanUniforms (Material mat)
		{
			if (mat == null)
				return;

//			mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);
			mat.SetFloat ("scale", atmosphereGlobalScale);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("RL", RL * atmosphereGlobalScale);
			mat.SetVector ("betaR", m_betaR / 1000.0f);
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetTexture ("_Sky_Transmittance", m_transmit);
			mat.SetTexture ("_Sky_Inscatter", m_inscatter);
			mat.SetTexture ("_Sky_Irradiance", m_irradiance);
			mat.SetFloat ("_Sun_Intensity", 100f);
			mat.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToSun ().normalized);
			mat.SetVector ("_Globals_WorldCameraPos", farCamera.transform.position);
			mat.SetVector ("_Globals_Origin", parentCelestialBody.transform.position);
			mat.SetFloat ("_Exposure", m_rimHDRExposure);
		}




		public void InitPostprocessMaterial (Material mat)
		{
			mat.SetTexture ("_Transmittance", m_transmit);
			mat.SetTexture ("_Inscatter", m_inscatter);
			mat.SetTexture ("_Irradiance", m_irradiance);
			
			if (m_manager.GetCore ().render24bitDepthBuffer && !m_manager.GetCore ().d3d9)
				mat.SetTexture ("_customDepthTexture", m_manager.GetCore ().customDepthBufferTexture);

			//Consts, best leave these alone
			mat.SetFloat ("M_PI", Mathf.PI);
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("Rl", RL * atmosphereGlobalScale);
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

			mat.SetVector ("_camPos", farCamera.transform.position-parentCelestialBody.transform.position);  //better do this small calculation here
			mat.SetVector ("SUN_DIR", m_manager.GetSunNodeDirection ());
		}



		public void UpdatePostProcessMaterial (Material mat)
		{
			//mat.SetFloat ("atmosphereGlobalScale", atmosphereGlobalScale);

			mat.SetFloat ("_experimentalAtmoScale", experimentalAtmoScale);
//			mat.SetFloat ("_viewdirOffset", viewdirOffset);
		
			mat.SetFloat ("_global_alpha", postProcessingAlpha);
			mat.SetFloat ("_Exposure", postProcessExposure);
			mat.SetFloat ("_global_depth", postProcessDepth*1000000);
			mat.SetFloat ("_openglThreshold", openglThreshold);
//			mat.SetFloat ("_edgeThreshold", edgeThreshold);
			

			mat.SetFloat("_Scale", 1);

			mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			mat.SetVector ("SUN_DIR", m_manager.GetSunNodeDirection ());
			mat.SetFloat ("SUN_INTENSITY", sunIntensity);
			
			
			Matrix4x4 ctol1 = farCamera.cameraToWorldMatrix;
			Vector3d tmp = (farCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			
			Matrix4x4d viewMat = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
			                                    ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
			                                    ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
			                                    ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);

			viewMat = viewMat.Inverse ();
			Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);
			
			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4 ());
			mat.SetMatrix ("_ViewProjInv", viewProjMat.inverse);
			
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));

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
			
		}
		
		public void setManager (Manager manager)
		{
			m_manager = manager;
		}
		
		public void enablePostprocess ()
		{
			postprocessingEnabled = true;
		}
		
		public void disablePostprocess ()
		{
			postprocessingEnabled = false;
		}
		
		public void SetPostProcessExposure (float postExposure)
		{
			postProcessExposure = postExposure;
		}
		
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

		
		public void initiateOrRestart ()
		{
			string _file;

			m_inscatter.Create ();
			m_transmit.Create ();
			m_irradiance.Create ();

			EncodeFloat encode = new EncodeFloat ();

			_file = assetDir + m_filePath + "/inscatter.raw";
			encode.WriteIntoRenderTexture (m_inscatter, 4, _file);

			_file = assetDir + m_filePath + "/transmittance.raw";
			encode.WriteIntoRenderTexture (m_transmit, 3, _file);

			_file = assetDir + m_filePath + "/irradiance.raw";
			encode.WriteIntoRenderTexture (m_irradiance, 3, _file);
		}
		
		public void OnDestroy ()
		{
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

			Component.Destroy (updater);
			UnityEngine.Object.Destroy (updater);
			
			Component.Destroy (skyMR);
			UnityEngine.Object.Destroy (skyObject);
			
			Component.Destroy (atmosphereMeshrenderer);
			UnityEngine.Object.Destroy (atmosphereMesh);
			
			RestoreStockAtmosphere ();
			UnityEngine.Object.Destroy (alteredMaterial);
			UnityEngine.Object.Destroy (originalMaterial);
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
			} else {
				m_skyMaterialScaled.SetTexture ("_Sun_Glare", sunGlare);
				sunglareEnabled = true;
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
		

		public void tweakStockAtmosphere ()
		{
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (false);
					Material sharedMaterial = t.renderer.sharedMaterial;
					
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimBlend"), rimBlend / 100f);
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimPower"), rimpower / 100f);
					sharedMaterial.SetColor ("_SpecColor", new Color (specR / 100f, specG / 100f, specB / 100f));
					sharedMaterial.SetFloat ("_Shininess", shininess / 100);
					
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


		//Seriously this is ugly as hell fix this, and use lerp
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
//				edgeThreshold = configPoints [0].edgeThreshold;
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
//				edgeThreshold = configPoints [configPoints.Count - 1].edgeThreshold;
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
//						edgeThreshold = percentage * configPoints [j].edgeThreshold + (1 - percentage) * configPoints [j - 1].edgeThreshold;
						openglThreshold = percentage * configPoints [j].openglThreshold + (1 - percentage) * configPoints [j - 1].openglThreshold;
						viewdirOffset = percentage * configPoints [j].viewdirOffset + (1 - percentage) * configPoints [j - 1].viewdirOffset;
						currentConfigPoint = j;
					}
				}
			}
		}
	}
}