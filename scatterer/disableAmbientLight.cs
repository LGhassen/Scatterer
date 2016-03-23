
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
		Color ambientLight;
		Light[] lights;
		GameObject sunLight,scaledspaceSunLight;
		float originalScaledSunlightIntensity=0f;

		public disableAmbientLight()
		{
			//find sunlight
			lights = (Light[]) Light.FindObjectsOfType(typeof( Light));
			Debug.Log ("number of lights" + lights.Length);
			foreach (Light _light in lights)
			{
				Debug.Log("name:"+_light.gameObject.name);
				Debug.Log("intensity:"+_light.intensity.ToString());
				Debug.Log ("mask:"+_light.cullingMask.ToString());
				Debug.Log ("type:"+_light.type.ToString());
				Debug.Log ("Parent:"+_light.transform.parent.gameObject.name);
				Debug.Log ("range:"+_light.range.ToString());
				
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledspaceSunLight=_light.gameObject;
					Debug.Log("Found scaled sunlight");
				}
				
				if (_light.gameObject.name == "SunLight")
				{
					sunLight=_light.gameObject;
					Debug.Log("Found sunlight");
				}
				
			}

			originalScaledSunlightIntensity = scaledspaceSunLight.light.intensity;

		}

		public void OnPreRender()
		{
			ambientLight = RenderSettings.ambientLight;
			RenderSettings.ambientLight = Color.black;
			scaledspaceSunLight .light.intensity=0.6f;
		}

		public void OnPostRender()
		{
			RenderSettings.ambientLight = ambientLight;
			scaledspaceSunLight.light.intensity = originalScaledSunlightIntensity;
		}

		public void restoreLight()
		{
			scaledspaceSunLight.light.intensity = originalScaledSunlightIntensity;
		}
	}
}

