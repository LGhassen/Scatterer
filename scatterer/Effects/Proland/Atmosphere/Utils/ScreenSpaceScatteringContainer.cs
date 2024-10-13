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
    public class ScreenSpaceScattering : MonoBehaviour
    {
        public Material material;
        
        MeshRenderer scatteringMR;
        public bool hasOcean = false;
        bool quarterRes = false;

        //Dictionary to check if we added the ScatteringCommandBuffer to the camera
        private Dictionary<Camera,ScatteringCommandBuffer> cameraToScatteringCommandBuffer = new Dictionary<Camera,ScatteringCommandBuffer>();
        
        public void Init(bool inQuarterRes)
        {
            scatteringMR = gameObject.GetComponent<MeshRenderer>();
            material.SetOverrideTag("IgnoreProjector", "True");
            scatteringMR.sharedMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
            
            scatteringMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            scatteringMR.receiveShadows = false;
            scatteringMR.enabled = true;

            GetComponent<MeshFilter>().mesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));
            
            gameObject.layer = (int) 15;

            quarterRes = inQuarterRes;
        }
        
        public void SetActive(bool active)
        {
            scatteringMR.enabled = active;
        }

        void OnWillRenderObject()
        {
            if (scatteringMR.enabled && material != null)
            {
                Camera cam = Camera.current;
                
                if (!cam)
                    return;

                if (cameraToScatteringCommandBuffer.ContainsKey (cam))
                {
                    if (cameraToScatteringCommandBuffer[cam] != null)
                    {
                        cameraToScatteringCommandBuffer[cam].EnableForThisFrame();
                    }
                }
                else
                {
                    // Add null to the cameras we don't want to render on so we don't do a string compare every time, including reflection probe cameras
                    // DepthCamera and NearCamera render Kerbal portraits (also using dual camera system), they are disabled because of an issue with the screen copy
                    // and being mostly unnoticeable
                    if ((cam.name == "TRReflectionCamera") || (cam.name=="Reflection Probes Camera") || (cam.name == "NearCamera") || (cam.name == "DepthCamera"))
                    {
                        cameraToScatteringCommandBuffer[cam] = null;
                    }
                    else
                    {
                        ScatteringCommandBuffer scatteringCommandBuffer = (ScatteringCommandBuffer) cam.gameObject.AddComponent(typeof(ScatteringCommandBuffer));
                        scatteringCommandBuffer.targetRenderer = scatteringMR;
                        scatteringCommandBuffer.targetMaterial = material;
                        scatteringCommandBuffer.Initialize(hasOcean, quarterRes);
                        scatteringCommandBuffer.EnableForThisFrame();
                        
                        cameraToScatteringCommandBuffer[cam] = scatteringCommandBuffer;
                    }
                }
            }
        }

        public void OnDestroy()
        {
            foreach (var scatteringCommandBuffer in cameraToScatteringCommandBuffer.Values)
            {
                if (scatteringCommandBuffer)
                {
                    Component.Destroy(scatteringCommandBuffer);
                }
            }
        }
    }

    public class ScatteringCommandBuffer : MonoBehaviour
    {
        bool renderingEnabled = false;
        bool screenShotModeEnabledOnLastFrame = false;
        
        public MeshRenderer targetRenderer;
        public Material targetMaterial;
        
        private Camera targetCamera;
        private CommandBuffer rendererCommandBuffer, quarterResRendererCommandBuffer;

        //downscaledRenderTexture0 will hold scattering.RGB in RGB channels and extinction.R in alpha, downscaledRenderTexture1 will hold extinction.GB in RG
        private RenderTexture downscaledRenderTexture0, downscaledRenderTexture1, downscaledDepthRenderTexture;
        private Material downscaleDepthMaterial, compositeScatteringMaterial;

        int scatteringScreenCopyTextureId = Shader.PropertyToID("scatteringScreenCopy");

        public void Initialize(bool inHasOcean, bool quarterRes)
        {
            targetCamera = GetComponent<Camera>();

            targetCamera.depthTextureMode = targetCamera.depthTextureMode | DepthTextureMode.Depth;

            bool hdrEnabled = targetCamera.allowHDR;

            targetMaterial.SetInt(ShaderProperties._ZwriteVariable_PROPERTY, inHasOcean ? 1 : 0);

            //if no depth downscaling, render directly to screen
            rendererCommandBuffer = new CommandBuffer();
            rendererCommandBuffer.name = "Scatterer screen-space scattering CommandBuffer";

            CreateFullResCommandBuffer(hdrEnabled, false);

            if (quarterRes)
            {
                CreateRenderTextures();

                downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/DownscaleDepth")]);
                compositeScatteringMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/CompositeDownscaledScattering")]);
                Utils.EnableOrDisableShaderKeywords(compositeScatteringMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", inHasOcean);
                compositeScatteringMaterial.SetInt(ShaderProperties._ZwriteVariable_PROPERTY, inHasOcean ? 1 : 0);
                Utils.SetToneMapping(compositeScatteringMaterial);

                quarterResRendererCommandBuffer = new CommandBuffer();
                quarterResRendererCommandBuffer.name = "Scatterer 1/4 res screen-space scattering CommandBuffer";

                CreateQuarterResCommandBuffer(inHasOcean, hdrEnabled, scatteringScreenCopyTextureId);
            }
        }

        private void CreateQuarterResCommandBuffer(bool inHasOcean, bool hdrEnabled, int scatteringScreenCopyTextureId)
        {
            quarterResRendererCommandBuffer.Clear();
            quarterResRendererCommandBuffer.GetTemporaryRT(scatteringScreenCopyTextureId, targetCamera.pixelWidth, targetCamera.pixelHeight, 0, FilterMode.Bilinear, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
            quarterResRendererCommandBuffer.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), scatteringScreenCopyTextureId);
            quarterResRendererCommandBuffer.SetGlobalTexture("ScattererScreenCopy", scatteringScreenCopyTextureId);

            // Downscale depth
            if (inHasOcean)
                quarterResRendererCommandBuffer.Blit(null, downscaledDepthRenderTexture, downscaleDepthMaterial, 1);        //ocean depth buffer downsample
            else
                quarterResRendererCommandBuffer.Blit(null, downscaledDepthRenderTexture, downscaleDepthMaterial, 0);        //default depth buffer downsample

            quarterResRendererCommandBuffer.SetGlobalTexture("ScattererDownscaledScatteringDepth", downscaledDepthRenderTexture);

            // Render 1/4 res scattering+extinction to 1 RGBA + 1 RG texture
            RenderTargetIdentifier[] downscaledRenderTextures = { new RenderTargetIdentifier(downscaledRenderTexture0), new RenderTargetIdentifier(downscaledRenderTexture1) };
            quarterResRendererCommandBuffer.SetRenderTarget(downscaledRenderTextures, downscaledRenderTexture0.depthBuffer);
            quarterResRendererCommandBuffer.ClearRenderTarget(false, true, Color.clear);
            quarterResRendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 1); //pass 1 render to textures

            quarterResRendererCommandBuffer.ReleaseTemporaryRT(scatteringScreenCopyTextureId);

            quarterResRendererCommandBuffer.SetGlobalTexture("DownscaledScattering0", downscaledRenderTexture0);
            quarterResRendererCommandBuffer.SetGlobalTexture("DownscaledScattering1", downscaledRenderTexture1);

            // Render quad to screen that reads from downscaled textures and full res depth and performs near-depth upsampling
            quarterResRendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            quarterResRendererCommandBuffer.DrawRenderer(targetRenderer, compositeScatteringMaterial, 0, 0);
        }

        private void CreateFullResCommandBuffer(bool hdrEnabled, bool screenShotModeEnabled)
        {
            int width = targetCamera.pixelWidth;
            int height = targetCamera.pixelHeight;

            if (screenShotModeEnabled)
            {
                int superSizingFactor = Mathf.Max(GameSettings.SCREENSHOT_SUPERSIZE, 1);
                width = width * superSizingFactor;
                height = height * superSizingFactor;
            }

            rendererCommandBuffer.Clear();
            rendererCommandBuffer.GetTemporaryRT(scatteringScreenCopyTextureId, width, height, 0, FilterMode.Bilinear, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
            rendererCommandBuffer.Blit(new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget), scatteringScreenCopyTextureId);
            rendererCommandBuffer.SetGlobalTexture("ScattererScreenCopy", scatteringScreenCopyTextureId);

            rendererCommandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            rendererCommandBuffer.DrawRenderer(targetRenderer, targetMaterial, 0, 0); // pass 0 render to screen

            rendererCommandBuffer.ReleaseTemporaryRT(scatteringScreenCopyTextureId);
        }

        void CreateRenderTextures ()
        {
            int width, height;

            if (targetCamera.activeTexture)
            {
                width = targetCamera.activeTexture.width / 2;
                height = targetCamera.activeTexture.height / 2;
            }
            else
            {
                width = Screen.width / 2;
                height = Screen.height / 2;
            }

            bool hdrEnabled = targetCamera.allowHDR;

            downscaledRenderTexture0 = new RenderTexture (width, height, 0, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
            downscaledRenderTexture0.anisoLevel = 1;
            downscaledRenderTexture0.antiAliasing = 1;
            downscaledRenderTexture0.volumeDepth = 0;
            downscaledRenderTexture0.useMipMap = false;
            downscaledRenderTexture0.autoGenerateMips = false;
            downscaledRenderTexture0.filterMode = FilterMode.Point;
            downscaledRenderTexture0.Create ();

            downscaledRenderTexture1 = new RenderTexture (width, height, 0, hdrEnabled ? RenderTextureFormat.RGHalf : RenderTextureFormat.RG16);
            downscaledRenderTexture1.anisoLevel = 1;
            downscaledRenderTexture1.antiAliasing = 1;
            downscaledRenderTexture1.volumeDepth = 0;
            downscaledRenderTexture1.useMipMap = false;
            downscaledRenderTexture1.autoGenerateMips = false;
            downscaledRenderTexture1.filterMode = FilterMode.Point;
            downscaledRenderTexture1.Create ();

            downscaledDepthRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
            downscaledDepthRenderTexture.anisoLevel = 1;
            downscaledDepthRenderTexture.antiAliasing = 1;
            downscaledDepthRenderTexture.volumeDepth = 0;
            downscaledDepthRenderTexture.useMipMap = false;
            downscaledDepthRenderTexture.autoGenerateMips = false;
            downscaledDepthRenderTexture.filterMode = FilterMode.Point;
            downscaledDepthRenderTexture.Create();
        }
        
        public void EnableForThisFrame()
        {
            if (!renderingEnabled)
            {
                bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);

                if (quarterResRendererCommandBuffer != null && !screenShotModeEnabled)
                {
                    targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, quarterResRendererCommandBuffer);
                }
                else
                {
                    if (screenShotModeEnabledOnLastFrame != screenShotModeEnabled)
                    {
                        CreateFullResCommandBuffer(targetCamera.allowHDR, screenShotModeEnabled);
                    }
                    targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, rendererCommandBuffer);
                }

                screenShotModeEnabledOnLastFrame = screenShotModeEnabled;

                renderingEnabled = true;
            }
        }
        
        void OnPreRender()
        {
            targetMaterial.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, targetCamera.cameraToWorldMatrix);
        }

        void OnPostRender()
        {
            if (renderingEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
            {
                targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, rendererCommandBuffer);

                if (quarterResRendererCommandBuffer != null)
                { 
                    targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, quarterResRendererCommandBuffer);
                }

                renderingEnabled = false;
            }
        }
        
        public void OnDestroy ()
        {
            if (targetCamera && rendererCommandBuffer != null)
            {
                targetCamera.RemoveCommandBuffer (CameraEvent.BeforeForwardAlpha, rendererCommandBuffer);
                rendererCommandBuffer = null;

                if (quarterResRendererCommandBuffer != null)
                {
                    targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, quarterResRendererCommandBuffer);
                    quarterResRendererCommandBuffer = null;
                }

                renderingEnabled = true;

                if (downscaledDepthRenderTexture)
                    downscaledDepthRenderTexture.Release();

                if (downscaledRenderTexture0)
                    downscaledRenderTexture0.Release();

                if (downscaledRenderTexture1)
                    downscaledRenderTexture1.Release();
            }
        }
    }

    public class ScreenSpaceScatteringContainer : GenericLocalAtmosphereContainer
    {
        ScreenSpaceScattering screenSpaceScattering;

        public ScreenSpaceScatteringContainer (Material atmosphereMaterial, Transform parentTransform, float Rt, ProlandManager parentManager, bool quarterRes) : base (atmosphereMaterial, parentTransform, Rt, parentManager)
        {
            scatteringGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
            scatteringGO.name = "Scatterer screenspace scattering " + parentManager.parentCelestialBody.name;
            GameObject.Destroy (scatteringGO.GetComponent<Collider> ());
            scatteringGO.transform.localScale = Vector3.one;

            //for now just disable this from reflection probe because no idea how to add the effect on it, no access to depth buffer and I don't feel like the perf hit would be worth to enable it
            //this will be handled by the ocean if it is present
            //TODO: remove this after finalizing 1/4 res since now we just use commandbuffer
            if (!manager.hasOcean || !Scatterer.Instance.mainSettings.useOceanShaders)
            {
                DisableEffectsChecker disableEffectsChecker = scatteringGO.AddComponent<DisableEffectsChecker> ();
                disableEffectsChecker.manager = this.manager;
            }

            screenSpaceScattering = scatteringGO.AddComponent<ScreenSpaceScattering>();

            scatteringGO.transform.position = parentTransform.position;
            scatteringGO.transform.parent   = parentTransform;
            
            screenSpaceScattering.material = atmosphereMaterial;
            screenSpaceScattering.material.CopyKeywordsFrom (atmosphereMaterial);

            screenSpaceScattering.hasOcean = manager.hasOcean && Scatterer.Instance.mainSettings.useOceanShaders;
            screenSpaceScattering.Init(quarterRes);
        }

        public override void UpdateContainer ()
        {
            bool isEnabled = !underwater && !inScaledSpace && activated;
            screenSpaceScattering.SetActive(isEnabled);
            scatteringGO.SetActive(isEnabled);
        }

        public override void Cleanup ()
        {
            SetActivated (false);
            if(scatteringGO)
            {
                if(scatteringGO.transform && scatteringGO.transform.parent)
                {
                        scatteringGO.transform.parent = null;
                }
                Component.Destroy(screenSpaceScattering);
                GameObject.Destroy(scatteringGO);
            }
        }
    }
}
