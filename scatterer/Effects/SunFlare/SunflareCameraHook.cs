
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
		public float useDbufferOnCamera;

		public SunflareCameraHook ()
		{
		}

		public void OnPreRender()
		{
			if(!ReferenceEquals(flare,null))
			{
				flare.updateProperties ();
				flare.sunglareMaterial.SetFloat(ShaderProperties.renderOnCurrentCamera_PROPERTY,1.0f);
				flare.sunglareMaterial.SetFloat(ShaderProperties.useDbufferOnCamera_PROPERTY,useDbufferOnCamera);
			}
		}

		public void OnPostRender()
		{
			if(!ReferenceEquals(flare,null))
			{
				flare.ClearExtinction ();
				flare.sunglareMaterial.SetFloat(ShaderProperties.renderOnCurrentCamera_PROPERTY,0.0f);
				flare.sunglareMaterial.SetFloat(ShaderProperties.useDbufferOnCamera_PROPERTY,useDbufferOnCamera);
			}
		}
	}
}

