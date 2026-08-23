using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using System.Collections.Generic;

namespace Scatterer
{
	// Temporal anti-aliasing from Unity post-processing stack V2
	// Modified to not blur the ocean, removed dependency on other postprocessing classes I don't need here so we can initialize and make everything work by just adding this to a camera
	// Generating motion vectors for the ocean is both expensive and unnecessary so we should just not apply TAA on the ocean, otherwise it would just blur it without proper motion vectors
	public class TemporalAntiAliasing : GenericAntiAliasing
	{
		public float jitterSpread = 0.9f;				// The diameter (in texels) inside which jitter samples are spread. Smaller values result in crisper but more aliased output, while larger values result in more stable, but blurrier, output. Range(0.1f, 1f)
		public float sharpness = 0.25f;					// Controls the amount of sharpening applied to the color buffer. High values may introduce dark-border artifacts. Range(0f, 3f)
		public float stationaryBlending = 0.90f;		// The blend coefficient for a stationary fragment. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)
		public float motionBlending = 0.55f;			// The blend coefficient for a fragment with significant motion. Controls the percentage of history sample blended into the final color. Range(0f, 0.99f)

		public Vector2 jitter { get; private set; }		// The current jitter amount
		public int sampleIndex { get; private set; }	// The current sample index
		
		enum Pass { SolverDilate, SolverNoDilate }
		
		bool m_ResetHistory = true;
		bool hdrEnabled = false;
		
		const int k_SampleCount = 8;

		// Shared Halton sequence for cameras in the scaled/far/near group. Opt-in per instance
		// via useSharedJitter so each composited frame uses the same sub-pixel offset across
		// every camera that draws into the composite, while IVA / other cameras keep their own.
		private static int sharedSampleIndex;
		private static int sharedJitterFrame = -1;
		private static Vector2 sharedJitterOffset;

		private static Vector2 GetSharedJitterOffset()
		{
			if (Time.frameCount == sharedJitterFrame) return sharedJitterOffset;

			sharedJitterOffset = new Vector2(
				HaltonSeq.Get((sharedSampleIndex & 1023) + 1, 2) - 0.5f,
				HaltonSeq.Get((sharedSampleIndex & 1023) + 1, 3) - 0.5f);

			if (++sharedSampleIndex >= k_SampleCount) sharedSampleIndex = 0;
			sharedJitterFrame = Time.frameCount;
			return sharedJitterOffset;
		}

		// Ping-pong between two history textures as we can't read & write the same target in the same pass
		const int eyesCount = 2; const int historyTexturesCount = 2;
		RenderTexture[][] historyTextures = new RenderTexture[eyesCount][];

		int[] m_HistoryPingPong = new int [eyesCount];

		Camera targetCamera;
		CommandBuffer temporalAACommandBuffer;
		Material temporalAAMaterial;

		DepthTextureMode originalDepthTextureMode;
		public bool checkOceanDepth = false;
		public bool jitterTransparencies = false;
		public bool resetMotionVectors = true;
		public bool useSharedJitter = false;

		public enum Role { JitterAndResolve, JitterOnly }
		public Role role = Role.JitterAndResolve;
		private Role lastRole = Role.JitterAndResolve;
		private bool commandBufferAttached = false;

        private static int jitterProperty = Shader.PropertyToID("_Jitter");
        private static int keepPreviousMotionVectorsProperty = Shader.PropertyToID("TAA_KeepPreviousMotionVectors");
        private static int useFloatingOriginCameraMotionProperty = Shader.PropertyToID("TAA_UseFloatingOriginCameraMotion");
        private static int previousFrameTransformProperty = Shader.PropertyToID("TAA_PreviousFrameTransform");
        private static int useVolumetricCloudsMotionVectorsProperty = Shader.PropertyToID("TAA_UseVolumetricCloudsMotionVectors");

        private static CameraEvent TAACameraEvent = CameraEvent.AfterForwardAlpha;  // BeforeImageEffects doesn't work well

        private Vector3d previousCameraPosition;
        private Vector3d previousRenderCameraPosition;
        private Vector3d accumulatedFloatingOriginOffset;
        private int previousCameraFrame = -1;
        private bool previousCameraMapView;
        private bool registeredForFloatingOriginShifts;
        private bool registeredForVesselChanges;
        private const float vesselRendererMotionThreshold = 0.00001f;
        private readonly Dictionary<Vessel, List<VesselRendererMotionState>> vesselRendererMotionStates =
            new Dictionary<Vessel, List<VesselRendererMotionState>>();

        private class VesselRendererMotionState
        {
            public Renderer renderer;
            public Matrix4x4 previousLocalToWorldMatrix;
        }
		bool firstFrame = true;

		public void Awake()
		{
			targetCamera = GetComponent<Camera>();
            if (targetCamera == Scatterer.Instance.scaledSpaceCamera
                || targetCamera == Scatterer.Instance.farCamera
                || targetCamera == Scatterer.Instance.nearCamera)
            {
                GameEvents.onFloatingOriginShift.Add(OnFloatingOriginShift);
                registeredForFloatingOriginShifts = true;
            }

            if (targetCamera == Scatterer.Instance.nearCamera)
            {
                GameEvents.onVesselLoaded.Add(RegisterVesselRenderers);
                GameEvents.onVesselWasModified.Add(RegisterVesselRenderers);
                GameEvents.onVesselPartCountChanged.Add(RegisterVesselRenderers);
                GameEvents.onVesselUnloaded.Add(UnregisterVesselRenderers);
                GameEvents.onVesselWillDestroy.Add(UnregisterVesselRenderers);
                registeredForVesselChanges = true;

                foreach (Vessel vessel in FlightGlobals.VesselsLoaded)
                    RegisterVesselRenderers(vessel);
            }

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

            Shader.SetGlobalTexture(ShaderProperties.scattererReconstructedCloud_PROPERTY, Texture2D.whiteTexture); // This is used to blend cloud motion vectors with scene motion vectors, if raymarched volumetrics are loaded they override this

            temporalAAMaterial.SetInt(useVolumetricCloudsMotionVectorsProperty, targetCamera == Scatterer.Instance.nearCamera ? 1 : 0);

			temporalAACommandBuffer = new CommandBuffer();
			temporalAACommandBuffer.name = $"Scatterer TAA CommandBuffer for {targetCamera.name}";
	}

		internal DepthTextureMode GetCameraFlags()
		{
			return DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }

        private Role EffectiveRole()
        {
            if (role == Role.JitterAndResolve)
				return Role.JitterAndResolve;

            if (targetCamera == Scatterer.Instance.scaledSpaceCamera && MapView.MapIsEnabled)
                return Role.JitterAndResolve;

            return Role.JitterOnly;
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

		private static class RuntimeUtilities
		{
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
		
			public static Matrix4x4 GetJitteredOrthographicProjectionMatrix(Camera camera, Vector2 offset)
			{
				float vertical = camera.orthographicSize;
				float horizontal = vertical * camera.aspect;

				offset.x *= horizontal / (0.5f * camera.pixelWidth);
				offset.y *= vertical / (0.5f * camera.pixelHeight);

				float left = offset.x - horizontal;
				float right = offset.x + horizontal;
				float top = offset.y + vertical;
				float bottom = offset.y - vertical;

				return Matrix4x4.Ortho(left, right, bottom, top, camera.nearClipPlane, camera.farClipPlane);
			}

			public static Matrix4x4 GenerateJitteredProjectionMatrixFromOriginal(int screenWidth, int screenHeight, Matrix4x4 origProj, Vector2 jitter)
			{
				var planes = origProj.decomposeProjection;

				float vertFov = Mathf.Abs(planes.top) + Mathf.Abs(planes.bottom);
				float horizFov = Mathf.Abs(planes.left) + Mathf.Abs(planes.right);

				var planeJitter = new Vector2(jitter.x * horizFov / screenWidth,
					jitter.y * vertFov / screenHeight);

				planes.left += planeJitter.x;
				planes.right += planeJitter.x;
				planes.top += planeJitter.y;
				planes.bottom += planeJitter.y;

				var jitteredMatrix = Matrix4x4.Frustum(planes);

				return jitteredMatrix;
			}
		}
		
		/// Generates a jittered projection matrix for a given camera.
		public Matrix4x4 GetJitteredProjectionMatrix(Camera camera)
		{
			Matrix4x4 cameraProj;
			jitter = useSharedJitter ? GetSharedJitterOffset() : GenerateRandomOffset();
			jitter *= jitterSpread;

			cameraProj = camera.orthographic
				? RuntimeUtilities.GetJitteredOrthographicProjectionMatrix(camera, jitter)
				: RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(camera, jitter);

			jitter = new Vector2(jitter.x / camera.pixelWidth, jitter.y / camera.pixelHeight);
			return cameraProj;
		}
		
		
		/// Prepares the jittered and non jittered projection matrices
		public void ConfigureJitteredProjectionMatrix(Camera camera)
		{
			// camera.nonJitteredProjectionMatrix = camera.projectionMatrix; // should this be here?  it's in the stock postprocess code
			camera.projectionMatrix = GetJitteredProjectionMatrix(camera);
			camera.useJitteredProjectionMatrixForTransparentRendering = jitterTransparencies;
		}

		public void ConfigureStereoJitteredProjectionMatrices(Camera camera)
		{
            jitter = useSharedJitter ? GetSharedJitterOffset() : GenerateRandomOffset();
            jitter *= jitterSpread;

			// see PostProcessRenderContext.camera set property
			int screenWidth, screenHeight;
			if (camera.stereoEnabled)
			{
				screenWidth = XRSettings.eyeTextureWidth;
				screenHeight = XRSettings.eyeTextureHeight;
			}
			else
			{
				screenWidth = camera.pixelWidth;
				screenHeight = camera.pixelHeight;
			}

            for (var eye = Camera.StereoscopicEye.Left; eye <= Camera.StereoscopicEye.Right; eye++)
            {
                // This saves off the device generated projection matrices as non-jittered
                camera.CopyStereoDeviceProjectionMatrixToNonJittered(eye);
                var originalProj = camera.GetStereoNonJitteredProjectionMatrix(eye);

                // Currently no support for custom jitter func, as VR devices would need to provide
                // original projection matrix as input along with jitter
                var jitteredMatrix = RuntimeUtilities.GenerateJitteredProjectionMatrixFromOriginal(screenWidth, screenHeight, originalProj, jitter);
                camera.SetStereoProjectionMatrix(eye, jitteredMatrix);
            }

            // jitter has to be scaled for the actual eye texture size, not just the intermediate texture size
            // which could be double-wide in certain stereo rendering scenarios
            jitter = new Vector2(jitter.x / screenWidth, jitter.y / screenHeight);
            camera.useJitteredProjectionMatrixForTransparentRendering = jitterTransparencies;
		}
		
		RenderTexture CheckHistory(int id, CommandBuffer cmd, int activeEye)
		{	
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
		
        private void OnFloatingOriginShift(Vector3d offset, Vector3d nonFrameOffset)
        {
            // Celestial and terrain geometry is outside the Krakensbane frame, so its
            // authoritative shift includes the additional non-frame component.
            accumulatedFloatingOriginOffset += offset + nonFrameOffset;
        }

        private void RegisterVesselRenderers(Vessel vessel)
        {
            if (vessel == null)
                return;

            List<VesselRendererMotionState> rendererStates = new List<VesselRendererMotionState>();
            foreach (Part part in vessel.parts)
            {
                foreach (Renderer renderer in part.FindModelComponents<Renderer>())
                {
                    if (renderer == null)
                        continue;

                    renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    rendererStates.Add(new VesselRendererMotionState
                    {
                        renderer = renderer,
                        previousLocalToWorldMatrix = renderer.localToWorldMatrix
                    });
                }
            }

            vesselRendererMotionStates[vessel] = rendererStates;
        }

        private void UnregisterVesselRenderers(Vessel vessel)
        {
            if (vessel != null)
                vesselRendererMotionStates.Remove(vessel);
        }

        private static bool HasModelMatrixChanged(Matrix4x4 current, Matrix4x4 previous)
        {
            for (int i = 0; i < 16; i++)
            {
                if (Mathf.Abs(current[i] - previous[i]) > vesselRendererMotionThreshold)
                    return true;
            }

            return false;
        }

        private void UpdateVesselRendererMotionModes()
        {
            foreach (List<VesselRendererMotionState> rendererStates in vesselRendererMotionStates.Values)
            {
                for (int i = rendererStates.Count - 1; i >= 0; i--)
                {
                    VesselRendererMotionState rendererState = rendererStates[i];
                    if (rendererState.renderer == null)
                    {
                        rendererStates.RemoveAt(i);
                        continue;
                    }

                    Matrix4x4 currentLocalToWorldMatrix = rendererState.renderer.localToWorldMatrix;
                    MotionVectorGenerationMode motionVectorMode =
                        HasModelMatrixChanged(currentLocalToWorldMatrix, rendererState.previousLocalToWorldMatrix)
                            ? MotionVectorGenerationMode.Object
                            : MotionVectorGenerationMode.ForceNoMotion;

                    if (rendererState.renderer.motionVectorGenerationMode != motionVectorMode)
                        rendererState.renderer.motionVectorGenerationMode = motionVectorMode;

                    rendererState.previousLocalToWorldMatrix = currentLocalToWorldMatrix;
                }
            }
        }

        private void UpdateFloatingOriginCameraMotion()
        {
            bool isScaledCamera = targetCamera == Scatterer.Instance.scaledSpaceCamera;
            Shader.SetGlobalInt(useFloatingOriginCameraMotionProperty, registeredForFloatingOriginShifts ? 1 : 0);

            if (!registeredForFloatingOriginShifts)
                return;

            // Track floating-origin shifts directly in double precision. Comparing the resulting
            // real movement with movement already visible on the Unity camera isolates only the
            // translation missing from Unity's previous view-projection matrix.
            Vector3d currentRenderCameraPosition = targetCamera.transform.position;
            Vector3d currentCameraPosition = isScaledCamera
                ? (ScaledSpace.ScaledToLocalSpace(currentRenderCameraPosition) + accumulatedFloatingOriginOffset)
                    * ScaledSpace.InverseScaleFactor
                : currentRenderCameraPosition + accumulatedFloatingOriginOffset;

            Matrix4x4 previousFrameTransform = Matrix4x4.identity;
            Vector3d realCameraMovement = Vector3d.zero;
            Vector3d renderCameraMovement = Vector3d.zero;
            Vector3d missingCameraMovement = Vector3d.zero;
            bool mapView = MapView.MapIsEnabled;
            bool hasPreviousCameraFrame =
                previousCameraFrame == Time.frameCount - 1 && previousCameraMapView == mapView;
            if (hasPreviousCameraFrame)
            {
                realCameraMovement = currentCameraPosition - previousCameraPosition;
                renderCameraMovement = currentRenderCameraPosition - previousRenderCameraPosition;
                missingCameraMovement = realCameraMovement - renderCameraMovement;
                previousFrameTransform = Matrix4x4.Translate((Vector3)missingCameraMovement);
            }

            Shader.SetGlobalMatrix(previousFrameTransformProperty, previousFrameTransform);

            previousCameraPosition = currentCameraPosition;
            previousRenderCameraPosition = currentRenderCameraPosition;
            previousCameraFrame = Time.frameCount;
            previousCameraMapView = mapView;
        }

		//adapted from the original render() method
		public void OnPreCull()
		{
            if (registeredForVesselChanges)
                UpdateVesselRendererMotionModes();

            UpdateFloatingOriginCameraMotion();
            Shader.SetGlobalInt(ShaderProperties.ScattererUseCustomDepthTexture_PROPERTY,
                targetCamera == Scatterer.Instance.scaledSpaceCamera && targetCamera.actualRenderingPath == RenderingPath.DeferredShading ? 1 : 0);

			bool screenShotModeEnabled = GameSettings.TAKE_SCREENSHOT.GetKeyDown(false);

			float currentFps = 1.0f / Time.deltaTime;
			bool aboveFpsThreshold = currentFps >= Scatterer.Instance.mainSettings.disableTaaBelowFrameRateThreshold
				|| !Scatterer.Instance.mainSettings.useSubpixelMorphologicalAntialiasing;

			if (!screenShotModeEnabled && aboveFpsThreshold)
			{
				Role effectiveRole = EffectiveRole();
				if (effectiveRole != lastRole)
				{
					ResetHistory();
					firstFrame = true;
				}
				lastRole = effectiveRole;

				if (effectiveRole == Role.JitterAndResolve)
				{
					ConfigureJitteredProjectionMatrix(targetCamera);
					temporalAAMaterial.SetVector(jitterProperty, jitter);
					temporalAACommandBuffer.Clear();

					int activeEye = targetCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Right ? 1 : 0;

					int pingPongIndex = m_HistoryPingPong[activeEye];
					RenderTexture historyRead = CheckHistory(++pingPongIndex % 2, temporalAACommandBuffer, activeEye);
					RenderTexture historyWrite = CheckHistory(++pingPongIndex % 2, temporalAACommandBuffer, activeEye);
					m_HistoryPingPong[activeEye] = ++pingPongIndex % 2;

					if (firstFrame)
					{
						temporalAACommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, historyWrite);
					}
					else
					{
						//TODO: move to shader properties
						if (checkOceanDepth)
							Utils.EnableOrDisableShaderKeywords(temporalAAMaterial, "CUSTOM_OCEAN_ON", "CUSTOM_OCEAN_OFF", Scatterer.Instance.scattererCelestialBodiesManager.isCustomOceanEnabledOnScattererPlanet);

						temporalAAMaterial.SetTexture(ShaderProperties._HistoryTex_PROPERTY, historyRead);

						int pass = (int)Pass.SolverDilate;

						temporalAACommandBuffer.SetGlobalTexture(ShaderProperties._ScreenColor_PROPERTY, BuiltinRenderTextureType.CameraTarget);
						temporalAACommandBuffer.Blit(null, historyWrite, temporalAAMaterial, pass);

						temporalAACommandBuffer.Blit(historyWrite, BuiltinRenderTextureType.CameraTarget);
					}

					targetCamera.AddCommandBuffer(TAACameraEvent, temporalAACommandBuffer);
					commandBufferAttached = true;

					m_ResetHistory = false;
					firstFrame = false;
				}
				else
				{
					ConfigureJitteredProjectionMatrix(targetCamera);
				}

				if (resetMotionVectors)
				{
					Shader.SetGlobalInt(keepPreviousMotionVectorsProperty, 0);
				}
				else
				{
                    Shader.SetGlobalInt(keepPreviousMotionVectorsProperty, 1);
                }
			}
			else
            {
				firstFrame = true;
			}
		}

		public void OnPostRender()
		{
			ResetProjection();
			if (commandBufferAttached)
			{
				targetCamera.RemoveCommandBuffer(TAACameraEvent, temporalAACommandBuffer);
				commandBufferAttached = false;
			}
		}

		// This is needed otherwise transparencies jitter
		public void ResetProjection()
        {
			targetCamera.ResetProjectionMatrix();
			targetCamera.nonJitteredProjectionMatrix = targetCamera.projectionMatrix;
		}

		public void OnDestroy()
		{
            if (registeredForFloatingOriginShifts)
                GameEvents.onFloatingOriginShift.Remove(OnFloatingOriginShift);

            if (registeredForVesselChanges)
            {
                GameEvents.onVesselLoaded.Remove(RegisterVesselRenderers);
                GameEvents.onVesselWasModified.Remove(RegisterVesselRenderers);
                GameEvents.onVesselPartCountChanged.Remove(RegisterVesselRenderers);
                GameEvents.onVesselUnloaded.Remove(UnregisterVesselRenderers);
                GameEvents.onVesselWillDestroy.Remove(UnregisterVesselRenderers);
                vesselRendererMotionStates.Clear();
            }

			if (commandBufferAttached && temporalAACommandBuffer != null)
			{
				targetCamera.RemoveCommandBuffer(TAACameraEvent, temporalAACommandBuffer);
				commandBufferAttached = false;
			}

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

			sampleIndex = 0;
			m_HistoryPingPong[0] = 0;
			m_HistoryPingPong[1] = 1;

			ResetHistory();

			targetCamera.ResetProjectionMatrix();
		}
	}
}
