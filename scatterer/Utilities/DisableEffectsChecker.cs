//used just to remove the postprocessing and the ocean from texture replacer's reflections because they look messed up and bog down performance
//this part gets added to the postprocessingCube, it will then detect when TR attempts to render it and a script to the TR camera to disable the effects on it
//the TR camera only gets created only once an IVA kerbal appears on screen, and thus it is necessary to do this as the camera may not exist when scatterer is initializing

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
	public class DisableEffectsChecker : MonoBehaviour
	{
		Dictionary<Camera,DisableEffectsForTextureReplacer> camToEffectsDisablerDictionary =  new Dictionary<Camera,DisableEffectsForTextureReplacer>() ;
		public ProlandManager manager;

		public DisableEffectsChecker ()
		{
		}

		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			if (cam.name == "TRReflectionCamera" && !camToEffectsDisablerDictionary.ContainsKey(cam))
			{
				camToEffectsDisablerDictionary[cam] = (DisableEffectsForTextureReplacer) cam.gameObject.AddComponent(typeof(DisableEffectsForTextureReplacer));
				camToEffectsDisablerDictionary[cam].manager = manager;

				Utils.LogDebug("Post-processing and ocean effects disabled from Texture Replacer reflections");
			}

		}

		public void OnDestroy()
		{
			if (camToEffectsDisablerDictionary.Count != 0) 
			{
				foreach (var _val in camToEffectsDisablerDictionary.Values)
				{
					Component.Destroy (_val);
					UnityEngine.Object.Destroy (_val);
				}
				camToEffectsDisablerDictionary.Clear();
			}
		}
	}
}

