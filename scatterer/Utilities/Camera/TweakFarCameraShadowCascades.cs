
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

	public class TweakFarCameraShadowCascades : MonoBehaviour
	{

		public TweakFarCameraShadowCascades()
		{

		}

		public void OnPreRender()
		{
			if (Scatterer.Instance.unifiedCameraEnabled)
			{
				QualitySettings.shadowCascade4Split = new Vector3(0.006f, 0.040f, 0.200f);
			}
			else
			{
				QualitySettings.shadowCascade4Split = new Vector3(0.005f, 0.025f, 0.125f);
			}
		}

		public void OnPostRender()
		{
			if (Scatterer.Instance.unifiedCameraEnabled)
			{
				QualitySettings.shadowCascade4Split = new Vector3(0.05041852f, 0.1527327f, 0.2643032f);
			}
		}

		public void restoreLight()
		{

		}
	}
}

