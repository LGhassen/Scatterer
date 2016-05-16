using System;
using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{
	public class planetShineSource
	{
		[Persistent] public string bodyName;
		[Persistent] public Vector3 color;
		[Persistent] public float intensity;
		[Persistent] public bool isSun;
		public CelestialBody body;
	}
}

