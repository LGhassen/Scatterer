//Just a small class to notify the skynode if the PQS enables or disables

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace scatterer
{
	public class PQSEnableDisableNotify : PQSMod
	{
		SkyNode targetSkyNode;

		public override void OnSphereInactive()
		{
			if (targetSkyNode!=null)
				targetSkyNode.SwitchEffectsScaled ();
		}

		public override void OnSphereActive()
		{
			if (targetSkyNode!=null)
				targetSkyNode.SwitchEffectsLocal ();
		}
		
		public void Apply(PQS pqs, SkyNode inSkyNode)
		{
			if (pqs != null)
			{
				this.sphere = pqs;
				this.transform.parent = pqs.transform;
				this.requirements = PQS.ModiferRequirements.Default;
				this.modEnabled = true;
				this.order += 10;

				this.transform.localPosition = Vector3.zero;
				this.transform.localRotation = Quaternion.identity;
				this.transform.localScale = Vector3.one;

				targetSkyNode = inSkyNode;
			}
		}
		
		public void Remove()
		{
			gameObject.DestroyGameObject();
		}
	}
}