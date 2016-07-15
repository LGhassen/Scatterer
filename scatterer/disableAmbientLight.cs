
using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{

	public class disableAmbientLight : MonoBehaviour
	{
//		Color ambientLight;

		float ambientIntensity=0f;

		Light[] lights;
		GameObject sunLight,scaledspaceSunLight;
		float originalScaledSunlightIntensity=0f;


		Light _scaledspaceSunLight;

		public disableAmbientLight()
		{
			//find sunlight
			lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
			foreach (Light _light in lights)
			{	
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledspaceSunLight=_light.gameObject;
//					Debug.Log("[Scatterer] Found scaled sunlight");
				}
				
				if (_light.gameObject.name == "SunLight")
				{
					sunLight=_light.gameObject;
//					Debug.Log("[Scatterer] Found sunlight");
				}				
			}

			_scaledspaceSunLight = scaledspaceSunLight.GetComponent<Light> ();
			originalScaledSunlightIntensity = _scaledspaceSunLight.intensity;

		}

		public void OnPreRender()
		{
			ambientIntensity = RenderSettings.ambientIntensity;
			RenderSettings.ambientIntensity = 0f;
			_scaledspaceSunLight.intensity=0.95f;

		}

		public void OnPostRender()
		{
			restoreLight ();
		}

		public void restoreLight()
		{
			RenderSettings.ambientIntensity = ambientIntensity;
			_scaledspaceSunLight.intensity = originalScaledSunlightIntensity;
		}
	}
}

