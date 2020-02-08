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
	public class SunlightModulatorPostRenderHook : MonoBehaviour
	{
		public SunlightModulatorPostRenderHook ()
		{
		}

		public void OnPostRender()
		{
			Scatterer.Instance.sunlightModulatorInstance.restoreOriginalColor ();
		}
	}
}

