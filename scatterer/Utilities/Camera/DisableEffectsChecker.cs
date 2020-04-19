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
		Dictionary<Camera,DisableEffectsForReflectionsCamera> camToEffectsDisablerDictionary =  new Dictionary<Camera,DisableEffectsForReflectionsCamera>() ;
		public ProlandManager manager;

		public DisableEffectsChecker ()
		{
		}

		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			// TODO: find solutions for the Reflection Probes Camera mess, seriously squad? local and scaledScenery in the same culling mask?
			if (!camToEffectsDisablerDictionary.ContainsKey(cam) && ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera")))
			{
				camToEffectsDisablerDictionary[cam] = (DisableEffectsForReflectionsCamera) cam.gameObject.AddComponent(typeof(DisableEffectsForReflectionsCamera));
				camToEffectsDisablerDictionary[cam].manager = manager;

				Utils.LogDebug("Post-processing and ocean effects disabled from reflections Camera "+cam.name);
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

