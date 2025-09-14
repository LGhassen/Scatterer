//used just to remove the ocean from texture replacer's reflections because it looks messed up and bogs down performance
//this part gets added to the postprocessingCube, it will then detect when TR attempts to render it and a script to the TR camera to disable the effects on it
//the TR camera only gets created only once an IVA kerbal appears on screen, and thus it is necessary to do this as the camera may not exist when scatterer is initializing

using UnityEngine;
using System.Collections.Generic;

namespace Scatterer
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

            if (!camToEffectsDisablerDictionary.ContainsKey(cam))
            {
                if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera"))
                {
                    camToEffectsDisablerDictionary[cam] = (DisableEffectsForReflectionsCamera) cam.gameObject.AddComponent(typeof(DisableEffectsForReflectionsCamera));
                    camToEffectsDisablerDictionary[cam].manager = manager;
                    
                    Utils.LogDebug("Ocean effects disabled from reflections Camera "+cam.name);
                }
                else
                {
                    //we add it anyway to avoid doing a string compare
                    camToEffectsDisablerDictionary[cam] = null;
                }
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

