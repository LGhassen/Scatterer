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
	// Temporal anti-aliasing from Unity post-processing stack V2
	// To be modified to not blur the ocean, removed dependency on other postprocessing classes I don't need here so we can initialize and make everything work by just adding this to a camera
	// Generating motion vectors for the ocean is both expensive and unnecessary so we should just not apply TAA on the ocean, otherwise it would just blur it without proper motion vectors
	public class TemporalAntiAliasing : MonoBehaviour
	{
		public float jitterSpread = 0.75f;				//The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable, but blurrier, output. Range(0.1f, 1f)
		public float sharpness = 0.25f;					//Controls the amount of sharpening applied to the color buffer. High values may introduce dark-border artifacts. Range(0f, 3f)
		public float stationaryBlending = 0.95f;		//The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)
		public float motionBlending = 0.85f;			//The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)

		public Vector2 jitter { get; private set; }		// The current jitter amount
		public int sampleIndex { get; private set; }	// The current sample index
		
		enum Pass {SolverDilate,SolverNoDilate}
		
		readonly RenderTargetIdentifier[] m_Mrt = new RenderTargetIdentifier[2];
		bool m_ResetHistory = true;
		
		const int k_SampleCount = 8;
		
		// Ping-pong between two history textures as we can't read & write the same target in the same pass
		const int k_NumEyes = 1; const int k_NumHistoryTextures = 2;
		RenderTexture[][] m_HistoryTextures = new RenderTexture[k_NumEyes][];
		RenderTexture screenCopy;
		
		int[] m_HistoryPingPong = new int [k_NumEyes];

		Camera targetCamera;
		CommandBuffer temporalAACommandBuffer;
		Material temporalAAMaterial;

		public TemporalAntiAliasing()
		{
			targetCamera = GetComponent<Camera> ();
			targetCamera.depthTextureMode = DepthTextureMode.MotionVectors;

			int width, height;
			
			if (!ReferenceEquals (targetCamera.activeTexture, null))
			{
				width = targetCamera.activeTexture.width;
				height = targetCamera.activeTexture.height;
			}
			else
			{
				width = Screen.width;
				height = Screen.height;
			}

			screenCopy = new RenderTexture (width, height, 0, RenderTextureFormat.ARGB32);
			screenCopy.anisoLevel = 1;
			screenCopy.antiAliasing = 1;
			screenCopy.volumeDepth = 0;
			screenCopy.useMipMap = false;
			screenCopy.autoGenerateMips = false;
			screenCopy.Create ();

			temporalAAMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/TemporalAntialiasing")]);

			const float kMotionAmplification = 100f * 60f;
			temporalAAMaterial.SetFloat("_Sharpness", sharpness);
			temporalAAMaterial.SetVector("_FinalBlendParameters", new Vector4(stationaryBlending, motionBlending, kMotionAmplification, 0f));

			temporalAACommandBuffer = new CommandBuffer ();
		}

		internal DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
		}
		
		internal void ResetHistory()
		{
			m_ResetHistory = true;
		}
		
		Vector2 GenerateRandomOffset()
		{
			// The variance between 0 and the actual halton sequence values reveals noticeable instability
			// in Unity's shadow maps, so we avoid index 0.
			var offset = new Vector2(
				HaltonSeq.Get((sampleIndex & 1023) + 1, 2) - 0.5f,
				HaltonSeq.Get((sampleIndex & 1023) + 1, 3) - 0.5f
				);
			
			if (++sampleIndex >= k_SampleCount)
				sampleIndex = 0;
			
			return offset;
		}

		/// Gets a jittered perspective projection matrix for a given camera.
		public static Matrix4x4 GetJitteredPerspectiveProjectionMatrix(Camera camera, Vector2 offset)
		{
			float near = camera.nearClipPlane;
			float far = camera.farClipPlane;
			
			float vertical = Mathf.Tan(0.5f * Mathf.Deg2Rad * camera.fieldOfView) * near;
			float horizontal = vertical * camera.aspect;
			
			offset.x *= horizontal / (0.5f * camera.pixelWidth);
			offset.y *= vertical / (0.5f * camera.pixelHeight);
			
			var matrix = camera.projectionMatrix;
			
			matrix[0, 2] += offset.x / horizontal;
			matrix[1, 2] += offset.y / vertical;
			
			return matrix;
		}


		/// Generates a jittered projection matrix for a given camera.
		public Matrix4x4 GetJitteredProjectionMatrix(Camera camera)
		{
			Matrix4x4 cameraProj;
			jitter = GenerateRandomOffset();
			jitter *= jitterSpread;

			cameraProj = GetJitteredPerspectiveProjectionMatrix(camera, jitter);

			jitter = new Vector2(jitter.x / camera.pixelWidth, jitter.y / camera.pixelHeight);
			return cameraProj;
		}
		
		
		/// Prepares the jittered and non jittered projection matrices.
		/// //What calls this though?
		/// //From te method render in postProcessing layer, which I will move here
		public void ConfigureJitteredProjectionMatrix()
		{
			targetCamera.ResetProjectionMatrix();

			targetCamera.nonJitteredProjectionMatrix = targetCamera.projectionMatrix;
			targetCamera.projectionMatrix = GetJitteredProjectionMatrix(targetCamera);
			targetCamera.useJitteredProjectionMatrixForTransparentRendering = false;

			temporalAAMaterial.SetVector("_Jitter", jitter);
		}
		
		void GenerateHistoryName(RenderTexture rt, int id)
		{
			rt.name = "Temporal Anti-aliasing History id #" + id;
		}
		
		RenderTexture CheckHistory(int id, CommandBuffer cmd)
		{
			int activeEye = 0;
			
			if (m_HistoryTextures[activeEye] == null)
				m_HistoryTextures[activeEye] = new RenderTexture[k_NumHistoryTextures];
			
			var rt = m_HistoryTextures[activeEye][id];
			
			if (m_ResetHistory || rt == null || !rt.IsCreated())
			{
				RenderTexture.ReleaseTemporary(rt);

				rt = RenderTexture.GetTemporary (screenCopy.width, screenCopy.height, 0, RenderTextureFormat.ARGB32);

				GenerateHistoryName(rt, id);
				
				rt.filterMode = FilterMode.Bilinear;
				m_HistoryTextures[activeEye][id] = rt;
				
				//context.command.BlitFullscreenTriangle(context.source, rt); //this should be just a blit from the screen to RT if I understand correctly
																			  //I think this is done just to init the history textures
				cmd.Blit(BuiltinRenderTextureType.CameraTarget, rt);
			}
			
			return m_HistoryTextures[activeEye][id];
		}

		//adapted from the original render() method
		public void OnPreCull()
		{
			temporalAACommandBuffer.Clear ();

			ConfigureJitteredProjectionMatrix ();

			int activeEye = 0;

			//maybe we can set the ocean params here if we need to not AA the ocean, check instance etc

			int pp = m_HistoryPingPong[activeEye];
			RenderTexture historyRead  = CheckHistory(++pp % 2, temporalAACommandBuffer);
			RenderTexture historyWrite = CheckHistory(++pp % 2, temporalAACommandBuffer);
			m_HistoryPingPong[activeEye] = ++pp % 2;

			temporalAAMaterial.SetTexture("_HistoryTex", historyRead);

						
			int pass = (int)Pass.SolverDilate;
			//m_Mrt[0] = context.destination;			//replace with builtin rt thing?
//			m_Mrt [0] = new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget);
//			m_Mrt [1] = historyWrite;
			
//			cmd.BlitFullscreenTriangle(context.source, m_Mrt, context.source, sheet, pass); //this should blit the actual TAA shader

			//temporalAACommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, screenCopy);
			//temporalAACommandBuffer.Blit (screenCopy, m_Mrt, temporalAAMaterial, pass);
//			temporalAACommandBuffer.Blit (screenCopy, m_Mrt);

			//temporalAACommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, historyWrite, temporalAAMaterial, pass); //this didn't seem to work? not getting main tex?

//			temporalAACommandBuffer.Blit (null, historyWrite, temporalAAMaterial, pass); //this didn't seem to work? not getting main tex?

			temporalAACommandBuffer.SetGlobalTexture ("_MainTex", new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget));
			temporalAACommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, historyWrite);

			temporalAACommandBuffer.SetGlobalTexture ("_MainTex", historyWrite);
			temporalAACommandBuffer.Blit (historyWrite, BuiltinRenderTextureType.CameraTarget, temporalAAMaterial, pass);

			temporalAACommandBuffer.SetGlobalTexture ("_MainTex", new RenderTargetIdentifier (BuiltinRenderTextureType.CameraTarget));
			temporalAACommandBuffer.Blit (BuiltinRenderTextureType.CameraTarget, historyWrite);

//			temporalAACommandBuffer.Blit (historyWrite, BuiltinRenderTextureType.CameraTarget);

			//add commandbuffer to camera thing
			targetCamera.AddCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer);

			m_ResetHistory = false;
		}

		public void OnPostRender()
		{
			//remove commandbuffer from camera
			targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer);
		}
		
		public void Release()
		{
			Utils.LogInfo ("Temporal AA release called!");

			if (m_HistoryTextures != null)
			{
				for (int i = 0; i < m_HistoryTextures.Length; i++)
				{
					if (m_HistoryTextures[i] == null)
						continue;

					for (int j = 0; j < m_HistoryTextures[i].Length; j++)
					{
						RenderTexture.ReleaseTemporary(m_HistoryTextures[i][j]);
						m_HistoryTextures[i][j] = null;
					}
					m_HistoryTextures[i] = null;
				}
			}

			sampleIndex = 0;
			m_HistoryPingPong[0] = 0;

			ResetHistory();

			screenCopy.Release ();

			if (!ReferenceEquals(temporalAACommandBuffer,null))
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer);

		}
	}
}
