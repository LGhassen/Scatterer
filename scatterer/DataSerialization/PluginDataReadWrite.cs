using System;
using System.Collections;
using System.Collections.Generic;
using KSP;
using KSP.IO;
using UnityEngine;


namespace scatterer
{
	public partial class Core
	{
		class PluginDataReadWrite
		{
			[Persistent]
			string guiModifierKey1String=KeyCode.LeftAlt.ToString();
			
			[Persistent]
			string guiModifierKey2String=KeyCode.RightAlt.ToString();
			
			[Persistent]
			string guiKey1String=KeyCode.F10.ToString();
			
			[Persistent]
			string guiKey2String=KeyCode.F11.ToString();
			
			[Persistent]
			int scrollSectionHeight = 500;
			
			[Persistent]
			Vector2 inGameWindowLocation=Vector2.zero;
			
			
			public void loadPluginDataToCore ()
			{
				try
				{
					ConfigNode confNode = ConfigNode.Load (Core.instance.path + "/config/PluginData/pluginData.cfg");
					ConfigNode.LoadObjectFromConfig (this, confNode);
					
					Core.Instance.inGameWindowLocation=inGameWindowLocation;
					Core.Instance.scrollSectionHeight=scrollSectionHeight;
					Core.Instance.guiModifierKey1String=guiModifierKey1String;
					Core.Instance.guiModifierKey2String=guiModifierKey2String;
					Core.Instance.guiKey1String=guiKey1String;
					Core.Instance.guiKey2String=guiKey2String;
				}
				catch (Exception stupid)
				{
					Utils.Log("Couldn't load pluginData "+stupid.ToString());
				}
			}

			public void saveCorePluginData ()
			{
				try
				{
					inGameWindowLocation=Core.Instance.inGameWindowLocation;
					scrollSectionHeight=Core.Instance.scrollSectionHeight;
					guiModifierKey1String=Core.Instance.guiModifierKey1String;
					guiModifierKey2String=Core.Instance.guiModifierKey2String;
					guiKey1String=Core.Instance.guiKey1String;
					guiKey2String=Core.Instance.guiKey2String;

					ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
					cnTemp.Save (Core.instance.path + "/config/PluginData/pluginData.cfg");
				}
				catch (Exception stupid)
				{
					Utils.Log("Couldn't save pluginData "+stupid.ToString());
				}
			}
		}
	}
}