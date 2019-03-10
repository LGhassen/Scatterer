//using System;
using System.Collections;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using System.Reflection;
//using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace scatterer
{
	public partial class Core
	{
		class MainSettingsReadWrite
		{
			[Persistent]
			public bool autosavePlanetSettingsOnSceneChange=true;
			
			[Persistent]
			public bool disableAmbientLight=false;
			
			[Persistent]
			public bool integrateWithEVEClouds=false;
			
			[Persistent]
			public bool overrideNearClipPlane=false;
			
			[Persistent]
			public float nearClipPlane=0.5f;
			
			[Persistent]
			public bool useOceanShaders = true;
			
			[Persistent]
			public bool shadowsOnOcean = true;
			
			[Persistent]
			public bool oceanSkyReflections = true;
			
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
			
			[Persistent]
			public bool terrainShadows = true;
			
			[Persistent]
			public float shadowNormalBias=0.4f;
			
			[Persistent]
			public float shadowBias=0.125f;
			
			[Persistent]
			public float shadowsDistance=100000f;

			[Persistent]
			public int m_fourierGridSize = 128;
			
			public void loadPluginMainSettingsToCore ()
			{
				UrlDir.UrlConfig[] baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");
				if (baseConfigs.Length == 0)
				{
					Debug.Log ("[Scatterer] No config file found, check your install");
					return;
				}
				
				if (baseConfigs.Length > 1)
				{
					Debug.Log ("[Scatterer] Multiple config files detected, check your install");
				}

				ConfigNode.LoadObjectFromConfig (this, (baseConfigs [0]).config);

				Core.Instance.autosavePlanetSettingsOnSceneChange=autosavePlanetSettingsOnSceneChange;
				Core.Instance.disableAmbientLight=disableAmbientLight;
				Core.Instance.integrateWithEVEClouds=integrateWithEVEClouds;
				Core.Instance.overrideNearClipPlane=overrideNearClipPlane;
				Core.Instance.nearClipPlane=nearClipPlane;
				Core.Instance.useOceanShaders=useOceanShaders;
				Core.Instance.shadowsOnOcean=shadowsOnOcean;
				Core.Instance.oceanSkyReflections=oceanSkyReflections;
				Core.Instance.oceanPixelLights=oceanPixelLights;
				Core.Instance.fullLensFlareReplacement=fullLensFlareReplacement;
				Core.Instance.sunlightExtinction=sunlightExtinction;
				Core.Instance.underwaterLightDimming=underwaterLightDimming;
				Core.Instance.showMenuOnStart=showMenuOnStart;
				Core.Instance.useEclipses=useEclipses;
				Core.Instance.useRingShadows=useRingShadows;
				Core.Instance.terrainShadows=terrainShadows;
				Core.Instance.shadowNormalBias=shadowNormalBias;
				Core.Instance.shadowBias=shadowBias;
				Core.Instance.shadowsDistance=shadowsDistance;
				Core.Instance.m_fourierGridSize=m_fourierGridSize;
			}

			//sorry, this is so ugly
			public void saveCoreMainSettingsIfChanged()
			{
				UrlDir.UrlConfig[] baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config");

				if (baseConfigs.Length == 0)
				{
					Debug.Log ("[Scatterer] No config file found, check your install");
					return;
				}
				
				if (baseConfigs.Length > 1)
				{
					Debug.Log ("[Scatterer] Multiple config files detected, check your install");
				}
				
				ConfigNode.LoadObjectFromConfig (this, (baseConfigs [0]).config);

				bool configChanged =
					(Core.Instance.autosavePlanetSettingsOnSceneChange != autosavePlanetSettingsOnSceneChange ||
					Core.Instance.disableAmbientLight != disableAmbientLight ||
					Core.Instance.integrateWithEVEClouds != integrateWithEVEClouds ||
					Core.Instance.overrideNearClipPlane != overrideNearClipPlane ||
					Core.Instance.nearClipPlane != nearClipPlane ||
					Core.Instance.useOceanShaders != useOceanShaders ||
					Core.Instance.shadowsOnOcean != shadowsOnOcean ||
					Core.Instance.oceanSkyReflections != oceanSkyReflections ||
					Core.Instance.oceanPixelLights != oceanPixelLights ||
					Core.Instance.fullLensFlareReplacement != fullLensFlareReplacement ||
					Core.Instance.sunlightExtinction != sunlightExtinction ||
					Core.Instance.underwaterLightDimming != underwaterLightDimming ||
					Core.Instance.showMenuOnStart != showMenuOnStart ||
					Core.Instance.useEclipses != useEclipses ||
					Core.Instance.useRingShadows != useRingShadows ||
					Core.Instance.terrainShadows != terrainShadows ||
					Core.Instance.shadowNormalBias != shadowNormalBias ||
					Core.Instance.shadowBias != shadowBias ||
					Core.Instance.shadowsDistance != shadowsDistance ||
					Core.Instance.m_fourierGridSize != m_fourierGridSize);

				if (configChanged)
				{
					autosavePlanetSettingsOnSceneChange = Core.Instance.autosavePlanetSettingsOnSceneChange;
					disableAmbientLight = Core.Instance.disableAmbientLight;
					integrateWithEVEClouds = Core.Instance.integrateWithEVEClouds;
					overrideNearClipPlane = Core.Instance.overrideNearClipPlane;
					nearClipPlane = Core.Instance.nearClipPlane;
					useOceanShaders = Core.Instance.useOceanShaders;
					shadowsOnOcean = Core.Instance.shadowsOnOcean;
					oceanSkyReflections = Core.Instance.oceanSkyReflections;
					oceanPixelLights = Core.Instance.oceanPixelLights;
					fullLensFlareReplacement = Core.Instance.fullLensFlareReplacement;
					sunlightExtinction = Core.Instance.sunlightExtinction;
					underwaterLightDimming = Core.Instance.underwaterLightDimming;
					showMenuOnStart = Core.Instance.showMenuOnStart;
					useEclipses = Core.Instance.useEclipses;
					useRingShadows = Core.Instance.useRingShadows;
					terrainShadows = Core.Instance.terrainShadows;
					shadowNormalBias = Core.Instance.shadowNormalBias;
					shadowBias = Core.Instance.shadowBias;
					shadowsDistance = Core.Instance.shadowsDistance;
					m_fourierGridSize = Core.Instance.m_fourierGridSize;

					Debug.Log("[Scatterer] Main config changed");

					baseConfigs [0].config = ConfigNode.CreateConfigFromObject (this);
					baseConfigs [0].config.name = "Scatterer_config";
					Debug.Log ("[Scatterer] Saving settings to: " + baseConfigs [0].parent.url+".cfg");
					baseConfigs [0].parent.SaveConfigs ();
				}
				else
				{
					Debug.Log("[Scatterer] No changes to main config, skipping saving.");
				}

			}
		}
	}
}

