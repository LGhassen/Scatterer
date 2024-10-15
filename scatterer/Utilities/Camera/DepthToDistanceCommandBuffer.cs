using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    public class DepthToDistanceCommandBuffer : MonoBehaviour
    {

        private CommandBuffer buffer;
        public Camera camera;
        private Material material;
        private static RenderTexture renderTexture;
        private bool initialized = false;
        public static RenderTexture RenderTexture { get => renderTexture; }

        private void Init()
        {
            // after depth texture is rendered on far and near cameras, copy it and merge it as a single distance buffer
            camera = gameObject.GetComponent<Camera>();
            buffer = new CommandBuffer();
            buffer.name = "Scatterer DepthToDistanceCommandBuffer";

            if (!renderTexture)
            {
                if (camera == null || camera.activeTexture == null)
                {
                    return;
                }
                renderTexture = RenderTextureUtils.CreateRenderTexture(camera.activeTexture.width, camera.activeTexture.height, RenderTextureFormat.RFloat, false, FilterMode.Point);
            }

            material = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/DepthToDistance")]);

            if (camera == Utils.getEarliestLocalCamera())
            {
                buffer.SetRenderTarget(renderTexture);
                buffer.ClearRenderTarget(false, true, Color.white);
            }

            buffer.Blit(null, renderTexture, material);

            camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);

            initialized = true;
        }

        public void OnPostRender()
        {
            if (initialized && camera.activeTexture != null)
            {
                if (renderTexture.width != camera.activeTexture.width || renderTexture.height != camera.activeTexture.height)
                {
                    renderTexture.Release();
                    renderTexture = null;

                    buffer.Clear();
                    buffer = null;

                    initialized = false;
                }
            }

            if (!initialized)
                Init();
        }

        public void OnDestroy()
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
        }
    }
}