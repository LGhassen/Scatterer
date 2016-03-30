
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
//			Debug.Log ("number of lights" + lights.Length);
			foreach (Light _light in lights)
			{
//				Debug.Log("name:"+_light.gameObject.name);
//				Debug.Log("intensity:"+_light.intensity.ToString());
//				Debug.Log ("mask:"+_light.cullingMask.ToString());
//				Debug.Log ("type:"+_light.type.ToString());
//				Debug.Log ("Parent:"+_light.transform.parent.gameObject.name);
//				Debug.Log ("range:"+_light.range.ToString());
				
				if (_light.gameObject.name == "Scaledspace SunLight")
				{
					scaledspaceSunLight=_light.gameObject;
					Debug.Log("[Scatterer] Found scaled sunlight");
				}
				
				if (_light.gameObject.name == "SunLight")
				{
					sunLight=_light.gameObject;
					Debug.Log("[Scatterer] Found sunlight");
				}
				
			}

			_scaledspaceSunLight = scaledspaceSunLight.GetComponent<Light> ();
			originalScaledSunlightIntensity = _scaledspaceSunLight.intensity;

		}

		public void OnPreRender()
		{
//			ambientLight = RenderSettings.ambientLight;
//			RenderSettings.ambientLight = Color.black;
			ambientIntensity = RenderSettings.ambientIntensity;
			RenderSettings.ambientIntensity = 0f;
			_scaledspaceSunLight.intensity=0.95f;

		}

		public void OnPostRender()
		{
//			RenderSettings.ambientLight = ambientLight;
			restoreLight ();
		}

		public void restoreLight()
		{
			RenderSettings.ambientIntensity = ambientIntensity;
			_scaledspaceSunLight.intensity = originalScaledSunlightIntensity;
		}
	}
}

