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
	public class EVEReflectionHandler
	{
		//map EVE 2d cloud materials to planet names
		public Dictionary<String, List<Material> > EVEClouds = new Dictionary<String, List<Material> >();

		//map EVE CloudObjects to planet names
		//as far as I understand CloudObjects in EVE contain the 2d clouds and the volumetrics for a given
		//layer on a given planet, however due to the way they are handled in EVE they don't directly reference
		//their parent planet and the volumetrics are only created when the PQS is active
		//I map them here to facilitate accessing the volumetrics later
		public Dictionary<String, List<object>> EVECloudObjects = new Dictionary<String, List<object>>();
		public object EVEinstance;
				
		List<Projector> EVEprojector=new List<Projector> {}; int projectorCount=0;

		public EVEReflectionHandler ()
		{
		}

		public void Start()
		{
			MapEVEClouds ();
			mapEVEshadowProjectors ();
		}

		public void MapEVEClouds()
		{
			Utils.LogDebug ("mapping EVE clouds");
			EVEClouds.Clear();
			EVECloudObjects.Clear ();
			
			//find EVE base type
			Type EVEType = ReflectionUtils.getType("Atmosphere.CloudsManager"); 
			
			if (EVEType == null)
			{
				Utils.LogDebug("Eve assembly type not found");
				return;
			}
			else
			{
				Utils.LogDebug("Eve assembly type found");
			}
			
			Utils.LogDebug("Eve assembly version: " + EVEType.Assembly.GetName().ToString());
			
			const BindingFlags flags =  BindingFlags.FlattenHierarchy |  BindingFlags.NonPublic | BindingFlags.Public | 
				BindingFlags.Instance | BindingFlags.Static;
			
			try
			{
				//				EVEinstance = EVEType.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
				EVEinstance = EVEType.GetField("instance", flags).GetValue(null) ;
			}
			catch (Exception)
			{
				Utils.LogDebug("No EVE Instance found");
				return;
			}
			if (EVEinstance == null)
			{
				Utils.LogError("Failed grabbing EVE Instance");
				return;
			}
			else
			{
				Utils.LogInfo("Successfully grabbed EVE Instance");
			}
			
			IList objectList = EVEType.GetField ("ObjectList", flags).GetValue (EVEinstance) as IList;
			
			foreach (object _obj in objectList)
			{
				String body = _obj.GetType().GetField("body", flags).GetValue(_obj) as String;
				
				if (EVECloudObjects.ContainsKey(body))
				{
					EVECloudObjects[body].Add(_obj);
				}
				else
				{
					List<object> objectsList = new List<object>();
					objectsList.Add(_obj);
					EVECloudObjects.Add(body,objectsList);
				}
				
				object cloud2dObj;
				if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				{
					object cloudsPQS = _obj.GetType().GetField("cloudsPQS", flags).GetValue(_obj) as object;
					
					if (cloudsPQS==null)
					{
						Utils.LogDebug("cloudsPQS not found for layer on planet :"+body);
						continue;
					}
					cloud2dObj = cloudsPQS.GetType().GetField("mainMenuLayer", flags).GetValue(cloudsPQS) as object;
				}
				else
				{
					cloud2dObj = _obj.GetType().GetField("layer2D", flags).GetValue(_obj) as object;
				}
				
				if (cloud2dObj==null)
				{
					Utils.LogDebug("layer2d not found for layer on planet :"+body);
					continue;
				}
				
				GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
				if (cloudmesh==null)
				{
					Utils.LogDebug("cloudmesh null");
					return;
				}
				
				if (EVEClouds.ContainsKey(body))
				{
					EVEClouds[body].Add(cloudmesh.GetComponent < MeshRenderer > ().material);
				}
				else
				{
					List<Material> cloudsList = new List<Material>();
					cloudsList.Add(cloudmesh.GetComponent < MeshRenderer > ().material);
					EVEClouds.Add(body,cloudsList);
				}
				Utils.LogDebug("Detected EVE 2d cloud layer for planet: "+body);
			}
		}

		void mapEVEshadowProjectors()
		{
			if (EVEprojector == null)
				return;
			EVEprojector.Clear ();
			//Material atmosphereMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/AtmosphericLocalScatter")]);
			Projector[] list = (Projector[]) Projector.FindObjectsOfType(typeof(Projector));
			if (list == null)
				return;
			for(int i=0;i<list.Length;i++)
			{
				if (list[i].material != null && list[i].material.name != null && list[i].material.name == "EVE/CloudShadow")
				{
					EVEprojector.Add(list[i]);
					//list[i].material = atmosphereMaterial;
				}
			}
			projectorCount = EVEprojector.Count;
		}
		
		void disableEVEshadowProjectors()
		{
			try
			{
				for (int i=0; i<projectorCount; i++) {
					EVEprojector [i].enabled = false;
				}
			}
			catch (Exception)
			{
				Utils.LogDebug ("Null EVE shadow projectors, remapping...");
				mapEVEshadowProjectors ();
			}
		}
		
		void enableEVEshadowProjectors()
		{
			for(int i=0;i<projectorCount;i++)
			{
				EVEprojector[i].enabled=true;
			}
		}
	}
}

