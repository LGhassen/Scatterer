using System;
using UnityEngine;

namespace scatterer
{
	public class AtmosphereProjector : MonoBehaviour
	{
		public Projector projector = null;
		GameObject projectorGO = null;
		bool inScaledSpace = false;
		bool underwater = false;
		bool activated = true;

		public AtmosphereProjector (Material atmosphereMaterial, Transform parentTransform, float Rt)
		{
			projectorGO =  new GameObject("Scatterer atmosphere projector "+atmosphereMaterial.name);
						
			projector = projectorGO.AddComponent<Projector>();

			projector.aspectRatio = 1;
			projector.orthographic = true;
			projector.orthographicSize = 2*Rt;
			projector.nearClipPlane = 1;
			projector.farClipPlane = 4*Rt;
			projector.ignoreLayers = ~((1<<0) | (1<<1) | (1<<4) | (1<<15) | (1<<16) | (1<<19)); //ignore all except 4 water 15 local 16 kerbals and 19 parts

			projectorGO.layer = 15;

			projectorGO.transform.position = parentTransform.forward * 2*Rt + parentTransform.position;
			projectorGO.transform.forward  = parentTransform.position - projectorGO.transform.position;
			projectorGO.transform.parent   = parentTransform;

			projector.material = atmosphereMaterial;
			projector.material.CopyKeywordsFrom (atmosphereMaterial);
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

		public void updateProjector ()
		{
			bool isEnabled = !underwater && !inScaledSpace && activated;
			projector.enabled = isEnabled;
			projectorGO.SetActive(isEnabled);
		}

		public void CleanUp()
		{
			Utils.LogDebug ("AtmosphereProjector CleanUp called");
			if(!ReferenceEquals(projectorGO,null))
			{
				if(!ReferenceEquals(projectorGO.transform,null))
				{
					if(!ReferenceEquals(projectorGO.transform.parent,null))
					{
						projectorGO.transform.parent = null;
					}
				}

				Component.Destroy(projector);
				GameObject.DestroyImmediate(projectorGO);
				projector = null;
				projectorGO = null;
			}
		}
	}
}

