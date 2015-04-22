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
		static Dictionary<String, Shader> shaderDictionary = new Dictionary<String, Shader>();
		private void Awake()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				GameEvents.onGameSceneLoadRequested.Add(GameSceneLoaded);
			}
		}
		
		public static Shader GetShaderFromName(String name)
		{
			if (shaderDictionary.ContainsKey(name))
			{
				return shaderDictionary[name];
			}
			else
			{
				//Assembly assembly = Assembly.GetExecutingAssembly();
				Shader shader = ShaderTool.GetShader2("ReplacementShaders/" +name + ".shader");
				shaderDictionary[name] = shader;
				return shader;
			}
		}
		
		private void GameSceneLoaded(GameScenes scene)
		{
			if (scene == GameScenes.SPACECENTER || scene == GameScenes.FLIGHT)
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
			case "KSP/Diffuse":
				replacementShader = GetShaderFromName("CompiledDiffuse");
				break;
			case "KSP/Unlit":
				replacementShader = GetShaderFromName("CompiledUnlit");
				break;
			case "KSP/Specular":
				replacementShader = GetShaderFromName("CompiledSpecular");
				break;
			case "KSP/Bumped":
				replacementShader = GetShaderFromName("CompiledBumped");
				break;
			case "KSP/Bumped Specular":
				replacementShader = GetShaderFromName("CompiledBumpedSpecular");
				break;
			case "KSP/Emissive/Specular":
				replacementShader = GetShaderFromName("CompiledEmissiveSpecular");
				break;
			case "KSP/Emissive/Bumped Specular":
				replacementShader = GetShaderFromName("CompiledEmissiveBumpedSpecular");
				break;
			case "Terrain/PQS/PQS Main - Optimised":
				replacementShader = GetShaderFromName("CompiledPQSMainOptimised");
				break;
			case "Terrain/PQS/PQS Main Shader":
				replacementShader = GetShaderFromName("CompiledPQSMainShader");
				break;
			case "Terrain/PQS/Ocean Surface Quad":
				replacementShader = GetShaderFromName("CompiledPQSOceanSurfaceQuad");
				break;
			case "Terrain/PQS/Ocean Surface Quad (Fallback)":
				replacementShader = GetShaderFromName("CompiledPQSOceanSurfaceQuadFallback");
				break;
			case "Terrain/PQS/Sphere Projection SURFACE QUAD (AP) ":
				replacementShader = GetShaderFromName("CompiledPQSProjectionAerialQuadRelative");
				break;
			case "Terrain/PQS/Sphere Projection SURFACE QUAD (Fallback) ":
				replacementShader = GetShaderFromName("CompiledPQSProjectionFallback");
				break;
			case "Terrain/PQS/Sphere Projection SURFACE QUAD":
				replacementShader = GetShaderFromName("CompiledPQSProjectionSurfaceQuad");
				break;
			case "Terrain/Scaled Planet (Simple)": //not this
				replacementShader = GetShaderFromName("CompiledScaledPlanetSimple");
				break;
			case "Terrain/Scaled Planet (RimAerial)": //not this
				replacementShader = GetShaderFromName("CompiledScaledPlanetRimAerial");
				break;
			case "Unlit/Transparent":	//not this one
				replacementShader = GetShaderFromName("CompiledUnlitAlpha");
				break;
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