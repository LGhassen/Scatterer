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

namespace Scatterer
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
		public bool cloudIntegrationUsesScattererSunColors = false;
		public bool flatScaledSpaceModel = false;
		
		public double m_radius = 600000.0;
		public float mainMenuScaleFactor = 1f;
		
		OceanFFTgpu oceanNode;
		public SkyNode skyNode;

		public Color sunColor;
		public CelestialBody parentCelestialBody;
		public Transform parentScaledTransform;
		public Transform parentLocalTransform;

		public bool sunsUseIntensityCurves;
		public CelestialBody sunCelestialBody;
		public List<CelestialBody> eclipseCasters=new List<CelestialBody> {};
		public List<AtmoPlanetShineSource> planetshineSources=new List<AtmoPlanetShineSource> {};
		public List<SecondarySun> secondarySuns=new List<SecondarySun> {};

		public Light mainSunLight, mainScaledSunLight;
		public ScattererCelestialBody scattererCelestialBody;

		public Matrix4x4 planetShineSourcesMatrix=Matrix4x4.zero;
		public Matrix4x4 planetShineRGBMatrix=Matrix4x4.zero;					//Contains the colors set in the scatterer config
		public Matrix4x4 planetShineOriginalRGBMatrix=Matrix4x4.zero;			//Contains the original colors of the directional lights, if available

		public void Init(ScattererCelestialBody scattererBody)
		{
			scattererCelestialBody = scattererBody;
			parentCelestialBody = scattererBody.celestialBody;
			sunColor=scattererBody.sunColor;
			flatScaledSpaceModel = scattererBody.flatScaledSpaceModel;
			usesCloudIntegration = scattererBody.usesCloudIntegration;
			cloudIntegrationUsesScattererSunColors = scattererBody.cloudIntegrationUsesScattererSunColors;
			hasOcean = scattererBody.hasOcean;
			sunsUseIntensityCurves = scattererBody.sunsUseIntensityCurves;
			
			sunCelestialBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.mainSunCelestialBody);

			mainSunLight = findLight (scattererBody.mainSunCelestialBody);
			mainScaledSunLight = findScaledLight (scattererBody.mainSunCelestialBody);

			if (ReferenceEquals (mainSunLight, null))
			{
				Utils.LogError ("No light found for " + scattererBody.mainSunCelestialBody + " for body " + parentCelestialBody.name + ". Defaulting to main sunlight, godrays, lightrays and caustics may look wrong, check your Kopernicus configuration.");
				mainSunLight = Scatterer.Instance.sunLight;
			}
			else
			{
				if (Scatterer.Instance.mainSettings.terrainShadows)
					Scatterer.Instance.SetShadowsForLight (mainSunLight);
				else
					Scatterer.Instance.DisableCustomShadowResForLight (mainSunLight);
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
						mainMenuScaleFactor = (_go.transform.localScale.x / sctBodyTransform.localScale.x);
						m_radius = mainMenuScaleFactor * parentCelestialBody.Radius;
					}
				}
			}
			else
			{
				m_radius = parentCelestialBody.Radius;
			}

			InitSecondarySuns ();

			InitSkyAndOceanNodes ();
		}

		//TODO: move to utils
		Light findLight (string sunCelestialBody)
		{
			Light light = Scatterer.Instance.lights.SingleOrDefault (_light => (_light != null) && (_light.gameObject != null) && (_light.gameObject.name == sunCelestialBody));

			if (ReferenceEquals(light,null) && (sunCelestialBody == "Sun"))
				light = Scatterer.Instance.sunLight;

			return light;
		}
		
		Light findScaledLight (string sunCelestialBody)
		{
			Light light = Scatterer.Instance.lights.SingleOrDefault (_light => (_light != null) && (_light.gameObject != null) && (_light.gameObject.name == ("Scaledspace SunLight "+sunCelestialBody)));

			if (ReferenceEquals(light,null) && (sunCelestialBody == "Sun"))
				light = Scatterer.Instance.scaledSpaceSunLight;
			
			return  light;
		}

		void FindEclipseCasters (ScattererCelestialBody scattererBody)
		{
			if (Scatterer.Instance.mainSettings.useEclipses) {
				for (int k = 0; k < scattererBody.eclipseCasters.Count; k++) {
					var cc = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.eclipseCasters [k]);
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
				for (int k = 0; k < scattererBody.planetshineSources.Count; k++)
				{
					var cc = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == scattererBody.planetshineSources [k].bodyName);
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
			skyNode = (SkyNode)Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SkyNode));
			skyNode.prolandManager = this;
			skyNode.SetCelestialBodyName (parentCelestialBody.name);
			skyNode.SetParentScaledTransform (parentScaledTransform);
			skyNode.SetParentLocalTransform (parentLocalTransform);
			skyNode.usesCloudIntegration = usesCloudIntegration;
			skyNode.mainMenuScaleFactor = mainMenuScaleFactor;

			if (skyNode.LoadFromConfigNode ())
			{
				skyNode.Init ();
				if (hasOcean && Scatterer.Instance.mainSettings.useOceanShaders && (HighLogic.LoadedScene != GameScenes.MAINMENU))
				{
					if (Scatterer.Instance.mainSettings.oceanFoam)
						oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
					else
						oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

					oceanNode.Init (this);
				}
			}
		}
		
		public void Update()
		{
			if (secondarySuns.Count > 0)
			{
				UpdateSecondarySuns();
			}

			skyNode.UpdateNode();
			
			if (!ReferenceEquals(oceanNode,null))
			{
				oceanNode.UpdateNode();
			}
		}

		void FindSecondarySuns (ScattererCelestialBody scattererBody)
		{
			foreach (SecondarySunConfig sunConfig in scattererBody.secondarySuns)
			{
				SecondarySun secondarySun = SecondarySun.FindSecondarySun (sunConfig);
				if (!ReferenceEquals (secondarySun, null))
				{
					secondarySun.sunLight = findLight (sunConfig.celestialBodyName);
					secondarySun.scaledSunLight = findScaledLight (sunConfig.celestialBodyName);
					secondarySuns.Add (secondarySun);
				}
			}
		}
		
		void InitSecondarySuns ()
		{
			FindSecondarySuns (scattererCelestialBody);

			planetShineRGBMatrix = Matrix4x4.zero;
			
			for (int i = 0; i < Math.Min (4, secondarySuns.Count); i++)
			{
				planetShineRGBMatrix.SetRow (i, new Vector4 (secondarySuns[i].config.sunColor.r, secondarySuns[i].config.sunColor.g, secondarySuns[i].config.sunColor.b, 1.0f));
				if (secondarySuns[i].scaledSunLight != null)
					planetShineOriginalRGBMatrix.SetRow (i, new Vector4 (secondarySuns[i].scaledSunLight.color.r, secondarySuns[i].scaledSunLight.color.g, secondarySuns[i].scaledSunLight.color.b, 1.0f));
				else
					planetShineOriginalRGBMatrix.SetRow(i, planetShineRGBMatrix.GetRow(i));
			}
		}

		void UpdateSecondarySuns ()
		{
			planetShineSourcesMatrix = Matrix4x4.zero;

			for (int i = 0; i < Math.Min (4, secondarySuns.Count); i++)
			{
				Vector3 sourcePosRelPlanet = Vector3.Scale (secondarySuns[i].celestialBody.position - parentCelestialBody.GetTransform ().position, new Vector3d (ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor));	//has to be this that is borked
				planetShineSourcesMatrix.SetRow (i, new Vector4 (sourcePosRelPlanet.x, sourcePosRelPlanet.y, sourcePosRelPlanet.z, 1.0f));

				if (secondarySuns[i].scaledSunLight != null)
				{
					if (sunsUseIntensityCurves)
					{
						planetShineRGBMatrix[i,3] = secondarySuns[i].scaledSunLight.intensity;
					}

					if (!cloudIntegrationUsesScattererSunColors)
					{
						planetShineOriginalRGBMatrix.SetRow (i, new Vector4 (secondarySuns[i].scaledSunLight.color.r, secondarySuns[i].scaledSunLight.color.g, secondarySuns[i].scaledSunLight.color.b, secondarySuns[i].scaledSunLight.intensity));
					}
				}
			}
		}
		
		public void OnDestroy()
		{
			if (!ReferenceEquals(skyNode,null))
			{
				skyNode.Cleanup();
				Component.DestroyImmediate(skyNode);
			}
			
			if (!ReferenceEquals(oceanNode,null)) {
				oceanNode.Cleanup();
				Component.DestroyImmediate(oceanNode);
			}
		}

		//TODO: change this so that it takes the new configNode and that's all? May not be possible depending on if it needs to recreate lightraysRenderer and stuff
		//Therefor add an option to init from configNode? yep
		public void reBuildOcean()
		{
			if (!ReferenceEquals(oceanNode,null))
			{
				oceanNode.Cleanup();
				Component.Destroy(oceanNode);
				UnityEngine.Object.Destroy(oceanNode);

				if (Scatterer.Instance.mainSettings.oceanFoam)
					oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanWhiteCaps));
				else
					oceanNode = (OceanFFTgpu) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(OceanFFTgpu));

				oceanNode.Init(this);

				Utils.LogDebug("Rebuilt Ocean");
			}
		}

		public Vector3 getDirectionToMainSun()
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
			return oceanNode;
		}
		
		public SkyNode GetSkyNode() {
			return skyNode;
		}

		public Color getIntensityModulatedSunColor()
		{
			return (sunsUseIntensityCurves ? sunColor * mainScaledSunLight.intensity : sunColor);
		}
	}
}