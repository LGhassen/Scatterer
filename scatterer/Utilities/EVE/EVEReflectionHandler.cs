using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Scatterer
{
	public struct EVECloudLayer
	{
		public object CloudObject;
		public Material Clouds2dMaterial;
		public MeshRenderer Clouds2dMeshRenderer;
		public Material CloudShadowMaterial;
		public Material ParticleVolumetricsMaterial;
	}

	public class EVEReflectionHandler
	{
		public Dictionary<String, List<EVECloudLayer>> EVECloudLayers = new Dictionary<String, List<EVECloudLayer>>();

		public object EVEInstance;
		private EventVoid onCloudsApplyEvent;

		private const BindingFlags flags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

		public EVEReflectionHandler ()
		{
		}

		public void Start()
		{
			MapEVEClouds ();
		}

		public void MapEVEClouds()
        {
            Utils.LogDebug("Mapping EVE clouds");

            CleanUp();
            EVECloudLayers.Clear();

            Type EVEType = null;

            if (!GetEVETypeAndInstance(ref EVEType, ref EVEInstance))
                return;

			GetOnApplyEvent(ref EVEType);

			IList objectList = EVEType.GetField("ObjectList", flags).GetValue(EVEInstance) as IList;

            foreach (object cloudObject in objectList)
            {
                String body = cloudObject.GetType().GetField("body", flags).GetValue(cloudObject) as String;

				EVECloudLayer cloudLayer = new EVECloudLayer();
				cloudLayer.CloudObject = cloudObject;

				Map2DLayer(ref cloudLayer, cloudObject, body);
				MapParticleVolumetrics(ref cloudLayer, cloudObject, body);

				if (EVECloudLayers.ContainsKey(body))
                {
                    EVECloudLayers[body].Add(cloudLayer);
                }
                else
                {
                    List<EVECloudLayer> cloudsList = new List<EVECloudLayer>() { cloudLayer };
                    EVECloudLayers.Add(body, cloudsList);
                }
			}
        }

		private void Map2DLayer(ref EVECloudLayer cloudLayer, object cloudObject, string body)
        {
			object cloud2dObj;
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
			{
				object cloudsPQS = cloudObject.GetType().GetField("cloudsPQS", flags).GetValue(cloudObject) as object;

				if (cloudsPQS == null)
				{
					Utils.LogDebug("cloudsPQS not found for layer on planet :" + body);
					return;
				}
				cloud2dObj = cloudsPQS.GetType().GetField("mainMenuLayer", flags).GetValue(cloudsPQS) as object;
			}
			else
			{
				cloud2dObj = cloudObject.GetType().GetField("layer2D", flags).GetValue(cloudObject) as object;
			}

			if (cloud2dObj != null)
			{
				GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
				if (cloudmesh == null)
				{
					Utils.LogDebug("cloudmesh null");
					return;
				}

				Material shadowMaterial = null;

				object screenSpaceShadow = null;

				try
				{
					screenSpaceShadow = cloud2dObj.GetType().GetField("screenSpaceShadow", flags).GetValue(cloud2dObj) as object;
				}
				catch (Exception) { }

				if (screenSpaceShadow != null)
				{
					shadowMaterial = screenSpaceShadow.GetType().GetField("material", flags).GetValue(screenSpaceShadow) as Material;
				}
				else
				{
					Projector shadowProjector = cloud2dObj.GetType().GetField("ShadowProjector", flags).GetValue(cloud2dObj) as Projector;

					if (shadowProjector != null && shadowProjector.material != null)
					{
						shadowMaterial = shadowProjector.material;
					}
				}

				cloudLayer.Clouds2dMeshRenderer = cloudmesh.GetComponent<MeshRenderer>();
				cloudLayer.Clouds2dMaterial = cloudLayer.Clouds2dMeshRenderer.material;
				cloudLayer.CloudShadowMaterial = shadowMaterial;

				cloudLayer.Clouds2dMaterial.renderQueue = 2999; //fix for EVE cloud renderqueue, TODO: check if outdated and remove it

				Utils.LogDebug("Detected EVE 2d cloud layer for planet: " + body);
			}
		}

		private void MapParticleVolumetrics(ref EVECloudLayer cloudLayer, object cloudObject, string body)
		{
			try
			{
				object cloudsPQS = cloudObject.GetType().GetField("cloudsPQS", flags)?.GetValue(cloudObject) as object;
				object layerVolume = cloudsPQS?.GetType().GetField("layerVolume", flags)?.GetValue(cloudsPQS) as object;
				if (layerVolume == null)
				{
					Utils.LogDebug("No particle volumetric cloud for layer on planet: " + body);
					return;
				}

				Material ParticleMaterial = layerVolume.GetType().GetField("ParticleMaterial", flags)?.GetValue(layerVolume) as Material;

				if (ParticleMaterial == null)
				{
					Utils.LogDebug("Particle volumetric cloud has no material on planet: " + body);
					return;
				}

				cloudLayer.ParticleVolumetricsMaterial = ParticleMaterial;
				Utils.LogDebug("Particle volumetric cloud mapped for layer on planet: " + body);
			}
			catch (Exception stupid)
			{
				Utils.LogDebug("Particle volumetric clouds error on planet: " + body + stupid.ToString());
			}
		}

		public void invokeClouds2dReassign(string celestialBodyName)
		{	
			foreach (var cloudLayer in EVECloudLayers[celestialBodyName])
			{
				if (cloudLayer.CloudObject != null)
				{ 
					object cloud2dObj = cloudLayer.CloudObject.GetType ().GetField ("layer2D", flags).GetValue (cloudLayer.CloudObject) as object;
					if (cloud2dObj == null) {
						Utils.LogDebug (" layer2d not found for layer on planet: " + celestialBodyName);
						continue;
					}
				
					bool cloud2dScaled = (bool)cloud2dObj.GetType ().GetField ("isScaled", flags).GetValue (cloud2dObj);
				
					MethodInfo scaledGetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetGetMethod ();
					MethodInfo scaledSetter = cloud2dObj.GetType ().GetProperty ("Scaled").GetSetMethod ();
				
					//if in scaled mode, switch it to local then back to scaled, to set all the properties
					if (cloud2dScaled)
						scaledSetter.Invoke (cloud2dObj, new object[] { !cloud2dScaled });
				
					scaledSetter.Invoke (cloud2dObj, new object[] { cloud2dScaled });

					//set the radius for use in the scatterer shader to have smooth scattering
					float radius = (float) cloud2dObj.GetType ().GetField ("radius", flags).GetValue (cloud2dObj);
					GameObject cloudmesh = cloud2dObj.GetType().GetField("CloudMesh", flags).GetValue(cloud2dObj) as GameObject;
					cloudmesh.GetComponent < MeshRenderer > ().material.SetFloat("_Radius",radius);
				}
			}
		}

		private bool GetEVETypeAndInstance(ref Type type, ref object instance)
		{
			type = ReflectionUtils.getType("Atmosphere.CloudsManager");

			if (type == null)
			{
				Utils.LogDebug("Eve assembly type not found");
				return false;
			}

			Utils.LogDebug("Eve assembly version: " + type.Assembly.GetName().ToString());

			try
			{
				instance = type.GetField("instance", flags).GetValue(null);
			}
			catch (Exception)
			{
				Utils.LogDebug("No EVE Instance found");
				return false;
			}
			if (instance == null)
			{
				Utils.LogError("Failed grabbing EVE Instance");
				return false;
			}

			Utils.LogInfo("Successfully grabbed EVE Instance");
			return true;
		}

		private void GetOnApplyEvent(ref Type EVEType)
		{
			try
			{
				onCloudsApplyEvent = EVEType.GetField("onApply", flags).GetValue(EVEInstance) as EventVoid;

				if (onCloudsApplyEvent != null)
				{
					onCloudsApplyEvent.Add(Scatterer.Instance.TriggerOnCloudReapplied);
				}
			}
			catch (Exception)
			{
				Utils.LogDebug("No EVE onCloudsApplyEvent found");
			}
		}

		public void OnCloudsReapplied()
		{
			MapEVEClouds();

			foreach (ScattererCelestialBody _cel in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies)
			{
				if (_cel.active)
				{
					_cel.prolandManager.skyNode.InitEVEClouds();
				}
			}

			Scatterer.Instance.OnRenderTexturesLost(); // to recreate any ocean stuck with old cloud shadows
		}

		public void CleanUp()
        {
			if (onCloudsApplyEvent != null)
				onCloudsApplyEvent.Remove(Scatterer.Instance.TriggerOnCloudReapplied);
		}
	}
}

