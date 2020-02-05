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
	 * A manger to organise what order update functions are called
	 * Provides a location for common settings and allows the nodes to access each other
	 */
	public class Manager: MonoBehaviour
	{
		
		public bool hasOcean = false;
		public bool usesCloudIntegration = true;
		public bool flatScaledSpaceModel = false;
		
		public double m_radius = 600000.0f;
		
		OceanWhiteCaps m_oceanNode;
		public SkyNode m_skyNode;

		public Vector3 sunColor;
		public CelestialBody parentCelestialBody;
		public Transform parentScaledTransform;
		public Transform parentLocalTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters=new List<CelestialBody> {};
		public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
		

		public void Init(ScattererCelestialBody scattererBody)
		{
			parentCelestialBody = scattererBody.celestialBody;
			sunColor=scattererBody.sunColor;
			flatScaledSpaceModel = scattererBody.flatScaledSpaceModel;
			usesCloudIntegration = scattererBody.usesCloudIntegration;
			hasOcean = scattererBody.hasOcean;
			
			sunCelestialBody = Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.mainSunCelestialBody);
			
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				parentScaledTransform = Utils.GetMainMenuObject(scattererBody.celestialBodyName).transform;
				parentLocalTransform  = Utils.GetMainMenuObject(scattererBody.celestialBodyName).transform;
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
				GameObject _go = Utils.GetMainMenuObject(parentCelestialBody.name);
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
			if (Core.Instance.mainSettings.useEclipses) {
				for (int k = 0; k < scattererBody.eclipseCasters.Count; k++) {
					var cc = Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.eclipseCasters [k]);
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
			if (Core.Instance.mainSettings.usePlanetShine) {
				for (int k = 0; k < scattererBody.planetshineSources.Count; k++) {
					var cc = Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.planetshineSources [k].bodyName);
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
			m_skyNode = (SkyNode)Core.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SkyNode));
			m_skyNode.setManager (this);
			m_skyNode.setCelestialBodyName (parentCelestialBody.name);
			m_skyNode.setParentScaledTransform (parentScaledTransform);
			m_skyNode.setParentLocalTransform (parentLocalTransform);
			m_skyNode.usesCloudIntegration = usesCloudIntegration;
			if (m_skyNode.loadFromConfigNode ()) {
				m_skyNode.Init ();
				if (hasOcean && Core.Instance.mainSettings.useOceanShaders && (HighLogic.LoadedScene != GameScenes.MAINMENU)) {
					m_oceanNode = (OceanWhiteCaps)Core.Instance.farCamera.gameObject.AddComponent (typeof(OceanWhiteCaps));
					m_oceanNode.setManager (this);
					m_oceanNode.loadFromConfigNode ();
					m_oceanNode.Init ();
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
		public void reBuildOcean()
		{
			if (!ReferenceEquals(m_oceanNode,null))
			{
				m_oceanNode.Cleanup();
				Component.Destroy(m_oceanNode);
				UnityEngine.Object.Destroy(m_oceanNode);

				m_oceanNode = (OceanWhiteCaps) Core.Instance.farCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
				m_oceanNode.setManager(this);
				m_oceanNode.loadFromConfigNode();
				m_oceanNode.Init();

				if (Core.Instance.mainSettings.oceanRefraction && Core.Instance.bufferRenderingManager.refractionTexture.IsCreated())
				{
					Core.Instance.bufferRenderingManager.refractionTexture.Create();
				}

				Utils.LogDebug("Rebuilt Ocean");
			}
			
		}

		public Vector3 getDirectionToSun()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				return (Core.Instance.mainMenuLight.transform.forward*(-1));
			}
			else
				return (sunCelestialBody.GetTransform().position - parentCelestialBody.GetTransform().position);			
		}

		public Vector3 getDirectionToCelestialBody(CelestialBody target)
		{
			return (target.GetTransform().position - parentCelestialBody.GetTransform().position);			
		}

		public double GetRadius() {
			return m_radius;
		}

		public OceanWhiteCaps GetOceanNode() {
			return m_oceanNode;
		}
		
		public SkyNode GetSkyNode() {
			return m_skyNode;
		}
	}
}