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
		[Persistent] public bool hasOcean;
		[Persistent] public bool usesCloudIntegration;
		[Persistent] public List<string> eclipseCasters=new List<string> {};
		[Persistent] public bool flatScaledSpaceModel;

		[Persistent] public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
	


		public CelestialBody celestialBody;
		public Transform transform;
		public bool hasTransform = false;
		public bool active;
		public Manager m_manager;
		public Material originalPlanetMaterialBackup;
		
		
		public ScattererCelestialBody (string inCelestialBodyName, string inTransformName, float inloadDistance,
		                               float inUnloadDistance, bool inHasOcean, List<string> inEclipseCasters,
		                               List<AtmoPlanetShineSource> inPlanetShineSources, string inSun, bool inUsesCloudIntegration, bool inFlatScaledSpaceModel)
		{
			celestialBodyName = inCelestialBodyName;
			transformName=inTransformName;
			loadDistance = inloadDistance;
			unloadDistance=inUnloadDistance;
			hasOcean = inHasOcean;
			eclipseCasters = inEclipseCasters;

			planetshineSources = inPlanetShineSources;
			mainSunCelestialBody = inSun;
			usesCloudIntegration = inUsesCloudIntegration;
			flatScaledSpaceModel = inFlatScaledSpaceModel;
		}
		
		public ScattererCelestialBody ()
		{
			
		}
	}
}

