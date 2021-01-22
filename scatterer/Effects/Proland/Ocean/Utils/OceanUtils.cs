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
		public static bool oceanRemoved = false;

		public static void removeStockOceansIfNotDone()
		{
			if (!oceanRemoved)
			{
				removeStockOceans();
				oceanRemoved = true;
			}
		}

		private static void removeStockOceans()
		{
			foreach (ScattererCelestialBody sctBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				if (sctBody.hasOcean)
				{
					bool removed = false;
					var celBody = Scatterer.Instance.scattererCelestialBodiesManager.CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
					if (celBody == null) {
						celBody = Scatterer.Instance.scattererCelestialBodiesManager.CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
					}
					
					if (celBody != null)
					{
						PQS pqs = celBody.pqsController;
						if ((pqs != null) && (pqs.ChildSpheres != null) && (pqs.ChildSpheres.Count () != 0)) {
							
							PQS ocean = pqs.ChildSpheres [0];
							if (ocean != null)
							{
								GameObject go = new GameObject ("Scatterer stock ocean disabler "+sctBody.celestialBodyName);
								FakeOceanPQS fakeOcean = go.AddComponent<FakeOceanPQS> ();
								fakeOcean.Apply (ocean);
								
								removed = true;
							}
						}
					}
					if (!removed)
					{
						Utils.LogDebug ("Couldn't remove stock ocean for " + sctBody.celestialBodyName);
					}
				}
			}
			Utils.LogDebug ("Removed stock oceans");
		}


		//We can disable scatterer oceans and re-enable stock oceans without restarting the game
		//but once started the stock ocean "pollutes" the scene with gameObjects that remain and lower performance so it isn't recommended but the option is there
		public static void restoreOceansIfNeeded()
		{
			if (oceanRemoved)
			{
				FakeOceanPQS[] fakes = (FakeOceanPQS[])FakeOceanPQS.FindObjectsOfType (typeof(FakeOceanPQS));

				foreach (FakeOceanPQS fake in fakes)
				{
					fake.Remove ();
					Component.Destroy(fake);
				}

				oceanRemoved = false;

				Utils.LogDebug ("Stock oceans restored");
			}
		}
		
		public static void restoreOceanForBody(ScattererCelestialBody sctBody)
		{
			if (oceanRemoved && sctBody.hasOcean)
			{
				GameObject go = GameObject.Find ("Scatterer stock ocean disabler "+sctBody.celestialBodyName);
				if (!ReferenceEquals(go,null))
				{
					FakeOceanPQS fake = go.GetComponent<FakeOceanPQS>();
					if (!ReferenceEquals(fake,null))
					{
						fake.Remove ();
						Component.Destroy(fake);
					}
				}
			}
		}
	}
}

