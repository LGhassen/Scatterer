using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSP.IO;

namespace scatterer
{
	/*
	 * A manger to organise what order update functions are called
	 * Provides a location for common settings and allows the nodes to access each other
	 */
	public class Manager: MonoBehaviour
	{
		
		public bool hasOcean = false;
		
		public double m_radius = 600000.0f;
		int waitBeforeReloadCnt=0;
		
		
		OceanWhiteCaps m_oceanNode;
		public SkyNode m_skyNode;

		
		public CelestialBody parentCelestialBody;
		public Transform ParentPlanetTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters;

//		public List<CelestialBody> additionalSuns;
//		public List<CelestialBody> planetShineLightSources;

		public List<atmoPlanetShineSource> planetshineSources;

		
		// Initialization
		public void Awake() {
			m_radius = parentCelestialBody.Radius;
			//			print (m_radius);
			
			m_skyNode = new SkyNode();
			m_skyNode.setManager(this);
			m_skyNode.SetParentCelestialBody(parentCelestialBody);
			m_skyNode.setParentPlanetTransform(ParentPlanetTransform);

			m_skyNode.loadFromConfigNode();


			m_skyNode.Start();		
			
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode = new OceanWhiteCaps();
				m_oceanNode.setManager(this);

				m_oceanNode.loadFromConfigNode();
				m_oceanNode.Start();

			}
		}
		
		
		public void Update()
		{	
			
			m_skyNode.UpdateNode();
			
			if (hasOcean && Core.Instance.useOceanShaders)
			{
				m_oceanNode.UpdateNode();

				if (!m_oceanNode.rendertexturesCreated)
				{
					waitBeforeReloadCnt++;
					if (waitBeforeReloadCnt >= 2)
					{
						reBuildOcean ();
						waitBeforeReloadCnt = 0;
					}
				}
			}
		}
		
		public void OnDestroy() {
			m_skyNode.OnDestroy();
			UnityEngine.Object.Destroy(m_skyNode);
			
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode.OnDestroy();
				UnityEngine.Object.Destroy(m_oceanNode);
			}
		}
		
		
		//this fixes the alt-enter bug the really stupid way but it's fast and simple so it'll do
		public void reBuildOcean(){
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode.OnDestroy();
				UnityEngine.Object.Destroy(m_oceanNode);
				m_oceanNode = new OceanWhiteCaps();
				m_oceanNode.setManager(this);
				m_oceanNode.loadFromConfigNode();
				m_oceanNode.Start();

				if (Core.Instance.oceanRefraction)
				{
					Core.Instance.refractionCam.waterMeshRenderers=m_oceanNode.waterMeshRenderers;
					Core.Instance.refractionCam.numGrids = m_oceanNode.numGrids;
				}

				Debug.Log("[Scatterer] Rebuilt Ocean");
			}
			
		}
		
		
		public void setParentCelestialBody(CelestialBody parent) {
			parentCelestialBody = parent;
		}
		
		public void setParentPlanetTransform(Transform parentTransform) {
			ParentPlanetTransform = parentTransform;
		}
		
		public void setSunCelestialBody(CelestialBody sun) {
			sunCelestialBody = sun;
		}
		
		public Vector3 getDirectionToSun()
		{
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