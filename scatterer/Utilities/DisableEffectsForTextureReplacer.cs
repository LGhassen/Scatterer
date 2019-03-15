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
//			if (!ReferenceEquals(null,manager.GetSkyNode ().localScatteringProjector))
//				manager.GetSkyNode ().localScatteringProjector.setActivated(false);
			if (!ReferenceEquals(manager.GetOceanNode (),null))
				manager.GetOceanNode ().setWaterMeshrenderersEnabled (false);

		}

		public void OnPostRender()
		{
//			if (!ReferenceEquals (null, manager.GetSkyNode ().localScatteringProjector))
//				manager.GetSkyNode ().localScatteringProjector.setActivated (true);
			if (!ReferenceEquals(manager.GetOceanNode (),null))
				manager.GetOceanNode ().setWaterMeshrenderersEnabled (true);
		}
	}
}

