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
		SunlightModulator targetModulator;

		public SunlightModulatorPreRenderHook ()
		{
		}

		public void Init(SunlightModulator target)
		{
			targetModulator = target;
		}

		public void OnPreCull() //needs to be onPreCull, onPreRender is too late
		{
			targetModulator.applyColorModulation ();
		}
	}
}

