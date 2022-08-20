/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 * 
 * 
 */

using UnityEngine;

namespace Scatterer {
	/*
	 * Extend the base class OceanNode to provide the data need 
	 * to create the waves using fourier transform which can then be applied
	 * to the projected grid handled by the OceanNode.
	 * All the fourier transforms are performed on the GPU
	 */
	public class OceanFFTgpu: OceanNode {

		WriteFloat m_writeFloat;

		public float WAVE_CM = 0.23f;	// Eq 59
		public float WAVE_KM = 370.0f;	// Eq 59

		[Persistent] public float AMP = 1.0f;
		
		Material m_initSpectrumMat, m_initDisplacementMat;
		
		public int mapsAniso = 2;

		[Persistent] public float m_windSpeed = 5.0f;							//A higher wind speed gives greater swell to the waves
		[Persistent] public float m_omega = 0.84f;								//A lower number means the waves last longer and will build up larger waves
		[Persistent] public float m_gravity = 9.81f;

		public Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);				//Size in meters (i.e. in spatial domain) of each grid
		public Vector4 m_choppyness = new Vector4(2.3f, 2.1f, 1.3f, 0.9f);		//strenght of sideways displacement for each grid

		public int m_fourierGridSize = 64;										//This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.
		
		private int m_varianceSize = SystemInfo.supportsComputeShaders ? 16 : 4;
		
		float m_fsize;
		float m_maxSlopeVariance;
		protected int m_idx = 0;
		protected Vector4 m_spectrumOffset;
		protected Vector4 m_inverseGridSizes;
		
		protected RenderTexture m_spectrum01, m_spectrum23;
		protected RenderTexture m_WTable;
		
		public bool rendertexturesCreated {
			get {
				return (m_WTable.IsCreated());
			}
		}

		RenderTexture[] m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2;
		RenderTexture[] m_fourierBuffer3, m_fourierBuffer4;
		public RenderTexture m_map0, m_map1, m_map2, m_map3, m_map4;

		private ComputeShader m_varianceShader;
		RenderTexture m_varianceRenderTexture;
		
		Texture3D m_varianceTexture;
		Vector2 m_varianceMax;
		
		protected FourierGPU m_fourier;
		
		public override float GetMaxSlopeVariance()
		{
			return m_maxSlopeVariance;
		}

		[Persistent] public float maxWaveInteractionShipAltitude = 500.0f;

		// These are all oceanWhiteCaps settings but I add them here so they don't mess up serialization of gui
		public float m_foamMipMapBias = -2.0f;		
		[Persistent] public float m_whiteCapStr = 0.1f;		
		[Persistent] public float shoreFoam = 1.0f;		
		[Persistent] public float m_farWhiteCapStr = 0.1f;

		private GPUWaveInteractionHandler waveInteractionHandler = null;

		public override void Init(ProlandManager manager)
		{
			base.Init(manager);

			if (m_gravity == 0f)
			{
				m_gravity = (float)(prolandManager.parentCelestialBody.GeeASL * 9.81d);
			}
		
			m_initSpectrumMat = new Material(ShaderReplacer.Instance.LoadedShaders[ ("Proland/Ocean/InitSpectrum")]);
			m_initDisplacementMat = new Material(ShaderReplacer.Instance.LoadedShaders[ ("Proland/Ocean/InitDisplacement")]);

			m_fourierGridSize = Scatterer.Instance.mainSettings.m_fourierGridSize;
			
			if (m_fourierGridSize > 256) {
				Utils.LogDebug("Proland::OceanFFT::Start	- fourier grid size must not be greater than 256, changing to 256");
				m_fourierGridSize = 256;
			}
			
			if (!Mathf.IsPowerOfTwo(m_fourierGridSize)) {
				Utils.LogDebug("Proland::OceanFFT::Start	- fourier grid size must be pow2 number, changing to nearest pow2 number");
				m_fourierGridSize = Mathf.NextPowerOfTwo(m_fourierGridSize);
			}
			
			m_fsize = (float) m_fourierGridSize;
			m_spectrumOffset = new Vector4(1.0f + 0.5f / m_fsize, 1.0f + 0.5f / m_fsize, 0, 0);
			
			
			float factor = 2.0f * Mathf.PI * m_fsize;
			m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);
			
			
			m_fourier = new FourierGPU(m_fourierGridSize);
			
			m_writeFloat = new WriteFloat(m_fourierGridSize, m_fourierGridSize);

			//Create the data needed to make the waves each frame
			CreateRenderTextures();
			GenerateWavesSpectrum();
			CreateWTable();
			
			m_initSpectrumMat.SetTexture (ShaderProperties._Spectrum01_PROPERTY, m_spectrum01);
			m_initSpectrumMat.SetTexture (ShaderProperties._Spectrum23_PROPERTY, m_spectrum23);
			m_initSpectrumMat.SetTexture (ShaderProperties._WTable_PROPERTY, m_WTable);
			m_initSpectrumMat.SetVector (ShaderProperties._Offset_PROPERTY, m_spectrumOffset);
			m_initSpectrumMat.SetVector (ShaderProperties._InverseGridSizes_PROPERTY, m_inverseGridSizes);
			
			m_initDisplacementMat.SetVector (ShaderProperties._InverseGridSizes_PROPERTY, m_inverseGridSizes);

			m_oceanMaterial.SetVector (ShaderProperties._Ocean_MapSize_PROPERTY, new Vector2(m_fsize, m_fsize));
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_Choppyness_PROPERTY, m_choppyness);
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_GridSizes_PROPERTY, m_gridSizes);

			Utils.EnableOrDisableShaderKeywords (m_oceanMaterial, "FOAM_ON", "FOAM_OFF", false);

			if (SystemInfo.supportsAsyncGPUReadback && SystemInfo.supportsComputeShaders && Scatterer.Instance.mainSettings.oceanCraftWaveInteractions)
			{
				waveInteractionHandler = new GPUWaveInteractionHandler(maxWaveInteractionShipAltitude, prolandManager.parentCelestialBody.isHomeWorld);
			}
		}
		
		Vector2 GetSlopeVariances(Vector2 k, float A, float B, float C, float spectrumX, float spectrumY) {
			float w = 1.0f - Mathf.Exp(A * k.x * k.x + B * k.x * k.y + C * k.y * k.y);
			return new Vector2((k.x * k.x) * w, (k.y * k.y) * w) * (spectrumX * spectrumX + spectrumY * spectrumY) * 2.0f;
		}
		

		/// <summary>
		/// Iterate over the spectrum and find the variance.
		/// Use in the BRDF equations.
		/// </summary>
		Vector2 ComputeVariance(float slopeVarianceDelta, float[] inSpectrum01, float[] inSpectrum23, float idxX, float idxY, float idxZ) {
			const float SCALE = 10.0f;
			
			float A = Mathf.Pow(idxX / ((float) m_varianceSize - 1.0f), 4.0f) * SCALE;
			float C = Mathf.Pow(idxZ / ((float) m_varianceSize - 1.0f), 4.0f) * SCALE;
			float B = (2.0f * idxY / ((float) m_varianceSize - 1.0f) - 1.0f) * Mathf.Sqrt(A * C);
			A = -0.5f * A;
			B = -B;
			C = -0.5f * C;
			
			Vector2 slopeVariances = new Vector2(slopeVarianceDelta, slopeVarianceDelta);
			
			for (int x = 0; x < m_fourierGridSize; x++) {
				for (int y = 0; y < m_fourierGridSize; y++) {
					int i = x >= m_fsize / 2.0f ? x - m_fourierGridSize : x;
					int j = y >= m_fsize / 2.0f ? y - m_fourierGridSize : y;
					
					Vector2 k = new Vector2(i, j) * 2.0f * Mathf.PI;
					
					slopeVariances += GetSlopeVariances(k / m_gridSizes.x, A, B, C, inSpectrum01[(x + y * m_fourierGridSize) * 4 + 0], inSpectrum01[(x + y * m_fourierGridSize) * 4 + 1]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.y, A, B, C, inSpectrum01[(x + y * m_fourierGridSize) * 4 + 2], inSpectrum01[(x + y * m_fourierGridSize) * 4 + 3]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.z, A, B, C, inSpectrum23[(x + y * m_fourierGridSize) * 4 + 0], inSpectrum23[(x + y * m_fourierGridSize) * 4 + 1]);
					slopeVariances += GetSlopeVariances(k / m_gridSizes.w, A, B, C, inSpectrum23[(x + y * m_fourierGridSize) * 4 + 2], inSpectrum23[(x + y * m_fourierGridSize) * 4 + 3]);
				}
			}
			
			return slopeVariances;
		}
		
		
		/*
		 * Initializes the data to the shader that needs to 
		 * have the fourier transform applied to it this frame.
		 */
		protected virtual void InitWaveSpectrum(float t)
		{
			// init heights (0) and slopes (1,2)
			RenderTexture[] buffers012 = new RenderTexture[]
			{
				m_fourierBuffer0[1], m_fourierBuffer1[1], m_fourierBuffer2[1]
			};
			m_initSpectrumMat.SetFloat (ShaderProperties._T_PROPERTY, t);
			RTUtility.MultiTargetBlit(buffers012, m_initSpectrumMat, 0);
			
			// Init displacement (3,4)
			RenderTexture[] buffers34 = new RenderTexture[]
			{
				m_fourierBuffer3[1], m_fourierBuffer4[1]
			};
			m_initDisplacementMat.SetTexture (ShaderProperties._Buffer1_PROPERTY, m_fourierBuffer1[1]);
			m_initDisplacementMat.SetTexture (ShaderProperties._Buffer2_PROPERTY, m_fourierBuffer2[1]);
			//			RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat);
			RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat, 0);
		}
	

		public override void UpdateNode()
		{
			if (!prolandManager.skyNode.inScaledSpace || (prolandManager.skyNode.simulateOceanInteraction && (waveInteractionHandler != null)))
			{
				//keep within low float exponents, otherwise we lose precision
				float t = (float)(Planetarium.GetUniversalTime() % 100000.0);

				InitWaveSpectrum(t);
				
				//Perform fourier transform and record what is the current index
				m_idx = m_fourier.PeformFFT(m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2);
				m_fourier.PeformFFT(m_fourierBuffer3, m_fourierBuffer4);
				
				//Copy the contents of the completed fourier transform to the map textures.
				//You could just use the buffer textures (m_fourierBuffer0,1,2,etc) to read from for the ocean shader 
				//but they need to have mipmaps and unity updates the mipmaps
				//every time the texture is renderer into. This impacts performance during fourier transform stage as mipmaps would be updated every pass
				//and there is no way to disable and then enable mipmaps on render textures in Unity at time of writting.
				
				Graphics.Blit(m_fourierBuffer0[m_idx], m_map0);
				Graphics.Blit(m_fourierBuffer1[m_idx], m_map1);
				Graphics.Blit(m_fourierBuffer2[m_idx], m_map2);
				Graphics.Blit(m_fourierBuffer3[m_idx], m_map3);
				Graphics.Blit(m_fourierBuffer4[m_idx], m_map4);
				
				//m_oceanMaterial.SetFloat (ShaderProperties._Ocean_HeightOffset_PROPERTY, m_oceanLevel);				
				m_oceanMaterial.SetFloat (ShaderProperties._Ocean_HeightOffset_PROPERTY, 0f);
				if (SystemInfo.supportsComputeShaders)
				{
					m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Variance_PROPERTY, m_varianceRenderTexture );
				}
				else
				{
					m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Variance_PROPERTY, m_varianceTexture);
				}
				m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map0_PROPERTY, m_map0);
				m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map1_PROPERTY, m_map1);
				m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map2_PROPERTY, m_map2);
				m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map3_PROPERTY, m_map3);
				m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map4_PROPERTY, m_map4);
				m_oceanMaterial.SetVector (ShaderProperties._VarianceMax_PROPERTY, SystemInfo.supportsComputeShaders? Vector2.one : m_varianceMax);
				
				// don't need to be done every frame
				if (waveInteractionHandler != null)
				{
					waveInteractionHandler.SetMaterialProperties(m_choppyness, m_gridSizes, m_map0, m_map3, m_map4);
				}
			}

			base.UpdateNode();
		}
		
		public override void Cleanup()
		{
			base.Cleanup();
			
			m_map0.Release();
			m_map1.Release();
			m_map2.Release();
			m_map3.Release();
			m_map4.Release();
			
			m_spectrum01.Release();
			m_spectrum23.Release();
			
			m_WTable.Release();
			
			for (int i = 0; i < 2; i++) {
				m_fourierBuffer0[i].Release();
				m_fourierBuffer1[i].Release();
				m_fourierBuffer2[i].Release();
				m_fourierBuffer3[i].Release();
				m_fourierBuffer4[i].Release();
			}

			if (waveInteractionHandler != null)
            {
				waveInteractionHandler.Cleanup();
			}
		}
		
		protected virtual void CreateRenderTextures()
		{
			RenderTextureFormat mapFormat = RenderTextureFormat.ARGBFloat;
			RenderTextureFormat fourierTransformformat = RenderTextureFormat.ARGBHalf;
			
			//These texture hold the actual data use in the ocean renderer
			CreateMap(ref m_map0, mapFormat, mapsAniso, true);
			CreateMap(ref m_map1, mapFormat, mapsAniso, true);
			CreateMap(ref m_map2, mapFormat, mapsAniso, true);
			CreateMap(ref m_map3, mapFormat, mapsAniso, true);
			CreateMap(ref m_map4, mapFormat, mapsAniso, true);
			
			//These textures are used to perform the fourier transform
			CreateBuffer(ref m_fourierBuffer0, fourierTransformformat); //heights
			CreateBuffer(ref m_fourierBuffer1, fourierTransformformat); // slopes X
			CreateBuffer(ref m_fourierBuffer2, fourierTransformformat); // slopes Y
			CreateBuffer(ref m_fourierBuffer3, fourierTransformformat); // displacement X
			CreateBuffer(ref m_fourierBuffer4, fourierTransformformat); // displacement Y
			
			//These textures hold the specturm the fourier transform is performed on
			m_spectrum01 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, fourierTransformformat);
			m_spectrum01.filterMode = FilterMode.Point;
			m_spectrum01.wrapMode = TextureWrapMode.Repeat;
			m_spectrum01.enableRandomWrite = false;
			m_spectrum01.Create();
			
			m_spectrum23 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, fourierTransformformat);
			m_spectrum23.filterMode = FilterMode.Point;
			m_spectrum23.wrapMode = TextureWrapMode.Repeat;
			m_spectrum23.enableRandomWrite = false;
			m_spectrum23.Create();
			

			m_WTable = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, fourierTransformformat);
			m_WTable.filterMode = FilterMode.Point;
			m_WTable.wrapMode = TextureWrapMode.Clamp;
			m_WTable.enableRandomWrite = false;
			m_WTable.Create();

			if (SystemInfo.supportsComputeShaders)
			{
				m_varianceRenderTexture = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RHalf);
				m_varianceRenderTexture.volumeDepth = m_varianceSize;
				m_varianceRenderTexture.wrapMode = TextureWrapMode.Clamp;
				m_varianceRenderTexture.filterMode = FilterMode.Bilinear;
				m_varianceRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
				m_varianceRenderTexture.enableRandomWrite = true;
				m_varianceRenderTexture.useMipMap = true;
				m_varianceRenderTexture.Create();
			}
			else
			{
				m_varianceTexture = new Texture3D (m_varianceSize, m_varianceSize, m_varianceSize, TextureFormat.ARGB32, true);
				m_varianceTexture.wrapMode = TextureWrapMode.Clamp;
				m_varianceTexture.filterMode = FilterMode.Bilinear;
			}
			
		}
		
		protected void CreateBuffer(ref RenderTexture[] tex, RenderTextureFormat format)
		{
			tex = new RenderTexture[2];
			
			for (int i = 0; i < 2; i++)
			{
				tex[i] = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
				tex[i].filterMode = FilterMode.Point;
				tex[i].wrapMode = TextureWrapMode.Clamp;
				tex[i].Create();
			}
		}
		
		protected void CreateMap(ref RenderTexture map, RenderTextureFormat format, int ansio, bool useMipMaps)
		{
			map = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);
			map.filterMode = FilterMode.Trilinear;
			map.wrapMode = TextureWrapMode.Repeat;
			map.anisoLevel = ansio;
			map.useMipMap = useMipMaps;
			map.Create();
		}
		
		float sqr(float x) {
			return x * x;
		}
		
		float omega(float k)
		{
			return Mathf.Sqrt(m_gravity * k * (1.0f + sqr(k / WAVE_KM)));
		} // Eq 24
		
		float Spectrum(float kx, float ky, bool omnispectrum)
		{
			//I know this is a big chunk of ugly math but dont worry to much about what it all means
			//It recreates a statistcally representative model of a wave spectrum in the frequency domain.

			//How to rotate the windDirection in the spectrum: https://github.com/Scrawk/Ceto/blob/07f8f45955a989983fe2330ea17eaf3f44c82031/Assets/Ceto/Scripts/Spectrum/CustomWaveSpectrumExample.cs#L248-L250
			//But I prefere to rotate the Ux and Uy axis, that way can change the wind direction without regenerating spectrum

			float U10 = m_windSpeed;
			
			// phase speed
			float k = Mathf.Sqrt(kx * kx + ky * ky);
			float c = omega(k) / k;
			
			// spectral peak
			float kp = m_gravity * sqr(m_omega / U10); // after Eq 3
			float cp = omega(kp) / kp;
			
			// friction velocity
			float z0 = 3.7e-5f * sqr(U10) / m_gravity * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
			float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60
			
			float Lpm = Mathf.Exp(-5.0f / 4.0f * sqr(kp / k)); // after Eq 3
			float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
			float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
			float Gamma = Mathf.Exp(-1.0f / (2.0f * sqr(sigma)) * sqr(Mathf.Sqrt(k / kp) - 1.0f));
			float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
			float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
			float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
			float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31
			
			float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
			float Fm = Mathf.Exp(-0.25f * sqr(k / WAVE_KM - 1.0f)); // Eq 41
			float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)
			
			Bh *= Lpm; // bug fix???
			
			if (omnispectrum) return AMP * (Bl + Bh) / (k * sqr(k)); // Eq 30
			
			float a0 = Mathf.Log(2.0f) / 4.0f;
			float ap = 4.0f;
			float am = 0.13f * u_star / WAVE_CM; // Eq 59
			float Delta = (float) System.Math.Tanh(a0 + ap * Mathf.Pow(c / cp, 2.5f) + am * Mathf.Pow(WAVE_CM / c, 2.5f)); // Eq 57
			
			float phi = Mathf.Atan2(ky, kx);
			
			if (kx < 0.0f) return 0.0f;
			
			Bl *= 2.0f;
			Bh *= 2.0f;
			
			// remove waves perpendicular to wind dir
			float tweak = Mathf.Sqrt(Mathf.Max(kx / Mathf.Sqrt(kx * kx + ky * ky), 0.0f));
			
			return AMP * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * sqr(sqr(k))) * tweak; // Eq 67
		}
		
		Vector2 GetSpectrumSample(float i, float j, float lengthScale, float kMin)
		{
			float dk = 2.0f * Mathf.PI / lengthScale;
			float kx = i * dk;
			float ky = j * dk;
			Vector2 result = new Vector2(0.0f, 0.0f);
			
			float rnd = UnityEngine.Random.value;
			
			if (Mathf.Abs(kx) >= kMin || Mathf.Abs(ky) >= kMin) {
				float S = Spectrum(kx, ky, false);
				float h = Mathf.Sqrt(S / 2.0f) * dk;
				
				float phi = rnd * 2.0f * Mathf.PI;
				result.x = h * Mathf.Cos(phi);
				result.y = h * Mathf.Sin(phi);
			}
			
			return result;
		}
		
		float GetSlopeVariance(float kx, float ky, Vector2 spectrumSample)
		{
			float kSquare = kx * kx + ky * ky;
			float real = spectrumSample.x;
			float img = spectrumSample.y;
			float hSquare = real * real + img * img;
			return kSquare * hSquare * 2.0f;
		}
		
		void GenerateWavesSpectrum()
		{
			// Slope variance due to all waves, by integrating over the full spectrum.
			// Used by the BRDF rendering model
			float theoreticSlopeVariance = 0.0f;
			float k = 5e-3f;
			while (k < 1e3f) {
				float nextK = k * 1.001f;
				theoreticSlopeVariance += k * k * Spectrum(k, 0, true) * (nextK - k);
				k = nextK;
			}
			
			float[] spectrum01 = new float[m_fourierGridSize * m_fourierGridSize * 4];
			float[] spectrum23 = new float[m_fourierGridSize * m_fourierGridSize * 4];

			int idx;
			float i;
			float j;
			float totalSlopeVariance = 0.0f;
			Vector2 sample12XY;
			Vector2 sample12ZW;
			Vector2 sample34XY;
			Vector2 sample34ZW;
			
			UnityEngine.Random.seed = 0;
			
			for (int x = 0; x < m_fourierGridSize; x++)
			{
				for (int y = 0; y < m_fourierGridSize; y++)
				{
					idx = x + y * m_fourierGridSize;
					i = (x >= m_fourierGridSize / 2) ? (float)(x - m_fourierGridSize) : (float) x;
					j = (y >= m_fourierGridSize / 2) ? (float)(y - m_fourierGridSize) : (float) y;
					
					sample12XY = GetSpectrumSample(i, j, m_gridSizes.x, Mathf.PI / m_gridSizes.x);
					sample12ZW = GetSpectrumSample(i, j, m_gridSizes.y, Mathf.PI * m_fsize / m_gridSizes.x);
					sample34XY = GetSpectrumSample(i, j, m_gridSizes.z, Mathf.PI * m_fsize / m_gridSizes.y);
					sample34ZW = GetSpectrumSample(i, j, m_gridSizes.w, Mathf.PI * m_fsize / m_gridSizes.z);

					spectrum01[idx * 4 + 0] = sample12XY.x;
					spectrum01[idx * 4 + 1] = sample12XY.y;
					spectrum01[idx * 4 + 2] = sample12ZW.x;
					spectrum01[idx * 4 + 3] = sample12ZW.y;
					
					spectrum23[idx * 4 + 0] = sample34XY.x;
					spectrum23[idx * 4 + 1] = sample34XY.y;
					spectrum23[idx * 4 + 2] = sample34ZW.x;
					spectrum23[idx * 4 + 3] = sample34ZW.y;
					
					i *= 2.0f * Mathf.PI;
					j *= 2.0f * Mathf.PI;
					
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.x, j / m_gridSizes.x, sample12XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.y, j / m_gridSizes.y, sample12ZW);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.z, j / m_gridSizes.z, sample34XY);
					totalSlopeVariance += GetSlopeVariance(i / m_gridSizes.w, j / m_gridSizes.w, sample34ZW);
				}
			}

			//This can be replaced by a texture2D now and the whole thing with a set raw data + apply
			m_writeFloat.WriteIntoRenderTexture(m_spectrum01, 4, spectrum01);
			m_writeFloat.WriteIntoRenderTexture(m_spectrum23, 4, spectrum23);
			
			if (!SystemInfo.supportsComputeShaders)
			{
				// Compute variance for the BRDF, copied from the dx9 project
				float slopeVarianceDelta = 0.5f * (theoreticSlopeVariance - totalSlopeVariance);

				m_varianceMax = new Vector2 (float.NegativeInfinity, float.NegativeInfinity);
			
				Vector2[, , ] variance32bit = new Vector2[m_varianceSize, m_varianceSize, m_varianceSize];
				Color[] variance8bit = new Color[m_varianceSize * m_varianceSize * m_varianceSize];

				for (int x = 0; x < m_varianceSize; x++)
				{
					for (int y = 0; y < m_varianceSize; y++)
					{
						for (int z = 0; z < m_varianceSize; z++)
						{
							variance32bit [x, y, z] = ComputeVariance (slopeVarianceDelta, spectrum01, spectrum23, x, y, z);

							if (variance32bit [x, y, z].x > m_varianceMax.x)
								m_varianceMax.x = variance32bit [x, y, z].x;
							if (variance32bit [x, y, z].y > m_varianceMax.y)
								m_varianceMax.y = variance32bit [x, y, z].y;
						}
					}
				}
			
				for (int x = 0; x < m_varianceSize; x++)
				{
					for (int y = 0; y < m_varianceSize; y++)
					{
						for (int z = 0; z < m_varianceSize; z++)
						{
							idx = x + y * m_varianceSize + z * m_varianceSize * m_varianceSize;
						
							variance8bit [idx] = new Color (variance32bit [x, y, z].x / m_varianceMax.x, variance32bit [x, y, z].y / m_varianceMax.y, 0.0f, 1.0f);
						}
					}
				}
			
				m_varianceTexture.SetPixels (variance8bit);
				m_varianceTexture.Apply ();

				m_maxSlopeVariance = 0.0f;
				for (int v = 0; v < m_varianceSize*m_varianceSize*m_varianceSize; v++)
				{
					m_maxSlopeVariance = Mathf.Max (m_maxSlopeVariance, variance8bit [v].r * m_varianceMax.x);
					m_maxSlopeVariance = Mathf.Max (m_maxSlopeVariance, variance8bit [v].g * m_varianceMax.y);
				}
			}
			else
			{
				m_varianceShader = ShaderReplacer.Instance.LoadedComputeShaders["SlopeVariance"];

				m_varianceShader.SetFloat("_SlopeVarianceDelta", 0.5f * (theoreticSlopeVariance - totalSlopeVariance));
				m_varianceShader.SetFloat("_VarianceSize", (float)m_varianceSize);
				m_varianceShader.SetFloat("_Size", m_fsize);
				m_varianceShader.SetVector("_GridSizes", m_gridSizes);
				m_varianceShader.SetTexture(0, "_Spectrum01", m_spectrum01);
				m_varianceShader.SetTexture(0, "_Spectrum23", m_spectrum23);
				m_varianceShader.SetTexture(0, "des", m_varianceRenderTexture);
				
				m_varianceShader.Dispatch(0,m_varianceSize/4,m_varianceSize/4,m_varianceSize/4);
				
				//Find the maximum value for slope variance
				ComputeBuffer buffer = new ComputeBuffer(m_varianceSize*m_varianceSize*m_varianceSize, sizeof(float));
				CBUtility.ReadFromRenderTexture(m_varianceRenderTexture, 1, buffer, ShaderReplacer.Instance.LoadedComputeShaders["ReadData"]);
				
				float[] varianceData = new float[m_varianceSize*m_varianceSize*m_varianceSize];
				buffer.GetData(varianceData);
				
				m_maxSlopeVariance = 0.0f;
				for(int v = 0; v < m_varianceSize*m_varianceSize*m_varianceSize; v++) {
					m_maxSlopeVariance = Mathf.Max(m_maxSlopeVariance, varianceData[v]);
				}
				
				buffer.Release();
			}
			
		}
		
		void CreateWTable()
		{
			//Some values need for the InitWaveSpectrum function can be precomputed
			Vector2 uv, st;
			float k1, k2, k3, k4, w1, w2, w3, w4;
			
			float[] table = new float[m_fourierGridSize * m_fourierGridSize * 4];
			
			for (int x = 0; x < m_fourierGridSize; x++) {
				for (int y = 0; y < m_fourierGridSize; y++) {
					uv = new Vector2(x, y) / m_fsize;
					
					st.x = uv.x > 0.5f ? uv.x - 1.0f : uv.x;
					st.y = uv.y > 0.5f ? uv.y - 1.0f : uv.y;
					
					k1 = (st * m_inverseGridSizes.x).magnitude;
					k2 = (st * m_inverseGridSizes.y).magnitude;
					k3 = (st * m_inverseGridSizes.z).magnitude;
					k4 = (st * m_inverseGridSizes.w).magnitude;
					
					w1 = Mathf.Sqrt(m_gravity * k1 * (1.0f + k1 * k1 / (WAVE_KM * WAVE_KM)));
					w2 = Mathf.Sqrt(m_gravity * k2 * (1.0f + k2 * k2 / (WAVE_KM * WAVE_KM)));
					w3 = Mathf.Sqrt(m_gravity * k3 * (1.0f + k3 * k3 / (WAVE_KM * WAVE_KM)));
					w4 = Mathf.Sqrt(m_gravity * k4 * (1.0f + k4 * k4 / (WAVE_KM * WAVE_KM)));
					
					table[(x + y * m_fourierGridSize) * 4 + 0] = w1;
					table[(x + y * m_fourierGridSize) * 4 + 1] = w2;
					table[(x + y * m_fourierGridSize) * 4 + 2] = w3;
					table[(x + y * m_fourierGridSize) * 4 + 3] = w4;
					
				}
			}

			m_writeFloat.WriteIntoRenderTexture(m_WTable, 4, table);
		}
		
		Vector2 GetSpectrum(float t, float w, float s0x, float s0y, float s0cx, float s0cy)
		{
			float c = Mathf.Cos(w * t);
			float s = Mathf.Sin(w * t);
			return new Vector2((s0x + s0cx) * c - (s0y + s0cy) * s, (s0x - s0cx) * s + (s0y - s0cy) * c);
		}
		
		Vector2 COMPLEX(Vector2 z)
		{
			return new Vector2(-z.y, z.x); // returns i times z (complex number)
		}
		
		int BitReverse(int i)
		{
			int j = i;
			int Sum = 0;
			int W = 1;
			int M = m_fourierGridSize / 2;
			while (M != 0)
			{
				j = ((i & M) > M - 1) ? 1 : 0;
				Sum += j * W;
				W *= 2;
				M /= 2;
			}
			return Sum;
		}

		//FixedUpdate is responsible for physics, apply part displacement here
		public void FixedUpdate()
		{
			if (waveInteractionHandler != null && prolandManager.GetSkyNode().simulateOceanInteraction)
			{
				waterHeightAtCameraPosition = waveInteractionHandler.UpdateInteractions(height, waterHeightAtCameraPosition, ux.ToVector3(), uy.ToVector3(), offsetVector3);
			}
		}
	}
	
}