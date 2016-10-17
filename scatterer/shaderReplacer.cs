using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
//using Utils;

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)]
	public class ShaderReplacer : MonoBehaviour
	{
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
//			if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.FLIGHT)
			{
				Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
				foreach (Material mat in materials)
				{
					ReplaceShader(mat);
				}
			}
		}
		
		public static void ReplaceShader(Material mat)
		{
			String name = mat.shader.name;
			Shader replacementShader = null;
			switch (name)
			{
				//			case "KSP/Diffuse":
				//				replacementShader = GetShaderFromName("CompiledDiffuse");
				//				break;
				//			case "KSP/Unlit":
				//				replacementShader = GetShaderFromName("CompiledUnlit");
				//				break;
				//			case "KSP/Specular":
				//				replacementShader = GetShaderFromName("CompiledSpecular");
				//				break;
				//			case "KSP/Bumped":
				//				replacementShader = GetShaderFromName("CompiledBumped");
				//				break;
				//			case "KSP/Bumped Specular":
				//				replacementShader = GetShaderFromName("CompiledBumpedSpecular");
				//				break;
				//			case "KSP/Emissive/Specular":
				//				replacementShader = GetShaderFromName("CompiledEmissiveSpecular");
				//				break;
				//			case "KSP/Emissive/Bumped Specular":
				//				replacementShader = GetShaderFromName("CompiledEmissiveBumpedSpecular");
				//				break;
//			case "Terrain/PQS/PQS Main - Optimised":
//				Debug.Log("[Scatterer] replacing Terrain/PQS/PQS Main - Optimised");
////				replacementShader = GetShaderFromName("ScattererPQS");
//				Debug.Log("[Scatterer] Shader replaced");
//				break;
//			case "Scatterer/Terrain - test":
//				Debug.Log("[Scatterer] replacing Scatterer/Terrain - test");
//				replacementShader = GetShaderFromName("ScattererPQS");
//				Debug.Log("[Scatterer] Shader replaced");
//				break;
//			case "Terrain/PQS/Sphere Projection SURFACE QUAD":
//				Debug.Log("[Scatterer] replacing Terrain/PQS/Sphere Projection SURFACE QUAD");
//				replacementShader = GetShaderFromName("PQSProjectionSurfaceQuad");
//				Debug.Log("[Scatterer] Shader replaced");
//				break;

			case "EVE/Cloud":
				Debug.Log("[Scatterer] replacing EVE/Cloud");
				replacementShader = Core.Instance.LoadedShaders["Scatterer-EVE/Cloud"];
//				MeshRenderer[] meshrenderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
//				foreach (MeshRenderer _mr in meshrenderers)
//				{
//					if ((_mr.material == mat) || (_mr.material.shader.name == mat.shader.name)) 
//					{
//						Debug.Log("parent of EVE/Cloud" + _mr.gameObject.name);
//						Debug.Log("parent transform" + _mr.gameObject.transform.parent.gameObject.name);
//					}
//				}
				Debug.Log("[Scatterer] Shader replaced");
				break;
			case "Scatterer-EVE/Cloud":
				Debug.Log("[Scatterer] replacing EVE/Cloud");
				replacementShader = Core.Instance.LoadedShaders["Scatterer-EVE/Cloud"];
				Debug.Log("[Scatterer] Shader replaced");
				break;

//			case "EVE/CloudVolumeParticle":
//				Debug.Log("[Scatterer] EVE/CloudVolumeParticle");
//				replacementShader = GetShaderFromName("CloudVolumeParticle");
//				Debug.Log("[Scatterer] Shader replaced");
//				break;


//			case "Terrain/PQS/PQS Main Shader":
//				replacementShader = GetShaderFromName("CompiledPQSMainShader");
//				break;
//			case "Terrain/PQS/Ocean Surface Quad":
//				replacementShader = GetShaderFromName("CompiledPQSOceanSurfaceQuad");
//				break;
//			case "Terrain/PQS/Ocean Surface Quad (Fallback)":
//				replacementShader = GetShaderFromName("CompiledPQSOceanSurfaceQuadFallback");
//				break;
//			case "Terrain/PQS/Sphere Projection SURFACE QUAD (AP) ":
//				replacementShader = GetShaderFromName("CompiledPQSProjectionAerialQuadRelative");
//				break;
//			case "Terrain/PQS/Sphere Projection SURFACE QUAD (Fallback) ":
//				replacementShader = GetShaderFromName("CompiledPQSProjectionFallback");
//				break;
//			case "Terrain/PQS/Sphere Projection SURFACE QUAD":
//				replacementShader = GetShaderFromName("CompiledPQSProjectionSurfaceQuad");
//				break;
//			case "Terrain/Scaled Planet (Simple)": 
//				replacementShader = GetShaderFromName("CompiledScaledPlanetSimple");
//				break;
//			case "Terrain/Scaled Planet (RimAerial)": 
//				replacementShader = GetShaderFromName("CompiledScaledPlanetRimAerial");
//				break;
				//			case "Unlit/Transparent":	
				//				replacementShader = GetShaderFromName("CompiledUnlitAlpha");
				//				break;
			default:
				return;
				
			}
			if (replacementShader != null)
			{
				mat.shader = replacementShader;
			}
		}
	}
}