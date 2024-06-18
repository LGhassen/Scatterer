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


namespace Scatterer
{
	public class QualityPresetsLoader
	{
		public static String[] GetPresetsList()
		{
			List<String> presetStrings = new List<String> ();

			UrlDir.UrlConfig[] presetsList = GameDatabase.Instance.GetConfigs ("Scatterer_quality_preset");

			foreach (UrlDir.UrlConfig _url in presetsList)
			{
				ConfigNode[] configNodeArray = _url.config.GetNodes("Quality_preset");
				
				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name"))
					{
						presetStrings.Add(_cn.GetValue("name"));
					}
				}
			}

			return presetStrings.ToArray();
		}

		public static void LoadPresetIntoMainSettings(MainSettingsReadWrite settings, string presetName)
		{
			List<String> presetStrings = new List<String> ();
			
			UrlDir.UrlConfig[] presetsList = GameDatabase.Instance.GetConfigs ("Scatterer_quality_preset");
			
			foreach (UrlDir.UrlConfig _url in presetsList)
			{
				ConfigNode[] configNodeArray = _url.config.GetNodes("Quality_preset");
				
				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name") && (_cn.GetValue("name") == presetName))
					{
						LoadConfigNodeIntoMainSettings(settings, _cn);
						break;
					}
				}
			}
		}

		private static void LoadConfigNodeIntoMainSettings(MainSettingsReadWrite settings, ConfigNode cn)
		{
			MainSettingsReadWrite tempMainSettings = new MainSettingsReadWrite();
			Type targetType = tempMainSettings.GetType ();

			ConfigNode.LoadObjectFromConfig (tempMainSettings, cn);

			foreach (FieldInfo fi in targetType.GetFields())
			{
				if (cn.HasValue(fi.Name))
				{
					fi.SetValue(settings, fi.GetValue(tempMainSettings));
				}
			}
		}

		public static string FindPresetOfCurrentSettings(MainSettingsReadWrite settings)
		{	
			UrlDir.UrlConfig[] presetsList = GameDatabase.Instance.GetConfigs ("Scatterer_quality_preset");
			
			foreach (UrlDir.UrlConfig _url in presetsList)
			{
				ConfigNode[] configNodeArray = _url.config.GetNodes("Quality_preset");
				
				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name"))
					{
						if (PresetMatchesCurrentSettings(settings,_cn))
						{
							return _cn.GetValue("name");
						}
					}
				}
			}

			return ("No preset - custom settings");
		}


		private static bool PresetMatchesCurrentSettings(MainSettingsReadWrite settings, ConfigNode cn)
		{
			MainSettingsReadWrite tempMainSettings = new MainSettingsReadWrite();
			Type targetType = tempMainSettings.GetType ();
			
			ConfigNode.LoadObjectFromConfig (tempMainSettings, cn);
			bool match = true;
			bool compared = false;

			foreach (FieldInfo fi in targetType.GetFields())
			{
				if (cn.HasValue(fi.Name))
				{
					if (!CompareFields(fi, settings, tempMainSettings))
					{
						return false;
					}
					compared = true;
				}
			}
			return match && compared;
		}

		private static bool CompareFields(FieldInfo fi, MainSettingsReadWrite settings1, MainSettingsReadWrite settings2)
		{
			if (fi.FieldType == typeof(float))
			{
				return ((float)(fi.GetValue (settings1)) == (float)(fi.GetValue (settings2)));
			}
			else if (fi.FieldType == typeof(int))
			{
				return ((int)(fi.GetValue (settings1)) == (int)(fi.GetValue (settings2)));
			}
			else if (fi.FieldType == typeof(bool))
			{
				return ((bool)(fi.GetValue (settings1)) == (bool)(fi.GetValue (settings2)));
			}
			else if (fi.FieldType == typeof(Vector3))
			{
				return ((Vector3)(fi.GetValue (settings1)) == (Vector3)(fi.GetValue (settings2)));
			}

			Utils.LogError("Unhandled preset type? "+fi.FieldType.ToString());
			return false;
		}
	}
}

