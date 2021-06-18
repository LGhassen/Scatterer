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
	/*
	 * A manager to organise what order update functions are called
	 * Manages all effects taken from Proland (ie Precomputed Scattering and Ocean, maybe one day forests...)
	 * The class structures from original Proland are kept, with some added Utilities related to scatterer and KSP
	 * Provides a location for common settings and allows the nodes to access each other
	 */
	public class ProlandManager: MonoBehaviour
	{
		
		public bool hasOcean = false;
		public bool usesCloudIntegration = true;
		public bool flatScaledSpaceModel = false;
		
		public double m_radius = 600000.0f;
		
		OceanFFTgpu oceanNode;
		public SkyNode skyNode;

		public Color sunColor;
		public CelestialBody parentCelestialBody;
		public Transform parentScaledTransform;
		public Transform parentLocalTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters=new List<CelestialBody> {};
		public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};

		public Light mainSunLight;
		public ScattererCelestialBody scattererCelestialBody;

		public void Init(ScattererCelestialBody scattererBody)
		{
			scattererCelestialBody = scattererBody;
			parentCelestialBody = scattererBody.celestialBody;
			sunColor=scattererBody.sunColor;
			flatScaledSpaceModel = scattererBody.flatScaledSpaceModel;
			usesCloudIntegration = scattererBody.usesCloudIntegration;
			hasOcean = scattererBody.hasOcean;
			
			sunCelestialBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.mainSunCelestialBody);

			if (scattererBody.mainSunCelestialBody == "Sun")
				mainSunLight = Scatterer.Instance.sunLight;
			else
			{
				mainSunLight = Scatterer.Instance.lights.SingleOrDefault (_light => (_light != null) && (_light.gameObject !=null) && (_light.gameObject.name == scattererBody.mainSunCelestialBody));

				if (ReferenceEquals (mainSunLight, null))
				{
					Utils.LogError ("No light found for " + scattererBody.mainSunCelestialBody + " for body " + parentCelestialBody.name + ". Defaulting to main sunlight, godrays, lightrays and caustics may look wrong, check your Kopernicus configuration.");
					mainSunLight = Scatterer.Instance.sunLight;
				}
				else
				{
					if (Scatterer.Instance.mainSettings.terrainShadows)
						Scatterer.Instance.SetShadowsForLight(mainSunLight);
					else
						Scatterer.Instance.DisableCustomShadowResForLight(mainSunLight);
				}
					
			}

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
					parentScaledTransform = Utils.GetMainMenuObject(scattererBody.celestialBody).transform;
					parentLocalTransform  = Utils.GetMainMenuObject(scattererBody.celestialBody).transform;
			}
			else
			{
				parentScaledTransform = scattererBody.transform;
				parentLocalTransform  = scattererBody.celestialBody.transform;
			}

			FindEclipseCasters (scattererBody);
			FindPlanetShineSources (scattererBody);

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				GameObject _go = Utils.GetMainMenuObject(scattererBody.celestialBody);
				if (_go)
				{
					MeshRenderer _mr = _go.GetComponent<MeshRenderer> ();
					if (_mr)
					{
						var sctBodyTransform = ScaledSpace.Instance.transform.FindChild (parentCelestialBody.name);
						m_radius = (_go.transform.localScale.x / sctBodyTransform.localScale.x) * parentCelestialBody.Radius;
					}
				}
			}
			else
			{
				m_radius = parentCelestialBody.Radius;
			}

			InitSkyAndOceanNodes ();
		}

		void FindEclipseCasters (ScattererCelestialBody scattererBody)
		{
			if (Scatterer.Instance.mainSettings.useEclipses) {
				for (int k = 0; k < scattererBody.eclipseCasters.Count; k++) {
					var cc = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.eclipseCasters [k]);
					if (cc == null)
						Utils.LogDebug ("Eclipse caster " + scattererBody.eclipseCasters [k] + " not found for " + scattererBody.celestialBodyName);
					else {
						eclipseCasters.Add (cc);
						Utils.LogDebug ("Added eclipse caster " + scattererBody.eclipseCasters [k] + " for " + scattererBody.celestialBodyName);
					}
				}
			}
		}

		void FindPlanetShineSources (ScattererCelestialBody scattererBody)
		{
			if (Scatterer.Instance.mainSettings.usePlanetShine) {
				for (int k = 0; k < scattererBody.planetshineSources.Count; k++) {
					var cc = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.planetshineSources [k].bodyName);
					if (cc == null)
						Utils.LogDebug ("planetshine source " + scattererBody.planetshineSources [k].bodyName + " not found for " + scattererBody.celestialBodyName);
					else {
						AtmoPlanetShineSource src = scattererBody.planetshineSources [k];
						src.body = cc;
						scattererBody.planetshineSources [k].body = cc;
						planetshineSources.Add (src);
						Utils.LogDebug ("Added planetshine source" + scattererBody.planetshineSources [k].bodyName + " for " + scattererBody.celestialBodyName);
					}
				}
			}
		}

		void InitSkyAndOceanNodes ()
		{
			skyNode = (SkyNode)Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SkyNode));
			skyNode.prolandManager = this;
			skyNode.SetCelestialBodyName (parentCelestialBody.name);
			skyNode.SetParentScaledTransform (parentScaledTransform);
			skyNode.SetParentLocalTransform (parentLocalTransform);
			skyNode.usesCloudIntegration = usesCloudIntegration;
			if (skyNode.LoadFromConfigNode ())
			{
				skyNode.Init ();
				if (hasOcean && Scatterer.Instance.mainSettings.useOceanShaders && (HighLogic.LoadedScene != GameScenes.MAINMENU))
				{
					if (Scatterer.Instance.mainSettings.oceanFoam)
						oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
					else
						oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

					oceanNode.Init (this);
				}
			}
		}
		
		public void Update()
		{	
			skyNode.UpdateNode();
			
			if (!ReferenceEquals(oceanNode,null))
			{
				oceanNode.UpdateNode();
			}
		}
		
		public void OnDestroy()
		{
			if (!ReferenceEquals(skyNode,null))
			{
				skyNode.Cleanup();
				Component.DestroyImmediate(skyNode);
			}
			
			if (!ReferenceEquals(oceanNode,null)) {
				oceanNode.Cleanup();
				Component.DestroyImmediate(oceanNode);
			}
		}

		//TODO: change this so that it takes the new configNode and that's all? May not be possible depending on if it needs to recreate lightraysRenderer and stuff
		//Therefor add an option to init from configNode? yep
		public void reBuildOcean()
		{
			if (!ReferenceEquals(oceanNode,null))
			{
				oceanNode.Cleanup();
				Component.Destroy(oceanNode);
				UnityEngine.Object.Destroy(oceanNode);

				if (Scatterer.Instance.mainSettings.oceanFoam)
					oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
				else
					oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

				oceanNode.Init(this);

				Utils.LogDebug("Rebuilt Ocean");
			}
		}

		public Vector3 getDirectionToSun()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				return (Scatterer.Instance.mainMenuLight.gameObject.transform.forward*(-1));
			}
			else
				return (sunCelestialBody.GetTransform().position - parentCelestialBody.GetTransform().position).normalized;			
		}

		public Vector3 getDirectionToCelestialBody(CelestialBody target)
		{
			return (target.GetTransform().position - parentCelestialBody.GetTransform().position);			
		}

		public double GetRadius() {
			return m_radius;
		}

		public OceanFFTgpu GetOceanNode() {
			return oceanNode;
		}
		
		public SkyNode GetSkyNode() {
			return skyNode;
		}
	}
}