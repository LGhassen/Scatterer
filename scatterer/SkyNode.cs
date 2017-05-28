#define skyScaledBox

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
		public UrlDir.UrlConfig configUrl;
		
		public GameObject atmosphereMesh;
		MeshRenderer atmosphereMeshrenderer;
		MeshFilter atmosphereMeshFilter;
		
		SimplePostProcessCube skyScaledCube;
		GameObject skyScaledMesh;
		MeshRenderer skyScaledMeshrenderer;
		
		SimplePostProcessCube skyLocalCube;
		GameObject skyLocalMesh;
		MeshRenderer skyLocalMeshrenderer;
		
		float localSkyAltitude;
		
		[Persistent]
		public bool displayInterpolatedVariables = false;
		
		UpdateOnCameraPreCull updater;
		bool updaterAdded = false;
		
		Matrix4x4 castersMatrix1=Matrix4x4.zero;
		Matrix4x4 castersMatrix2=Matrix4x4.zero;
		
		public Matrix4x4 planetShineSourcesMatrix=Matrix4x4.zero;
		public Matrix4x4 planetShineRGBMatrix=Matrix4x4.zero;
		
		Vector3 sunPosRelPlanet=Vector3.zero;
		
		//public bool scaledMode = false;
		public bool scaledMode = true;
		
		public float postDist = -4500f;
		//		public float postDist = -4000f;
		public float percentage;
		public int currentConfigPoint;
		
		EncodeFloat encode;
		//		EncodeFloat2D encode;
		
		[Persistent]
		public float experimentalAtmoScale=1f;
		//float viewdirOffset=0f;
		
		Matrix4x4 p;
		
		public float sunIntensity = 100f;
		
		public float oceanSigma = 0.04156494f;
		public float _Ocean_Threshold = 25f;
		
		[Persistent]
		public float mapExtinctionMultiplier = 1f;
		[Persistent]
		public float mapExtinctionTint = 1f;
		[Persistent]
		public float mapSkyExtinctionRimFade = 1f;
		[Persistent]
		public float mapGroundExtinctionFade = 0f;
		
		[Persistent]
		public float
			_mapExtinctionScatterIntensity = 1f;
		
		
		[Persistent]
		public bool drawSkyOverClouds = true;
		
		[Persistent]
		public float drawOverCloudsAltitude = 100000f;

		
		Vector3 sunViewPortPos=Vector3.zero;

		float alt;
		public float trueAlt;
		PluginConfiguration cfg = KSP.IO.PluginConfiguration.CreateForType < SkyNode > (null);

		[Persistent]
		//public float MapViewScale = 1f;

		CelestialBody parentCelestialBody;
		Transform ParentPlanetTransform;
		
		GameObject ringObject;
		float ringInnerRadius, ringOuterRadius;
		Texture2D ringTexture;

		bool hasRingObjectAndShadowActivated = false;

		bool stocksunglareEnabled = true;
				
		//atmosphere properties
		public ConfigPoint interpolatedSettings= new ConfigPoint();
		[Persistent]
		public float mapExposure = 0.15f;
		[Persistent]
		public float mapSkyRimExposure = 0.15f;
		[Persistent]
		public float cloudColorMultiplier=3f;
		[Persistent]
		public float cloudScatteringMultiplier=0.2f;
		[Persistent]
		public float cloudSkyIrradianceMultiplier=0.05f;
		
		[Persistent]
		public float volumetricsColorMultiplier = 1f;
//		[Persistent]
//		public float volumetricsScatteringMultiplier=1f;
//		[Persistent]
//		public float volumetricsSkyIrradianceMultiplier = 1f;

		PQS CurrentPQS = null;

		PQSMod_CelestialBodyTransform currentPQSMod_CelestialBodyTransform=null;

		public bool inScaledSpace = true;

		bool initiated = false;

		public List<Material> EVEvolumetrics = new List<Material>();
		bool mapVolumetrics=false;
		int waitCounter=0;
		
		public Camera farCamera , nearCamera, scaledSpaceCamera;
		
		public bool postprocessingEnabled = true;

		

		[Persistent]
		public float mapAlphaGlobal = 1f;
		float m_radius; // = 600000.0f;
		//The radius of the planet (Rg), radius of the atmosphere (Rt)
		[Persistent]
		public float Rg; // = 600000.0f;
		[Persistent]
		public float Rt; // = (64200f/63600f) * 600000.0f;
		[Persistent]
		float RL; // = (64210.0f/63600f) * 600000.0f;
		[Persistent]
		public float atmosphereGlobalScale = 1f;

		[Persistent]
		public bool EVEIntegration_preserveCloudColors = false;

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
		public Material m_skyMaterialScaled, m_skyMaterialLocal;
		
		//		public Material sunglareMaterial;

		Material sunflareExtinctionMaterial;
		
		Material originalMaterial;
		Material alteredMaterial;
		[Persistent]
		Vector3 m_betaR = new Vector3 (5.8e-3f, 1.35e-2f, 3.31e-2f);
		//Asymmetry factor for the mie phase function
		//A higher number means more light is scattered in the forward direction
		[Persistent]
		public float
			m_mieG = 0.85f;
		//string m_filePath = "/Proland/Textures/Atmo";
		public Matrix4x4d m_cameraToScreenMatrix;
		
		Texture2D m_inscatter, m_irradiance;
		public Texture2D m_transmit;
		
		public Manager m_manager;
		
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
		public List < ConfigPoint > configPoints = new List < ConfigPoint > {
			new ConfigPoint(5000f, 1f, 0.25f,0.25f, 1f, 0.4f, 0.23f, 1f, 100f,0f, 0f, 250f, 0.5f,0f,100f,100f,1f,1f)
			, new ConfigPoint(15000f, 1f, 0.15f,0.15f, 1f, 8f, 0.23f, 1f, 100f,0f,0f, 250f, 0.5f,0f,100f,100f,1f,1f)
		};
		//public string assetDir;

		[Persistent]
		public string assetPath;
		
		public void Start ()
		{
			m_radius = (float) m_manager.GetRadius ();
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
			
			//Inscatter is responsible for the change in the sky color as the sun moves
			//The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture
			//and the shader manually performs the sample for the 4th dimension
			m_inscatter = new Texture2D (RES_MU_S * RES_NU, RES_MU * RES_R, TextureFormat.RGBAHalf,false);
			
			//			m_inscatter = new RenderTexture (RES_MU_S * RES_NU, RES_MU * RES_R, 0, RenderTextureFormat.ARGBFloat);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			//Transmittance is responsible for the change in the sun color as it moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			//			m_transmit = new Texture2D (TRANSMITTANCE_W, TRANSMITTANCE_H, TextureFormat.ARGB32,false);
			m_transmit = new Texture2D (TRANSMITTANCE_W, TRANSMITTANCE_H, TextureFormat.RGBAHalf,false);
			
			//			m_transmit = new RenderTexture (TRANSMITTANCE_W, TRANSMITTANCE_H, 0, RenderTextureFormat.ARGBFloat);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;
			
			//Irradiance is responsible for the change in the sky color as the sun moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			//			m_irradiance = new Texture2D (SKY_W, SKY_H, TextureFormat.ARGB32,false);
			m_irradiance = new Texture2D (SKY_W, SKY_H, TextureFormat.RGBAHalf,false);
			
			//			m_irradiance = new RenderTexture (SKY_W, SKY_H, 0, RenderTextureFormat.ARGBFloat);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;
						
			loadPrecomputedTables ();

			m_skyMaterialScaled = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/SkyScaled")]);
			m_skyMaterialLocal = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/SkyLocal")]);
			m_atmosphereMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericScatter")]);

			m_skyMaterialLocal.SetOverrideTag ("IgnoreProjector", "True");
			m_skyMaterialScaled.SetOverrideTag ("IgnoreProjector", "True");
			m_atmosphereMaterial.SetOverrideTag ("IgnoreProjector", "True");

//			if (Core.Instance.useEclipses)
//			{
//				m_skyMaterialScaled.EnableKeyword ("ECLIPSES_ON");
//				m_skyMaterialScaled.DisableKeyword ("ECLIPSES_OFF");
//				m_skyMaterialLocal.EnableKeyword ("ECLIPSES_ON");
//				m_skyMaterialLocal.DisableKeyword ("ECLIPSES_OFF");
//			}
//			else
//			{
//				m_skyMaterialScaled.DisableKeyword ("ECLIPSES_ON");
//				m_skyMaterialScaled.EnableKeyword ("ECLIPSES_OFF");
//				m_skyMaterialLocal.DisableKeyword ("ECLIPSES_ON");
//				m_skyMaterialLocal.EnableKeyword ("ECLIPSES_OFF");
//			}
//			
//			if (Core.Instance.usePlanetShine)
//			{
//				m_skyMaterialScaled.EnableKeyword ("PLANETSHINE_ON");
//				m_skyMaterialScaled.DisableKeyword ("PLANETSHINE_OFF");
//				m_skyMaterialLocal.EnableKeyword ("PLANETSHINE_ON");
//				m_skyMaterialLocal.DisableKeyword ("PLANETSHINE_OFF");
//				m_atmosphereMaterial.EnableKeyword ("PLANETSHINE_ON");
//				m_atmosphereMaterial.DisableKeyword ("PLANETSHINE_OFF");
//				
//				
//			}
//			else
//			{
//				m_skyMaterialScaled.DisableKeyword ("PLANETSHINE_ON");
//				m_skyMaterialScaled.EnableKeyword ("PLANETSHINE_OFF");
//				m_skyMaterialLocal.DisableKeyword ("PLANETSHINE_ON");
//				m_skyMaterialLocal.EnableKeyword ("PLANETSHINE_OFF");
//				m_atmosphereMaterial.DisableKeyword ("PLANETSHINE_ON");
//				m_atmosphereMaterial.EnableKeyword ("PLANETSHINE_OFF");
//			}

			if (Core.Instance.useAlternateShaderSQRT)
			{
				m_skyMaterialScaled.EnableKeyword ("DEFAULT_SQRT_OFF");
				m_skyMaterialScaled.DisableKeyword ("DEFAULT_SQRT_ON");
				m_skyMaterialLocal.EnableKeyword ("DEFAULT_SQRT_OFF");
				m_skyMaterialLocal.DisableKeyword ("DEFAULT_SQRT_ON");
			}
			else
			{
				m_skyMaterialScaled.EnableKeyword ("DEFAULT_SQRT_ON");
				m_skyMaterialScaled.DisableKeyword ("DEFAULT_SQRT_OFF");
				m_skyMaterialLocal.EnableKeyword ("DEFAULT_SQRT_ON");
				m_skyMaterialLocal.DisableKeyword ("DEFAULT_SQRT_OFF");

			}
			
			
			if (Core.Instance.useGodrays)
			{
				m_atmosphereMaterial.EnableKeyword("GODRAYS_ON");
				m_atmosphereMaterial.DisableKeyword("GODRAYS_OFF");
			}
			else
			{
				m_atmosphereMaterial.DisableKeyword("GODRAYS_ON");
				m_atmosphereMaterial.EnableKeyword("GODRAYS_OFF");
			}
			if (Core.Instance.useEclipses)
			{
				m_atmosphereMaterial.EnableKeyword ("ECLIPSES_ON");
				m_atmosphereMaterial.DisableKeyword ("ECLIPSES_OFF");
			}
			else
			{
				m_atmosphereMaterial.DisableKeyword ("ECLIPSES_ON");
				m_atmosphereMaterial.EnableKeyword ("ECLIPSES_OFF");
			}

			InitPostprocessMaterial (m_atmosphereMaterial);
						
			CurrentPQS = parentCelestialBody.pqsController;
			
			if (CurrentPQS)
			{
				currentPQSMod_CelestialBodyTransform = CurrentPQS.GetComponentsInChildren<PQSMod_CelestialBodyTransform> () [0];
			}


			atmosphereMesh = new GameObject ();

			if (atmosphereMesh.GetComponent<MeshFilter> ())
				atmosphereMeshFilter = atmosphereMesh.GetComponent<MeshFilter> ();
			else
				atmosphereMeshFilter = atmosphereMesh.AddComponent<MeshFilter>();

			atmosphereMeshFilter.mesh.Clear ();
			atmosphereMeshFilter.mesh = MeshFactory.MakePlaneWithFrustumIndexes();
			atmosphereMeshFilter.mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f,1e8f, 1e8f));
			if (atmosphereMesh.GetComponent<MeshRenderer> ())
				atmosphereMeshrenderer = atmosphereMesh.GetComponent<MeshRenderer> ();
			else
				atmosphereMeshrenderer = atmosphereMesh.AddComponent<MeshRenderer>();

			atmosphereMeshrenderer.sharedMaterial = m_atmosphereMaterial;
			atmosphereMeshrenderer.material = m_atmosphereMaterial;

			atmosphereMeshrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			atmosphereMeshrenderer.receiveShadows = false;
			atmosphereMeshrenderer.enabled = true;

			atmosphereMesh.layer = 15;

			
			#if skyScaledBox
			float skySphereSize = 2*(4 * (Rt-Rg) + Rg) / ScaledSpace.ScaleFactor;
			localSkyAltitude = 6 * (Rt-Rg) + Rg;
			skyScaledCube = new SimplePostProcessCube (skySphereSize, m_skyMaterialScaled,true);
			skyScaledMesh = skyScaledCube.GameObject;
			skyScaledMesh.layer = 10;
			skyScaledMesh.transform.position = ParentPlanetTransform.position;
			skyScaledMesh.transform.parent = ParentPlanetTransform;
			skyScaledMeshrenderer = skyScaledCube.GameObject.GetComponent < MeshRenderer > ();
			skyScaledMeshrenderer.material = m_skyMaterialScaled;
			
			
			//skyScaledMeshrenderer.enabled = false;
			
			if (Core.Instance.drawAtmoOnTopOfClouds && drawSkyOverClouds)
				m_skyMaterialScaled.renderQueue=3002;
			else
				m_skyMaterialScaled.renderQueue=3001;

			
			skyLocalCube = new SimplePostProcessCube (40000, m_skyMaterialLocal,false);
			skyLocalMesh = skyLocalCube.GameObject;
			skyLocalMesh.layer = 15;
			skyLocalMeshrenderer = skyLocalCube.GameObject.GetComponent < MeshRenderer > ();
			skyLocalMeshrenderer.material = m_skyMaterialLocal;
			
			
			m_skyMaterialLocal.renderQueue=1000; //render to background unless over clouds
			//skyLocalMeshrenderer.enabled = true;

			//start in scaledMode
			skyScaledMeshrenderer.enabled = true;
			skyLocalMeshrenderer.enabled=false;
			
			#endif



			if (Core.Instance.useRingShadows)
			{
				ringObject = GameObject.Find (parentCelestialBody.name + "Ring");
				if (ringObject)
				{
					
					Debug.Log ("[Scatterer] Found ring for " + parentCelestialBody.name);

					Material ringMat = ringObject.GetComponent < MeshRenderer > ().material;

					hasRingObjectAndShadowActivated = true;
					
					MonoBehaviour[] scripts = (MonoBehaviour[]) ringObject.GetComponents<MonoBehaviour>();
					
					foreach( MonoBehaviour _script in scripts)
					{						
						//Debug.Log("script.GetType().ToString() "+_script.GetType().ToString());

						if (_script.GetType().ToString().Contains("Ring")) //production-quality code
						{
							const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
								BindingFlags.Instance | BindingFlags.Static;

							FieldInfo[] fields = _script.GetType().GetFields(flags);

							foreach(FieldInfo fi in fields)
							{
								//Debug.Log("fi.Name "+fi.Name+" fi.GetType() "+fi.GetType());
							}
							
							try
							{
								ringTexture = _script.GetType().GetField("texture", flags).GetValue(_script) as Texture2D;
								Debug.Log("[Scatterer] ring texture fetch successful");
								Debug.Log("[Scatterer] ringTexture.width "+ringTexture.width.ToString());
								Debug.Log("[Scatterer] ringTexture.height "+ringTexture.height.ToString());

//								ringInnerRadius = (float) _script.GetType().GetField("innerRadius", flags).GetValue(_script);
//								Debug.Log ("[Scatterer] ring innerRadius (scaled) " + ringInnerRadius.ToString ());
//
//								ringOuterRadius = (float) _script.GetType().GetField("outerRadius", flags).GetValue(_script);
//								Debug.Log ("[Scatterer] ring outerRadius (scaled) " + ringOuterRadius.ToString ());

								//ringMR
								MeshRenderer ringMR = _script.GetType().GetField("ringMR", flags).GetValue(_script) as MeshRenderer;
								Debug.Log("[Scatterer] ringMR fetch successful");

								ringInnerRadius = ringMR.material.GetFloat("innerRadius");
								ringOuterRadius = ringMR.material.GetFloat("outerRadius");

								Debug.Log ("[Scatterer] ring innerRadius (with parent scale) " + ringInnerRadius.ToString ());
								Debug.Log ("[Scatterer] ring outerRadius (with parent scale) " + ringOuterRadius.ToString ());

								ringInnerRadius *= 6000; //*6000 to convert to local space size
								ringOuterRadius *= 6000;
							}
							catch (Exception e)
							{
								Debug.Log("[Scatterer] Kopernicus ring exception "+e.ToString());
								Debug.Log("[Scatterer] Disabling ring shadows for "+parentCelestialBody.name);
								hasRingObjectAndShadowActivated=false;
							}
						}
					}
				}
			}

//			if (hasRingObjectAndShadowActivated)
//			{
//				m_skyMaterialScaled.EnableKeyword ("RINGSHADOW_ON");
//				m_skyMaterialScaled.DisableKeyword ("RINGSHADOW_OFF");
//				m_skyMaterialLocal.EnableKeyword ("RINGSHADOW_ON");
//				m_skyMaterialLocal.DisableKeyword ("RINGSHADOW_OFF");
//			}
//			else
//			{
//				m_skyMaterialScaled.DisableKeyword ("RINGSHADOW_ON");
//				m_skyMaterialScaled.EnableKeyword ("RINGSHADOW_OFF");
//				m_skyMaterialLocal.DisableKeyword ("RINGSHADOW_ON");
//				m_skyMaterialLocal.EnableKeyword ("RINGSHADOW_OFF");
//			}

			InitUniforms (m_skyMaterialScaled);
			InitUniforms (m_skyMaterialLocal);

			if (Core.Instance.fullLensFlareReplacement)
			{
				sunflareExtinctionMaterial = new Material (ShaderReplacer.Instance.LoadedShaders ["Scatterer/sunFlareExtinction"]);
				sunflareExtinctionMaterial.SetFloat ("Rg", Rg);
				sunflareExtinctionMaterial.SetFloat ("Rt", Rt);
				sunflareExtinctionMaterial.SetTexture ("_Sky_Transmittance", m_transmit);

				if (hasRingObjectAndShadowActivated)
				{
					sunflareExtinctionMaterial.SetFloat ("ringInnerRadius", ringInnerRadius);
					sunflareExtinctionMaterial.SetFloat ("ringOuterRadius", ringOuterRadius);
					
					sunflareExtinctionMaterial.SetVector ("ringNormal", ringObject.transform.up);
					
					sunflareExtinctionMaterial.SetTexture ("ringTexture", ringTexture);
				}

			}
			
		}
		
		public void UpdateStuff () //to be called by update at camera rythm for some graphical stuff
		{
			if (!inScaledSpace)
			{
				skyLocalMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
				skyLocalMesh.transform.localRotation = farCamera.transform.localRotation;
				skyLocalMesh.transform.rotation = farCamera.transform.rotation;

				if (!MapView.MapIsEnabled) {
					if (postprocessingEnabled) {
						InitPostprocessMaterial (m_atmosphereMaterial);
						UpdatePostProcessMaterial (m_atmosphereMaterial);
					}
				}
			}

			if (Core.Instance.useEclipses)
			{
				float scaleFactor=ScaledSpace.ScaleFactor;
				
				sunPosRelPlanet=Vector3.zero;
				if (scaledMode)
					sunPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
				else
					sunPosRelPlanet = m_manager.sunCelestialBody.transform.position;
				
				//build eclipse casters matrix
				castersMatrix1 = Matrix4x4.zero;
				castersMatrix2 = Matrix4x4.zero;
				
				Vector3 casterPosRelPlanet;
				for (int i=0; i< Mathf.Min(4, m_manager.eclipseCasters.Count); i++)
				{
					if (scaledMode)
						casterPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.eclipseCasters [i].transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
					else
						casterPosRelPlanet = m_manager.eclipseCasters [i].transform.position;
					
					castersMatrix1.SetRow (i, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y,
					                                       casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
				}
				
				
				
				for (int i=4; i< Mathf.Min(8, m_manager.eclipseCasters.Count); i++)
				{
					if (scaledMode)
						casterPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.eclipseCasters [i].transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
					else
						casterPosRelPlanet = m_manager.eclipseCasters [i].transform.position;
					castersMatrix2.SetRow (i - 4, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y,
					                                           casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
				}
			}

			if (Core.Instance.usePlanetShine)
			{
				planetShineRGBMatrix = Matrix4x4.zero;
				planetShineSourcesMatrix = Matrix4x4.zero;
				
				for (int i=0; i< Mathf.Min(4, m_manager.planetshineSources.Count); i++)
				{	
					Vector3 sourcePosRelPlanet;
					
					//offset lightsource position to make light follow lit crescent
					//i.e light doesn't come from the center of the planet but follows the lit side
					//1/4 of the way from center to surface should be fine
					Vector3d offsetPos=m_manager.planetshineSources[i].body.position
						+0.25*m_manager.planetshineSources[i].body.Radius*
							(m_manager.sunCelestialBody.position-m_manager.planetshineSources[i].body.position).normalized;
					
					if (scaledMode)
						sourcePosRelPlanet = Vector3.Scale(offsetPos - parentCelestialBody.GetTransform().position,new Vector3d(6000f,6000f,6000f));
					else
						sourcePosRelPlanet = offsetPos - parentCelestialBody.GetTransform().position;
					
					planetShineSourcesMatrix.SetRow (i, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y,
					                                                 sourcePosRelPlanet.z, m_manager.planetshineSources[i].isSun? 1.0f:0.0f ));
					
					float intensity = m_manager.planetshineSources[i].intensity;
					
					planetShineRGBMatrix.SetRow (i, new Vector4(m_manager.planetshineSources[i].color.x,m_manager.planetshineSources[i].color.y,
					                                            m_manager.planetshineSources[i].color.z,intensity));
				}
			}
			
			//update EVE cloud shaders
			//maybe refactor?
			if (Core.Instance.integrateWithEVEClouds && m_manager.usesCloudIntegration)
			{
				try
				{
					int size;

					//2d clouds
					if(Core.Instance.EVEClouds.ContainsKey(parentCelestialBody.name))
					{
						size = Core.Instance.EVEClouds[parentCelestialBody.name].Count;
						for (int i=0;i<size;i++)
						{
							//keep these for now or something breaks in the extinction
							//InitUniforms(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							SetUniforms(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							
							//InitPostprocessMaterial(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							
							//if (!inScaledSpace)
							UpdatePostProcessMaterial(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							
							//						Core.Instance.EVEClouds[parentCelestialBody.name][i].SetVector
							//							("_PlanetOrigin", m_manager.parentCelestialBody.transform.position);
							
							Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
								("cloudColorMultiplier", cloudColorMultiplier);
							Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
								("cloudScatteringMultiplier", cloudScatteringMultiplier);
							Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
								("cloudSkyIrradianceMultiplier", cloudSkyIrradianceMultiplier);


							Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("SCATTERER_ON");
							Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("SCATTERER_OFF");
						}
					}

					//volumetrics
					//if in local mode and mapping is done
					if (!inScaledSpace && !mapVolumetrics)
					{
						size = EVEvolumetrics.Count;

						for (int i=0;i<size;i++)
						{
							//keep these for now or something breaks in the extinction
							InitUniforms(EVEvolumetrics[i]);
							SetUniforms(EVEvolumetrics[i]);
							
							InitPostprocessMaterial(EVEvolumetrics[i]);
							UpdatePostProcessMaterial(EVEvolumetrics[i]);
							
							EVEvolumetrics[i].SetVector
								("_PlanetWorldPos", m_manager.parentCelestialBody.transform.position);
							
							EVEvolumetrics[i].SetFloat
								("cloudColorMultiplier", volumetricsColorMultiplier);
//							EVEvolumetrics[i].SetFloat
//								("cloudScatteringMultiplier", volumetricsScatteringMultiplier);
//							EVEvolumetrics[i].SetFloat
//								("cloudSkyIrradianceMultiplier", volumetricsSkyIrradianceMultiplier);

							EVEvolumetrics[i].EnableKeyword ("SCATTERER_ON");
							EVEvolumetrics[i].DisableKeyword ("SCATTERER_OFF");
						}
					}
				}
				catch (Exception)
				{
					Debug.Log("[Scatterer] Null EVE clouds, remapping...");
					Core.Instance.mapEVEClouds();
					mapVolumetrics=true;
				}
			}

			//update extinction for sunflares
			if (Core.Instance.fullLensFlareReplacement)
			{
				foreach (SunFlare customSunFlare in Core.Instance.customSunFlares)
				{
					//render extinction to texture
					sunflareExtinctionMaterial.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToCelestialBody (customSunFlare.source).normalized);

					if (!MapView.MapIsEnabled)
						sunflareExtinctionMaterial.SetVector ("_Globals_WorldCameraPos", Core.Instance.farCamera.transform.position - parentCelestialBody.transform.position);
					else
						sunflareExtinctionMaterial.SetVector ("_Globals_WorldCameraPos", (Vector3) ScaledSpace.ScaledToLocalSpace(Core.Instance.scaledSpaceCamera.transform.position) - parentCelestialBody.transform.position);

					Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 0); //pass 0 for sunflare extinction

					if (hasRingObjectAndShadowActivated)
					{
						sunflareExtinctionMaterial.SetVector("ringNormal", ringObject.transform.up);
						Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 1); //pass 1 for ring extinction
					}
				}
			}

			//			Shader.SetGlobalVector ("_PlanetOrigin", m_manager.parentCelestialBody.transform.position);
			//			Shader.SetGlobalFloat (ShaderProperties._GlobalOceanAlpha_PROPERTY, _GlobalOceanAlpha);
			
		}
		
		
		public void UpdateNode ()
		{

			if ((CurrentPQS != null) && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				bool prevState = inScaledSpace;
				inScaledSpace = !(CurrentPQS.isActive);
				//if we go from scaled to local space
				if (!inScaledSpace && prevState)
				{
					//set flag to map EVE volumetrics after a few frames
					if (Core.Instance.integrateWithEVEClouds)
						mapVolumetrics=true;
				}

				//if we go from local to scaled, clear volumetrics
				if (inScaledSpace && !prevState)
					EVEvolumetrics.Clear();
			}
			else
			{
				inScaledSpace = true;
			}

			//if we need to map volumetrics
			//wait for a few frames while EVE does its magic
			//then do the mapping
			if (mapVolumetrics)
			{
				if (waitCounter<10)
				{
					waitCounter++;
				}
				else
				{
					mapVolumetrics=false;
					waitCounter=0;
					mapEVEvolumetrics(); //do this last so if it fails we just ignore it
				}
			}

			if (!initiated)
			{
				m_radius = (float) m_manager.GetRadius ();
				
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;
				//				sunglareCutoffAlt = experimentalAtmoScale*(Rt - Rg);
				
				scaledSpaceCamera=Core.Instance.scaledSpaceCamera;

				{
					farCamera=Core.Instance.farCamera;
					nearCamera=Core.Instance.nearCamera;
				}
				
				
				backupAtmosphereMaterial ();
				tweakStockAtmosphere ();

				//disable postprocessing and ocean effects for Texture Replacer reflections
				DisableEffectsChecker effectsDisabler = atmosphereMesh.AddComponent<DisableEffectsChecker>();
				effectsDisabler.skynode = this;

//				if (Core.Instance.useOceanShaders && m_manager.hasOcean)
//				{
//					Core.Instance.refractionCam.waterMeshRenderers=m_manager.GetOceanNode().waterMeshRenderers;
//					Core.Instance.refractionCam.numGrids = m_manager.GetOceanNode().numGrids;
//					Core.Instance.refractionCam.postProcessingCube = atmosphereMeshrenderer;
//					Core.Instance.refractionCam.iSkyNode = this;
//					Debug.Log("skynode added refraction cam");
//				}

				//after the shader has been replaced by the modified scatterer shader, the properties are lost and need to be set again
				//call EVE clouds2D.reassign() method to set the shader properties
				if (Core.Instance.integrateWithEVEClouds)
				{
					try
					{
						const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
							BindingFlags.Instance | BindingFlags.Static;

						foreach (object _obj in Core.Instance.EVECloudObjects[parentCelestialBody.name]) 
						{
							object cloud2dObj = _obj.GetType().GetField("layer2D", flags).GetValue(_obj) as object;
							if (cloud2dObj==null)
							{
								Debug.Log("[Scatterer] layer2d not found for layer on planet: "+parentCelestialBody.name);
								continue;
							}

							bool cloud2dScaled = (bool) cloud2dObj.GetType().GetField("isScaled", flags).GetValue(cloud2dObj);

							MethodInfo scaledGetter = cloud2dObj.GetType().GetProperty("Scaled").GetGetMethod();
							MethodInfo scaledSetter = cloud2dObj.GetType().GetProperty("Scaled").GetSetMethod();

							//if in scaled mode, switch it to local then back to scaled, to set all the properties
							if (cloud2dScaled)
								scaledSetter.Invoke(cloud2dObj,new object[] { !cloud2dScaled });

							scaledSetter.Invoke(cloud2dObj,new object[] { cloud2dScaled });
						}

						//initialize other params here
						int size = Core.Instance.EVEClouds[parentCelestialBody.name].Count;
						for (int i=0;i<size;i++)
						{
							InitUniforms(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							InitPostprocessMaterial(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
							
							if (EVEIntegration_preserveCloudColors)
							{
								Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("PRESERVECLOUDCOLORS_ON");
								Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("PRESERVECLOUDCOLORS_OFF");
							}
							else
							{
								Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("PRESERVECLOUDCOLORS_OFF");
								Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("PRESERVECLOUDCOLORS_ON");
							}
							
						}
					}

					catch (Exception stupid)
					{
						Debug.Log ("[Scatterer] Error calling clouds2d.reassign() on planet: " + parentCelestialBody.name + stupid.ToString ());
					}
				}


				initiated = true;
				
			}
			else
			{
				if(!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				{
					alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
					
					trueAlt = alt - m_radius;
					
					
					interpolateVariables ();
				}
				
				if (scaledSpaceCamera && !updaterAdded)
				{
					
					updater = (UpdateOnCameraPreCull)scaledSpaceCamera.gameObject.AddComponent (typeof(UpdateOnCameraPreCull));

					//updater.settings (m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
					updater.settings (m_skyMaterialScaled, m_manager, this,parentCelestialBody.transform);
					
					updaterAdded = true;
				}
				
				atmosphereMeshrenderer.enabled = (!inScaledSpace) && (postprocessingEnabled);

				if(Core.Instance.useOceanShaders && m_manager.hasOcean)
				{
					atmosphereMeshrenderer.enabled = atmosphereMeshrenderer.enabled && (trueAlt >= 0);
				}

				bool localSkyCondition;
				if(!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
					localSkyCondition=alt > localSkyAltitude;
				else
					localSkyCondition = true;

				if(CurrentPQS!=null)
				{
					localSkyCondition=localSkyCondition && !CurrentPQS.isActive;   //inScaledSpace
				}
				
				if ((localSkyCondition || MapView.MapIsEnabled ) ^ scaledMode)
				{
					toggleScaledMode();
				}
				
				if (!scaledMode && Core.Instance.drawAtmoOnTopOfClouds && drawSkyOverClouds)
				{
					if (trueAlt<drawOverCloudsAltitude)
					{
						//						Debug.Log("under clouds");
						m_skyMaterialLocal.renderQueue=1000;
					}
					else
					{
						//						Debug.Log("over clouds");
						m_skyMaterialLocal.renderQueue=3002;
					}
				}
				
				if (Core.Instance.drawAtmoOnTopOfClouds && drawSkyOverClouds)
					m_skyMaterialScaled.renderQueue=3002;
				else
					m_skyMaterialScaled.renderQueue=2999; //this lets modified EVE clouds draw over sky
			}
		}
		
		
		
		
		public void SetUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need

			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, interpolatedSettings.viewdirOffset);
			}
			else
			{
				mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, 0f);
			}

			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
				mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
				mat.SetFloat (ShaderProperties.extinctionMultiplier_PROPERTY, interpolatedSettings.skyExtinctionMultiplier);
				mat.SetFloat (ShaderProperties.extinctionRimFade_PROPERTY, interpolatedSettings.skyextinctionRimFade);
				mat.SetFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, interpolatedSettings._extinctionScatterIntensity);
				mat.SetFloat (ShaderProperties.extinctionGroundFade_PROPERTY, interpolatedSettings.skyextinctionGroundFade);
			}
			else
			{
				mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, mapAlphaGlobal);
				mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, mapExtinctionTint);
				mat.SetFloat (ShaderProperties.extinctionMultiplier_PROPERTY, mapExtinctionMultiplier);
				mat.SetFloat (ShaderProperties.extinctionRimFade_PROPERTY, mapSkyExtinctionRimFade);
				mat.SetFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, _mapExtinctionScatterIntensity);
				mat.SetFloat (ShaderProperties.extinctionGroundFade_PROPERTY, mapGroundExtinctionFade);
			}
			
			
			mat.SetFloat (ShaderProperties.scale_PROPERTY, 1);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			
			//used to determine the view ray direction in the sky shader
			if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{				
				mat.SetMatrix (ShaderProperties._Globals_WorldToCamera_PROPERTY, farCamera.worldToCameraMatrix);
				mat.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, farCamera.worldToCameraMatrix.inverse);
			}
			else
			{
				mat.SetMatrix (ShaderProperties._Globals_WorldToCamera_PROPERTY, scaledSpaceCamera.worldToCameraMatrix);
				mat.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, scaledSpaceCamera.worldToCameraMatrix.inverse);
			}
			
			
			
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetFloat (ShaderProperties._Sun_Intensity_PROPERTY, 100f);
			
			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);
			
			Shader.SetGlobalVector (ShaderProperties._Godray_WorldSunDir_PROPERTY, m_manager.sunCelestialBody.transform.position
			                        - parentCelestialBody.transform.position);
			
			
			if (!MapView.MapIsEnabled && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION)) 
			{
				p = farCamera.projectionMatrix;
			}
			else
			{
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			Vector3 temp = ScaledSpace.ScaledToLocalSpace (scaledSpaceCamera.transform.position);
			mat.SetVector (ShaderProperties._Globals_WorldCameraPos_PROPERTY, temp);
			
			
			#if skyScaledBox
			if (scaledMode)
				mat.SetVector (ShaderProperties._Scatterer_Origin_PROPERTY, Vector3.Scale(ParentPlanetTransform.transform.position, new Vector3(6000f,6000f,6000f)));
			else
				mat.SetVector (ShaderProperties._Scatterer_Origin_PROPERTY, parentCelestialBody.transform.position);
			#else
			mat.SetVector (ShaderProperties._Scatterer_Origin_PROPERTY, parentCelestialBody.transform.position);
			#endif
			
			if (!MapView.MapIsEnabled) {
				mat.SetFloat (ShaderProperties._Exposure_PROPERTY, interpolatedSettings.skyExposure);
				mat.SetFloat (ShaderProperties._RimExposure_PROPERTY, interpolatedSettings.skyRimExposure);
			} else {
				mat.SetFloat (ShaderProperties._Exposure_PROPERTY, mapExposure);
				mat.SetFloat (ShaderProperties._RimExposure_PROPERTY, mapSkyRimExposure);
			}
			
			
			
			if (Core.Instance.useEclipses)
			{
				mat.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				mat.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
				mat.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                       sunPosRelPlanet.z, (float)m_manager.sunCelestialBody.Radius));
			}
			
			
			if (Core.Instance.usePlanetShine)
			{
				mat.SetMatrix ("planetShineSources", planetShineSourcesMatrix);
				mat.SetMatrix ("planetShineRGB", planetShineRGBMatrix);
			}

			if (hasRingObjectAndShadowActivated)
			{
				mat.SetVector("ringNormal", ringObject.transform.up);
			}
			
		}
		
		
		public void SetOceanUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			
			mat.SetFloat (ShaderProperties._Exposure_PROPERTY, interpolatedSettings.skyRimExposure);
			
			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			
			mat.SetFloat (ShaderProperties.scale_PROPERTY, 1);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetFloat (ShaderProperties._Sun_Intensity_PROPERTY, 100f);
			
			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);

			mat.SetVector("_camForward", farCamera.transform.forward);
		}
		
		
		
		
		public void InitPostprocessMaterial (Material mat)
		{
			
			mat.SetTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
			
			//			if (Core.Instance.render24bitDepthBuffer)
			mat.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Core.Instance.customDepthBufferTexture);
			
			if (Core.Instance.useGodrays)
				mat.SetTexture (ShaderProperties._godrayDepthTexture_PROPERTY, Core.Instance.godrayDepthTexture);
			
			//Consts, best leave these alone
			mat.SetFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rl_PROPERTY, RL * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RES_R_PROPERTY, RES_R);
			mat.SetFloat (ShaderProperties.RES_MU_PROPERTY, RES_MU);
			mat.SetFloat (ShaderProperties.RES_MU_S_PROPERTY, RES_MU_S);
			mat.SetFloat (ShaderProperties.RES_NU_PROPERTY, RES_NU);
			mat.SetFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			mat.SetFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			
			mat.SetVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f);
			mat.SetVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f) / 0.9f);
			
			mat.SetFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f);
			mat.SetFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f);
			
			
			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun().normalized);

			if (Core.Instance.usePlanetShine)
			{
				mat.EnableKeyword ("PLANETSHINE_ON");
				mat.DisableKeyword ("PLANETSHINE_OFF");	
			}
			else
			{
				mat.DisableKeyword ("PLANETSHINE_ON");
				mat.EnableKeyword ("PLANETSHINE_OFF");
			}
		}
		
		
		public void UpdatePostProcessMaterial (Material mat)
		{
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("Rl", RL * atmosphereGlobalScale);
			
			
			mat.SetFloat ("_experimentalAtmoScale", experimentalAtmoScale);
			
			
			mat.SetFloat ("_global_alpha", interpolatedSettings.postProcessAlpha);
			mat.SetFloat ("_Exposure", interpolatedSettings.postProcessExposure);
			mat.SetFloat ("_global_depth", interpolatedSettings.postProcessDepth *1000000);
			
			
			mat.SetFloat ("_Post_Extinction_Tint", interpolatedSettings._Post_Extinction_Tint);
			mat.SetFloat ("postExtinctionMultiplier", interpolatedSettings.postExtinctionMultiplier);

			mat.SetFloat ("_openglThreshold", interpolatedSettings.openglThreshold);

			
			mat.SetFloat("_Scale", 1);

			mat.SetVector ("SUN_DIR", m_manager.getDirectionToSun ().normalized);
			mat.SetFloat ("SUN_INTENSITY", sunIntensity);
			
			if (farCamera)
			{
				mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
				mat.SetVector ("_camPos", farCamera.transform.position - parentCelestialBody.transform.position);  //better do this small calculation here

				mat.SetVector("_camForward", farCamera.transform.forward);

				Vector3d tmp = (farCamera.transform.position) - m_manager.parentCelestialBody.transform.position;

				Matrix4x4 ctol1 = farCamera.cameraToWorldMatrix;
				Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);

				Matrix4x4d viewMat = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
				                                     ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
				                                     ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
				                                     ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);
				
				viewMat = viewMat.Inverse ();				
				Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4 ());
				mat.SetMatrix ("_ViewProjInv", viewProjMat.inverse);
			
				//set directions of frustum corners in world space
				//used to reconstruct world pos from view-space depth

				Vector3 topLeft = farCamera.ViewportPointToRay(new Vector3(0f,1f,0f)).direction;
				topLeft.Normalize();
				
				Vector3 topRight = farCamera.ViewportPointToRay(new Vector3(1f,1f,0f)).direction;
				topRight.Normalize();
				
				Vector3 bottomRight = farCamera.ViewportPointToRay(new Vector3(1f,0f,0f)).direction;
				bottomRight.Normalize();
				
				Vector3 bottomLeft = farCamera.ViewportPointToRay(new Vector3(0f,0f,0f)).direction;
				bottomRight.Normalize();

				Matrix4x4 _frustumCorners = Matrix4x4.identity;

				_frustumCorners.SetRow (0, bottomLeft); 
				_frustumCorners.SetRow (1, bottomRight);		
				_frustumCorners.SetRow (2, topLeft);
				_frustumCorners.SetRow (3, topRight);	
				
				mat.SetMatrix ("scattererFrustumCorners", _frustumCorners);
			}
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			if (Core.Instance.usePlanetShine)
			{
				mat.SetMatrix ("planetShineSources", planetShineSourcesMatrix);
				mat.SetMatrix ("planetShineRGB", planetShineRGBMatrix);
			}
		}
		
		public void InitUniforms (Material mat)
		{
			//Init uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			
			mat.SetFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f);
			mat.SetTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
			mat.SetFloat (ShaderProperties.scale_PROPERTY, Rg * 1 / m_radius);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			mat.SetFloat (ShaderProperties.TRANSMITTANCE_W_PROPERTY, TRANSMITTANCE_W);
			mat.SetFloat (ShaderProperties.TRANSMITTANCE_H_PROPERTY, TRANSMITTANCE_H);
			mat.SetFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			mat.SetFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			mat.SetFloat (ShaderProperties.RES_R_PROPERTY, RES_R);
			mat.SetFloat (ShaderProperties.RES_MU_PROPERTY, RES_MU);
			mat.SetFloat (ShaderProperties.RES_MU_S_PROPERTY, RES_MU_S);
			mat.SetFloat (ShaderProperties.RES_NU_PROPERTY, RES_NU);
			mat.SetFloat (ShaderProperties.AVERAGE_GROUND_REFLECTANCE_PROPERTY, AVERAGE_GROUND_REFLECTANCE);
			mat.SetFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f);
			mat.SetFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f);
			mat.SetVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f);
			mat.SetVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f) / 0.9f);
			//			mat.SetFloat (ShaderProperties._sunglareScale_PROPERTY, sunglareScale);	

			//ring shadow parameters
			if (hasRingObjectAndShadowActivated)
			{
				mat.EnableKeyword ("RINGSHADOW_ON");
				mat.DisableKeyword ("RINGSHADOW_OFF");

				mat.SetFloat ("ringInnerRadius", ringInnerRadius);
				mat.SetFloat ("ringOuterRadius", ringOuterRadius);

				mat.SetVector ("ringNormal", ringObject.transform.up);

				mat.SetTexture ("ringTexture", ringTexture);
			}
			else
			{
				mat.DisableKeyword ("RINGSHADOW_ON");
				mat.EnableKeyword ("RINGSHADOW_OFF");
				mat.DisableKeyword ("RINGSHADOW_ON");
				mat.EnableKeyword ("RINGSHADOW_OFF");
			}


			if (Core.Instance.useEclipses)
			{
				mat.EnableKeyword ("ECLIPSES_ON");
				mat.DisableKeyword ("ECLIPSES_OFF");
			}
			else
			{
				mat.DisableKeyword ("ECLIPSES_ON");
				mat.EnableKeyword ("ECLIPSES_OFF");
			}
			
			if (Core.Instance.usePlanetShine)
			{
				mat.EnableKeyword ("PLANETSHINE_ON");
				mat.DisableKeyword ("PLANETSHINE_OFF");	
			}
			else
			{
				mat.DisableKeyword ("PLANETSHINE_ON");
				mat.EnableKeyword ("PLANETSHINE_OFF");
			}
		}

		
		public void setManager (Manager manager)
		{
			m_manager = manager;
		}
		
		public void togglePostProcessing()
		{
			postprocessingEnabled = !postprocessingEnabled;
		}
		
		public void SetParentCelestialBody (CelestialBody inPlanet)
		{
			parentCelestialBody = inPlanet;
		}
		
		public void setParentPlanetTransform (Transform parentTransform)
		{
			ParentPlanetTransform = parentTransform;
		}
		
		
		void loadPrecomputedTables ()
		{
			
			//load from .half, probably an 8 mb leak every scene change
			//if no .half file exists, load from .raw file and create .half file
			string _file = Core.Instance.gameDataPath + assetPath + "/inscatter.half";
			if (System.IO.File.Exists(_file))
				m_inscatter.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				loadAndConvertRawFile("inscatter",m_inscatter,4);
			
			_file = Core.Instance.gameDataPath + assetPath + "/transmittance.half";
			
			if (System.IO.File.Exists(_file))
				m_transmit.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				loadAndConvertRawFile("transmittance",m_transmit,3);
			
			_file = Core.Instance.gameDataPath + assetPath + "/irradiance.half";
			
			if (System.IO.File.Exists(_file))
				m_irradiance.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				loadAndConvertRawFile("irradiance",m_irradiance,3);
			
			
			m_inscatter.Apply ();
			m_transmit.Apply ();
			m_irradiance.Apply ();
			
			
			encode = null;
		}
		
		
		void loadAndConvertRawFile(string textureName, Texture2D targetTexture2D, int channels)
		{
			
			if(encode==null)
				encode = new EncodeFloat ();
			
			string _file = Core.Instance.gameDataPath + assetPath + "/"+textureName+".raw";

			if (!System.IO.File.Exists(_file))
			{
				Debug.Log("[Scatterer] no "+textureName+".raw or "+textureName+".half file found for "
				          +parentCelestialBody.name);
				return;
			}
			
			RenderTexture activeRT = RenderTexture.active;
			RenderTexture tempRT = new RenderTexture (targetTexture2D.width, targetTexture2D.height, 0, RenderTextureFormat.ARGBFloat);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			tempRT.Create ();
			
			encode.WriteIntoRenderTexture (tempRT, channels, _file);
						
			RenderTexture.active = tempRT;
			targetTexture2D.ReadPixels(new Rect(0, 0, targetTexture2D.width, targetTexture2D.height), 0, 0);
			targetTexture2D.Apply();
			
			RenderTexture.active = activeRT;
			tempRT.Release ();
			
			_file = Core.Instance.gameDataPath + assetPath + "/"+textureName+".half";
			
			byte[] bytes = targetTexture2D .GetRawTextureData();
			System.IO.File.WriteAllBytes(_file ,bytes);
			
			Debug.Log ("[Scatterer] Converted "+textureName+".raw to "+textureName+".half");
			
			UnityEngine.Object.Destroy (tempRT);
			bytes = null;
		}
		
		
		
		public void OnDestroy ()
		{
			saveToConfigNode ();
			if (m_transmit)
			{
				UnityEngine.Object.Destroy (m_transmit);
			}

			if (m_irradiance)
			{
				UnityEngine.Object.Destroy (m_irradiance);
			}

			if (m_inscatter)
			{
				UnityEngine.Object.Destroy (m_inscatter);
			}

			Component.Destroy (updater);
			UnityEngine.Object.Destroy (updater);

			UnityEngine.Object.Destroy(atmosphereMesh);
			Component.Destroy(atmosphereMeshrenderer);
			Component.Destroy(atmosphereMeshFilter);
			
			Component.Destroy (skyScaledMeshrenderer);
			UnityEngine.Object.Destroy (skyScaledMesh);
			
			Component.Destroy (skyLocalMeshrenderer);
			UnityEngine.Object.Destroy (skyLocalMesh);
			RestoreStockAtmosphere ();
			UnityEngine.Object.Destroy (alteredMaterial);
			UnityEngine.Object.Destroy (originalMaterial);

			//disable eve integration scatterer flag
			if (Core.Instance.integrateWithEVEClouds && m_manager.usesCloudIntegration)
			{
				try
				{
					int size;
					
					//2d clouds
					if(Core.Instance.EVEClouds.ContainsKey(parentCelestialBody.name))
					{
						size = Core.Instance.EVEClouds[parentCelestialBody.name].Count;
						for (int i=0;i<size;i++)
						{
							Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("SCATTERER_ON");
							Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("SCATTERER_OFF");
						}
					}
					
					//volumetrics
					//if in local mode and mapping is done
					if (!inScaledSpace && !mapVolumetrics)
					{
						size = EVEvolumetrics.Count;
						
						for (int i=0;i<size;i++)
						{
							EVEvolumetrics[i].DisableKeyword ("SCATTERER_ON");
							EVEvolumetrics[i].EnableKeyword ("SCATTERER_OFF");
						}
					}
				}
				catch (Exception)
				{
					//TODO
				}
			}
		}
		
		void toggleScaledMode()
		{
			if (scaledMode) //switch to localMode
			{
				if (Core.Instance.useOceanShaders && m_manager.hasOcean && Core.Instance.oceanRefraction)
				{
					Core.Instance.refractionCam.waterMeshRenderers=m_manager.GetOceanNode().waterMeshRenderers;
					Core.Instance.refractionCam.numGrids = m_manager.GetOceanNode().numGrids;
					Core.Instance.refractionCam.postProcessingCube = atmosphereMeshrenderer;
					Core.Instance.refractionCam.iSkyNode = this;
					Debug.Log("skynode added refraction cam");
				}

				skyScaledMeshrenderer.enabled = false;
				skyLocalMeshrenderer.enabled=true;

				updater.settings (m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
				
				scaledMode=false;
				Debug.Log("[Scatterer] Sky switched to local mode");
			}
			else   //switch to scaledMode
			{
				skyScaledMeshrenderer.enabled = true;
				skyLocalMeshrenderer.enabled=false;

				updater.settings (m_skyMaterialScaled, m_manager, this,parentCelestialBody.transform);
				
				scaledMode=true;
				Debug.Log("[Scatterer] Sky switched to scaled mode");
			}
		}

		void toggleStockSunglare ()
		{
			if (stocksunglareEnabled) {
				Sun.Instance.sunFlare.enabled = false;
			} else {
				Sun.Instance.sunFlare.enabled = true;
			}
			stocksunglareEnabled = !stocksunglareEnabled;
		}

		public void loadFromConfigNode ()
		{
			ConfigNode cnToLoad = new ConfigNode();

			foreach (UrlDir.UrlConfig _url in Core.Instance.atmoConfigs)
			{
				if (_url.config.TryGetNode(parentCelestialBody.name,ref cnToLoad))
				{
					configUrl = _url;
					Debug.Log("[Scatterer] Atmosphere config found for: "+parentCelestialBody.name);
					break;
				}
			}

			ConfigNode.LoadObjectFromConfig (this, cnToLoad);
			
			m_radius = (float) m_manager.GetRadius ();
			
			Rt = (Rt / Rg) * m_radius;
			RL = (RL / Rg) * m_radius;
			Rg = m_radius;
		}
		
		public void saveToConfigNode ()
		{
			configUrl.config.RemoveNodes (parentCelestialBody.name);

			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
			cnTemp.name = parentCelestialBody.name;
			configUrl.config.AddNode (cnTemp);

			Debug.Log("[Scatterer] saving "+parentCelestialBody.name+
			          " atmo config to: "+configUrl.parent.url);
			configUrl.parent.SaveConfigs ();
		}
		
		public void backupAtmosphereMaterial ()
		{
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (true);
					//					originalMaterial = (Material)Material.Instantiate (t.renderer.sharedMaterial);
					Renderer tRenderer=(Renderer) t.GetComponent(typeof(Renderer));
					originalMaterial = (Material)Material.Instantiate (tRenderer.sharedMaterial);
					
					//					alteredMaterial = (Material)Material.Instantiate (t.renderer.sharedMaterial);
					alteredMaterial = (Material)Material.Instantiate (tRenderer.sharedMaterial);
					//					t.renderer.sharedMaterial = alteredMaterial;
					tRenderer.sharedMaterial = alteredMaterial;
					i = t.childCount + 10;
				}
			}
		}

		//surely there is a simpler way to do this
		public void RestoreStockAtmosphere ()
		{
			Transform t = ScaledSpace.Instance.transform.FindChild (ParentPlanetTransform.name);
			for (int i = 0; i < t.childCount; i++) {
				if (t.GetChild (i).gameObject.layer == 9) {
					t.GetChild (i).gameObject.GetComponent < MeshRenderer > ().gameObject.SetActive (true);
					if (originalMaterial)
					{
						Renderer tRenderer=(Renderer) t.GetComponent(typeof(Renderer));
						//						t.renderer.sharedMaterial = (Material)Material.Instantiate (originalMaterial);
						tRenderer.sharedMaterial = (Material)Material.Instantiate (originalMaterial);
					}
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
					Renderer tRenderer=(Renderer) t.GetComponent(typeof(Renderer));
					//					Material sharedMaterial = t.renderer.sharedMaterial;
					Material sharedMaterial = tRenderer.sharedMaterial;
					
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimBlend"), rimBlend / 100f);
					sharedMaterial.SetFloat (Shader.PropertyToID ("_rimPower"), rimpower / 100f);
					sharedMaterial.SetColor ("_SpecColor", new Color (specR / 100f, specG / 100f, specB / 100f));
					sharedMaterial.SetFloat ("_Shininess", shininess / 100);
					
					i = t.childCount + 10;
				}
			}
		}

		public void interpolateVariables ()
		{
			if (trueAlt <= configPoints [0].altitude)
			{
				interpolatedSettings.getValuesFrom(configPoints [0]);
				currentConfigPoint = 0;	
			}
			else if (trueAlt > configPoints [configPoints.Count - 1].altitude) 
			{
				interpolatedSettings.getValuesFrom(configPoints [configPoints.Count - 1]);
				currentConfigPoint = configPoints.Count;
			}
			else 
			{
				for (int j = 1; j < configPoints.Count; j++)
				{
					if ((trueAlt > configPoints [j - 1].altitude) && (trueAlt <= configPoints [j].altitude))
					{
						percentage = (trueAlt - configPoints [j - 1].altitude) / (configPoints [j].altitude - configPoints [j - 1].altitude);
						interpolatedSettings.interpolateValuesFrom(configPoints [j - 1], configPoints [j], percentage);
						currentConfigPoint = j;
					}
				}
			}
		}

		public void mapEVEvolumetrics()
		{
			Debug.Log ("[Scatterer] Mapping EVE volumetrics for planet: "+parentCelestialBody.name);

			EVEvolumetrics.Clear ();

			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;

			if (Core.Instance.EVECloudObjects.ContainsKey (parentCelestialBody.name)) //EVECloudObjects contain both the 2d clouds and the volumetrics, here we extract the volumetrics
			{
				List<object> cloudObjs = Core.Instance.EVECloudObjects [parentCelestialBody.name];
				
				foreach (object _obj in cloudObjs)
				{
					try
					{
						object cloudsPQS = _obj.GetType ().GetField ("cloudsPQS", flags).GetValue (_obj) as object;
						object layerVolume = cloudsPQS.GetType ().GetField ("layerVolume", flags).GetValue (cloudsPQS) as object;
						Material ParticleMaterial = layerVolume.GetType ().GetField ("ParticleMaterial", flags).GetValue (layerVolume) as Material;
											
						ParticleMaterial.EnableKeyword ("SCATTERER_ON");
						ParticleMaterial.DisableKeyword ("SCATTERER_OFF");

						EVEvolumetrics.Add (ParticleMaterial);
					}
					catch (Exception stupid)
					{
						Debug.Log ("[Scatterer] Volumetric clouds error on planet: " + parentCelestialBody.name + stupid.ToString ());
					}
				}				
				Debug.Log ("[Scatterer] Detected " + EVEvolumetrics.Count + " EVE volumetric layers for planet: " + parentCelestialBody.name);
			}
			else
			{
				Debug.Log ("[Scatterer] No cloud objects for planet: " + parentCelestialBody.name);
			}
		}

		public void togglePreserveCloudColors()
		{
			if (Core.Instance.integrateWithEVEClouds)
			{
				if(Core.Instance.EVEClouds.ContainsKey(parentCelestialBody.name)) //change to a bool hasclouds
				{
					int size = Core.Instance.EVEClouds[parentCelestialBody.name].Count;
					for (int i=0;i<size;i++)
					{
						if (EVEIntegration_preserveCloudColors)
						{
							Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("PRESERVECLOUDCOLORS_OFF");
							Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("PRESERVECLOUDCOLORS_ON");
						}
						else
						{
							Core.Instance.EVEClouds[parentCelestialBody.name][i].EnableKeyword ("PRESERVECLOUDCOLORS_ON");
							Core.Instance.EVEClouds[parentCelestialBody.name][i].DisableKeyword ("PRESERVECLOUDCOLORS_OFF");
						}
					}
				}
				EVEIntegration_preserveCloudColors =!EVEIntegration_preserveCloudColors;
			}
		}

	}
}