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

			Utils.EnableOrDisableShaderKeywords (oceanNode.m_oceanMaterial, "REFRACTIONS_AND_TRANSPARENCY_ON", "REFRACTIONS_AND_TRANSPARENCY_OFF", (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera));
		}

		public void OnDestroy()
		{
		}
	}
}