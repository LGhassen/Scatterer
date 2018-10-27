
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
		}

		public void OnPostRender()
		{
			flare.clearExtinction ();	
		}
	}
}

