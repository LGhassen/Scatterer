using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSP.IO;

namespace scatterer
{
	/*
	 * A manger to organise what order update functions are called
	 * Provides a location for common settings and allows the nodes to access each other
	 * 
	 */
	public class Manager : MonoBehaviour 
	{
		
		
		//update counter and manager state, used to know if the manager is working
		int updateCnt=0;
		string managerState="not initialized";
		
		//parent core
		Core m_core;
		
		[SerializeField]
		float m_radius= 600000.0f;
		
		
		OceanWhiteCaps m_oceanNode;
		public SkyNode m_skyNode;
		SunNode m_sunNode;

		public CelestialBody parentCelestialBody;
		public Transform ParentPlanetTransform;

		CelestialBody sunCelestialBody;
		
		// Initialization
		public void Awake() 
		{
			managerState = "waking up";
			m_radius = (float)parentCelestialBody.Radius;
//			print (m_radius);
			
			m_sunNode = new SunNode();
			m_sunNode.Start ();

			m_skyNode = new SkyNode();
			m_skyNode.setManager (this);
			m_skyNode.SetParentCelestialBody (parentCelestialBody);
			m_skyNode.setParentPlanetTransform (ParentPlanetTransform);
//			print ("skynode parent CB and PP set");
			//m_skyNode.loadSettings ();
			m_skyNode.Start ();
			m_skyNode.loadFromConfigNode ();
			//m_skyNode.loadFromConfigNode ();
//			print ("skynode started");


			m_oceanNode = new OceanWhiteCaps();
			m_oceanNode.setManager (this);
			m_oceanNode.setCore (m_core);

			m_oceanNode.Start ();

			managerState = "awake";
		}
		
		
		public void Update () 
		{
			managerState = "updating";
			m_radius = (float)parentCelestialBody.Radius;
			
			//Update the sky and sun


			m_sunNode.setDirectionToSun (getDirectionToSun ());
			m_sunNode.UpdateNode();

			
			m_skyNode.UpdateNode();
			m_oceanNode.UpdateNode ();


			
			updateCnt++;
			managerState = "update done "+updateCnt.ToString();
			//print (managerState);
		}

		public void OnDestroy()
		{
			m_skyNode.OnDestroy ();
			Destroy (m_skyNode);

			Destroy (m_sunNode);

			m_oceanNode.OnDestroy ();
			Destroy (m_oceanNode);
		}
		
		
		public void setParentCelestialBody (CelestialBody parent)
		{
			parentCelestialBody = parent;
		}

		public void setParentPlanetTransform (Transform parentTransform)
		{
			ParentPlanetTransform = parentTransform;
		}
		
		public void setSunCelestialBody (CelestialBody sun)
		{
			sunCelestialBody = sun;
		}
		
		public Vector3 getDirectionToSun()
		{
//			if (m_skyNode.debugSettings [0]) {
				return((sunCelestialBody.GetTransform ().position - parentCelestialBody.GetTransform ().position));
//			} else {
//				return((ScaledSpace.LocalToScaledSpace(sunCelestialBody.GetTransform ().position)-ScaledSpace.LocalToScaledSpace(parentCelestialBody.GetTransform ().position)));
//			
//			}

		}
		
		
		public string getManagerState() {
			return managerState;
		}
		
		
		public float GetRadius() {
			return m_radius;
		}	
		
		public Vector3 GetSunNodeDirection() {
			return m_sunNode.GetDirection();
		}
		
		public Matrix4x4 GetSunWorldToLocalRotation(){
			
			return m_sunNode.GetWorldToLocalRotation();
		}
		
		public OceanNode GetOceanNode() {
			return m_oceanNode;
		}

		public SkyNode GetSkyNode() {
			return m_skyNode;
		}
		
		public void SetSunNodeUniforms(Material mat){
			m_sunNode.SetUniforms (mat);
		}
		
		public void SetCore(Core core){
			m_core=core;
		}

		public Core GetCore(){
			return (m_core);
		}	
	}
}
