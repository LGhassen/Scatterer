using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
	public abstract class GenericAntiAliasing  : MonoBehaviour
	{
		public GenericAntiAliasing ()
		{
		}

		public abstract void Cleanup();
	}
}

