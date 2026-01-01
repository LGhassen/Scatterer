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

namespace Scatterer
{
    /*
     * Extend the base class OceanNode to provide the data need 
     * to create the waves using fourier transform which can then be applied
     * to the projected grid handled by the OceanNode.
     * All the fourier transforms are performed on the GPU
     */
    public class OceanFFTgpu: OceanNode
    {
        public float WAVE_CM = 0.23f;    // Eq 59
        public float WAVE_KM = 370.0f;    // Eq 59

        [Persistent] public float AMP = 1.0f;
        
        Material m_initSpectrumMat, m_initDisplacementMat;
        
        public int mapsAniso = 0;

        [Persistent] public float m_windSpeed = 5.0f;                            //A higher wind speed gives greater swell to the waves
        [Persistent] public float m_omega = 0.84f;                                //A lower number means the waves last longer and will build up larger waves
        [Persistent] public float m_gravity = 9.81f;

        public Vector4 m_gridSizes = new Vector4(5488, 392, 28, 2);                //Size in meters (i.e. in spatial domain) of each grid
        public Vector4 m_choppyness = new Vector4(2.3f, 2.1f, 1.3f, 0.9f);        //strenght of sideways displacement for each grid

        public int m_fourierGridSize = 64;                                      //This is the fourier transform size, must pow2 number. Recommend no higher or lower than 64, 128 or 256.

        private int m_varianceSize = 16;

        float m_fsize;
        float m_maxSlopeVariance;
        protected int m_idx = 0;
        protected Vector4 m_spectrumOffset;
        protected Vector4 m_inverseGridSizes;
        
        protected RenderTexture m_spectrum01, m_spectrum23;
        protected Texture2D m_WTable;

        RenderTexture[] m_fourierBuffer0, m_fourierBuffer1, m_fourierBuffer2;
        RenderTexture[] m_fourierBuffer3, m_fourierBuffer4;

        RenderTexture normalizedVarianceRenderTexture;
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

        public void Awake()
        {
            
        }

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
                Utils.LogDebug("Proland::OceanFFT::Start    - fourier grid size must not be greater than 256, changing to 256");
                m_fourierGridSize = 256;
            }
            
            if (!Mathf.IsPowerOfTwo(m_fourierGridSize)) {
                Utils.LogDebug("Proland::OceanFFT::Start    - fourier grid size must be pow2 number, changing to nearest pow2 number");
                m_fourierGridSize = Mathf.NextPowerOfTwo(m_fourierGridSize);
            }
            
            m_fsize = (float) m_fourierGridSize;
            m_spectrumOffset = new Vector4(1.0f + 0.5f / m_fsize, 1.0f + 0.5f / m_fsize, 0, 0);
            
            
            float factor = 2.0f * Mathf.PI * m_fsize;
            m_inverseGridSizes = new Vector4(factor / m_gridSizes.x, factor / m_gridSizes.y, factor / m_gridSizes.z, factor / m_gridSizes.w);
            
            m_fourier = new FourierGPU(m_fourierGridSize);

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
        
        Vector2 GetSlopeVariances(Vector2 k, float A, float B, float C, float spectrumX, float spectrumY)
        {
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
            //            RTUtility.MultiTargetBlit(buffers34, m_initDisplacementMat);
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
                
                m_fourierBuffer0[m_idx].GenerateMips();
                m_fourierBuffer1[m_idx].GenerateMips();
                m_fourierBuffer2[m_idx].GenerateMips();
                m_fourierBuffer3[m_idx].GenerateMips();
                m_fourierBuffer4[m_idx].GenerateMips();         

                m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map0_PROPERTY, m_fourierBuffer0[m_idx]);
                m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map1_PROPERTY, m_fourierBuffer1[m_idx]);
                m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map2_PROPERTY, m_fourierBuffer2[m_idx]);
                m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map3_PROPERTY, m_fourierBuffer3[m_idx]);
                m_oceanMaterial.SetTexture (ShaderProperties._Ocean_Map4_PROPERTY, m_fourierBuffer4[m_idx]);

                m_oceanMaterial.SetFloat(ShaderProperties._Ocean_HeightOffset_PROPERTY, 0f); // doesn't need to be in update
                m_oceanMaterial.SetVector(ShaderProperties._VarianceMax_PROPERTY, m_varianceMax); // doesn't need to be in update
                Shader.SetGlobalVector(ShaderProperties._VarianceMax_PROPERTY, m_varianceMax); // this is just for the SSR atm

                if (waveInteractionHandler != null)
                {
                    waveInteractionHandler.PerformQueries(m_choppyness, m_gridSizes, m_fourierBuffer0[m_idx], m_fourierBuffer3[m_idx], m_fourierBuffer4[m_idx]);
                }
            }

            base.UpdateNode();
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            m_spectrum01.Release();
            m_spectrum23.Release();

            Object.Destroy(m_WTable);
            
            for (int i = 0; i < 2; i++) {
                m_fourierBuffer0[i].Release();
                m_fourierBuffer1[i].Release();
                m_fourierBuffer2[i].Release();
                m_fourierBuffer3[i].Release();
                m_fourierBuffer4[i].Release();
            }

            normalizedVarianceRenderTexture.Release();

            if (waveInteractionHandler != null)
            {
                waveInteractionHandler.Cleanup();
            }
        }
        
        protected virtual void CreateRenderTextures()
        {
            RenderTextureFormat mapFormat = RenderTextureFormat.ARGBHalf;

            CreateBuffer(ref m_fourierBuffer0, mapFormat, mapsAniso, true, false); // heights
            CreateBuffer(ref m_fourierBuffer1, mapFormat, mapsAniso, true, false); // slopes X
            CreateBuffer(ref m_fourierBuffer2, mapFormat, mapsAniso, true, false); // slopes Y
            CreateBuffer(ref m_fourierBuffer3, mapFormat, mapsAniso, true, false); // displacement X
            CreateBuffer(ref m_fourierBuffer4, mapFormat, mapsAniso, true, false); // displacement Y
            
            m_spectrum01 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, mapFormat, 0);
            m_spectrum01.filterMode = FilterMode.Point;
            m_spectrum01.wrapMode = TextureWrapMode.Repeat;
            m_spectrum01.Create();
            
            m_spectrum23 = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, mapFormat, 0);
            m_spectrum23.filterMode = FilterMode.Point;
            m_spectrum23.wrapMode = TextureWrapMode.Repeat;
            m_spectrum23.Create();

            m_WTable = new Texture2D(m_fourierGridSize, m_fourierGridSize, TextureFormat.RGBAHalf, false, true);
            m_WTable.filterMode = FilterMode.Point;
            m_WTable.wrapMode = TextureWrapMode.Clamp;

            normalizedVarianceRenderTexture = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RHalf);
            normalizedVarianceRenderTexture.volumeDepth = m_varianceSize;
            normalizedVarianceRenderTexture.wrapMode = TextureWrapMode.Clamp;
            normalizedVarianceRenderTexture.filterMode = FilterMode.Bilinear;
            normalizedVarianceRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            normalizedVarianceRenderTexture.enableRandomWrite = SystemInfo.supportsComputeShaders && Scatterer.Instance.usingDirectX;
            normalizedVarianceRenderTexture.useMipMap = false;
            normalizedVarianceRenderTexture.Create();
        }
        
        protected void CreateBuffer(ref RenderTexture[] tex, RenderTextureFormat format, int aniso, bool useMipMaps, bool autoGenerateMipMaps)
        {
            tex = new RenderTexture[2];
            
            for (int i = 0; i < 2; i++)
            {
                CreateMap(ref tex[i], format, aniso, useMipMaps, autoGenerateMipMaps);
            }
        }
        
        protected void CreateMap(ref RenderTexture map, RenderTextureFormat format, int aniso, bool useMipMaps, bool autoGenerateMipMaps)
        {
            map = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, format);

            if (aniso > 0)
            {
                map.filterMode = FilterMode.Trilinear;
                map.anisoLevel = aniso;
            }
            else
            {
                map.filterMode = FilterMode.Bilinear;
            }

            map.wrapMode = TextureWrapMode.Repeat;
            map.useMipMap = useMipMaps;
            map.autoGenerateMips = autoGenerateMipMaps;
            map.Create();
        }
        
        float Sqr(float x)
        {
            return x * x;
        }
        
        float Omega(float k)
        {
            // Eq 24
            return Mathf.Sqrt(m_gravity * k * (1.0f + Sqr(k / WAVE_KM)));
        }
        
        float Spectrum(float kx, float ky, bool omnispectrum)
        {
            //I know this is a big chunk of ugly math but dont worry to much about what it all means
            //It recreates a statistcally representative model of a wave spectrum in the frequency domain.

            //How to rotate the windDirection in the spectrum: https://github.com/Scrawk/Ceto/blob/07f8f45955a989983fe2330ea17eaf3f44c82031/Assets/Ceto/Scripts/Spectrum/CustomWaveSpectrumExample.cs#L248-L250
            //But I prefere to rotate the Ux and Uy axis, that way can change the wind direction without regenerating spectrum

            float U10 = m_windSpeed;
            
            // phase speed
            float k = Mathf.Sqrt(kx * kx + ky * ky);
            float c = Omega(k) / k;
            
            // spectral peak
            float kp = m_gravity * Sqr(m_omega / U10); // after Eq 3
            float cp = Omega(kp) / kp;
            
            // friction velocity
            float z0 = 3.7e-5f * Sqr(U10) / m_gravity * Mathf.Pow(U10 / cp, 0.9f); // Eq 66
            float u_star = 0.41f * U10 / Mathf.Log(10.0f / z0); // Eq 60
            
            float Lpm = Mathf.Exp(-5.0f / 4.0f * Sqr(kp / k)); // after Eq 3
            float gamma = (m_omega < 1.0f) ? 1.7f : 1.7f + 6.0f * Mathf.Log(m_omega); // after Eq 3 // log10 or log?
            float sigma = 0.08f * (1.0f + 4.0f / Mathf.Pow(m_omega, 3.0f)); // after Eq 3
            float Gamma = Mathf.Exp(-1.0f / (2.0f * Sqr(sigma)) * Sqr(Mathf.Sqrt(k / kp) - 1.0f));
            float Jp = Mathf.Pow(gamma, Gamma); // Eq 3
            float Fp = Lpm * Jp * Mathf.Exp(-m_omega / Mathf.Sqrt(10.0f) * (Mathf.Sqrt(k / kp) - 1.0f)); // Eq 32
            float alphap = 0.006f * Mathf.Sqrt(m_omega); // Eq 34
            float Bl = 0.5f * alphap * cp / c * Fp; // Eq 31
            
            float alpham = 0.01f * (u_star < WAVE_CM ? 1.0f + Mathf.Log(u_star / WAVE_CM) : 1.0f + 3.0f * Mathf.Log(u_star / WAVE_CM)); // Eq 44
            float Fm = Mathf.Exp(-0.25f * Sqr(k / WAVE_KM - 1.0f)); // Eq 41
            float Bh = 0.5f * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)
            
            Bh *= Lpm; // bug fix???
            
            if (omnispectrum) return AMP * (Bl + Bh) / (k * Sqr(k)); // Eq 30
            
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
            
            return AMP * (Bl + Bh) * (1.0f + Delta * Mathf.Cos(2.0f * phi)) / (2.0f * Mathf.PI * Sqr(Sqr(k))) * tweak; // Eq 67
        }

        void GenerateWavesSpectrum()
        {
            // Slope variance due to all waves, by integrating over the full spectrum.
            // Used by the BRDF rendering model
            float theoreticSlopeVariance = 0.0f;
            float k = 5e-3f;
            while (k < 1e3f)
            {
                float nextK = k * 1.001f;
                theoreticSlopeVariance += k * k * Spectrum(k, 0, true) * (nextK - k);
                k = nextK;
            }

            var spectrumShader = ShaderReplacer.Instance.LoadedShaders["Scatterer/GenerateSpectrum"];
            var spectrumMaterial = new Material(spectrumShader);
            var noiseTexture = new Texture2D(m_fourierGridSize, m_fourierGridSize, TextureFormat.RGBAHalf, false);
            var slopeVarianceRenderTexture = new RenderTexture(m_fourierGridSize, m_fourierGridSize, 0, RenderTextureFormat.RFloat);
            slopeVarianceRenderTexture.useMipMap = true;
            slopeVarianceRenderTexture.autoGenerateMips = false;
            slopeVarianceRenderTexture.Create();

            for (int x = 0; x < m_fourierGridSize; x++)
            {
                for (int y = 0; y < m_fourierGridSize; y++)
                {
                    noiseTexture.SetPixel(x, y, new Color(Random.value, Random.value, Random.value, Random.value));
                }
            }

            noiseTexture.Apply();
            spectrumMaterial.SetTexture("noiseTexture", noiseTexture);
            spectrumMaterial.SetFloat("gravity", m_gravity);
            spectrumMaterial.SetFloat("windSpeed", m_windSpeed);
            spectrumMaterial.SetFloat("omega", m_omega);
            spectrumMaterial.SetFloat("amp", AMP);
            spectrumMaterial.SetFloat("fftSize", m_fsize);
            spectrumMaterial.SetVector("gridSizes", m_gridSizes);

            RTUtility.MultiTargetBlit(new RenderTexture[] { m_spectrum01, m_spectrum23, slopeVarianceRenderTexture }, spectrumMaterial, 0);

            // The sum for total slope variance can be reconstructed by multiplying the value in the highest mip by the texel count
            slopeVarianceRenderTexture.GenerateMips();

            GenerateVarianceGPU(theoreticSlopeVariance, slopeVarianceRenderTexture);

            slopeVarianceRenderTexture.Release();
            Object.Destroy(noiseTexture);

            m_oceanMaterial.SetTexture(ShaderProperties._Ocean_Variance_PROPERTY, normalizedVarianceRenderTexture);
        }

        private void GenerateVarianceGPU(float theoreticSlopeVariance, RenderTexture slopeVarianceTexture)
        {
            if (SystemInfo.supportsComputeShaders && Scatterer.Instance.usingDirectX)
            {
                GenerateVarianceCompute(theoreticSlopeVariance, slopeVarianceTexture);   
            }
            else
            {
                GenerateVarianceGraphics(theoreticSlopeVariance, slopeVarianceTexture);
            }
        }

        private void GenerateVarianceCompute(float theoreticSlopeVariance, RenderTexture slopeVarianceTexture)
        {
            var computeVarianceShader = ShaderReplacer.Instance.LoadedComputeShaders["SlopeVariance"];

            // This will hold the result float scaled up by 100000.0
            var maxVarianceBuffer = new ComputeBuffer(1, sizeof(uint), ComputeBufferType.Default);
            maxVarianceBuffer.SetData(new uint[] { 0 });

            var rawVarianceRenderTexture = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RFloat);
            rawVarianceRenderTexture.volumeDepth = m_varianceSize;
            rawVarianceRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            rawVarianceRenderTexture.enableRandomWrite = true;
            rawVarianceRenderTexture.useMipMap = false;
            rawVarianceRenderTexture.Create();

            computeVarianceShader.SetFloat("theoreticSlopeVariance", theoreticSlopeVariance);
            computeVarianceShader.SetTexture(0, "slopeVarianceTexture", slopeVarianceTexture);
            computeVarianceShader.SetFloat("_VarianceSize", (float)m_varianceSize);
            computeVarianceShader.SetFloat("_Size", m_fsize);
            computeVarianceShader.SetVector("_GridSizes", m_gridSizes);
            computeVarianceShader.SetTexture(0, "_Spectrum01", m_spectrum01);
            computeVarianceShader.SetTexture(0, "_Spectrum23", m_spectrum23);
            computeVarianceShader.SetTexture(0, "variance32", rawVarianceRenderTexture);
            computeVarianceShader.SetBuffer(0, "maxVarianceBuffer", maxVarianceBuffer);

            computeVarianceShader.Dispatch(0, m_varianceSize / 4, m_varianceSize / 4, m_varianceSize / 4);

            // Perform the normalization
            computeVarianceShader.SetTexture(1, "variance32", rawVarianceRenderTexture);
            computeVarianceShader.SetTexture(1, "variance16", normalizedVarianceRenderTexture);
            computeVarianceShader.SetBuffer(1, "maxVarianceBuffer", maxVarianceBuffer);
            computeVarianceShader.Dispatch(1, m_varianceSize / 4, m_varianceSize / 4, m_varianceSize / 4);

            // Read back the max value which we'll use to get back the original values from the normalized values in ocean shader and in SSR
            uint[] readback = new uint[1];
            maxVarianceBuffer.GetData(readback);
            m_maxSlopeVariance = readback[0] / 100000f;
            m_varianceMax = new Vector2(m_maxSlopeVariance, m_maxSlopeVariance);

            maxVarianceBuffer.Release();
            rawVarianceRenderTexture.Release();
        }

        public static void Blit3D(RenderTexture tex, int slice, int size, Material blitMat, int pass)
        {
            GL.PushMatrix();
            GL.LoadOrtho();

            Graphics.SetRenderTarget(tex, 0, CubemapFace.Unknown, slice);

            float z = Mathf.Clamp01(slice / (float)(size - 1));

            blitMat.SetPass(pass);

            GL.Begin(GL.QUADS);

            GL.TexCoord3(0, 0, z);
            GL.Vertex3(0, 0, 0);
            GL.TexCoord3(1, 0, z);
            GL.Vertex3(1, 0, 0);
            GL.TexCoord3(1, 1, z);
            GL.Vertex3(1, 1, 0);
            GL.TexCoord3(0, 1, z);
            GL.Vertex3(0, 1, 0);

            GL.End();

            GL.PopMatrix();
        }

        private void GenerateVarianceGraphics(float theoreticSlopeVariance, RenderTexture slopeVarianceTexture)
        {
            var varianceShader = ShaderReplacer.Instance.LoadedShaders["Scatterer/SlopeVariance"];
            var varianceMaterial = new Material(varianceShader);

            var rawVarianceRenderTexture = new RenderTexture(m_varianceSize, m_varianceSize, 0, RenderTextureFormat.RFloat);
            rawVarianceRenderTexture.volumeDepth = m_varianceSize;
            rawVarianceRenderTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            rawVarianceRenderTexture.useMipMap = false;
            rawVarianceRenderTexture.Create();

            var maxVarianceRenderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGBFloat);
            maxVarianceRenderTexture.useMipMap = false;
            maxVarianceRenderTexture.Create();

            varianceMaterial.SetFloat("theoreticSlopeVariance", theoreticSlopeVariance);
            varianceMaterial.SetTexture("slopeVarianceTexture", slopeVarianceTexture);
            varianceMaterial.SetFloat("_VarianceSize", (float)m_varianceSize);
            varianceMaterial.SetFloat("_Size", m_fsize);
            varianceMaterial.SetVector("_GridSizes", m_gridSizes);
            varianceMaterial.SetTexture("_Spectrum01", m_spectrum01);
            varianceMaterial.SetTexture("_Spectrum23", m_spectrum23);

            // Generate variance
            for (int i = 0; i < m_varianceSize; i++)
            {
                varianceMaterial.SetInt("zSlice", i);
                Blit3D(rawVarianceRenderTexture, i, m_varianceSize, varianceMaterial, 0);
            }

            // Find the max
            varianceMaterial.SetTexture("variance32", rawVarianceRenderTexture);
            Graphics.Blit(null, maxVarianceRenderTexture, varianceMaterial, 1);

            // Perform the normalization
            varianceMaterial.SetTexture("maxVariance", maxVarianceRenderTexture);
            for (int i = 0; i < m_varianceSize; i++)
            {
                varianceMaterial.SetInt("zSlice", i);
                Blit3D(normalizedVarianceRenderTexture, i, m_varianceSize, varianceMaterial, 2);
            }

            // Read back the max
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture.active = maxVarianceRenderTexture;

            var readTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            readTexture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            readTexture.Apply();
            Color pixel = readTexture.GetPixel(0, 0);
            m_maxSlopeVariance = pixel.r;
            m_varianceMax = new Vector2(m_maxSlopeVariance, m_maxSlopeVariance);

            // Cleanup
            RenderTexture.active = previousActive;
            Object.Destroy(readTexture);
            rawVarianceRenderTexture.Release();
            maxVarianceRenderTexture.Release();
        }

        // WTable is a precomputed dispersion relation texture that stores the angular
        // frequencies for wave propagation over time, not strictly needed on modern hardware
        void CreateWTable()
        {
            Vector2 uv, st;
            float k1, k2, k3, k4, w1, w2, w3, w4;

            Color[] pixels = new Color[m_fourierGridSize * m_fourierGridSize];

            for (int x = 0; x < m_fourierGridSize; x++)
            {
                for (int y = 0; y < m_fourierGridSize; y++)
                {
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

                    pixels[x + y * m_fourierGridSize] = new Color(w1, w2, w3, w4);

                }
            }

            m_WTable.SetPixels(pixels);
            m_WTable.Apply();
        }

        // FixedUpdate is responsible for physics, apply part displacement here
        public void FixedUpdate()
        {
            if (waveInteractionHandler != null && prolandManager.GetSkyNode().simulateOceanInteraction)
            {
                waterHeightAtCameraPosition = waveInteractionHandler.UpdateInteractions(height, waterHeightAtCameraPosition, ux.ToVector3(), uy.ToVector3(), OffsetVector3);
            }
        }
    }
    
}