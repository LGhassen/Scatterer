using System;
using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{
	public class scattererCelestialBody
	{
		[Persistent] public string celestialBodyName;
		[Persistent] public string transformName;
		[Persistent] public float loadDistance;
		[Persistent] public float unloadDistance;
		[Persistent] public bool hasOcean;
		[Persistent] public List<string> eclipseCasters=new List<string> {};

		[Persistent] public List<planetShineSource> planetshineSources=new List<planetShineSource> {};
	


		public CelestialBody celestialBody;
		public Transform transform;
		public bool hasTransform = false;
		public bool active;
		public Manager m_manager;
		public Material originalPlanetMaterialBackup;
		
		
		public scattererCelestialBody (string inCelestialBodyName, string inTransformName, float inloadDistance,
		                               float inUnloadDistance, bool inHasOcean, List<string> inEclipseCasters,
		                               List<planetShineSource> inPlanetShineSources)
		{
			celestialBodyName = inCelestialBodyName;
			transformName=inTransformName;
			loadDistance = inloadDistance;
			unloadDistance=inUnloadDistance;
			hasOcean = inHasOcean;
			eclipseCasters = inEclipseCasters;

			planetshineSources = inPlanetShineSources;
		}

		public scattererCelestialBody (CelestialBody inCelestialBody, Transform inTransform)
		{
			transform = inTransform;
			celestialBody = inCelestialBody;
		}
		
		public scattererCelestialBody ()
		{
			
		}
	}
}

