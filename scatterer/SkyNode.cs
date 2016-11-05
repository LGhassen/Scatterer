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

		SimplePostProcessCube postProcessCube;
		GameObject atmosphereMesh;
		MeshRenderer atmosphereMeshrenderer;
		
		SimplePostProcessCube skyScaledCube;
		GameObject skyScaledMesh;
		MeshRenderer skyScaledMeshrenderer;
		
		SimplePostProcessCube skyLocalCube;
		GameObject skyLocalMesh;
		MeshRenderer skyLocalMeshrenderer;
		
		float localSkyAltitude;
		
		[Persistent]
		public bool displayInterpolatedVariables = false;
		
		updateAtCameraRythm updater;
		bool updaterAdded = false;
		
		Matrix4x4 castersMatrix1=Matrix4x4.zero;
		Matrix4x4 castersMatrix2=Matrix4x4.zero;
		
		public Matrix4x4 planetShineSourcesMatrix=Matrix4x4.zero;
		public Matrix4x4 planetShineRGBMatrix=Matrix4x4.zero;
		
		Vector3 sunPosRelPlanet=Vector3.zero;
		
		public bool scaledMode = false;
		
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
		public float MapViewScale = 1f;

		CelestialBody parentCelestialBody;
		Transform ParentPlanetTransform;
		
		
		bool stocksunglareEnabled = true;
				
		//atmosphere properties
		public configPoint interpolatedSettings= new configPoint();
		[Persistent]
		public float mapExposure = 0.15f;
		[Persistent]
		public float mapSkyRimExposure = 0.15f;
		[Persistent]
		public float cloudColorMultiplier=1f;
		[Persistent]
		public float cloudScatteringMultiplier=1f;
		[Persistent]
		public float cloudExtinctionHeightMultiplier=1f;
		
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
			new configPoint(5000f, 1f, 0.25f,0.25f, 1f, 0.4f, 0.23f, 1f, 100f,0f, 0f, 250f, 0.5f,0f,100f,100f,1f,1f)
			, new configPoint(15000f, 1f, 0.15f,0.15f, 1f, 8f, 0.23f, 1f, 100f,0f,0f, 250f, 0.5f,0f,100f,100f,1f,1f)
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

			if (Core.Instance.useEclipses)
			{
				m_skyMaterialScaled.EnableKeyword ("ECLIPSES_ON");
				m_skyMaterialScaled.DisableKeyword ("ECLIPSES_OFF");
				m_skyMaterialLocal.EnableKeyword ("ECLIPSES_ON");
				m_skyMaterialLocal.DisableKeyword ("ECLIPSES_OFF");
			}
			else
			{
				m_skyMaterialScaled.DisableKeyword ("ECLIPSES_ON");
				m_skyMaterialScaled.EnableKeyword ("ECLIPSES_OFF");
				m_skyMaterialLocal.DisableKeyword ("ECLIPSES_ON");
				m_skyMaterialLocal.EnableKeyword ("ECLIPSES_OFF");
			}
			
			if (Core.Instance.usePlanetShine)
			{
				m_skyMaterialScaled.EnableKeyword ("PLANETSHINE_ON");
				m_skyMaterialScaled.DisableKeyword ("PLANETSHINE_OFF");
				m_skyMaterialLocal.EnableKeyword ("PLANETSHINE_ON");
				m_skyMaterialLocal.DisableKeyword ("PLANETSHINE_OFF");
				m_atmosphereMaterial.EnableKeyword ("PLANETSHINE_ON");
				m_atmosphereMaterial.DisableKeyword ("PLANETSHINE_OFF");
				
				
			}
			else
			{
				m_skyMaterialScaled.DisableKeyword ("PLANETSHINE_ON");
				m_skyMaterialScaled.EnableKeyword ("PLANETSHINE_OFF");
				m_skyMaterialLocal.DisableKeyword ("PLANETSHINE_ON");
				m_skyMaterialLocal.EnableKeyword ("PLANETSHINE_OFF");
				m_atmosphereMaterial.DisableKeyword ("PLANETSHINE_ON");
				m_atmosphereMaterial.EnableKeyword ("PLANETSHINE_OFF");
			}

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
			
			
			InitUniforms (m_skyMaterialScaled);
			InitUniforms (m_skyMaterialLocal);
			
			
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
			
			
			postProcessCube = new SimplePostProcessCube (10000, m_atmosphereMaterial,false);
			atmosphereMesh = postProcessCube.GameObject;
			atmosphereMesh.layer = 15;
			atmosphereMeshrenderer = postProcessCube.GameObject.GetComponent < MeshRenderer > ();
			atmosphereMeshrenderer.material = m_atmosphereMaterial;
			
			
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
			
			
			skyScaledMeshrenderer.enabled = false;
			
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
//			m_skyMaterialLocal.renderQueue=3001; //render to background unless over clouds
			skyLocalMeshrenderer.enabled = true;
			
			#endif
			
			
		}
		
		public void UpdateStuff () //to be called by update at camera rythm for some graphical stuff
		{
			//			skyScaledMesh.transform.position = ParentPlanetTransform.position;
			
			atmosphereMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
			atmosphereMesh.transform.localRotation = farCamera.transform.localRotation;
			atmosphereMesh.transform.rotation = farCamera.transform.rotation;
			
			skyLocalMesh.transform.position = farCamera.transform.position + postDist * farCamera.transform.forward;
			skyLocalMesh.transform.localRotation = farCamera.transform.localRotation;
			skyLocalMesh.transform.rotation = farCamera.transform.rotation;
			
			if ((!inScaledSpace) && (!MapView.MapIsEnabled))
			{
				if (postprocessingEnabled) {
					InitPostprocessMaterial (m_atmosphereMaterial);
					UpdatePostProcessMaterial (m_atmosphereMaterial);
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
				
				//build and set planetShine sources and RGB
				//				int currentCount=0;
				//
				//				for (int i=0; i< Mathf.Min(4, m_manager.additionalSuns.Count); i++)
				//				{
				//
				//					Vector3 sourcePosRelPlanet = (m_manager.additionalSuns[i].position - parentCelestialBody.GetTransform().position).normalized;
				//
				//					planetShineSourcesMatrix.SetRow (currentCount, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y,
				//					                                                  sourcePosRelPlanet.z, 1.0f));
				//		
				//					planetShineRGBMatrix.SetRow (currentCount, Vector4.one);
				//
				//					currentCount++;
				//				}
				//
				//
				//				for (int i=0; i< Mathf.Min(4, m_manager.planetShineLightSources.Count); i++)
				//				{
				//					if (currentCount>3)
				//						break;
				//
				//					Vector3 sourcePosRelPlanet = (m_manager.planetShineLightSources[i].position - parentCelestialBody.GetTransform().position).normalized;
				//					
				//					planetShineSourcesMatrix.SetRow (currentCount, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y,
				//					                                                  sourcePosRelPlanet.z, 0.0f));
				//
				//					planetShineRGBMatrix.SetRow (currentCount, Vector4.one);
				//					
				//					currentCount++;
				//				}
				
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
					
					//compute reflected light intensity if source is not a sun
					//moved to shader for better, less rigid effect
					//					if (!m_manager.planetshineSources[i].isSun)
					//					{
					//						Vector3 sunPosRelPlanet = (m_manager.sunCelestialBody.position - parentCelestialBody.GetTransform().position).normalized;
					//
					//						//intensity *= Mathf.SmoothStep(0,1, Mathf.Clamp01(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet)));
					////						intensity *= Mathf.SmoothStep(0,1, 0.5f*(1+(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet))));
					//						intensity *= 0.5f*(1+(-Vector3.Dot(sourcePosRelPlanet,sunPosRelPlanet)));
					//					}
					
					planetShineRGBMatrix.SetRow (i, new Vector4(m_manager.planetshineSources[i].color.x,m_manager.planetshineSources[i].color.y,
					                                            m_manager.planetshineSources[i].color.z,intensity));
				}
				//
				//				Debug.Log (planetShineSourcesMatrix.ToString());
			}
			
			
			//			InitUniformsGlobal();
			//			SetUniformsGlobal ();
			//
			//			InitPostprocessMaterialGlobal();
			//			UpdatePostProcessMaterialGlobal();
			
			//update EVE cloud shaders
			//maybe refactor?
			if ((Core.Instance.integrateWithEVEClouds) && (Core.Instance.EVEClouds.ContainsKey(parentCelestialBody.name)))
			{
				//2d clouds
				int size = Core.Instance.EVEClouds[parentCelestialBody.name].Count;
				for (int i=0;i<size;i++)
				{
					//keep these for now or something breaks in the extinction
					InitUniforms(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
					SetUniforms(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
					
					InitPostprocessMaterial(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
					UpdatePostProcessMaterial(Core.Instance.EVEClouds[parentCelestialBody.name][i]);
					
					Core.Instance.EVEClouds[parentCelestialBody.name][i].SetVector
						("_PlanetOrigin", m_manager.parentCelestialBody.transform.position);

					Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
						("cloudColorMultiplier", cloudColorMultiplier);
					Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
						("cloudScatteringMultiplier", cloudScatteringMultiplier);
					Core.Instance.EVEClouds[parentCelestialBody.name][i].SetFloat
						("cloudExtinctionHeightMultiplier", cloudExtinctionHeightMultiplier);
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
							("_PlanetOrigin", m_manager.parentCelestialBody.transform.position);
						
						EVEvolumetrics[i].SetFloat
							("cloudColorMultiplier", cloudColorMultiplier);
						EVEvolumetrics[i].SetFloat
							("cloudScatteringMultiplier", cloudScatteringMultiplier);
						EVEvolumetrics[i].SetFloat
							("cloudExtinctionHeightMultiplier", cloudExtinctionHeightMultiplier);
					}
				}
			}
			
			//			Shader.SetGlobalVector ("_PlanetOrigin", m_manager.parentCelestialBody.transform.position);
			//			Shader.SetGlobalFloat (ShaderProperties._GlobalOceanAlpha_PROPERTY, _GlobalOceanAlpha);
			
		}
		
		
		public void UpdateNode ()
		{

			if (CurrentPQS != null)
			{
				bool prevState = inScaledSpace;
				inScaledSpace = !(CurrentPQS.isActive);
				//if we go from scaled to local space
				if (!inScaledSpace && prevState)
				{
					//set flag to map EVE volumetrics after a few frames
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
				if (waitCounter<6)
				{
					waitCounter++;
				}
				else
				{
					mapEVEvolumetrics();
					mapVolumetrics=false;
					waitCounter=0;
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
				//				if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				{
					farCamera=Core.Instance.farCamera;
					nearCamera=Core.Instance.nearCamera;
				}
				
				
				backupAtmosphereMaterial ();
				tweakStockAtmosphere ();
				
				initiated = true;
				
			}
			else
			{
				//				if(!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				alt = Vector3.Distance (farCamera.transform.position, parentCelestialBody.transform.position);
				//				else
				//					alt = Vector3.Distance (ScaledSpace.ScaledToLocalSpace(scaledSpaceCamera.transform.position), parentCelestialBody.transform.position);
				
				trueAlt = alt - m_radius;
				
				
				interpolateVariables ();
				
				if (scaledSpaceCamera && !updaterAdded)
				{
					
					updater = (updateAtCameraRythm)scaledSpaceCamera.gameObject.AddComponent (typeof(updateAtCameraRythm));
					
					//start in localmode
					//					updater.settings (m_mesh, m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
					updater.settings (m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
					
					updaterAdded = true;
				}
				
				atmosphereMeshrenderer.enabled = (!inScaledSpace) && (postprocessingEnabled);
				
				bool localSkyCondition;

				localSkyCondition=alt > localSkyAltitude;
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
					m_skyMaterialScaled.renderQueue=3001;
			}
		}
		
		
		
		
		public void SetUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			
			
			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, interpolatedSettings.viewdirOffset);
			}
			else
			{
				mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, 0f);
			}
			
			mat.SetFloat (ShaderProperties.extinctionGroundFade_PROPERTY, interpolatedSettings.skyextinctionGroundFade);
			
			if (!MapView.MapIsEnabled)
			{
				mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
				mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
				mat.SetFloat (ShaderProperties.extinctionMultiplier_PROPERTY, interpolatedSettings.skyExtinctionMultiplier);
				mat.SetFloat (ShaderProperties.extinctionRimFade_PROPERTY, interpolatedSettings.skyextinctionRimFade);
				mat.SetFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, interpolatedSettings._extinctionScatterIntensity);
			}
			else
			{
				mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, mapAlphaGlobal);
				mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, mapExtinctionTint);
				mat.SetFloat (ShaderProperties.extinctionMultiplier_PROPERTY, mapExtinctionMultiplier);
				mat.SetFloat (ShaderProperties.extinctionRimFade_PROPERTY, mapSkyExtinctionRimFade);
				mat.SetFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, _mapExtinctionScatterIntensity);
			}
			
			
			mat.SetFloat (ShaderProperties.scale_PROPERTY, 1);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			
			//used to determine the view ray direction in the sky shader
			if (!MapView.MapIsEnabled)
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
			
			
			if (!MapView.MapIsEnabled) {
				p = farCamera.projectionMatrix;
			} else {
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			mat.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, m_cameraToScreenMatrix.ToMatrix4x4 ());
			mat.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			Vector3 temp = ScaledSpace.ScaledToLocalSpace (scaledSpaceCamera.transform.position);
			mat.SetVector (ShaderProperties._Globals_WorldCameraPos_PROPERTY, temp);
			
			
			#if skyScaledBox
			if (scaledMode)
				mat.SetVector (ShaderProperties._Globals_Origin_PROPERTY, Vector3.Scale(ScaledSpace.LocalToScaledSpace(parentCelestialBody.transform.position),new Vector3(6000f,6000f,6000f)));
			else
				mat.SetVector (ShaderProperties._Globals_Origin_PROPERTY, parentCelestialBody.transform.position);
			#else
			mat.SetVector (ShaderProperties._Globals_Origin_PROPERTY, parentCelestialBody.transform.position);
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
		}
		
		public void InitPostprocessMaterialGlobal ()
		{
			
			Shader.SetGlobalTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			
			Shader.SetGlobalTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			
			Shader.SetGlobalTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
			
			//			if (Core.Instance.render24bitDepthBuffer && !Core.Instance.d3d9)
			Shader.SetGlobalTexture (ShaderProperties._customDepthTexture_PROPERTY, Core.Instance.customDepthBufferTexture);
			
			if (Core.Instance.useGodrays)
				Shader.SetGlobalTexture (ShaderProperties._godrayDepthTexture_PROPERTY, Core.Instance.godrayDepthTexture);
			
			//Consts, best leave these alone
			Shader.SetGlobalFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			
			Shader.SetGlobalFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rl_PROPERTY, RL * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.RES_R_PROPERTY, RES_R);
			Shader.SetGlobalFloat (ShaderProperties.RES_MU_PROPERTY, RES_MU);
			Shader.SetGlobalFloat (ShaderProperties.RES_MU_S_PROPERTY, RES_MU_S);
			Shader.SetGlobalFloat (ShaderProperties.RES_NU_PROPERTY, RES_NU);
			Shader.SetGlobalFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			Shader.SetGlobalFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			
			Shader.SetGlobalVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f);
			Shader.SetGlobalFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			Shader.SetGlobalVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f);
			Shader.SetGlobalVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f) / 0.9f);
			
			Shader.SetGlobalFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f);
			Shader.SetGlobalFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f);
			
			Shader.SetGlobalVector (ShaderProperties._camPos_PROPERTY, farCamera.transform.position-parentCelestialBody.transform.position);  //better do this small calculation here
			Shader.SetGlobalVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun ().normalized);
		}
		
		
		public void UpdatePostProcessMaterial (Material mat)
		{
			
			//			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			//			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			//			mat.SetFloat (ShaderProperties.Rl_PROPERTY, RL * atmosphereGlobalScale);
			//
			//			//mat.SetFloat (ShaderProperties.atmosphereGlobalScale_PROPERTY, atmosphereGlobalScale);
			//
			//			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			////			mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, viewdirOffset);
			//		
			//			mat.SetFloat (ShaderProperties._global_alpha_PROPERTY, postProcessingAlpha);
			//			mat.SetFloat (ShaderProperties._Exposure_PROPERTY, postProcessExposure);
			//			mat.SetFloat (ShaderProperties._global_depth_PROPERTY, postProcessDepth*1000000);
			//
			//			mat.SetFloat (ShaderProperties._Post_Extinction_Tint_PROPERTY, _Post_Extinction_Tint);
			//			mat.SetFloat (ShaderProperties.postExtinctionMultiplier_PROPERTY, postExtinctionMultiplier);
			//
			//
			//			mat.SetFloat (ShaderProperties._openglThreshold_PROPERTY, openglThreshold);
			////			mat.SetFloat (ShaderProperties._edgeThreshold_PROPERTY, edgeThreshold);
			//			
			//
			//			mat.SetFloat(ShaderProperties._Scale_PROPERTY, 1);
			//
			//			mat.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, farCamera.worldToCameraMatrix.inverse);
			//			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun ().normalized);
			//			mat.SetFloat (ShaderProperties.SUN_INTENSITY_PROPERTY, sunIntensity);
			//			
			//			
			//			Matrix4x4 ctol1 = farCamera.cameraToWorldMatrix;
			//			Vector3d tmp = (farCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			//			
			//			Matrix4x4d viewMat = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
			//			                                    ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
			//			                                    ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
			//			                                    ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);
			//
			//			viewMat = viewMat.Inverse ();
			//			Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);
			//			
			//			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4 ());
			//			mat.SetMatrix (ShaderProperties._ViewProjInv_PROPERTY, viewProjMat.inverse);
			//			
			//			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			//
			////			if (currentPQSMod_CelestialBodyTransform)
			////			{
			////				float fadeStart = currentPQSMod_CelestialBodyTransform.planetFade.fadeStart;
			////				float fadeEnd = currentPQSMod_CelestialBodyTransform.planetFade.fadeEnd;
			////				mat.SetFloat (ShaderProperties._fade_PROPERTY, Mathf.Lerp (1f, 0f, (trueAlt - fadeStart) / (fadeEnd - fadeStart)));
			////			}
			//
			//			mat.SetVector (ShaderProperties._camPos_PROPERTY, farCamera.transform.position-parentCelestialBody.transform.position);  //better do this small calculation here
			//
			////			if (Core.Instance.useEclipses)
			////			{
			////				mat.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
			////				mat.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
			////				mat.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
			////				                                               sunPosRelPlanet.z, (float)m_manager.sunCelestialBody.Radius));
			////			}
			
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
			
			mat.SetMatrix ("_Globals_CameraToWorld", farCamera.worldToCameraMatrix.inverse);
			mat.SetVector ("SUN_DIR", m_manager.getDirectionToSun ().normalized);
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
			mat.SetVector ("_camPos", farCamera.transform.position-parentCelestialBody.transform.position);  //better do this small calculation here
			
			if (Core.Instance.usePlanetShine)
			{
				mat.SetMatrix ("planetShineSources", planetShineSourcesMatrix);
				mat.SetMatrix ("planetShineRGB", planetShineRGBMatrix);
			}
		}
		
		public void UpdatePostProcessMaterialGlobal ()
		{
			
			Shader.SetGlobalFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rl_PROPERTY, RL * atmosphereGlobalScale);
			
			//Shader.SetGlobalFloat (ShaderPropertiesatmosphereGlobalScale_PROPERTY, atmosphereGlobalScale);
			
			Shader.SetGlobalFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			//			Shader.SetGlobalFloat (ShaderProperties._viewdirOffset_PROPERTY, viewdirOffset);
			
			Shader.SetGlobalFloat (ShaderProperties._global_alpha_PROPERTY, interpolatedSettings.postProcessAlpha);
			Shader.SetGlobalFloat (ShaderProperties._Exposure_PROPERTY, interpolatedSettings.postProcessExposure);
			Shader.SetGlobalFloat (ShaderProperties._global_depth_PROPERTY, interpolatedSettings.postProcessDepth*1000000);
			
			Shader.SetGlobalFloat (ShaderProperties._Post_Extinction_Tint_PROPERTY, interpolatedSettings._Post_Extinction_Tint);
			Shader.SetGlobalFloat (ShaderProperties.postExtinctionMultiplier_PROPERTY, interpolatedSettings.postExtinctionMultiplier);
			
			
			Shader.SetGlobalFloat (ShaderProperties._openglThreshold_PROPERTY, interpolatedSettings.openglThreshold);
			//			Shader.SetGlobalFloat (ShaderProperties._edgeThreshold_PROPERTY, edgeThreshold);
			
			
			Shader.SetGlobalFloat (ShaderProperties._Scale_PROPERTY, 1);
			
			Shader.SetGlobalMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, farCamera.worldToCameraMatrix.inverse);
			Shader.SetGlobalVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun ().normalized);
			Shader.SetGlobalFloat (ShaderProperties.SUN_INTENSITY_PROPERTY, sunIntensity);
			
			
			Matrix4x4 ctol1 = farCamera.cameraToWorldMatrix;
			Vector3d tmp = (farCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			
			Matrix4x4d viewMat = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
			                                     ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
			                                     ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
			                                     ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);
			
			viewMat = viewMat.Inverse ();
			Matrix4x4 projMat = GL.GetGPUProjectionMatrix (farCamera.projectionMatrix, false);
			
			Matrix4x4 viewProjMat = (projMat * viewMat.ToMatrix4x4 ());
			Shader.SetGlobalMatrix (ShaderProperties._ViewProjInv_PROPERTY, viewProjMat.inverse);
			
			Shader.SetGlobalFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			//			if (currentPQSMod_CelestialBodyTransform)
			//			{
			//				float fadeStart = currentPQSMod_CelestialBodyTransform.planetFade.fadeStart;
			//				float fadeEnd = currentPQSMod_CelestialBodyTransform.planetFade.fadeEnd;
			//				Shader.SetGlobalFloat (ShaderProperties._fade_PROPERTY, Mathf.Lerp (1f, 0f, (trueAlt - fadeStart) / (fadeEnd - fadeStart)));
			//			}
			
			Shader.SetGlobalVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);
			
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
		}
		
		public void InitUniformsGlobal ()
		{
			Shader.SetGlobalFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			Shader.SetGlobalFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			Shader.SetGlobalVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f);
			
			Shader.SetGlobalTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			Shader.SetGlobalTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			Shader.SetGlobalTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
			Shader.SetGlobalFloat (ShaderProperties.scale_PROPERTY, Rg * 1 / m_radius);
			Shader.SetGlobalFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			Shader.SetGlobalFloat (ShaderProperties.TRANSMITTANCE_W_PROPERTY, TRANSMITTANCE_W);
			Shader.SetGlobalFloat (ShaderProperties.TRANSMITTANCE_H_PROPERTY, TRANSMITTANCE_H);
			Shader.SetGlobalFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			Shader.SetGlobalFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			Shader.SetGlobalFloat (ShaderProperties.RES_R_PROPERTY, RES_R);
			Shader.SetGlobalFloat (ShaderProperties.RES_MU_PROPERTY, RES_MU);
			Shader.SetGlobalFloat (ShaderProperties.RES_MU_S_PROPERTY, RES_MU_S);
			Shader.SetGlobalFloat (ShaderProperties.RES_NU_PROPERTY, RES_NU);
			Shader.SetGlobalFloat (ShaderProperties.AVERAGE_GROUND_REFLECTANCE_PROPERTY, AVERAGE_GROUND_REFLECTANCE);
			Shader.SetGlobalFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f);
			Shader.SetGlobalFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f);
			Shader.SetGlobalVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f);
			Shader.SetGlobalVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f) / 0.9f);
			//			Shader.SetGlobalFloat (ShaderProperties._sunglareScale_PROPERTY, sunglareScale);	
		}
		
		public void SetUniformsGlobal ()
		{
			
			Shader.SetGlobalFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			if (!MapView.MapIsEnabled)
			{
				Shader.SetGlobalFloat (ShaderProperties._viewdirOffset_PROPERTY, interpolatedSettings.viewdirOffset);
			}
			else
			{
				Shader.SetGlobalFloat (ShaderProperties._viewdirOffset_PROPERTY, 0f);
			}
			
			Shader.SetGlobalFloat (ShaderProperties.extinctionGroundFade_PROPERTY, interpolatedSettings.skyextinctionGroundFade);
			
			if (!MapView.MapIsEnabled)
			{
				Shader.SetGlobalFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
				Shader.SetGlobalFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
				Shader.SetGlobalFloat (ShaderProperties.extinctionMultiplier_PROPERTY, interpolatedSettings.skyExtinctionMultiplier);
				Shader.SetGlobalFloat (ShaderProperties.extinctionRimFade_PROPERTY, interpolatedSettings.skyextinctionRimFade);
				Shader.SetGlobalFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, interpolatedSettings._extinctionScatterIntensity);
			}
			else
			{
				Shader.SetGlobalFloat (ShaderProperties._Alpha_Global_PROPERTY, mapAlphaGlobal);
				Shader.SetGlobalFloat (ShaderProperties._Extinction_Tint_PROPERTY, mapExtinctionTint);
				Shader.SetGlobalFloat (ShaderProperties.extinctionMultiplier_PROPERTY, mapExtinctionMultiplier);
				Shader.SetGlobalFloat (ShaderProperties.extinctionRimFade_PROPERTY, mapSkyExtinctionRimFade);
				Shader.SetGlobalFloat (ShaderProperties._extinctionScatterIntensity_PROPERTY, _mapExtinctionScatterIntensity);
			}
			
			
			Shader.SetGlobalFloat (ShaderProperties.scale_PROPERTY, 1);
			Shader.SetGlobalFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			Shader.SetGlobalFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			
			//used to determine the view ray direction in the sky shader
			if (!MapView.MapIsEnabled)
			{				
				Shader.SetGlobalMatrix (ShaderProperties._Globals_WorldToCamera_PROPERTY, farCamera.worldToCameraMatrix);
				Shader.SetGlobalMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, farCamera.worldToCameraMatrix.inverse);
			}
			else
			{
				Shader.SetGlobalMatrix (ShaderProperties._Globals_WorldToCamera_PROPERTY, scaledSpaceCamera.worldToCameraMatrix);
				Shader.SetGlobalMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, scaledSpaceCamera.worldToCameraMatrix.inverse);
			}
			
			
			
			Shader.SetGlobalFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			Shader.SetGlobalFloat (ShaderProperties._Sun_Intensity_PROPERTY, 100f);
			
			//			Shader.SetGlobalFloat (ShaderProperties._sunglareScale_PROPERTY, sunglareScale);
			
			Shader.SetGlobalVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);
			
			Shader.SetGlobalVector (ShaderProperties._Godray_WorldSunDir_PROPERTY, m_manager.sunCelestialBody.transform.position
			                        - parentCelestialBody.transform.position);
			
			
			if (!MapView.MapIsEnabled) {
				p = farCamera.projectionMatrix;
			} else {
				p = scaledSpaceCamera.projectionMatrix;
			}
			
			
			m_cameraToScreenMatrix = new Matrix4x4d (p);
			Shader.SetGlobalMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, m_cameraToScreenMatrix.ToMatrix4x4 ());
			Shader.SetGlobalMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, m_cameraToScreenMatrix.Inverse ().ToMatrix4x4 ());
			
			Vector3 temp = ScaledSpace.ScaledToLocalSpace (scaledSpaceCamera.transform.position);
			Shader.SetGlobalVector (ShaderProperties._Globals_WorldCameraPos_PROPERTY, temp);
			
			#if skyScaledBox
			if (scaledMode)
				Shader.SetGlobalVector (ShaderProperties._Globals_Origin_PROPERTY, Vector3.Scale(ScaledSpace.LocalToScaledSpace(parentCelestialBody.transform.position),new Vector3(6000f,6000f,6000f)));
			else
				Shader.SetGlobalVector (ShaderProperties._Globals_Origin_PROPERTY, parentCelestialBody.transform.position);
			#else
			Shader.SetGlobalVector (ShaderProperties._Globals_Origin_PROPERTY, parentCelestialBody.transform.position);
			#endif
			
			
			if (!MapView.MapIsEnabled) {
				Shader.SetGlobalFloat (ShaderProperties._Exposure_PROPERTY, interpolatedSettings.skyExposure);
				Shader.SetGlobalFloat (ShaderProperties._RimExposure_PROPERTY, interpolatedSettings.skyRimExposure);
			} else {
				Shader.SetGlobalFloat (ShaderProperties._Exposure_PROPERTY, mapExposure);
				Shader.SetGlobalFloat (ShaderProperties._RimExposure_PROPERTY, mapSkyRimExposure);
			}
			
			
			if (Core.Instance.useEclipses)
			{
				float scaleFactor=ScaledSpace.ScaleFactor;
				
				Vector3 sunPosRelPlanet=Vector3.zero;
				if (scaledMode)
					sunPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
				else
					sunPosRelPlanet = m_manager.sunCelestialBody.transform.position;
				Shader.SetGlobalVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                                sunPosRelPlanet.z, (float)m_manager.sunCelestialBody.Radius));
				
				
				
				//build and set casters matrix
				castersMatrix1 = Matrix4x4.zero;
				castersMatrix2 = Matrix4x4.zero;
				
				
				//				Debug.Log("scalefactor "+scaleFactor.ToString());
				
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
				
				Shader.SetGlobalMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				Shader.SetGlobalMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
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
			
			
			
			//			UnityEngine.Object.Destroy (black);
			//			UnityEngine.Object.Destroy (sunGlare);
			
			Component.Destroy (updater);
			UnityEngine.Object.Destroy (updater);
			
			//			Component.Destroy (skyMR);
			//			UnityEngine.Object.Destroy (skyObject);
			
			Component.Destroy (atmosphereMeshrenderer);
			UnityEngine.Object.Destroy (atmosphereMesh);
			
			Component.Destroy (skyScaledMeshrenderer);
			UnityEngine.Object.Destroy (skyScaledMesh);
			
			Component.Destroy (skyLocalMeshrenderer);
			UnityEngine.Object.Destroy (skyLocalMesh);
			
			RestoreStockAtmosphere ();
			UnityEngine.Object.Destroy (alteredMaterial);
			UnityEngine.Object.Destroy (originalMaterial);
		}
		
		void toggleScaledMode()
		{
			if (scaledMode) //switch to localMode
			{
				skyScaledMeshrenderer.enabled = false;
				skyLocalMeshrenderer.enabled=true;
				
				//				updater.settings (m_mesh, m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
				updater.settings (m_skyMaterialLocal, m_manager, this,parentCelestialBody.transform);
				
				scaledMode=false;
				Debug.Log("[Scatterer] Sky switched to local mode");
			}
			else   //switch to scaledMode
			{
				skyScaledMeshrenderer.enabled = true;
				skyLocalMeshrenderer.enabled=false;
				
				//				updater.settings (m_mesh, m_skyMaterialScaled, m_manager, this,parentCelestialBody.transform);
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
					Debug.Log("[Scatterer] config found for: "+parentCelestialBody.name);
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

			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;

			Debug.Log (Core.Instance.EVECloudObjects.Count.ToString ());
			Debug.Log (Core.Instance.EVECloudObjects[parentCelestialBody.name].Count.ToString ());
			Debug.Log (parentCelestialBody.name);
			List<object> cloudObjs = Core.Instance.EVECloudObjects[parentCelestialBody.name];

			try
			{
				foreach (object _obj in cloudObjs)
				{
					object cloudsPQS = _obj.GetType().GetField("cloudsPQS", flags).GetValue(_obj) as object;
					object layerVolume = cloudsPQS.GetType().GetField("layerVolume", flags).GetValue(cloudsPQS) as object;
					Material ParticleMaterial = layerVolume.GetType().GetField("ParticleMaterial", flags).GetValue(layerVolume) as Material;
					EVEvolumetrics.Add (ParticleMaterial);
				}
			}
			catch (Exception)
			{
				Debug.Log("[Scatterer] Null volumetric clouds on planet: "+parentCelestialBody.name);
				return;
			}
			
			Debug.Log("[Scatterer] Detected "+EVEvolumetrics.Count+" EVE volumetric layers for planet: "+parentCelestialBody.name);
		}
	}
}