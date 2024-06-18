using System;
using UnityEngine;

namespace Scatterer
{
	public class AtmosphereProjectorContainer : GenericLocalAtmosphereContainer
	{
		public Projector projector = null;

		public AtmosphereProjectorContainer (Material atmosphereMaterial, Transform parentTransform, float Rt, ProlandManager parentManager) : base (atmosphereMaterial, parentTransform, Rt, parentManager)
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

		public override void UpdateContainer ()
		{
			bool isEnabled = !underwater && !inScaledSpace && activated;
			projector.enabled = isEnabled;
			scatteringGO.SetActive(isEnabled);
		}

		public override void Cleanup ()
		{
			SetActivated (false);
			if(scatteringGO)
			{
				if(scatteringGO.transform && scatteringGO.transform.parent)
				{
						scatteringGO.transform.parent = null;
				}

				Component.Destroy(projector);
				GameObject.DestroyImmediate(scatteringGO);
				projector = null;
				scatteringGO = null;
			}
		}
	}
}

