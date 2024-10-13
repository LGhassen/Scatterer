using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class SkySphereContainer
    {
        GameObject skySphereGO;
        MeshRenderer skySphereMR;

        public GameObject GameObject { get { return skySphereGO; } }
        public MeshRenderer MeshRenderer { get { return skySphereMR; } }

        Transform parentLocalTransform, parentScaledTransform;

        MeshFilter skySphereMF;
        
        public SkySphereContainer(float size, Material material, Transform inParentLocalTransform, Transform inParentScaledTransform)
        {
            skySphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy (skySphereGO.GetComponent<Collider> ());

            skySphereGO.transform.localScale = Vector3.one;
            
            skySphereMF = skySphereGO.GetComponent<MeshFilter>();
            Vector3[] verts = skySphereMF.mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i].normalized * size;
            }
            skySphereMF.mesh.vertices = verts;
            skySphereMF.mesh.RecalculateBounds();
            skySphereMF.mesh.RecalculateNormals();
            
            skySphereMR = skySphereGO.GetComponent<MeshRenderer>();
            skySphereMR.sharedMaterial = material;

            material.SetShaderPassEnabled("localSky", false); // Local pass will render manually via CommandBuffer

            Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);

            skySphereMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            skySphereMR.receiveShadows = false;
            skySphereMR.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            skySphereMR.enabled = true;

            if (HighLogic.LoadedScene == GameScenes.MAINMENU)
                skySphereGO.layer = 15;
            else
                skySphereGO.layer = 9;

            skySphereGO.transform.position = inParentScaledTransform.position;
            skySphereGO.transform.parent = inParentScaledTransform;

            parentScaledTransform = inParentScaledTransform;
            parentLocalTransform = inParentLocalTransform;
        }
        
        public void SwitchLocalMode()
        {
            skySphereGO.layer = 15;

            skySphereGO.transform.parent = null;

            skySphereGO.transform.position = parentLocalTransform.position;
            skySphereGO.transform.localScale = new Vector3(ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor);
            skySphereGO.transform.parent = parentLocalTransform;

            Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", true);

            skySphereGO.AddComponent<SkySphereLocalRenderer> ().Init(skySphereMR.sharedMaterial, skySphereMR, skySphereGO);
        }
        
        public void SwitchScaledMode()
        {
            skySphereGO.layer = 9;

            skySphereGO.transform.parent = null;

            skySphereGO.transform.position = parentScaledTransform.position;
            skySphereGO.transform.localScale = Vector3.one;
            skySphereGO.transform.parent = parentScaledTransform;

            Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);

            var scrCopy = skySphereGO.GetComponent<SkySphereLocalRenderer> ();

            if (scrCopy)
                UnityEngine.Component.Destroy (scrCopy);

        }

        public void Resize(float size)
        {    
            Vector3[] verts = skySphereMF.mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = verts[i].normalized * size;
            }
            skySphereMF.mesh.vertices = verts;
            skySphereMF.mesh.RecalculateBounds();
            skySphereMF.mesh.RecalculateNormals();
        }

        public void Cleanup()
        {
            if (skySphereMR != null)
            {
                skySphereMR.enabled = false;
                UnityEngine.Component.Destroy (skySphereMR);
            }

            if (skySphereGO != null)
            {
                UnityEngine.Object.Destroy(skySphereGO);
            }
        }
    }

    public class SkySphereLocalRenderer : MonoBehaviour
    {
        Material material;
        Renderer renderer;
        GameObject skySphereGO;

        Dictionary<Camera, SkySphereLocalCommandBuffer> cameraToLocalSkyCommandBuffer = new Dictionary<Camera, SkySphereLocalCommandBuffer>();

        public void Init(Material material, Renderer renderer, GameObject skySphereGO)
        {
            this.material = material;
            this.renderer = renderer;
            this.skySphereGO = skySphereGO;
        }

        void OnWillRenderObject()
        {
            Camera cam = Camera.current;

            if (!cam)
                return;

            if (cam == Scatterer.Instance.nearCamera && !Scatterer.Instance.unifiedCameraMode)
            {
                material.SetFloat(ShaderProperties.renderSkyOnCurrentCamera_PROPERTY, 0f);
            }
            else
            {
                material.SetFloat(ShaderProperties.renderSkyOnCurrentCamera_PROPERTY, 1f);
            }

            ScreenCopyCommandBuffer.EnableScreenCopyForFrame(cam);

            if (skySphereGO.layer == 15)
            {
                RenderLocalSkyForFrame(cam);
            }
        }

        private void RenderLocalSkyForFrame(Camera cam)
        {
            if (cameraToLocalSkyCommandBuffer.ContainsKey(cam))
            {
                if (cameraToLocalSkyCommandBuffer[cam])
                    cameraToLocalSkyCommandBuffer[cam].EnableForFrame();
            }
            else
            {
                SkySphereLocalCommandBuffer renderer = (SkySphereLocalCommandBuffer)cam.gameObject.AddComponent(typeof(SkySphereLocalCommandBuffer));
                renderer.Init(cam, material, this.renderer);

                cameraToLocalSkyCommandBuffer[cam] = renderer;
            }
        }

        public void OnDestroy()
        {
            if (cameraToLocalSkyCommandBuffer != null)
            {
                foreach (var component in cameraToLocalSkyCommandBuffer.Values)
                {
                    Destroy(component);
                }
            }
        }
    }

    public class SkySphereLocalCommandBuffer : MonoBehaviour
    {
        Camera targetCamera;
        CommandBuffer commandBuffer;

        bool isEnabled = false;

        private static CameraEvent localSkySphereEvent = CameraEvent.BeforeImageEffectsOpaque;

        public void Init(Camera targetCamera, Material material, Renderer renderer)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "Scatterer local sky commandBuffer";
            commandBuffer.DrawRenderer(renderer, material, 0, 2); // Local pass

            this.targetCamera = targetCamera;
        }

        public void EnableForFrame()
        {
            if (!isEnabled && commandBuffer != null)
            {
                targetCamera.AddCommandBuffer(localSkySphereEvent, commandBuffer);
                isEnabled = true;
            }
        }

        public void OnPreRender()
        {
            if (isEnabled)
            {
                // This is for the volumetrics compose pass, since the scatterer sky composes the clouds on the sky early
                // For the SSR to see it
                Shader.SetGlobalInt(ShaderProperties.ScattererLocalSkyActiveOnCurrentCamera_PROPERTY, 1);
            }
        }

        public void OnPostRender()
        {
            if (isEnabled)
            {
                targetCamera.RemoveCommandBuffer(localSkySphereEvent, commandBuffer);
                Shader.SetGlobalInt(ShaderProperties.ScattererLocalSkyActiveOnCurrentCamera_PROPERTY, 0);
                isEnabled = false;
            }
        }

        public void OnDestroy()
        {
            if (targetCamera != null && commandBuffer != null)
            {
                targetCamera.RemoveCommandBuffer(localSkySphereEvent, commandBuffer);
            }

            if (commandBuffer != null)
            {
                commandBuffer.Release();
            }
        }
    }
}