using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class RaymarchedGodraysRenderer : MonoBehaviour
    {
        private SkyNode parentSkyNode;
        private Camera targetCamera;
        private Light targetLight;

        private Material scatteringOcclusionMaterial, downscaleDepthMaterial;
        Mesh mesh;

        private int screenWidth, screenHeight;
        private int renderWidth, renderHeight;

        // This is indexed by [isRightEye][flip]
        private FlipFlop<FlipFlop<RenderTexture>> godraysRT, depthRT;
        private bool useFlipBuffer = true;

        private RenderTexture downscaledDepth;

        // Indexed by isRightEye
        private FlipFlop<CommandBuffer> godraysCommandBuffer;
        private FlipFlop<Matrix4x4> previousV;
        private FlipFlop<Matrix4x4> previousP;

        private Vector3d previousParentPosition = Vector3d.zero;

        private bool renderingEnabled = false;

        private bool hasOcean = false;
        private bool useCloudGodrays = true;
        private bool useTerrainGodrays = false;
        private int stepCount = 50;

        private bool godraysScreenShotModeEnabled = false;
        private int screenshotModeIterations = 8;

        private static CameraEvent ScatteringOcclusionCameraEvent = CameraEvent.AfterForwardOpaque;

        public RaymarchedGodraysRenderer()
        {

        }

        public bool Init(Light inputLight, SkyNode inputParentSkyNode, bool useCloudGodrays, bool useTerrainGodrays, int stepCount, int screenshotModeIterations)
        {
            if (ShaderReplacer.Instance.LoadedShaders.ContainsKey("Scatterer/RaymarchScatteringOcclusion")) // TODO: change this to not duplicate the key
            {
                scatteringOcclusionMaterial = new Material(ShaderReplacer.Instance.LoadedShaders["Scatterer/RaymarchScatteringOcclusion"]);
            }
            else
            {
                Utils.LogError("Godrays Scattering Occlusion shader can't be found, godrays can't be added");
                return false;
            }

            downscaleDepthMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/DownscaleDepth")]);

            downscaleDepthMaterial.EnableKeyword("COPY_ONLY_OFF");
            downscaleDepthMaterial.DisableKeyword("COPY_ONLY_ON");

            if (!inputLight)
            {
                Utils.LogError("Godrays light is null, godrays can't be added");
                return false;
            }

            targetLight = inputLight;
            parentSkyNode = inputParentSkyNode;

            hasOcean = parentSkyNode.prolandManager.hasOcean && Scatterer.Instance.mainSettings.useOceanShaders;

            this.useCloudGodrays = useCloudGodrays;
            this.useTerrainGodrays = useTerrainGodrays;
            this.stepCount = stepCount;
            this.screenshotModeIterations = screenshotModeIterations;

            SetStepCountAndKeywords(scatteringOcclusionMaterial);

            scatteringOcclusionMaterial.SetTexture("StbnBlueNoise", ShaderReplacer.stbn);
            scatteringOcclusionMaterial.SetVector("stbnDimensions", new Vector3(ShaderReplacer.stbnDimensions.x, ShaderReplacer.stbnDimensions.y, ShaderReplacer.stbnDimensions.z));

            scatteringOcclusionMaterial.SetFloat("screenshotModeIterations", (float)screenshotModeIterations);

            targetCamera = gameObject.GetComponent<Camera>();

            bool supportVR = VRUtils.VREnabled();

            if (supportVR)
            {
                VRUtils.GetEyeTextureResolution(out screenWidth, out screenHeight);
            }
            else
            {
                screenWidth = Screen.width;
                screenHeight = Screen.height;
            }

            // Terrain godrays are higher frequency and need higher resolution to avoid artifacts and aliasing, cloud godrays are fine with lower
            if (useTerrainGodrays)
            {
                renderWidth = screenWidth   / 4;
                renderHeight = screenHeight / 2;
            }
            else
            {
                renderWidth  = screenWidth  / 4;
                renderHeight = screenHeight / 4;
            }

            godraysRT = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear);
            depthRT = VRUtils.CreateVRFlipFlopRT(supportVR, renderWidth, renderHeight, RenderTextureFormat.RFloat, FilterMode.Point);

            downscaledDepth = RenderTextureUtils.CreateRenderTexture(renderWidth, renderHeight, RenderTextureFormat.RFloat, false, FilterMode.Point);

            godraysCommandBuffer = new FlipFlop<CommandBuffer>(VRUtils.VREnabled() ? new CommandBuffer() : null, new CommandBuffer());

            if (godraysCommandBuffer[false] != null)
            {
                godraysCommandBuffer[false].name = "Scatterer raymarched godrays CommandBuffer";
            }

            if (godraysCommandBuffer[true] != null)
            {
                godraysCommandBuffer[true].name = "Scatterer raymarched godrays CommandBuffer";
            }


            GameObject tempGO = GameObject.CreatePrimitive(PrimitiveType.Quad);

            mesh = Instantiate(tempGO.GetComponent<MeshFilter>().mesh);

            GameObject.Destroy(tempGO);

            return true;
        }

        public void SetStepCountAndKeywords(Material mat)
        {
            SetStepCount(mat);

            mat.EnableKeyword("GODRAYS_RAYMARCHED");
            mat.DisableKeyword("GODRAYS_OFF");
            mat.DisableKeyword("RAYMARCHED_GODRAYS_OFF");
            mat.DisableKeyword("GODRAYS_LEGACY");

            if (useCloudGodrays && useTerrainGodrays)
            {
                mat.SetFloat(ShaderProperties.godraysSoftwareSwitch_PROPERTY, 3f);
                mat.EnableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
                mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
                mat.DisableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
            }
            else if (useCloudGodrays)
            {
                mat.SetFloat(ShaderProperties.godraysSoftwareSwitch_PROPERTY, 1f);
                mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
                mat.EnableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
                mat.DisableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
            }
            else if (useTerrainGodrays)
            {
                mat.SetFloat(ShaderProperties.godraysSoftwareSwitch_PROPERTY, 2f);
                mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_TERRAIN_ON");
                mat.DisableKeyword("RAYMARCHED_GODRAYS_CLOUDS_ON");
                mat.EnableKeyword("RAYMARCHED_GODRAYS_TERRAIN_ON");
            }
        }

        public void SetStepCount(Material mat)
        {
            mat.SetFloat(ShaderProperties.godraysStepCount_PROPERTY, stepCount);
        }

        void OnPreRender()
        {
            if (parentSkyNode && !parentSkyNode.inScaledSpace)
            {
                renderingEnabled = true;

                bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);

                if (godraysScreenShotModeEnabled != screenShotModeEnabled)
                {
                    ResizeRenderTextures(screenShotModeEnabled);

                    if (screenShotModeEnabled)
                    {
                        downscaleDepthMaterial.EnableKeyword("COPY_ONLY_ON");
                        downscaleDepthMaterial.DisableKeyword("COPY_ONLY_OFF");
                    }
                    else
                    {
                        downscaleDepthMaterial.EnableKeyword("COPY_ONLY_OFF");
                        downscaleDepthMaterial.DisableKeyword("COPY_ONLY_ON");
                    }

                    SetStepCount(scatteringOcclusionMaterial);

                    godraysScreenShotModeEnabled = screenShotModeEnabled;
                }

                // volumeDepthMaterial.SetTexture(ShaderProperties._ShadowMapTextureCopyScatterer_PROPERTY, ShadowMapCopy.RenderTexture);

                // TODO: remove unneded calls?
                parentSkyNode.InitUniforms(scatteringOcclusionMaterial);
                parentSkyNode.SetUniforms(scatteringOcclusionMaterial);
                parentSkyNode.UpdatePostProcessMaterialUniforms(scatteringOcclusionMaterial);

                scatteringOcclusionMaterial.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, targetCamera.cameraToWorldMatrix);

                scatteringOcclusionMaterial.SetVector(ShaderProperties._planetPos_PROPERTY, parentSkyNode.parentLocalTransform.position); // check if needed

                bool isRightEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right;

                CommandBuffer commandBuffer = godraysCommandBuffer[isRightEye];

                int frame = Time.frameCount % ShaderReplacer.stbnDimensions.z;
                commandBuffer.SetGlobalFloat(ShaderProperties.frameNumber_PROPERTY, frame);

                commandBuffer.Clear();

                if (hasOcean)
                    commandBuffer.Blit(null, downscaledDepth, downscaleDepthMaterial, 1);      //ocean depth buffer downsample
                else
                    commandBuffer.Blit(null, downscaledDepth, downscaleDepthMaterial, 0);      //default depth buffer downsample

                scatteringOcclusionMaterial.SetTexture(ShaderProperties.downscaledDepth_PROPERTY, downscaledDepth);
                scatteringOcclusionMaterial.SetTexture(ShaderProperties.historyGodrayOcclusionBuffer_PROPERTY, godraysRT[isRightEye][!useFlipBuffer]);
                scatteringOcclusionMaterial.SetTexture(ShaderProperties.historyGodrayDepthBuffer_PROPERTY, depthRT[isRightEye][!useFlipBuffer]);

                var prevV = previousV[isRightEye];

                // Add the frame to frame offset of the parent body, this contains both the movement of the body and the floating origin
                Vector3d currentOffset = parentSkyNode.parentLocalTransform.position - previousParentPosition;
                previousParentPosition = parentSkyNode.parentLocalTransform.position;

                //transform to camera space
                var currentV = VRUtils.GetViewMatrixForCamera(targetCamera);
                Vector3 floatOffset = currentV.MultiplyVector(-currentOffset);

                //inject in the previous view matrix
                prevV.m03 += floatOffset.x;
                prevV.m13 += floatOffset.y;
                prevV.m23 += floatOffset.z;

                var prevP = previousP[isRightEye];

                scatteringOcclusionMaterial.SetMatrix(ShaderProperties.previousVP_PROPERTY, prevP * prevV);

                var currentP = VRUtils.GetNonJitteredProjectionMatrixForCamera(targetCamera); // Note: This isn't the GPU projection matrix (GL.GetGPUprojection matrix) equivalent to UNITY_MATRIX_P, but the code that uses this is adapted from code originally using unity_CameraInvProjection

                scatteringOcclusionMaterial.SetMatrix(ShaderProperties.inverseProjection_PROPERTY, currentP.inverse);

                RenderTargetIdentifier[] RenderTargets = { new RenderTargetIdentifier(godraysRT[isRightEye][useFlipBuffer]), new RenderTargetIdentifier(depthRT[isRightEye][useFlipBuffer]) };

                commandBuffer.SetRenderTarget(RenderTargets, godraysRT[isRightEye][useFlipBuffer].depthBuffer);

                if (!screenShotModeEnabled)
                {
                    commandBuffer.DrawMesh(mesh, Matrix4x4.identity, scatteringOcclusionMaterial, 0, 0);
                }
                else
                {
                    commandBuffer.ClearRenderTarget(false, true, Color.clear);
                    for (int i=0;i< screenshotModeIterations;i++)
                    {
                        commandBuffer.SetGlobalFloat(ShaderProperties.frameNumber_PROPERTY, frame);
                        commandBuffer.DrawMesh(mesh, Matrix4x4.identity, scatteringOcclusionMaterial, 0, 1);
                        frame++;
                        frame = frame % ShaderReplacer.stbnDimensions.z;
                    }
                }

                commandBuffer.SetGlobalTexture(ShaderProperties._godrayDepthTexture_PROPERTY, RenderTargets[0]);
                commandBuffer.SetGlobalTexture(ShaderProperties.downscaledGodrayDepth_PROPERTY, downscaledDepth);

                targetCamera.AddCommandBuffer(ScatteringOcclusionCameraEvent, commandBuffer); // This renders after the ocean even though they are on the same event because it gets added later (OnPreRender vs OnWillRenderObject)
            }
        }

        private void ResizeRenderTextures(bool screenShotModeEnabled)
        {
            int newWidth, newHeight;

            if (screenShotModeEnabled)
            {
                // There is an issue with my downscaling shader where it stays stuck at the original texelSize(?)
                // and can't handle downscaling so make this a simple copy at the same resolution
                int superSizingFactor = Mathf.Max(GameSettings.SCREENSHOT_SUPERSIZE, 1);
                newWidth = screenWidth * superSizingFactor;
                newHeight = screenHeight * superSizingFactor;
            }
            else
            {
                newWidth = renderWidth;
                newHeight = renderHeight;
            }

            VRUtils.ResizeVRFlipFlopRT(ref godraysRT, newWidth, newHeight);
            VRUtils.ResizeVRFlipFlopRT(ref depthRT, newWidth, newHeight);

            RenderTextureUtils.ResizeRT(downscaledDepth, newWidth, newHeight);
        }

        void OnPostRender()
        {
            if (renderingEnabled)
            {
                bool isRightEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right;
                var commandBuffer = godraysCommandBuffer[isRightEye];

                targetCamera.RemoveCommandBuffer(ScatteringOcclusionCameraEvent, commandBuffer);

                previousP[isRightEye] = GL.GetGPUProjectionMatrix(VRUtils.GetNonJitteredProjectionMatrixForCamera(targetCamera), false);
                previousV[isRightEye] = VRUtils.GetViewMatrixForCamera(targetCamera);

                bool doneRendering = targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left;

                if (doneRendering)
                {
                    renderingEnabled = false;
                    useFlipBuffer = !useFlipBuffer;
                }

                Shader.SetGlobalTexture(ShaderProperties._godrayDepthTexture_PROPERTY, Texture2D.blackTexture);
            }
        }
        
        public void OnDestroy()
        {
            if (targetCamera != null)
            { 
                if (godraysCommandBuffer[true] != null)
                {
                    targetCamera.RemoveCommandBuffer(ScatteringOcclusionCameraEvent, godraysCommandBuffer[true]);
                }

                targetCamera.RemoveCommandBuffer(ScatteringOcclusionCameraEvent, godraysCommandBuffer[false]);
            }
            
            if (downscaledDepth != null)
            {
                downscaledDepth.Release();
            }

            VRUtils.ReleaseVRFlipFlopRT(ref godraysRT);
            VRUtils.ReleaseVRFlipFlopRT(ref depthRT);
        }
    }
}

