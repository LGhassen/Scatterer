using UnityEngine;
using System;

namespace scatterer
{
	public class SkySphereKSCUpdater : MonoBehaviour
	{
		public Transform parentLocalTransform;

		public SkySphereKSCUpdater()
		{
		}

		void Update ()
		{
			gameObject.transform.position = ScaledSpace.LocalToScaledSpace (parentLocalTransform.position);
		}
	}
}

