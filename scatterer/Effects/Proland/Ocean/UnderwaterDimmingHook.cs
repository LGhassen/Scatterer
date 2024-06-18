using UnityEngine;

namespace Scatterer
{
	public class UnderwaterDimmingHook : MonoBehaviour
	{
		public OceanNode oceanNode;

		public void OnPostRender()
		{
			oceanNode.applyUnderwaterDimming ();
		}
	}
}

