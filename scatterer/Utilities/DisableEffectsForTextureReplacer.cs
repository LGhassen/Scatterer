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

namespace scatterer
{
	public class DisableEffectsForTextureReplacer : MonoBehaviour
	{
		public Manager manager;

		public DisableEffectsForTextureReplacer ()
		{
		}

		public void OnPreCull()
		{
			manager.GetSkyNode().atmosphereMeshrenderer.enabled = false;
			if (manager.hasOcean && Core.Instance.useOceanShaders)
				manager.GetOceanNode ().setWaterMeshrenderersEnabled (false);

		}

		public void OnPostRender()
		{
			manager.GetSkyNode().atmosphereMeshrenderer.enabled = true;
			if (manager.hasOcean && Core.Instance.useOceanShaders)
				manager.GetOceanNode ().setWaterMeshrenderersEnabled (true);
		}
	}
}

