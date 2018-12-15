using UnityEngine;

namespace scatterer
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

