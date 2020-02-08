using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace scatterer
{
	public class OceanCameraUpdateHook : MonoBehaviour
	{
		public OceanNode oceanNode;

		// Whenever any camera will render us, call the method which updates the material with the right params
		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam || MapView.MapIsEnabled || oceanNode.m_manager.m_skyNode.inScaledSpace)
				return;

			oceanNode.updateCameraSpecificUniforms (oceanNode.m_oceanMaterial, cam);

			if (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera)
			{
				oceanNode.m_oceanMaterial.EnableKeyword("REFRACTIONS_AND_TRANSPARENCY_ON");
				oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_OFF");
			}
			else
			{
				oceanNode.m_oceanMaterial.EnableKeyword("REFRACTIONS_AND_TRANSPARENCY_OFF");
				oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_ON");
			}
		}

		public void OnDestroy()
		{
		}
	}
}