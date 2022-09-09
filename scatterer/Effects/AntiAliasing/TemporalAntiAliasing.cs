using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
	// Temporal anti-aliasing from Unity post-processing stack V2
	// Modified to not blur the ocean, removed dependency on other postprocessing classes I don't need here so we can initialize and make everything work by just adding this to a camera
	// Generating motion vectors for the ocean is both expensive and unnecessary so we should just not apply TAA on the ocean, otherwise it would just blur it without proper motion vectors
	public class TemporalAntiAliasing : GenericAntiAliasing
	{
		public float jitterSpread = 0.9f;				//The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable, but blurrier, output. Range(0.1f, 1f)
		public float sharpness = 0.25f;					//Controls the amount of sharpening applied to the color buffer. High values may introduce dark-border artifacts. Range(0f, 3f)
		public float stationaryBlending = 0.90f;		//The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)
		public float motionBlending = 0.55f;			//The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)

		public Vector2 jitter { get; private set; }		// The current jitter amount
		public int sampleIndex { get; private set; }	// The current sample index
		
		enum Pass {SolverDilate,SolverNoDilate}
		
		readonly RenderTargetIdentifier[] m_Mrt = new RenderTargetIdentifier[2];
		bool m_ResetHistory = true;
		
		const int k_SampleCount = 8;
		
		// Ping-pong between two history textures as we can't read & write the same target in the same pass
		const int k_NumEyes = 1; const int k_NumHistoryTextures = 2;
		RenderTexture[][] m_HistoryTextures = new RenderTexture[k_NumEyes][];
		
		int[] m_HistoryPingPong = new int [k_NumEyes];

		Camera targetCamera;
		CommandBuffer temporalAACommandBuffer;
		Material temporalAAMaterial;

		DepthTextureMode originalDepthTextureMode;
		public bool checkOceanDepth = false;
		public bool jitterTransparencies = false;

		public void Awake()
		{
			targetCamera = GetComponent<Camera>();
			originalDepthTextureMode = targetCamera.depthTextureMode;
			targetCamera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			targetCamera.forceIntoRenderTexture = true;

			temporalAAMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/TemporalAntialiasing")]);
			Utils.EnableOrDisableShaderKeywords(temporalAAMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", false);

			jitterSpread = Scatterer.Instance.mainSettings.taaJitterSpread;
			sharpness = Scatterer.Instance.mainSettings.taaSharpness;
			stationaryBlending = Scatterer.Instance.mainSettings.taaStationaryBlending;
			motionBlending = Scatterer.Instance.mainSettings.taaMotionBlending;

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
		
		
		/// Prepares the jittered and non jittered projection matrices
		public void ConfigureJitteredProjectionMatrix()
		{
			targetCamera.projectionMatrix = GetJitteredProjectionMatrix(targetCamera);
			targetCamera.useJitteredProjectionMatrixForTransparentRendering = jitterTransparencies;

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

				int width, height;
				
				if (targetCamera.activeTexture)
				{
					width = targetCamera.activeTexture.width;
					height = targetCamera.activeTexture.height;
				}
				else
				{
					width = Screen.width;
					height = Screen.height;
				}

				rt = RenderTexture.GetTemporary (width, height, 0, RenderTextureFormat.ARGB32);

				GenerateHistoryName(rt, id);
				
				rt.filterMode = FilterMode.Bilinear;
				m_HistoryTextures[activeEye][id] = rt;

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

			//TODO: move to shader properties
			if (checkOceanDepth)
				Utils.EnableOrDisableShaderKeywords (temporalAAMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", Scatterer.Instance.scattererCelestialBodiesManager.isCustomOceanEnabledOnScattererPlanet);

			int pp = m_HistoryPingPong[activeEye];
			RenderTexture historyRead  = CheckHistory(++pp % 2, temporalAACommandBuffer);
			RenderTexture historyWrite = CheckHistory(++pp % 2, temporalAACommandBuffer);
			m_HistoryPingPong[activeEye] = ++pp % 2;

			temporalAAMaterial.SetTexture("_HistoryTex", historyRead);
						
			int pass = (int)Pass.SolverDilate;

			temporalAACommandBuffer.SetGlobalTexture ("_ScreenColor", BuiltinRenderTextureType.CameraTarget);
			temporalAACommandBuffer.Blit (null, historyWrite, temporalAAMaterial, pass);

			temporalAACommandBuffer.Blit (historyWrite, BuiltinRenderTextureType.CameraTarget);

			targetCamera.AddCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer); // BeforeImageEffects doesn't work well

			m_ResetHistory = false;
		}

		public void OnPostRender()
		{
			ResetProjection();
			targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer);
		}

		// This is needed otherwise transparencies jitter
		public void ResetProjection()
        {
			targetCamera.ResetProjectionMatrix();
			targetCamera.nonJitteredProjectionMatrix = targetCamera.projectionMatrix;
		}
		
		public void OnDestroy()
		{
			if (temporalAACommandBuffer != null)
				targetCamera.RemoveCommandBuffer (CameraEvent.AfterForwardAlpha, temporalAACommandBuffer);

			targetCamera.depthTextureMode = originalDepthTextureMode;

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

			targetCamera.ResetProjectionMatrix();
		}
	}
}
