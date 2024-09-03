// I split the scaled planet material into 2 materials, one for the main light pass and another for the secondary light passes so I can inject eclipses in-between the two materials
// This causes issues with Kopernicus ondemand though, when it unloads the planet the texture is lost, when it loads it back it only updates the texture for the original material (main pass)
// This syncs up the second material every few frames

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KSP.IO;

namespace Scatterer
{
    public class PlanetSecondaryLightUpdater : MonoBehaviour
    {
        Material sourceMaterial, targetMaterial;

        public PlanetSecondaryLightUpdater ()
        {
        }

        public void Init(Material inSourceMaterial, Material inTargetMaterial)
        {
            sourceMaterial = inSourceMaterial;
            targetMaterial = inTargetMaterial;

            try {StartCoroutine(UpdateCoroutine ());}
            catch (Exception e){Utils.LogError("Error when starting PlanetSecondaryLightUpdater::UpdateCoroutine coroutine "+e.Message);};
        }
        
        IEnumerator UpdateCoroutine()
        {
            while (true)
            {
                if (sourceMaterial && targetMaterial)
                {
                    targetMaterial.CopyPropertiesFromMaterial (sourceMaterial);
                    targetMaterial.SetShaderPassEnabled ("ForwardBase", false);
                    targetMaterial.SetShaderPassEnabled ("ForwardAdd", true);
                    targetMaterial.renderQueue = 2002;
                }
                
                yield return new WaitForSeconds (1.3f);
            }
        }

        public void OnDestroy()
        {
            StopAllCoroutines ();
        }
    }
}

