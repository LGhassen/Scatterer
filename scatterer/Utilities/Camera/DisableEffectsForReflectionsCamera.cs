// Used just to remove the ocean from reflection probesbecause it looks messed up and bog down performance
// this script gets added to the camera to disable the effects on

using UnityEngine;

namespace Scatterer
{
    public class DisableEffectsForReflectionsCamera : MonoBehaviour
    {
        public ProlandManager manager;

        public DisableEffectsForReflectionsCamera ()
        {
        }

        //also add EVE cloud Projectors, EVE/PlanetLight, underwaterProjector, sunflare, should be all
        //and scatteringProjector not disabling correctly
        public void OnPreCull()
        {
            if (manager.GetOceanNode())
            {
                manager.GetOceanNode().SetWaterMeshrenderersEnabled (false);
            }
        }

        public void OnPostRender()
        {
            if (manager.GetOceanNode())
                manager.GetOceanNode().SetWaterMeshrenderersEnabled (true);
        }
    }
}

