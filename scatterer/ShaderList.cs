using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, true)]
class DumpShaderList : MonoBehaviour
{

//	PQS kerbinOcean;

	void Start ()
	{

	}

	void Update ()
	{

//					 PQSMod[] pqsmods = Resources.FindObjectsOfTypeAll<PQSMod> ();
//		
////					sorted = new List<string> (MeshRenderers);
////					sorted.Sort ();
//		
////					using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/MeshRenderers.txt"))
//		//				foreach (var sh in sorted) {
//					for (int i=0;i<pqsmods.Length;i++)
//		//					file.WriteLine (sh.ToString ());
//					Debug.Log(pqsmods[i].ToString());


//			Shader[] shaderList = Resources.FindObjectsOfTypeAll<Shader> ();
//
////			List<string> sorted = new List<string> (shaderList);
////			sorted.Sort ();
//		
//			using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/shaders.txt"))
////				foreach (var sh in sorted) {
//			for (int i=0;i<shaderList.Length;i++)
//					file.WriteLine (shaderList[i].ToString ());
//				
//
//			Material[] materials = Resources.FindObjectsOfTypeAll<Material> ();
//
////			sorted = new List<string> (materials);
////			sorted.Sort ();
//
//			using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/LoadedShaders.txt"))
////				foreach (var sh in sorted) {
//			for (int i=0;i<materials.Length;i++)
//		{
////					file.WriteLine (sh.ToString ());
//			file.WriteLine(materials[i].ToString());
//				}

		
//			MeshRenderer[] MeshRenderers = Resources.FindObjectsOfTypeAll<MeshRenderer> ();

//			sorted = new List<string> (MeshRenderers);
//			sorted.Sort ();

//			using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/MeshRenderers.txt"))
////				foreach (var sh in sorted) {
//			for (int i=0;i<MeshRenderers.Length;i++)
////					file.WriteLine (sh.ToString ());
//			file.WriteLine(MeshRenderers[i].ToString());




//		if (!kerbinOcean) {
//			
//			pqscont [] PQSs = Resources.FindObjectsOfTypeAll<PQS> ();
//
//			for (int i=0; i<PQSs.Length; i++) {
////					Debug.Log(PQSs[i].ToString());
//				if (PQSs [i].ToString().StartsWith ("KerbinOcean")) {
//					kerbinOcean = PQSs [i];
//				}
//			}
//		}
//			else
//		{
//			kerbinOcean.enabled = false;
//			kerbinOcean.DeactivateSphere ();
//			Debug.Log("[DumpShaderList] Disabled kerbinOcean")	;
//		}

//		for (int i=0; i<MeshRenderers.Length; i++)
//		{
//
//			string name = MeshRenderers[i].material.ToString();
//			if (name.StartsWith("KerbinWater")  /*|| name.StartsWith("OceanMoonWater") || name.StartsWith("PQSOceanFallback")*/)
//			{
////				Debug.Log("Material: "+ name + "MR: "+ MeshRenderers[i].ToString());
////				MeshRenderers[i].enabled=false;
//
//				if (MeshRenderers[i].transform.parent)
//					Debug.Log(MeshRenderers[i].transform.parent.name);
//
//			}
//		}



		//			MeshRenderer[] MeshRenderers = Resources.FindObjectsOfTypeAll<MeshRenderer> ();

//			sorted = new List<string> (MeshRenderers);
//			sorted.Sort ();

//			using (System.IO.StreamWriter file = new System.IO.StreamWriter(KSPUtil.ApplicationRootPath + "/MeshRenderers.txt"))
////				foreach (var sh in sorted) {
//			for (int i=0;i<MeshRenderers.Length;i++)
////					file.WriteLine (sh.ToString ());
//			file.WriteLine(MeshRenderers[i].ToString());
		


//		var _unkn = MeshRenderers.Single(_mr => _mr.material.ToString().StartsWith("KerbinWater"));
//			if (_unkn )
//				Debug.Log(_unkn.ToString());
				

	
	}
}
