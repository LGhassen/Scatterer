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
	public class SkyNode: MonoBehaviour
	{
		[Persistent] protected string name;

		[Persistent] public float Rg;	//The radius of the planet
		[Persistent] public float Rt;	//Radius of the atmosphere
		[Persistent] float RL;			//RL = Rt + epsilon used to avoid some artifacts due to numerical precision issues in the old Proland code (the one used in scatterer)
		[Persistent] float HR = 8.0f;	//Half heights for the atmosphere air density (HR) and particle density (HM), this is the height in km that half the particles are found below
		[Persistent] float HM = 1.2f;
		[Persistent] Vector3 BETA_MSca = new Vector3 (4e-3f, 4e-3f, 4e-3f); //scatter coefficient for mie
		[Persistent] Vector3 m_betaR = new Vector3 (5.8e-3f, 1.35e-2f, 3.31e-2f);
		[Persistent] public float m_mieG = 0.85f; //Asymmetry factor for the mie phase function, a higher number means more light is scattered in the forward direction

		[Persistent] public string assetPath;

		[Persistent] public float experimentalAtmoScale=1f;
		[Persistent] public float atmosphereGlobalScale = 1f;

		[Persistent] public float godrayStrength = 0.8f;
		[Persistent] public float flattenScaledSpaceMesh = 0f;
		[Persistent] public float rimBlend = 20f;
		[Persistent] public float rimpower = 600f;
		[Persistent] public float specR = 0f;
		[Persistent] public float specG = 0f;
		[Persistent] public float specB = 0f;
		[Persistent] public float shininess = 0f;

		[Persistent] public float cloudColorMultiplier=3f;
		[Persistent] public float cloudScatteringMultiplier=0.2f;
		[Persistent] public float cloudSkyIrradianceMultiplier=0.05f;
		[Persistent] public float volumetricsColorMultiplier = 1f;
		[Persistent] public bool EVEIntegration_preserveCloudColors = false;
//		[Persistent] public float godrayCloudAlphaThreshold = 0.25f;

		[Persistent] public List < ConfigPoint > configPoints = new List < ConfigPoint > ();

		Texture2D m_inscatter, m_irradiance;
		public Texture2D m_transmit;

		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;
		const int RES_R = 32;
		const int RES_MU = 128;
		const int RES_MU_S = 32;
		const int RES_NU = 8;
		
		string celestialBodyName;
		public Transform parentScaledTransform, parentLocalTransform;

		public ProlandManager m_manager;
		public UrlDir.UrlConfig configUrl;
		
		public bool usesCloudIntegration = true;
		public List<Material> EVEvolumetrics = new List<Material>();
		bool mappedVolumetrics=false;
		
		public float altitude;
		public float percentage;
		public int currentConfigPoint;
		public ConfigPoint interpolatedSettings= new ConfigPoint();
		public bool inScaledSpace = true, simulateOceanInteraction=false;

		Vector3 sunPosRelPlanet=Vector3.zero;
		Matrix4x4 castersMatrix1=Matrix4x4.zero;
		Matrix4x4 castersMatrix2=Matrix4x4.zero;
		public Matrix4x4 planetShineSourcesMatrix=Matrix4x4.zero;
		public Matrix4x4 planetShineRGBMatrix=Matrix4x4.zero;

		SkySphereContainer skySphere;
		GameObject stockSkyGameObject;
		MeshRenderer stockScaledPlanetMeshRenderer;
		Mesh originalScaledMesh, tweakedScaledmesh;
		public ScaledScatteringContainer scaledScatteringContainer;
		public Material localScatteringMaterial,skyMaterial,scaledScatteringMaterial,sunflareExtinctionMaterial;
		public AbstractLocalAtmosphereContainer localScatteringContainer;
		public GodraysRenderer godraysRenderer;
		public bool postprocessingEnabled = true;

		GameObject ringObject;
		float ringInnerRadius, ringOuterRadius;
		Texture2D ringTexture;
		bool hasRingObjectAndShadowActivated = false;
	
		bool skyNodeInitiated = false;

		public void Init ()
		{
			float celestialBodyRadius = (float) m_manager.GetRadius ();
			Rt = Rt * (celestialBodyRadius / Rg);
			RL = RL * (celestialBodyRadius / Rg);
			Rg = celestialBodyRadius;
						
			InitPrecomputedTables ();

			skyMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/SkySphere")]);
			scaledScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/ScaledPlanetScattering")]);

			if (Scatterer.Instance.mainSettings.useDepthBufferMode)
				localScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthBufferScattering")]);
			else
				localScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);


			skyMaterial.SetOverrideTag ("IgnoreProjector", "True");
			scaledScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");
			localScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");

			Utils.EnableOrDisableShaderKeywords (localScatteringMaterial, "ECLIPSES_ON", "ECLIPSES_OFF", Scatterer.Instance.mainSettings.useEclipses);
			Utils.EnableOrDisableShaderKeywords (localScatteringMaterial, "DISABLE_UNDERWATER_ON", "DISABLE_UNDERWATER_OFF", m_manager.hasOcean);

			if (Scatterer.Instance.mainSettings.useGodrays && Scatterer.Instance.unifiedCameraMode && !ReferenceEquals (m_manager.parentCelestialBody.pqsController, null)
			    && Scatterer.Instance.mainSettings.terrainShadows && (Scatterer.Instance.mainSettings.unifiedCamShadowResolutionOverride != 0))
			{
				godraysRenderer = (GodraysRenderer) Utils.getEarliestLocalCamera().gameObject.AddComponent (typeof(GodraysRenderer));
				if (!godraysRenderer.Init(m_manager.mainSunLight, this))
				{
					Component.Destroy (godraysRenderer);
					godraysRenderer = null;
				}
			}

			if (Scatterer.Instance.mainSettings.useRingShadows)
			{
				InitKopernicusRings ();
			}

			InitSkySphere ();

			InitPostprocessMaterialUniforms (localScatteringMaterial);
			TweakScaledMesh ();
			InitScaledScattering ();

			if (!ReferenceEquals (m_manager.parentCelestialBody.pqsController, null))
			{
				m_manager.parentCelestialBody.pqsController.isActive = false; 	//sometimes the PQS is forgotten as "active" if a ship is loaded directly around another body, this would mess with the mod
																				//this sets it to false, if it's really active it will be set to active automatically. EVE mod seems also to have a fix for this
			}

			if ((HighLogic.LoadedScene != GameScenes.MAINMENU) && (HighLogic.LoadedScene != GameScenes.TRACKSTATION)) // &&useLocalScattering
			{
				if (Scatterer.Instance.mainSettings.useDepthBufferMode)
					localScatteringContainer = new ScreenSpaceScatteringContainer(localScatteringMaterial, parentLocalTransform, Rt, m_manager);
				else
					localScatteringContainer = new AtmosphereProjectorContainer (localScatteringMaterial, parentLocalTransform, Rt, m_manager);

			}



			if (Scatterer.Instance.mainSettings.fullLensFlareReplacement)
			{
				sunflareExtinctionMaterial = new Material (ShaderReplacer.Instance.LoadedShaders ["Scatterer/sunFlareExtinction"]);
				sunflareExtinctionMaterial.SetFloat (ShaderProperties.Rg_PROPERTY, Rg);
				sunflareExtinctionMaterial.SetFloat (ShaderProperties.Rt_PROPERTY, Rt);
				sunflareExtinctionMaterial.SetTexture (ShaderProperties._Sky_Transmittance_PROPERTY, m_transmit);

				if (hasRingObjectAndShadowActivated)
				{
					sunflareExtinctionMaterial.SetFloat (ShaderProperties.ringInnerRadius_PROPERTY, ringInnerRadius);
					sunflareExtinctionMaterial.SetFloat (ShaderProperties.ringOuterRadius_PROPERTY, ringOuterRadius);
					sunflareExtinctionMaterial.SetVector (ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
					sunflareExtinctionMaterial.SetTexture (ShaderProperties.ringTexture_PROPERTY, ringTexture);
				}

				Utils.EnableOrDisableShaderKeywords (sunflareExtinctionMaterial, "DISABLE_UNDERWATER_ON", "DISABLE_UNDERWATER_OFF", m_manager.hasOcean);
			}

			stockScaledPlanetMeshRenderer = (MeshRenderer) parentScaledTransform.GetComponent<MeshRenderer>();
			
			TweakStockAtmosphere();

			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				InitEVEClouds();
			}
			
			skyNodeInitiated = true;
			Utils.LogDebug("Skynode initiated for "+celestialBodyName);
		}

		void InitSkySphere ()
		{
			float skySphereSize = 2 * (4 * (Rt - Rg) + Rg) / ScaledSpace.ScaleFactor;
			skySphere = new SkySphereContainer (skySphereSize, skyMaterial, parentLocalTransform, parentScaledTransform);

			if (HighLogic.LoadedScene != GameScenes.MAINMENU)
			{
				if (m_manager.parentCelestialBody.pqsController != null && m_manager.parentCelestialBody.pqsController.isActive && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
				{
					skySphere.SwitchLocalMode ();
				}
				else
				{
					skySphere.SwitchScaledMode ();
				}
			}

			skyMaterial.renderQueue = 2999;
			InitUniforms (skyMaterial);
		}

		public void InitScaledScattering ()
		{
			scaledScatteringContainer = new ScaledScatteringContainer (parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh, scaledScatteringMaterial, parentLocalTransform, parentScaledTransform);

			if (HighLogic.LoadedScene != GameScenes.MAINMENU)
			{
				if (m_manager.parentCelestialBody.pqsController != null && m_manager.parentCelestialBody.pqsController.isActive && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
				{
					scaledScatteringContainer.SwitchLocalMode ();
				}
				else
				{
					scaledScatteringContainer.SwitchScaledMode ();
				}
			}
			
			scaledScatteringMaterial.renderQueue = 2998;
			InitUniforms (scaledScatteringMaterial);
		}
		
		public void OnPreRender()
		{
			UpdateGraphicsUniforms ();

			if (!MapView.MapIsEnabled && Scatterer.Instance.mainSettings.sunlightExtinction)
			{
				UpdateMainLightExtinction ();
			}
		}

		public void UpdateGraphicsUniforms()
		{
			if (!inScaledSpace && !MapView.MapIsEnabled && postprocessingEnabled && !ReferenceEquals(localScatteringContainer,null))
			{
				UpdatePostProcessMaterialUniforms (localScatteringContainer.material);
			}
			if (Scatterer.Instance.mainSettings.useEclipses)
			{
				UpdateEclipseCasters ();
			}
			if (Scatterer.Instance.mainSettings.usePlanetShine)
			{
				UpdatePlanetShine ();
			}
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				UpdateEVECloudMaterials ();
			}
			if (!ReferenceEquals(Scatterer.Instance.sunflareManager,null))
			{
				UpdateSunflareExtinctions ();
			}

			if (!ReferenceEquals(scaledScatteringContainer,null))
				scaledScatteringContainer.MeshRenderer.enabled = stockScaledPlanetMeshRenderer.enabled;

			if (!ReferenceEquals (localScatteringContainer, null))
			{
				localScatteringContainer.setInScaledSpace (inScaledSpace);
				localScatteringContainer.updateContainer ();

				if (m_manager.parentCelestialBody.pqsController != null)
				{
					float planetOpactiy = m_manager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY);
					localScatteringMaterial.SetInt (ShaderProperties._ZwriteVariable_PROPERTY, (planetOpactiy > 0f) ? 1 : 0);
				}
			}

			SetUniforms (skyMaterial);
			SetUniforms (scaledScatteringMaterial);
		}
		
		
		public void UpdateNode ()
		{
			if ((m_manager.parentCelestialBody.pqsController != null) && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				bool previousState = inScaledSpace;

				inScaledSpace = !m_manager.parentCelestialBody.pqsController.isActive;

				//if we go from scaled to local space
				if (!inScaledSpace && previousState)
				{
					SwitchEffectsLocal();
				}
				
				//if we go from local to scaled
				if (inScaledSpace && !previousState)
				{
					SwitchEffectsScaled();
				}


				//For wave interactions, consider we are in scaledSpace only if we aren't in map view, otherwise we can be in the surface but pqsController is inactive
				if (!MapView.MapIsEnabled)
					simulateOceanInteraction = m_manager.parentCelestialBody.pqsController.isActive;
			}
			else
			{
				inScaledSpace = true;
				simulateOceanInteraction = false;
			}

			if (skyNodeInitiated)
			{
				InterpolateVariables ();

				if (m_manager.hasOcean && !Scatterer.Instance.mainSettings.useOceanShaders)
				{
					skySphere.MeshRenderer.enabled = (altitude>=0f);
					stockSkyGameObject.SetActive(altitude<0f); //re-enable stock sky meshrenderer, for compatibility with stock underwater effect
				}
			}
		}

		public void SwitchEffectsScaled()
		{
			Utils.LogInfo ("Skynode switch effects to scaled mode");

			if (!ReferenceEquals(skySphere,null))
				skySphere.SwitchScaledMode ();
			if (!ReferenceEquals(scaledScatteringContainer,null))
				scaledScatteringContainer.SwitchScaledMode ();
			EVEvolumetrics.Clear();
		}

		public void SwitchEffectsLocal()
		{
			Utils.LogInfo ("Skynode switch effects to local mode");

			if (!ReferenceEquals(skySphere,null))
				skySphere.SwitchLocalMode();
			if (!ReferenceEquals(scaledScatteringContainer,null))
				scaledScatteringContainer.SwitchLocalMode ();

			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				//really strange but when changing scenes StartCoroutine can return a nullref, even though I check all references
				try {StartCoroutine(DelayedMapEVEVolumetrics());}
				catch (Exception){}
			}
		}
		
		public void SetUniforms (Material mat)
		{
			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);
			mat.SetFloat (ShaderProperties._viewdirOffset_PROPERTY, interpolatedSettings.viewdirOffset);
			mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
			mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
			mat.SetFloat (ShaderProperties.extinctionTint_PROPERTY, interpolatedSettings.extinctionTint); //extinctionTint for scaled+local
			mat.SetFloat (ShaderProperties.extinctionThickness_PROPERTY, interpolatedSettings.extinctionThickness);

			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);

			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ());

			mat.SetFloat (ShaderProperties._SkyExposure_PROPERTY, interpolatedSettings.skyExposure);
			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);

			if (Scatterer.Instance.mainSettings.useEclipses)
			{
				mat.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				mat.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
				mat.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                       sunPosRelPlanet.z, (float)m_manager.sunCelestialBody.Radius));
			}
			if (Scatterer.Instance.mainSettings.usePlanetShine)
			{
				mat.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, planetShineSourcesMatrix);
				mat.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, planetShineRGBMatrix);
			}

			if (hasRingObjectAndShadowActivated)
			{
				mat.SetVector(ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
			}

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetFloat(ShaderProperties._godrayStrength_PROPERTY, godrayStrength);
			}
		}
		
		
		public void SetOceanUniforms (Material mat)
		{
			if (mat == null)
				return;

			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);
			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);

			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.RL_PROPERTY, RL * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToSun ());
			mat.SetVector(ShaderProperties._camForward_PROPERTY, Scatterer.Instance.nearCamera.transform.forward);

			mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);			
			mat.SetFloat (ShaderProperties._SkyExposure_PROPERTY, interpolatedSettings.skyExposure);

			UpdatePostProcessMaterialUniforms (mat);
		}
		

		public void InitPostprocessMaterialUniforms (Material mat)
		{
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);

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
			
			
			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun());

			Utils.EnableOrDisableShaderKeywords (mat, "PLANETSHINE_ON", "PLANETSHINE_OFF", Scatterer.Instance.mainSettings.usePlanetShine);

			//When using custom ocean shaders, we don't reuse the ocean mesh to render scattering separately: Instead ocean shader handles scattering internally
			//When the ocean starts fading out when transitioning to orbit, ocean shader stops doing scattering, and stops writing to z-buffer
			//The ocean floor vertexes are then used by the scattering shader, moving them to the surface to render scattering, this is not needed for stock ocean so disable it
			Utils.EnableOrDisableShaderKeywords (mat, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", Scatterer.Instance.mainSettings.useOceanShaders && m_manager.hasOcean);

			Utils.EnableOrDisableShaderKeywords (mat, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);

			if (m_manager.flatScaledSpaceModel && m_manager.parentCelestialBody.pqsController)
				mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 0f);
			else
				mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 1f);

			mat.SetColor (ShaderProperties._sunColor_PROPERTY, m_manager.sunColor);

			float camerasOverlap = 0f;
			if (!Scatterer.Instance.unifiedCameraMode)
				camerasOverlap = Scatterer.Instance.nearCamera.farClipPlane - Scatterer.Instance.farCamera.nearClipPlane;

			mat.SetFloat(ShaderProperties._ScattererCameraOverlap_PROPERTY,camerasOverlap);

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetTexture(ShaderProperties._godrayDepthTexture_PROPERTY,godraysRenderer.volumeDepthTexture);
			}
			Utils.EnableOrDisableShaderKeywords (mat, "GODRAYS_ON", "GODRAYS_OFF", !ReferenceEquals (godraysRenderer, null));
		}

		
		public void UpdatePostProcessMaterialUniforms (Material mat)
		{
			//all these don't need to be on update, just make it so that the UI sets them
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg * atmosphereGlobalScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt * atmosphereGlobalScale);

			mat.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);

			mat.SetFloat (ShaderProperties._global_alpha_PROPERTY, interpolatedSettings.postProcessAlpha);
			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);
			mat.SetFloat (ShaderProperties._global_depth_PROPERTY, interpolatedSettings.postProcessDepth *1000000);

			if (m_manager.flatScaledSpaceModel && m_manager.parentCelestialBody.pqsController)
			{
				if (MapView.MapIsEnabled)
					mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 0f);
				else
					mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 1f - m_manager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY));
			}

			mat.SetFloat (ShaderProperties._Post_Extinction_Tint_PROPERTY, interpolatedSettings.extinctionTint);
			mat.SetFloat (ShaderProperties.extinctionThickness_PROPERTY, interpolatedSettings.extinctionThickness);

			mat.SetFloat (ShaderProperties._openglThreshold_PROPERTY, interpolatedSettings.openglThreshold);

			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, m_manager.getDirectionToSun ());
			mat.SetVector (ShaderProperties._planetPos_PROPERTY, parentLocalTransform.position);


			if (Scatterer.Instance.mainSettings.usePlanetShine)
			{
				mat.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, planetShineSourcesMatrix);
				mat.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, planetShineRGBMatrix);
			}

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetFloat("_godrayStrength", godrayStrength);
			}
		}

		public void InitUniforms (Material mat)
		{
			if (mat == null)
				return;
			
			mat.SetFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f);
			mat.SetTexture (ShaderProperties._Transmittance_PROPERTY, m_transmit);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
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

			if (hasRingObjectAndShadowActivated)
			{
				Utils.EnableOrDisableShaderKeywords (mat, "RINGSHADOW_ON", "RINGSHADOW_OFF", true);
				mat.SetFloat (ShaderProperties.ringInnerRadius_PROPERTY, ringInnerRadius);
				mat.SetFloat (ShaderProperties.ringOuterRadius_PROPERTY, ringOuterRadius);
				mat.SetVector (ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
				mat.SetTexture (ShaderProperties.ringTexture_PROPERTY, ringTexture);
			}
			else
			{
				Utils.EnableOrDisableShaderKeywords (mat, "RINGSHADOW_ON", "RINGSHADOW_OFF", false);
			}

			Utils.EnableOrDisableShaderKeywords (mat, "ECLIPSES_ON", "ECLIPSES_OFF", Scatterer.Instance.mainSettings.useEclipses && HighLogic.LoadedScene != GameScenes.MAINMENU ); //disable bugged eclipses on main menu
			Utils.EnableOrDisableShaderKeywords (mat, "PLANETSHINE_ON", "PLANETSHINE_OFF", Scatterer.Instance.mainSettings.usePlanetShine);
			Utils.EnableOrDisableShaderKeywords (mat, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);

			mat.SetFloat (ShaderProperties.flatScaledSpaceModel_PROPERTY, m_manager.flatScaledSpaceModel ? 1f : 0f );
			mat.SetColor (ShaderProperties._sunColor_PROPERTY, m_manager.sunColor);

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetTexture(ShaderProperties._godrayDepthTexture_PROPERTY,godraysRenderer.volumeDepthTexture);
			}
			Utils.EnableOrDisableShaderKeywords (mat, "GODRAYS_ON", "GODRAYS_OFF", !ReferenceEquals (godraysRenderer, null));
		}

		public void TogglePostProcessing()
		{
			postprocessingEnabled = !postprocessingEnabled;
		}
		
		void InitPrecomputedTables ()
		{
			//Inscatter is responsible for the change in the sky color as the sun moves. The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture and the shader manually performs the sample for the 4th dimension
			//To get scatterer running in dx9, the texture was packed into a 2D texture. Although dx9 is deprecated now I haven't changed this back because it works
			m_inscatter = new Texture2D (RES_MU_S * RES_NU, RES_MU * RES_R, TextureFormat.RGBAHalf,false);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			//Transmittance is responsible for the change in the sun color as it moves. The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_transmit = new Texture2D (TRANSMITTANCE_W, TRANSMITTANCE_H, TextureFormat.RGBAHalf,false);
			m_transmit.wrapMode = TextureWrapMode.Clamp;
			m_transmit.filterMode = FilterMode.Bilinear;
			
			//Irradiance is responsible for the change in light emitted from the sky as the sun moves. The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_irradiance = new Texture2D (SKY_W, SKY_H, TextureFormat.RGBAHalf,false);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;

			//load from .half, if no .half file exists, load from .raw file and create .half file
			string _file = Utils.GameDataPath + assetPath + "/inscatter.half";
			if (System.IO.File.Exists(_file))
				m_inscatter.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				LoadAndConvertRawFile("inscatter",m_inscatter,4);
			
			_file = Utils.GameDataPath + assetPath + "/transmittance.half";
			
			if (System.IO.File.Exists(_file))
				m_transmit.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				LoadAndConvertRawFile("transmittance",m_transmit,3);
			
			_file = Utils.GameDataPath + assetPath + "/irradiance.half";
			
			if (System.IO.File.Exists(_file))
				m_irradiance.LoadRawTextureData (System.IO.File.ReadAllBytes (_file));
			else
				LoadAndConvertRawFile("irradiance",m_irradiance,3);

			m_inscatter.Apply ();
			m_transmit.Apply ();
			m_irradiance.Apply ();
		}
		
		void LoadAndConvertRawFile(string textureName, Texture2D targetTexture2D, int channels)
		{
			EncodeFloat	encode = new EncodeFloat ();
			
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
			if (Scatterer.Instance.mainSettings.autosavePlanetSettingsOnSceneChange)
			{
				SaveToConfigNode ();
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

			if (!ReferenceEquals (skySphere, null))
			{
				skySphere.Cleanup ();
			}

			if (!ReferenceEquals (scaledScatteringContainer, null))
			{
				scaledScatteringContainer.Cleanup ();
			}

			if (localScatteringContainer)
			{
				UnityEngine.Object.Destroy (localScatteringContainer);
			}

			if (!ReferenceEquals (godraysRenderer, null))
			{
				godraysRenderer.Cleanup();
				Component.DestroyImmediate(godraysRenderer);
			}

			//disable eve integration scatterer flag
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				try
				{
					int size;
					
					//2d clouds
					if(Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey(celestialBodyName))
					{
						size = Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName].Count;
						for (int i=0;i<size;i++)
						{
							Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName][i].Clouds2dMaterial.DisableKeyword ("SCATTERER_ON");
							Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName][i].Clouds2dMaterial.EnableKeyword ("SCATTERER_OFF");
						}
					}
					
					//volumetrics
					//if in local mode and mapping is done
					if (!inScaledSpace && mappedVolumetrics)
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

			if (!ReferenceEquals(originalScaledMesh,null))
				parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh = originalScaledMesh;
		}

		public bool LoadFromConfigNode ()
		{
			ConfigNode cnToLoad = new ConfigNode();
			ConfigNode[] configNodeArray;
			bool found = false;

			foreach (UrlDir.UrlConfig _url in Scatterer.Instance.planetsConfigsReader.atmoConfigs)
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
			
				float celestialBodyRadius = (float)m_manager.GetRadius ();
			
				Rt = (Rt / Rg) * celestialBodyRadius;
				RL = (RL / Rg) * celestialBodyRadius;
				Rg = celestialBodyRadius;
			}
			else
			{
				Utils.LogDebug(" Atmosphere config not found for: "+celestialBodyName);
				Utils.LogDebug(" Removing "+celestialBodyName +" from planets list");

				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Remove(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == celestialBodyName));

				m_manager.OnDestroy();
				UnityEngine.Object.Destroy (m_manager);
				UnityEngine.Object.Destroy (this);
			}

			return found;
		}
		
		public void SaveToConfigNode ()
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
		
		public void TweakStockAtmosphere ()
		{
			for (int i = 0; i < parentScaledTransform.childCount; i++)
			{
				if (parentScaledTransform.GetChild (i).gameObject.layer == 9)
				{
					if (parentScaledTransform.GetChild (i).gameObject.name=="Atmosphere")
					{
						stockSkyGameObject = parentScaledTransform.GetChild (i).gameObject;
						stockSkyGameObject.SetActive(false);
						break;
					}
				}
			}

			Material sharedMaterial = stockScaledPlanetMeshRenderer.sharedMaterial;
			
			sharedMaterial.SetFloat (Shader.PropertyToID ("_rimBlend"), rimBlend / 100f);
			sharedMaterial.SetFloat (Shader.PropertyToID ("_rimPower"), rimpower / 100f);

			if (sharedMaterial.shader.name == "Terrain/Scaled Planet (RimAerial) Standard")
			{
				sharedMaterial.SetColor ("_SpecColor", new Color (specR / 255f, specG / 255f, specB / 255f));
				if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				{
					sharedMaterial.SetFloat ("_Shininess", shininess / 140f); //for some reason still too strong in main menu
				}
				else
				{
					sharedMaterial.SetFloat ("_Shininess", shininess / 120f);
				}
			}
			else
			{
				sharedMaterial.SetColor ("_SpecColor", new Color (specR / 100f, specG / 100f, specB / 100f));
				sharedMaterial.SetFloat ("_Shininess", shininess / 100f);
			}

			if (!ReferenceEquals (m_manager.parentCelestialBody.pqsController, null))
			{
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.surfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.fallbackMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.lowQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.mediumQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.highQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(m_manager.parentCelestialBody.pqsController.ultraQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
			}
		}
		
		public void TweakScaledMesh()
		{
			if (ReferenceEquals (originalScaledMesh, null))
			{
				originalScaledMesh = parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh;
			}

			tweakedScaledmesh = (Mesh)Instantiate (originalScaledMesh);
			
			double scaledRadius = m_manager.GetRadius () / (ScaledSpace.ScaleFactor * parentScaledTransform.localScale.x);
			
			Vector3[] verts = tweakedScaledmesh.vertices;
			
			for (int i=0; i<verts.Length; i++)
			{
				if (verts [i].magnitude > scaledRadius)
				{
					verts [i] = verts [i].normalized * (Mathf.Lerp (verts [i].magnitude, (float)scaledRadius, flattenScaledSpaceMesh));
				}
			}
			
			tweakedScaledmesh.vertices = verts;
			tweakedScaledmesh.RecalculateNormals ();
			tweakedScaledmesh.RecalculateTangents ();
			tweakedScaledmesh.RecalculateBounds ();
			
			parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh = tweakedScaledmesh;
		}

		public void InterpolateVariables ()
		{
			if(!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				altitude = Vector3.Distance (Scatterer.Instance.nearCamera.transform.position, parentLocalTransform.position) - Rg;
			}

			if ((HighLogic.LoadedScene == GameScenes.MAINMENU) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION) || MapView.MapIsEnabled)
			{
				interpolatedSettings.getValuesFrom(configPoints [configPoints.Count - 1]);
				currentConfigPoint = configPoints.Count;
				return;
			}

			if (altitude <= configPoints [0].altitude)
			{
				interpolatedSettings.getValuesFrom(configPoints [0]);
				currentConfigPoint = 0;	
			}
			else if (altitude > configPoints [configPoints.Count - 1].altitude) 
			{
				interpolatedSettings.getValuesFrom(configPoints [configPoints.Count - 1]);
				currentConfigPoint = configPoints.Count;
			}
			else 
			{
				//TODO, replace this with binary search, implement method directly in configPoints class, which will implement a list of config points
				for (int j = 1; j < configPoints.Count; j++)
				{
					if ((altitude > configPoints [j - 1].altitude) && (altitude <= configPoints [j].altitude))
					{
						percentage = (altitude - configPoints [j - 1].altitude) / (configPoints [j].altitude - configPoints [j - 1].altitude);
						interpolatedSettings.interpolateValuesFrom(configPoints [j - 1], configPoints [j], percentage);
						currentConfigPoint = j;
					}
				}
			}
		}

		void UpdateMainLightExtinction ()
		{
			Vector3 extinctionPosition = (FlightGlobals.ActiveVessel ? FlightGlobals.ActiveVessel.transform.position : Scatterer.Instance.nearCamera.transform.position) - parentLocalTransform.position;
			float lerpedScale = Mathf.Lerp (1f, experimentalAtmoScale, (extinctionPosition.magnitude - Rg) / 2000f);
			//hack but keeps the extinction beautiful at sea level, and matches the clouds when you get higher
			Color extinction = AtmosphereUtils.getExtinction (extinctionPosition, m_manager.getDirectionToSun (), Rt, Rg, m_transmit, lerpedScale);
			extinction = Color.Lerp(Color.white, extinction, interpolatedSettings.extinctionThickness);
			Scatterer.Instance.sunlightModulatorsManagerInstance.ModulateByColor (m_manager.mainSunLight, extinction);

		}

		void UpdateSunflareExtinctions ()
		{
			foreach (SunFlare customSunFlare in Scatterer.Instance.sunflareManager.scattererSunFlares)
			{
				sunflareExtinctionMaterial.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, m_manager.getDirectionToCelestialBody (customSunFlare.source).normalized);
				sunflareExtinctionMaterial.SetFloat (ShaderProperties._experimentalAtmoScale_PROPERTY, experimentalAtmoScale);

				if (!MapView.MapIsEnabled)
					sunflareExtinctionMaterial.SetVector (ShaderProperties._Globals_WorldCameraPos_PROPERTY, Scatterer.Instance.nearCamera.transform.position - parentLocalTransform.position);
				else
					sunflareExtinctionMaterial.SetVector (ShaderProperties._Globals_WorldCameraPos_PROPERTY, (Vector3)ScaledSpace.ScaledToLocalSpace (Scatterer.Instance.scaledSpaceCamera.transform.position) - parentLocalTransform.position);

				Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 0);

				if (hasRingObjectAndShadowActivated)
				{
					sunflareExtinctionMaterial.SetVector (ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
					Graphics.Blit (null, customSunFlare.extinctionTexture, sunflareExtinctionMaterial, 1);
				}
			}
		}

		void UpdateEclipseCasters ()
		{
			float scaleFactor = ScaledSpace.ScaleFactor;
			sunPosRelPlanet = Vector3.zero;
			sunPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (m_manager.sunCelestialBody.transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor));
			castersMatrix1 = Matrix4x4.zero;
			castersMatrix2 = Matrix4x4.zero;
			Vector3 casterPosRelPlanet;
			for (int i = 0; i < Mathf.Min (4, m_manager.eclipseCasters.Count); i++)
			{
				casterPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (m_manager.eclipseCasters [i].transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor));
				castersMatrix1.SetRow (i, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y, casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
			}
			for (int i = 4; i < Mathf.Min (8, m_manager.eclipseCasters.Count); i++)
			{
				casterPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (m_manager.eclipseCasters [i].transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor));
				castersMatrix2.SetRow (i - 4, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y, casterPosRelPlanet.z, (float)m_manager.eclipseCasters [i].Radius));
			}
		}

		void UpdatePlanetShine ()
		{
			planetShineRGBMatrix = Matrix4x4.zero;
			planetShineSourcesMatrix = Matrix4x4.zero;
			for (int i = 0; i < Mathf.Min (4, m_manager.planetshineSources.Count); i++)
			{
				Vector3 sourcePosRelPlanet;
				//offset lightsource position to make light follow lit crescent
				//i.e light doesn't come from the center of the planet but follows the lit side
				//1/4 of the way from center to surface should be fine
				Vector3d offsetPos = m_manager.planetshineSources [i].body.position + 0.25 * m_manager.planetshineSources [i].body.Radius * (m_manager.sunCelestialBody.position - m_manager.planetshineSources [i].body.position).normalized;
				sourcePosRelPlanet = Vector3.Scale (offsetPos - m_manager.parentCelestialBody.GetTransform ().position, new Vector3d (ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor));
				planetShineSourcesMatrix.SetRow (i, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y, sourcePosRelPlanet.z, m_manager.planetshineSources [i].isSun ? 1.0f : 0.0f));
				float intensity = m_manager.planetshineSources [i].intensity;
				planetShineRGBMatrix.SetRow (i, new Vector4 (m_manager.planetshineSources [i].color.x, m_manager.planetshineSources [i].color.y, m_manager.planetshineSources [i].color.z, intensity));
			}
		}

		void InitKopernicusRings ()
		{
			ringObject = GameObject.Find (celestialBodyName + "Ring");
			if (ringObject) {
				Utils.LogDebug (" Found ring for " + celestialBodyName);
				Material ringMat = ringObject.GetComponent<MeshRenderer> ().material;
				hasRingObjectAndShadowActivated = true;
				MonoBehaviour[] scripts = (MonoBehaviour[])ringObject.GetComponents<MonoBehaviour> ();
				foreach (MonoBehaviour _script in scripts) {
					if (_script.GetType ().ToString ().Contains ("Ring")) {
						const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
						FieldInfo[] fields = _script.GetType ().GetFields (flags);
						foreach (FieldInfo fi in fields) {
							//Utils.Log("fi.Name "+fi.Name+" fi.GetType() "+fi.GetType());
						}
						try {
							ringTexture = _script.GetType ().GetField ("texture", flags).GetValue (_script) as Texture2D;
							Utils.LogDebug (" ring texture fetch successful");
							Utils.LogDebug (" ringTexture.width " + ringTexture.width.ToString ());
							Utils.LogDebug (" ringTexture.height " + ringTexture.height.ToString ());
							MeshRenderer ringMR = _script.GetType ().GetField ("ringMr", flags).GetValue (_script) as MeshRenderer;
							Utils.LogDebug (" ring MeshRenderer fetch successful");
							ringInnerRadius = ringMR.material.GetFloat ("innerRadius");
							ringOuterRadius = ringMR.material.GetFloat ("outerRadius");
							Utils.LogDebug (" ring innerRadius (with parent scale) " + ringInnerRadius.ToString ());
							Utils.LogDebug (" ring outerRadius (with parent scale) " + ringOuterRadius.ToString ());
							int tiles = (int)_script.GetType ().GetField ("tiles", flags).GetValue (_script);
							if (tiles > 0) {
								throw new Exception ("Scatterer doesn't support tiled/thick Kopernicus rings (not implemented)");
							}
							ringInnerRadius *= ScaledSpace.ScaleFactor;
							ringInnerRadius = Mathf.Max(ringInnerRadius,(float)(m_manager.m_radius)* (1f + 10f/600000f)); //prevent inner ring radius from intersecting planet's radius because that's stupid and it breaks the shader
							ringOuterRadius *= ScaledSpace.ScaleFactor;
						}
						catch (Exception e) {
							Utils.LogError ("Kopernicus ring exception: " + e.ToString ());
							Utils.LogDebug ("Disabling ring shadows for " + celestialBodyName);
							hasRingObjectAndShadowActivated = false;
						}
					}
				}
			}
		}

		public void InitEVEClouds()
		{
			if (!ReferenceEquals (Scatterer.Instance.eveReflectionHandler.EVEinstance, null) && Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey(celestialBodyName))
			{
				try
				{
					// After the shader has been replaced by the modified scatterer shader, the properties are lost and need to be set again
					// Call EVE clouds2D.reassign() method to set the shader properties
					Scatterer.Instance.eveReflectionHandler.invokeClouds2dReassign(celestialBodyName);

					int size = Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName].Count;
					for (int i=0; i<size; i++)
					{
						InitUniforms (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial);
						InitPostprocessMaterialUniforms (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial);

						Utils.EnableOrDisableShaderKeywords (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial, "PRESERVECLOUDCOLORS_ON", "PRESERVECLOUDCOLORS_OFF", EVEIntegration_preserveCloudColors);
						
					}
				}
				catch (Exception stupid)
				{
					Utils.LogError ("Error initiating EVE Clouds on planet: " + celestialBodyName + " Exception returned: " + stupid.ToString ());
				}
			}
		}
		
		IEnumerator DelayedMapEVEVolumetrics()
		{
			mappedVolumetrics = false;
			for (int i=0; i<5; i++)
				yield return new WaitForFixedUpdate ();
			MapEVEVolumetrics();
		}

		public void MapEVEVolumetrics()
		{
			Scatterer.Instance.eveReflectionHandler.mapEVEVolumetrics (celestialBodyName, EVEvolumetrics);

			foreach (Material particleMaterial in EVEvolumetrics)
			{
				particleMaterial.EnableKeyword ("SCATTERER_ON");
				particleMaterial.DisableKeyword ("SCATTERER_OFF");
			
				InitUniforms (particleMaterial);
				InitPostprocessMaterialUniforms (particleMaterial);
			
				Utils.EnableOrDisableShaderKeywords (particleMaterial, "SCATTERER_USE_ORIG_DIR_COLOR_ON", "SCATTERER_USE_ORIG_DIR_COLOR_OFF", Scatterer.Instance.mainSettings.sunlightExtinction);
			}

			mappedVolumetrics = true;
		}

		void UpdateEVECloudMaterials ()
		{
			if (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey (celestialBodyName))
			{
				foreach (EVEClouds2d clouds2d in Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName])
				{
					SetUniforms (clouds2d.Clouds2dMaterial);
					//if (!inScaledSpace)
					UpdatePostProcessMaterialUniforms (clouds2d.Clouds2dMaterial);
					clouds2d.Clouds2dMaterial.SetFloat (ShaderProperties.cloudColorMultiplier_PROPERTY, cloudColorMultiplier);
					clouds2d.Clouds2dMaterial.SetFloat (ShaderProperties.cloudScatteringMultiplier_PROPERTY, cloudScatteringMultiplier);
					clouds2d.Clouds2dMaterial.SetFloat (ShaderProperties.cloudSkyIrradianceMultiplier_PROPERTY, cloudSkyIrradianceMultiplier);
					
					//why is this here? try without it?
					clouds2d.Clouds2dMaterial.EnableKeyword ("SCATTERER_ON");
					clouds2d.Clouds2dMaterial.DisableKeyword ("SCATTERER_OFF");
				}
			}
			
			if (!inScaledSpace && mappedVolumetrics)
			{
				foreach (Material volumetricsMat in EVEvolumetrics)
				{
					//TODO: simplify, take one or the other, doesn't need to be done very frame also
					SetUniforms (volumetricsMat);
					UpdatePostProcessMaterialUniforms (volumetricsMat);
					volumetricsMat.SetVector (ShaderProperties._PlanetWorldPos_PROPERTY, parentLocalTransform.position);
					volumetricsMat.SetFloat (ShaderProperties.cloudColorMultiplier_PROPERTY, volumetricsColorMultiplier);
				}
			}
		}

		public void TogglePreserveCloudColors()
		{
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds)
			{
				if(Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey(celestialBodyName)) //change to a bool hasclouds
				{
					int size = Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName].Count;
					for (int i=0;i<size;i++)
					{
						Utils.EnableOrDisableShaderKeywords (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary[celestialBodyName][i].Clouds2dMaterial, "PRESERVECLOUDCOLORS_OFF", "PRESERVECLOUDCOLORS_ON", EVEIntegration_preserveCloudColors);
					}
				}
				EVEIntegration_preserveCloudColors =!EVEIntegration_preserveCloudColors;
			}
		}

		public void SetCelestialBodyName(string name) {
			celestialBodyName = name;
		}
		
		public void SetParentScaledTransform(Transform parentTransform) {
			parentScaledTransform = parentTransform;
		}
		
		public void SetParentLocalTransform(Transform parentTransform) {
			parentLocalTransform = parentTransform;
		}

		//to be called on loss of rendertextures, ie alt-enter
		public void ReInitMaterialUniformsOnRenderTexturesLoss()
		{
			if (!ReferenceEquals (localScatteringContainer, null))
			{
				InitPostprocessMaterialUniforms (localScatteringContainer.material);
			}
		}	
	}
}
