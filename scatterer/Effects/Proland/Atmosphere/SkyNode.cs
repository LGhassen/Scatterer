using UnityEngine;
using System;
using System.IO;
using System.Collections;
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

		public float Rg;	//The radius of the planet
		public float Rt;	//Radius of the atmosphere, calculated automatically from HR and HM and the scattering factors
		[Persistent] public float atmosphereStartRadiusScale = 1f;
		[Persistent] public float HR = 8.0f;	//Half heights for the atmosphere air density (HR) and particle density (HM), this is the height in km that half the particles are found below
		[Persistent] public float HM = 1.2f;
		[Persistent] public Vector3 m_betaR = new Vector3 (5.8e-3f, 1.35e-2f, 3.31e-2f);
		[Persistent] public Vector3 BETA_MSca = new Vector3 (4e-3f, 4e-3f, 4e-3f); //scatter coefficient for mie
		[Persistent] public float m_mieG = 0.85f; //Asymmetry factor for the mie phase function, a higher number means more light is scattered in the forward direction
		[Persistent] public float averageGroundReflectance = 0.1f;
		[Persistent] public bool multipleScattering = true;
		public bool previewMode = false;

		public float mainMenuScaleFactor = 1f;

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

		[Persistent] public bool adjustScaledTexture = false;

		[Persistent] public float scaledLandBrightnessAdjust = 1f;
		[Persistent] public float scaledLandContrastAdjust   = 1f;
		[Persistent] public float scaledLandSaturationAdjust = 1f;
				
		[Persistent] public float scaledOceanBrightnessAdjust = 1f;
		[Persistent] public float scaledOceanContrastAdjust   = 1f;
		[Persistent] public float scaledOceanSaturationAdjust = 1f;

		Texture3D m_inscatter;
		Texture2D m_irradiance;

		//Dimensions of the tables
		const int TRANSMITTANCE_W = 256;
		const int TRANSMITTANCE_H = 64;
		const int SKY_W = 64;
		const int SKY_H = 16;

		Vector4 PRECOMPUTED_SCTR_LUT_DIM = AtmoPreprocessor.PRECOMPUTED_SCTR_LUT_DIM_DEFAULT;

		string celestialBodyName;
		public Transform parentScaledTransform, parentLocalTransform;

		public ProlandManager prolandManager;
		public UrlDir.UrlConfig configUrl;
		public bool isConfigModuleManagerPatch = true;
		
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

		SkySphereContainer skySphere;
		GameObject stockSkyGameObject;
		MeshRenderer stockScaledPlanetMeshRenderer;
		Mesh originalScaledMesh, tweakedScaledmesh;
		public ScaledScatteringContainer scaledScatteringContainer;
		public Material localScatteringMaterial, skyMaterial, scaledScatteringMaterial, sunflareExtinctionMaterial, scaledEclipseMaterial;
		public GenericLocalAtmosphereContainer localScatteringContainer;
		public GodraysRenderer godraysRenderer;
		public bool postprocessingEnabled = true;

		GameObject ringObject;
		float ringInnerRadius, ringOuterRadius;
		Texture2D ringTexture;
		bool hasRingObjectAndShadowActivated = false;
	
		bool skyNodeInitiated = false;
		public bool useEclipses = false;

		Texture originalPlanetTexture;
		RenderTexture adjustedPlanetTexture;

		public void Init ()
		{
			InitPrecomputedAtmo ();

			skyMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/SkySphere")]);
			scaledScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/ScaledPlanetScattering")]);

			if (Scatterer.Instance.mainSettings.useDepthBufferMode)
				localScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthBufferScattering")]);
			else
				localScatteringMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);

			skyMaterial.SetOverrideTag ("IgnoreProjector", "True");
			scaledScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");
			localScatteringMaterial.SetOverrideTag ("IgnoreProjector", "True");

			useEclipses = Scatterer.Instance.mainSettings.useEclipses && (prolandManager.eclipseCasters.Count > 0) && HighLogic.LoadedScene != GameScenes.MAINMENU ; //disable bugged eclipses on main menu
			Utils.EnableOrDisableShaderKeywords (localScatteringMaterial, "ECLIPSES_ON", "ECLIPSES_OFF", useEclipses);
			Utils.EnableOrDisableShaderKeywords (localScatteringMaterial, "DISABLE_UNDERWATER_ON", "DISABLE_UNDERWATER_OFF", prolandManager.hasOcean);

			if (Scatterer.Instance.mainSettings.useGodrays && Scatterer.Instance.unifiedCameraMode && !ReferenceEquals (prolandManager.parentCelestialBody.pqsController, null)
			    && Scatterer.Instance.mainSettings.terrainShadows && (Scatterer.Instance.mainSettings.unifiedCamShadowResolutionOverride != 0))
			{
				godraysRenderer = (GodraysRenderer) Utils.getEarliestLocalCamera().gameObject.AddComponent (typeof(GodraysRenderer));
				if (!godraysRenderer.Init(prolandManager.mainSunLight, this))
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

			if (!ReferenceEquals (prolandManager.parentCelestialBody.pqsController, null))
			{
				prolandManager.parentCelestialBody.pqsController.isActive = false; 	//sometimes the PQS is forgotten as "active" if a ship is loaded directly around another body, this would mess with the mod
																				//this sets it to false, if it's really active it will be set to active automatically. EVE mod seems also to have a fix for this
			}

			if ((HighLogic.LoadedScene != GameScenes.MAINMENU) && (HighLogic.LoadedScene != GameScenes.TRACKSTATION)) // &&useLocalScattering
			{
				if (Scatterer.Instance.mainSettings.useDepthBufferMode)
					localScatteringContainer = new ScreenSpaceScatteringContainer(localScatteringMaterial, parentLocalTransform, Rt, prolandManager, Scatterer.Instance.mainSettings.quarterResScattering && ReferenceEquals (godraysRenderer, null));
				else
					localScatteringContainer = new AtmosphereProjectorContainer (localScatteringMaterial, parentLocalTransform, Rt, prolandManager);

			}

			if (Scatterer.Instance.mainSettings.fullLensFlareReplacement)
			{
				sunflareExtinctionMaterial = new Material (ShaderReplacer.Instance.LoadedShaders ["Scatterer/sunFlareExtinction"]);
				InitUniforms(sunflareExtinctionMaterial);

				if (hasRingObjectAndShadowActivated)
				{
					sunflareExtinctionMaterial.SetFloat (ShaderProperties.ringInnerRadius_PROPERTY, ringInnerRadius);
					sunflareExtinctionMaterial.SetFloat (ShaderProperties.ringOuterRadius_PROPERTY, ringOuterRadius);
					sunflareExtinctionMaterial.SetVector (ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
					sunflareExtinctionMaterial.SetTexture (ShaderProperties.ringTexture_PROPERTY, ringTexture);
				}

				Utils.EnableOrDisableShaderKeywords (sunflareExtinctionMaterial, "DISABLE_UNDERWATER_ON", "DISABLE_UNDERWATER_OFF", prolandManager.hasOcean);
			}

			if (useEclipses || hasRingObjectAndShadowActivated)
			{
				scaledEclipseMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/ScaledPlanetEclipse")]);
				scaledEclipseMaterial.renderQueue = 2001;
				
				Utils.EnableOrDisableShaderKeywords (scaledEclipseMaterial, "ECLIPSES_ON", "ECLIPSES_OFF", useEclipses);
				Utils.EnableOrDisableShaderKeywords (scaledEclipseMaterial, "RINGSHADOW_ON", "RINGSHADOW_OFF", hasRingObjectAndShadowActivated);
				
				InitUniforms(scaledEclipseMaterial);
			}

			stockScaledPlanetMeshRenderer = (MeshRenderer) parentScaledTransform.GetComponent<MeshRenderer>();

			try {StartCoroutine(DelayedTweakStockPlanet ());}
			catch (Exception e){Utils.LogError("Error when starting SkyNode::DelayedTweakStockPlanet coroutine "+e.Message);};

			InitEVEClouds ();
			
			skyNodeInitiated = true;
			Utils.LogDebug("Skynode initiated for "+celestialBodyName);
		}

		void InitSkySphere ()
		{
			float skySphereSize = (1.5f * (Rt - Rg*atmosphereStartRadiusScale) + Rg*atmosphereStartRadiusScale) / ScaledSpace.ScaleFactor;
			skySphere = new SkySphereContainer (skySphereSize, skyMaterial, parentLocalTransform, parentScaledTransform);

			if (HighLogic.LoadedScene != GameScenes.MAINMENU)
			{
				if (prolandManager.parentCelestialBody.pqsController != null && prolandManager.parentCelestialBody.pqsController.isActive && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
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
				if (prolandManager.parentCelestialBody.pqsController != null && prolandManager.parentCelestialBody.pqsController.isActive && HighLogic.LoadedScene != GameScenes.TRACKSTATION)
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
				UpdateLightExtinctions ();
			}
		}

		public void UpdateGraphicsUniforms()
		{
			if (!inScaledSpace && !MapView.MapIsEnabled && postprocessingEnabled && !ReferenceEquals(localScatteringContainer,null))
			{
				UpdatePostProcessMaterialUniforms (localScatteringContainer.material);
			}
			if (useEclipses)
			{
				UpdateEclipseCasters ();
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

				if (prolandManager.parentCelestialBody.pqsController != null && !Scatterer.Instance.mainSettings.useDepthBufferMode)
				{
					float planetOpactiy = prolandManager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY);
					localScatteringMaterial.SetInt (ShaderProperties._ZwriteVariable_PROPERTY, (planetOpactiy > 0f) ? 1 : 0);
				}
			}

			SetUniforms (skyMaterial);
			SetUniforms (scaledScatteringMaterial);

			if (!ReferenceEquals (sunflareExtinctionMaterial, null))
				SetUniforms (sunflareExtinctionMaterial);

			if (!ReferenceEquals (scaledEclipseMaterial, null))
			{
				scaledEclipseMaterial.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, prolandManager.getDirectionToMainSun ());
				
				scaledEclipseMaterial.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				scaledEclipseMaterial.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
				scaledEclipseMaterial.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                                         sunPosRelPlanet.z, (float)prolandManager.sunCelestialBody.Radius));
			}
		}
		
		
		public void UpdateNode ()
		{
			if ((prolandManager.parentCelestialBody.pqsController != null) && !(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				bool previousState = inScaledSpace;

				inScaledSpace = !prolandManager.parentCelestialBody.pqsController.isActive;

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
					simulateOceanInteraction = prolandManager.parentCelestialBody.pqsController.isActive;
			}
			else
			{
				inScaledSpace = true;
				simulateOceanInteraction = false;
			}

			if (skyNodeInitiated)
			{
				InterpolateVariables ();

				if (prolandManager.hasOcean && !Scatterer.Instance.mainSettings.useOceanShaders)
				{
					skySphere.MeshRenderer.enabled = (altitude>=0f);
					stockSkyGameObject.SetActive(altitude<0f); //re-enable stock sky meshrenderer, for compatibility with stock underwater effect
				}
			}
		}

		public void SwitchEffectsScaled()
		{
			Utils.LogInfo ("Skynode switch effects to scaled mode: "+prolandManager.parentCelestialBody.name);

			if (!ReferenceEquals(skySphere,null))
				skySphere.SwitchScaledMode ();
			if (!ReferenceEquals(scaledScatteringContainer,null))
				scaledScatteringContainer.SwitchScaledMode ();
			if (!ReferenceEquals(localScatteringContainer,null))
				localScatteringContainer.setActivated(false);
			EVEvolumetrics.Clear();
		}

		public void SwitchEffectsLocal()
		{
			Utils.LogInfo ("Skynode switch effects to local mode "+prolandManager.parentCelestialBody.name);

			if (!ReferenceEquals(skySphere,null))
				skySphere.SwitchLocalMode();
			if (!ReferenceEquals(scaledScatteringContainer,null))
				scaledScatteringContainer.SwitchLocalMode ();
			if (!ReferenceEquals(localScatteringContainer,null))
				localScatteringContainer.setActivated(true);

			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && usesCloudIntegration)
			{
				//really strange but when changing scenes StartCoroutine can return a nullref, even though I check all references
				try {StartCoroutine(DelayedMapEVEVolumetrics());}
				catch (Exception){}
			}
		}
		
		public void SetUniforms (Material mat)
		{
			mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);
			mat.SetFloat (ShaderProperties._Extinction_Tint_PROPERTY, interpolatedSettings.skyExtinctionTint);
			mat.SetFloat (ShaderProperties.extinctionTint_PROPERTY, interpolatedSettings.extinctionTint); //extinctionTint for scaled+local
			mat.SetFloat (ShaderProperties.extinctionThickness_PROPERTY, interpolatedSettings.extinctionThickness);

			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg*atmosphereStartRadiusScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt);

			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, prolandManager.getDirectionToMainSun ());

			mat.SetFloat (ShaderProperties._SkyExposure_PROPERTY, interpolatedSettings.skyExposure);
			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);

			if (useEclipses)
			{
				mat.SetMatrix (ShaderProperties.lightOccluders1_PROPERTY, castersMatrix1);
				mat.SetMatrix (ShaderProperties.lightOccluders2_PROPERTY, castersMatrix2);
				mat.SetVector (ShaderProperties.sunPosAndRadius_PROPERTY, new Vector4 (sunPosRelPlanet.x, sunPosRelPlanet.y,
				                                                                       sunPosRelPlanet.z, (float)prolandManager.sunCelestialBody.Radius));
			}
			if ((prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine)
			{
				mat.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, prolandManager.planetShineSourcesMatrix);
				mat.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, prolandManager.planetShineRGBMatrix);
			}

			if (hasRingObjectAndShadowActivated)
			{
				mat.SetVector(ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
			}

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetFloat(ShaderProperties._godrayStrength_PROPERTY, godrayStrength);
			}

			mat.SetColor (ShaderProperties._sunColor_PROPERTY, prolandManager.getIntensityModulatedSunColor());
		}
		
		
		public void SetOceanUniforms (Material mat)
		{
			if (mat == null)
				return;

			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);

			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg*atmosphereStartRadiusScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			mat.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, prolandManager.getDirectionToMainSun ());
			mat.SetVector(ShaderProperties._camForward_PROPERTY, Scatterer.Instance.nearCamera.transform.forward);

			mat.SetFloat (ShaderProperties._Alpha_Global_PROPERTY, interpolatedSettings.skyAlpha);			
			mat.SetFloat (ShaderProperties._SkyExposure_PROPERTY, interpolatedSettings.skyExposure);

			UpdatePostProcessMaterialUniforms (mat);
		}
		

		public void InitPostprocessMaterialUniforms (Material mat)
		{
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));

			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);

			mat.SetFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg*atmosphereStartRadiusScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt);
			mat.SetVector("PRECOMPUTED_SCTR_LUT_DIM", PRECOMPUTED_SCTR_LUT_DIM);
			mat.SetFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			mat.SetFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			
			mat.SetVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f / mainMenuScaleFactor);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f / mainMenuScaleFactor);
			mat.SetVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f / mainMenuScaleFactor) / 0.9f);
			
			mat.SetFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f * mainMenuScaleFactor);
			mat.SetFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f * mainMenuScaleFactor);
			
			
			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, prolandManager.getDirectionToMainSun());

			Utils.EnableOrDisableShaderKeywords (mat, "PLANETSHINE_ON", "PLANETSHINE_OFF", (prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine);

			//When using custom ocean shaders, we don't reuse the ocean mesh to render scattering separately: Instead ocean shader handles scattering internally
			//When the ocean starts fading out when transitioning to orbit, ocean shader stops doing scattering, and stops writing to z-buffer
			//The ocean floor vertexes are then used by the scattering shader, moving them to the surface to render scattering, this is not needed for stock ocean so disable it
			Utils.EnableOrDisableShaderKeywords (mat, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", Scatterer.Instance.mainSettings.useOceanShaders && prolandManager.hasOcean);

			Utils.EnableOrDisableShaderKeywords (mat, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);

			if (prolandManager.flatScaledSpaceModel && prolandManager.parentCelestialBody.pqsController)
				mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 0f);
			else
				mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 1f);

			mat.SetColor (ShaderProperties._sunColor_PROPERTY, prolandManager.getIntensityModulatedSunColor());
			mat.SetColor (ShaderProperties.cloudSunColor_PROPERTY, prolandManager.cloudIntegrationUsesScattererSunColors ? prolandManager.getIntensityModulatedSunColor() : prolandManager.mainScaledSunLight.color * prolandManager.mainScaledSunLight.intensity );

			float camerasOverlap = 0f;
			if (!Scatterer.Instance.unifiedCameraMode)
				camerasOverlap = Scatterer.Instance.nearCamera.farClipPlane - Scatterer.Instance.farCamera.nearClipPlane;

			mat.SetFloat(ShaderProperties._ScattererCameraOverlap_PROPERTY,camerasOverlap);

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetTexture(ShaderProperties._godrayDepthTexture_PROPERTY,godraysRenderer.volumeDepthTexture);
			}
			Utils.EnableOrDisableShaderKeywords (mat, "GODRAYS_ON", "GODRAYS_OFF", !ReferenceEquals (godraysRenderer, null));

			mat.SetInt ("TONEMAPPING_MODE", Scatterer.Instance.mainSettings.scatteringTonemapper);
		}

		
		public void UpdatePostProcessMaterialUniforms (Material mat)
		{
			mat.SetFloat (ShaderProperties._global_alpha_PROPERTY, interpolatedSettings.postProcessAlpha);
			mat.SetFloat (ShaderProperties._ScatteringExposure_PROPERTY, interpolatedSettings.scatteringExposure);
			mat.SetFloat (ShaderProperties._global_depth_PROPERTY, interpolatedSettings.postProcessDepth *1000000);

			if (prolandManager.flatScaledSpaceModel && prolandManager.parentCelestialBody.pqsController)
			{
				if (MapView.MapIsEnabled)
					mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 0f);
				else
					mat.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, 1f - prolandManager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY));
			}

			mat.SetFloat (ShaderProperties._Post_Extinction_Tint_PROPERTY, interpolatedSettings.extinctionTint);
			mat.SetFloat (ShaderProperties.extinctionThickness_PROPERTY, interpolatedSettings.extinctionThickness);

			mat.SetVector (ShaderProperties.SUN_DIR_PROPERTY, prolandManager.getDirectionToMainSun ());
			mat.SetVector (ShaderProperties._planetPos_PROPERTY, parentLocalTransform.position);

			mat.SetColor (ShaderProperties.cloudSunColor_PROPERTY, prolandManager.cloudIntegrationUsesScattererSunColors ? prolandManager.getIntensityModulatedSunColor() : prolandManager.mainScaledSunLight.color * prolandManager.mainScaledSunLight.intensity );

			if ((prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine)
			{
				mat.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, prolandManager.planetShineSourcesMatrix);
				mat.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, prolandManager.planetShineRGBMatrix);
				mat.SetMatrix (ShaderProperties.cloudPlanetShineRGB_PROPERTY, prolandManager.cloudIntegrationUsesScattererSunColors ?  prolandManager.planetShineRGBMatrix : prolandManager.planetShineOriginalRGBMatrix );
			}

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetFloat(ShaderProperties._godrayStrength_PROPERTY, godrayStrength);
			}

			mat.SetColor (ShaderProperties._sunColor_PROPERTY, prolandManager.getIntensityModulatedSunColor());
		}

		public void InitUniforms (Material mat)
		{
			if (mat == null)
				return;
			
			mat.SetFloat (ShaderProperties.M_PI_PROPERTY, Mathf.PI);
			mat.SetFloat (ShaderProperties.mieG_PROPERTY, Mathf.Clamp (m_mieG, 0.0f, 0.99f));
			
			mat.SetVector (ShaderProperties.betaR_PROPERTY, m_betaR / 1000.0f / mainMenuScaleFactor);
			mat.SetTexture (ShaderProperties._Inscatter_PROPERTY, m_inscatter);
			mat.SetTexture (ShaderProperties._Irradiance_PROPERTY, m_irradiance);
			mat.SetFloat (ShaderProperties.Rg_PROPERTY, Rg*atmosphereStartRadiusScale);
			mat.SetFloat (ShaderProperties.Rt_PROPERTY, Rt);
			
			mat.SetFloat (ShaderProperties.TRANSMITTANCE_W_PROPERTY, TRANSMITTANCE_W);
			mat.SetFloat (ShaderProperties.TRANSMITTANCE_H_PROPERTY, TRANSMITTANCE_H);
			mat.SetFloat (ShaderProperties.SKY_W_PROPERTY, SKY_W);
			mat.SetFloat (ShaderProperties.SKY_H_PROPERTY, SKY_H);
			mat.SetVector("PRECOMPUTED_SCTR_LUT_DIM", PRECOMPUTED_SCTR_LUT_DIM);
			mat.SetFloat (ShaderProperties.HR_PROPERTY, HR * 1000.0f * mainMenuScaleFactor);
			mat.SetFloat (ShaderProperties.HM_PROPERTY, HM * 1000.0f * mainMenuScaleFactor);
			mat.SetVector (ShaderProperties.betaMSca_PROPERTY, BETA_MSca / 1000.0f / mainMenuScaleFactor);
			mat.SetVector (ShaderProperties.betaMEx_PROPERTY, (BETA_MSca / 1000.0f / mainMenuScaleFactor) / 0.9f);

			if (hasRingObjectAndShadowActivated)
			{
				Utils.EnableOrDisableShaderKeywords (mat, "RINGSHADOW_ON", "RINGSHADOW_OFF", true);
				mat.SetFloat ("useRingShadow", 1f);
				mat.SetFloat (ShaderProperties.ringInnerRadius_PROPERTY, ringInnerRadius);
				mat.SetFloat (ShaderProperties.ringOuterRadius_PROPERTY, ringOuterRadius);
				mat.SetVector (ShaderProperties.ringNormal_PROPERTY, ringObject.transform.up);
				mat.SetTexture (ShaderProperties.ringTexture_PROPERTY, ringTexture);
			}
			else
			{
				Utils.EnableOrDisableShaderKeywords (mat, "RINGSHADOW_ON", "RINGSHADOW_OFF", false);
				mat.SetFloat ("useRingShadow", 0f);
			}

			Utils.EnableOrDisableShaderKeywords (mat, "ECLIPSES_ON", "ECLIPSES_OFF", useEclipses );
			mat.SetFloat ("useEclipses", useEclipses ? 1f : 0f);

			Utils.EnableOrDisableShaderKeywords (mat, "PLANETSHINE_ON", "PLANETSHINE_OFF", (prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine);
			Utils.EnableOrDisableShaderKeywords (mat, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);

			mat.SetFloat (ShaderProperties.flatScaledSpaceModel_PROPERTY, prolandManager.flatScaledSpaceModel ? 1f : 0f );
			mat.SetColor (ShaderProperties._sunColor_PROPERTY, prolandManager.getIntensityModulatedSunColor());
			mat.SetColor (ShaderProperties.cloudSunColor_PROPERTY, prolandManager.cloudIntegrationUsesScattererSunColors ? prolandManager.getIntensityModulatedSunColor() : prolandManager.mainScaledSunLight.color * prolandManager.mainScaledSunLight.intensity);

			if (!ReferenceEquals (godraysRenderer, null))
			{
				mat.SetTexture(ShaderProperties._godrayDepthTexture_PROPERTY,godraysRenderer.volumeDepthTexture);
			}
			Utils.EnableOrDisableShaderKeywords (mat, "GODRAYS_ON", "GODRAYS_OFF", !ReferenceEquals (godraysRenderer, null));

			mat.SetInt ("TONEMAPPING_MODE", Scatterer.Instance.mainSettings.scatteringTonemapper);
		}

		public void TogglePostProcessing()
		{
			postprocessingEnabled = !postprocessingEnabled;
		}
		
		void InitPrecomputedAtmo ()
		{
			Rg = (float) prolandManager.GetRadius ();
			Rt = AtmoPreprocessor.CalculateRt (Rg*atmosphereStartRadiusScale, HR*mainMenuScaleFactor, HM*mainMenuScaleFactor, m_betaR/mainMenuScaleFactor, BETA_MSca/mainMenuScaleFactor);

			//Inscatter is responsible for the change in the sky color as the sun moves. The raw file is a 4D array of 32 bit floats with a range of 0 to 1.589844
			//As there is not such thing as a 4D texture the data is packed into a 3D texture and the shader manually performs the sample for the 4th dimension
			m_inscatter = new Texture3D((int)(PRECOMPUTED_SCTR_LUT_DIM.x), (int)(PRECOMPUTED_SCTR_LUT_DIM.y), (int)(PRECOMPUTED_SCTR_LUT_DIM.z * PRECOMPUTED_SCTR_LUT_DIM.w), TextureFormat.RGBAHalf, false);
			m_inscatter.wrapMode = TextureWrapMode.Clamp;
			m_inscatter.filterMode = FilterMode.Bilinear;
			
			//Irradiance is responsible for the change in light emitted from the sky as the sun moves. The raw file is a 2D array of 32 bit floats with a range of 0 to 1
			m_irradiance = new Texture2D (SKY_W, SKY_H, TextureFormat.RGBAHalf,false);
			m_irradiance.wrapMode = TextureWrapMode.Clamp;
			m_irradiance.filterMode = FilterMode.Bilinear;

			//Compute atmo hash and path
			string cachePath = Utils.GameDataPath + "/ScattererAtmosphereCache/PluginData";
			float originalRt = AtmoPreprocessor.CalculateRt ((float) prolandManager.parentCelestialBody.Radius * atmosphereStartRadiusScale, HR, HM, m_betaR, BETA_MSca);
			string atmohash = AtmoPreprocessor.GetAtmoHash((float) prolandManager.parentCelestialBody.Radius * atmosphereStartRadiusScale, originalRt, m_betaR, BETA_MSca, m_mieG, HR, HM, averageGroundReflectance, multipleScattering, PRECOMPUTED_SCTR_LUT_DIM);
			cachePath += "/" + atmohash;

			string inscatterPath = cachePath+"/inscatter.half";
			string irradiancePath = cachePath+"/irradiance.half";

			if (!System.IO.File.Exists (inscatterPath) || !System.IO.File.Exists (irradiancePath))
			{
				Utils.LogInfo("No atmosphere cache for "+prolandManager.parentCelestialBody.name+", generating new atmosphere");
				AtmoPreprocessor.Instance.Generate ((float) prolandManager.parentCelestialBody.Radius * atmosphereStartRadiusScale, originalRt, m_betaR, BETA_MSca, m_mieG, HR, HM, averageGroundReflectance, multipleScattering, PRECOMPUTED_SCTR_LUT_DIM, cachePath);
			}

			m_inscatter.SetPixelData (System.IO.File.ReadAllBytes (inscatterPath),0,0);
			m_irradiance.SetPixelData  (System.IO.File.ReadAllBytes (irradiancePath),0,0);

			m_inscatter.Apply();
			m_irradiance.Apply ();
		}

		public void ApplyAtmoFromUI(Vector4 inBETA_R, Vector4 inBETA_MSca, float inMIE_G, float inHR, float inHM, float inGRref, bool inMultiple, bool fastPreviewMode, float inAtmosphereStartRadiusScale)
		{
			m_betaR = inBETA_R;
			BETA_MSca = inBETA_MSca;
			m_mieG = inMIE_G;
			HR = inHR;
			HM = inHM;
			averageGroundReflectance = inGRref;
			multipleScattering = inMultiple;
			PRECOMPUTED_SCTR_LUT_DIM = fastPreviewMode ? AtmoPreprocessor.PRECOMPUTED_SCTR_LUT_DIM_PREVIEW : AtmoPreprocessor.PRECOMPUTED_SCTR_LUT_DIM_DEFAULT ;
			atmosphereStartRadiusScale = inAtmosphereStartRadiusScale;

			InitPrecomputedAtmo ();

			float skySphereSize = 2 * (4 * (Rt - Rg*atmosphereStartRadiusScale) + Rg*atmosphereStartRadiusScale) / ScaledSpace.ScaleFactor;
			skySphere.Resize (skySphereSize);

			ReinitAllMaterials();
		}

		void ReinitAllMaterials()
		{
			if (!ReferenceEquals(scaledEclipseMaterial,null))
				InitUniforms(scaledEclipseMaterial);
			if (!ReferenceEquals(skyMaterial,null))	//need also here to resize the sky sphere thingy
				InitUniforms(skyMaterial);
			if (!ReferenceEquals(scaledScatteringMaterial,null))
				InitUniforms(scaledScatteringMaterial);
			if (!ReferenceEquals(sunflareExtinctionMaterial,null))
				InitUniforms(sunflareExtinctionMaterial);
			if (!ReferenceEquals(localScatteringMaterial,null))
				InitPostprocessMaterialUniforms(localScatteringMaterial);

			ReInitMaterialUniformsOnRenderTexturesLoss ();

			foreach (Material particleMaterial in EVEvolumetrics)
			{	
				InitUniforms (particleMaterial);
				InitPostprocessMaterialUniforms (particleMaterial);
			}

			if (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary.ContainsKey(celestialBodyName))
			{
				foreach (EVEClouds2d eveClouds2d in Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName])
				{
					InitUniforms (eveClouds2d.Clouds2dMaterial);
					InitPostprocessMaterialUniforms (eveClouds2d.Clouds2dMaterial);
				}
			}

			if (!ReferenceEquals(prolandManager.GetOceanNode(),null))
			{
				if (!ReferenceEquals(prolandManager.GetOceanNode().m_oceanMaterial,null))
				{
					InitUniforms (prolandManager.GetOceanNode().m_oceanMaterial);
					InitPostprocessMaterialUniforms (prolandManager.GetOceanNode().m_oceanMaterial);
				}

				if (!ReferenceEquals(prolandManager.GetOceanNode().underwaterMaterial,null))
				{
					InitPostprocessMaterialUniforms (prolandManager.GetOceanNode().underwaterMaterial);
				}
			}
		}

		//Also try to make this stay on scene changes and unload/reloads?
		//void ReInitAtmoFromUI()
		//{
		//
		//}
		
		public void Cleanup ()
		{
			try {StopAllCoroutines ();}
			catch (Exception){}

			if (Scatterer.Instance.mainSettings.autosavePlanetSettingsOnSceneChange && !isConfigModuleManagerPatch)
			{
				SaveToConfigNode ();
			}

			if (m_irradiance)
			{
				UnityEngine.Object.DestroyImmediate (m_irradiance);
			}

			if (m_inscatter)
			{
				UnityEngine.Object.DestroyImmediate (m_inscatter);
			}

			if (!ReferenceEquals (skySphere, null))
			{
				skySphere.Cleanup ();
			}

			if (!ReferenceEquals (scaledScatteringContainer, null))
			{
				scaledScatteringContainer.Cleanup ();
			}

			if (!ReferenceEquals (localScatteringContainer, null))
			{
				localScatteringContainer.OnDestroy();
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

			RestoreStockScaledTexture ();
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
			
				Rg = (float) prolandManager.GetRadius ();
				Rt = AtmoPreprocessor.CalculateRt (Rg * atmosphereStartRadiusScale, HR*mainMenuScaleFactor, HM*mainMenuScaleFactor, m_betaR/mainMenuScaleFactor, BETA_MSca/mainMenuScaleFactor);

				godrayStrength = Mathf.Min(godrayStrength,1.0f);

				//compare parentConfigNode with the one on disk to determine if it's a ModuleManager Patch
				string parentConfigNodePath = Utils.GameDataPath + configUrl.parent.url +".cfg";
				if (System.IO.File.Exists(parentConfigNodePath))
				{
					ConfigNode cnParentFromFile = ConfigNode.Load(Utils.GameDataPath + configUrl.parent.url +".cfg");

					if (cnParentFromFile.HasNode("Scatterer_atmosphere"))
					{
						int comparisonResult = configUrl.config.ToString().CompareTo(cnParentFromFile.GetNode("Scatterer_atmosphere").ToString());
						isConfigModuleManagerPatch = (comparisonResult != 0);
					}
				}
			}
			else
			{
				Utils.LogError(" Atmosphere config not found for: "+celestialBodyName);
				Utils.LogDebug(" Removing "+celestialBodyName +" from planets list");

				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Remove(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == celestialBodyName));

				prolandManager.OnDestroy();
				UnityEngine.Object.Destroy (prolandManager);
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

		void DisableStockSky ()
		{
			for (int i = 0; i < parentScaledTransform.childCount; i++) {
				if (parentScaledTransform.GetChild (i).gameObject.layer == 9) {
					if (parentScaledTransform.GetChild (i).gameObject.name == "Atmosphere") {
						stockSkyGameObject = parentScaledTransform.GetChild (i).gameObject;
						stockSkyGameObject.SetActive (false);
						break;
					}
				}
			}
		}

		// Have to delay this until Kopernicus loads the on-demand textures
		IEnumerator DelayedTweakStockPlanet()
		{
			while (true)
			{
				if (!ReferenceEquals(stockScaledPlanetMeshRenderer.sharedMaterial.GetTexture("_MainTex"),null))
				{
					if (adjustScaledTexture)
						TweakStockScaledTexture ();
					
					TweakStockAtmosphere ();

					yield return StartCoroutine(CheckOnDemandUnload());	//once loaded, wait for it to unload and resume checking again
				}

				yield return new WaitForSeconds(1f);
			}
		}

		// Check if kopernicus on demand unloads the planet, then we have to start waiting for it to load it again
		IEnumerator CheckOnDemandUnload()
		{
			while (true)
			{
				if (ReferenceEquals(stockScaledPlanetMeshRenderer.sharedMaterial.GetTexture("_MainTex"),null))
					break;

				yield return new WaitForSeconds(3f);
			}
		}

		public void TweakStockAtmosphere ()	//move to utils/scaledUtils etc
		{
			DisableStockSky ();
			List<Material> materials = new List<Material>(stockScaledPlanetMeshRenderer.sharedMaterials);
			materials.RemoveAll(x => x.shader.name == "Scatterer/ScaledPlanetEclipse");

			if (useEclipses)
			{
				// Split the main pass and the additive light passes into separate materials with different renderqueues so we can inject eclipses after the first pass, and make it apply only to the main pass
				if (ReferenceEquals(parentScaledTransform.GetComponent<PlanetSecondaryLightUpdater>(),null))
				{
					materials.ElementAt(0).SetShaderPassEnabled("ForwardBase", true);
					materials.ElementAt(0).SetShaderPassEnabled("ForwardAdd", false);

					materials.Add(Material.Instantiate(materials.ElementAt(0)));

					materials.ElementAt(1).CopyPropertiesFromMaterial(materials.ElementAt(0));
					materials.ElementAt(1).SetShaderPassEnabled("ForwardBase", false);
					materials.ElementAt(1).SetShaderPassEnabled("ForwardAdd", true);
					materials.ElementAt(1).renderQueue = 2002;

					PlanetSecondaryLightUpdater secondaryLightUpdater = parentScaledTransform.gameObject.AddComponent<PlanetSecondaryLightUpdater>();
					secondaryLightUpdater.Init(materials.ElementAt(0), materials.ElementAt(1));
				}

				materials.Add(scaledEclipseMaterial);
			}

			stockScaledPlanetMeshRenderer.sharedMaterials = materials.ToArray ();

			foreach (Material sharedMaterial in materials)
			{
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
			}

			if (!ReferenceEquals (prolandManager.parentCelestialBody.pqsController, null))
			{
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.surfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.fallbackMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.lowQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.mediumQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.highQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
				Utils.EnableOrDisableShaderKeywords(prolandManager.parentCelestialBody.pqsController.ultraQualitySurfaceMaterial,"AERIAL_ON", "AERIAL_OFF", false);
			}
		}

		public void TweakStockScaledTexture () 	//move to utils/scaledUtils etc
		{
			List<Material> materials = new List<Material>(stockScaledPlanetMeshRenderer.sharedMaterials);
			
			foreach (Material sharedMaterial in materials)
			{	
				if (sharedMaterial.shader.name.Contains("Terrain/Scaled Planet (RimAerial)"))
				{
					if (ReferenceEquals(originalPlanetTexture,null))
						originalPlanetTexture = sharedMaterial.GetTexture("_MainTex");

					if (!ReferenceEquals(originalPlanetTexture,null))
					{
						if (ReferenceEquals(adjustedPlanetTexture,null))
						{
							adjustedPlanetTexture = new RenderTexture(originalPlanetTexture.width, originalPlanetTexture.height, 0, RenderTextureFormat.ARGB32);
							adjustedPlanetTexture.name = "ScattererAdjustedPlanetMap";
							adjustedPlanetTexture.autoGenerateMips = true;
							adjustedPlanetTexture.Create();
						}

						Material imageAdjustMat = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/ScaledTextureAdjust")]);
						imageAdjustMat.SetTexture("inputTexture", originalPlanetTexture);
						
						imageAdjustMat.SetFloat("_scaledLandBrightnessAdjust", scaledLandBrightnessAdjust);
						imageAdjustMat.SetFloat("_scaledLandContrastAdjust",   scaledLandContrastAdjust);
						imageAdjustMat.SetFloat("_scaledLandSaturationAdjust", scaledLandSaturationAdjust);

						imageAdjustMat.SetFloat("_scaledOceanBrightnessAdjust", scaledOceanBrightnessAdjust);
						imageAdjustMat.SetFloat("_scaledOceanContrastAdjust",   scaledOceanContrastAdjust);
						imageAdjustMat.SetFloat("_scaledOceanSaturationAdjust", scaledOceanSaturationAdjust);

						Graphics.Blit(originalPlanetTexture, adjustedPlanetTexture, imageAdjustMat);						
						sharedMaterial.SetTexture("_MainTex", adjustedPlanetTexture);
					}
				}
			}
		}

		public void RestoreStockScaledTexture () 	//move to utils/scaledUtils etc
		{
			if (!ReferenceEquals (originalPlanetTexture, null))
			{
				List<Material> materials = new List<Material>(stockScaledPlanetMeshRenderer.sharedMaterials);

				foreach (Material sharedMaterial in materials)
				{	
					if (sharedMaterial.shader.name.Contains("Terrain/Scaled Planet (RimAerial)"))
					{
						sharedMaterial.SetTexture("_MainTex", originalPlanetTexture);
					}
				}
			}

			if (!ReferenceEquals (adjustedPlanetTexture, null))
			{
				adjustedPlanetTexture.Release();
			}
		}
		
		public void TweakScaledMesh() 	//move to utils/scaledUtils etc
		{
			if (ReferenceEquals (originalScaledMesh, null))
			{
				originalScaledMesh = parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh;
			}

			tweakedScaledmesh = (Mesh)Instantiate (originalScaledMesh);
			
			double scaledRadius = prolandManager.GetRadius () / (ScaledSpace.ScaleFactor * parentScaledTransform.localScale.x);
			
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

		void UpdateLightExtinctions ()
		{
			Vector3 extinctionPosition = (FlightGlobals.ActiveVessel ? FlightGlobals.ActiveVessel.transform.position : Scatterer.Instance.nearCamera.transform.position) - parentLocalTransform.position;
			Color extinction = AtmosphereUtils.getExtinction (extinctionPosition, prolandManager.getDirectionToMainSun (), Rt, Rg * atmosphereStartRadiusScale, HR*1000f, HM*1000f, m_betaR / 1000f, BETA_MSca / 1000f / 0.9f);

			extinction = Color.Lerp(Color.white, extinction, interpolatedSettings.extinctionThickness);
			Scatterer.Instance.sunlightModulatorsManagerInstance.ModulateByColor (prolandManager.mainSunLight, extinction);

			foreach(SecondarySun secondarySun in prolandManager.secondarySuns)
			{
				if (!ReferenceEquals(secondarySun.sunLight, null))
				{
					extinction = AtmosphereUtils.getExtinction (extinctionPosition, (secondarySun.celestialBody.GetTransform().position - prolandManager.parentCelestialBody.GetTransform().position).normalized,  Rt, Rg * atmosphereStartRadiusScale, HR*1000f, HM*1000f, m_betaR / 1000f, BETA_MSca / 1000f / 0.9f);
					extinction = Color.Lerp(Color.white, extinction, interpolatedSettings.extinctionThickness);	//consider getting rid of extinction thickness and tint now
					Scatterer.Instance.sunlightModulatorsManagerInstance.ModulateByColor (secondarySun.sunLight, extinction);
				}
			}
		}

		void UpdateSunflareExtinctions ()
		{
			foreach (SunFlare customSunFlare in Scatterer.Instance.sunflareManager.scattererSunFlares.Values)
			{
				if (customSunFlare.FlareRendering)	//not sure if it's worth it to try and add more intelligent culling here, like checking if a ray to the flare intersects the planet/atmo?
				{
					sunflareExtinctionMaterial.SetVector (ShaderProperties._Sun_WorldSunDir_PROPERTY, prolandManager.getDirectionToCelestialBody (customSunFlare.source).normalized);
					
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
		}
		
		void UpdateEclipseCasters ()
		{
			float scaleFactor = ScaledSpace.ScaleFactor;
			sunPosRelPlanet = Vector3.zero;
			sunPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (prolandManager.sunCelestialBody.transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor));
			castersMatrix1 = Matrix4x4.zero;
			castersMatrix2 = Matrix4x4.zero;
			Vector3 casterPosRelPlanet;
			for (int i = 0; i < Mathf.Min (4, prolandManager.eclipseCasters.Count); i++)
			{
				casterPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (prolandManager.eclipseCasters [i].transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor)); //wtf is this? this is doing local to scaled and back to local?
				castersMatrix1.SetRow (i, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y, casterPosRelPlanet.z, (float)prolandManager.eclipseCasters [i].Radius));
			}
			for (int i = 4; i < Mathf.Min (8, prolandManager.eclipseCasters.Count); i++)
			{
				casterPosRelPlanet = Vector3.Scale (ScaledSpace.LocalToScaledSpace (prolandManager.eclipseCasters [i].transform.position), new Vector3 (scaleFactor, scaleFactor, scaleFactor));
				castersMatrix2.SetRow (i - 4, new Vector4 (casterPosRelPlanet.x, casterPosRelPlanet.y, casterPosRelPlanet.z, (float)prolandManager.eclipseCasters [i].Radius));
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
							ringInnerRadius = Mathf.Max(ringInnerRadius,(float)(prolandManager.m_radius)* (1f + 10f/600000f)); //prevent inner ring radius from intersecting planet's radius because that's stupid and it breaks the shader
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
						Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial.EnableKeyword ("SCATTERER_ON");
						Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial.DisableKeyword ("SCATTERER_OFF");

						InitUniforms (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial);
						InitPostprocessMaterialUniforms (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial);

						if (HighLogic.LoadedScene == GameScenes.MAINMENU)
						{
							//Wrongly defined to ON in mainmenu by EVE, causing messed up extinction calculations
							Utils.EnableOrDisableShaderKeywords (Scatterer.Instance.eveReflectionHandler.EVEClouds2dDictionary [celestialBodyName] [i].Clouds2dMaterial, "WORLD_SPACE_ON", "WORLD_SPACE_OFF", false);
						}

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
					clouds2d.Clouds2dMaterial.SetFloat (ShaderProperties.preserveCloudColors_PROPERTY, EVEIntegration_preserveCloudColors ? 1f : 0f);
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
			EVEIntegration_preserveCloudColors =!EVEIntegration_preserveCloudColors;
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
