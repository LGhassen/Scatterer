// Exists only to notify the screenCopy commandBuffer that a screen copy is needed for this frame

using System;
using UnityEngine;

namespace scatterer
{
	public class ScreenCopierNotifier : MonoBehaviour
	{
		public bool isEnabled = false;

		public ScreenCopierNotifier ()
		{
		}

		void OnWillRenderObject()
		{
			ScreenCopyCommandBuffer.EnableForThisFrame (Camera.current);
		}
	}
}

