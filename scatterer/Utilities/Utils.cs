using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace scatterer
{
	public static class Utils
	{
		public static GameObject GetMainMenuObject(string name)
		{
			GameObject kopernicusMainMenuObject = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault
				(b => b.name == (name+"(Clone)") && b.transform.parent.name.Contains("Scene"));
			
			if (kopernicusMainMenuObject != null)
				return kopernicusMainMenuObject;
			
			GameObject kspMainMenuObject = GameObject.FindObjectsOfType<GameObject>().FirstOrDefault(b => b.name == name && b.transform.parent.name.Contains("Scene"));
			
			if (kspMainMenuObject == null)
			{
				throw new Exception("No correct main menu object found for "+name);
			}
			
			return kspMainMenuObject;
		}

		public static Transform GetScaledTransform (string body)
		{
			return (ScaledSpace.Instance.transform.FindChild (body));	
		}

		public static void FixKopernicusRingsRenderQueue ()
		{
			foreach (CelestialBody _cb in Core.Instance.CelestialBodies) {
				GameObject ringObject;
				ringObject = GameObject.Find (_cb.name + "Ring");
				if (ringObject) {
					ringObject.GetComponent<MeshRenderer> ().material.renderQueue = 3005;
					Utils.LogDebug ("Found rings for " + _cb.name);
				}
			}
		}
		
		public static void FixSunsCoronaRenderQueue ()
		{
			foreach(ScattererCelestialBody _scattererCB in Core.Instance.scattererCelestialBodies)
			{
				Transform scaledSunTransform = Utils.GetScaledTransform (_scattererCB.mainSunCelestialBody);
				foreach (Transform child in scaledSunTransform) {
					MeshRenderer temp = child.gameObject.GetComponent<MeshRenderer> ();
					if (temp != null)
						temp.material.renderQueue = 2998;
				}
			}
		}

		public static void LogDebug(string log)
		{
			Debug.Log ("[Scatterer][Debug] " + log);
		}
		
		public static void LogInfo(string log)
		{
			Debug.Log ("[Scatterer][Info] " + log);
		}
		
		public static void LogError(string log)
		{
			Debug.Log ("[Scatterer][Error] " + log);
		}

		private static string pluginPath;
		public static string PluginPath
		{
			get
			{
				if (ReferenceEquals(null,pluginPath))
				{
					string codeBase = Assembly.GetExecutingAssembly ().CodeBase;
					UriBuilder uri = new UriBuilder (codeBase);
					pluginPath = Uri.UnescapeDataString (uri.Path);
					pluginPath = Path.GetDirectoryName (pluginPath);					
				}
				return pluginPath;
			}
		}

		private static string gameDataPath;
		public static string GameDataPath
		{
			get
			{
				if (ReferenceEquals(null,gameDataPath))
				{
					gameDataPath= KSPUtil.ApplicationRootPath + "GameData/";				
				}
				return gameDataPath;
			}
		}
	}
}

