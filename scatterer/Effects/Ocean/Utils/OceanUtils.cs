using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace scatterer
{
	public static class OceanUtils
	{
		public static void removeStockOceans()
		{
			Material invisibleOcean = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
			foreach (ScattererCelestialBody sctBody in Core.Instance.scattererCelestialBodies)
			{
				if (sctBody.hasOcean)
				{
					bool removed = false;
					var celBody = Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
					if (celBody == null)
					{
						celBody = Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
					}
					
					if (celBody != null)
					{
						//Thanks to rbray89 for this snippet and the FakeOcean class which disable the stock ocean in a clean way
						PQS pqs = celBody.pqsController;
						if ((pqs != null) && (pqs.ChildSpheres!= null) && (pqs.ChildSpheres.Count() != 0))
						{
							
							PQS ocean = pqs.ChildSpheres [0];
							if (ocean != null)
							{
								ocean.surfaceMaterial = invisibleOcean;
								ocean.surfaceMaterial.SetOverrideTag("IgnoreProjector","True");
								ocean.surfaceMaterial.SetOverrideTag("ForceNoShadowCasting","True");
								
								removed = true;
							}
						}
					}
					if (!removed) {
						Utils.Log ("Couldn't remove stock ocean for " + sctBody.celestialBodyName);
					}
				}
			}
			Utils.Log ("Removed stock oceans");
		}
	}
}

