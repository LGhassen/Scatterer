using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using System.Collections;

namespace scattererShaders
{

	public class CreateAssetBundles
	{
		[MenuItem ("Assets/Build AssetBundles")]
		static void BuildAllAssetBundles ()
		{
			// Put the bundles in a folder called "AssetBundles"
			//var outDir = "Assets/AssetBundles";
			var outDir = "D:/gh/Steam/steamapps/common/Kerbal Space Program/GameData/scatterer/shaders";

			if (!Directory.Exists (outDir))
				Directory.CreateDirectory (outDir);

			var opts = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ForceRebuildAssetBundle;

			BuildTarget[] platforms = { BuildTarget.StandaloneWindows, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 };
			string[] platformExts = { "-windows", "-macosx", "-linux" };
			for (var i = 0; i < platforms.Length; ++i)
			{
				BuildPipeline.BuildAssetBundles(outDir, opts, platforms[i]);
				var outFile = outDir + "/scatterershaders" + platformExts[i];
				FileUtil.ReplaceFile(outDir + "/scatterershaders", outFile);
			}


			//cleanup
			foreach (string file in Directory.GetFiles(outDir, "*.*").Where(item => (item.EndsWith(".meta") || item.EndsWith(".manifest"))))
			{
				File.Delete(file);
			}
			File.Delete (outDir + "/CompiledAssetBundles");
			File.Delete(outDir+"/scatterershaders");
			File.Delete(outDir+"/shaders");
		}
	}

}