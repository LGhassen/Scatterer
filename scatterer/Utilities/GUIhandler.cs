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
		int windowId = UnityEngine.Random.Range(int.MinValue,int.MaxValue);

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
		//float cloudExtinctionMultiplier = 1f;
		float volumetricsColorMultiplier=1f;
		//		float volumetricsScatteringMultiplier=1f;
		//		float volumetricsSkyIrradianceMultiplier = 1f;
		float mieG = 0.85f;
		float openglThreshold = 10f;
		float _GlobalOceanAlpha = 1f;
		
		float edgeThreshold = 1f;
		
		float extinctionMultiplier = 1f;
		float extinctionTint = 1f;
		float skyExtinctionRimFade=0f;
		float skyExtinctionGroundFade=0f;
		
		float _extinctionScatterIntensity=1f;
		float _mapExtinctionScatterIntensity=1f;
		
		float mapExtinctionMultiplier = 1f;
		float mapExtinctionTint = 1f;
		float mapSkyExtinctionRimFade=1f;
		float specR = 0f, specG = 0f, specB = 0f, shininess = 0f;
		
		//ConfigPoint variables 		
		float pointAltitude = 0f;
		float newCfgPtAlt = 0f;
		int configPointsCnt;
		bool showInterpolatedValues = false;
		float postProcessingalpha = 78f;
		float postProcessDepth = 200f;
		
		float _Post_Extinction_Tint=100f;
		float postExtinctionMultiplier=100f;
		
		float postProcessExposure = 18f;
		
		//sky properties
		float exposure = 25f;
		float skyRimExposure = 25f;
		float alphaGlobal = 100f;
		float mapExposure = 15f;
		float mapSkyRimeExposure = 15f;
		float mapAlphaGlobal = 100f;

		float oceanLevel = 0f;
		float oceanAlpha = 1f;
		float oceanAlphaRadius = 3000f;
//		float oceanScale = 1f;
//		float WAVE_CM = 0.23f;
//		float WAVE_KM = 370.0f;
		float AMP = 1.0f;
		float m_windSpeed = 5.0f; //A higher wind speed gives greater swell to the waves
		float m_omega = 0.84f; //A lower number means the waves last longer and will build up larger waves
		
		int m_ansio = 2;
		int m_varianceSize = 4;
		int m_foamAnsio = 9;
		float m_foamMipMapBias = -2.0f;
		float m_whiteCapStr = 0.1f;
		float farWhiteCapStr = 0.1f;
		float choppynessMultiplier = 1f;
		
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
		//		After all it's a basic UI for tweaking settings and it does it's job
		public void DrawScattererWindow (int windowId)
		{
			GUItoggle("Hide",ref Core.Instance.visible);
			
			if (Core.Instance.mainMenu)  //MAIN MENU options
			{ 
				GUILayout.Label (String.Format ("Scatterer: features selector"));
				Core.Instance.useOceanShaders = GUILayout.Toggle(Core.Instance.useOceanShaders, "Ocean shaders (may require game restart on change)");
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Ocean: fourierGridSize (64:fast,128:normal,256:HQ)");
				Core.Instance.m_fourierGridSize = (Int32)(Convert.ToInt32 (GUILayout.TextField (Core.Instance.m_fourierGridSize.ToString ())));
				GUILayout.EndHorizontal ();
				
				Core.Instance.oceanSkyReflections = GUILayout.Toggle(Core.Instance.oceanSkyReflections, "Ocean: accurate sky reflection");
				Core.Instance.oceanRefraction = GUILayout.Toggle(Core.Instance.oceanRefraction, "Ocean: refraction effects");
				Core.Instance.oceanPixelLights = GUILayout.Toggle(Core.Instance.oceanPixelLights, "Ocean: lights compatibility (huge performance hit when lights on)");
				
				//Core.Instance.usePlanetShine = GUILayout.Toggle(Core.Instance.usePlanetShine, "PlanetShine");
				Core.Instance.integrateWithEVEClouds = GUILayout.Toggle(Core.Instance.integrateWithEVEClouds, "Integrate effects with EVE clouds (may require restart)");
				
				Core.Instance.drawAtmoOnTopOfClouds= GUILayout.Toggle(Core.Instance.drawAtmoOnTopOfClouds, "Draw atmo on top of EVE clouds(old cloud shading, use with EVE 7-4)");
				
				Core.Instance.fullLensFlareReplacement=GUILayout.Toggle(Core.Instance.fullLensFlareReplacement, "Lens flare shader");
				Core.Instance.useEclipses = GUILayout.Toggle(Core.Instance.useEclipses, "Eclipses (WIP, sky/orbit only for now)");
				Core.Instance.useRingShadows = GUILayout.Toggle(Core.Instance.useRingShadows, "Kopernicus ring shadows");
				Core.Instance.useGodrays = GUILayout.Toggle(Core.Instance.useGodrays, "Godrays (early WIP)");
				
				Core.Instance.terrainShadows = GUILayout.Toggle(Core.Instance.terrainShadows, "Terrain shadows");
				GUILayout.BeginHorizontal ();
				
				GUILayout.Label ("Shadow bias");
				Core.Instance.shadowBias = float.Parse (GUILayout.TextField (Core.Instance.shadowBias.ToString ("0.000")));
				
				GUILayout.Label ("Shadow normal bias");
				Core.Instance.shadowNormalBias = float.Parse (GUILayout.TextField (Core.Instance.shadowNormalBias.ToString ("0.000")));
				
				GUILayout.EndHorizontal ();
				
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Menu scroll section height");
				Core.Instance.scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (Core.Instance.scrollSectionHeight.ToString ())));
				GUILayout.EndHorizontal ();
				
				Core.Instance.disableAmbientLight = GUILayout.Toggle(Core.Instance.disableAmbientLight, "Disable scaled space ambient light");
				
				Core.Instance.showMenuOnStart = GUILayout.Toggle(Core.Instance.showMenuOnStart, "Show this menu on start-up");
				
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
						if (!MapView.MapIsEnabled)
						{
							GUILayout.BeginHorizontal ();
							GUILayout.Label ("New point altitude:");
							newCfgPtAlt = Convert.ToSingle (GUILayout.TextField (newCfgPtAlt.ToString ()));
							if (GUILayout.Button ("Add"))
							{
								Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints.Insert (selectedConfigPoint + 1,
								                                                                                   new ConfigPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100, skyRimExposure/100,
								                 postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100,
								                 extinctionMultiplier / 100, extinctionTint / 100, skyExtinctionRimFade/100, skyExtinctionGroundFade/100,
								                 openglThreshold, edgeThreshold / 100,viewdirOffset,_Post_Extinction_Tint/100,
								                 postExtinctionMultiplier/100, _GlobalOceanAlpha/100, _extinctionScatterIntensity/100));
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
							
							
							GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
							GUIfloat("AtmosphereGlobalScale", ref atmosphereGlobalScale, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);
							GUIfloat("experimentalViewDirOffset", ref viewdirOffset,ref _cur.viewdirOffset);
							
							
							//								GUILayout.Label("Sky/Orbit shader");
							
							GUIfloat("Sky/orbit Alpha", ref alphaGlobal, ref _cur.skyAlpha);
							GUIfloat("Sky/orbit Exposure", ref exposure, ref _cur.skyExposure);
							//								tonemapper.setExposure(exposure);
							GUIfloat ("Sky/orbit Rim Exposure", ref skyRimExposure, ref _cur.skyRimExposure);
							
							GUIfloat("extinctionMultiplier", ref extinctionMultiplier, ref _cur.skyExtinctionMultiplier);
							GUIfloat("extinctionTint", ref extinctionTint, ref _cur.skyExtinctionTint);
							GUIfloat("extinctionRimFade", ref skyExtinctionRimFade ,ref  _cur.skyextinctionRimFade);
							GUIfloat("extinctionGroundFade", ref skyExtinctionGroundFade, ref _cur.skyextinctionGroundFade);
							GUIfloat("extinctionScatterIntensity", ref _extinctionScatterIntensity, ref _cur._extinctionScatterIntensity);
							
							//								GUILayout.Label("Post-processing shader");
							
							GUIfloat("Post Processing Alpha", ref postProcessingalpha, ref _cur.postProcessAlpha);
							GUIfloat("Post Processing Depth", ref postProcessDepth,ref _cur.postProcessDepth);
							GUIfloat("Post Processing Extinction Multiplier", ref postExtinctionMultiplier, ref _cur.postExtinctionMultiplier);
							GUIfloat("Post Processing Extinction Tint", ref _Post_Extinction_Tint, ref _cur._Post_Extinction_Tint);
							GUIfloat("Post Processing Exposure", ref postProcessExposure ,ref _cur.postProcessExposure);
							
							//								if (!d3d9)
							{
								GUIfloat("Depth buffer Threshold", ref openglThreshold, ref _cur.openglThreshold);
							}
							
							GUIfloat("_GlobalOceanAlpha", ref _GlobalOceanAlpha, ref _cur._GlobalOceanAlpha);
							
							
						} 
						
						else
						{
							_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (Core.Instance.scrollSectionHeight));
							
							GUIfloat("experimentalAtmoScale", ref experimentalAtmoScale,ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.experimentalAtmoScale);
							GUIfloat("AtmosphereGlobalScale", ref atmosphereGlobalScale, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.atmosphereGlobalScale);
							
							GUIfloat("Map view alpha", ref mapAlphaGlobal, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapAlphaGlobal);
							GUIfloat("Map view exposure", ref mapExposure, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExposure);
							GUIfloat("Map view rim exposure", ref mapSkyRimeExposure, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyRimExposure);
							
							GUIfloat("MapExtinctionMultiplier", ref mapExtinctionMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionMultiplier);
							GUIfloat("MapExtinctionTint", ref mapExtinctionTint, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapExtinctionTint);
							GUIfloat("MapExtinctionRimFade", ref mapSkyExtinctionRimFade, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.mapSkyExtinctionRimFade);
							GUIfloat("MapExtinctionScatterIntensity", ref _mapExtinctionScatterIntensity, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode._mapExtinctionScatterIntensity);
						}
						
						
						GUIfloat("mieG", ref mieG, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.m_mieG);
						
						if (Core.Instance.integrateWithEVEClouds && Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.usesCloudIntegration)
						{
							GUIfloat("Cloud Color Multiplier", ref cloudColorMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudColorMultiplier);
							GUIfloat("Cloud Scattering Multiplier", ref cloudScatteringMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudScatteringMultiplier);
							GUIfloat("Cloud Sky irradiance Multiplier", ref cloudSkyIrradianceMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.cloudSkyIrradianceMultiplier);
							
							GUIfloat("Volumetrics Color Multiplier", ref volumetricsColorMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsColorMultiplier);

							GUILayout.BeginHorizontal ();
							GUILayout.Label ("Preserve cloud colors ");
							GUILayout.TextField ( Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.EVEIntegration_preserveCloudColors.ToString ());
							if (GUILayout.Button ("Toggle"))
								Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.togglePreserveCloudColors();
							GUILayout.EndHorizontal ();

							//								GUIfloat("Volumetrics Scattering Multiplier", ref volumetricsScatteringMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsScatteringMultiplier);
							//								GUIfloat("Volumetrics Sky irradiance Multiplier", ref volumetricsSkyIrradianceMultiplier, ref Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.volumetricsSkyIrradianceMultiplier);
						}
						
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("RimBlend");
						rimBlend = Convert.ToSingle (GUILayout.TextField (rimBlend.ToString ()));
						
						GUILayout.Label ("RimPower");
						rimpower = Convert.ToSingle (GUILayout.TextField (rimpower.ToString ()));
						
						if (GUILayout.Button ("Set")) {
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimBlend = rimBlend;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.rimpower = rimpower;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
						}
						GUILayout.EndHorizontal ();
						
						GUILayout.BeginHorizontal ();
						GUILayout.Label ("Spec: R");
						specR = (float)(Convert.ToDouble (GUILayout.TextField (specR.ToString ())));
						
						GUILayout.Label ("G");
						specG = (float)(Convert.ToDouble (GUILayout.TextField (specG.ToString ())));
						
						GUILayout.Label ("B");
						specB = (float)(Convert.ToDouble (GUILayout.TextField (specB.ToString ())));
						
						GUILayout.Label ("shine");
						shininess = (float)(Convert.ToDouble (GUILayout.TextField (shininess.ToString ())));
						
						if (GUILayout.Button ("Set")) {
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specR = specR;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specG = specG;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.specB = specB;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.shininess = shininess;
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.tweakStockAtmosphere ();
						}
						GUILayout.EndHorizontal ();
						
						if (!MapView.MapIsEnabled)
						{
							GUILayout.BeginHorizontal ();
							if (Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint == 0)
								GUILayout.Label ("Current state:Ground, cfgPoint 0");
							else if (Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint >= configPointsCnt)
								GUILayout.Label (String.Format ("Current state:Orbit, cfgPoint{0}", Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1));
							else
								GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage)), Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint - 1, (int)(100 * Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.percentage), Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.currentConfigPoint));
							GUILayout.EndHorizontal ();
						}
						GUILayout.EndScrollView ();
						
						GUILayout.BeginHorizontal ();
						GUItoggle("Toggle depth buffer", ref Core.Instance.depthbufferEnabled);
						
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
							Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.displayInterpolatedVariables = showInterpolatedValues;
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
									if (!_cel.m_manager.m_skyNode.inScaledSpace)
										_cel.m_manager.m_skyNode.mapEVEvolumetrics();
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
							GUIfloat ("ocean Level", ref oceanLevel, ref oceanNode.m_oceanLevel);
							GUIfloat ("Alpha/WhiteCap Radius", ref oceanAlphaRadius, ref oceanNode.alphaRadius);
							GUIfloat ("ocean Alpha", ref oceanAlpha, ref oceanNode.oceanAlpha);
							
							GUIfloat ("Transparency Depth", ref transparencyDepth, ref oceanNode.transparencyDepth);
							GUIfloat ("Darkness Depth", ref darknessDepth, ref oceanNode.darknessDepth);
							GUIfloat ("Refraction Index", ref refractionIndex, ref oceanNode.refractionIndex);
							GUIfloat ("Shore foam", ref shoreFoam, ref oceanNode.shoreFoam);
							
							GUIfloat ("whiteCapStr (foam)", ref m_whiteCapStr, ref oceanNode.m_whiteCapStr);
							GUIfloat ("far whiteCapStr", ref farWhiteCapStr, ref oceanNode.m_farWhiteCapStr);
							GUIfloat ("choppyness multiplier", ref choppynessMultiplier, ref oceanNode.choppynessMultiplier);
							
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
							GUILayout.Label ("current fourierGridSize: "+Core.Instance.m_fourierGridSize.ToString());
							GUILayout.EndHorizontal ();
							
							//GUIint("Ocean renderqueue", ref oceanRenderQueue, ref oceanRenderQueue,1);
						}	
						GUILayout.EndScrollView ();
						
						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Rebuild ocean")) {
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
							if (Core.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)))
								Component.Destroy(Core.Instance.nearCamera.gameObject.GetComponent (typeof(Wireframe)));
							
							if (Core.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)))
								Component.Destroy(Core.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)));
							
							if (Core.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
								Component.Destroy(Core.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
							
							wireFrame=false;
						}
						
						else
						{
							Core.Instance.nearCamera.gameObject.AddComponent (typeof(Wireframe));
							Core.Instance.farCamera.gameObject.AddComponent (typeof(Wireframe));
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
			
			local = float.Parse (GUILayout.TextField (local.ToString ("00000.0000")));
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
			//			postProcessDepth = 10000 * selected.postProcessDepth;
			postProcessDepth = selected.postProcessDepth;
			
			_Post_Extinction_Tint = selected._Post_Extinction_Tint;
			postExtinctionMultiplier = selected.postExtinctionMultiplier;
			
			postProcessExposure = selected.postProcessExposure;
			exposure = selected.skyExposure;
			skyRimExposure = selected.skyRimExposure;
			alphaGlobal = selected.skyAlpha;
			
			openglThreshold = selected.openglThreshold;
			
			_GlobalOceanAlpha = selected._GlobalOceanAlpha;
			//			edgeThreshold = selected.edgeThreshold * 100;
			
			
			mapAlphaGlobal = skyNode.mapAlphaGlobal;
			mapExposure = skyNode.mapExposure;
			mapSkyRimeExposure = skyNode.mapSkyRimExposure;
			configPointsCnt = skyNode.configPoints.Count;
			
			specR = skyNode.specR;
			specG = skyNode.specG;
			specB = skyNode.specB;
			shininess = skyNode.shininess;
			
			
			rimBlend = skyNode.rimBlend;
			rimpower = skyNode.rimpower;
			
			//MapViewScale = skyNode.MapViewScale;
			extinctionMultiplier = selected.skyExtinctionMultiplier;
			extinctionTint = selected.skyExtinctionTint;
			skyExtinctionRimFade = selected.skyextinctionRimFade;
			skyExtinctionGroundFade = selected.skyextinctionGroundFade;
			_extinctionScatterIntensity = selected._extinctionScatterIntensity;
			
			mapExtinctionMultiplier = skyNode.mapExtinctionMultiplier;
			mapExtinctionTint = skyNode.mapExtinctionTint;
			mapSkyExtinctionRimFade= skyNode.mapSkyExtinctionRimFade;
			_mapExtinctionScatterIntensity = skyNode._mapExtinctionScatterIntensity;
			
			showInterpolatedValues = skyNode.displayInterpolatedVariables;
			
			mieG = skyNode.m_mieG;
			
			experimentalAtmoScale = skyNode.experimentalAtmoScale;
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
			
			oceanLevel = oceanNode.m_oceanLevel;
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
			
			//oceanScale = oceanNode.oceanScale;
			
			choppynessMultiplier = oceanNode.choppynessMultiplier;
			
//			WAVE_CM = oceanNode.WAVE_CM;
//			WAVE_KM = oceanNode.WAVE_KM;
			AMP = oceanNode.AMP;
			
			m_windSpeed = oceanNode.m_windSpeed;
			m_omega = oceanNode.m_omega;
			
			m_gridSizes = oceanNode.m_gridSizes;
			m_choppyness = oceanNode.m_choppyness;
			//			m_fourierGridSize = oceanNode.m_fourierGridSize;
			
			m_ansio = oceanNode.m_ansio;
			
			m_varianceSize = oceanNode.m_varianceSize;
			m_foamAnsio = oceanNode.m_foamAnsio;
			m_foamMipMapBias = oceanNode.m_foamMipMapBias;
			m_whiteCapStr = oceanNode.m_whiteCapStr;
			farWhiteCapStr = oceanNode.m_farWhiteCapStr;
			
			m_resolution = oceanNode.m_resolution;
			//			m_fourierGridSize = oceanNode.m_fourierGridSize;
			
		}
		
		
		
		public void loadConfigPoint (int point)
		{
			ConfigPoint _cur = Core.Instance.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configPoints [point];
			
			postProcessDepth = _cur.postProcessDepth;
			_Post_Extinction_Tint = _cur._Post_Extinction_Tint;
			postExtinctionMultiplier = _cur.postExtinctionMultiplier;
			postProcessExposure = _cur.postProcessExposure;
			postProcessingalpha = _cur.postProcessAlpha;
			
			alphaGlobal = _cur.skyAlpha;
			exposure = _cur.skyExposure;
			skyRimExposure = _cur.skyRimExposure;
			extinctionMultiplier = _cur.skyExtinctionMultiplier;
			extinctionTint = _cur.skyExtinctionTint;
			skyExtinctionRimFade = _cur.skyextinctionRimFade;
			skyExtinctionGroundFade = _cur.skyextinctionGroundFade;
			_extinctionScatterIntensity = _cur._extinctionScatterIntensity;
			
			pointAltitude = _cur.altitude;
			
			openglThreshold = _cur.openglThreshold;
			_GlobalOceanAlpha = _cur._GlobalOceanAlpha;
			viewdirOffset = _cur.viewdirOffset;
		}


	}
}

