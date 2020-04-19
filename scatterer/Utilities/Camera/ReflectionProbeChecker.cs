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
	public class ReflectionProbeChecker : MonoBehaviour
	{
		Dictionary<Camera,ReflectionProbeFixer> camToFixer =  new Dictionary<Camera,ReflectionProbeFixer>() ;

		public ReflectionProbeChecker ()
		{
		}

		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			if (!camToFixer.ContainsKey(cam) && (cam.name=="Reflection Probes Camera"))
			{
				camToFixer[cam] = (ReflectionProbeFixer) cam.gameObject.AddComponent(typeof(ReflectionProbeFixer));

				Utils.LogDebug("Added reflection probe fixer to "+cam.name);
			}

		}

		public void OnDestroy()
		{
			if (camToFixer.Count != 0) 
			{
				foreach (var _val in camToFixer.Values)
				{
					Component.Destroy (_val);
					UnityEngine.Object.Destroy (_val);
				}
				camToFixer.Clear();
			}
		}
	}
}

