using System;
using UnityEngine;
using System.Collections.Generic;

namespace Scatterer
{
	public class AtmoPlanetShineSource
	{
		[Persistent] public string bodyName;
		[Persistent] public Vector3 color;
		[Persistent] public float intensity;
		[Persistent] public bool isSun;
		public CelestialBody body;
	}

	public class PlanetShineLightSource
	{
		[Persistent] public string bodyName;
		[Persistent] public Vector3 color;
		[Persistent] public float intensity;
		[Persistent] public bool isSun;
		[Persistent] public string mainSunCelestialBody;
		[Persistent] public float localRange;
		[Persistent] public float scaledRange;
		[Persistent] public float fadeRadius;
	}
}

