using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using UnityEngine;

namespace Scatterer
{
	public abstract class GenericLocalAtmosphereContainer
	{	
		protected GameObject scatteringGO = null;
		protected bool inScaledSpace = true;
		protected bool underwater = false;
		protected bool activated = false;
		public Material material;
		public ProlandManager manager;

		public GenericLocalAtmosphereContainer (Material atmosphereMaterial, Transform parentTransform, float Rt, ProlandManager parentManager)
		{
			material = atmosphereMaterial;
			manager = parentManager;
		}

		public void SetActivated (bool pEnabled)
		{
			activated = pEnabled;
		}
		
		public void SetInScaledSpace (bool pInScaledSpace)
		{
			inScaledSpace = pInScaledSpace;
		}
		
		public void SetUnderwater (bool pUnderwater)
		{
			underwater = pUnderwater;
		}
		
		public abstract void UpdateContainer ();

		public abstract void Cleanup ();
	}
}