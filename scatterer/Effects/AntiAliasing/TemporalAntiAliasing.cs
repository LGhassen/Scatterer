using UnityEngine;
using UnityEngine.Rendering;

namespace Scatterer
{
	// Temporal anti-aliasing from Unity post-processing stack V2
	// Modified to not blur the ocean, removed dependency on other postprocessing classes I don't need here so we can initialize and make everything work by just adding this to a camera
	// Also modified to store previous motion vectors and compare them to the current's to eliminate ghosting (where motion vectors are correct)
	// Generating motion vectors for the ocean is both expensive and unnecessary so we should just not apply TAA on the ocean, otherwise it would just blur it without proper motion vectors
	public class TemporalAntiAliasing : GenericAntiAliasing
	{
		public float jitterSpread = 0.9f;				//The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable, but blurrier, output. Range(0.1f, 1f)
		public float sharpness = 0.25f;					//Controls the amount of sharpening applied to the color buffer. High values may introduce dark-border artifacts. Range(0f, 3f)
		public float stationaryBlending = 0.90f;		//The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)
		public float motionBlending = 0.55f;			//The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)

		public Vector2 jitter { get; private set; }		// The current jitter amount
		public int sampleIndex { get; private set; }	// The current sample index
		
		enum Pass {SolverDilate, SolverNoDilate}
		
		readonly RenderTargetIdentifier[] m_Mrt = new RenderTargetIdentifier[2];
		bool m_ResetHistory = true;
		bool hdrEnabled = false;
		
		const int k_SampleCount = 8;
		
		// Ping-pong between two history textures as we can't read & write the same target in the same pass
		const int eyesCount = 1; const int historyTexturesCount = 2;
		RenderTexture[][] historyTextures = new RenderTexture[eyesCount][];
		RenderTexture[] historyMotionVectors = new RenderTexture[eyesCount];

		int[] m_HistoryPingPong = new int [eyesCount];

		Camera targetCamera;
		CommandBuffer temporalAACommandBuffer;
		Material temporalAAMaterial, copyMotionVectorsMaterial;

		DepthTextureMode originalDepthTextureMode;
		public bool checkOceanDepth = false;
		public bool jitterTransparencies = false;

		private static CameraEvent TAACameraEvent = CameraEvent.AfterForwardAlpha;  // BeforeImageEffects doesn't work well

		public void Awake()
		{
			targetCamera = GetComponent<Camera>();
			originalDepthTextureMode = targetCamera.depthTextureMode;
			targetCamera.depthTextureMode = DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
			targetCamera.forceIntoRenderTexture = true;

			temporalAAMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/TemporalAntialiasing")]);
			Utils.EnableOrDisableShaderKeywords(temporalAAMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", false);

			copyMotionVectorsMaterial = new Material(ShaderReplacer.Instance.LoadedShaders[("Scatterer/CopyMotionVectors")]);

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

			temporalAAMaterial.SetVector("_Jitter", jitter); // TODO: shader properties
		}
		
		RenderTexture CheckHistory(int id, CommandBuffer cmd)
		{
			int activeEye = 0;
			
			if (historyTextures[activeEye] == null)
				historyTextures[activeEye] = new RenderTexture[historyTexturesCount];

			if (hdrEnabled != targetCamera.allowHDR)
				ResetHistory();

			var historyRT = historyTextures[activeEye][id];
			
			if (m_ResetHistory || historyRT == null || !historyRT.IsCreated())
			{
				RenderTexture.ReleaseTemporary(historyRT);

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

				hdrEnabled = targetCamera.allowHDR;

				historyRT = RenderTexture.GetTemporary (width, height, 0, hdrEnabled ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.ARGB32);
				historyRT.name = "Temporal Anti-aliasing History id #" + id;
				
				historyRT.filterMode = FilterMode.Bilinear;
				historyTextures[activeEye][id] = historyRT;

				cmd.Blit(BuiltinRenderTextureType.CameraTarget, historyRT);
			}
			
			return historyTextures[activeEye][id];
		}

		
		RenderTexture CheckMotionVectorsHistory()
		{
			int activeEye = 0;

			var historyMotionVectorsRT = historyMotionVectors[activeEye];

			if (m_ResetHistory || historyMotionVectorsRT == null || !historyMotionVectorsRT.IsCreated())
			{
				RenderTexture.ReleaseTemporary(historyMotionVectorsRT);

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

				historyMotionVectorsRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.RGHalf);
				historyMotionVectorsRT.name = "Temporal Anti-aliasing Motion Vectors History";

				historyMotionVectorsRT.filterMode = FilterMode.Bilinear;
				historyMotionVectors[activeEye] = historyMotionVectorsRT;
			}

			return historyMotionVectors[activeEye];
		}
		

		//adapted from the original render() method
		public void OnPreCull()
		{
			bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);

			if (!screenShotModeEnabled)
			{ 
				temporalAACommandBuffer.Clear ();

				ConfigureJitteredProjectionMatrix ();

				int activeEye = 0;

				//TODO: move to shader properties
				if (checkOceanDepth)
					Utils.EnableOrDisableShaderKeywords (temporalAAMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", Scatterer.Instance.scattererCelestialBodiesManager.isCustomOceanEnabledOnScattererPlanet);

				int pingPongIndex = m_HistoryPingPong[activeEye];
				RenderTexture historyRead  = CheckHistory(++pingPongIndex % 2, temporalAACommandBuffer);
				RenderTexture historyWrite = CheckHistory(++pingPongIndex % 2, temporalAACommandBuffer);
				m_HistoryPingPong[activeEye] = ++pingPongIndex % 2;

				RenderTexture motionVectorsHistory = CheckMotionVectorsHistory();

				temporalAAMaterial.SetTexture("_HistoryTex", historyRead); // TODO: shader properties
				temporalAAMaterial.SetTexture("_HistoryMotionVectorsTex", motionVectorsHistory); // TODO: shader properties

				int pass = (int)Pass.SolverDilate;

				temporalAACommandBuffer.SetGlobalTexture ("_ScreenColor", BuiltinRenderTextureType.CameraTarget);  // TODO: shader properties
				temporalAACommandBuffer.Blit (null, historyWrite, temporalAAMaterial, pass);

				temporalAACommandBuffer.Blit (historyWrite, BuiltinRenderTextureType.CameraTarget);
				temporalAACommandBuffer.Blit (null, motionVectorsHistory, copyMotionVectorsMaterial);

				targetCamera.AddCommandBuffer (TAACameraEvent, temporalAACommandBuffer);

				m_ResetHistory = false;
			}
		}

		public void OnPostRender()
		{
			ResetProjection();
			targetCamera.RemoveCommandBuffer (TAACameraEvent, temporalAACommandBuffer);
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
				targetCamera.RemoveCommandBuffer (TAACameraEvent, temporalAACommandBuffer);

			targetCamera.depthTextureMode = originalDepthTextureMode;

			if (historyTextures != null)
			{
				for (int i = 0; i < historyTextures.Length; i++)
				{
					if (historyTextures[i] == null)
						continue;

					for (int j = 0; j < historyTextures[i].Length; j++)
					{
						RenderTexture.ReleaseTemporary(historyTextures[i][j]);
						historyTextures[i][j] = null;
					}
					historyTextures[i] = null;
				}
			}

			if (historyMotionVectors != null)
            {
				for (int i = 0; i < historyMotionVectors.Length; i++)
				{
					if (historyMotionVectors[i] == null)
						continue;

					RenderTexture.ReleaseTemporary(historyMotionVectors[i]);

					historyMotionVectors[i] = null;
				}
			}

			sampleIndex = 0;
			m_HistoryPingPong[0] = 0;

			ResetHistory();

			targetCamera.ResetProjectionMatrix();
		}
	}
}
