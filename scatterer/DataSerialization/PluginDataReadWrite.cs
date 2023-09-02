using System;
using System.Collections;
using System.Collections.Generic;
using KSP;
using KSP.IO;
using UnityEngine;


namespace Scatterer
{
	public class PluginDataReadWrite
	{
		[Persistent]
		public string guiModifierKey1String=KeyCode.LeftAlt.ToString();
		
		[Persistent]
		public string guiModifierKey2String=KeyCode.RightAlt.ToString();
		
		[Persistent]
		public string guiKey1String=KeyCode.F10.ToString();
		
		[Persistent]
		public string guiKey2String=KeyCode.F11.ToString();
		
		[Persistent]
		public int scrollSectionHeight = 500;
		
		[Persistent]
		public Vector2 inGameWindowLocation=Vector2.zero;
		
		public KeyCode guiKey1, guiKey2, guiModifierKey1, guiModifierKey2 ;
		
		public void loadPluginData ()
		{
			try
			{
				ConfigNode confNode = ConfigNode.Load (Utils.PluginPath + "/config/PluginData/pluginData.cfg");
				ConfigNode.LoadObjectFromConfig (this, confNode);

				guiKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey1String);
				guiKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiKey2String);
				
				guiModifierKey1 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey1String);
				guiModifierKey2 = (KeyCode)Enum.Parse(typeof(KeyCode), guiModifierKey2String);
			}
			catch (Exception stupid)
			{
				Utils.LogError("Couldn't load pluginData "+stupid.ToString());
			}
		}
		
		public void savePluginData ()
		{
			try
			{
				var dir = Utils.PluginPath + "/config/PluginData";
				if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

				ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
				cnTemp.Save (dir + "/pluginData.cfg");
			}
			catch (Exception stupid)
			{
				Utils.LogError("Couldn't save pluginData "+stupid.ToString());
			}
		}
	}
}