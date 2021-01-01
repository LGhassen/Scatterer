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

		public CelestialBody[] CelestialBodies;

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

		//consider moving parts to a co-routine, so that we only need to check one CB to enable/disable per frame
		//TODO: refactor
		void UpdateProlandManagers ()
		{
			pqsEnabledOnScattererPlanet = false;
			underwater = false;
			customOceanEnabledOnScattererPlanet = false;

			foreach (ScattererCelestialBody scattererCelestialBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				float distanceToCamera, distanceToShip = 0f;
				if (scattererCelestialBody.hasTransform)
				{
					distanceToCamera = Vector3.Distance (ScaledSpace.ScaledToLocalSpace (Scatterer.Instance.scaledSpaceCamera.transform.position), ScaledSpace.ScaledToLocalSpace (scattererCelestialBody.transform.position));
					//don't unload planet the player ship is close to if panning away in map view
					if (FlightGlobals.ActiveVessel)
					{
						distanceToShip = Vector3.Distance (FlightGlobals.ActiveVessel.transform.position, ScaledSpace.ScaledToLocalSpace (scattererCelestialBody.transform.position));
					}
					if (scattererCelestialBody.active)
					{
						if (distanceToCamera > scattererCelestialBody.unloadDistance && (distanceToShip > scattererCelestialBody.unloadDistance || distanceToShip == 0f))
						{
							scattererCelestialBody.m_manager.OnDestroy ();
							UnityEngine.Object.Destroy (scattererCelestialBody.m_manager);
							scattererCelestialBody.m_manager = null;
							scattererCelestialBody.active = false;
							callCollector = true;
							Utils.LogDebug ("Effects unloaded for " + scattererCelestialBody.celestialBodyName);
						}
						else
						{
							scattererCelestialBody.m_manager.Update ();
							{
								if (!scattererCelestialBody.m_manager.m_skyNode.inScaledSpace) {
									pqsEnabledOnScattererPlanet = true;
								}
								if (!ReferenceEquals (scattererCelestialBody.m_manager.GetOceanNode (), null) && pqsEnabledOnScattererPlanet)
								{
									customOceanEnabledOnScattererPlanet = true;
									underwater = scattererCelestialBody.m_manager.GetOceanNode ().isUnderwater;
								}
							}
						}
					}
					else
					{
						if (distanceToCamera < scattererCelestialBody.loadDistance && scattererCelestialBody.transform && scattererCelestialBody.celestialBody)
						{
							try
							{
								if (HighLogic.LoadedScene == GameScenes.TRACKSTATION || HighLogic.LoadedScene == GameScenes.MAINMENU)
								{
									scattererCelestialBody.hasOcean = false;
								}

								scattererCelestialBody.m_manager = new ProlandManager ();
								scattererCelestialBody.m_manager.Init (scattererCelestialBody);
								scattererCelestialBody.active = true;
								Scatterer.Instance.guiHandler.selectedConfigPoint = 0;
								Scatterer.Instance.guiHandler.displayOceanSettings = false;
								Scatterer.Instance.guiHandler.selectedPlanet = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.IndexOf (scattererCelestialBody);
								Scatterer.Instance.guiHandler.getSettingsFromSkynode ();

								if (!ReferenceEquals (scattererCelestialBody.m_manager.GetOceanNode (), null))
								{
									Scatterer.Instance.guiHandler.buildOceanGUI ();
								}
								callCollector = true;
								Utils.LogDebug ("Effects loaded for " + scattererCelestialBody.celestialBodyName);
							}
							catch (Exception exception)
							{

								if (HighLogic.LoadedScene != GameScenes.MAINMENU)
									Utils.LogError ("Effects couldn't be loaded for " + scattererCelestialBody.celestialBodyName + " because of exception: " + exception.ToString ());

								try
								{
									scattererCelestialBody.m_manager.OnDestroy ();
								}
								catch (Exception exception2)
								{
									Utils.LogDebug ("manager couldn't be removed for " + scattererCelestialBody.celestialBodyName + " because of exception: " + exception2.ToString ());
								}
								Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Remove (scattererCelestialBody);
								if (HighLogic.LoadedScene != GameScenes.MAINMENU)
									Utils.LogDebug ("" + scattererCelestialBody.celestialBodyName + " removed from active planets.");
								return;
							}
						}
					}
				}
			}
		}


		void findCelestialBodies()
		{
			CelestialBodies = (CelestialBody[])CelestialBody.FindObjectsOfType (typeof(CelestialBody));

			foreach (ScattererCelestialBody sctBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				var _idx = 0;

				Utils.LogDebug("Finding ScattererCelestialBody name: "+sctBody.celestialBodyName+". TransformName: "+sctBody.transformName);

				var celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.celestialBodyName);
				
				if (celBody == null)
				{
					Utils.LogDebug("ScattererCelestialBody not found by name, trying transformName");
					celBody = CelestialBodies.SingleOrDefault (_cb => _cb.bodyName == sctBody.transformName);
				}

				if (celBody == null)
				{
					Utils.LogError("ScattererCelestialBody "+sctBody.celestialBodyName+" not found by name, or transformName. Effects for this body won't be available.");
					continue;
				}
				else				
				{
					_idx = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.IndexOf (sctBody);
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
					sctBody.hasTransform = true;
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


		public void Cleanup()
		{
			foreach (ScattererCelestialBody scattererCelestialBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{	
				if (scattererCelestialBody.active)
				{
					scattererCelestialBody.m_manager.OnDestroy ();
					UnityEngine.Object.Destroy (scattererCelestialBody.m_manager);
					scattererCelestialBody.m_manager = null;
					scattererCelestialBody.active = false;
				}
			}
		}
	}
}

