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
		public bool sunflareOptions=false;

		enum MainMenuTabs
		{
			QualityPresets,
			IndividualSettings
		}

		enum IndividualSettingsTabs
		{
			Scattering,
			Ocean,
			Sunflare,
			Lighting,
			Shadows,
			EVEintegration,
			Misc
		}

		MainMenuTabs selectedMainMenuTab = MainMenuTabs.QualityPresets;
		IndividualSettingsTabs selectedIndividualSettingsTab = IndividualSettingsTabs.Scattering;

		public int selectedPlanet = 0;
		public int selectedConfigPoint = 0;
		bool wireFrame = false;

		private Vector2 _scroll;
		private Vector2 _scroll2;
		public bool displayOceanSettings = false;

		Vector3 sunColor=Vector3.one;
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

		float godrayStrength = 1.0f;
//		float godrayCloudAlphaThreshold = 0.1f;

		float extinctionThickness = 1f;
		float skyExtinctionTint = 1f;
		
		float specR = 0f, specG = 0f, specB = 0f, shininess = 0f, flattenScaledSpaceMesh = 0f;
		
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

		//sunflare stuff
		String[] sunflareStrings;

		int selSunflareGridInt = 0;
		bool editingSunflare = false;
		string sunflareText = "";
		private Vector2 sunflareScrollPosition = new Vector2();
		
		String[] qualityPresetsStrings;
		string currentPreset;
		int selQualityPresetInt = 0;

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
				//button planet settings or sunflares?
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Planet settings"))
				{
					sunflareOptions = false;
				}
				if (Scatterer.Instance.mainSettings.fullLensFlareReplacement && !ReferenceEquals(Scatterer.Instance.sunflareManager,null) && !ReferenceEquals(Scatterer.Instance.sunflareManager.scattererSunFlares,null))
				{
					if (GUILayout.Button ("Sunflare settings"))
					{
						sunflareStrings = new string[Scatterer.Instance.sunflareManager.scattererSunFlares.Count];
						
						for (int i=0; i<Scatterer.Instance.sunflareManager.scattererSunFlares.Count; i++)
						{
							sunflareStrings[i] = Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(i).Value.sourceName;
						}
						
						sunflareOptions = true;
					}
				}
				GUILayout.EndHorizontal ();

				if (!sunflareOptions)
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
					GUILayout.BeginVertical ();
					selSunflareGridInt = GUILayout.SelectionGrid (selSunflareGridInt, sunflareStrings, 1);
					if (GUILayout.Button ("Edit Selected"))
					{
						sunflareText = string.Copy(Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.configNodeToLoad.ToString());
						editingSunflare = true;
					}
					GUILayout.EndVertical ();
					
					if (editingSunflare)
					{
						sunflareScrollPosition = GUILayout.BeginScrollView(sunflareScrollPosition, false, true, GUILayout.Width(800) ,GUILayout.MinHeight(Scatterer.Instance.pluginData.scrollSectionHeight + 100));
						sunflareText = GUILayout.TextArea(sunflareText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
						GUILayout.EndScrollView();
						
						GUILayout.BeginHorizontal ();
						
						if (GUILayout.Button("Reimport"))
						{
							sunflareText = string.Copy(Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.configNodeToLoad.ToString());
						}
						
						if (GUILayout.Button("Apply"))
						{
							ConfigNode node = ConfigNode.Parse(sunflareText);
							Utils.LogInfo("Applying sunflare config from UI:\r\n"+sunflareText);
							Scatterer.Instance.sunflareManager.scattererSunFlares.ElementAt(selSunflareGridInt).Value.ApplyFromUI(node.GetNode("Sun"));

						}
						
						if (GUILayout.Button("Copy to clipboard"))
						{
							GUIUtility.systemCopyBuffer = sunflareText;
						}
						
						if (GUILayout.Button ("Print to Log"))
						{
							Utils.LogInfo("Sunflare config print to log:\r\n"+sunflareText);
						}
						
						GUILayout.EndHorizontal ();
					}
				}
			}
			else
			{
				GUILayout.Label (String.Format ("Inactive in tracking station and VAB/SPH"));
				GUILayout.EndHorizontal ();
			}


			GUI.DragWindow();
		}

		void DrawOptionsMenu ()
		{
			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label ("Menu scroll section height");
				Scatterer.Instance.pluginData.scrollSectionHeight = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.pluginData.scrollSectionHeight.ToString ())));
			}
			GUILayout.EndHorizontal ();
			
			GUILayout.BeginHorizontal ();
			{
				GUILayout.Label (".cfg file used (display only):");
				GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.baseConfigs [0].parent.url);
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			{
				if (GUILayout.Button ("Quality Presets"))
				{
					selectedMainMenuTab = MainMenuTabs.QualityPresets;
				}
				if (GUILayout.Button ("Customize Settings"))
				{
					selectedMainMenuTab = MainMenuTabs.IndividualSettings;
				}
			}
			GUILayout.EndHorizontal ();

			if (selectedMainMenuTab == MainMenuTabs.QualityPresets)
			{
				DrawQualityPresets();
			}
			else
			{
				DrawIndividualSettings ();
			}
		}

		void DrawQualityPresets ()
		{
			if (ReferenceEquals (qualityPresetsStrings, null))
			{
				qualityPresetsStrings = QualityPresetsLoader.GetPresetsList ();
				currentPreset = QualityPresetsLoader.FindPresetOfCurrentSettings(Scatterer.Instance.mainSettings);

				int index = qualityPresetsStrings.IndexOf(currentPreset);

				if (index != -1)
				{
					selQualityPresetInt = index;
				}
			}
			else
			{
				GUILayout.BeginVertical ();
				GUILayout.BeginHorizontal ();
				{
					GUILayout.Label("Current preset:");
					GUILayout.TextField(currentPreset);
				}
				GUILayout.EndHorizontal ();
				selQualityPresetInt = GUILayout.SelectionGrid (selQualityPresetInt, qualityPresetsStrings, 1);
				GUILayout.Label("");
				if (GUILayout.Button ("Apply preset"))
				{
					if (qualityPresetsStrings.Count() > 0)
					{
						Utils.LogInfo("Applying quality preset "+qualityPresetsStrings[selQualityPresetInt]);
						QualityPresetsLoader.LoadPresetIntoMainSettings(Scatterer.Instance.mainSettings, qualityPresetsStrings[selQualityPresetInt]);
						currentPreset = qualityPresetsStrings[selQualityPresetInt];
					}
				}
				GUILayout.EndVertical ();
			}
		}

		void DrawIndividualSettings ()
		{
			GUILayout.BeginHorizontal ();
			{
				if (GUILayout.Button ("Scattering")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Scattering;
				}
				if (GUILayout.Button ("Ocean")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Ocean;
				}
				if (GUILayout.Button ("Sunflare")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Sunflare;
				}
				if (GUILayout.Button ("Lighting")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Lighting;
				}
				if (GUILayout.Button ("Shadows")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Shadows;
				}
				if (GUILayout.Button ("EVE Clouds")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.EVEintegration;
				}
				if (GUILayout.Button ("Misc.")) {
					selectedIndividualSettingsTab = IndividualSettingsTabs.Misc;
				}
			}
			GUILayout.EndHorizontal ();
			if (selectedIndividualSettingsTab == IndividualSettingsTabs.Scattering) {
				Scatterer.Instance.mainSettings.useGodrays = GUILayout.Toggle (Scatterer.Instance.mainSettings.useGodrays, "Godrays (Requires unified camera, long-distance shadows and shadowMapResolution override, Directx11 only)");
				if (Scatterer.Instance.mainSettings.useGodrays)
				{
					//Godrays tesselation placeholder
				}
				Scatterer.Instance.mainSettings.useDepthBufferMode = !GUILayout.Toggle (!Scatterer.Instance.mainSettings.useDepthBufferMode, "Use projector mode (Slower, less compatible but supports MSAA)");
				Scatterer.Instance.mainSettings.useDepthBufferMode = GUILayout.Toggle (Scatterer.Instance.mainSettings.useDepthBufferMode, "Use depth buffer mode (Recommended: Faster, better compatible with Parallax and trees/scatters, disables MSAA in flight/KSC)");
				if (Scatterer.Instance.mainSettings.useDepthBufferMode)
				{
					GUILayout.BeginHorizontal ();
					{
						GUILayout.Label ("\t");
						GUILayout.BeginVertical ();
						{
							Scatterer.Instance.mainSettings.quarterResScattering = GUILayout.Toggle (Scatterer.Instance.mainSettings.quarterResScattering, "Render scattering in 1/4 resolution (speedup, incompatible and disabled with godrays)");
							Scatterer.Instance.mainSettings.mergeDepthPrePass = GUILayout.Toggle (Scatterer.Instance.mainSettings.mergeDepthPrePass, "Merge depth pre-pass into main depth for culling (experimental, may give small speedup but may cause z-fighting");
							Scatterer.Instance.mainSettings.useSubpixelMorphologicalAntialiasing = GUILayout.Toggle (Scatterer.Instance.mainSettings.useSubpixelMorphologicalAntialiasing, "Subpixel Morphological Antialiasing (SMAA, recommended)")  && !Scatterer.Instance.mainSettings.useTemporalAntiAliasing;
							if (Scatterer.Instance.mainSettings.useSubpixelMorphologicalAntialiasing)
							{
								GUILayout.BeginHorizontal ();
								{
									GUILayout.Label ("SMAA quality (1:normal,2:high)");
									Scatterer.Instance.mainSettings.smaaQuality = (Int32) Mathf.Clamp( (float)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.smaaQuality.ToString ()))),1f,2f);
								}
								GUILayout.EndHorizontal ();
							}

							Scatterer.Instance.mainSettings.useTemporalAntiAliasing = GUILayout.Toggle (Scatterer.Instance.mainSettings.useTemporalAntiAliasing, "Temporal Antialiasing (Not recommended, causes shadow flickering from some angles)") && !Scatterer.Instance.mainSettings.useSubpixelMorphologicalAntialiasing;
						}
						GUILayout.EndVertical ();
					}
					GUILayout.EndHorizontal ();
				}
			}
			else
				if (selectedIndividualSettingsTab == IndividualSettingsTabs.Ocean) {
					Scatterer.Instance.mainSettings.useOceanShaders = GUILayout.Toggle (Scatterer.Instance.mainSettings.useOceanShaders, "Ocean shaders");
					if (Scatterer.Instance.mainSettings.useOceanShaders) {
						GUILayout.BeginHorizontal ();
						{
							GUILayout.Label ("\t");
							GUILayout.BeginVertical ();
							{
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Fourier grid size (64:fast,128:normal,256:HQ)");
								Scatterer.Instance.mainSettings.m_fourierGridSize = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.m_fourierGridSize.ToString ())));
								GUILayout.EndHorizontal ();
								GUILayout.BeginHorizontal ();
								GUILayout.Label ("Mesh resolution (pixels covered by a mesh quad, lower is better but slower)");
								Scatterer.Instance.mainSettings.oceanMeshResolution = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.oceanMeshResolution.ToString ())));
								GUILayout.EndHorizontal ();
								Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions, "Transparency and refractions");
								Scatterer.Instance.mainSettings.oceanFoam = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanFoam, "Foam");
								Scatterer.Instance.mainSettings.oceanSkyReflections = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanSkyReflections, "Sky reflections");
								Scatterer.Instance.mainSettings.shadowsOnOcean = GUILayout.Toggle (Scatterer.Instance.mainSettings.shadowsOnOcean, "Surface receives shadows");
								Scatterer.Instance.mainSettings.oceanPixelLights = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanPixelLights, "Secondary lights compatibility (huge performance hit when lights on)");
								Scatterer.Instance.mainSettings.oceanCaustics = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCaustics, "Underwater caustics");
								Scatterer.Instance.mainSettings.oceanLightRays = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanLightRays, "Underwater light rays (requires ocean surface shadows)");
								GUI.contentColor = SystemInfo.supportsAsyncGPUReadback && SystemInfo.supportsComputeShaders ? Color.white : Color.gray;
								Scatterer.Instance.mainSettings.oceanCraftWaveInteractions = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCraftWaveInteractions, "Waves interact with ships (Requires asyncGPU readback, Directx11 only)");
								if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractions) {
									GUILayout.BeginHorizontal ();
									{
										GUILayout.Label ("\t");
										GUILayout.BeginVertical ();
										{
											Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideWaterCrashTolerance = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideWaterCrashTolerance, "Override water crash tolerance");
											if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideWaterCrashTolerance) {
												GUILayout.BeginHorizontal ();
												GUILayout.Label ("Crash tolerance (default is 1.2)");
												Scatterer.Instance.mainSettings.buoyancyCrashToleranceMultOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.buoyancyCrashToleranceMultOverride.ToString ("00.00")));
												GUILayout.EndHorizontal ();
											}
											Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideDrag = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideDrag, "Override water drag");
											if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideDrag) {
												GUILayout.BeginHorizontal ();
												GUILayout.Label ("Drag scalar (default is 4.5)");
												Scatterer.Instance.mainSettings.buoyancyWaterDragScalarOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.buoyancyWaterDragScalarOverride.ToString ("00.00")));
												GUILayout.EndHorizontal ();
												GUILayout.BeginHorizontal ();
												GUILayout.Label ("Angular drag scalar (default is 0.001");
												Scatterer.Instance.mainSettings.buoyancyWaterAngularDragScalarOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.buoyancyWaterAngularDragScalarOverride.ToString ("0.0000000")));
												GUILayout.EndHorizontal ();
											}
											Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideRecoveryVelocity = GUILayout.Toggle (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideRecoveryVelocity, "Override max water recovery velocity");
											if (Scatterer.Instance.mainSettings.oceanCraftWaveInteractionsOverrideRecoveryVelocity) {
												GUILayout.BeginHorizontal ();
												GUILayout.Label ("Maximum recovery velocity (default is 0.3)");
												Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.waterMaxRecoveryVelocity.ToString ("00.00")));
												GUILayout.EndHorizontal ();
											}
										}
										GUILayout.EndVertical ();
									}
									GUILayout.EndHorizontal ();
								}
							}
							GUILayout.EndVertical ();
						}
						GUILayout.EndHorizontal ();
					}
				}
				else
					if (selectedIndividualSettingsTab == IndividualSettingsTabs.Lighting) {
						Scatterer.Instance.mainSettings.useEclipses = GUILayout.Toggle (Scatterer.Instance.mainSettings.useEclipses, "Eclipses (WIP, sky/orbit only for now)");
						Scatterer.Instance.mainSettings.useRingShadows = GUILayout.Toggle (Scatterer.Instance.mainSettings.useRingShadows, "Kopernicus ring shadows (linear only, tiled rings not supported)");
						Scatterer.Instance.mainSettings.disableAmbientLight = GUILayout.Toggle (Scatterer.Instance.mainSettings.disableAmbientLight, "Disable scaled space ambient light");
						Scatterer.Instance.mainSettings.sunlightExtinction = GUILayout.Toggle (Scatterer.Instance.mainSettings.sunlightExtinction, "Sunlight extinction (direct sun light changes color with sunset/dusk)");
						Scatterer.Instance.mainSettings.underwaterLightDimming = GUILayout.Toggle (Scatterer.Instance.mainSettings.underwaterLightDimming, "Dim light underwater");
					}
					else
						if (selectedIndividualSettingsTab == IndividualSettingsTabs.Shadows) {
							Scatterer.Instance.mainSettings.d3d11ShadowFix = GUILayout.Toggle (Scatterer.Instance.mainSettings.d3d11ShadowFix, "1.9+ Directx11 flickering shadows fix (recommended for 1.9, 1.10)");
							Scatterer.Instance.mainSettings.terrainShadows = GUILayout.Toggle (Scatterer.Instance.mainSettings.terrainShadows, "Long-Distance Terrain shadows");
							if (Scatterer.Instance.mainSettings.terrainShadows) {
								GUILayout.BeginHorizontal ();
								{
									GUILayout.Label ("  ");
									GUILayout.BeginVertical ();
									{
										GUI.contentColor = Scatterer.Instance.unifiedCameraMode ? Color.white : Color.gray;
										GUILayout.Label ((Scatterer.Instance.unifiedCameraMode ? "[Active] " : "[Inactive] ") + "Unified camera mode (1.9+ Directx11):");
										GUILayout.BeginHorizontal ();
										{
											GUILayout.Label ("\t");
											GUILayout.BeginVertical ();
											{
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadows Distance (in meters):");
													Scatterer.Instance.mainSettings.unifiedCamShadowsDistance = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.unifiedCamShadowsDistance.ToString ("0")));
												}
												GUILayout.EndHorizontal ();
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadowmap resolution (power of 2, zero for no override):");
													Scatterer.Instance.mainSettings.unifiedCamShadowResolutionOverride = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.unifiedCamShadowResolutionOverride.ToString ())));
												}
												GUILayout.EndHorizontal ();
												GUIvector3NoButton ("Shadow cascade splits (zeroes for no override):", ref Scatterer.Instance.mainSettings.unifiedCamShadowCascadeSplitsOverride);
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadow bias (0 for no override)");
													Scatterer.Instance.mainSettings.unifiedCamShadowBiasOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.unifiedCamShadowBiasOverride.ToString ("0.000")));
													GUILayout.Label ("Normal bias (0 for no override)");
													Scatterer.Instance.mainSettings.unifiedCamShadowNormalBiasOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.unifiedCamShadowNormalBiasOverride.ToString ("0.000")));
												}
												GUILayout.EndHorizontal ();
											}
											GUILayout.EndVertical ();
										}
										GUILayout.EndHorizontal ();
										GUI.contentColor = !Scatterer.Instance.unifiedCameraMode ? Color.white : Color.gray;
										GUILayout.Label ((!Scatterer.Instance.unifiedCameraMode ? "[Active] " : "[Inactive] ") + "Dual camera mode (1.8, 1.9 and 1.10 Opengl):");
										GUILayout.BeginHorizontal ();
										{
											GUILayout.Label ("\t");
											GUILayout.BeginVertical ();
											{
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadows Distance (in meters):");
													Scatterer.Instance.mainSettings.dualCamShadowsDistance = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.dualCamShadowsDistance.ToString ("0")));
												}
												GUILayout.EndHorizontal ();
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadowmap resolution (power of 2, zero for no override):");
													Scatterer.Instance.mainSettings.dualCamShadowResolutionOverride = (Int32)(Convert.ToInt32 (GUILayout.TextField (Scatterer.Instance.mainSettings.dualCamShadowResolutionOverride.ToString ())));
												}
												GUILayout.EndHorizontal ();
												GUIvector3NoButton ("Shadow cascade splits (zeroes for no override):", ref Scatterer.Instance.mainSettings.dualCamShadowCascadeSplitsOverride);
												GUILayout.BeginHorizontal ();
												{
													GUILayout.Label ("Shadow bias (0 for no override)");
													Scatterer.Instance.mainSettings.dualCamShadowBiasOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.dualCamShadowBiasOverride.ToString ("0.000")));
													GUILayout.Label ("Normal bias (0 for no override)");
													Scatterer.Instance.mainSettings.dualCamShadowNormalBiasOverride = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.dualCamShadowNormalBiasOverride.ToString ("0.000")));
												}
												GUILayout.EndHorizontal ();
											}
											GUILayout.EndVertical ();
										}
										GUILayout.EndHorizontal ();
										GUILayout.EndVertical ();
									}
									GUILayout.EndHorizontal ();
								}
							}
						}
						else
							if (selectedIndividualSettingsTab == IndividualSettingsTabs.Sunflare) {
								Scatterer.Instance.mainSettings.fullLensFlareReplacement = GUILayout.Toggle (Scatterer.Instance.mainSettings.fullLensFlareReplacement, "Lens flare shader");
							}
							else
								if (selectedIndividualSettingsTab == IndividualSettingsTabs.EVEintegration) {
									Scatterer.Instance.mainSettings.integrateWithEVEClouds = GUILayout.Toggle (Scatterer.Instance.mainSettings.integrateWithEVEClouds, "Integrate effects with EVE clouds (may require restart)");
									//				if (Scatterer.Instance.mainSettings.integrateWithEVEClouds)
									//				{
									//					Scatterer.Instance.mainSettings.integrateEVECloudsGodrays = GUILayout.Toggle (Scatterer.Instance.mainSettings.integrateEVECloudsGodrays, "EVE clouds cast godrays (require godrays)");
									//				}
								}
								else
									if (selectedIndividualSettingsTab == IndividualSettingsTabs.Misc) {
										GUILayout.BeginHorizontal ();
										{
											Scatterer.Instance.mainSettings.overrideNearClipPlane = GUILayout.Toggle (Scatterer.Instance.mainSettings.overrideNearClipPlane, "Override Near ClipPlane (not recommended - restart on disable)");
											Scatterer.Instance.mainSettings.nearClipPlane = float.Parse (GUILayout.TextField (Scatterer.Instance.mainSettings.nearClipPlane.ToString ("0.000")));
										}
										GUILayout.EndHorizontal ();
										GUILayout.BeginHorizontal ();
										{
											Scatterer.Instance.mainSettings.useDithering = GUILayout.Toggle (Scatterer.Instance.mainSettings.useDithering, "Use dithering (Reduces color banding in sky and scattering, disable if you notice dithering patterns");
										}
										GUILayout.EndHorizontal ();
									}
		}
		//Core.Instance.mainSettings.usePlanetShine = GUILayout.Toggle(Core.Instance.usePlanetShine, "PlanetShine");
		//Core.Instance.mainSettings.useGodrays = GUILayout.Toggle(Core.Instance.useGodrays, "Godrays (early WIP)");

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
				GUILayout.Label ("Godrays");
				GUIfloat ("Godray strength*", ref godrayStrength, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.godrayStrength);
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
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.TogglePreserveCloudColors ();
				GUILayout.EndHorizontal ();
//				GUIfloat ("Godray alpha threshold* (alpha value above which a cloud casts a godray)", ref godrayCloudAlphaThreshold, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.godrayCloudAlphaThreshold);
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
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.TweakStockAtmosphere ();
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
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.flattenScaledSpaceMesh = flattenScaledSpaceMesh;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.TweakStockAtmosphere ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Flatten scaled mesh");
			flattenScaledSpaceMesh = (float)(Convert.ToDouble (GUILayout.TextField (flattenScaledSpaceMesh.ToString ("0.000"))));
			if (GUILayout.Button ("Set")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.flattenScaledSpaceMesh = flattenScaledSpaceMesh;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.TweakScaledMesh();
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.scaledScatteringContainer.ApplyNewMesh(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh);
			}
			GUILayout.EndHorizontal ();

			GUILayout.Label ("Misc");
			GUIColorNoButton("Sunlight color (Not saved automatically, save manually to PlanetsList)", ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.sunColor);

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
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.TogglePostProcessing ();
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

			if (!Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.isConfigModuleManagerPatch)
			{
				if (GUILayout.Button ("Save atmo"))
				{
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.SaveToConfigNode ();
				}
			}
			if (GUILayout.Button ("Load atmo")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.LoadFromConfigNode ();
				getSettingsFromSkynode ();
				loadConfigPoint (selectedConfigPoint);
				//Restore sun color, hacky I know
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.sunColor = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.scattererCelestialBody.sunColor;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");

			GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.isConfigModuleManagerPatch)
				guiStyle.normal.textColor = Color.red;

			GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.isConfigModuleManagerPatch ? "ModuleManager patch detected, saving disabled" : Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.configUrl.parent.url, guiStyle);
			GUILayout.EndHorizontal ();
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Map EVE clouds")) {
					Scatterer.Instance.eveReflectionHandler.MapEVEClouds ();
					foreach (ScattererCelestialBody _cel in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies) {
						if (_cel.active) {
							_cel.m_manager.m_skyNode.InitEVEClouds ();
							if (!_cel.m_manager.m_skyNode.inScaledSpace)
								_cel.m_manager.m_skyNode.MapEVEVolumetrics ();
						}
					}
				}
				GUILayout.EndHorizontal ();
			}
		}


		public void buildOceanGUI()
		{
			OceanFFTgpu oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();

			oceanModularGUI.ClearModules ();

			oceanModularGUI.AddModule (new GUIModuleLabel ("Surface settings"));
			oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Upwelling Color", oceanNode, "m_oceanUpwellingColor"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Transparency Depth", oceanNode, "transparencyDepth"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Foam strength (m_whiteCapStr)", oceanNode, "m_whiteCapStr"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Shore/shallow foam strength", oceanNode, "shoreFoam"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Far foam strength (m_farWhiteCapStr)", oceanNode, "m_farWhiteCapStr"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Far foam strength radius (alphaRadius)", oceanNode, "alphaRadius"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Sky reflection strength", oceanNode, "skyReflectionStrength"));

			oceanModularGUI.AddModule (new GUIModuleLabel ("Underwater Settings"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Refraction Index", oceanNode, "refractionIndex"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Darkness Depth", oceanNode, "darknessDepth"));
			oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Underwater Color", oceanNode, "m_UnderwaterColor"));

			oceanModularGUI.AddModule (new GUIModuleLabel ("Caustics/lightrays Settings"));
			if (Scatterer.Instance.mainSettings.oceanCaustics || Scatterer.Instance.mainSettings.oceanLightRays)
			{
				oceanModularGUI.AddModule (new GUIModuleString ("Caustics texture path", oceanNode, "causticsTexturePath"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("Caustics layer 1 scale", oceanNode, "causticsLayer1Scale"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("caustics layer 1 speed", oceanNode, "causticsLayer1Speed"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("Caustics Layer 2 scale", oceanNode, "causticsLayer2Scale"));
				oceanModularGUI.AddModule (new GUIModuleVector2 ("caustics Layer 2 speed", oceanNode, "causticsLayer2Speed"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics texture multiply", oceanNode, "causticsMultiply"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics underwater light boost", oceanNode, "causticsUnderwaterLightBoost"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics minimum brightness (of dark areas in the caustics texture)", oceanNode, "causticsMinBrightness"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Caustics blur depth", oceanNode, "causticsBlurDepth"));
				oceanModularGUI.AddModule (new GUIModuleFloat ("Light rays strength", oceanNode, "lightRaysStrength"));
			}

			oceanModularGUI.AddModule (new GUIModuleLabel ("To apply press \"rebuild ocean\" and wait"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("Keep in mind this saves your current settings"));

			oceanModularGUI.AddModule (new GUIModuleLabel ("Waves physical model settings"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Wave amplitude (AMP)", oceanNode, "AMP"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Wind Speed (m/s)", oceanNode, "m_windSpeed"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Omega (inverse wave age)", oceanNode, "m_omega"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Gravity (m/s², set to 0 for auto)", oceanNode, "m_gravity"));
			oceanModularGUI.AddModule(new GUIModuleFloat("Off screen vertex coverage (Increase with big waves)", oceanNode, "offScreenVertexStretch"));

			oceanModularGUI.AddModule (new GUIModuleLabel ("Performance settings"));
			oceanModularGUI.AddModule (new GUIModuleLabel ("Current fourierGridSize (change from KSC menu): " + Scatterer.Instance.mainSettings.m_fourierGridSize.ToString ()));
			oceanModularGUI.AddModule (new GUIModuleLabel ("Current mesh resolution (change from KSC menu): " + Scatterer.Instance.mainSettings.oceanMeshResolution.ToString ()));
		}

		void DrawOceanGUI ()
		{
			OceanFFTgpu oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ();
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

			if (!Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.m_skyNode.isConfigModuleManagerPatch)
			{
				if (GUILayout.Button ("Save ocean"))
				{
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].m_manager.GetOceanNode ().saveToConfigNode ();
				}
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

						if (Scatterer.Instance.farCamera && Scatterer.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)))
								Component.Destroy (Scatterer.Instance.farCamera.gameObject.GetComponent (typeof(Wireframe)));
					}
					if (Scatterer.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)))
						Component.Destroy (Scatterer.Instance.scaledSpaceCamera.gameObject.GetComponent (typeof(Wireframe)));
					wireFrame = false;
				}
				else {
					if (HighLogic.LoadedScene != GameScenes.TRACKSTATION) {
						Scatterer.Instance.nearCamera.gameObject.AddComponent (typeof(Wireframe));
						if (Scatterer.Instance.farCamera)
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

		public void GUIvector3NoButton (string label, ref Vector3 target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			target.x = float.Parse (GUILayout.TextField (target.x.ToString ("0.0000")));
			target.y = float.Parse (GUILayout.TextField (target.y.ToString ("0.0000")));
			target.z = float.Parse (GUILayout.TextField (target.z.ToString ("0.0000")));

			GUILayout.EndHorizontal ();
		}

		public void GUIColorNoButton (string label, ref Color target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			target.r = float.Parse (GUILayout.TextField (target.r.ToString ("0.0000")));
			target.g = float.Parse (GUILayout.TextField (target.g.ToString ("0.0000")));
			target.b = float.Parse (GUILayout.TextField (target.b.ToString ("0.0000")));
			
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
			flattenScaledSpaceMesh = skyNode.flattenScaledSpaceMesh;

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

			godrayStrength = skyNode.godrayStrength;
//			godrayCloudAlphaThreshold = skyNode.godrayCloudAlphaThreshold;
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

