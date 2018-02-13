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
		public bool usesCloudIntegration = true;
		
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

		public List<AtmoPlanetShineSource> planetshineSources;

		
		// Initialization
		public void Awake()
		{
			m_radius = parentCelestialBody.Radius;

			m_skyNode = (SkyNode) Core.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SkyNode));
			m_skyNode.setManager(this);
			m_skyNode.usesCloudIntegration = usesCloudIntegration;
			m_skyNode.SetParentCelestialBody(parentCelestialBody);
			m_skyNode.setParentPlanetTransform(ParentPlanetTransform);
			
			if (m_skyNode.loadFromConfigNode())
			{
				m_skyNode.Init();		
				
				if (hasOcean && Core.Instance.useOceanShaders)
				{
					m_oceanNode = (OceanWhiteCaps) Core.Instance.farCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
					m_oceanNode.setManager(this);
					
					m_oceanNode.loadFromConfigNode();
					m_oceanNode.Init();
					
				}
				
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
		
		public void OnDestroy()
		{
			m_skyNode.Cleanup();

			Component.Destroy(m_skyNode);

			UnityEngine.Object.Destroy(m_skyNode);
			
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode.Cleanup();
				UnityEngine.Object.Destroy(m_oceanNode);
			}
		}
		
		
		//this fixes the alt-enter bug the really stupid way but it's fast and simple so it'll do
		public void reBuildOcean()
		{
			if (hasOcean && Core.Instance.useOceanShaders)
			{
				m_oceanNode.Cleanup();
				Component.Destroy(m_oceanNode);
				UnityEngine.Object.Destroy(m_oceanNode);

				m_oceanNode = (OceanWhiteCaps) Core.Instance.farCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
				m_oceanNode.setManager(this);
				m_oceanNode.loadFromConfigNode();
				m_oceanNode.Init();

				if (Core.Instance.oceanRefraction && Core.Instance.bufferRenderingManager.refractionTexture.IsCreated())
				{
					Core.Instance.bufferRenderingManager.refractionTexture.Create();
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