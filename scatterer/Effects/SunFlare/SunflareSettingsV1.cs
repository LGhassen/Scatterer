using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;

namespace scatterer
{
	public class SunflareSettingsV1
	{
		[Persistent]
		public int syntaxVersion = 1;

		[Persistent]
		public string assetPath;
		
		[Persistent]
		public float sunGlareFadeDistance = 250000;
		[Persistent]
		public float ghostFadeDistance = 13500000;
		
		//input settings
		[Persistent]
		public Vector3 flareSettings = Vector3.zero;
		[Persistent]
		public Vector3 spikesSettings = Vector3.zero;
		
		[Persistent]
		public List<Vector4> ghost1SettingsList1=new List<Vector4>{};
		[Persistent]
		public List<Vector4> ghost1SettingsList2=new List<Vector4>{};
		
		[Persistent]
		public List<Vector4> ghost2SettingsList1=new List<Vector4>{};
		[Persistent]
		public List<Vector4> ghost2SettingsList2=new List<Vector4>{};
		
		[Persistent]
		public List<Vector4> ghost3SettingsList1=new List<Vector4>{};
		[Persistent]
		public List<Vector4> ghost3SettingsList2=new List<Vector4>{};
		
		[Persistent]
		public Vector3 flareColor = Vector3.one;

		public SunflareSettingsV1 ()
		{
		}
	}
}

