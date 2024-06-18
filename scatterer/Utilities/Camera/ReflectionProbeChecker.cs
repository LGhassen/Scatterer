using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace Scatterer
{
	public class ReflectionProbeChecker : MonoBehaviour
	{
		Dictionary<Camera,ReflectionProbeFixer> camToFixer =  new Dictionary<Camera,ReflectionProbeFixer>() ;

		public ReflectionProbeChecker ()
		{
		}

		public void OnUpdate()
		{
			gameObject.transform.position = Scatterer.Instance.nearCamera.transform.position + (Scatterer.Instance.nearCamera.transform.forward * -5000f);
		}

		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			if (!camToFixer.ContainsKey(cam))
			{
				if (cam.name=="Reflection Probes Camera")
				{
					camToFixer[cam] = (ReflectionProbeFixer) cam.gameObject.AddComponent(typeof(ReflectionProbeFixer));

					//Grab the reflection probe and set it's distance to 100km so reflections no longer disappear when using cameraTools stationary camera
					ReflectionProbe[] probes = Resources.FindObjectsOfTypeAll<ReflectionProbe> ();
					foreach (ReflectionProbe _probe in probes)
					{
                        float size = Mathf.Max(10000000f, _probe.size.x);
                        _probe.size = new Vector3(size, size, size);
                    }

					Utils.LogDebug("Added reflection probe fixer to "+cam.name);
				}
				else
				{
					//we add it anyway to avoid doing a string compare
					camToFixer[cam] = null;
				}
			}

		}

		public void OnDestroy()
		{
			if (camToFixer.Count != 0) 
			{
				foreach (var _val in camToFixer.Values)
				{
					if (_val)
					{
						Component.DestroyImmediate (_val);
					}
				}
				camToFixer.Clear();
			}
		}
	}
}

