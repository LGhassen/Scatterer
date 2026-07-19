using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
    [DefaultExecutionOrder(-1)]
    public class OrderedAfterForwardOpaqueCommandBuffer : MonoBehaviour
    {
        private static Dictionary<Camera, OrderedAfterForwardOpaqueCommandBuffer> CameraToOrderedCommandBuffer = new Dictionary<Camera, OrderedAfterForwardOpaqueCommandBuffer>();
        
        public static void AddCommandBuffer(Camera cam, CommandBuffer commandBuffer, int order)
        {
            if (CameraToOrderedCommandBuffer.ContainsKey (cam))
            {
                if(CameraToOrderedCommandBuffer[cam])
                    CameraToOrderedCommandBuffer[cam].AddCommandBuffer(commandBuffer, order);
            }
            else
            {
                OrderedAfterForwardOpaqueCommandBuffer handler = (OrderedAfterForwardOpaqueCommandBuffer) cam.gameObject.AddComponent(typeof(OrderedAfterForwardOpaqueCommandBuffer));
                
                handler.Initialize();
                CameraToOrderedCommandBuffer[cam] = handler;
            }
        }

        public static void RemoveCommandBuffer(Camera cam, CommandBuffer commandBuffer)
        {
            if (CameraToOrderedCommandBuffer.ContainsKey(cam) && CameraToOrderedCommandBuffer[cam] != null)
                CameraToOrderedCommandBuffer[cam].RemoveCommandBuffer(commandBuffer);
        }

        private bool isEnabled = false;
        private bool isInitialized = false;
        private Camera targetCamera;
        private readonly SortedList<int, List<CommandBuffer>> buffers = new SortedList<int, List<CommandBuffer>>();
        
        public void Initialize()
        {
            targetCamera = GetComponent<Camera> ();
            isInitialized = true;
        }

        public void AddCommandBuffer(CommandBuffer commandBuffer, int order)
        {
            if (isInitialized)
            {
                if (!buffers.ContainsKey(order))
                { 
                    buffers[order] = new List<CommandBuffer>();
                }

                buffers[order].Add(commandBuffer);

                isEnabled = true;
            }
        }

        void OnPreRender()
        {
            if (!isInitialized || !isEnabled || buffers.Count == 0)
                return;

            foreach (var bucket in buffers.Values)
            { 
                foreach (var cb in bucket)
                { 
                    targetCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cb);
                }
            }
        }

        // Caller manages removals, this class won't manage it automatically
        public void RemoveCommandBuffer(CommandBuffer cb)
        {
            targetCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cb);
        }

        void OnPostRender()
        {
            if (!isInitialized)
            {
                Initialize();
            }
            else if (isEnabled && targetCamera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Left)
            {
                buffers.Clear();
                isEnabled = false;
            }
        }
    }
}

