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
	public class GUIhandler: MonoBehaviour
	{

		public Rect windowRect = new Rect (0, 0, 400, 50);
		public int windowId;

		public bool visible = false;
		public bool mainOptions=false;

		public int selectedPlanet = 0;
		public int selectedConfigPoint = 0;
		bool wireFrame = false;

		private Vector2 _scroll;
		private Vector2 _scroll2;
		public bool displayOceanSettings = false;

		float experimentalAtmoScale=1f;
		float viewdirOffset=0f;

		float rimBlend = 20f;
		float rimpower = 600f;
		float cloudColorMultiplier=1f;
		float cloudScatteringMultiplier=1f;
		float cloudSkyIrradianceMultiplier = 1f;
		float volumetricsColorMultiplier=1f;

		float mieG = 0.85f;
		float openglThreshold = 10f;

		float extinctionThickness = 1f;
		float skyExtinctionTint = 1f;
		
		float specR = 0f, specG = 0f, specB = 0f, shininess = 0f;
		
		//ConfigPoint variables 		
		float pointAltitude = 0f;
		float newCfgPtAlt = 0f;
		int configPointsCnt;
		float postProcessingalpha = 78f;
		float postProcessDepth = 200f;
		
		float extinctionTint=100f;
		
		float postProcessExposure = 18f;
		
		//sky properties
		float exposure = 25f;
		float alphaGlobal = 100f;

		//other stuff
		float atmosphereGlobalScale = 1000f;

		ModularGUI oceanModularGUI = new ModularGUI();

		public GUIhandler ()
		{
		}

		public void Init()
		{
			windowId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			mainOptions = (HighLogic.LoadedScene == GameScenes.SPACECENTER);
			windowRect.x=Scatterer.Instance.pluginData.inGameWindowLocation.x;
			windowRect.y=Scatterer.Instance.pluginData.inGameWindowLocation.y;
		}

		public void UpdateGUIvisible()
		{
			if ((Input.GetKey (Scatterer.Instance.pluginData.guiModifierKey1) || Input.GetKey (Scatterer.Instance.pluginData.guiModifierKey2)) && (Input.GetKeyDown (Scatterer.Instance.pluginData.guiKey1) || (Input.GetKeyDown (Scatterer.Instance.pluginData.guiKey2))))
			{
				if (ToolbarButton.Instance.button!= null)
				{
					if (visible)
						ToolbarButton.Instance.button.SetFalse(false);
					else
						ToolbarButton.Instance.button.SetTrue(false);
				}
				visible = !visible;
			}
		}

		public void DrawGui()
		{
			if (visible)
			{
				windowRect = GUILayout.Window (windowId, windowRect, DrawScattererWindow,"Scatterer v"+Scatterer.Instance.versionNumber+": "
				                               + Scatterer.Instance.pluginData.guiModifierKey1String+"/"+Scatterer.Instance.pluginData.guiModifierKey2String +"+" +Scatterer.Instance.pluginData.guiKey1String
				                               +"/"+Scatterer.Instance.pluginData.guiKey2String+" toggle");
				
				//prevent window from going offscreen
				windowRect.x = Mathf.Clamp(windowRect.x,0,Screen.width-windowRect.width);
				windowRect.y = Mathf.Clamp(windowRect.y,0,Screen.height-windowRect.height);
			}
		}
		
		public void DrawScattererWindow (int windowId)
		{
			if (mainOptions)
			{ 
				DrawOptionsMenu ();
			}
			
			else if (Scatterer.Instance.isActive)
			{
				DrawPlanetSelectionHeader ();

				if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active)
				{
					configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
					
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Atmosphere settings"))
					{
						displayOceanSettings = false;
					}					
					if (GUILayout.Button ("Ocean settings"))
					{
						displayOceanSettings = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean;
					}
					GUILayout.EndHorizontal ();					

					if (!displayOceanSettings)
					{
						DrawAtmosphereGUI ();
					}
					else
					{
						DrawOceanGUI ();
					}
					
					DrawSharedFooterGUI ();
				}
			}
			else
			{
				GUILayout.Label (String.Format ("Inactive in tracking station and VAB/SPH"));
				GUILayout.EndHorizontal ();
			}


			GUI.DragWindow();
		}

		static void DrawOptionsMenu ()
		{
			GUILayout.Label (String.Format ("Scatterer: features selector"));
			Scatterer.Instance.mainSettings.useOceanShaders = GUILayout.Toggle (Scatterer.Instance.mainSettings.useOceanShaders, "Ocean shaders (may require game restart on change)");
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Ocean: fourierGridSize (64:fast,128:normal,256:HQ)");
			Scatterer.Instance.mainSettings.m_fourierGridSize = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.m_fourierGridSize.ToString ())));
			GUILayout.EndHorizontal ();
			Scatterer.Instance.mainSettings.oceanSkyReflections = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanSkyReflections, "Ocean: Accurate sky reflection");
			Scatterer.Instance.mainSettings.shadowsOnOcean = GUILayout.Toggle (Scatterer.Instance.mainSettings.shadowsOnOcean, "Ocean: Craft/Terrain shadows (may have artifacts on Directx11)");
			Scatterer.Instance.mainSettings.oceanPixelLights = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanPixelLights, "Ocean: lights compatibility (huge performance hit when lights on)");
			Scatterer.Instance.mainSettings.oceanCaustics = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCaustics, "Ocean: Caustics");
			//Core.Instance.mainSettings.usePlanetShine = GUILayout.Toggle(Core.Instance.usePlanetShine, "PlanetShine");
			Scatterer.Instance.mainSettings.integrateWithEVEClouds = GUILayout.Toggle (Scatterer.Instance.mainSettings.integrateWithEVEClouds, "Integrate effects with EVE clouds (may require restart)");
			Scatterer.Instance.mainSettings.fullLensFlareReplacement = GUILayout.Toggle (Scatterer.Instance.mainSettings.fullLensFlareReplacement, "Lens flare shader");
			Scatterer.Instance.mainSettings.useEclipses = GUILayout.Toggle (Scatterer.Instance.mainSettings.useEclipses, "Eclipses (WIP, sky/orbit only for now)");
			Scatterer.Instance.mainSettings.useRingShadows = GUILayout.Toggle (Scatterer.Instance.mainSettings.useRingShadows, "Kopernicus ring shadows");
			//Core.Instance.mainSettings.useGodrays = GUILayout.Toggle(Core.Instance.useGodrays, "Godrays (early WIP)");
			Scatterer.Instance.mainSettings.terrainShadows = GUILayout.Toggle (Scatterer.Instance.mainSettings.terrainShadows, "Terrain shadows");
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Shadow bias");
			Scatterer.Instance.mainSettings.shadowBias = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.shadowBias.ToString ("0.000")));
			GUILayout.Label ("Shadow normal bias");
			Scatterer.Instance.mainSettings.shadowNormalBias = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.shadowNormalBias.ToString ("0.000")));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			Scatterer.Instance.mainSettings.overrideNearClipPlane = GUILayout.Toggle (Scatterer.Instance.mainSettings.overrideNearClipPlane, "Override Near ClipPlane (not recommended - restart on disable)");
			Scatterer.Instance.mainSettings.nearClipPlane = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.nearClipPlane.ToString ("0.000")));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Menu scroll section height");
			Scatterer.Instance.pluginData.scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.pluginData.scrollSectionHeight.ToString ())));
			GUILayout.EndHorizontal ();
			Scatterer.Instance.mainSettings.disableAmbientLight = GUILayout.Toggle (Scatterer.Instance.mainSettings.disableAmbientLight, "Disable scaled space ambient light");
			Scatterer.Instance.mainSettings.sunlightExtinction = GUILayout.Toggle (Scatterer.Instance.mainSettings.sunlightExtinction, "Sunlight extinction (direct sun light changes color with sunset/dusk)");
			Scatterer.Instance.mainSettings.underwaterLightDimming = GUILayout.Toggle (Scatterer.Instance.mainSettings.underwaterLightDimming, "Dim light underwater");
			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");
			GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.baseConfigs [0].parent.url);
			GUILayout.EndHorizontal ();
		}

		void DrawPlanetSelectionHeader ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Planet:");
			if (GUILayout.Button ("<")) {
				if (selectedPlanet > 0) {
					selectedPlanet -= 1;
					selectedConfigPoint = 0;
					if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active) {
						loadConfigPoint (selectedConfigPoint);
						getSettingsFromSkynode ();
						if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean) {
							buildOceanGUI ();
						}
					}
				}
			}
			GUILayout.TextField ((Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].celestialBodyName).ToString ());
			if (GUILayout.Button (">")) {
				if (selectedPlanet < Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Count - 1) {
					selectedPlanet += 1;
					selectedConfigPoint = 0;
					if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active) {
						loadConfigPoint (selectedConfigPoint);
						getSettingsFromSkynode ();
						if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean) {
							buildOceanGUI ();
						}
					}
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Planet loaded:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active.ToString () + "                                Has ocean:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean.ToString ());
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Load distance:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].loadDistance.ToString () + "                             Unload distance:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].unloadDistance.ToString ());
			GUILayout.EndHorizontal ();
		}

		void DrawAtmosphereGUI ()
		{
			ConfigPoint _cur = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint];

			//if (!MapView.MapIsEnabled)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("New point altitude:");
				newCfgPtAlt = Convert.ToSingle (GUILayout.TextField (newCfgPtAlt.ToString ()));
				if (GUILayout.Button ("Add")) {
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Insert (selectedConfigPoint + 1, new ConfigPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100, postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100, skyExtinctionTint / 100, openglThreshold, viewdirOffset, extinctionTint / 100, extinctionThickness));
					selectedConfigPoint += 1;
					configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
					loadConfigPoint (selectedConfigPoint);
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Config point:");
				if (GUILayout.Button ("<")) {
					if (selectedConfigPoint > 0) {
						selectedConfigPoint -= 1;
						loadConfigPoint (selectedConfigPoint);
					}
				}
				GUILayout.TextField ((selectedConfigPoint).ToString ());
				if (GUILayout.Button (">")) {
					if (selectedConfigPoint < configPointsCnt - 1) {
						selectedConfigPoint += 1;
						loadConfigPoint (selectedConfigPoint);
					}
				}
				//GUILayout.Label (String.Format("Total:{0}", configPointsCnt));
				if (GUILayout.Button ("Delete")) {
					if (configPointsCnt <= 1)
						print ("Can't delete config point, one or no points remaining");
					else {
						Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.RemoveAt (selectedConfigPoint);
						if (selectedConfigPoint >= configPointsCnt - 1) {
							selectedConfigPoint = configPointsCnt - 2;
						}
						configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
						loadConfigPoint (selectedConfigPoint);
					}
				}
				GUILayout.EndHorizontal ();
				GUIfloat ("Point altitude", ref pointAltitude, ref _cur.altitude);
				_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (Scatterer.Instance.pluginData.scrollSectionHeight));
				GUILayout.Label ("(settings with a * are global and not cfgPoint dependent)");
				GUILayout.Label ("Atmo");
				GUIfloat ("ExperimentalAtmoScale*", ref experimentalAtmoScale, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
				GUIfloat ("AtmosphereGlobalScale*", ref atmosphereGlobalScale, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);
				GUIfloat ("mieG*", ref mieG, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_mieG);
				GUILayout.Label ("Sky");
				GUIfloat ("Sky Exposure", ref exposure, ref _cur.skyExposure);
				GUIfloat ("Sky Alpha", ref alphaGlobal, ref _cur.skyAlpha);
				GUIfloat ("Sky Extinction Tint", ref skyExtinctionTint, ref _cur.skyExtinctionTint);
				GUILayout.Label ("Scattering and Extinction");
				GUIfloat ("Scattering Exposure (scaled+local)", ref postProcessExposure, ref _cur.scatteringExposure);
				GUIfloat ("Extinction Tint (scaled+local)", ref extinctionTint, ref _cur.extinctionTint);
				GUIfloat ("Extinction Thickness (scaled+local)", ref extinctionThickness, ref _cur.extinctionThickness);
				GUILayout.Label ("Post Processing");
				GUIfloat ("Post Processing Alpha", ref postProcessingalpha, ref _cur.postProcessAlpha);
				GUIfloat ("Post Processing Depth", ref postProcessDepth, ref _cur.postProcessDepth);
				GUILayout.Label ("Artifact Fixes");
				GUIfloat ("ViewDirOffset", ref viewdirOffset, ref _cur.viewdirOffset);
				GUIfloat ("Depth buffer Threshold", ref openglThreshold, ref _cur.openglThreshold);
			}
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.usesCloudIntegration) {
				GUILayout.Label ("EVE integration");
				GUIfloat ("Cloud Color Multiplier*", ref cloudColorMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudColorMultiplier);
				GUIfloat ("Cloud Scattering Multiplier*", ref cloudScatteringMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudScatteringMultiplier);
				GUIfloat ("Cloud Sky irradiance Multiplier*", ref cloudSkyIrradianceMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudSkyIrradianceMultiplier);
				GUIfloat ("Volumetrics Color Multiplier*", ref volumetricsColorMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsColorMultiplier);
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Preserve cloud colors*");
				GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.EVEIntegration_preserveCloudColors.ToString ());
				if (GUILayout.Button ("Toggle"))
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePreserveCloudColors ();
				GUILayout.EndHorizontal ();
				//								GUIfloat("Volumetrics Scattering Multiplier", ref volumetricsScatteringMultiplier, ref Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsScatteringMultiplier);
				//								GUIfloat("Volumetrics Sky irradiance Multiplier", ref volumetricsSkyIrradianceMultiplier, ref Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsSkyIrradianceMultiplier);
			}
			GUILayout.Label ("ScaledSpace model");
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("RimBlend*");
			rimBlend = Convert.ToSingle (GUILayout.TextField (rimBlend.ToString ()));
			GUILayout.Label ("RimPower*");
			rimpower = Convert.ToSingle (GUILayout.TextField (rimpower.ToString ()));
			if (GUILayout.Button ("Set")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Spec*: R");
			specR = (float)(Convert.ToDouble (GUILayout.TextField (specR.ToString ())));
			GUILayout.Label ("G");
			specG = (float)(Convert.ToDouble (GUILayout.TextField (specG.ToString ())));
			GUILayout.Label ("B");
			specB = (float)(Convert.ToDouble (GUILayout.TextField (specB.ToString ())));
			GUILayout.Label ("shine*");
			shininess = (float)(Convert.ToDouble (GUILayout.TextField (shininess.ToString ())));
			if (GUILayout.Button ("Set")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR = specR;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG = specG;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB = specB;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess = shininess;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.EndScrollView ();
			GUILayout.BeginHorizontal ();
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0)
				GUILayout.Label ("Current state:Lowest configPoint, cfgPoint 0");
			else
				if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt)
					GUILayout.Label (String.Format ("Current state:Highest configPoint, cfgPoint{0}", Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1));
				else
					GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage)), Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage), Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Toggle PostProcessing")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePostProcessing ();
			}
			GUILayout.EndHorizontal ();
			//							GUILayout.BeginHorizontal ();
			//							if (GUILayout.Button ("toggle sky"))
			//							{
			//								Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled = !Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled;
			//								if (Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled)
			//									Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere();
			//								else
			//									Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.RestoreStockAtmosphere();
			//							}
			//							GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Save atmo")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.saveToConfigNode ();
			}
			if (GUILayout.Button ("Load atmo")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.loadFromConfigNode ();
				getSettingsFromSkynode ();
				loadConfigPoint (selectedConfigPoint);
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");
			GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configUrl.parent.url);
			GUILayout.EndHorizontal ();
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Map EVE clouds")) {
					Scatterer.Instance.eveReflectionHandler.MapEVEClouds ();
					foreach (ScattererCelestialBody _cel in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies) {
						if (_cel.active) {
							_cel.m_manager.m_skyNode.initiateEVEClouds ();
							if (!_cel.m_manager.m_skyNode.inScaledSpace)
								_cel.m_manager.m_skyNode.mapEVEVolumetrics ();
						}
					}
				}
				GUILayout.EndHorizontal ();
			}
		}


		public void buildOceanGUI()
		{
			OceanWhiteCaps oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();

			oceanModularGUI.ClearModules ();
			oceanModularGUI.AddModule(new GUIModuleFloat("Alpha/WhiteCap Radius", oceanNode, "alphaRadius")); //TODO rename this and check what it does
			oceanModularGUI.AddModule(new GUIModuleFloat("ocean Alpha", oceanNode, "oceanAlpha"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Transparency Depth", oceanNode, "transparencyDepth"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Darkness Depth", oceanNode, "darknessDepth"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Refraction Index", oceanNode, "refractionIndex"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Shore foam strength", oceanNode, "shoreFoam"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Foam strength (m_whiteCapStr)", oceanNode, "m_whiteCapStr"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Far foam strength (m_farWhiteCapStr)", oceanNode, "m_farWhiteCapStr"));
			oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Upwelling Color", oceanNode, "m_oceanUpwellingColor"));
			oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Underwater Color", oceanNode, "m_UnderwaterColor"));

			if (Scatterer.Instance.mainSettings.oceanCaustics)
			{
				oceanModularGUI.AddModule (new GUIModuleVector2 ("Caustics layer 1 scale", oceanNode, "causticsLayer1Scale"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("caustics layer 1 speed", oceanNode, "causticsLayer1Speed"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("Caustics Layer 2 scale", oceanNode, "causticsLayer2Scale"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("caustics Layer 2 speed", oceanNode, "causticsLayer2Speed"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics texture multiply", oceanNode, "causticsMultiply"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics underwater light boost", oceanNode, "causticsUnderwaterLightBoost"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics minimum brightness (of dark areas in the caustics texture)", oceanNode, "causticsMinBrightness"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics blur depth", oceanNode, "causticsBlurDepth"));
			}

			oceanModularGUI.AddModule (new GUIModuleLabel ("To apply the next setting press \"rebuild ocean\" and wait"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("Keep in mind this saves your current settings"));
			oceanModularGUI.AddModule(new GUIModuleFloat("AMP (default 1)", oceanNode, "AMP"));
			oceanModularGUI.AddModule(new GUIModuleFloat("wind Speed", oceanNode, "m_windSpeed"));
			oceanModularGUI.AddModule(new GUIModuleFloat("omega: inverse wave age", oceanNode, "m_omega"));
			oceanModularGUI.AddModule(new GUIModuleFloat("foamMipMapBias", oceanNode, "m_foamMipMapBias"));
			oceanModularGUI.AddModule(new GUIModuleInt("m_ansio", oceanNode, "m_ansio"));
			oceanModularGUI.AddModule(new GUIModuleInt("m_foamAnsio", oceanNode, "m_foamAnsio"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("Performance settings"));
			oceanModularGUI.AddModule(new GUIModuleInt("m_varianceSize (sun reflection, power of 2)", oceanNode, "m_varianceSize"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("m_varianceSize increases rebuild time exponentially"));
			oceanModularGUI.AddModule(new GUIModuleInt("Ocean mesh resolution (lower is better)", oceanNode, "m_resolution"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("current fourierGridSize: " + Scatterer.Instance.mainSettings.m_fourierGridSize.ToString ()));
		}

		void DrawOceanGUI ()
		{
			OceanWhiteCaps oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();
			//GUItoggle("Toggle ocean", ref stockOcean);
			_scroll2 = GUILayout.BeginScrollView (_scroll2, false, true, GUILayout.Width (400), GUILayout.Height (Scatterer.Instance.pluginData.scrollSectionHeight + 100));
			{
				oceanModularGUI.RenderGUI();
			}
			GUILayout.EndScrollView ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Apply settings/Rebuild ocean")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.reBuildOcean ();
				buildOceanGUI();
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Save ocean")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
			}
			if (GUILayout.Button ("Load ocean")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().loadFromConfigNode ();
				buildOceanGUI ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");
			GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().configUrl.parent.url);
			GUILayout.EndHorizontal ();
		}

		void DrawSharedFooterGUI ()
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Toggle WireFrame")) {
				if (wireFrame) {
					if (HighLogic.LoadedScene != GameScenes.TRACKSTATION) {
						if (Scatterer.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)))
							Component.Destroy (Scatterer.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)));
						if (Scatterer.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)))
							Component.Destroy (Scatterer.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)));
					}
					if (Scatterer.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (Scatterer.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
					wireFrame = false;
				}
				else {
					if (HighLogic.LoadedScene != GameScenes.TRACKSTATION) {
						Scatterer.Instance.nearCamera.gameObject.AddComponent (typeof(Wireframe));
						Scatterer.Instance.farCamera.gameObject.AddComponent (typeof(Wireframe));
					}
					Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(Wireframe));
					wireFrame = true;
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Reload shader bundles")) {
				ShaderReplacer.Instance.LoadAssetBundle ();
			}
			GUILayout.EndHorizontal ();
		}

		public void GUIfloat (string label, ref float local, ref float target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			local = float.Parse (GUILayout.TextField (local.ToString ("00000.000000")));
			if (GUILayout.Button ("Set")) {
				target = local;
			}
			GUILayout.EndHorizontal ();
		}
		
		public void GUIint (string label, ref int local, ref int target, int divisionFactor)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			local = (Int32)(Convert.ToInt32 (GUILayout.TextField (local.ToString ())));
			
			
			if (GUILayout.Button ("Set")) {
				target = local / divisionFactor;
			}
			GUILayout.EndHorizontal ();
		}

		public void GUIvector2 (string label, ref float localR, ref float localG, ref Vector2 target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			localR = float.Parse (GUILayout.TextField (localR.ToString ("0000.00000")));
			localG = float.Parse (GUILayout.TextField (localG.ToString ("0000.00000")));
			
			if (GUILayout.Button ("Set")) {
				target = new Vector2 (localR, localG);
			}
			GUILayout.EndHorizontal ();
		}

		public void GUIvector3 (string label, ref float localR, ref float localG, ref float localB, ref Vector3 target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			localR = float.Parse (GUILayout.TextField (localR.ToString ("0000.00000")));
			localG = float.Parse (GUILayout.TextField (localG.ToString ("0000.00000")));
			localB = float.Parse (GUILayout.TextField (localB.ToString ("0000.00000")));
			
			if (GUILayout.Button ("Set")) {
				target = new Vector3 (localR, localG, localB);
			}
			GUILayout.EndHorizontal ();
		}
		
		
		public void GUItoggle (string label, ref bool toToggle)
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button (label))
				toToggle = !toToggle;
			GUILayout.EndHorizontal ();
		}

		public void getSettingsFromSkynode ()
		{
			SkyNode skyNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode;
			ConfigPoint selected = skyNode.configPoints [selectedConfigPoint];
			
			postProcessingalpha = selected.postProcessAlpha;
			postProcessDepth = selected.postProcessDepth;
			
			extinctionTint = selected.extinctionTint;
			
			postProcessExposure = selected.scatteringExposure;
			exposure = selected.skyExposure;
			alphaGlobal = selected.skyAlpha;
			
			openglThreshold = selected.openglThreshold;			

			configPointsCnt = skyNode.configPoints.Count;
			
			specR = skyNode.specR;
			specG = skyNode.specG;
			specB = skyNode.specB;
			shininess = skyNode.shininess;

			rimBlend = skyNode.rimBlend;
			rimpower = skyNode.rimpower;

			skyExtinctionTint = selected.skyExtinctionTint;

			extinctionThickness = selected.extinctionThickness;

			mieG = skyNode.m_mieG;
			
			experimentalAtmoScale = skyNode.experimentalAtmoScale;
			atmosphereGlobalScale = skyNode.atmosphereGlobalScale;
			viewdirOffset = selected.viewdirOffset;
			
			cloudColorMultiplier = skyNode.cloudColorMultiplier;
			cloudScatteringMultiplier = skyNode.cloudScatteringMultiplier;
			cloudSkyIrradianceMultiplier = skyNode.cloudSkyIrradianceMultiplier;
			
			volumetricsColorMultiplier = skyNode.volumetricsColorMultiplier;
			//			volumetricsScatteringMultiplier = skyNode.volumetricsScatteringMultiplier;
			//			volumetricsSkyIrradianceMultiplier = skyNode.volumetricsSkyIrradianceMultiplier;
		}
		
		public void loadConfigPoint (int point)
		{
			ConfigPoint _cur = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point];
			
			postProcessDepth = _cur.postProcessDepth;
			extinctionTint = _cur.extinctionTint;
			postProcessExposure = _cur.scatteringExposure;
			postProcessingalpha = _cur.postProcessAlpha;
			
			alphaGlobal = _cur.skyAlpha;
			exposure = _cur.skyExposure;
			skyExtinctionTint = _cur.skyExtinctionTint;

			extinctionThickness = _cur.extinctionThickness;
			
			pointAltitude = _cur.altitude;
			
			openglThreshold = _cur.openglThreshold;
			viewdirOffset = _cur.viewdirOffset;
		}


	}
}

