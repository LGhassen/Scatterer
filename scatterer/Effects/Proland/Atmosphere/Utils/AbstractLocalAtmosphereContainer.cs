using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using UnityEngine;

namespace scatterer
{
	public abstract class AbstractLocalAtmosphereContainer : MonoBehaviour
	{	
		protected GameObject scatteringGO = null;
		protected bool inScaledSpace = false;
		protected bool underwater = false;
		protected bool activated = true;
		public Material material;
		public ProlandManager manager;

		public AbstractLocalAtmosphereContainer (Material atmosphereMaterial, Transform parentTransform, float Rt, ProlandManager parentManager)
		{
			material = atmosphereMaterial;
			manager = parentManager;
		}

		public void setActivated (bool pEnabled)
		{
			activated = pEnabled;
		}
		
		public void setInScaledSpace (bool pInScaledSpace)
		{
			inScaledSpace = pInScaledSpace;
		}
		
		public void setUnderwater (bool pUnderwater)
		{
			underwater = pUnderwater;
		}
		
		public abstract void updateContainer ();
	}
}