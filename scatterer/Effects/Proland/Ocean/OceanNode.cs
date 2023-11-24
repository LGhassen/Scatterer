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

namespace Scatterer
{
	//TODO: refactor and clean up this class
	public abstract class OceanNode: MonoBehaviour
	{
		public UrlDir.UrlConfig configUrl;
		
		public ProlandManager prolandManager;

		public Material m_oceanMaterial;
		OceanRenderingHook oceanRenderingHook;

		[Persistent]
		public Vector3 m_oceanUpwellingColor = new Vector3 (0.0039f, 0.0156f, 0.047f);

		[Persistent]
		public Vector3 m_UnderwaterColor = new Vector3 (0.1f, 0.75f, 0.8f);

		//Size of each grid in the projected grid. (number of pixels on screen)
		public int m_resolution = 4;

		[Persistent]
		public float offScreenVertexStretch = 1.25f;

		[Persistent]
		public float alphaRadius = 3000f;

		[Persistent]
		public float transparencyDepth = 60f;

		[Persistent]
		public float darknessDepth = 1000f;

		[Persistent]
		public float refractionIndex = 1.33f;

		[Persistent]
		public float skyReflectionStrength = 1f;

		public bool isUnderwater = false;
		bool underwaterMode = false;
		bool oceanDraw = true;

		public int numGrids;
		Mesh[] m_screenGrids;

		GameObject[] waterGameObjects;
		public MeshRenderer[] waterMeshRenderers;
		MeshFilter[] waterMeshFilters;
		
		public GenericLocalAtmosphereContainer underwaterScattering;
		public Material underwaterMaterial;

		public Vector3 offsetVector3{
			get {
				return offset.ToVector3();
			}
		}

		public double height = 0;
		public Vector3d2 m_Offset = Vector3d2.Zero();
		public Vector3d2 offset = Vector3d2.Zero(), ux=Vector3d2.Zero(), uy=Vector3d2.Zero(), uz=Vector3d2.Zero(), oo=Vector3d2.Zero();

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
		[Persistent]
		public float lightRaysStrength=1f;

		public CausticsShadowMaskModulate causticsShadowMaskModulator;
		public CausticsLightRaysRenderer causticsLightRaysRenderer;

		protected float waterHeightAtCameraPosition = 0f;

		public virtual void Init (ProlandManager manager)
		{
			m_resolution = Scatterer.Instance.mainSettings.oceanMeshResolution;
			prolandManager = manager;
			loadFromConfigNode ();

			InitOceanMaterial ();

			//Worth moving to projected Grid Class?
			CreateProjectedGridMeshes (true);

			oceanCameraProjectionMatModifier = waterGameObjects[0].AddComponent<OceanCameraUpdateHook>();
			oceanCameraProjectionMatModifier.oceanNode = this;

			oceanRenderingHook = waterGameObjects[0].AddComponent<OceanRenderingHook>();
			oceanRenderingHook.targetMaterial = m_oceanMaterial;
			oceanRenderingHook.targetRenderer = waterMeshRenderers [0];
			oceanRenderingHook.celestialBodyName = prolandManager.parentCelestialBody.name;

			DisableEffectsChecker disableEffectsChecker = waterGameObjects[0].AddComponent<DisableEffectsChecker>();
			disableEffectsChecker.manager = this.prolandManager;

			if (Scatterer.Instance.mainSettings.shadowsOnOcean || Scatterer.Instance.mainSettings.oceanLightRays)
			{
				ShadowMapRetrieveCommandBuffer retriever = prolandManager.mainSunLight.gameObject.GetComponent<ShadowMapRetrieveCommandBuffer>();
				if (!retriever)
					prolandManager.mainSunLight.gameObject.AddComponent (typeof(ShadowMapRetrieveCommandBuffer));
			}

			InitUnderwaterMaterial ();

			if (Scatterer.Instance.mainSettings.useDepthBufferMode)
				underwaterScattering = new ScreenSpaceScatteringContainer(underwaterMaterial,prolandManager.parentLocalTransform,(float)prolandManager.m_radius, prolandManager, false);	//this shouldn't need quarter res as it isn't expensive
			else
				underwaterScattering = new AtmosphereProjectorContainer(underwaterMaterial,prolandManager.parentLocalTransform,(float)prolandManager.m_radius, prolandManager);

			underwaterScattering.SetInScaledSpace(false);
			underwaterScattering.SetActivated(false);
			underwaterScattering.UpdateContainer ();

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
				causticsShadowMaskModulator = (CausticsShadowMaskModulate) prolandManager.mainSunLight.gameObject.AddComponent (typeof(CausticsShadowMaskModulate));
				if(!causticsShadowMaskModulator.Init(causticsTexturePath, causticsLayer1Scale, causticsLayer1Speed, causticsLayer2Scale, causticsLayer2Speed,
				                                     causticsMultiply, causticsMinBrightness, (float)manager.GetRadius(), causticsBlurDepth, prolandManager.mainSunLight))
				{
					UnityEngine.Object.DestroyImmediate (causticsShadowMaskModulator);
					causticsShadowMaskModulator = null;
				}

				if (Scatterer.Instance.mainSettings.oceanLightRays)
				{
					causticsLightRaysRenderer = (CausticsLightRaysRenderer) waterGameObjects[0].AddComponent<CausticsLightRaysRenderer>();
					if (!causticsLightRaysRenderer.Init(causticsTexturePath, causticsLayer1Scale, causticsLayer1Speed, causticsLayer2Scale, causticsLayer2Speed,
					                                    causticsMultiply, causticsMinBrightness, (float)manager.GetRadius(), causticsBlurDepth, this, lightRaysStrength))
					{
						UnityEngine.Object.DestroyImmediate (causticsLightRaysRenderer);
						causticsLightRaysRenderer = null;
					}
				}
			}
		}	

		public virtual void UpdateNode ()
		{
			oceanDraw = !MapView.MapIsEnabled && !prolandManager.skyNode.inScaledSpace;

			foreach (MeshRenderer _mr in waterMeshRenderers)
			{
				_mr.enabled = oceanDraw;
			}

			isUnderwater = height < waterHeightAtCameraPosition;

			underwaterScattering.SetActivated(isUnderwater);

			if (underwaterMode ^ isUnderwater)
			{
				toggleUnderwaterMode();
			}

			if (causticsShadowMaskModulator)
			{
				causticsShadowMaskModulator.isEnabled = oceanDraw && (prolandManager.GetSkyNode().altitude < 6000f);
				causticsShadowMaskModulator.UpdateCaustics ();
			}			
		}

		public void updateNonCameraSpecificUniforms (Material oceanMaterial)
		{
			prolandManager.GetSkyNode ().SetOceanUniforms (oceanMaterial);

			if (underwaterMode)
			{
				prolandManager.GetSkyNode ().UpdatePostProcessMaterialUniforms (underwaterMaterial);
			}

			planetOpacity = 1f - prolandManager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat (ShaderProperties._PlanetOpacity_PROPERTY);
			m_oceanMaterial.SetFloat (ShaderProperties._PlanetOpacity_PROPERTY, planetOpacity);

			m_oceanMaterial.SetInt (ShaderProperties._ZwriteVariable_PROPERTY, Scatterer.Instance.mainSettings.useDepthBufferMode || (planetOpacity == 1) ? 1 : 0); //if planetOpacity!=1, ie fading out the sea, disable scattering on it and enable the projector scattering, for the projector scattering to work need to disable zwrite
		}

		public void OnPreCull()
		{
			if (!MapView.MapIsEnabled && Scatterer.Instance.nearCamera && prolandManager.skyNode.simulateOceanInteraction)
			{
				updateNonCameraSpecificUniforms(m_oceanMaterial);
			}
		}

		void CreateProjectedGridMeshes (bool use32BitIndexMesh)
		{
			//Create the projected grid. The resolution is the size in pixels
			//of each square in the grid. If not using 32-bit index meshes, the verts of
			//the mesh will exceed the max verts for a mesh in Unity. In this case
			//split the mesh up into smaller meshes.
			m_resolution = Mathf.Max (1, m_resolution);
			//The number of squares in the grid on the x and y axis
			int NX = Screen.width / m_resolution;
			int NY = Screen.height / m_resolution;
			numGrids = 1;

			const int MAX_VERTS = 65000; //The number of meshes need to make a grid of this resolution, if not using 32-bit index meshes

			if (!use32BitIndexMesh && (NX * NY > MAX_VERTS))
			{
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
				if (use32BitIndexMesh)
					m_screenGrids [i] = MeshFactory.MakePlane32BitIndexFormat (NX, NY, MeshFactory.PLANE.XY, false, true, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				else
					m_screenGrids [i] = MeshFactory.MakePlane (NX, NY, MeshFactory.PLANE.XY, false, true, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				m_screenGrids [i].bounds = new Bounds (Vector3.zero, new Vector3 (1e8f, 1e8f, 1e8f));
				waterGameObjects [i] = new GameObject ();
				waterGameObjects [i].transform.position = prolandManager.parentCelestialBody.transform.position;
				waterGameObjects [i].transform.parent = prolandManager.parentCelestialBody.transform;
				//might be redundant
				waterMeshFilters [i] = waterGameObjects [i].AddComponent<MeshFilter> ();
				waterMeshFilters [i].mesh.Clear ();
				waterMeshFilters [i].mesh = m_screenGrids [i];
				waterGameObjects [i].layer = 15;
				waterMeshRenderers [i] = waterGameObjects [i].AddComponent<MeshRenderer> ();

				if (Scatterer.Instance.mainSettings.oceanPixelLights)
				{
					m_oceanMaterial.SetPass (1); //Disable the main pass so we can render it with our commandbuffer. Pixel light passes render after scattering in depth buffer mode, and before scattering in projector mode
					waterMeshRenderers [i].sharedMaterial = m_oceanMaterial;
					waterMeshRenderers [i].material = m_oceanMaterial;
				}
				else
				{
					waterMeshRenderers [i].material = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
				}
				waterMeshRenderers [i].receiveShadows = Scatterer.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable);
				waterMeshRenderers [i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				waterMeshRenderers [i].enabled = true;
			}
		}

		void InitOceanMaterial ()
		{
			m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/OceanWhiteCaps")]);

			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "SKY_REFLECTIONS_ON", "SKY_REFLECTIONS_OFF", Scatterer.Instance.mainSettings.oceanSkyReflections);
			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "PLANETSHINE_ON", "PLANETSHINE_OFF", (prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine);

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

//			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "SCATTERER_MERGED_DEPTH_OFF", "SCATTERER_MERGED_DEPTH_ON", Scatterer.Instance.unifiedCameraMode);

			if (Scatterer.Instance.mainSettings.useDepthBufferMode)
			{
				m_oceanMaterial.EnableKeyword("DEPTH_BUFFER_MODE_ON");
				m_oceanMaterial.DisableKeyword("PROJECTOR_MODE");
				m_oceanMaterial.DisableKeyword("PROJECTOR_MODE_GODRAYS");
			}
			else
			{
				m_oceanMaterial.DisableKeyword("DEPTH_BUFFER_MODE_ON");
				if (prolandManager.skyNode.legacyGodraysRenderer)
				{
					m_oceanMaterial.EnableKeyword("PROJECTOR_MODE_GODRAYS");
					m_oceanMaterial.DisableKeyword("PROJECTOR_MODE");
				}
				else
				{
					m_oceanMaterial.EnableKeyword("PROJECTOR_MODE");
					m_oceanMaterial.DisableKeyword("PROJECTOR_MODE_GODRAYS");
				}
			}

			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "DEPTH_BUFFER_MODE_ON", "DEPTH_BUFFER_MODE_OFF", Scatterer.Instance.mainSettings.useDepthBufferMode);


			m_oceanMaterial.SetOverrideTag ("IgnoreProjector", "True");
			
			prolandManager.GetSkyNode ().InitUniforms (m_oceanMaterial);

			if (!Scatterer.Instance.unifiedCameraMode)
				m_oceanMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Scatterer.Instance.bufferManager.depthTexture);

			m_oceanMaterial.renderQueue=2502;
			prolandManager.GetSkyNode ().InitPostprocessMaterialUniforms (m_oceanMaterial);
			
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_Color_PROPERTY, m_oceanUpwellingColor);
			m_oceanMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_ScreenGridSize_PROPERTY, new Vector2 ((float)m_resolution / (float)Screen.width, (float)m_resolution / (float)Screen.height));

			//oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(radius+m_oceanLevel));
			m_oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(prolandManager.GetRadius()));

			m_oceanMaterial.SetFloat (ShaderProperties.alphaRadius_PROPERTY, alphaRadius);

			m_oceanMaterial.SetFloat ("skyReflectionStrength", skyReflectionStrength);
			m_oceanMaterial.SetFloat ("refractionIndex", refractionIndex); //these don't need to be updated every frame
			m_oceanMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			m_oceanMaterial.SetFloat ("darknessDepth", darknessDepth);

			float camerasOverlap = 0f;
			if (!Scatterer.Instance.unifiedCameraMode)
				camerasOverlap = Scatterer.Instance.nearCamera.farClipPlane - Scatterer.Instance.farCamera.nearClipPlane;

			m_oceanMaterial.SetFloat("_ScattererCameraOverlap",camerasOverlap);

			m_oceanMaterial.SetFloat ("offScreenVertexStretch", offScreenVertexStretch);
		}
		
		void InitUnderwaterMaterial ()
		{
			if (Scatterer.Instance.mainSettings.useDepthBufferMode)
				underwaterMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/UnderwaterScatterDepthBuffer")]);
			else
				underwaterMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/UnderwaterScatterProjector")]);

			prolandManager.GetSkyNode ().InitPostprocessMaterialUniforms (underwaterMaterial);
			underwaterMaterial.renderQueue = 2502; //draw over fairings which is 2450 and over ocean which is 2501
			
			underwaterMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			underwaterMaterial.SetFloat ("darknessDepth", darknessDepth);
			underwaterMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			underwaterMaterial.SetFloat ("Rg",(float)prolandManager.m_radius);

			Utils.EnableOrDisableShaderKeywords (underwaterMaterial, "DITHERING_ON", "DITHERING_OFF", Scatterer.Instance.mainSettings.useDithering);
		}

		void toggleUnderwaterMode()
		{
			if (underwaterMode) //switch to over water
			{
				underwaterScattering.SetActivated(false);
				underwaterScattering.UpdateContainer ();
				m_oceanMaterial.EnableKeyword("UNDERWATER_OFF");
				m_oceanMaterial.DisableKeyword("UNDERWATER_ON");
				prolandManager.GetSkyNode().SetUnderwater(false);
			}
			else   //switch to underwater 
			{
				underwaterScattering.SetActivated(true);
				underwaterScattering.UpdateContainer ();
				m_oceanMaterial.EnableKeyword("UNDERWATER_ON");
				m_oceanMaterial.DisableKeyword("UNDERWATER_OFF");
				prolandManager.GetSkyNode().SetUnderwater(true);
			}

			underwaterMode = !underwaterMode;
		}

		public virtual void OnDestroy ()
		{	
			if (oceanCameraProjectionMatModifier)
			{
				Component.Destroy (oceanCameraProjectionMatModifier);
				UnityEngine.Object.Destroy (oceanCameraProjectionMatModifier);
			}
			
			for (int i = 0; i < numGrids; i++)
			{
				DestroyImmediate(waterGameObjects[i]);
				Component.DestroyImmediate(waterMeshFilters[i]);
				Component.DestroyImmediate(waterMeshRenderers[i]);
				
				UnityEngine.Object.DestroyImmediate(m_screenGrids [i]);
			}
			
			
			UnityEngine.Object.DestroyImmediate(m_oceanMaterial);
			UnityEngine.Object.DestroyImmediate(underwaterMaterial);
			
			if (underwaterDimmingHook)
				Component.DestroyImmediate(underwaterDimmingHook);
			
			if (underwaterScattering!=null)
			{
				underwaterScattering.Cleanup();
			}

			if (causticsShadowMaskModulator)
			{
				UnityEngine.Object.DestroyImmediate (causticsShadowMaskModulator);
			}

			if (causticsLightRaysRenderer)
			{
				UnityEngine.Object.DestroyImmediate(causticsLightRaysRenderer);
			}

			if (oceanRenderingHook)
			{
				Component.DestroyImmediate(oceanRenderingHook);
			}
		}

		public void applyUnderwaterDimming () //called OnPostRender of scaledSpace Camera by hook, needs to be done before farCamera onPreCull where the color is set
		{
			if (!MapView.MapIsEnabled && isUnderwater)
			{
				float finalDim = 1f;
				if (Scatterer.Instance.mainSettings.underwaterLightDimming)
				{
					float underwaterDim = Mathf.Abs(Vector3.Distance (Scatterer.Instance.nearCamera.transform.position, prolandManager.parentLocalTransform.position)-(float)prolandManager.m_radius);
					underwaterDim = Mathf.Lerp(1.0f,0.0f,underwaterDim / darknessDepth);
					finalDim*=underwaterDim;
				}
				if (causticsShadowMaskModulator)
				{
					finalDim*=causticsUnderwaterLightBoost; //replace by caustics multiplier
				}
				Scatterer.Instance.sunlightModulatorsManagerInstance.ModulateByAttenuation(prolandManager.mainSunLight, finalDim);
			}	
		}

		public void saveToConfigNode ()
		{
			ConfigNode[] configNodeArray;
			bool found = false;
			
			configNodeArray = configUrl.config.GetNodes("Ocean");
			
			foreach(ConfigNode _cn in configNodeArray)
			{
				if (_cn.HasValue("name") && _cn.GetValue("name") == prolandManager.parentCelestialBody.name)
				{
					ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
					_cn.ClearData();
					ConfigNode.Merge (_cn, cnTemp);
					_cn.name="Ocean";
					Utils.LogDebug("saving "+prolandManager.parentCelestialBody.name+
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
					if (_cn.HasValue("name") && _cn.GetValue("name") == prolandManager.parentCelestialBody.name)
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
				Utils.LogDebug("Ocean config found for: "+prolandManager.parentCelestialBody.name);
				
				ConfigNode.LoadObjectFromConfig (this, cnToLoad);		
			}
			else
			{
				Utils.LogDebug("Ocean config not found for: "+prolandManager.parentCelestialBody.name);
				Utils.LogDebug("Removing ocean for "+prolandManager.parentCelestialBody.name +" from planets list");
				
				(Scatterer.Instance.planetsConfigsReader.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == prolandManager.parentCelestialBody.name)).hasOcean = false;
				
				UnityEngine.Component.DestroyImmediate (this);
			}
		}

		public void setWaterMeshrenderersEnabled (bool enabled)
		{
			for (int i=0; i < numGrids; i++)
			{
				waterMeshRenderers[i].enabled=enabled && oceanDraw;
			}
		}
	}
}
