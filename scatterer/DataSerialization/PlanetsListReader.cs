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
	public class PlanetsListReader
	{
		[Persistent]
		public List <ScattererCelestialBody> scattererCelestialBodies = new List <ScattererCelestialBody> {};

		[Persistent]
		public List<PlanetShineLightSource> celestialLightSourcesData=new List<PlanetShineLightSource> {};

		[Persistent]
		public List<string> sunflares=new List<string> {};

		public void loadPlanetsListToCore ()
		{
			ConfigNode[] confNodes = GameDatabase.Instance.GetConfigNodes ("Scatterer_planetsList");
			if (confNodes.Length == 0) {
				Utils.LogDebug ("No planetsList file found, check your install");
				return;
			}
			
			if (confNodes.Length > 1) {
				Utils.LogDebug ("Multiple planetsList files detected, check your install");
			}

			ConfigNode.LoadObjectFromConfig (this, confNodes [0]);

			Core.Instance.scattererCelestialBodies = scattererCelestialBodies;
			Core.Instance.celestialLightSourcesData = celestialLightSourcesData;
			Core.Instance.sunflaresList = sunflares;
		}
	}
}

