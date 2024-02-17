//used just to remove the postprocessing and the ocean from texture replacer's reflections because they look messed up and bog down performance
//this script gets added to the camera to disable the effects on

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
			if (manager.GetSkyNode ().localScatteringContainer != null)
			{
				manager.GetSkyNode().localScatteringContainer.SetActivated (false);
				manager.GetSkyNode().localScatteringContainer.UpdateContainer();
			}

			if (manager.GetOceanNode())
			{
				manager.GetOceanNode().setWaterMeshrenderersEnabled (false);
			}
		}

		public void OnPostRender()
		{
			if (manager.GetSkyNode ().localScatteringContainer != null)
				manager.GetSkyNode().localScatteringContainer.SetActivated (true);

			if (manager.GetOceanNode())
				manager.GetOceanNode().setWaterMeshrenderersEnabled (true);
		}
	}
}

