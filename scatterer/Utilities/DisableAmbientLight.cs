
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

	public class DisableAmbientLight : MonoBehaviour
	{
		Color ambientLight;
		Color originalAmbientLight;

		Light[] lights;
		GameObject sunLight,scaledspaceSunLight;
		float originalScaledSunlightIntensity=0f;


		Light _scaledspaceSunLight;

		public DisableAmbientLight()
		{
			//find sunlight
			lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
			foreach (Light _light in lights)
			{	
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledspaceSunLight=_light.gameObject;
					Debug.Log("[Scatterer] disableAmbientLight: Found scaled sunlight");
				}
				
				if (_light.gameObject.name == "SunLight")
				{
					sunLight=_light.gameObject;
					Debug.Log("[Scatterer] disableAmbientLight: Found sunlight");
				}				
			}



			ambientLight = Color.black;
			_scaledspaceSunLight = scaledspaceSunLight.GetComponent<Light> ();

		}

		public void OnPreRender()
		{
			originalAmbientLight = RenderSettings.ambientLight;
			RenderSettings.ambientLight = ambientLight;

			originalScaledSunlightIntensity = _scaledspaceSunLight.intensity;
			_scaledspaceSunLight.intensity=0.95f;
		}

		public void OnPostRender()
		{
			restoreLight ();
		}

		public void restoreLight()
		{
			RenderSettings.ambientLight = originalAmbientLight;
			_scaledspaceSunLight.intensity = originalScaledSunlightIntensity;
		}
	}
}

