using System;
using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{
	public class atmoPlanetShineSource
	{
		[Persistent] public string bodyName;
		[Persistent] public Vector3 color;
		[Persistent] public float intensity;
		[Persistent] public bool isSun;
		public CelestialBody body;
	}

	public class planetShineLightSource
	{
		[Persistent] public string bodyName;
		[Persistent] public Vector3 color;
		[Persistent] public float intensity;
		[Persistent] public bool isSun;
		[Persistent] public float localRange;
		[Persistent] public float scaledRange;
		[Persistent] public float fadeRadius;
	}
}

