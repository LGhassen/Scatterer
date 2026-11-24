using System;
using System.Collections;
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

        public static Vector3Int stbnDimensions = new Vector3Int(128, 128, 64);
        public static Texture2D stbn;

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
            path = GetPluginPath();

            LoadAssets();
        }

        public void LoadAssetBundle()
        {
            LoadAssets();
        }

        private void LoadAssets()
        {
            FinishAssetBundleLoad();

            LoadedShaders.Clear ();
            LoadedComputeShaders.Clear ();
            LoadedTextures.Clear ();

            if (bundle != null)
            {
                foreach (Shader shader in bundle.LoadAllAssets<Shader>())
                    LoadedShaders.Add(shader.name, shader);

                foreach (ComputeShader computeShader in bundle.LoadAllAssets<ComputeShader>())
                    LoadedComputeShaders.Add(computeShader.name, computeShader);

                foreach (Texture texture in bundle.LoadAllAssets<Texture>())
                    LoadedTextures.Add(texture.name, texture);

                // The loaded assets stay alive, we just don't need the raw bundle any more
                bundle.Unload(false);
                bundle = null;
            }

            if (stbn == null)
            {
                stbn = new Texture2D((int)stbnDimensions.x, (int)(stbnDimensions.y * stbnDimensions.z), TextureFormat.R8, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Repeat
                };
                stbn.LoadRawTextureData(File.ReadAllBytes(path + "/stbn.R8"));
                stbn.Apply();

                Utils.LogDebug("Stbn loaded");
            }
        }

        public void ReplaceEVEshaders()
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

            var shadersToReplace = new List<string>() { "Cloud", "ScreenSpaceCloudShadow", "CloudVolumeParticle", "GeometryCloudVolumeParticle", "GeometryCloudVolumeParticleToTexture", "RaymarchCloud", "CompositeRaymarchedClouds", "ReconstructRaymarchedClouds" };

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
    
        #region Asset Bundle Preloading
        private static AssetBundle bundle;
        private static AssetBundleCreateRequest bundleCreateRequest;
        private static AssetBundleRequest assetsRequest;

        private static string GetPluginPath()
        {
            string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
            UriBuilder uri = new UriBuilder (codeBase);
            return Path.GetDirectoryName (Uri.UnescapeDataString (uri.Path));
        }

        private static string GetBundlePath()
        {
            string bundleName = Application.platform == RuntimePlatform.WindowsPlayer && SystemInfo.graphicsDeviceVersion.StartsWith("Direct")
                ? "scatterershaders-directx"
                : "scatterershaders-opengl";

            return Path.Combine(GetPluginPath(), "shaders", bundleName);
        }

        private static void FinishAssetBundleLoad()
        {
            if (bundleCreateRequest != null)
            {
                // Blocks until the async load completes
                bundle = bundleCreateRequest.assetBundle;
                bundleCreateRequest = null;
            }

            if (bundle == null)
                bundle = AssetBundle.LoadFromFile(GetBundlePath());

            if (bundle == null)
            {
                Utils.LogError("Failed to load shader asset bundle at " + GetBundlePath());
                assetsRequest = null;
                return;
            }

            if (assetsRequest != null)
            {
                // Wait for the async asset load so we don't unload the bundle underneath it
                _ = assetsRequest.allAssets;
                assetsRequest = null;
            }
        }

        internal static IEnumerator PreloadAssetBundle()
        {
            if (instance != null)
                yield break;

            var request = AssetBundle.LoadFromFileAsync(GetBundlePath());
            bundleCreateRequest = request;

            if (!request.isDone)
                yield return request;

            // Somebody has forcibly completed the bundle load
            if (bundleCreateRequest != request)
                yield break;

            bundleCreateRequest = null;
            bundle = request.assetBundle;

            if (bundle == null)
            {
                Utils.LogError("Failed to load shader asset bundle at " + GetBundlePath());
                yield break;
            }

            var assets = bundle.LoadAllAssetsAsync();
            assetsRequest = assets;

            if (!assets.isDone)
                yield return assets;

            // Somebody has forcibly completed the asset load
            if (assetsRequest != assets)
                yield break;

            assetsRequest = null;
            EnsureLoaded();
        }

        internal static void EnsureLoaded() => _ = Instance;
        #endregion
    }
}
