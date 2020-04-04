
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

	public class DisableAmbientLight : MonoBehaviour
	{
		Color ambientLight, originalAmbientLight;

		private void Awake()
		{
			ambientLight = Color.black;
		}

		public void OnPreRender()
		{
			originalAmbientLight = RenderSettings.ambientLight;
			RenderSettings.ambientLight = ambientLight;
		}

		public void OnPostRender()
		{
			restoreLight ();
		}

		public void restoreLight()
		{
			RenderSettings.ambientLight = originalAmbientLight;
		}
	}
}

