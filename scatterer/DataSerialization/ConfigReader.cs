using System.Collections;
using System.Collections.Generic;
using KSP;
using KSP.IO;
using UnityEngine;

namespace scatterer
{
	public class ConfigReader
	{
		[Persistent]
		public List <ScattererCelestialBody> scattererCelestialBodies = new List <ScattererCelestialBody> {};

		[Persistent]
		public List<PlanetShineLightSource> celestialLightSourcesData=new List<PlanetShineLightSource> {};

		public ConfigNode[] sunflareConfigs;
		public UrlDir.UrlConfig[] baseConfigs,atmoConfigs,oceanConfigs;

		public void loadConfigs ()
		{
			baseConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_config"); //only used for displaying filepath

			ConfigNode[] confNodes = GameDatabase.Instance.GetConfigNodes ("Scatterer_planetsList");
			if (confNodes.Length == 0) {
				Utils.LogError ("No planetsList file found, check your install");
				return;
			}
			
			if (confNodes.Length > 1) {
				Utils.LogError ("Multiple planetsList files detected, check your install");
			}

			ConfigNode.LoadObjectFromConfig (this, confNodes [0]);

			atmoConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_atmosphere");
			oceanConfigs = GameDatabase.Instance.GetConfigs ("Scatterer_ocean");
			sunflareConfigs = GameDatabase.Instance.GetConfigNodes ("Scatterer_sunflare");
		}
	}
}

