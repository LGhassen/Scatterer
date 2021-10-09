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
	public class SunflareGUI
	{
		String[] sunflareStrings;
		
		int selSunflareGridInt = 0;
		private Vector2 sunflareListScrollPosition = new Vector2();
		bool editingSunflare = false;
		string sunflareText = "";
		private Vector2 sunflareEditScrollPosition = new Vector2();

		public SunflareGUI ()
		{
		}

		public void InitSunflareGUI()
		{
			sunflareStrings = new string[Scatterer.Instance.sunflareManager.scattererSunFlares.Count];
			
			for (int i=0; i<Scatterer.Instance.sunflareManager.scattererSunFlares.Count; i++)
			{
				sunflareStrings[i] = Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(i).Value.sourceName;
			}
		}

		public void DrawSunflareGUI()
		{
			GUILayout.BeginVertical ();
			sunflareListScrollPosition = GUILayout.BeginScrollView(sunflareListScrollPosition, false, true, GUILayout.MinHeight(100));
			selSunflareGridInt = GUILayout.SelectionGrid (selSunflareGridInt, sunflareStrings, 1);
			GUILayout.EndScrollView();
			if (GUILayout.Button ("Edit Selected"))
			{
				sunflareText = string.Copy(Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.configNodeToLoad.ToString());
				editingSunflare = true;
			}
			GUILayout.EndVertical ();
			
			if (editingSunflare)
			{
				sunflareEditScrollPosition = GUILayout.BeginScrollView(sunflareEditScrollPosition, false, true, GUILayout.Width(600) ,GUILayout.MinHeight(Scatterer.Instance.pluginData.scrollSectionHeight + 100));
				sunflareText = GUILayout.TextArea(sunflareText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				GUILayout.EndScrollView();
				
				GUILayout.BeginHorizontal ();
				
				if (GUILayout.Button("Reimport"))
				{
					sunflareText = string.Copy(Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.configNodeToLoad.ToString());
				}
				
				if (GUILayout.Button("Apply"))
				{
					ConfigNode node = ConfigNode.Parse(sunflareText);
					Utils.LogInfo("Applying sunflare config from UI:\r\n"+sunflareText);
					Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.ApplyFromUI(node.GetNode("Sun"));
					
				}
				
				if (GUILayout.Button("Copy to clipboard"))
				{
					GUIUtility.systemCopyBuffer = sunflareText;
				}
				
				if (GUILayout.Button ("Print to Log"))
				{
					Utils.LogInfo("Sunflare config print to log:\r\n"+sunflareText);
				}
				
				GUILayout.EndHorizontal ();
			}
		}
	}
}

