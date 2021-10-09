// Manages loading,unloading and updating for all the Scatterer-enabled celestial bodies
// Will spawn/delete/update a ProlandManager for each body if within range

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
	public class ScattererCelestialBodiesManager
	{
		bool callCollector=false;		
		public bool underwater = false;
		
		bool pqsEnabledOnScattererPlanet = false;
		public bool isPQSEnabledOnScattererPlanet{get{return pqsEnabledOnScattererPlanet;}}

		bool customOceanEnabledOnScattererPlanet = false;
		public bool isCustomOceanEnabledOnScattererPlanet{get{return customOceanEnabledOnScattererPlanet;}}

		public ScattererCelestialBodiesManager ()
		{
		}

		public void Init()
		{
			findCelestialBodies ();
		}

		public void Update()
		{
			UpdateProlandManagers ();
			CallCollectorIfNeeded ();
		}
		
		void UpdateProlandManagers ()
		{
			pqsEnabledOnScattererPlanet = false;
			underwater = false;
			customOceanEnabledOnScattererPlanet = false;

			foreach (ScattererCelestialBody scattererCelestialBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				float minDistance;	//smallest distance to either the camera or ship
				if (scattererCelestialBody.isFound)
				{
					minDistance = Vector3.Distance (Scatterer.Instance.scaledSpaceCamera.transform.position, scattererCelestialBody.transform.position) * ScaledSpace.ScaleFactor;
					minDistance = FlightGlobals.ActiveVessel ? Mathf.Min(minDistance, Vector3.Distance (FlightGlobals.ActiveVessel.transform.position, ScaledSpace.ScaledToLocalSpace (scattererCelestialBody.transform.position))) : minDistance;

					if (scattererCelestialBody.active)
					{
						if (minDistance > scattererCelestialBody.unloadDistance)
						{
							unloadEffectsForBody(scattererCelestialBody);
							break;
						}
						else
						{
							updateBody (scattererCelestialBody, ref pqsEnabledOnScattererPlanet, ref underwater, ref customOceanEnabledOnScattererPlanet);
						}
					}
					else
					{	
						if (minDistance < scattererCelestialBody.loadDistance && scattererCelestialBody.transform && scattererCelestialBody.celestialBody)
						{
							loadEffectsForBody (scattererCelestialBody);
							break;
						}
					}
				}
			}
		}

		void findCelestialBodies()
		{
			foreach (ScattererCelestialBody sctBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				Utils.LogDebug("Finding ScattererCelestialBody name: "+sctBody.celestialBodyName+". TransformName: "+sctBody.transformName);

				var celBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
				
				if (celBody == null)
				{
					Utils.LogDebug("ScattererCelestialBody not found by name, trying transformName");
					celBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
				}

				if (celBody == null)
				{
					Utils.LogError("ScattererCelestialBody "+sctBody.celestialBodyName+" not found by name, or transformName. Effects for this body won't be available.");
					continue;
				}
				else				
				{
					Utils.LogDebug ("Found ScattererCelestialBody: " + sctBody.celestialBodyName + ", actual ingame name: " + celBody.GetName ());
				}
				
				sctBody.celestialBody = celBody;
				
				var sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.transformName);
				if (!sctBodyTransform)
				{
					sctBodyTransform = ScaledSpace.Instance.transform.FindChild (sctBody.celestialBodyName);
				}
				else
				{
					sctBody.transform = sctBodyTransform;
					sctBody.isFound = true;
				}
				sctBody.active = false;
			}
		}

		void CallCollectorIfNeeded()
		{
			//TODO: determine if still needed anymore, ie test without
			if (callCollector)
			{
				GC.Collect();
				callCollector=false;
			}
		}

		void loadEffectsForBody (ScattererCelestialBody scattererCelestialBody)
		{
			try
			{
				if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
					scattererCelestialBody.hasOcean = false;
				
				scattererCelestialBody.prolandManager = new ProlandManager ();
				scattererCelestialBody.prolandManager.Init (scattererCelestialBody);
				scattererCelestialBody.active = true;
				
				if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Contains (scattererCelestialBody))
				{
					Scatterer.Instance.guiHandler.loadPlanet(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.IndexOf (scattererCelestialBody));
				}
				else
				{
					throw new Exception ("Planet already removed from planets list");
				}
				
				callCollector = true;
				Utils.LogDebug ("Effects loaded for " + scattererCelestialBody.celestialBodyName);
			}
			catch (Exception exception)
			{
				if (HighLogic.LoadedScene != GameScenes.MAINMENU || !exception.Message.Contains("No correct main menu object found for "))
					Utils.LogError ("Effects couldn't be loaded for " + scattererCelestialBody.celestialBodyName + ", " + exception.ToString ());
				
				try {
					scattererCelestialBody.prolandManager.OnDestroy ();
				}
				catch (Exception exception2) {
					Utils.LogDebug ("manager couldn't be removed for " + scattererCelestialBody.celestialBodyName + " because of exception: " + exception2.ToString ());
				}
				
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Remove (scattererCelestialBody);
				
				if (HighLogic.LoadedScene != GameScenes.MAINMENU)
				{
					OceanUtils.restoreOceanForBody (scattererCelestialBody);
					Utils.LogDebug ("" + scattererCelestialBody.celestialBodyName + " removed from active planets.");
				}
				
				return;
			}
		}

		ScattererCelestialBody updateBody (ScattererCelestialBody scattererCelestialBody, ref bool inPqsEnabledOnScattererPlanet, ref bool inUnderwater, ref bool inCustomOceanEnabledOnScattererPlanet)
		{
			scattererCelestialBody.prolandManager.Update ();
			inPqsEnabledOnScattererPlanet = inPqsEnabledOnScattererPlanet || !scattererCelestialBody.prolandManager.skyNode.inScaledSpace;
			if (inPqsEnabledOnScattererPlanet && !ReferenceEquals (scattererCelestialBody.prolandManager.GetOceanNode (), null)) {
				inCustomOceanEnabledOnScattererPlanet = true;
				inUnderwater = scattererCelestialBody.prolandManager.GetOceanNode ().isUnderwater;
			}
			return scattererCelestialBody;
		}
		
		void unloadEffectsForBody(ScattererCelestialBody scattererCelestialBody)
		{
			scattererCelestialBody.prolandManager.OnDestroy ();
			UnityEngine.Object.DestroyImmediate (scattererCelestialBody.prolandManager);
			scattererCelestialBody.prolandManager = null;
			scattererCelestialBody.active = false;
			callCollector = true;
			Utils.LogDebug ("Effects unloaded for " + scattererCelestialBody.celestialBodyName);
		}

		public void Cleanup()
		{
			foreach (ScattererCelestialBody scattererCelestialBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{	
				if (scattererCelestialBody.active)
				{
					scattererCelestialBody.prolandManager.OnDestroy ();
					UnityEngine.Object.DestroyImmediate (scattererCelestialBody.prolandManager);
					scattererCelestialBody.prolandManager = null;
					scattererCelestialBody.active = false;
					Utils.LogDebug ("Effects unloaded for " + scattererCelestialBody.celestialBodyName);
				}
			}
		}
	}
}

