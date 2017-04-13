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
		DisableEffectsForTextureReplacer effectsDisabler;
		public SkyNode skynode;

		public DisableEffectsChecker ()
		{
		}

		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam)
				return;

			if (cam.name == "TRReflectionCamera" && !effectsDisabler)
			{
				effectsDisabler = (DisableEffectsForTextureReplacer) cam.gameObject.AddComponent(typeof(DisableEffectsForTextureReplacer));

				Debug.Log("adding postprocesscube");
				effectsDisabler.postProcessingCube=skynode.atmosphereMesh.GetComponent<MeshRenderer>();

				if (skynode.m_manager.hasOcean && Core.Instance.useOceanShaders)
				{
					Debug.Log("adding watermeshrenderers");
					effectsDisabler.waterMeshRenderers = skynode.m_manager.GetOceanNode().waterMeshRenderers;
					Debug.Log("adding numgrid");
					effectsDisabler.numGrids = skynode.m_manager.GetOceanNode().numGrids;
				}
				Debug.Log("[Scatterer] Post-processing and ocean effects disabled from Texture Replacer reflections");
			}

		}

		public void OnDestroy()
		{
			if (effectsDisabler) 
			{
				Component.Destroy (effectsDisabler);
				UnityEngine.Object.Destroy (effectsDisabler);
			}
		}
	}
}

