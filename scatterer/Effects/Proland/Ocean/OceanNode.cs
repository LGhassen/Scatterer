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

namespace Scatterer
{
    //TODO: refactor and clean up this class
    public abstract class OceanNode: MonoBehaviour
    {
        public UrlDir.UrlConfig configUrl;
        
        public ProlandManager prolandManager;

        public Material m_oceanMaterial;
        OceanRenderingHook oceanRenderingHook;

        // Size of each grid in the projected grid. (number of pixels on screen)
        private int projectedGridPixelSize = 4;

        private int vertCountX, vertCountY;

        public int VertCountX { get => vertCountX; }
        public int VertCountY { get => vertCountY; }


        [Persistent] public float offScreenVertexStretch = 1.25f;
        [Persistent] public float alphaRadius = 3000f;
        [Persistent] public float transparencyDepth = 60f;
        [Persistent] public float darknessDepth = 1000f;
        [Persistent] public float refractionIndex = 1.33f;
        [Persistent] public float skyReflectionStrength = 1f;
        [Persistent] public Vector3 m_oceanUpwellingColor = new Vector3(0.0039f, 0.0156f, 0.047f);
        [Persistent] public Vector3 m_UnderwaterColor = new Vector3(0.1f, 0.75f, 0.8f);

        public bool isUnderwater = false;
        bool underwaterMode = false;
        bool drawOcean = true;

        Mesh oceanScreenGrid;

        GameObject waterGameObject;
        public MeshRenderer waterMeshRenderer;
        
        public GenericLocalAtmosphereContainer underwaterScattering;
        public Material underwaterMaterial;

        public Vector3 OffsetVector3
        {
            get
            {
                return offset.ToVector3();
            }
        }

        public double height = 0;
        public Vector3d2 m_Offset = Vector3d2.Zero();
        public Vector3d2 offset = Vector3d2.Zero(), ux=Vector3d2.Zero(), uy=Vector3d2.Zero(), uz=Vector3d2.Zero(), oo=Vector3d2.Zero();

        OceanCameraUpdateHook oceanCameraPropertiesUpdater;
        UnderwaterDimmingHook underwaterDimmingHook;

        public float planetOpacity=1f; //planetOpacity to fade out the ocean when PQS is fading out

        //Concrete classes must provide a function that returns the
        //variance of the waves need for the BRDF rendering of waves
        public abstract float GetMaxSlopeVariance ();

        //caustics
        [Persistent] public string causticsTexturePath="";
        [Persistent] public Vector2 causticsLayer1Scale;
        [Persistent] public Vector2 causticsLayer1Speed;
        [Persistent] public Vector2 causticsLayer2Scale;
        [Persistent] public Vector2 causticsLayer2Speed;
        [Persistent] public float causticsMultiply;
        [Persistent] public float causticsUnderwaterLightBoost;
        [Persistent] public float causticsMinBrightness;
        [Persistent] public float causticsBlurDepth;
        [Persistent] public float lightRaysStrength=1f;

        public CausticsShadowMaskModulate causticsShadowMaskModulator;
        public CausticsLightRaysRenderer causticsLightRaysRenderer;

        protected float waterHeightAtCameraPosition = 0f;

        public virtual void Init (ProlandManager manager)
        {
            projectedGridPixelSize = Scatterer.Instance.mainSettings.oceanMeshResolution;
            prolandManager = manager;
            
            LoadFromConfigNode();
            InitOceanMaterial();
            CreateProjectedGridMesh();

            oceanCameraPropertiesUpdater = waterGameObject.AddComponent<OceanCameraUpdateHook>(); // Why not just make this a general script you add to the waterGameObject?
            oceanCameraPropertiesUpdater.oceanNode = this;

            oceanRenderingHook = waterGameObject.AddComponent<OceanRenderingHook>(); // Why not merge this with the above?
            oceanRenderingHook.Init(m_oceanMaterial, waterMeshRenderer, vertCountX, vertCountY, oceanScreenGrid);

            DisableEffectsChecker disableEffectsChecker = waterGameObject.AddComponent<DisableEffectsChecker>(); 
            disableEffectsChecker.manager = this.prolandManager;

            // I think this shouldn't be needed with the commandBuffer rendering method, but I'll test it to be sure
            if (Scatterer.Instance.mainSettings.shadowsOnOcean || Scatterer.Instance.mainSettings.oceanLightRays)
            {
                ShadowMapRetrieveCommandBuffer retriever = prolandManager.mainSunLight.gameObject.GetComponent<ShadowMapRetrieveCommandBuffer>();
                if (!retriever)
                    prolandManager.mainSunLight.gameObject.AddComponent (typeof(ShadowMapRetrieveCommandBuffer));
            }

            InitUnderwaterMaterial ();

            underwaterScattering = new ScreenSpaceScatteringContainer(underwaterMaterial,prolandManager.parentLocalTransform,(float)prolandManager.m_radius, prolandManager, false);    //this shouldn't need quarter res as it isn't expensive
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
                    UnityEngine.Object.Destroy (causticsShadowMaskModulator);
                    causticsShadowMaskModulator = null;
                }

                if (Scatterer.Instance.mainSettings.oceanLightRays)
                {
                    causticsLightRaysRenderer = (CausticsLightRaysRenderer) waterGameObject.AddComponent<CausticsLightRaysRenderer>();
                    if (!causticsLightRaysRenderer.Init(causticsTexturePath, causticsLayer1Scale, causticsLayer1Speed, causticsLayer2Scale, causticsLayer2Speed,
                                                        causticsMultiply, causticsMinBrightness, (float)manager.GetRadius(), causticsBlurDepth, this, lightRaysStrength))
                    {
                        UnityEngine.Object.Destroy (causticsLightRaysRenderer);
                        causticsLightRaysRenderer = null;
                    }
                }
            }
        }    

        public virtual void UpdateNode ()
        {
            drawOcean = !MapView.MapIsEnabled && !prolandManager.skyNode.inScaledSpace;

            waterMeshRenderer.enabled = drawOcean;

            isUnderwater = height < waterHeightAtCameraPosition;

            underwaterScattering.SetActivated(isUnderwater);

            if (underwaterMode ^ isUnderwater) // why do you write code like this
            {
                toggleUnderwaterMode();
            }

            if (causticsShadowMaskModulator)
            {
                causticsShadowMaskModulator.isEnabled = drawOcean && (prolandManager.GetSkyNode().altitude < 6000f);
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

            m_oceanMaterial.SetInt (ShaderProperties._ZwriteVariable_PROPERTY, 1);
        }

        public void OnPreCull()
        {
            if (!MapView.MapIsEnabled && Scatterer.Instance.nearCamera && prolandManager.skyNode.simulateOceanInteraction)
            {
                updateNonCameraSpecificUniforms(m_oceanMaterial);
            }
        }

        void CreateProjectedGridMesh ()
        {
            
            projectedGridPixelSize = Mathf.Max (1, projectedGridPixelSize);
            
            //The number of squares in the grid on the x and y axis
            vertCountX = Screen.width / projectedGridPixelSize;
            vertCountY = Screen.height / projectedGridPixelSize;

            const int maxVertCountIn16BitIndexMesh = 65000; //The number of meshes needed to make a grid of this resolution, if not using 32-bit index meshes

            waterGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            waterGameObject.name = "Scatterer ocean water display";
            waterGameObject.transform.position = prolandManager.parentCelestialBody.transform.position;
            waterGameObject.transform.parent = prolandManager.parentCelestialBody.transform;
            waterGameObject.layer = 15;

            var collider =  waterGameObject.GetComponent<Collider>();

            if (collider != null)
            {
                Destroy(collider);
            }

            // Make the mesh. The end product will be a grid of verts that covers
            // the screen on the x and y axis with the z depth at 0. This grid is then
            // projected as the ocean by the shader
            if (vertCountX * vertCountY > maxVertCountIn16BitIndexMesh)
            { 
                oceanScreenGrid = MeshFactory.MakePlane32BitIndexFormat (vertCountX, vertCountY, MeshFactory.PLANE.XY, false, true, 0f, 1.0f);
            }
            else
            { 
                oceanScreenGrid = MeshFactory.MakePlane (vertCountX, vertCountY, MeshFactory.PLANE.XY, false, true, 0f, 1f);
            }

            oceanScreenGrid.bounds = new Bounds (Vector3.zero, new Vector3 (1e8f, 1e8f, 1e8f));

            var waterMeshFilter = waterGameObject.GetComponent<MeshFilter> ();
            waterMeshFilter.mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));

            waterMeshRenderer = waterGameObject.GetComponent<MeshRenderer> ();

            if (Scatterer.Instance.mainSettings.oceanPixelLights)
            {
                //m_oceanMaterial.SetPass (1); // This doesn't work it seems? check if main ocean is rendered twice, because it seems that all pases after 1 still render

                // These work perfectly
                m_oceanMaterial.SetShaderPassEnabled("MainPass", false);
                m_oceanMaterial.SetShaderPassEnabled("GbufferWrite", false);
                m_oceanMaterial.SetShaderPassEnabled("VertexPositions", false);
                m_oceanMaterial.SetShaderPassEnabled("GbufferMainLightingPass", false);
                m_oceanMaterial.SetShaderPassEnabled("DeferredReflections", false);

                waterMeshRenderer.sharedMaterial = m_oceanMaterial;
                waterMeshRenderer.material = m_oceanMaterial;
            }
            else
            {
                waterMeshRenderer.material = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
            }

            waterMeshRenderer.receiveShadows = Scatterer.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable);
            waterMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            waterMeshRenderer.enabled = true;
        }

        void InitOceanMaterial ()
        {
            //m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/OceanWhiteCaps")]);
            m_oceanMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/OceanWhiteCapsDeferred")]);

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

            m_oceanMaterial.EnableKeyword("DEPTH_BUFFER_MODE_ON");
            m_oceanMaterial.DisableKeyword("PROJECTOR_MODE");
            m_oceanMaterial.DisableKeyword("PROJECTOR_MODE_GODRAYS");

            Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "DEPTH_BUFFER_MODE_ON", "DEPTH_BUFFER_MODE_OFF", true);

            m_oceanMaterial.SetOverrideTag ("IgnoreProjector", "True");

            if (!Scatterer.Instance.unifiedCameraMode && DepthToDistanceCommandBuffer.RenderTexture != null)
                m_oceanMaterial.SetTexture(ShaderProperties._customDepthTexture_PROPERTY, DepthToDistanceCommandBuffer.RenderTexture);

            m_oceanMaterial.renderQueue=2502;
            
            m_oceanMaterial.SetVector (ShaderProperties._Ocean_Color_PROPERTY, m_oceanUpwellingColor);
            m_oceanMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
            m_oceanMaterial.SetVector (ShaderProperties._Ocean_ScreenGridSize_PROPERTY, new Vector2 ((float)projectedGridPixelSize / (float)Screen.width, (float)projectedGridPixelSize / (float)Screen.height));

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
            underwaterMaterial = new Material (ShaderReplacer.Instance.LoadedShaders [("Scatterer/UnderwaterScatterDepthBuffer")]);
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
            if (oceanCameraPropertiesUpdater)
            {
                Component.Destroy (oceanCameraPropertiesUpdater);
                UnityEngine.Object.Destroy (oceanCameraPropertiesUpdater);
            }
            

            Destroy(waterGameObject);
                
            UnityEngine.Object.Destroy(oceanScreenGrid);
            
            
            UnityEngine.Object.Destroy(m_oceanMaterial);
            UnityEngine.Object.Destroy(underwaterMaterial);
            
            if (underwaterDimmingHook)
                Component.Destroy(underwaterDimmingHook);
            
            if (underwaterScattering!=null)
            {
                underwaterScattering.Cleanup();
            }

            if (causticsShadowMaskModulator)
            {
                UnityEngine.Object.Destroy (causticsShadowMaskModulator);
            }

            if (causticsLightRaysRenderer)
            {
                UnityEngine.Object.Destroy(causticsLightRaysRenderer);
            }

            if (oceanRenderingHook)
            {
                Component.Destroy(oceanRenderingHook);
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
                SunlightModulatorsManager.Instance.ModulateByAttenuation(prolandManager.mainSunLight, finalDim);
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
        
        public void LoadFromConfigNode ()
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
                
                UnityEngine.Component.Destroy (this);
            }
        }

        public void setWaterMeshrenderersEnabled (bool enabled)
        {
            waterMeshRenderer.enabled = enabled && drawOcean;
        }
    }
}
