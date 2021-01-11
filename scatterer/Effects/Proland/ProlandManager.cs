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
		
		OceanFFTgpu m_oceanNode;
		public SkyNode m_skyNode;

		public Color sunColor;
		public CelestialBody parentCelestialBody;
		public Transform parentScaledTransform;
		public Transform parentLocalTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters=new List<CelestialBody> {};
		public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
		public Light mainSunLight;
		

		public void Init(ScattererCelestialBody scattererBody)
		{
			parentCelestialBody = scattererBody.celestialBody;
			sunColor=scattererBody.sunColor;
			flatScaledSpaceModel = scattererBody.flatScaledSpaceModel;
			usesCloudIntegration = scattererBody.usesCloudIntegration;
			hasOcean = scattererBody.hasOcean;
			
			sunCelestialBody = Scatterer.Instance.scattererCelestialBodiesManager.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.mainSunCelestialBody);

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
					var cc = Scatterer.Instance.scattererCelestialBodiesManager.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.eclipseCasters [k]);
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
					var cc = Scatterer.Instance.scattererCelestialBodiesManager.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.planetshineSources [k].bodyName);
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
			m_skyNode = (SkyNode)Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SkyNode));
			m_skyNode.m_manager = this;
			m_skyNode.SetCelestialBodyName (parentCelestialBody.name);
			m_skyNode.SetParentScaledTransform (parentScaledTransform);
			m_skyNode.SetParentLocalTransform (parentLocalTransform);
			m_skyNode.usesCloudIntegration = usesCloudIntegration;
			if (m_skyNode.LoadFromConfigNode ())
			{
				m_skyNode.Init ();
				if (hasOcean && Scatterer.Instance.mainSettings.useOceanShaders && (HighLogic.LoadedScene != GameScenes.MAINMENU))
				{
					if (Scatterer.Instance.mainSettings.oceanFoam)
						m_oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
					else
						m_oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

					m_oceanNode.Init (this);
				}
			}
		}
		
		public void Update()
		{	
			m_skyNode.UpdateNode();
			
			if (!ReferenceEquals(m_oceanNode,null))
			{
				m_oceanNode.UpdateNode();
			}
		}
		
		public void OnDestroy()
		{
			if (!ReferenceEquals(m_skyNode,null))
			{
				m_skyNode.Cleanup();
				Component.Destroy(m_skyNode);
			}
			
			if (!ReferenceEquals(m_oceanNode,null)) {
				m_oceanNode.Cleanup();
				Component.Destroy(m_oceanNode);
			}
		}
		
		
		//this fixes the alt-enter bug the really stupid way but it's fast and simple so it'll do
		//Do we still need this to fix alt-enter or any rendertexture lost?
		public void reBuildOcean()
		{
			if (!ReferenceEquals(m_oceanNode,null))
			{
				m_oceanNode.Cleanup();
				Component.Destroy(m_oceanNode);
				UnityEngine.Object.Destroy(m_oceanNode);

				if (Scatterer.Instance.mainSettings.oceanFoam)
					m_oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
				else
					m_oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

				m_oceanNode.Init(this);

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
			return m_oceanNode;
		}
		
		public SkyNode GetSkyNode() {
			return m_skyNode;
		}
	}
}