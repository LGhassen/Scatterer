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
		SunlightModulator targetModulator;

		public SunlightModulatorPostRenderHook ()
		{
		}

		public void Init(SunlightModulator target)
		{
			targetModulator = target;
		}

		public void OnPostRender()
		{
			targetModulator.restoreOriginalColor ();
		}

		public void OnDestroy()
		{
			targetModulator.postRenderHook = null;
		}
	}
}

