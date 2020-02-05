//note: allows adding a material to PQS, not currently in use as projector works better
//could be adapted for use with future grass/trees shader however

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Collections.Generic;


namespace scatterer
{
	public class AtmospherePQS : PQSMod
	{
		Material atmosphereMaterial;

		public override void  OnSphereActive()
		{
		}

		public override void OnQuadCreate(PQ quad)
		{
			List<Material> materials = new List<Material> (quad.meshRenderer.sharedMaterials);
			materials.Add (atmosphereMaterial);
			quad.meshRenderer.sharedMaterials= materials.ToArray();
		}

		public override void OnQuadDestroy(PQ quad)
		{
			List<Material> materials = new List<Material> (quad.meshRenderer.sharedMaterials);
			materials.Remove(materials.Find(mat => mat.shader.name == "Scatterer/AtmosphericLocalScatter")); //probably slow
			quad.meshRenderer.sharedMaterials = materials.ToArray ();
		}

		public void Apply(PQS pqs)
		{
			if (pqs != null)
			{
				this.sphere = pqs;
				this.transform.parent = pqs.transform;
				this.requirements = PQS.ModiferRequirements.Default;
				this.modEnabled = true;
				this.order = 10; //?
				
				this.transform.localPosition = Vector3.zero;
				this.transform.localRotation = Quaternion.identity;
				this.transform.localScale = Vector3.one;

				atmosphereMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);

				Utils.LogDebug ("AtmospherePQS applied");
			}
		}

		public void Cleanup()
		{
			Utils.LogDebug ("AtmospherePQS removed");
		}
		
	}
}