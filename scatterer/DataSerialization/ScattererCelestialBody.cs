using System;
using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{
	public class ScattererCelestialBody
	{
		[Persistent] public string celestialBodyName;
		[Persistent] public string transformName;
		[Persistent] public float loadDistance;
		[Persistent] public float unloadDistance;
		[Persistent] public string mainSunCelestialBody;
		[Persistent] public Color sunColor = Color.white;
		[Persistent] public bool hasOcean;
		[Persistent] public bool usesCloudIntegration;
		[Persistent] public List<string> eclipseCasters=new List<string> {};
		[Persistent] public bool flatScaledSpaceModel;

		[Persistent] public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
			
		public CelestialBody celestialBody;
		public Transform transform;
		public bool isFound = false;
		public bool active;
		public ProlandManager prolandManager;
		
		public ScattererCelestialBody ()
		{
			
		}
	}
}

