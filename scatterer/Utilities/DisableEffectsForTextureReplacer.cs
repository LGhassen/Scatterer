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
		public MeshRenderer[] waterMeshRenderers;
		public MeshRenderer postProcessingCube;
		public int numGrids=0;

		public DisableEffectsForTextureReplacer ()
		{
		}

		public void OnPreCull()
		{
			postProcessingCube.enabled = false;

			for (int i=0; i < numGrids; i++)
			{
				waterMeshRenderers[i].enabled=false;
			}

		}

		public void OnPostRender()
		{
			postProcessingCube.enabled = true;

			for (int i=0; i < numGrids; i++)
			{
				waterMeshRenderers[i].enabled=true;
			}
			
		}
	}
}

