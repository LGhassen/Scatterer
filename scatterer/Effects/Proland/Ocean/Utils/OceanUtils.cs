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
			Material invisibleOceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);

			FakeOceanPQS[] fakes = (FakeOceanPQS[])FakeOceanPQS.FindObjectsOfType (typeof(FakeOceanPQS));

			// if we haven't already added ocean disablers
			if (fakes.Length == 0)
			{
				foreach (ScattererCelestialBody sctBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
				{
					if (sctBody.hasOcean) {
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
									//Add the material to hide it in the first few frames when switching back from map mode, and also just in case
									ocean.surfaceMaterial = invisibleOceanMaterial;
									ocean.surfaceMaterial.SetOverrideTag ("IgnoreProjector", "True");
									ocean.surfaceMaterial.SetOverrideTag ("ForceNoShadowCasting", "True");

									GameObject go = new GameObject ();
									FakeOceanPQS fakeOcean = go.AddComponent<FakeOceanPQS> ();
									fakeOcean.Apply (ocean);

									removed = true;
								}
							}
						}
						if (!removed) {
							Utils.LogDebug ("Couldn't remove stock ocean for " + sctBody.celestialBodyName);
						}
					}
				}
				Utils.LogDebug ("Removed stock oceans");
			}
			else
			{
				Utils.LogDebug ("Stock oceans already removed");
			}
		}


		//Technically we could disable scatterer oceans and re-enable stock oceans without restarting the game if we wanted
		//but once started the stock ocean "pollutes" the scene with gameObjects that remain and lower our performance even
		//if we disable the stock ocean again, so we'll just require users to restart the game and have better performance
		public static void restoreOceansIfNeeded()
		{
			FakeOceanPQS[] fakes = (FakeOceanPQS[])FakeOceanPQS.FindObjectsOfType (typeof(FakeOceanPQS));

			foreach (FakeOceanPQS fake in fakes)
			{
				fake.Remove();
			}
		}
	}
}

