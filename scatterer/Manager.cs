using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSP.IO;

namespace scatterer {
	/*
	 * A manger to organise what order update functions are called
	 * Provides a location for common settings and allows the nodes to access each other
	 */
	public class Manager: MonoBehaviour {

		
		//parent core
//		Core Core.Instance;
		public bool hasOcean = false;
		
		public double m_radius = 600000.0f;
		int waitBeforeReloadCnt=0;
		
		
		OceanWhiteCaps m_oceanNode;
		public SkyNode m_skyNode;
		SunNode m_sunNode;
		
		public CelestialBody parentCelestialBody;
		public Transform ParentPlanetTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters;
		
		// Initialization
		public void Awake() {
			m_radius = parentCelestialBody.Radius;
			//			print (m_radius);
			
			m_sunNode = new SunNode();
			m_sunNode.Start();
			
			m_skyNode = new SkyNode();
			m_skyNode.setManager(this);
			m_skyNode.SetParentCelestialBody(parentCelestialBody);
			m_skyNode.setParentPlanetTransform(ParentPlanetTransform);
			//			print ("skynode parent CB and PP set");
			//m_skyNode.loadSettings ();
			m_skyNode.loadFromConfigNode(false);

			m_skyNode.Start();

			//m_skyNode.loadFromConfigNode(false);
			//m_skyNode.loadFromConfigNode ();
			//			print ("skynode started");
			
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode = new OceanWhiteCaps();
				m_oceanNode.setManager(this);
				m_oceanNode.setCore(Core.Instance);

				m_oceanNode.loadFromConfigNode(false);
				m_oceanNode.Start();

			}
		}
		
		
		public void Update()
		{

			m_sunNode.setDirectionToSun(getDirectionToSun());
			m_sunNode.UpdateNode();
			
			
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
			
			UnityEngine.Object.Destroy(m_sunNode);
			
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode.OnDestroy();
				UnityEngine.Object.Destroy(m_oceanNode);
			}
		}
		
		
		//this fixes the alt-enter bug the really stupid way but it's fast so it'll do for now
		public void reBuildOcean() {
			if (hasOcean && Core.Instance.useOceanShaders) {
				m_oceanNode.OnDestroy();
				UnityEngine.Object.Destroy(m_oceanNode);
				m_oceanNode = new OceanWhiteCaps();
				m_oceanNode.setManager(this);
				m_oceanNode.setCore(Core.Instance);
				m_oceanNode.loadFromConfigNode(false);
				m_oceanNode.Start();
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
		
		public Vector3 getDirectionToSun() {
			//			if (m_skyNode.debugSettings [0]) {
			return ((sunCelestialBody.GetTransform().position - parentCelestialBody.GetTransform().position));
			//			} else {
			//				return((ScaledSpace.LocalToScaledSpace(sunCelestialBody.GetTransform ().position)-ScaledSpace.LocalToScaledSpace(parentCelestialBody.GetTransform ().position)));
			//			
			//			}
			
		}

		public double GetRadius() {
			return m_radius;
		}
		
		public Vector3 GetSunNodeDirection() {
			return m_sunNode.GetDirection();
		}
		
		public Matrix4x4 GetSunWorldToLocalRotation() {
			
			return m_sunNode.GetWorldToLocalRotation();
		}
		
		public OceanWhiteCaps GetOceanNode() {
			return m_oceanNode;
		}
		
		public SkyNode GetSkyNode() {
			return m_skyNode;
		}
		
		public void SetSunNodeUniforms(Material mat) {
			m_sunNode.SetUniforms(mat);
		}

		
//		public Core GetCore() {
//			return (Core.Instance);
//		}
	}
}