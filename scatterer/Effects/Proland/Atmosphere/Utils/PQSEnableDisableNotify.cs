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
				targetSkyNode.SwitchEffectsScaled ();
		}

		public override void OnSphereActive()
		{
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
				this.order = 100;

				targetSkyNode = inSkyNode;
			}
		}
		
		public void Remove()
		{
			this.sphere = null;
			this.transform.parent = null;
			this.modEnabled = false;
			gameObject.DestroyGameObject();
		}
	}
}