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
	public class AtmoGUI
	{
		float Rg = 600000.0f;
		float atmosphereStartRadiusScale = 1f;
		float HR = 3.2f;
		float HM = 0.48f;

		Vector3 m_betaR = new Vector3(0.029f, 0.0675f, 0.1655f);
		Vector3 BETA_MSca = new Vector3(0.02f,0.02f,0.02f);

		Vector3 ozoneAbsorption = new Vector3(0.0000003426f, 0.0000008298f, 0.000000036f);
		float ozoneHeight = 25f;
		float ozoneFalloff = 15f;
		bool useOzone = false;

		float m_mieG = 0.85f;
		float AVERAGE_GROUND_REFLECTANCE = 0.1f;

		float rescale = 1f, thickenRayleigh = 1f, thickenMie = 1f, thickenOzone = 1f;
		
		bool multipleScattering = true;
		bool fastPreviewMode = false;

		SkyNode targetSkyNode;
		int selPlanet;

		public AtmoGUI ()
		{
		}

		public void drawAtmoGUI(int selectedPlanet)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Atmosphere start radius     ");				
			GUILayout.TextField((Rg * atmosphereStartRadiusScale).ToString());
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Scale start radius");
			atmosphereStartRadiusScale=(float)(float.Parse(GUILayout.TextField(atmosphereStartRadiusScale.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Atmo height (auto)");
			GUILayout.TextField((AtmoPreprocessor.CalculateRt (Rg*atmosphereStartRadiusScale, HR, HM, m_betaR, BETA_MSca, useOzone, ozoneHeight, ozoneFalloff)-Rg*atmosphereStartRadiusScale).ToString());
			GUILayout.EndHorizontal();

			GUIvector3NoButton ("Rayleigh Scattering - Beta_R:", ref m_betaR);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Thicken");
			thickenRayleigh=(float)(float.Parse(GUILayout.TextField(thickenRayleigh.ToString("00.000"))));
			if (GUILayout.Button ("Go"))
			{
				m_betaR*=thickenRayleigh;
				generate();
			}
			GUILayout.EndHorizontal();

			GUIvector3NoButton ("Mie Scattering - Beta_MSca:", ref BETA_MSca);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Thicken");
			thickenMie=(float)(float.Parse(GUILayout.TextField(thickenMie.ToString("00.000"))));
			if (GUILayout.Button ("Go"))
			{
				BETA_MSca*=thickenMie;
				generate();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Mie G (Mie phase function asymmetry)");
			m_mieG = (float)(float.Parse(GUILayout.TextField(m_mieG.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Rayleigh density scale height");	
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("HR (in KM)");	
			HR=(float)(float.Parse(GUILayout.TextField(HR.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Mie density scale height");	
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("HM (in KM)");	
			HM=(float)(float.Parse(GUILayout.TextField(HM.ToString())));
			GUILayout.EndHorizontal();

			GUIvector3NoButton("Ozone absorption:", ref ozoneAbsorption);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Thicken");
			thickenOzone = (float)(float.Parse(GUILayout.TextField(thickenOzone.ToString("00.000"))));
			if (GUILayout.Button("Go"))
			{
				ozoneAbsorption *= thickenOzone;
				generate();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ozone layer altitude (km)");
			ozoneHeight = (float)(float.Parse(GUILayout.TextField(ozoneHeight.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Ozone layer falloff/extents (km)");
			ozoneFalloff = (float)(float.Parse(GUILayout.TextField(ozoneFalloff.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Use Ozone: " + useOzone.ToString() + " ");
			if (GUILayout.Button("Toggle"))
				useOzone = !useOzone;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Auto rescale");
			rescale=(float)(float.Parse(GUILayout.TextField(rescale.ToString("00.000"))));
			if (GUILayout.Button ("Go"))
			{
				HR*=rescale;
				HM*=rescale;
				m_betaR/=rescale;
				BETA_MSca/=rescale;
				ozoneHeight *= rescale;
				ozoneFalloff *= rescale;
				ozoneAbsorption /= rescale;
				generate();
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Average ground reflectance");
			AVERAGE_GROUND_REFLECTANCE=(float)(float.Parse(GUILayout.TextField(AVERAGE_GROUND_REFLECTANCE.ToString())));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Multiple scattering: "+multipleScattering.ToString()+" ");	
			if (GUILayout.Button ("Toggle"))
				multipleScattering = !multipleScattering;
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Fast preview mode: "+fastPreviewMode.ToString());	
			if (GUILayout.Button ("Toggle"))
				fastPreviewMode = !fastPreviewMode;
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Generate"))
			{
				generate();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button ("Delete atmo cache"))
			{
				AtmoPreprocessor.deleteCache();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal ();
			if (!targetSkyNode.isConfigModuleManagerPatch)
			{
				if (GUILayout.Button ("Save atmo"))
				{
					targetSkyNode.SaveToConfigNode ();
				}
			}
			if (GUILayout.Button ("Load atmo")) {
				targetSkyNode.LoadFromConfigNode ();
				loadSettingsForPlanet(selPlanet);

			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label (".cfg file used:");
			GUIStyle guiStyle = new GUIStyle(GUI.skin.textArea);
			if (targetSkyNode.isConfigModuleManagerPatch)
				guiStyle.normal.textColor = Color.red;
			
			GUILayout.TextField (targetSkyNode.isConfigModuleManagerPatch ? "ModuleManager patch detected, saving disabled" : targetSkyNode.configUrl.parent.url, guiStyle);
			GUILayout.EndHorizontal ();
		}


		void generate()
		{
			Utils.LogDebug ("Generating atmosphere from UI for planet: " + targetSkyNode.prolandManager.scattererCelestialBody.celestialBodyName + "" +
				"With settings:" +
				"Rg " + (Rg*atmosphereStartRadiusScale).ToString () +
				" HR " + HR.ToString () +
				" HM " + HM.ToString () +
				" m_betaR " + m_betaR.ToString () +
				" BETA_MSca " + BETA_MSca.ToString () +
				" m_mieG " + m_mieG.ToString () +
				" ozoneAbsorption " + ozoneAbsorption.ToString() +
				" ozoneHeight " + ozoneHeight.ToString() +
				" ozoneFalloff " + ozoneFalloff.ToString() +
				" useOzone " + useOzone.ToString() +
				" AVERAGE_GROUND_REFLECTANCE " + AVERAGE_GROUND_REFLECTANCE.ToString () +
				" multipleScattering " + multipleScattering.ToString () +
				" fastPreviewMode " + fastPreviewMode.ToString ());

			targetSkyNode.ApplyAtmoFromUI (m_betaR, BETA_MSca, m_mieG, HR, HM, AVERAGE_GROUND_REFLECTANCE, multipleScattering, fastPreviewMode, atmosphereStartRadiusScale, useOzone, ozoneAbsorption, ozoneHeight, ozoneFalloff);
		}

		public void loadSettingsForPlanet(int selectedPlanet)
		{
			if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active)
			{
				targetSkyNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetSkyNode();
				selPlanet = selectedPlanet;

				Rg = targetSkyNode.Rg;
				atmosphereStartRadiusScale = targetSkyNode.atmosphereStartRadiusScale;
				HR = targetSkyNode.HR;
				HM = targetSkyNode.HM;
				m_betaR = targetSkyNode.m_betaR;
				BETA_MSca = targetSkyNode.BETA_MSca;
				m_mieG = targetSkyNode.m_mieG;
				AVERAGE_GROUND_REFLECTANCE = targetSkyNode.averageGroundReflectance;
				multipleScattering = targetSkyNode.multipleScattering;

				ozoneAbsorption = targetSkyNode.ozoneAbsorption;
				ozoneHeight = targetSkyNode.ozoneHeight;
				ozoneFalloff = targetSkyNode.ozoneFalloff;
				useOzone = targetSkyNode.useOzone;

				fastPreviewMode = false;
				rescale=1f;
				thickenRayleigh=1f;
				thickenMie=1f;
			}
		}

		public void GUIvector3NoButton (string label, ref Vector3 target)
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label (label);
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			GUILayout.Label ("R");
			target.x = float.Parse(GUILayout.TextField(target.x.ToString()));
			GUILayout.Label ("G");
			target.y = float.Parse(GUILayout.TextField(target.y.ToString()));
			GUILayout.Label ("B");
			target.z = float.Parse (GUILayout.TextField (target.z.ToString()));
			GUILayout.EndHorizontal ();
		}
	}
}

