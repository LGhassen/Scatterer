﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using KSP.IO;

namespace scatterer
{
	/*
	 * A manger to organise what order update functions are called, the running of tasks and the drawing of the terrain.
	 * Provides a location for common settings and allows the nodes to access each other.
	 * Also sets uniforms that are considered global.
	 * Must have a scheduler script attached to the same gameobject
	 * 
	 */
	public class Manager : MonoBehaviour 
	{


		//update counter and manager state, used to know if the manager is working
		int updateCnt=0;
		string managerState="not initialized";

		//parent core
		Core m_core;


		int[] cam=new int[7];
		
		[SerializeField]
		float m_radius= 600000.0f;
		

		//OceanNode m_oceanNode;
		public SkyNode m_skyNode;
		SunNode m_sunNode;
				
		CelestialBody parentCelestialBody;
		CelestialBody sunCelestialBody;
										
		// Initialization
		public void Awake() 
		{
			managerState = "waking up";

			m_sunNode = new SunNode();
			m_sunNode.Start ();
			m_skyNode = new SkyNode();
			m_skyNode.setManager (this);
			m_skyNode.SetParentCelestialBody (parentCelestialBody);
			m_skyNode.loadSettings ();
			m_skyNode.Start ();

			for (int i=0;i<7;i++)
			{
				cam[i]=1;
			}

			m_radius = (float)parentCelestialBody.Radius;

			managerState = "awake";
		}

		
		public void Update () 
		{
			managerState = "updating";

			//Update the sky and sun
			m_sunNode.setDirectionToSun (getDirectionToSun ());
			m_sunNode.UpdateNode();
			m_radius = (float)parentCelestialBody.Radius;

			m_skyNode.UpdateNode();

			updateCnt++;
			managerState = "update done "+updateCnt.ToString();
			//print (managerState);
		}
		

		public void setParentCelestialBody (CelestialBody parent)
		{
			parentCelestialBody = parent;
		}
		
		public void setSunCelestialBody (CelestialBody sun)
		{
			sunCelestialBody = sun;
		}
		
		public Vector3 getDirectionToSun()
		{
			return((sunCelestialBody.GetTransform ().position-parentCelestialBody.GetTransform ().position));
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
		
		//		public OceanNode GetOceanNode() {
		//			return m_oceanNode;
		//		}
		
		public void SetSunNodeUniforms(Material mat){
			m_sunNode.SetUniforms (mat);
		}
		
		public void SetCore(Core core){
			m_core=core;
		}		
	}
}

