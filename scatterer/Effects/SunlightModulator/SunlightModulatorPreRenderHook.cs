using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	public class SunlightModulatorPreRenderHook : MonoBehaviour
	{
		public SunlightModulatorPreRenderHook ()
		{
		}

		public void OnPreCull() //needs to be onPreCull, onPreRender is too late
		{
			Core.Instance.sunlightModulatorInstance.applyColorModulation ();
		}
	}
}

