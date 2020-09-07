/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 *
 *
 */
using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.Rendering;

using KSP.IO;

namespace scatterer
{
	//TODO: refactor and clean up this class
	public abstract class OceanNode: MonoBehaviour
	{
		public UrlDir.UrlConfig configUrl;
		
		public ProlandManager m_manager;

		public Material m_oceanMaterial;

		[Persistent]
		public Vector3 m_oceanUpwellingColor = new Vector3 (0.0039f, 0.0156f, 0.047f);

		[Persistent]
		public Vector3 m_UnderwaterColor = new Vector3 (0.1f, 0.75f, 0.8f);

		//Size of each grid in the projected grid. (number of pixels on screen)		
		[Persistent]
		public int m_resolution = 4;

		public const int MAX_VERTS = 65000;
		[Persistent]
		public float oceanScale = 1f;
		[Persistent]
		public float oceanAlpha = 1f;

		[Persistent]
		public float alphaRadius = 3000f;

		[Persistent]
		public float transparencyDepth = 60f;

		[Persistent]
		public float darknessDepth = 1000f;

		[Persistent]
		public float refractionIndex = 1.33f;

		public bool isUnderwater = false;
		bool underwaterMode = false;

		public int numGrids;
		Mesh[] m_screenGrids;

		GameObject[] waterGameObjects;
		public MeshRenderer[] waterMeshRenderers;
		MeshFilter[] waterMeshFilters;

		//TODO: merge AtmosphereProjector class and it's material, make material public, rename AtmosphereProjector to LocalEffectProjector
		public AtmosphereProjector underwaterProjector;
		Material underwaterMaterial;

		public Vector3 offsetVector3{
			get {
				return offset.ToVector3();
			}
		}

		public double h = 0;
		public Vector3d2 m_Offset = Vector3d2.Zero ();
		public Vector3d2 offset;
		public Vector3d2 ux, uy, uz, oo;

		OceanCameraUpdateHook oceanCameraProjectionMatModifier;
		UnderwaterDimmingHook underwaterDimmingHook;

		public float planetOpacity=1f; //planetOpacity to fade out the ocean when PQS is fading out

		//Concrete classes must provide a function that returns the
		//variance of the waves need for the BRDF rendering of waves
		public abstract float GetMaxSlopeVariance ();

		//caustics
		[Persistent]
		public string causticsTexturePath="";
		[Persistent]
		public Vector2 causticsLayer1Scale;
		[Persistent]
		public Vector2 causticsLayer1Speed;
		[Persistent]
		public Vector2 causticsLayer2Scale;
		[Persistent]
		public Vector2 causticsLayer2Speed;
		[Persistent]
		public float causticsMultiply;
		[Persistent]
		public float causticsUnderwaterLightBoost;
		[Persistent]
		public float causticsMinBrightness;
		[Persistent]
		public float causticsBlurDepth;

		public CausticsShadowMaskModulate causticsShadowMaskModulator;
		public CausticsLightRaysRenderer causticsLightRaysRenderer;
		
		public virtual void Init (ProlandManager manager)
		{
			m_manager = manager;
			loadFromConfigNode ();

			InitOceanMaterial ();

			//Worth moving to projected Grid Class?
			CreateProjectedGrid ();

			oceanCameraProjectionMatModifier = waterGameObjects[0].AddComponent<OceanCameraUpdateHook>();
			oceanCameraProjectionMatModifier.oceanNode = this;

			waterGameObjects[0].AddComponent<ScreenCopierNotifier>();

			InitUnderwaterMaterial ();

			underwaterProjector = new AtmosphereProjector(underwaterMaterial,m_manager.parentLocalTransform,(float)m_manager.m_radius);
			underwaterProjector.setActivated(false);

			//dimming
			//TODO: maybe this can be changed, instead of complicated hooks on the Camera, add it to the light, like causticsShadowMaskModulate?
			if ((Scatterer.Instance.mainSettings.underwaterLightDimming || Scatterer.Instance.mainSettings.oceanCaustics) && (HighLogic.LoadedScene != GameScenes.MAINMENU))
			{
				underwaterDimmingHook = (UnderwaterDimmingHook) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(UnderwaterDimmingHook));
				underwaterDimmingHook.oceanNode = this;
			}

			if (Scatterer.Instance.mainSettings.oceanCaustics && (HighLogic.LoadedScene == GameScenes.FLIGHT))
			{
				//why doesn't this work with IVA camera? do they have a separate light?
				causticsShadowMaskModulator = (CausticsShadowMaskModulate) Scatterer.Instance.sunLight.gameObject.AddComponent (typeof(CausticsShadowMaskModulate));
				if(!causticsShadowMaskModulator.Init(causticsTexturePath, causticsLayer1Scale, causticsLayer1Speed, causticsLayer2Scale, causticsLayer2Speed,
				                                     causticsMultiply, causticsMinBrightness, (float)manager.GetRadius(), causticsBlurDepth))
				{
					UnityEngine.Object.DestroyImmediate (causticsShadowMaskModulator);
					causticsShadowMaskModulator = null;
				}

				if (Scatterer.Instance.mainSettings.oceanLightRays && Scatterer.Instance.mainSettings.shadowsOnOcean)
				{
					causticsLightRaysRenderer = (CausticsLightRaysRenderer) waterGameObjects[0].AddComponent<CausticsLightRaysRenderer>();
					if (!causticsLightRaysRenderer.Init(causticsTexturePath, causticsLayer1Scale, causticsLayer1Speed, causticsLayer2Scale, causticsLayer2Speed,
					                                    causticsMultiply, causticsMinBrightness, (float)manager.GetRadius(), causticsBlurDepth, this))
					{
						UnityEngine.Object.DestroyImmediate (causticsLightRaysRenderer);
						causticsLightRaysRenderer = null;
					}
				}
			}
		}	

		public virtual void UpdateNode ()
		{
			bool oceanDraw = !MapView.MapIsEnabled && !m_manager.m_skyNode.inScaledSpace && (planetOpacity > 0f);

			foreach (MeshRenderer _mr in waterMeshRenderers)
			{
				_mr.enabled= oceanDraw;
			}

			isUnderwater = ((Scatterer.Instance.nearCamera.transform.position - m_manager.parentLocalTransform.position).magnitude -(float)m_manager.m_radius) < 0f;

			underwaterProjector.projector.enabled = isUnderwater;

			if (underwaterMode ^ isUnderwater)
			{
				toggleUnderwaterMode();
			}

			if (!ReferenceEquals (causticsShadowMaskModulator, null))
			{
				causticsShadowMaskModulator.isEnabled = oceanDraw && (m_manager.GetSkyNode().trueAlt < 6000f);
				causticsShadowMaskModulator.UpdateCaustics ();
			}			
		}

		public void updateNonCameraSpecificUniforms (Material oceanMaterial)
		{
			m_manager.GetSkyNode ().SetOceanUniforms (oceanMaterial);

			if (underwaterMode)
			{
				m_manager.GetSkyNode ().UpdatePostProcessMaterial (underwaterMaterial);
			}
			
			planetOpacity = 1f - m_manager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY);
			m_oceanMaterial.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, planetOpacity);
			
			m_oceanMaterial.SetInt (ShaderProperties._ZwriteVariable_PROPERTY, (planetOpacity == 1) ? 1 : 0); //if planetOpacity!=1, ie fading out the sea, disable scattering on it and enable the projector scattering, for the projector scattering to work need to disable zwrite
		}

		public void OnPreCull() //OnPreCull of OceanNode (added to first local Camera to render) executes after OnPreCull of SkyNode (added to ScaledSpaceCamera, executes first)
		{
			if (!MapView.MapIsEnabled && Scatterer.Instance.nearCamera && !m_manager.m_skyNode.inScaledSpace)
			{
				updateNonCameraSpecificUniforms(m_oceanMaterial);
			}
		}

		void CreateProjectedGrid ()
		{
			//Create the projected grid. The resolution is the size in pixels
			//of each square in the grid. If the squares are small the size of
			//the mesh will exceed the max verts for a mesh in Unity. In this case
			//split the mesh up into smaller meshes.
			m_resolution = Mathf.Max (1, m_resolution);
			//The number of squares in the grid on the x and y axis
			int NX = Screen.width / m_resolution;
			int NY = Screen.height / m_resolution;
			numGrids = 1;
			//			const int MAX_VERTS = 65000;
			//The number of meshes need to make a grid of this resolution
			if (NX * NY > MAX_VERTS) {
				numGrids += (NX * NY) / MAX_VERTS;
			}
			m_screenGrids = new Mesh[numGrids];
			waterGameObjects = new GameObject[numGrids];
			waterMeshRenderers = new MeshRenderer[numGrids];
			waterMeshFilters = new MeshFilter[numGrids];
			//Make the meshes. The end product will be a grid of verts that cover
			//the screen on the x and y axis with the z depth at 0. This grid is then
			//projected as the ocean by the shader
			for (int i = 0; i < numGrids; i++)
			{
				NY = Screen.height / numGrids / m_resolution;
				m_screenGrids [i] = MeshFactory.MakePlane (NX, NY, MeshFactory.PLANE.XY, false, true, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				m_screenGrids [i].bounds = new Bounds (Vector3.zero, new Vector3 (1e8f, 1e8f, 1e8f));
				waterGameObjects [i] = new GameObject ();
				waterGameObjects [i].transform.parent = m_manager.parentCelestialBody.transform;
				//might be redundant
				waterMeshFilters [i] = waterGameObjects [i].AddComponent<MeshFilter> ();
				waterMeshFilters [i].mesh.Clear ();
				waterMeshFilters [i].mesh = m_screenGrids [i];
				waterGameObjects [i].layer = 15;
				waterMeshRenderers [i] = waterGameObjects [i].AddComponent<MeshRenderer> ();
				waterMeshRenderers [i].sharedMaterial = m_oceanMaterial;
				waterMeshRenderers [i].material = m_oceanMaterial;
				waterMeshRenderers [i].receiveShadows = Scatterer.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable);
				waterMeshRenderers [i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				waterMeshRenderers [i].enabled = true;
			}
		}
		
		void InitOceanMaterial ()
		{
			if (Scatterer.Instance.mainSettings.oceanPixelLights)
			{
				m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/OceanWhiteCapsPixelLights")]);
			}
			else
			{
				m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/OceanWhiteCaps")]);
			}

			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "SKY_REFLECTIONS_ON", "SKY_REFLECTIONS_OFF", Scatterer.Instance.mainSettings.oceanSkyReflections);
			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "PLANETSHINE_ON", "PLANETSHINE_OFF", Scatterer.Instance.mainSettings.usePlanetShine);
			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "REFRACTION_ON", "REFRACTION_OFF", Scatterer.Instance.mainSettings.oceanRefraction);

			if (Scatterer.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable))
			{
				Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "OCEAN_SHADOWS_HARD", "OCEAN_SHADOWS_SOFT", (QualitySettings.shadows == ShadowQuality.HardOnly));
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_OFF");
			}
			else
			{
				m_oceanMaterial.EnableKeyword ("OCEAN_SHADOWS_OFF");
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_HARD");
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_SOFT");
			}

			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "SCATTERER_MERGED_DEPTH_OFF", "SCATTERER_MERGED_DEPTH_ON", Scatterer.Instance.unifiedCameraMode);

			m_oceanMaterial.SetOverrideTag ("IgnoreProjector", "True");
			
			m_manager.GetSkyNode ().InitUniforms (m_oceanMaterial);

			if (!Scatterer.Instance.unifiedCameraMode)
				m_oceanMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Scatterer.Instance.bufferManager.depthTexture);
			
			//if (Core.Instance.oceanRefraction)
			m_oceanMaterial.SetTexture ("_BackgroundTexture", Scatterer.Instance.bufferManager.refractionTexture);
			m_oceanMaterial.renderQueue=2501;
			m_manager.GetSkyNode ().InitPostprocessMaterial (m_oceanMaterial);
			
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_Color_PROPERTY, m_oceanUpwellingColor);
			m_oceanMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_ScreenGridSize_PROPERTY, new Vector2 ((float)m_resolution / (float)Screen.width, (float)m_resolution / (float)Screen.height));
			//oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(radius+m_oceanLevel));
			m_oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(m_manager.GetRadius()));
			
			m_oceanMaterial.SetFloat (ShaderProperties._OceanAlpha_PROPERTY, oceanAlpha);
			m_oceanMaterial.SetFloat (ShaderProperties.alphaRadius_PROPERTY, alphaRadius);
			
			m_oceanMaterial.SetFloat ("refractionIndex", refractionIndex); //these don't need to be updated every frame
			m_oceanMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			m_oceanMaterial.SetFloat ("darknessDepth", darknessDepth);					
			m_oceanMaterial.SetTexture ("_BackgroundTexture", Scatterer.Instance.bufferManager.refractionTexture); //these don't need to be updated every frame

			float camerasOverlap = 0f;
			if (!Scatterer.Instance.unifiedCameraMode)
				camerasOverlap = Scatterer.Instance.nearCamera.farClipPlane - Scatterer.Instance.farCamera.nearClipPlane;

			m_oceanMaterial.SetFloat("_ScattererCameraOverlap",camerasOverlap);
		}
		
		void InitUnderwaterMaterial ()
		{
			underwaterMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/UnderwaterScatterProjector")]);
			m_manager.GetSkyNode ().InitPostprocessMaterial (underwaterMaterial);
			underwaterMaterial.renderQueue = 2502; //draw over fairings which is 2450 and over ocean which is 2501
			
			underwaterMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			underwaterMaterial.SetFloat ("darknessDepth", darknessDepth);
			underwaterMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			underwaterMaterial.SetFloat ("Rg",(float)m_manager.m_radius);

			Utils.EnableOrDisableShaderKeywords (underwaterMaterial, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);
		}

		void toggleUnderwaterMode()
		{
			if (underwaterMode) //switch to over water
			{
				underwaterProjector.setActivated(false);
				underwaterProjector.updateProjector ();
				m_oceanMaterial.EnableKeyword("UNDERWATER_OFF");
				m_oceanMaterial.DisableKeyword("UNDERWATER_ON");
				if (!ReferenceEquals(m_manager.GetSkyNode().localScatteringProjector,null))
					m_manager.GetSkyNode().localScatteringProjector.setUnderwater(false);

			}
			else   //switch to underwater 
			{
				underwaterProjector.setActivated(true);
				underwaterProjector.updateProjector ();
				m_oceanMaterial.EnableKeyword("UNDERWATER_ON");
				m_oceanMaterial.DisableKeyword("UNDERWATER_OFF");
				if (!ReferenceEquals(m_manager.GetSkyNode().localScatteringProjector,null))
					m_manager.GetSkyNode().localScatteringProjector.setUnderwater(true);
			}

			underwaterMode = !underwaterMode;
		}

		public virtual void Cleanup ()
		{
			Utils.LogDebug ("ocean node Cleanup");
			
			if (oceanCameraProjectionMatModifier)
			{
				oceanCameraProjectionMatModifier.OnDestroy ();
				Component.Destroy (oceanCameraProjectionMatModifier);
				UnityEngine.Object.Destroy (oceanCameraProjectionMatModifier);
			}
			
			for (int i = 0; i < numGrids; i++)
			{
				Destroy(waterGameObjects[i]);
				Component.Destroy(waterMeshFilters[i]);
				Component.Destroy(waterMeshRenderers[i]);
				
				UnityEngine.Object.Destroy (m_screenGrids [i]);
			}
			
			
			UnityEngine.Object.Destroy (m_oceanMaterial);
			UnityEngine.Object.Destroy (underwaterMaterial);
			
			if (underwaterDimmingHook)
				Component.Destroy (underwaterDimmingHook);
			
			if (!ReferenceEquals(null,underwaterProjector))
			{
				UnityEngine.Object.Destroy (underwaterProjector);
			}

			if (!ReferenceEquals(null,causticsShadowMaskModulator))
			{
				causticsShadowMaskModulator.OnDestroy();
				UnityEngine.Object.Destroy (causticsShadowMaskModulator);
			}
		}

		public void applyUnderwaterDimming () //called OnPostRender of scaledSpace Camera by hook, needs to be done before farCamera onPreCull where the color is set
		{
			if (!MapView.MapIsEnabled && isUnderwater)
			{
				float finalDim = 1f;
				if (Scatterer.Instance.mainSettings.underwaterLightDimming)
				{
					float underwaterDim = Mathf.Abs(Vector3.Distance (Scatterer.Instance.nearCamera.transform.position, m_manager.parentLocalTransform.position)-(float)m_manager.m_radius);
					underwaterDim = Mathf.Lerp(1.0f,0.0f,underwaterDim / darknessDepth);
					finalDim*=underwaterDim;
				}
				if (causticsShadowMaskModulator)
				{
					finalDim*=causticsUnderwaterLightBoost; //replace by caustics multiplier
				}
				Scatterer.Instance.sunlightModulatorInstance.modulateByAttenuation(finalDim);
			}	
		}

		public void saveToConfigNode ()
		{
			ConfigNode[] configNodeArray;
			bool found = false;
			
			configNodeArray = configUrl.config.GetNodes("Ocean");
			
			foreach(ConfigNode _cn in configNodeArray)
			{
				if (_cn.HasValue("name") && _cn.GetValue("name") == m_manager.parentCelestialBody.name)
				{
					ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
					_cn.ClearData();
					ConfigNode.Merge (_cn, cnTemp);
					_cn.name="Ocean";
					Utils.LogDebug("saving "+m_manager.parentCelestialBody.name+
					          " ocean config to: "+configUrl.parent.url);
					configUrl.parent.SaveConfigs ();
					found=true;
					break;
				}
			}
			
			if (!found)
			{
				Utils.LogDebug("couldn't find config file to save to");
			}
		}
		
		public void loadFromConfigNode ()
		{
			ConfigNode cnToLoad = new ConfigNode();
			ConfigNode[] configNodeArray;
			bool found = false;

			foreach (UrlDir.UrlConfig _url in Scatterer.Instance.planetsConfigsReader.oceanConfigs)
			{
				configNodeArray = _url.config.GetNodes("Ocean");
				
				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name") && _cn.GetValue("name") == m_manager.parentCelestialBody.name)
					{
						cnToLoad = _cn;
						configUrl = _url;
						found = true;
						break;
					}
				}
			}
			
			if (found)
			{
				Utils.LogDebug("Ocean config found for: "+m_manager.parentCelestialBody.name);
				
				ConfigNode.LoadObjectFromConfig (this, cnToLoad);		
			}
			else
			{
				Utils.LogDebug("Ocean config not found for: "+m_manager.parentCelestialBody.name);
				Utils.LogDebug("Removing ocean for "+m_manager.parentCelestialBody.name +" from planets list");
				
				(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == m_manager.parentCelestialBody.name)).hasOcean = false;
				
				this.Cleanup();
				UnityEngine.Object.Destroy (this);
			}
		}

		public void setWaterMeshrenderersEnabled (bool enabled)
		{
			for (int i=0; i < numGrids; i++)
			{
				waterMeshRenderers[i].enabled=enabled;
			}
		}
	}
}
