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

		public int selectedPlanet = 0;
		bool wireFrame = false;

		enum PlanetSettingsTabs
		{
			ConfigPoints,
			Atmo,
			Ocean
		}
		PlanetSettingsTabs selectedPlanetSettingsTab = PlanetSettingsTabs.ConfigPoints;

		MainOptionsGUI mainOptionsGUI = new MainOptionsGUI();
		ConfigPointGUI configPointGUI = new ConfigPointGUI ();
		AtmoGUI atmoGUI = new AtmoGUI ();
		OceanGUI oceanGUI = new OceanGUI ();
		SunflareGUI sunflareGUI = new SunflareGUI ();

		public GUIhandler ()
		{
		}

		public void Init()
		{
			windowId = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
			mainOptions = (HighLogic.LoadedScene == GameScenes.SPACECENTER);
			windowRect.x=Scatterer.Instance.pluginData.inGameWindowLocation.x;
			windowRect.y=Scatterer.Instance.pluginData.inGameWindowLocation.y;

			//prevent window from going offscreen, only do this on start as otherwise it messes with lower resolutions
			windowRect.x = Mathf.Clamp(windowRect.x, 0, Screen.width - windowRect.width);
			windowRect.y = Mathf.Clamp(windowRect.y, 0, Screen.height - windowRect.height);
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
			}
		}
		
		public void DrawScattererWindow (int windowId)
		{
			if (mainOptions)
			{ 
				mainOptionsGUI.DrawOptionsMenu ();
			}
			else if (Scatterer.Instance.isActive)
			{
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Planet settings"))
				{
					windowRect.width = 400;
					windowRect.height = 40;
					sunflareOptions = false;
				}
				if (Scatterer.Instance.mainSettings.fullLensFlareReplacement && !ReferenceEquals(Scatterer.Instance.sunflareManager,null) && !ReferenceEquals(Scatterer.Instance.sunflareManager.scattererSunFlares,null))
				{
					if (GUILayout.Button ("Sunflare settings"))
					{
						windowRect.width = 400;
						windowRect.height = 40;
						sunflareGUI.InitSunflareGUI();
						sunflareOptions = true;
					}
				}
				GUILayout.EndHorizontal ();

				if (sunflareOptions)
				{
					sunflareGUI.DrawSunflareGUI();
				}
				else
				{
					DrawPlanetSelectionHeader ();


					if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active)
					{	

						GUILayout.BeginHorizontal ();
						if (GUILayout.Button ("Config points"))
						{
							windowRect.width = 400;
							windowRect.height = 40;
							selectedPlanetSettingsTab = PlanetSettingsTabs.ConfigPoints;
						}
						if (GUILayout.Button ("Atmosphere"))
						{
							windowRect.width = 300;
							windowRect.height = 40;
							selectedPlanetSettingsTab = PlanetSettingsTabs.Atmo;
						}
						if (GUILayout.Button ("Ocean") && Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean)
						{
							windowRect.width = 400;
							windowRect.height = 40;
							selectedPlanetSettingsTab = PlanetSettingsTabs.Ocean;
						}
						GUILayout.EndHorizontal ();	

						if (selectedPlanetSettingsTab == PlanetSettingsTabs.ConfigPoints)
						{
							configPointGUI.DrawConfigPointGUI(selectedPlanet);
						}
						else if (selectedPlanetSettingsTab == PlanetSettingsTabs.Atmo)
						{
							atmoGUI.drawAtmoGUI (selectedPlanet);
						}
						else
						{
							oceanGUI.drawOceanGUI (selectedPlanet);
						}
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

		//move the getSettings, loadConfigPoint, selectedConfigPoint = 0; to method in ConfigPoint GUI
		void DrawPlanetSelectionHeader ()
		{
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Planet:");
			if (GUILayout.Button ("<"))
			{
				if (selectedPlanet > 0)
				{
					selectedPlanet -= 1;

					configPointGUI.loadSettingsForPlanet(selectedPlanet);
					atmoGUI.loadSettingsForPlanet(selectedPlanet);

					if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active && Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean)
					{
						oceanGUI.buildOceanGUI (selectedPlanet);
					}
				}
			}
			GUILayout.TextField ((Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].celestialBodyName).ToString ());
			if (GUILayout.Button (">")) {
				if (selectedPlanet < Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Count - 1) {
					selectedPlanet += 1;
					configPointGUI.loadSettingsForPlanet(selectedPlanet);
					atmoGUI.loadSettingsForPlanet(selectedPlanet);

					if (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active && Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean)
					{
						oceanGUI.buildOceanGUI (selectedPlanet);
					}
				}
			}
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Loaded:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].active.ToString () + ". Has ocean:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].hasOcean.ToString ());
			GUILayout.EndHorizontal ();
			GUILayout.BeginHorizontal ();
			GUILayout.Label ("Load distance:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].loadDistance.ToString () + ". Unload distance:" + Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].unloadDistance.ToString ());
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

		public void loadPlanet(int planetIndex)
		{
			selectedPlanetSettingsTab = PlanetSettingsTabs.ConfigPoints;
			selectedPlanet = planetIndex;
			configPointGUI.loadSettingsForPlanet (selectedPlanet);
			atmoGUI.loadSettingsForPlanet(selectedPlanet);

			if (!ReferenceEquals (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode(), null))
			{
				oceanGUI.buildOceanGUI (planetIndex);
			}
		}
	}
}

