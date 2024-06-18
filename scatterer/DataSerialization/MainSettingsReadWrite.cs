using System.Collections;
using System.Collections.Generic;
using KSP;
using KSP.IO;
using UnityEngine;

namespace Scatterer
{
	public class MainSettingsReadWrite
	{
		[Persistent]
		public bool autosavePlanetSettingsOnSceneChange=false;
		
		[Persistent]
		public bool disableAmbientLight=true;
		
		[Persistent]
		public bool integrateWithEVEClouds=true;

//		[Persistent]
//		public bool integrateEVECloudsGodrays=true;
		
		[Persistent]
		public bool overrideNearClipPlane=false;
		
		[Persistent]
		public float nearClipPlane=0.5f;
		
		[Persistent]
		public bool useOceanShaders = true;

		[Persistent]
		public bool oceanFoam = true;

		[Persistent]
		public bool oceanTransparencyAndRefractions = true;

		[Persistent]
		public bool shadowsOnOcean = true;
		
		[Persistent]
		public bool oceanSkyReflections = true;

		[Persistent]
		public bool oceanCaustics = true;

		[Persistent]
		public bool oceanLightRays = false;
		
		[Persistent]
		public bool oceanCraftWaveInteractions = true;

		[Persistent]
		public bool oceanCraftWaveInteractionsOverrideWaterCrashTolerance = true;
		
		[Persistent]
		public float buoyancyCrashToleranceMultOverride=1.2f * 3f;

		[Persistent]
		public bool oceanCraftWaveInteractionsOverrideDrag = true;

		[Persistent]
		public bool oceanCraftWaveInteractionsOverrideRecoveryVelocity = true;

		[Persistent]
		public float waterMaxRecoveryVelocity = 5f;
		
		[Persistent]
		public bool oceanPixelLights = false;
		
		[Persistent]
		public bool fullLensFlareReplacement = true;
		
		[Persistent]
		public bool sunlightExtinction = true;
		
		[Persistent]
		public bool underwaterLightDimming = true;
		
		[Persistent]
		public bool showMenuOnStart = true;
		
		[Persistent]
		public bool useEclipses = true;
		
		[Persistent]
		public bool useRingShadows = true;
		
		//[Persistent]
		public bool usePlanetShine = false;

		[Persistent]
		public bool d3d11ShadowFix = true;

		[Persistent]
		public bool useLegacyTerrainGodrays = false;

		//[Persistent]
		public bool mergeDepthPrePass = false;

		//[Persistent]
		public bool quarterResScattering = true;

		[Persistent]
		public bool useSubpixelMorphologicalAntialiasing = true;

		[Persistent]
		public int smaaQuality = 0;

		[Persistent]
		public bool useTemporalAntiAliasing = true;

		[Persistent]
		public float taaStationaryBlending = 0.90f;

		[Persistent]
		public float taaMotionBlending = 0.55f;

		[Persistent]
		public float taaJitterSpread = 0.9f;

		[Persistent]
		public float taaSharpness = 0.25f;

		[Persistent]
		public int disableTaaBelowFrameRateThreshold = 26;

		[Persistent]
		public bool terrainShadows = false;

		//0 - None, 1 - Bruneton, 2 - Uncharted, 3 - Hable but disabled
		[Persistent]
		public int scatteringTonemapper = 2;

		/*
		[Persistent]
		public float hableToeStrength;

		[Persistent]
		public float hableToeLength;

		[Persistent]
		public float hableShoulderStrength;

		[Persistent]
		public float hableShoulderLength;

		[Persistent]
		public float hableShoulderAngle;

		[Persistent]
		public float hableGamma;
		*/

		[Persistent]
		public float unifiedCamShadowsDistance=50000f;

		[Persistent]
		public float unifiedCamShadowNormalBiasOverride=0.4f;
		
		[Persistent]
		public float unifiedCamShadowBiasOverride=0.125f;

		[Persistent]
		public int unifiedCamShadowResolutionOverride=0;

		[Persistent]
		public Vector3 unifiedCamShadowCascadeSplitsOverride=Vector3.zero;
		
		[Persistent]
		public float dualCamShadowsDistance=50000f;

		[Persistent]
		public float dualCamShadowNormalBiasOverride=0.4f;
		
		[Persistent]
		public float dualCamShadowBiasOverride=0.125f;

		[Persistent]
		public int dualCamShadowResolutionOverride=0;

		[Persistent]
		public Vector3 dualCamShadowCascadeSplitsOverride=Vector3.zero;

		[Persistent]
		public bool useDithering = true;
		
		[Persistent]
		public int m_fourierGridSize = 64;

		[Persistent]
		public int oceanMeshResolution = 8;

		[Persistent]
		public bool useLowResolutionAtmosphere = false;

		public void loadMainSettings ()
		{
			UrlDir.UrlConfig[] baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");
			if (baseConfigs.Length == 0)
			{
				Utils.LogError ("No config file found, check your install");
				return;
			}
			
			if (baseConfigs.Length > 1)
			{
				Utils.LogError ("Multiple config files detected, check your install");
			}
			
			ConfigNode.LoadObjectFromConfig (this, (baseConfigs [0]).config);
		}
		
		public void saveMainSettingsIfChanged()
		{
			UrlDir.UrlConfig[] baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");
			
			if (baseConfigs.Length == 0)
			{
				Utils.LogDebug ("No config file found, check your install");
				return;
			}
			
			if (baseConfigs.Length > 1)
			{
				Utils.LogDebug ("Multiple config files detected, check your install");
			}
			
			MainSettingsReadWrite OldConfig = new MainSettingsReadWrite ();
			ConfigNode.LoadObjectFromConfig (OldConfig, (baseConfigs [0]).config);
			
			bool configChanged =
				(OldConfig.autosavePlanetSettingsOnSceneChange != autosavePlanetSettingsOnSceneChange ||
				 OldConfig.disableAmbientLight != disableAmbientLight ||
				 OldConfig.integrateWithEVEClouds != integrateWithEVEClouds ||
//				 OldConfig.integrateEVECloudsGodrays != integrateEVECloudsGodrays ||
				 OldConfig.overrideNearClipPlane != overrideNearClipPlane ||
				 OldConfig.nearClipPlane != nearClipPlane ||
				 OldConfig.useOceanShaders != useOceanShaders ||
				 OldConfig.oceanFoam != oceanFoam ||
				 OldConfig.oceanTransparencyAndRefractions != oceanTransparencyAndRefractions ||

				 OldConfig.shadowsOnOcean != shadowsOnOcean ||
				 OldConfig.oceanSkyReflections != oceanSkyReflections ||
				 OldConfig.oceanCaustics != oceanCaustics ||
				 OldConfig.oceanLightRays != oceanLightRays ||

				 OldConfig.oceanCraftWaveInteractions != oceanCraftWaveInteractions ||
				 OldConfig.oceanCraftWaveInteractionsOverrideWaterCrashTolerance != oceanCraftWaveInteractionsOverrideWaterCrashTolerance || 
				 OldConfig.buoyancyCrashToleranceMultOverride != buoyancyCrashToleranceMultOverride || 
				 OldConfig.oceanCraftWaveInteractionsOverrideDrag != oceanCraftWaveInteractionsOverrideDrag || 
				 OldConfig.oceanCraftWaveInteractionsOverrideRecoveryVelocity != oceanCraftWaveInteractionsOverrideRecoveryVelocity || 
				 OldConfig.waterMaxRecoveryVelocity != waterMaxRecoveryVelocity || 

				 OldConfig.oceanPixelLights != oceanPixelLights ||
				 OldConfig.fullLensFlareReplacement != fullLensFlareReplacement ||
				 OldConfig.sunlightExtinction != sunlightExtinction ||
				 OldConfig.underwaterLightDimming != underwaterLightDimming ||
				 OldConfig.showMenuOnStart != showMenuOnStart ||
				 OldConfig.useEclipses != useEclipses ||
				 OldConfig.useRingShadows != useRingShadows ||
				 OldConfig.d3d11ShadowFix != d3d11ShadowFix ||
				 OldConfig.useLegacyTerrainGodrays != useLegacyTerrainGodrays ||
				 //OldConfig.mergeDepthPrePass != mergeDepthPrePass ||
				 OldConfig.quarterResScattering != quarterResScattering ||
				 
				 OldConfig.useSubpixelMorphologicalAntialiasing != useSubpixelMorphologicalAntialiasing ||
				 OldConfig.smaaQuality != smaaQuality ||
				 
				 OldConfig.useTemporalAntiAliasing  != useTemporalAntiAliasing ||
				 OldConfig.taaStationaryBlending != taaStationaryBlending ||
				 OldConfig.taaMotionBlending != taaMotionBlending ||
				 OldConfig.taaJitterSpread != taaJitterSpread ||
				 OldConfig.taaSharpness != taaSharpness ||
				 OldConfig.disableTaaBelowFrameRateThreshold != disableTaaBelowFrameRateThreshold ||

				 OldConfig.terrainShadows != terrainShadows ||
				 OldConfig.scatteringTonemapper != scatteringTonemapper ||

				 /*
				 OldConfig.hableToeStrength != hableToeStrength ||
				 OldConfig.hableToeLength != hableToeLength ||
				 OldConfig.hableShoulderStrength != hableShoulderStrength ||
				 OldConfig.hableShoulderLength != hableShoulderLength ||
				 OldConfig.hableShoulderAngle != hableShoulderAngle ||
				 OldConfig.hableGamma != hableGamma ||
				 */

				 OldConfig.useDithering != useDithering ||

				 OldConfig.unifiedCamShadowsDistance != unifiedCamShadowsDistance ||
				 OldConfig.unifiedCamShadowNormalBiasOverride != unifiedCamShadowNormalBiasOverride ||
				 OldConfig.unifiedCamShadowBiasOverride != unifiedCamShadowBiasOverride ||
				 OldConfig.unifiedCamShadowCascadeSplitsOverride != unifiedCamShadowCascadeSplitsOverride ||
				 OldConfig.unifiedCamShadowResolutionOverride != unifiedCamShadowResolutionOverride ||

				 OldConfig.dualCamShadowsDistance != dualCamShadowsDistance ||
				 OldConfig.dualCamShadowNormalBiasOverride != dualCamShadowNormalBiasOverride ||
				 OldConfig.dualCamShadowBiasOverride != dualCamShadowBiasOverride ||
				 OldConfig.dualCamShadowCascadeSplitsOverride != dualCamShadowCascadeSplitsOverride ||
				 OldConfig.dualCamShadowResolutionOverride != dualCamShadowResolutionOverride ||

				 OldConfig.m_fourierGridSize != m_fourierGridSize ||
				 OldConfig.oceanMeshResolution != oceanMeshResolution) ||

				 OldConfig.useLowResolutionAtmosphere != useLowResolutionAtmosphere;
			
			if (configChanged)
			{
				Utils.LogDebug("Main config changed");
				
				baseConfigs [0].config = ConfigNode.CreateConfigFromObject (this);
				baseConfigs [0].config.name = "Scatterer_config";
				Utils.LogDebug ("Saving settings to: " + baseConfigs [0].parent.url+".cfg");
				baseConfigs [0].parent.SaveConfigs ();
			}
			else
			{
				Utils.LogDebug("No changes to main config, skipping saving.");
			}

		}
	}
}