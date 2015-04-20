using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using KSP;
using UnityEngine;





[KSPAddon(KSPAddon.Startup.Flight, true)]
class DumpShaderList : MonoBehaviour
{
	void Start()
	{


		Shader[] shaderList =Resources.FindObjectsOfTypeAll<Shader> ();
		
		//Log.Normal("{0} loaded shaders", shaders.Count);
		//List<string> sorted = new List<string>(shaders); sorted.Sort();
		
		using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/shaders.txt"))
			//foreach (var sh in sorted)
			for (int i=0;i<shaderList.Length;i++)
		{
			file.WriteLine(shaderList[i].ToString());
		}

		Material[] materials = Resources.FindObjectsOfTypeAll<Material>();

		using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/LoadedShaders.txt"))
			//foreach (var sh in sorted)
			for (int i=0;i<materials.Length;i++)
		{
			file.WriteLine(materials[i].ToString());
			file.WriteLine(materials[i].shader.ToString());
		}
	}
}