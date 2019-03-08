
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
	public class SunflareCameraHook : MonoBehaviour
	{
		public SunFlare flare;

		public SunflareCameraHook ()
		{
		}

		public void OnPreRender()
		{
			flare.updateProperties ();
			flare.sunglareMaterial.SetFloat("renderOnCurrentCamera",1.0f);
		}

		public void OnPostRender()
		{
			flare.clearExtinction ();
			flare.sunglareMaterial.SetFloat("renderOnCurrentCamera",0.0f);
		}
	}
}

