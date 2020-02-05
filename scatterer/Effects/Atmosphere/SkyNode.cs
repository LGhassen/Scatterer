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
		protected string name;
		public UrlDir.UrlConfig configUrl;
		
		public GameObject atmosphereMesh;
		public MeshRenderer atmosphereMeshrenderer;
		
		SimpleRenderingShape SkySphere;
		GameObject skySphereGameObject;
		MeshRenderer skySphereMeshRenderer, stockSkyMeshRenderer;

		public bool usesCloudIntegration = true;
		
		Matrix4x4 castersMatrix1=Matrix4x4.zero;
		Matrix4x4 castersMatrix2=Matrix4x4.zero;
		
		public Matrix4x4 planetShineSourcesMatrix=Matrix4x4.zero;
		public Matrix4x4 planetShineRGBMatrix=Matrix4x4.zero;
		
		Vector3 sunPosRelPlanet=Vector3.zero;
		
		public float postDist = -4500f;
		public float percentage;
		public int currentConfigPoint;
		
		EncodeFloat encode;
		
		[Persistent]
		public float experimentalAtmoScale=1f;
		
		public float oceanSigma = 0.04156494f;
		public float _Ocean_Threshold = 25f;

		Vector3 sunViewPortPos=Vector3.zero;

		float alt;
		public float trueAlt;
		
		string celestialBodyName;
		Transform parentScaledTransform, parentLocalTransform;

		GameObject ringObject;
		float ringInnerRadius, ringOuterRadius;
		Texture2D ringTexture;

		bool hasRingObjectAndShadowActivated = false;
				
		//atmosphere properties
		public ConfigPoint interpolatedSettings= new ConfigPoint();

		[Persistent]
		public float cloudColorMultiplier=3f;
		[Persistent]
		public float cloudScatteringMultiplier=0.2f;
		[Persistent]
		public float cloudSkyIrradianceMultiplier=0.05f;
		
		[Persistent]
		public float volumetricsColorMultiplier = 1f;
		
		public bool inScaledSpace = true;

		bool skyNodeInitiated = false;

		public List<Material> EVEvolumetrics = new List<Material>();
		bool mapVolumetrics=false;
		int waitCounter=0;
		
		public bool postprocessingEnabled = true;
		
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

		//Half heights for the atmosphere air density (HR) and particle density (HM)
		//This is the height in km that half the particles are found below
		[Persistent]
		float HR = 8.0f;
		[Persistent]
		float HM = 1.2f;
		//scatter coefficient for mie
		[Persistent]
		Vector3 BETA_MSca = new Vector3 (4e-3f, 4e-3f, 4e-3f);
		public Material localScatteringMaterial,m_skyMaterial,scaledScatteringMaterial,sunflareExtinctionMaterial;
		public AtmosphereProjector localScatteringProjector;

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
		public List < ConfigPoint > configPoints = new List < ConfigPoint > ();
		//public string assetDir;

		[Persistent]
		public string assetPath;
		
		public void Init ()
		{
			m_radius = (float) m_manager.GetRadius ();
			Rt = Rt * (m_radius / Rg);
			RL = RL * (m_radius / Rg);
			Rg = m_radius;
			
			//Inscatter is responsible for the change in the sky color as the sun moves
			//The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture
			//and the shader manually performs the sample for the 4th dimension
			//To get scatterer running in dx9, the texture was packed into a 2D texture
			m_inscatter = new Texture2D (RES_MU_S * RES_NU, RES_MU * RES_R, TextureFormat.RGBAHalf,false);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			//Transmittance is responsible for the change in the sun color as it moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_transmit = new Texture2D (TRANSMITTANCE_W, TRANSMITTANCE_H, TextureFormat.RGBAHalf,false);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;

			
			//Irradiance is responsible for the change in the sky color as the sun moves
			//The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_irradiance = new Texture2D (SKY_W, SKY_H, TextureFormat.RGBAHalf,false);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;
						
			loadPrecomputedTables ();

			m_skyMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/SkySphere")]);
			scaledScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/ScaledPlanetScattering")]);
			localScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);

			m_skyMaterial.SetOverrideTag ("IgnoreProjector", "True");
			scaledScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");
			localScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");
			
//			if (Core.Instance.useGodrays)
//			{
//				localScatteringMaterial.EnableKeyword("GODRAYS_ON");
//				localScatteringMaterial.DisableKeyword("GODRAYS_OFF");
//			}
//			else
//			{
				localScatteringMaterial.DisableKeyword("GODRAYS_ON");
				localScatteringMaterial.EnableKeyword("GODRAYS_OFF");
//			}
			if (Core.Instance.mainSettings.useEclipses)
			{
				localScatteringMaterial.EnableKeyword ("ECLIPSES_ON");
				localScatteringMaterial.DisableKeyword ("ECLIPSES_OFF");
			}
			else
			{
				localScatteringMaterial.DisableKeyword ("ECLIPSES_ON");
				localScatteringMaterial.EnableKeyword ("ECLIPSES_OFF");
			}
			if (m_manager.hasOcean)
			{
				localScatteringMaterial.EnableKeyword ("DISABLE_UNDERWATER_ON");
				localScatteringMaterial.DisableKeyword ("DISABLE_UNDERWATER_OFF");
			}
			else
			{
				localScatteringMaterial.DisableKeyword ("DISABLE_UNDERWATER_ON");
				localScatteringMaterial.EnableKeyword ("DISABLE_UNDERWATER_OFF");
			}

			InitPostprocessMaterial (localScatteringMaterial);

			if (!ReferenceEquals (m_manager.parentCelestialBody.pqsController, null))
			{
				m_manager.parentCelestialBody.pqsController.isActive = false; 	//sometimes the PQS is forgotten as "active" if a ship is loaded directly around another body, this would mess with the mod
													//this sets it to false, if it's really active it will be set to active automatically. EVE mod seems also to have a fix for this
			}

			atmosphereMesh = new GameObject ();

			localScatteringProjector = new AtmosphereProjector(localScatteringMaterial,parentLocalTransform,Rt);

			float skySphereSize = 2*(4 * (Rt-Rg) + Rg) / ScaledSpace.ScaleFactor;
			SkySphere = new SimpleRenderingShape (skySphereSize, m_skyMaterial,true);
			skySphereGameObject = SkySphere.GameObject;

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				skySphereGameObject.layer = 15;
			else
				skySphereGameObject.layer = 9;

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				SkySphereKSCUpdater updater = (SkySphereKSCUpdater) skySphereGameObject.AddComponent(typeof(SkySphereKSCUpdater));
				updater.parentLocalTransform = parentLocalTransform;
			}
			else
			{
				skySphereGameObject.transform.position = parentScaledTransform.position;
				skySphereGameObject.transform.parent = parentScaledTransform;
			}

			skySphereMeshRenderer = SkySphere.GameObject.GetComponent < MeshRenderer > ();
			skySphereMeshRenderer.material = m_skyMaterial;

			m_skyMaterial.renderQueue=2999; //this lets modified EVE clouds draw over sky
			scaledScatteringMaterial.renderQueue=2999;

			skySphereMeshRenderer.enabled = true;

			if (Core.Instance.mainSettings.useRingShadows)
			{
				ringObject = GameObject.Find (celestialBodyName + "Ring");
				if (ringObject)
				{
					
					Utils.LogDebug (" Found ring for " + celestialBodyName);

					Material ringMat = ringObject.GetComponent < MeshRenderer > ().material;

					hasRingObjectAndShadowActivated = true;
					
					MonoBehaviour[] scripts = (MonoBehaviour[]) ringObject.GetComponents<MonoBehaviour>();
					
					foreach( MonoBehaviour _script in scripts)
					{						
						if (_script.GetType().ToString().Contains("Ring")) //production-quality code
						{
							const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
								BindingFlags.Instance | BindingFlags.Static;

							FieldInfo[] fields = _script.GetType().GetFields(flags);

							foreach(FieldInfo fi in fields)
							{
								//Utils.Log("fi.Name "+fi.Name+" fi.GetType() "+fi.GetType());
							}
							
							try
							{
								ringTexture = _script.GetType().GetField("texture", flags).GetValue(_script) as Texture2D;
								Utils.LogDebug(" ring texture fetch successful");
								Utils.LogDebug(" ringTexture.width "+ringTexture.width.ToString());
								Utils.LogDebug(" ringTexture.height "+ringTexture.height.ToString());

								MeshRenderer ringMR = _script.GetType().GetField("ringMR", flags).GetValue(_script) as MeshRenderer;
								Utils.LogDebug(" ring MeshRenderer fetch successful");

								ringInnerRadius = ringMR.material.GetFloat("innerRadius");
								ringOuterRadius = ringMR.material.GetFloat("outerRadius");

								Utils.LogDebug (" ring innerRadius (with parent scale) " + ringInnerRadius.ToString ());
								Utils.LogDebug (" ring outerRadius (with parent scale) " + ringOuterRadius.ToString ());

								ringInnerRadius *= 6000; //*6000 to convert to local space size
								ringOuterRadius *= 6000;
							}
							catch (Exception e)
							{
								Utils.LogDebug(" Kopernicus ring exception "+e.ToString());
								Utils.LogDebug(" Disabling ring shadows for "+celestialBodyName);
								hasRingObjectAndShadowActivated=false;
							}
						}
					}
				}
			}

			InitUniforms (m_skyMaterial);
			InitUniforms (scaledScatteringMaterial);

			if (Core.Instance.mainSettings.fullLensFlareReplacement)
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

				if (m_manager.hasOcean)
				{
					sunflareExtinctionMaterial.EnableKeyword ("DISABLE_UNDERWATER_ON");
					sunflareExtinctionMaterial.DisableKeyword ("DISABLE_UNDERWATER_OFF");
				}
				else
				{
					sunflareExtinctionMaterial.DisableKeyword ("DISABLE_UNDERWATER_ON");
					sunflareExtinctionMaterial.EnableKeyword ("DISABLE_UNDERWATER_OFF");
				}
			}
		}
		
		public void OnPreRender() //changed from onPrecull so sunlightModulator can be called before it on camera PreCull, seems to not break anything
		{
			UpdateStuff ();	
			SetUniforms (m_skyMaterial);
			SetUniforms (scaledScatteringMaterial);

			if (!MapView.MapIsEnabled && Core.Instance.mainSettings.sunlightExtinction)
			{
				Vector3 extinctionPosition = (FlightGlobals.ActiveVessel ? FlightGlobals.ActiveVessel.transform.position : Core.Instance.farCamera.transform.position)- parentLocalTransform.position;

				float lerpedScale = Mathf.Lerp(1f,experimentalAtmoScale,(extinctionPosition.magnitude-m_radius)/2000f); //hack but keeps the extinction beautiful at sea level, and matches the clouds when you get higher

				Core.Instance.sunlightModulatorInstance.modulateByColor(
					AtmosphereUtils.getExtinction(extinctionPosition, m_manager.getDirectionToSun ().normalized,
				                              Rt, Rg, m_transmit,lerpedScale));
			}
		}

		public void UpdateStuff () //to be called by onPrerender for some graphical stuff
		{
			if (!inScaledSpace)
			{
				if (!MapView.MapIsEnabled) {
					if (postprocessingEnabled) {
						UpdatePostProcessMaterial (localScatteringProjector.projector.material);
					}
				}
			}
			if (Core.Instance.mainSettings.useEclipses)
			{
				float scaleFactor=ScaledSpace.ScaleFactor;
				
				sunPosRelPlanet=Vector3.zero;
				sunPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.sunCelestialBody.transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
				
				//build eclipse casters matrix
				castersMatrix1 = Matrix4x4.zero;
				castersMatrix2 = Matrix4x4.zero;
				Vector3 casterPosRelPlanet;
				for (int i=0; i< Mathf.Min(4, m_manager.eclipseCasters.Count); i++)
				{
					casterPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.eclipseCasters [i].transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
					
					castersMatrix1.SetRow (i, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y,
					                                       casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
				}
				
				for (int i=4; i< Mathf.Min(8, m_manager.eclipseCasters.Count); i++)
				{
					casterPosRelPlanet = Vector3.Scale(ScaledSpace.LocalToScaledSpace(m_manager.eclipseCasters [i].transform.position),new Vector3(scaleFactor, scaleFactor,scaleFactor));
					castersMatrix2.SetRow (i - 4, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y,
					                                           casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
				}
			}
			if (Core.Instance.mainSettings.usePlanetShine)
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

					sourcePosRelPlanet = Vector3.Scale(offsetPos - m_manager.parentCelestialBody.GetTransform().position,new Vector3d(6000f,6000f,6000f));
					
					planetShineSourcesMatrix.SetRow (i, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y,
					                                                 sourcePosRelPlanet.z, m_manager.planetshineSources[i].isSun? 1.0f:0.0f ));
					
					float intensity = m_manager.planetshineSources[i].intensity;
					
					planetShineRGBMatrix.SetRow (i, new Vector4(m_manager.planetshineSources[i].color.x,m_manager.planetshineSources[i].color.y,
					                                            m_manager.planetshineSources[i].color.z,intensity));
				}
			}
			//update EVE cloud shaders
			//maybe refactor?
			if (Core.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				int size;
				
				//2d clouds
				if(Core.Instance.EVEClouds.ContainsKey(celestialBodyName))
				{
					size = Core.Instance.EVEClouds[celestialBodyName].Count;
					for (int i=0;i<size;i++)
					{
						SetUniforms(Core.Instance.EVEClouds[celestialBodyName][i]);
						
						//if (!inScaledSpace)
						UpdatePostProcessMaterial(Core.Instance.EVEClouds[celestialBodyName][i]);
						
						Core.Instance.EVEClouds[celestialBodyName][i].SetFloat
							("cloudColorMultiplier", cloudColorMultiplier);
						Core.Instance.EVEClouds[celestialBodyName][i].SetFloat
							("cloudScatteringMultiplier", cloudScatteringMultiplier);
						Core.Instance.EVEClouds[celestialBodyName][i].SetFloat
							("cloudSkyIrradianceMultiplier", cloudSkyIrradianceMultiplier);
						
						
						Core.Instance.EVEClouds[celestialBodyName][i].EnableKeyword ("SCATTERER_ON");
						Core.Instance.EVEClouds[celestialBodyName][i].DisableKeyword ("SCATTERER_OFF");
					}
				}
				
				//volumetrics
				//if in local mode and mapping is done
				if (!inScaledSpace && !mapVolumetrics)
				{
					size = EVEvolumetrics.Count;
					
					for (int i=0;i<size;i++)
					{
						//TODO: simplify, take one or the other
						SetUniforms(EVEvolumetrics[i]);
						UpdatePostProcessMaterial(EVEvolumetrics[i]);
						
						EVEvolumetrics[i].SetVector
							("_PlanetWorldPos", parentLocalTransform.position);
						
						EVEvolumetrics[i].SetFloat
							("cloudColorMultiplier", volumetricsColorMultiplier); //doesn't need to be done very frame
					}
				}
			}
			//update extinction for sunflares
			if (Core.Instance.mainSettings.fullLensFlareReplacement)
			{
				foreach (SunFlare customSunFlare in Core.Instance.customSunFlares)
				{
					//render extinction to texture
					sunflareExtinctionMaterial.SetVector ("_Sun_WorldSunDir", m_manager.getDirectionToCelestialBody (customSunFlare.source).normalized);
					sunflareExtinctionMaterial.SetFloat("_experimentalAtmoScale",experimentalAtmoScale);

					if (!MapView.MapIsEnabled)
						sunflareExtinctionMaterial.SetVector ("_Globals_WorldCameraPos", Core.Instance.farCamera.transform.position - parentLocalTransform.position);
					else
						sunflareExtinctionMaterial.SetVector ("_Globals_WorldCameraPos", (Vector3) ScaledSpace.ScaledToLocalSpace(Core.Instance.scaledSpaceCamera.transform.position) - parentLocalTransform.position);

					Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 0); //pass 0 for sunflare extinction

					if (hasRingObjectAndShadowActivated)
					{
						sunflareExtinctionMaterial.SetVector("ringNormal", ringObject.transform.up);
						Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 1); //pass 1 for ring extinction
					}
				}
			}

			scaledScatteringMaterial.SetFloat ("renderScattering", inScaledSpace ? 1f : 0f); //not sure this is a good way to do it
			localScatteringProjector.setInScaledSpace(inScaledSpace);
			localScatteringProjector.updateProjector ();
		}
		
		
		public void UpdateNode ()
		{
			if ((m_manager.parentCelestialBody.pqsController != null) && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				bool prevState = inScaledSpace;
				inScaledSpace = !(m_manager.parentCelestialBody.pqsController.isActive);
				//if we go from scaled to local space
				if (!inScaledSpace && prevState)
				{
					//set flag to map EVE volumetrics after a few frames
					if (Core.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
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
					mapEVEVolumetrics(); //do this last so if it fails we just ignore it
				}
			}

			if (!skyNodeInitiated)
			{
				m_radius = (float) m_manager.GetRadius ();
				
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;

				tweakStockAtmosphere();
				addScaledScatteringMaterialToPlanet();

				//disable postprocessing and ocean effects for Texture Replacer reflections
				DisableEffectsChecker effectsDisabler = atmosphereMesh.AddComponent<DisableEffectsChecker>();
				effectsDisabler.manager = m_manager;

				//after the shader has been replaced by the modified scatterer shader, the properties are lost and need to be set again
				//call EVE clouds2D.reassign() method to set the shader properties
				if (Core.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
				{
					initiateEVEClouds();
				}

				skyNodeInitiated = true;
				Utils.LogDebug(" Skynode initiated for "+celestialBodyName);
			}
			else
			{
				if(!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
				{
					alt = Vector3.Distance (Core.Instance.farCamera.transform.position, parentLocalTransform.position);
					
					trueAlt = alt - m_radius;
				}
				interpolateVariables ();

				if (m_manager.hasOcean && !Core.Instance.mainSettings.useOceanShaders)
				{
					skySphereMeshRenderer.enabled = (trueAlt>=0f);
					stockSkyMeshRenderer.enabled = (trueAlt<0f); //re-enable stock sky meshrenderer, for compatibility with stock underwater effect
				}
			}
		}
		
		public void SetUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, interpolatedSettings.viewdirOffset);
			mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
			mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
			mat.SetFloat ("extinctionTint", interpolatedSettings.extinctionTint); //extinctionTint for scaled+local
			mat.SetFloat ("extinctionThickness", interpolatedSettings.extinctionThickness);

			mat.SetFloat (ShaderProperties.scale_PROPERTY, 1);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);

			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);

			Vector3 temp = ScaledSpace.ScaledToLocalSpace (Core.Instance.scaledSpaceCamera.transform.position);

			mat.SetFloat ("_SkyExposure", interpolatedSettings.skyExposure);
			mat.SetFloat ("_ScatteringExposure", interpolatedSettings.scatteringExposure);

			if (Core.Instance.mainSettings.useEclipses)
			{
				mat.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				mat.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
				mat.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                       sunPosRelPlanet.z, (float)m_manager.sunCelestialBody.Radius));
			}
			if (Core.Instance.mainSettings.usePlanetShine)
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

			mat.SetFloat ("_ScatteringExposure", interpolatedSettings.scatteringExposure);

			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			
			mat.SetFloat (ShaderProperties.scale_PROPERTY, 1);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ().normalized);

			mat.SetVector("_camForward", Core.Instance.farCamera.transform.forward);

			UpdatePostProcessMaterial (mat);
		}
		

		public void InitPostprocessMaterial (Material mat)
		{
			mat.SetFloat ("mieG", Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);

			if (Core.Instance.bufferRenderingManager && (HighLogic.LoadedScene != GameScenes.TRACKSTATION) )
			{
				mat.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Core.Instance.bufferRenderingManager.depthTexture);
			}
			
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

			if (Core.Instance.mainSettings.usePlanetShine)
			{
				mat.EnableKeyword ("PLANETSHINE_ON");
				mat.DisableKeyword ("PLANETSHINE_OFF");	
			}
			else
			{
				mat.DisableKeyword ("PLANETSHINE_ON");
				mat.EnableKeyword ("PLANETSHINE_OFF");
			}

			if (m_manager.flatScaledSpaceModel && m_manager.parentCelestialBody.pqsController)
				mat.SetFloat ("_PlanetOpacity", 0f);
			else
				mat.SetFloat ("_PlanetOpacity", 1f);

			mat.SetVector ("_sunColor", m_manager.sunColor);

			float camerasOverlap = Core.Instance.nearCamera.farClipPlane - Core.Instance.farCamera.nearClipPlane;
			Utils.LogDebug(" Camera overlap: "+camerasOverlap.ToString());
			mat.SetFloat("_ScattererCameraOverlap",camerasOverlap);
		}

		
		public void UpdatePostProcessMaterial (Material mat)
		{
			mat.SetFloat ("Rg", Rg * atmosphereGlobalScale);
			mat.SetFloat ("Rt", Rt * atmosphereGlobalScale);
			mat.SetFloat ("Rl", RL * atmosphereGlobalScale);

			mat.SetFloat ("_experimentalAtmoScale", experimentalAtmoScale);

			mat.SetFloat ("_global_alpha", interpolatedSettings.postProcessAlpha);
			mat.SetFloat ("_ScatteringExposure", interpolatedSettings.scatteringExposure);
			mat.SetFloat ("_global_depth", interpolatedSettings.postProcessDepth *1000000);

			if (m_manager.flatScaledSpaceModel && m_manager.parentCelestialBody.pqsController)
			{
				if (MapView.MapIsEnabled)
					mat.SetFloat ("_PlanetOpacity", 0f);
				else
					mat.SetFloat ("_PlanetOpacity", 1f - m_manager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat ("_PlanetOpacity"));
			}
			
			mat.SetFloat ("_Post_Extinction_Tint", interpolatedSettings.extinctionTint);
			mat.SetFloat ("extinctionThickness", interpolatedSettings.extinctionThickness);

			mat.SetFloat ("_openglThreshold", interpolatedSettings.openglThreshold);

			mat.SetVector ("SUN_DIR", m_manager.getDirectionToSun ().normalized);
			mat.SetVector ("_planetPos", parentLocalTransform.position);  //better do this small calculation here


			if (Core.Instance.mainSettings.usePlanetShine)
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
			mat.SetFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f);
			mat.SetFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f);
			mat.SetVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f);
			mat.SetVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f) / 0.9f);

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


			if (Core.Instance.mainSettings.useEclipses)
			{
				mat.EnableKeyword ("ECLIPSES_ON");
				mat.DisableKeyword ("ECLIPSES_OFF");
			}
			else
			{
				mat.DisableKeyword ("ECLIPSES_ON");
				mat.EnableKeyword ("ECLIPSES_OFF");
			}
			
			if (Core.Instance.mainSettings.usePlanetShine)
			{
				mat.EnableKeyword ("PLANETSHINE_ON");
				mat.DisableKeyword ("PLANETSHINE_OFF");	
			}
			else
			{
				mat.DisableKeyword ("PLANETSHINE_ON");
				mat.EnableKeyword ("PLANETSHINE_OFF");
			}

			mat.SetFloat ("flatScaledSpaceModel", m_manager.flatScaledSpaceModel ? 1f : 0f );
			mat.SetVector ("_sunColor", m_manager.sunColor);
		}

		
		public void setManager (Manager manager)
		{
			m_manager = manager;
		}
		
		public void togglePostProcessing()
		{
			postprocessingEnabled = !postprocessingEnabled;
		}
		
		void loadPrecomputedTables ()
		{
			//load from .half, probably an 8 mb leak every scene change
			//if no .half file exists, load from .raw file and create .half file
			string _file = Utils.GameDataPath + assetPath + "/inscatter.half";
			if (System.IO.File.Exists(_file))
				m_inscatter.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				loadAndConvertRawFile("inscatter",m_inscatter,4);
			
			_file = Utils.GameDataPath + assetPath + "/transmittance.half";
			
			if (System.IO.File.Exists(_file))
				m_transmit.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				loadAndConvertRawFile("transmittance",m_transmit,3);
			
			_file = Utils.GameDataPath + assetPath + "/irradiance.half";
			
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
			
			string _file = Utils.GameDataPath + assetPath + "/"+textureName+".raw";

			if (!System.IO.File.Exists(_file))
			{
				Utils.LogDebug(" no "+textureName+".raw or "+textureName+".half file found for "
				          +celestialBodyName);
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
			
			_file = Utils.GameDataPath + assetPath + "/"+textureName+".half";
			
			byte[] bytes = targetTexture2D .GetRawTextureData();
			System.IO.File.WriteAllBytes(_file ,bytes);
			
			Utils.LogDebug (" Converted "+textureName+".raw to "+textureName+".half");
			
			UnityEngine.Object.Destroy (tempRT);
			bytes = null;
		}
		
		
		
		public void Cleanup ()
		{
			if (Core.Instance.mainSettings.autosavePlanetSettingsOnSceneChange)
			{
				saveToConfigNode ();
			}

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

			UnityEngine.Object.Destroy(atmosphereMesh);

			Component.Destroy (skySphereMeshRenderer);
			UnityEngine.Object.Destroy (skySphereGameObject);

			//disable eve integration scatterer flag
			if (Core.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				try
				{
					int size;
					
					//2d clouds
					if(Core.Instance.EVEClouds.ContainsKey(celestialBodyName))
					{
						size = Core.Instance.EVEClouds[celestialBodyName].Count;
						for (int i=0;i<size;i++)
						{
							Core.Instance.EVEClouds[celestialBodyName][i].DisableKeyword ("SCATTERER_ON");
							Core.Instance.EVEClouds[celestialBodyName][i].EnableKeyword ("SCATTERER_OFF");
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

			if (!ReferenceEquals(null,localScatteringProjector))
			{
				UnityEngine.Object.Destroy (localScatteringProjector);
			}
		}

		public bool loadFromConfigNode ()
		{
			ConfigNode cnToLoad = new ConfigNode();
			ConfigNode[] configNodeArray;
			bool found = false;

			foreach (UrlDir.UrlConfig _url in Core.Instance.atmoConfigs)
			{
				configNodeArray = _url.config.GetNodes("Atmo");
				//if (_url.config.TryGetNode("Atmo",ref cnToLoad))

				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name") && _cn.GetValue("name") == celestialBodyName)
					{
						cnToLoad = _cn;
						configUrl = _url;
						found = true;
						break;
					}
				}
			}

			if (found)
			{
				Utils.LogDebug(" Atmosphere config found for: "+celestialBodyName);

				ConfigNode.LoadObjectFromConfig (this, cnToLoad);		
			
				m_radius = (float)m_manager.GetRadius ();
			
				Rt = (Rt / Rg) * m_radius;
				RL = (RL / Rg) * m_radius;
				Rg = m_radius;
			}
			else
			{
				Utils.LogDebug(" Atmosphere config not found for: "+celestialBodyName);
				Utils.LogDebug(" Removing "+celestialBodyName +" from planets list");

				Core.Instance.scattererCelestialBodies.Remove(Core.Instance.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == celestialBodyName));

				m_manager.OnDestroy();
				UnityEngine.Object.Destroy (m_manager);
				UnityEngine.Object.Destroy (this);
			}

			return found;
		}
		
		public void saveToConfigNode ()
		{
			ConfigNode[] configNodeArray;
			bool found = false;

			configNodeArray = configUrl.config.GetNodes("Atmo");
			
			foreach(ConfigNode _cn in configNodeArray)
			{
				if (_cn.HasValue("name") && _cn.GetValue("name") == celestialBodyName)
				{
					ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
					_cn.ClearData();
					ConfigNode.Merge (_cn, cnTemp);
					_cn.name="Atmo";
					Utils.LogDebug(" saving "+celestialBodyName+" atmo config to: "+configUrl.parent.url);
					configUrl.parent.SaveConfigs ();
					found=true;
					break;
				}
			}

			if (!found)
			{
				Utils.LogDebug(" couldn't find config file to save to");
			}
		}
		
		public void tweakStockAtmosphere ()
		{
			for (int i = 0; i < parentScaledTransform.childCount; i++)
			{
				if (parentScaledTransform.GetChild (i).gameObject.layer == 9)
				{
					if (parentScaledTransform.GetChild (i).gameObject.GetComponent < MeshRenderer > () != skySphereMeshRenderer)
					{
						stockSkyMeshRenderer = parentScaledTransform.GetChild (i).gameObject.GetComponent < MeshRenderer > ();
						stockSkyMeshRenderer.enabled=false;
						break;
					}
				}
			}

			Renderer tRenderer=(Renderer) parentScaledTransform.GetComponent(typeof(Renderer));
			Material sharedMaterial = tRenderer.sharedMaterial;
			
			sharedMaterial.SetFloat (Shader.PropertyToID ("_rimBlend"), rimBlend / 100f);
			sharedMaterial.SetFloat (Shader.PropertyToID ("_rimPower"), rimpower / 100f);
			sharedMaterial.SetColor ("_SpecColor", new Color (specR / 100f, specG / 100f, specB / 100f));
			sharedMaterial.SetFloat ("_Shininess", shininess / 100);


		}

		public void addScaledScatteringMaterialToPlanet ()
		{
			MeshRenderer tMeshRenderer=(MeshRenderer) parentScaledTransform.GetComponent(typeof(MeshRenderer));
			List<Material> mats = tMeshRenderer.materials.ToList();
			mats.RemoveAll (mat => mat.name.Contains("Scatterer/ScaledPlanetScattering")); //clean up old materials

			mats.Add (scaledScatteringMaterial);
			tMeshRenderer.materials = mats.ToArray ();
		}

		public void interpolateVariables ()
		{
			if ((HighLogic.LoadedScene == GameScenes.MAINMENU) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION) || MapView.MapIsEnabled)
			{
				interpolatedSettings.getValuesFrom(configPoints [configPoints.Count - 1]);
				currentConfigPoint = configPoints.Count;
				return;
			}

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
				//TODO, replace this with binary search, implement method directly in configPoints class, which will implement a list of config points
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


		public void initiateEVEClouds()
		{
			if (!ReferenceEquals (Core.Instance.EVEinstance, null))
			{
				try
				{
					const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | 
						BindingFlags.Instance | BindingFlags.Static;
					
					foreach (object _obj in Core.Instance.EVECloudObjects[celestialBodyName])
					{
						object cloud2dObj = _obj.GetType ().GetField ("layer2D", flags).GetValue (_obj) as object;
						if (cloud2dObj == null)
						{
							Utils.LogDebug (" layer2d not found for layer on planet: " + celestialBodyName);
							continue;
						}
						
						bool cloud2dScaled = (bool)cloud2dObj.GetType ().GetField ("isScaled", flags).GetValue (cloud2dObj);
						
						MethodInfo scaledGetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetGetMethod ();
						MethodInfo scaledSetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetSetMethod ();
						
						//if in scaled mode, switch it to local then back to scaled, to set all the properties
						if (cloud2dScaled)
							scaledSetter.Invoke (cloud2dObj, new object[] { !cloud2dScaled });
						
						scaledSetter.Invoke (cloud2dObj, new object[] { cloud2dScaled });
					}
					
					//initialize other params here
					int size = Core.Instance.EVEClouds [celestialBodyName].Count;
					for (int i=0; i<size; i++)
					{
						InitUniforms (Core.Instance.EVEClouds [celestialBodyName] [i]);
						InitPostprocessMaterial (Core.Instance.EVEClouds [celestialBodyName] [i]);
						
						if (EVEIntegration_preserveCloudColors)
						{
							Core.Instance.EVEClouds [celestialBodyName] [i].EnableKeyword ("PRESERVECLOUDCOLORS_ON");
							Core.Instance.EVEClouds [celestialBodyName] [i].DisableKeyword ("PRESERVECLOUDCOLORS_OFF");
						} else {
							Core.Instance.EVEClouds [celestialBodyName] [i].EnableKeyword ("PRESERVECLOUDCOLORS_OFF");
							Core.Instance.EVEClouds [celestialBodyName] [i].DisableKeyword ("PRESERVECLOUDCOLORS_ON");
						}
						
					}
				}
				catch (Exception stupid)
				{
					Utils.LogDebug (" Error calling clouds2d.reassign() on planet: " + celestialBodyName + " Exception returned: " + stupid.ToString ());
				}
			}
		}

		public void mapEVEVolumetrics()
		{
			Utils.LogDebug (" Mapping EVE volumetrics for planet: "+celestialBodyName);

			EVEvolumetrics.Clear ();

			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;

			if (Core.Instance.EVECloudObjects.ContainsKey (celestialBodyName)) //EVECloudObjects contain both the 2d clouds and the volumetrics, here we extract the volumetrics
			{
				List<object> cloudObjs = Core.Instance.EVECloudObjects [celestialBodyName];
				
				foreach (object _obj in cloudObjs)
				{
					try
					{
						object cloudsPQS = _obj.GetType ().GetField ("cloudsPQS", flags).GetValue (_obj) as object;
						object layerVolume = cloudsPQS.GetType ().GetField ("layerVolume", flags).GetValue (cloudsPQS) as object;
						if (ReferenceEquals(layerVolume, null))
						{
							Utils.LogDebug (" No volumetric cloud for layer on planet: " + celestialBodyName);
							continue;
						}

						//TODO take this snippet and use it somewhere else to disable volumetrics when rendering
						//GameObject volumeHolder = layerVolume.GetType ().GetField ("VolumeHolder", flags).GetValue (layerVolume) as GameObject;

						Material ParticleMaterial = layerVolume.GetType ().GetField ("ParticleMaterial", flags).GetValue (layerVolume) as Material;

						if (ReferenceEquals(layerVolume, null))
						{
							Utils.LogDebug (" Volumetric cloud has no material on planet: " + celestialBodyName);
							continue;
						}

						ParticleMaterial.EnableKeyword ("SCATTERER_ON");
						ParticleMaterial.DisableKeyword ("SCATTERER_OFF");

						InitUniforms(ParticleMaterial);
						InitPostprocessMaterial(ParticleMaterial);

						if (Core.Instance.sunlightModulatorInstance)
						{
							ParticleMaterial.EnableKeyword ("SCATTERER_USE_ORIG_DIR_COLOR_ON");
							ParticleMaterial.DisableKeyword("SCATTERER_USE_ORIG_DIR_COLOR_OFF");
						}
						else
						{
							ParticleMaterial.DisableKeyword ("SCATTERER_USE_ORIG_DIR_COLOR_ON");
							ParticleMaterial.EnableKeyword("SCATTERER_USE_ORIG_DIR_COLOR_OFF");
						}

						EVEvolumetrics.Add (ParticleMaterial);
					}
					catch (Exception stupid)
					{
						Utils.LogDebug (" Volumetric clouds error on planet: " + celestialBodyName + stupid.ToString ());
					}
				}				
				Utils.LogDebug (" Detected " + EVEvolumetrics.Count + " EVE volumetric layers for planet: " + celestialBodyName);
			}
			else
			{
				Utils.LogDebug (" No cloud objects for planet: " + celestialBodyName);
			}
		}

		public void togglePreserveCloudColors()
		{
			if (Core.Instance.mainSettings.integrateWithEVEClouds)
			{
				if(Core.Instance.EVEClouds.ContainsKey(celestialBodyName)) //change to a bool hasclouds
				{
					int size = Core.Instance.EVEClouds[celestialBodyName].Count;
					for (int i=0;i<size;i++)
					{
						if (EVEIntegration_preserveCloudColors)
						{
							Core.Instance.EVEClouds[celestialBodyName][i].EnableKeyword ("PRESERVECLOUDCOLORS_OFF");
							Core.Instance.EVEClouds[celestialBodyName][i].DisableKeyword ("PRESERVECLOUDCOLORS_ON");
						}
						else
						{
							Core.Instance.EVEClouds[celestialBodyName][i].EnableKeyword ("PRESERVECLOUDCOLORS_ON");
							Core.Instance.EVEClouds[celestialBodyName][i].DisableKeyword ("PRESERVECLOUDCOLORS_OFF");
						}
					}
				}
				EVEIntegration_preserveCloudColors =!EVEIntegration_preserveCloudColors;
			}
		}

		public void setCelestialBodyName(string name) {
			celestialBodyName = name;
		}
		
		public void setParentScaledTransform(Transform parentTransform) {
			parentScaledTransform = parentTransform;
		}
		
		public void setParentLocalTransform(Transform parentTransform) {
			parentLocalTransform = parentTransform;
		}

		//to be called on loss of rendertextures, ie alt-enter
		public void reInitMaterialUniformsOnRenderTexturesLoss()
		{
			InitPostprocessMaterial (localScatteringProjector.projector.material);
		}	
	}
}