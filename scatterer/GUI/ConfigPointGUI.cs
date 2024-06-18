using System;
using UnityEngine;

namespace Scatterer
{
	public class ConfigPointGUI
	{
		public int selectedConfigPoint = 0;
		private Vector2 _scroll;

		Vector3 sunColor=Vector3.one;
		
		float rimBlend = 20f;
		float rimpower = 600f;
		float cloudColorMultiplier=1f;
		float cloudScatteringMultiplier=1f;
		float cloudSkyIrradianceMultiplier = 1f;
		float volumetricsColorMultiplier=1f;
		
		float godrayStrength = 1.0f;
		//		float godrayCloudAlphaThreshold = 0.1f;
		
		float extinctionThickness = 1f;
		float skyExtinctionTint = 1f;
		float noonSunlightExtinctionStrength = 1f;

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

		public ConfigPointGUI ()
		{
		}

		public void DrawConfigPointGUI (int selectedPlanet)
		{
			configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints.Count;
			
			ConfigPoint _cur = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints [selectedConfigPoint];
			
			//if (!MapView.MapIsEnabled)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("New point altitude:");
				newCfgPtAlt = Convert.ToSingle (GUILayout.TextField (newCfgPtAlt.ToString ()));
				if (GUILayout.Button ("Add")) {
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints.Insert (selectedConfigPoint + 1, new ConfigPoint (newCfgPtAlt, alphaGlobal / 100, exposure / 100, postProcessingalpha / 100, postProcessDepth / 10000, postProcessExposure / 100, skyExtinctionTint / 100, extinctionTint / 100, extinctionThickness));
					selectedConfigPoint += 1;
					configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints.Count;
					loadConfigPoint (selectedConfigPoint, selectedPlanet);
				}
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Config point:");
				if (GUILayout.Button ("<")) {
					if (selectedConfigPoint > 0) {
						selectedConfigPoint -= 1;
						loadConfigPoint (selectedConfigPoint, selectedPlanet);
					}
				}
				GUILayout.TextField ((selectedConfigPoint).ToString ());
				if (GUILayout.Button (">")) {
					if (selectedConfigPoint < configPointsCnt - 1) {
						selectedConfigPoint += 1;
						loadConfigPoint (selectedConfigPoint, selectedPlanet);
					}
				}
				//GUILayout.Label (String.Format("Total:{0}", configPointsCnt));
				if (GUILayout.Button ("Delete")) {
					if (configPointsCnt <= 1)
						Debug.LogError ("Can't delete config point, one or no points remaining");
					else {
						Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints.RemoveAt (selectedConfigPoint);
						if (selectedConfigPoint >= configPointsCnt - 1) {
							selectedConfigPoint = configPointsCnt - 2;
						}
						configPointsCnt = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints.Count;
						loadConfigPoint (selectedConfigPoint, selectedPlanet);
					}
				}
				GUILayout.EndHorizontal ();
				GUIfloat ("Point altitude", ref pointAltitude, ref _cur.altitude);
				_scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (Scatterer.Instance.pluginData.scrollSectionHeight));
				GUILayout.Label ("(settings with a * are global and not cfgPoint dependent)");

				GUILayout.Label ("Sky");
				GUIfloat ("Sky Exposure", ref exposure, ref _cur.skyExposure);
				GUIfloat ("Sky Alpha", ref alphaGlobal, ref _cur.skyAlpha);
				GUIfloat ("Sky Extinction Tint", ref skyExtinctionTint, ref _cur.skyExtinctionTint);
				GUILayout.Label ("Scattering and Extinction");
				GUIfloat ("Scattering Exposure (scaled+local)", ref postProcessExposure, ref _cur.scatteringExposure);
				GUIfloat ("Extinction Tint (scaled+local)", ref extinctionTint, ref _cur.extinctionTint);
				GUIfloat ("Extinction Thickness (scaled+local)", ref extinctionThickness, ref _cur.extinctionThickness);
				GUIfloat ("Noon Sunlight Extinction strength*", ref noonSunlightExtinctionStrength, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies[selectedPlanet].prolandManager.skyNode.noonSunlightExtinctionStrength);
				GUILayout.Label ("Post Processing");
				GUIfloat ("Post Processing Alpha", ref postProcessingalpha, ref _cur.postProcessAlpha);
				GUIfloat ("Post Processing Depth", ref postProcessDepth, ref _cur.postProcessDepth);
				GUILayout.Label ("Godrays");
				
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Legacy Godrays strength*");				
				godrayStrength = Mathf.Min(float.Parse (GUILayout.TextField (godrayStrength.ToString ("0.000"))),1.0f);
				if (GUILayout.Button ("Set")) {
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.godrayStrength = godrayStrength;
				}
				GUILayout.EndHorizontal ();
			}
			if (Scatterer.Instance.mainSettings.integrateWithEVEClouds && Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.usesCloudIntegration) {
				GUILayout.Label ("EVE integration");
				GUIfloat ("2d Cloud Color Multiplier*", ref cloudColorMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.cloudColorMultiplier);
				GUIfloat ("2d Cloud Scattering Multiplier*", ref cloudScatteringMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.cloudScatteringMultiplier);
				GUIfloat ("2d Cloud Sky irradiance Multiplier*", ref cloudSkyIrradianceMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.cloudSkyIrradianceMultiplier);
				GUIfloat ("Particle Volumetrics Color Multiplier*", ref volumetricsColorMultiplier, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.volumetricsColorMultiplier);
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Preserve 2d cloud colors*");
				GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.EVEIntegration_preserveCloudColors.ToString ());
				if (GUILayout.Button ("Toggle"))
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.TogglePreserveCloudColors ();
				GUILayout.EndHorizontal ();
				//				GUIfloat ("Godray alpha threshold* (alpha value above which a cloud casts a godray)", ref godrayCloudAlphaThreshold, ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.godrayCloudAlphaThreshold);
				//								GUIfloat("Volumetrics Scattering Multiplier", ref volumetricsScatteringMultiplier, ref Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.volumetricsScatteringMultiplier);
				//								GUIfloat("Volumetrics Sky irradiance Multiplier", ref volumetricsSkyIrradianceMultiplier, ref Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.volumetricsSkyIrradianceMultiplier);
			}
			GUILayout.Label ("ScaledSpace model");
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("RimBlend*");
			rimBlend = Convert.ToSingle (GUILayout.TextField (rimBlend.ToString ()));
			GUILayout.Label ("RimPower*");
			rimpower = Convert.ToSingle (GUILayout.TextField (rimpower.ToString ()));
			if (GUILayout.Button ("Set")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.rimBlend = rimBlend;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.rimpower = rimpower;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.TweakStockAtmosphere ();
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
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.specR = specR;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.specG = specG;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.specB = specB;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.shininess = shininess;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.flattenScaledSpaceMesh = flattenScaledSpaceMesh;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.TweakStockAtmosphere ();
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Flatten scaled mesh");
			flattenScaledSpaceMesh = (float)(Convert.ToDouble (GUILayout.TextField (flattenScaledSpaceMesh.ToString ("0.000"))));
			if (GUILayout.Button ("Set")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.flattenScaledSpaceMesh = flattenScaledSpaceMesh;
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.TweakScaledMesh();
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.scaledScatteringContainer.ApplyNewMesh(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.parentScaledTransform.GetComponent<MeshFilter> ().sharedMesh);
			}
			GUILayout.EndHorizontal ();

			Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.adjustScaledTexture = GUILayout.Toggle (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.adjustScaledTexture, "Adjust scaled texture");

			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.adjustScaledTexture)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Land (brightness/contrast/saturation)");
				GUILayout.EndHorizontal ();

				var node = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode;

				GUILayout.BeginHorizontal ();
				node.scaledLandBrightnessAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledLandBrightnessAdjust.ToString ("0.00"))));
				node.scaledLandContrastAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledLandContrastAdjust.ToString ("0.00"))));
				node.scaledLandSaturationAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledLandSaturationAdjust.ToString ("0.00"))));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Ocean (brightness/contrast/saturation)");
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				node.scaledOceanBrightnessAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledOceanBrightnessAdjust.ToString ("0.00"))));
				node.scaledOceanContrastAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledOceanContrastAdjust.ToString ("0.00"))));
				node.scaledOceanSaturationAdjust = (float)(Convert.ToDouble (GUILayout.TextField (node.scaledOceanSaturationAdjust.ToString ("0.00"))));
				GUILayout.EndHorizontal ();

				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Set")) {
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.TweakStockScaledTexture();
				}
				GUILayout.EndHorizontal ();

			}

			GUILayout.Label ("Misc");
			GUIColorNoButton("Sunlight color "+ Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.mainSunLight.name+ " (Not saved automatically, save manually to PlanetsList)", ref Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.sunColor);
			
			int index = 0;
			foreach (SecondarySun secondarySun in Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.secondarySuns)
			{
				Vector4 colorVect = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.planetShineRGBMatrix.GetRow(index);
				Color col = new Color(colorVect.x, colorVect.y, colorVect.z);
				GUIColorNoButton("Secondary sunlight color "+secondarySun.config.celestialBodyName, ref col);
				colorVect = new Vector4(col.r, col.g, col.b, colorVect.w);
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.planetShineRGBMatrix.SetRow(index, colorVect);
				index++;
			}
			
			GUILayout.EndScrollView ();
			GUILayout.BeginHorizontal ();
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.currentConfigPoint == 0)
				GUILayout.Label ("Current state:Lowest configPoint, cfgPoint 0");
			else
				if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.currentConfigPoint >= configPointsCnt)
					GUILayout.Label (String.Format ("Current state:Highest configPoint, cfgPoint{0}", Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.currentConfigPoint - 1));
			else
				GUILayout.Label (String.Format ("Current state:{0}% cfgPoint{1} + {2}% cfgPoint{3} ", (int)(100 * (1 - Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.percentage)), Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.currentConfigPoint - 1, (int)(100 * Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.percentage), Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.currentConfigPoint));
			GUILayout.EndHorizontal ();
			//							GUILayout.BeginHorizontal ();
			//							if (GUILayout.Button ("toggle sky"))
			//							{
			//								Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.skyEnabled = !Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.skyEnabled;
			//								if (Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.skyEnabled)
			//									Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.tweakStockAtmosphere();
			//								else
			//									Core.Instance.planetsListReader.scattererCelestialBodies [selectedPlanet].prolandManager.m_skyNode.RestoreStockAtmosphere();
			//							}
			//							GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			
			if (!Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.isConfigModuleManagerPatch)
			{
				if (GUILayout.Button ("Save atmo"))
				{
					Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.SaveToConfigNode ();
				}
			}
			if (GUILayout.Button ("Load atmo")) {
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.LoadFromConfigNode ();
				getSettingsFromSkynode (selectedPlanet);
				loadConfigPoint (selectedConfigPoint, selectedPlanet);
				//Restore sun color, hacky I know
				Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.sunColor = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.scattererCelestialBody.sunColor;
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");
			
			GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.isConfigModuleManagerPatch)
				guiStyle.normal.textColor = Color.red;
			
			GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.isConfigModuleManagerPatch ? "ModuleManager patch detected, saving disabled" : Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configUrl.parent.url, guiStyle);
			GUILayout.EndHorizontal ();
		}

		public void loadSettingsForPlanet(int selectedPlanet)
		{
			selectedConfigPoint = 0;
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active)
			{
				loadConfigPoint (selectedConfigPoint, selectedPlanet);
				getSettingsFromSkynode (selectedPlanet);
			}
		}

		public void getSettingsFromSkynode (int selectedPlanet)
		{
			SkyNode skyNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode;
			ConfigPoint selected = skyNode.configPoints [selectedConfigPoint];
			
			postProcessingalpha = selected.postProcessAlpha;
			postProcessDepth = selected.postProcessDepth;
			
			extinctionTint = selected.extinctionTint;
			
			postProcessExposure = selected.scatteringExposure;
			exposure = selected.skyExposure;
			alphaGlobal = selected.skyAlpha;
			
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
			
			cloudColorMultiplier = skyNode.cloudColorMultiplier;
			cloudScatteringMultiplier = skyNode.cloudScatteringMultiplier;
			cloudSkyIrradianceMultiplier = skyNode.cloudSkyIrradianceMultiplier;
			
			volumetricsColorMultiplier = skyNode.volumetricsColorMultiplier;
			//			volumetricsScatteringMultiplier = skyNode.volumetricsScatteringMultiplier;
			//			volumetricsSkyIrradianceMultiplier = skyNode.volumetricsSkyIrradianceMultiplier;
			
			godrayStrength = skyNode.godrayStrength;
			//			godrayCloudAlphaThreshold = skyNode.godrayCloudAlphaThreshold;
		}
		
		public void loadConfigPoint (int point, int selectedPlanet)
		{
			ConfigPoint _cur = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.configPoints [point];
			
			postProcessDepth = _cur.postProcessDepth;
			extinctionTint = _cur.extinctionTint;
			postProcessExposure = _cur.scatteringExposure;
			postProcessingalpha = _cur.postProcessAlpha;
			
			alphaGlobal = _cur.skyAlpha;
			exposure = _cur.skyExposure;
			skyExtinctionTint = _cur.skyExtinctionTint;
			
			extinctionThickness = _cur.extinctionThickness;
			
			pointAltitude = _cur.altitude;
		}

		public void GUIfloat (string label, ref float local, ref float target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			local = float.Parse (GUILayout.TextField (local.ToString ()));
			if (GUILayout.Button ("Set")) {
				target = local;
			}
			GUILayout.EndHorizontal ();
		}

		public void GUIColorNoButton (string label, ref Color target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			
			target.r = float.Parse (GUILayout.TextField (target.r.ToString ()));
			target.g = float.Parse (GUILayout.TextField (target.g.ToString ()));
			target.b = float.Parse (GUILayout.TextField (target.b.ToString ()));
			
			GUILayout.EndHorizontal ();
		}
	}
}

