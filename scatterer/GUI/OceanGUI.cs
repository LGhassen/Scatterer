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
    public class OceanGUI
    {
        ModularGUI oceanModularGUI = new ModularGUI();
        private Vector2 _scroll;

        public OceanGUI ()
        {
        }

        public void buildOceanGUI(int selectedPlanet)
        {
            OceanFFTgpu oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ();
            
            oceanModularGUI.ClearModules ();

            oceanModularGUI.AddModule(new GUIModuleLabel("To apply press \"rebuild ocean\" and wait"));
            oceanModularGUI.AddModule(new GUIModuleLabel("Keep in mind this saves your current settings"));

            oceanModularGUI.AddModule(new GUIModuleLabel(""));
            oceanModularGUI.AddModule(new GUIModuleLabel("Waves physical model settings"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Wave amplitude (AMP)", oceanNode, "AMP"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Wind Speed (m/s)", oceanNode, "m_windSpeed"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Omega (inverse wave age)", oceanNode, "m_omega"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Gravity (m/s², set to 0 for auto)", oceanNode, "m_gravity"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Off screen vertex coverage (Increase with big waves)", oceanNode, "offScreenVertexStretch"));

            oceanModularGUI.AddModule(new GUIModuleLabel(""));
            oceanModularGUI.AddModule (new GUIModuleLabel ("Surface shading settings"));
            oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Upwelling Color", oceanNode, "m_oceanUpwellingColor"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Transparency Depth", oceanNode, "transparencyDepth"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Foam strength (m_whiteCapStr)", oceanNode, "m_whiteCapStr"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Shore/shallow foam strength", oceanNode, "shoreFoam"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Far foam strength (m_farWhiteCapStr)", oceanNode, "m_farWhiteCapStr"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Far foam strength radius (alphaRadius)", oceanNode, "alphaRadius"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Sky reflection strength", oceanNode, "skyReflectionStrength"));

            oceanModularGUI.AddModule(new GUIModuleLabel(""));
            oceanModularGUI.AddModule (new GUIModuleLabel ("Underwater shading Settings"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Refraction Index", oceanNode, "refractionIndex"));
            oceanModularGUI.AddModule(new GUIModuleFloat("Darkness Depth", oceanNode, "darknessDepth"));
            oceanModularGUI.AddModule(new GUIModuleVector3("Ocean Underwater Color", oceanNode, "m_UnderwaterColor"));
            
            if (Scatterer.Instance.mainSettings.oceanCaustics || Scatterer.Instance.mainSettings.oceanLightRays)
            {
                oceanModularGUI.AddModule(new GUIModuleLabel(""));
                oceanModularGUI.AddModule(new GUIModuleLabel("Caustics/lightrays Settings"));
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

            oceanModularGUI.AddModule(new GUIModuleLabel(""));
            oceanModularGUI.AddModule (new GUIModuleLabel ("Performance settings"));
            oceanModularGUI.AddModule (new GUIModuleLabel ("Current fourierGridSize (change from KSC menu): " + Scatterer.Instance.mainSettings.m_fourierGridSize.ToString ()));
            oceanModularGUI.AddModule (new GUIModuleLabel ("Current mesh resolution (change from KSC menu): " + Scatterer.Instance.mainSettings.oceanMeshResolution.ToString ()));
        }
        
        public void drawOceanGUI (int selectedPlanet)
        {
            OceanFFTgpu oceanNode = Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ();
            //GUItoggle("Toggle ocean", ref stockOcean);
            _scroll = GUILayout.BeginScrollView (_scroll, false, true, GUILayout.Width (400), GUILayout.Height (Scatterer.Instance.pluginData.scrollSectionHeight + 100));
            {
                oceanModularGUI.RenderGUI();
            }
            GUILayout.EndScrollView ();
            GUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Apply settings/Rebuild ocean")) {
                Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ().saveToConfigNode ();
                Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.RebuildOcean ();
                buildOceanGUI(selectedPlanet);
            }
            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();
            
            if (!Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.skyNode.isConfigModuleManagerPatch)
            {
                if (GUILayout.Button ("Save ocean"))
                {
                    Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ().saveToConfigNode ();
                }
            }
            
            if (GUILayout.Button ("Load ocean")) {
                Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ().LoadFromConfigNode ();
                buildOceanGUI (selectedPlanet);
            }
            GUILayout.EndHorizontal ();
            GUILayout.BeginHorizontal ();
            GUILayout.Label (".cfg file used:");
            GUILayout.TextField (Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies [selectedPlanet].prolandManager.GetOceanNode ().configUrl.parent.url);
            GUILayout.EndHorizontal ();
        }

    }
}

