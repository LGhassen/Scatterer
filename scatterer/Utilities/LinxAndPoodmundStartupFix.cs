//Fixes slow game startup, thanks Linx and Poodmund for this

using System;
using UnityEngine;

namespace scatterer
{
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class LinxAndPoodmundStartupFix : MonoBehaviour
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

