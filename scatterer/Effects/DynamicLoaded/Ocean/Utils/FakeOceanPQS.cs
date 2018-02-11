using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;


//Thanks to rbray89 for this FakeOcean class which disable the stock ocean in a clean way
namespace scatterer
{
	public class FakeOceanPQS : PQSMod
	{
		public override void  OnSphereActive()
		{
			sphere.maxLevel = 0;
			sphere.minLevel = 0;
			KSPLog.print(sphere.transform.childCount);
			for (int i = 0; i < sphere.transform.childCount; i++)
			{
				Transform t = sphere.transform.GetChild(i);
				if(Regex.IsMatch(t.name, "[A-z][np]"))
					t.gameObject.SetActive(false);
			}
			sphere.maxLevel = 0;
			sphere.minLevel = 0;
		}
		
		public void Apply(PQS pqs)
		{
			if (pqs != null)
			{
				this.sphere = pqs;
				this.transform.parent = pqs.transform;
				this.requirements = PQS.ModiferRequirements.Default;
				this.modEnabled = true;
				this.order = 10;
				
				this.transform.localPosition = Vector3.zero;
				this.transform.localRotation = Quaternion.identity;
				this.transform.localScale = Vector3.one;
				
			}
		}
		
	}
}