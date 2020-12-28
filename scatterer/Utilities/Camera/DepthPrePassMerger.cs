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
using UnityEngine.Rendering;

namespace scatterer
{
	// Merges the depth pre-pass result into the screenbuffer's depth to reduce overdraw and make the pre-pass more useful
	// Only usable with MSAA off so only for depth buffer mode
	public class DepthPrePassMerger : MonoBehaviour
	{
		Material copyCameraDepthMaterial;
		CommandBuffer depthInitCommandBuffer;
		Camera targetCamera;

		public DepthPrePassMerger ()
		{
			Utils.LogInfo("Adding DepthPrePassMerger");
			targetCamera = GetComponent<Camera> ();

			Utils.LogInfo ("targetCamera.depthTextureMode " + targetCamera.depthTextureMode.ToString ());

//			if (targetCamera.depthTextureMode == DepthTextureMode.Depth)
			if (QualitySettings.antiAliasing == 0)
			{
				if (targetCamera.depthTextureMode == DepthTextureMode.None)
					targetCamera.depthTextureMode = DepthTextureMode.Depth;

				copyCameraDepthMaterial = new Material (ShaderReplacer.Instance.LoadedShaders["Scatterer/CopyCameraDepth"]);
				depthInitCommandBuffer = new CommandBuffer();
				depthInitCommandBuffer.name = "Scatterer depth merge commandbuffer";
				depthInitCommandBuffer.Blit(null, BuiltinRenderTextureType.CameraTarget, copyCameraDepthMaterial, 1);
				targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, depthInitCommandBuffer);
				Utils.LogInfo("DepthPrePassMerger Created");
			}
		}

//		public void OnPreCull()
//		{
//			if (!ReferenceEquals (targetCamera, null) && !ReferenceEquals (depthInitCommandBuffer, null))
//			{
//				targetCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, depthInitCommandBuffer);
//			}
//		}
//		
//		void OnPostRender()
//		{
//			if (!ReferenceEquals (targetCamera, null) && !ReferenceEquals (depthInitCommandBuffer, null))
//			{
//				targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, depthInitCommandBuffer);
//			}
//		}

		public void OnDestroy()
		{
			if (!ReferenceEquals (targetCamera, null) && !ReferenceEquals (depthInitCommandBuffer, null))
			{
				targetCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, depthInitCommandBuffer);
			}
		}
	}
}

