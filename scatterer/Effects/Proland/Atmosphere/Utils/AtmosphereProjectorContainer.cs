using System;
using UnityEngine;

namespace scatterer
{
	public class AtmosphereProjectorContainer : AbstractLocalAtmosphereContainer
	{
		public Projector projector = null;

		public AtmosphereProjectorContainer (Material atmosphereMaterial, Transform parentTransform, float Rt) : base (atmosphereMaterial, parentTransform, Rt)
		{
			scatteringGO =  new GameObject("Scatterer atmosphere projector "+atmosphereMaterial.name);
						
			projector = scatteringGO.AddComponent<Projector>();

			projector.aspectRatio = 1;
			projector.orthographic = true;
			projector.orthographicSize = 2*Rt;
			projector.nearClipPlane = 1;
			projector.farClipPlane = 4*Rt;
			projector.ignoreLayers = ~((1<<0) | (1<<1) | (1<<4) | (1<<15) | (1<<16) | (1<<19)); //ignore all except 4 water 15 local 16 kerbals and 19 parts

			scatteringGO.layer = 15;

			scatteringGO.transform.position = parentTransform.forward * 2*Rt + parentTransform.position;
			scatteringGO.transform.forward  = parentTransform.position - scatteringGO.transform.position;
			scatteringGO.transform.parent   = parentTransform;

			projector.material = atmosphereMaterial;
			projector.material.CopyKeywordsFrom (atmosphereMaterial);
		}

		public override void updateContainer ()
		{
			bool isEnabled = !underwater && !inScaledSpace && activated;
			projector.enabled = isEnabled;
			scatteringGO.SetActive(isEnabled);
		}

		~AtmosphereProjectorContainer()
		{
			setActivated (false);
			if(!ReferenceEquals(scatteringGO,null))
			{
				if(!ReferenceEquals(scatteringGO.transform,null))
				{
					if(!ReferenceEquals(scatteringGO.transform.parent,null))
					{
						scatteringGO.transform.parent = null;
					}
				}

				Component.Destroy(projector);
				GameObject.DestroyImmediate(scatteringGO);
				projector = null;
				scatteringGO = null;
			}
		}
	}
}

