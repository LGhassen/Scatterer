using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Scatterer
{
	public class ShaderReplacer
	{
		private static ShaderReplacer instance;
		public Dictionary<string, Shader> LoadedShaders = new Dictionary<string, Shader>();
		public Dictionary<string, ComputeShader> LoadedComputeShaders = new Dictionary<string, ComputeShader>();
		public Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();
		string path;

		public Dictionary<string, string> gameShaders = new Dictionary<string, string>();

		const string eveShaderPrefix = "EVE";
		const string scattererShaderPrefix = "Scatterer-EVE";

		private ShaderReplacer()
		{
			Init ();
		}
		
		public static ShaderReplacer Instance
		{
			get 
			{
				if (instance == null)
				{
					instance = new ShaderReplacer();
					Utils.LogDebug("ShaderReplacer instance created");
				}
				return instance;
			}
		}
		

		private void Init()
		{
			string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
			UriBuilder uri = new UriBuilder (codeBase);
			path = Uri.UnescapeDataString (uri.Path);
			path = Path.GetDirectoryName (path);

			LoadAssetBundle ();
		}

		public void LoadAssetBundle()
		{
			string shaderspath;

			if (Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith ("OpenGL"))
				shaderspath = path+"/shaders/scatterershaders-linux";   //fixes openGL on windows
			else
				if (Application.platform == RuntimePlatform.WindowsPlayer)
				shaderspath = path + "/shaders/scatterershaders-windows";
			else if (Application.platform == RuntimePlatform.LinuxPlayer)
				shaderspath = path+"/shaders/scatterershaders-linux";
			else
				shaderspath = path+"/shaders/scatterershaders-macosx";

			LoadedShaders.Clear ();
			LoadedComputeShaders.Clear ();
			LoadedTextures.Clear ();

			using (WWW www = new WWW("file://"+shaderspath))
			{
				AssetBundle bundle = www.assetBundle;
				Shader[] shaders = bundle.LoadAllAssets<Shader>();
				
				foreach (Shader shader in shaders)
				{
					//Utils.Log (""+shader.name+" loaded. Supported?"+shader.isSupported.ToString());
					LoadedShaders.Add(shader.name, shader);
				}

				ComputeShader[] computeShaders = bundle.LoadAllAssets<ComputeShader>();
				
				foreach (ComputeShader computeShader in computeShaders)
				{
					//Utils.LogInfo ("Compute shader "+computeShader.name+" loaded.");
					LoadedComputeShaders.Add(computeShader.name, computeShader);
				}

				Texture[] textures = bundle.LoadAllAssets<Texture>();

				foreach (Texture texture in textures)
				{
					LoadedTextures.Add(texture.name, texture);
				}

				bundle.Unload(false); // unload the raw asset bundle
				www.Dispose();
			}
		}

		public void replaceEVEshaders()
		{
			Utils.LogDebug ("Replacing EVE shaders");

			Type EVEshaderLoaderType = getType ("ShaderLoader.ShaderLoaderClass");

			if (EVEshaderLoaderType == null)
			{
				Utils.LogDebug("Eve shaderloader type not found");
				return;
			}

			Dictionary<string, Shader> EVEshaderDictionary = null;

			Utils.LogDebug("Eve shaderloader type found");
			Utils.LogDebug("Eve shaderloader version: " + EVEshaderLoaderType.Assembly.GetName().ToString());

			const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

			try
			{
				EVEshaderDictionary = EVEshaderLoaderType.GetField("shaderDictionary", flags).GetValue(null) as Dictionary<string, Shader>;
			}
			catch (Exception)
			{
				Utils.LogDebug("No EVE shader dictionary found");
				return;
			}

			Utils.LogDebug("Successfully grabbed EVE shader dictionary");

			var shadersToReplace = new List<string>() { "Cloud", "CloudVolumeParticle", "GeometryCloudVolumeParticle", "GeometryCloudVolumeParticleToTexture", "RaymarchCloud", "CompositeRaymarchedClouds", "ReconstructRaymarchedClouds" };

			foreach (var shaderName in shadersToReplace)
            {
				ReplaceOrAddShader(shaderName, EVEshaderDictionary);
			}

			//replace shaders of already created materials
			Material[] materials = Resources.FindObjectsOfTypeAll<Material>();
			foreach (Material mat in materials)
			{
					ReplaceShaderInMaterial(mat, shadersToReplace);
			}
		}

		public void ReplaceOrAddShader(string shadername, Dictionary<string, Shader> eveShaderDictionary)
		{
			string eveShaderName = eveShaderPrefix + "/" + shadername;
			string scattererShaderName = scattererShaderPrefix + "/" + shadername;

			if (LoadedShaders.ContainsKey(scattererShaderName))
			{
				if (eveShaderDictionary.ContainsKey(eveShaderName))
				{
					eveShaderDictionary[eveShaderName] = LoadedShaders[scattererShaderName];
				}
				else
				{
					eveShaderDictionary.Add(eveShaderName, LoadedShaders[scattererShaderName]);
				}

				Utils.LogDebug("replaced "+ shadername +" in EVE shader dictionary");
			}
			else
				Utils.LogDebug("Shader " + scattererShaderName + " not loaded?");
		}


		private void ReplaceShaderInMaterial(Material mat, List<string> shadersToReplace)
		{
			String name = mat.shader.name;

			if (name.StartsWith(eveShaderPrefix) && shadersToReplace.Contains(name.Substring(eveShaderPrefix.Length+1)))
			{
				Utils.LogDebug("replacing " + name);
				string replacementShaderName = scattererShaderPrefix + "/" + name.Substring(eveShaderPrefix.Length + 1);

				if (LoadedShaders.ContainsKey(replacementShaderName))
				{
					mat.shader = LoadedShaders[replacementShaderName];
					Utils.LogDebug("Shader replaced");
				}
				else
				{
					Utils.LogDebug("Shader " + replacementShaderName + " not loaded?");
				}
			}
		}

		internal static Type getType(string name)
		{
			Type type = null;
			AssemblyLoader.loadedAssemblies.TypeOperation(t =>
			{
				if (t.FullName == name)
					type = t;
			});
			
			if (type != null)
			{
				return type;
			}
			return null;
		}
	}
}