//Fixes slow game startup, thanks Linx and Poodmund for this

using System;
using UnityEngine;

namespace Scatterer
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class VsyncStartupFix : MonoBehaviour
	{
		public void Start()
		{
			FixVsync ();
		}
		

		private void FixVsync()
		{
			QualitySettings.vSyncCount = 0;
		}
	}
}

