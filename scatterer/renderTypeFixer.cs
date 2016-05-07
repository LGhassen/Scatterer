using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class renderTypeFixer : MonoBehaviour
	{
		static Dictionary<String, Shader> shaderDictionary = new Dictionary<String, Shader>();
		private void Awake()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				GameEvents.onGameSceneLoadRequested.Add(GameSceneLoaded);
			}
		}

		
		private void GameSceneLoaded(GameScenes scene)
		{
			if (scene == GameScenes.SPACECENTER || scene == GameScenes.FLIGHT)
			{
				Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
				foreach (Material mat in materials)
				{
					fixRenderType(mat);
				}
			}
		}
		
		public static void fixRenderType(Material mat)
		{
			String name = mat.shader.name;
			if ((name == "Terrain/PQS/PQS Main - Optimised")
			    || (name == "Terrain/PQS/PQS Main Shader")
			    || (name == "Terrain/PQS/Sphere Projection SURFACE QUAD (AP) ")
				|| (name == "Terrain/PQS/Sphere Projection SURFACE QUAD (Fallback) ")
			    || (name == "Terrain/PQS/Sphere Projection SURFACE QUAD")
				|| (name.Contains ("PQS Main - Extras")))
				
			{
				mat.SetOverrideTag("RenderType", "Opaque");
				Debug.Log("[Scatterer] Set renderType tag for "+mat.shader.name+" to \"Opaque\"");
			}
		}
	}
}