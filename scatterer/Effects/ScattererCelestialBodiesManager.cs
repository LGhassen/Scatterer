// Manages loading,unloading and updating for all the Scatterer-enabled celestial bodies
// Will spawn/delete/update a ProlandManager for each body if within range
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scatterer
{
	public class ScattererCelestialBodiesManager
	{	
		public bool underwater = false;
		
		bool pqsEnabledOnScattererPlanet = false;
		public bool isPQSEnabledOnScattererPlanet{get{return pqsEnabledOnScattererPlanet;}}

		bool customOceanEnabledOnScattererPlanet = false;
		public bool isCustomOceanEnabledOnScattererPlanet{get{return customOceanEnabledOnScattererPlanet;}}

        public const int startingScaledRenderQueue = 2900;

        public ScattererCelestialBodiesManager ()
        {
        }

        public void Init()
        {
            FindCelestialBodies ();
        }

		public void Update()
		{
			UpdateProlandManagers ();
		}
		
		void UpdateProlandManagers ()
		{
			pqsEnabledOnScattererPlanet = false;
			underwater = false;
			customOceanEnabledOnScattererPlanet = false;

            foreach (ScattererCelestialBody scattererCelestialBody in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
            {
                float minDistance;    //smallest distance to either the camera or ship
                if (scattererCelestialBody.isFound)
                {
                    scattererCelestialBody.currentDistanceFromCamera = Vector3.Distance (Scatterer.Instance.scaledSpaceCamera.transform.position, scattererCelestialBody.transform.position) * ScaledSpace.ScaleFactor;
                    minDistance = FlightGlobals.ActiveVessel ? Mathf.Min(scattererCelestialBody.currentDistanceFromCamera, Vector3.Distance (FlightGlobals.ActiveVessel.transform.position, ScaledSpace.ScaledToLocalSpace (scattererCelestialBody.transform.position))) : scattererCelestialBody.currentDistanceFromCamera;

                    if (scattererCelestialBody.active)
                    {
                        if (minDistance > scattererCelestialBody.unloadDistance)
                        {
                            UnloadEffectsForBody(scattererCelestialBody);
                            break;
                        }
                        else
                        {
                            UpdateBody (scattererCelestialBody, ref pqsEnabledOnScattererPlanet, ref underwater, ref customOceanEnabledOnScattererPlanet);
                        }
                    }
                    else
                    {    
                        if (minDistance < scattererCelestialBody.loadDistance && scattererCelestialBody.transform && scattererCelestialBody.celestialBody)
                        {
                            LoadEffectsForBody (scattererCelestialBody);
                            break;
                        }
                    }
                }
            }

            // Sort planets and cloud layers back to front to set the correct renderqueue for atmosphere and clouds
            // TODO: Do the same thing for planetary rings, needs splitting wring mesh in multiple parts to set behind/in-front
            List<ScattererCelestialBody> farthestToClosestActiveFound = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies
                .Where(b => b.isFound && b.active)
                .ToList();

            farthestToClosestActiveFound.Sort((a, b) => b.currentDistanceFromCamera.CompareTo(a.currentDistanceFromCamera));

            int currentRenderqueue = startingScaledRenderQueue - 1;

            foreach (ScattererCelestialBody scattererCelestialBody in farthestToClosestActiveFound)
            {
                scattererCelestialBody.prolandManager.skyNode.scaledScatteringMaterial.renderQueue = currentRenderqueue++;
                scattererCelestialBody.prolandManager.skyNode.skyMaterial.renderQueue = currentRenderqueue++;

                if (Scatterer.Instance.eveReflectionHandler.EVECloudLayers.TryGetValue(scattererCelestialBody.celestialBodyName, out var layers))
                {
                    var cameraAltitude = scattererCelestialBody.currentDistanceFromCamera - (float)scattererCelestialBody.celestialBody.Radius;

                    foreach (EVECloudLayer eveCloudLayer in layers)
                    {
                        if (eveCloudLayer.Clouds2dMaterial != null)
                        { 
                            eveCloudLayer.CurrentDistanceToCamera = Mathf.Abs(eveCloudLayer.Altitude - cameraAltitude);
                        }
                    }

                    layers.Sort((a, b) => b.CurrentDistanceToCamera.CompareTo(a.CurrentDistanceToCamera));

                    foreach (EVECloudLayer eveCloudLayer in layers)
                    {
                        if (eveCloudLayer.Clouds2dMaterial != null)
                        { 
                            eveCloudLayer.Clouds2dMaterial.renderQueue = currentRenderqueue++;
                        }
                    }
                }
            }
        }

        void FindCelestialBodies()
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

        void LoadEffectsForBody (ScattererCelestialBody scattererCelestialBody)
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
                    Scatterer.Instance.guiHandler.LoadPlanet(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.IndexOf (scattererCelestialBody));
                }
                else
                {
                    throw new Exception ("Planet already removed from planets list");
                }
                
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
                    Utils.LogDebug ("" + scattererCelestialBody.celestialBodyName + " removed from active planets.");
                }
                
                return;
            }
        }

        ScattererCelestialBody UpdateBody (ScattererCelestialBody scattererCelestialBody, ref bool inPqsEnabledOnScattererPlanet, ref bool inUnderwater, ref bool inCustomOceanEnabledOnScattererPlanet)
        {
            scattererCelestialBody.prolandManager.Update ();
            inPqsEnabledOnScattererPlanet = inPqsEnabledOnScattererPlanet || !scattererCelestialBody.prolandManager.skyNode.inScaledSpace;
            if (inPqsEnabledOnScattererPlanet && scattererCelestialBody.prolandManager.GetOceanNode()) {
                inCustomOceanEnabledOnScattererPlanet = true;
                inUnderwater = scattererCelestialBody.prolandManager.GetOceanNode ().isUnderwater;
            }
            return scattererCelestialBody;
        }
        
        void UnloadEffectsForBody(ScattererCelestialBody scattererCelestialBody)
        {
            scattererCelestialBody.prolandManager.OnDestroy ();
            UnityEngine.Object.Destroy (scattererCelestialBody.prolandManager);
            scattererCelestialBody.prolandManager = null;
            scattererCelestialBody.active = false;
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

