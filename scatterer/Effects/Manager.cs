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
		public bool flatScaledSpaceModel = false;
		
		public double m_radius = 600000.0f;
		
		OceanWhiteCaps m_oceanNode;
		public SkyNode m_skyNode;

		
		public CelestialBody parentCelestialBody;
		public Transform parentScaledTransform;
		public Transform parentLocalTransform;
		
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters;

		public List<AtmoPlanetShineSource> planetshineSources;

		public void Awake()
		{
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				GameObject _go = Core.GetMainMenuObject(parentCelestialBody.name);
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

			m_skyNode = (SkyNode) Core.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SkyNode));
			m_skyNode.setManager(this);
			m_skyNode.setCelestialBodyName (parentCelestialBody.name);
			m_skyNode.setParentScaledTransform (parentScaledTransform);
			m_skyNode.setParentLocalTransform (parentLocalTransform);

			m_skyNode.usesCloudIntegration = usesCloudIntegration;
			
			if (m_skyNode.loadFromConfigNode())
			{
				m_skyNode.Init();		
				
				if (hasOcean && Core.Instance.useOceanShaders && (HighLogic.LoadedScene !=GameScenes.MAINMENU))
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
			
			if (!ReferenceEquals(m_oceanNode,null))
			{
				m_oceanNode.UpdateNode();
			}
		}
		
		public void OnDestroy()
		{
			m_skyNode.Cleanup();

			Component.Destroy(m_skyNode);

			UnityEngine.Object.Destroy(m_skyNode);
			
			if (!ReferenceEquals(m_oceanNode,null)) {
				m_oceanNode.Cleanup();
				UnityEngine.Object.Destroy(m_oceanNode);
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
		
		public void setParentScaledTransform(Transform parentTransform) {
			parentScaledTransform = parentTransform;
		}

		public void setParentLocalTransform(Transform parentTransform) {
			parentLocalTransform = parentTransform;
		}
		
		public void setSunCelestialBody(CelestialBody sun) {
			sunCelestialBody = sun;
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