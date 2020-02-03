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

		float oceanAlpha = 1f;
		float oceanAlphaRadius = 3000f;
		float AMP = 1.0f;
		float m_windSpeed = 5.0f; //A higher wind speed gives greater swell to the waves
		float m_omega = 0.84f; //A lower number means the waves last longer and will build up larger waves
		
		int m_ansio = 2;
		int m_varianceSize = 4;
		int m_foamAnsio = 9;
		float m_foamMipMapBias = -2.0f;
		float m_whiteCapStr = 0.1f;
		float farWhiteCapStr = 0.1f;
		//float choppynessMultiplier = 1f;
		
		//		Vector3 m_oceanUpwellingColor = new Vector3 (0.039f, 0.156f, 0.47f);
		float oceanUpwellingColorR = 0.0039f;
		float oceanUpwellingColorG = 0.0156f;
		float oceanUpwellingColorB = 0.047f;
		
		float oceanUnderwaterColorR = 0.1f;
		float oceanUnderwaterColorG = 0.75f;
		float oceanUnderwaterColorB = 0.8f;
		
		float transparencyDepth = 60f;
		float darknessDepth = 1000f;
		float refractionIndex = 1.33f;
		float shoreFoam = 1.0f;
		
		int m_resolution = 4;
		//		int MAX_VERTS = 65000;
		
		Vector4 m_gridSizes = new Vector4 (5488, 392, 28, 2); //Size in meters (i.e. in spatial domain) of each grid
		Vector4 m_choppyness = new Vector4 (2.3f, 2.1f, 1.3f, 0.9f); //strengh of sideways displacement for each grid

		//other stuff
		float atmosphereGlobalScale = 1000f;

		public GUIhandler ()
		{
		}


		//		UI BUTTONS
		//		This isn't the most elegant section due to how many elements are here
		//		I don't care enough to do it in a cleaner way
		//		After all it's a basic UI for tweaking settings and it does its job
		public void DrawScattererWindow (int windowId)
		{
			if (Core.Instance.mainMenuOptions)  //KSC screen options
			{ 
				GUILayout.Label (String.Format ("Scatterer: features selector"));
				Core.Instance.mainSettings.useOceanShaders = GUILayout.Toggle(Core.Instance.mainSettings.useOceanShaders, "Ocean shaders (may require game restart on change)");
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Ocean: fourierGridSize (64:fast,128:normal,256:HQ)");
				Core.Instance.mainSettings.m_fourierGridSize = (Int32)(Convert.ToInt32 (GUILayout.TextField (Core.Instance.mainSettings.m_fourierGridSize.ToString ())));
				GUILayout.EndHorizontal ();
				
				Core.Instance.mainSettings.oceanSkyReflections = GUILayout.Toggle(Core.Instance.mainSettings.oceanSkyReflections, "Ocean: accurate sky reflection");
				Core.Instance.mainSettings.shadowsOnOcean = GUILayout.Toggle(Core.Instance.mainSettings.shadowsOnOcean, "Ocean: Craft/Terrain shadows (may have artifacts on Directx11)");
				Core.Instance.mainSettings.oceanPixelLights = GUILayout.Toggle(Core.Instance.mainSettings.oceanPixelLights, "Ocean: lights compatibility (huge performance hit when lights on)");
				
				//Core.Instance.mainSettings.usePlanetShine = GUILayout.Toggle(Core.Instance.usePlanetShine, "PlanetShine");
				Core.Instance.mainSettings.integrateWithEVEClouds = GUILayout.Toggle(Core.Instance.mainSettings.integrateWithEVEClouds, "Integrate effects with EVE clouds (may require restart)");
				
				Core.Instance.mainSettings.fullLensFlareReplacement=GUILayout.Toggle(Core.Instance.mainSettings.fullLensFlareReplacement, "Lens flare shader");
				Core.Instance.mainSettings.useEclipses = GUILayout.Toggle(Core.Instance.mainSettings.useEclipses, "Eclipses (WIP, sky/orbit only for now)");
				Core.Instance.mainSettings.useRingShadows = GUILayout.Toggle(Core.Instance.mainSettings.useRingShadows, "Kopernicus ring shadows");
				//Core.Instance.mainSettings.useGodrays = GUILayout.Toggle(Core.Instance.useGodrays, "Godrays (early WIP)");
				
				Core.Instance.mainSettings.terrainShadows = GUILayout.Toggle(Core.Instance.mainSettings.terrainShadows, "Terrain shadows");
				GUILayout.BeginHorizontal ();
				
				GUILayout.Label ("Shadow bias");
				Core.Instance.mainSettings.shadowBias = float.Parse (GUILayout.TextField (Core.Instance.mainSettings.shadowBias.ToString ("0.000")));
				
				GUILayout.Label ("Shadow normal bias");
				Core.Instance.mainSettings.shadowNormalBias = float.Parse (GUILayout.TextField (Core.Instance.mainSettings.shadowNormalBias.ToString ("0.000")));
				
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
				Core.Instance.mainSettings.overrideNearClipPlane = GUILayout.Toggle(Core.Instance.mainSettings.overrideNearClipPlane, "Override Near ClipPlane (not recommended - restart on disable)");
				Core.Instance.mainSettings.nearClipPlane = float.Parse (GUILayout.TextField (Core.Instance.mainSettings.nearClipPlane.ToString ("0.000")));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Menu scroll section height");
				Core.Instance.scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (Core.Instance.scrollSectionHeight.ToString ())));
				GUILayout.EndHorizontal ();
				
				Core.Instance.mainSettings.disableAmbientLight = GUILayout.Toggle(Core.Instance.mainSettings.disableAmbientLight, "Disable scaled space ambient light");
				Core.Instance.mainSettings.sunlightExtinction = GUILayout.Toggle(Core.Instance.mainSettings.sunlightExtinction, "Sunlight extinction (direct sun light changes color with sunset/dusk)");
				Core.Instance.mainSettings.underwaterLightDimming = GUILayout.Toggle(Core.Instance.mainSettings.underwaterLightDimming, "Dim light underwater");
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label (".cfg file used:");
				GUILayout.TextField(Core.Instance.baseConfigs [0].parent.url);
				GUILayout.EndHorizontal ();
			}
			
			else if (Core.Instance.isActive)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Planet:");
				
				if (GUILayout.Button ("<")) {
					if (selectedPlanet > 0) {
						selectedPlanet -= 1;
						selectedConfigPoint = 0;
						if (Core.Instance.scattererCelestialBodies [selectedPlanet].active) {
							loadConfigPoint (selectedConfigPoint);
							getSettingsFromSkynode ();
							if (Core.Instance.scattererCelestialBodies [selectedPlanet].hasOcean)
								getSettingsFromOceanNode ();
						}
					}
				}
				
				GUILayout.TextField ((Core.Instance.scattererCelestialBodies [selectedPlanet].celestialBodyName).ToString ());
				
				if (GUILayout.Button (">")) {
					if (selectedPlanet < Core.Instance.scattererCelestialBodies.Count - 1) {
						selectedPlanet += 1;
						selectedConfigPoint = 0;
						if (Core.Instance.scattererCelestialBodies [selectedPlanet].active) {
							loadConfigPoint (selectedConfigPoint);
							getSettingsFromSkynode ();
							if (Core.Instance.scattererCelestialBodies [selectedPlanet].hasOcean)
								getSettingsFromOceanNode ();
						}
					}
				}
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Planet loaded:" + Core.Instance.scattererCelestialBodies [selectedPlanet].active.ToString ()+
				                 "                                Has ocean:" + Core.Instance.scattererCelestialBodies [selectedPlanet].hasOcean.ToString ());
				GUILayout.EndHorizontal ();
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Load distance:" + Core.Instance.scattererCelestialBodies [selectedPlanet].loadDistance.ToString ()+
				                 "                             Unload distance:" + Core.Instance.scattererCelestialBodies [selectedPlanet].unloadDistance.ToString ());
				GUILayout.EndHorizontal ();
				
				if (Core.Instance.scattererCelestialBodies [selectedPlanet].active)
				{
					configPointsCnt = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
					
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Atmosphere settings")) {
						displayOceanSettings = false;
						//displaySunflareSettings = false;
					}
					
					if (GUILayout.Button ("Ocean settings")) {
						if (Core.Instance.scattererCelestialBodies [selectedPlanet].hasOcean)
							displayOceanSettings = true;
						
						//displaySunflareSettings = false;
					}
					
					//						if (GUILayout.Button ("Sunflare(s) Settings")) {
					//							displayOceanSettings = false;
					//							displaySunflareSettings = true;
					//						}
					GUILayout.EndHorizontal ();
					
					ConfigPoint _cur = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [selectedConfigPoint];
					
					if (!displayOceanSettings)
					{
						//if (!MapView.MapIsEnabled)
						{
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("New point altitude:");
							newCfgPtAlt = Convert.ToSingle (GUILayout.TextField (newCfgPtAlt.ToString ()));
							if (GUILayout.Button ("Add"))
							{
								Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Insert (selectedConfigPoint + 1,
								                                                                                   new ConfigPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100,
								                 postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100, skyExtinctionTint / 100,
								                 openglThreshold,viewdirOffset,extinctionTint/100,
								                 extinctionThickness));
								selectedConfigPoint += 1;
								configPointsCnt = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
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
								else
								{
									Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.RemoveAt (selectedConfigPoint);
									if (selectedConfigPoint >= configPointsCnt - 1)
									{
										selectedConfigPoint = configPointsCnt - 2;
									}
									configPointsCnt = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Count;
									loadConfigPoint (selectedConfigPoint);
								}
								
							}
							
							GUILayout.EndHorizontal ();
														
							GUIfloat("Point altitude", ref pointAltitude, ref _cur.altitude);
														
							_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (Core.Instance.scrollSectionHeight));

							GUILayout.Label("(settings with a * are global and not cfgPoint dependent)");
							GUILayout.Label("Atmo");
							GUIfloat("ExperimentalAtmoScale*", ref experimentalAtmoScale,ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
							GUIfloat("AtmosphereGlobalScale*", ref atmosphereGlobalScale, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);
							GUIfloat("mieG*", ref mieG, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_mieG);

							GUILayout.Label("Sky");
							GUIfloat("Sky Exposure", ref exposure, ref _cur.skyExposure);
							GUIfloat("Sky Alpha", ref alphaGlobal, ref _cur.skyAlpha);
							GUIfloat("Sky Extinction Tint", ref skyExtinctionTint, ref _cur.skyExtinctionTint);

							GUILayout.Label("Scattering and Extinction");
							GUIfloat("Scattering Exposure (scaled+local)", ref postProcessExposure ,ref _cur.scatteringExposure);
							GUIfloat("Extinction Tint (scaled+local)", ref extinctionTint, ref _cur.extinctionTint);
							GUIfloat("Extinction Thickness (scaled+local)", ref extinctionThickness, ref _cur.extinctionThickness);

							GUILayout.Label("Post Processing");
							GUIfloat("Post Processing Alpha", ref postProcessingalpha, ref _cur.postProcessAlpha);
							GUIfloat("Post Processing Depth", ref postProcessDepth,ref _cur.postProcessDepth);

							GUILayout.Label("Artifact Fixes");
							GUIfloat("ViewDirOffset", ref viewdirOffset,ref _cur.viewdirOffset);
							GUIfloat("Depth buffer Threshold", ref openglThreshold, ref _cur.openglThreshold);
						}
						
						if (Core.Instance.mainSettings.integrateWithEVEClouds && Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.usesCloudIntegration)
						{
							GUILayout.Label("EVE integration");
							GUIfloat("Cloud Color Multiplier*", ref cloudColorMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudColorMultiplier);
							GUIfloat("Cloud Scattering Multiplier*", ref cloudScatteringMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudScatteringMultiplier);
							GUIfloat("Cloud Sky irradiance Multiplier*", ref cloudSkyIrradianceMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudSkyIrradianceMultiplier);
							
							GUIfloat("Volumetrics Color Multiplier*", ref volumetricsColorMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsColorMultiplier);

							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Preserve cloud colors*");
							GUILayout.TextField ( Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.EVEIntegration_preserveCloudColors.ToString ());
							if (GUILayout.Button ("Toggle"))
								Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePreserveCloudColors();
							GUILayout.EndHorizontal ();

							//								GUIfloat("Volumetrics Scattering Multiplier", ref volumetricsScatteringMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsScatteringMultiplier);
							//								GUIfloat("Volumetrics Sky irradiance Multiplier", ref volumetricsSkyIrradianceMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsSkyIrradianceMultiplier);
						}

						GUILayout.Label("ScaledSpace model");
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("RimBlend*");
						rimBlend = Convert.ToSingle (GUILayout.TextField (rimBlend.ToString ()));
						
						GUILayout.Label ("RimPower*");
						rimpower = Convert.ToSingle (GUILayout.TextField (rimpower.ToString ()));
						
						if (GUILayout.Button ("Set")) {
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
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
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR = specR;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG = specG;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB = specB;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess = shininess;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
						}
						GUILayout.EndHorizontal ();
						GUILayout.EndScrollView ();

						GUILayout.BeginHorizontal ();
						if (Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0)
							GUILayout.Label ("Current state:Lowest configPoint, cfgPoint 0");
						else if (Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt)
							GUILayout.Label (String.Format ("Current state:Highest configPoint, cfgPoint{0}", Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1));
						else
							GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage)), Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage), Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button ("Toggle PostProcessing"))
						{
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePostProcessing();
						}
						GUILayout.EndHorizontal ();
						
						//							GUILayout.BeginHorizontal ();
						//							if (GUILayout.Button ("toggle sky"))
						//							{
						//								Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled = !Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled;
						//								if (Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.skyEnabled)
						//									Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere();
						//								else
						//									Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.RestoreStockAtmosphere();
						//							}
						//							GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Save atmo"))
						{
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.saveToConfigNode ();
						}
						
						if (GUILayout.Button ("Load atmo"))
						{
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.loadFromConfigNode ();
							getSettingsFromSkynode ();
							loadConfigPoint (selectedConfigPoint);
						}
						
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						GUILayout.Label (".cfg file used:");
						GUILayout.TextField(Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configUrl.parent.url);
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Map EVE clouds"))
						{
							Core.Instance.mapEVEClouds();
							foreach (ScattererCelestialBody _cel in Core.Instance.scattererCelestialBodies)
							{
								if (_cel.active)
								{
									_cel.m_manager.m_skyNode.initiateEVEClouds();

									if (!_cel.m_manager.m_skyNode.inScaledSpace)
										_cel.m_manager.m_skyNode.mapEVEVolumetrics();
								}
							}
						}
						GUILayout.EndHorizontal ();
					}
					else
					{
						OceanWhiteCaps oceanNode = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();
						//GUItoggle("Toggle ocean", ref stockOcean);
						_scroll2 = GUILayout.BeginScrollView (_scroll2, false, true, GUILayout.Width (400), GUILayout.Height (Core.Instance.scrollSectionHeight+100));
						{
							GUIfloat ("Alpha/WhiteCap Radius", ref oceanAlphaRadius, ref oceanNode.alphaRadius);
							GUIfloat ("ocean Alpha", ref oceanAlpha, ref oceanNode.oceanAlpha);
							
							GUIfloat ("Transparency Depth", ref transparencyDepth, ref oceanNode.transparencyDepth);
							GUIfloat ("Darkness Depth", ref darknessDepth, ref oceanNode.darknessDepth);
							GUIfloat ("Refraction Index", ref refractionIndex, ref oceanNode.refractionIndex);
							GUIfloat ("Shore foam", ref shoreFoam, ref oceanNode.shoreFoam);
							
							GUIfloat ("whiteCapStr (foam)", ref m_whiteCapStr, ref oceanNode.m_whiteCapStr);
							GUIfloat ("far whiteCapStr", ref farWhiteCapStr, ref oceanNode.m_farWhiteCapStr);
							
							GUIvector3 ("Ocean Upwelling Color", ref oceanUpwellingColorR, ref oceanUpwellingColorG, ref oceanUpwellingColorB, ref oceanNode.m_oceanUpwellingColor);
							
							GUIvector3 ("Ocean Underwater Color", ref oceanUnderwaterColorR, ref oceanUnderwaterColorG, ref oceanUnderwaterColorB, ref oceanNode.m_UnderwaterColor);
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("To apply the next setting press \"rebuild ocean\" and wait");
							GUILayout.EndHorizontal ();
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Keep in mind this saves your current settings");
							GUILayout.EndHorizontal ();
							
							//						        GUIfloat("ocean Scale", ref oceanScale, ref oceanNode.oceanScale);
							//								GUIfloat ("WAVE_CM (default 0.23)", ref WAVE_CM, ref oceanNode.WAVE_CM);
							//								GUIfloat ("WAVE_KM (default 370)", ref WAVE_KM, ref oceanNode.WAVE_KM);
							GUIfloat ("AMP (default 1)", ref AMP, ref oceanNode.AMP);
							
							GUIfloat ("wind Speed", ref m_windSpeed, ref oceanNode.m_windSpeed);
							GUIfloat ("omega: inverse wave age", ref m_omega, ref oceanNode.m_omega);
							
							GUIfloat ("foamMipMapBias", ref m_foamMipMapBias, ref oceanNode.m_foamMipMapBias);
							
							
							GUIint ("m_ansio", ref m_ansio, ref oceanNode.m_ansio, 1);
							GUIint ("m_foamAnsio", ref m_foamAnsio, ref oceanNode.m_foamAnsio, 1);
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Performance settings");
							GUILayout.EndHorizontal ();
							
							GUIint ("m_varianceSize (sun reflection, power of 2)", ref m_varianceSize, ref oceanNode.m_varianceSize, 1);
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("m_varianceSize increases rebuild time exponentially");
							GUILayout.EndHorizontal ();
							
							GUIint ("Ocean mesh resolution (lower is better)", ref m_resolution, ref oceanNode.m_resolution, 1);
							
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("current fourierGridSize: "+Core.Instance.mainSettings.m_fourierGridSize.ToString());
							GUILayout.EndHorizontal ();
							
							//GUIint("Ocean renderqueue", ref oceanRenderQueue, ref oceanRenderQueue,1);
						}	
						GUILayout.EndScrollView ();
						
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Apply settings/Rebuild ocean"))
						{
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.reBuildOcean ();
						}
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						
						
						if (GUILayout.Button ("Save ocean")) {
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
						}
						
						if (GUILayout.Button ("Load ocean")) {
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().loadFromConfigNode ();
							getSettingsFromOceanNode ();
						}
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						GUILayout.Label (".cfg file used:");
						GUILayout.TextField(Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode().configUrl.parent.url);
						GUILayout.EndHorizontal ();
					}
					
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Toggle WireFrame"))
					{
						if (wireFrame)
						{
							if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
							{
								if (Core.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)))
									Component.Destroy(Core.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)));

								if (Core.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)))
									Component.Destroy(Core.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)));
							}

							if (Core.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
								Component.Destroy(Core.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));

							wireFrame=false;
						}
						
						else
						{
							if (HighLogic.LoadedScene != GameScenes.TRACKSTATION)
							{
								Core.Instance.nearCamera.gameObject.AddComponent (typeof(Wireframe));
								Core.Instance.farCamera.gameObject.AddComponent (typeof(Wireframe));
							}
							Core.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(Wireframe));
							
							wireFrame=true;
						}
					}
					GUILayout.EndHorizontal ();
					
					GUILayout.BeginHorizontal ();
					if (GUILayout.Button ("Reload shader bundles"))
					{
						ShaderReplacer.Instance.LoadAssetBundle();
					}
					GUILayout.EndHorizontal ();
				}
			}
			else
			{
				GUILayout.Label (String.Format ("Inactive in tracking station and VAB/SPH"));
				GUILayout.EndHorizontal ();
			}


			GUI.DragWindow();
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
			SkyNode skyNode = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode;
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
		
		public void getSettingsFromOceanNode ()
		{
			OceanWhiteCaps oceanNode = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();

			oceanAlpha = oceanNode.oceanAlpha;
			oceanAlphaRadius = oceanNode.alphaRadius;
			
			oceanUpwellingColorR = oceanNode.m_oceanUpwellingColor.x;
			oceanUpwellingColorG = oceanNode.m_oceanUpwellingColor.y;
			oceanUpwellingColorB = oceanNode.m_oceanUpwellingColor.z;
			
			oceanUnderwaterColorR = oceanNode.m_UnderwaterColor.x;
			oceanUnderwaterColorG = oceanNode.m_UnderwaterColor.y;
			oceanUnderwaterColorB = oceanNode.m_UnderwaterColor.z;
			
			transparencyDepth = oceanNode.transparencyDepth;
			darknessDepth = oceanNode.darknessDepth;
			refractionIndex = oceanNode.refractionIndex;
			shoreFoam = oceanNode.shoreFoam;

			AMP = oceanNode.AMP;
			
			m_windSpeed = oceanNode.m_windSpeed;
			m_omega = oceanNode.m_omega;
			
			m_gridSizes = oceanNode.m_gridSizes;
			m_choppyness = oceanNode.m_choppyness;
			
			m_ansio = oceanNode.m_ansio;
			
			m_varianceSize = oceanNode.m_varianceSize;
			m_foamAnsio = oceanNode.m_foamAnsio;
			m_foamMipMapBias = oceanNode.m_foamMipMapBias;
			m_whiteCapStr = oceanNode.m_whiteCapStr;
			farWhiteCapStr = oceanNode.m_farWhiteCapStr;
			
			m_resolution = oceanNode.m_resolution;
		}

		
		public void loadConfigPoint (int point)
		{
			ConfigPoint _cur = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point];
			
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

