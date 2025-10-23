using System;
using Unity.Burst;
using Unity.Mathematics;

using UnityEngine;

namespace Scatterer.Burst
{
    internal unsafe struct OceanFFT
    {

        public int m_varianceSize;
        public int m_fourierGridSize;
        public float m_fsize;
        public Vector4 m_gridSizes;


        Vector2 GetSlopeVariances(Vector2 k, float A, float B, float C, float spectrumX, float spectrumY)
        {
            float w = 1.0f - Mathf.Exp(A * k.x * k.x + B * k.x * k.y + C * k.y * k.y);
            return new Vector2((k.x * k.x) * w, (k.y * k.y) * w) * (spectrumX * spectrumX + spectrumY * spectrumY) * 2.0f;
        }

        internal Vector2 ComputeVariance(
            float slopeVarianceDelta,
            float* inSpectrum01,
            float* inSpectrum23,
            float idxX,
            float idxY,
            float idxZ
        )
        {
            const float SCALE = 10.0f;

            float A = Mathf.Pow(idxX / ((float)m_varianceSize - 1.0f), 4.0f) * SCALE;
            float C = Mathf.Pow(idxZ / ((float)m_varianceSize - 1.0f), 4.0f) * SCALE;
            float B = (2.0f * idxY / ((float)m_varianceSize - 1.0f) - 1.0f) * Mathf.Sqrt(A * C);
            A = -0.5f * A;
            B = -B;
            C = -0.5f * C;

            Vector2 slopeVariances = new Vector2(slopeVarianceDelta, slopeVarianceDelta);

            for (int x = 0; x < m_fourierGridSize; x++)
            {
                for (int y = 0; y < m_fourierGridSize; y++)
                {
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

        internal void GetSlopeVariancesBurst(
            float A,
            float B,
            float C,
            float2 k,
            float4 gridSizes,
            float4 spectrum01,
            float4 spectrum23,
            ref float4 vx,
            ref float4 vy
        )
        {
            // The function we want to calculate here is
            //
            // var k = k.xy / gridSizes.[xyzw];
            // var spectrum = spectrum[01|23].[xy|zw]
            // var w = 1f - exp(A * k.x * k.x + B * k.x * k.y + C * k.y * k.y);
            // return w * k.xy * (spectrum.x * spectrum.x + spectrum.y * spectrum.y) * 2f;
            //
            // We just want to batch up as many parts as possible.

            float4 kx = k.xxxx / gridSizes;
            float4 ky = k.yyyy / gridSizes;
            float4 w = 1f - math.exp(A * kx * kx + B * kx * ky + C * ky * ky);

            float4 ss01 = spectrum01 * spectrum01;
            float4 ss23 = spectrum23 * spectrum23;
            float4 ss = new float4(ss01.xz + ss01.yw, ss23.xz + ss23.yw);

            vx += w * kx * ss * 2f;
            vy += w * ky * ss * 2f;
        }

        internal Vector2 ComputeVarianceBurst(
            float slopeVarianceDelta,
            float* inSpectrum01,
            float* inSpectrum23,
            float idxX,
            float idxY,
            float idxZ
        )
        {
            const float SCALE = 10.0f;

            float A = Pow4(idxX / ((float)m_varianceSize - 1.0f)) * SCALE;
            float C = Pow4(idxZ / ((float)m_varianceSize - 1.0f)) * SCALE;
            float B = (2.0f * idxY / ((float)m_varianceSize - 1.0f) - 1.0f) * Mathf.Sqrt(A * C);
            A = -0.5f * A;
            B = -B;
            C = -0.5f * C;

            float4 gridSizes = new float4(m_gridSizes.x, m_gridSizes.y, m_gridSizes.z, m_gridSizes.w);

            float4 vx = new float4(slopeVarianceDelta);
            float4 vy = new float4(slopeVarianceDelta);

            for (int y = 0; y < m_fourierGridSize; y++)
            {
                for (int x = 0; x < m_fourierGridSize; x++)
                {
                    int i = x >= m_fsize / 2.0f ? x - m_fourierGridSize : x;
                    int j = y >= m_fsize / 2.0f ? y - m_fourierGridSize : y;

                    float2 k = new float2(2 * i, 2 * j) * Mathf.PI;

                    float* p01 = &inSpectrum01[(x + y * m_fourierGridSize) * 4];
                    float* p23 = &inSpectrum01[(x + y * m_fourierGridSize) * 4];

                    float4 spectrum01 = new float4(p01[0], p01[1], p01[2], p01[3]);
                    float4 spectrum23 = new float4(p23[0], p23[1], p23[2], p23[3]);

                    GetSlopeVariancesBurst(
                        A, B, C,
                        k,
                        m_gridSizes,
                        spectrum01,
                        spectrum23,
                        ref vx,
                        ref vy
                    );
                }
            }

            return new Vector2(Hadd(vx), Hadd(vy));
        }

        static float Pow4(float v)
        {
            v *= v;
            v *= v;
            return v;
        }
        
        static float Hadd(float4 v)
        {
            float2 h = v.xy + v.yz;
            return h.x + h.y;
        }

        delegate void ComputeVarianceBurstDelegate(
            ref OceanFFT fft,
            float slopeVarianceDelta,
            float* inSpectrum01,
            float* inSpectrum23,
            float idxX,
            float idxY,
            float idxZ,
            out Vector2 variance
        );

        static FunctionPointer<ComputeVarianceBurstDelegate>? ComputeVarianceFp = null;
        static ComputeVarianceBurstDelegate ComputeVarianceDelegate = null;

        public Vector2 ComputeVariance(
            float slopeVarianceDelta,
            float[] inSpectrum01,
            float[] inSpectrum23,
            float idxX,
            float idxY,
            float idxZ
        )
        {
            var bound = m_fourierGridSize * m_fourierGridSize * 4;
            if (inSpectrum01.Length < bound)
                throw new InvalidOperationException("inSpectrum01 length too small for fourier grid size");
            if (inSpectrum23.Length < bound)
                throw new InvalidOperationException("inSpectrum23 length too small for fourier grid size");

            // Normally in a unity project an IL post-processor rewrites things
            // so that you can just call a method annotated with [BurstCompile]
            // and have things
            if (ComputeVarianceFp is null)
            {
                var fp = BurstCompiler.CompileFunctionPointer<ComputeVarianceBurstDelegate>(OceanFFTExt.ComputeVarianceBurst);

                // We need to save both of these so that the function pointer is
                // not garbage collected.
                ComputeVarianceFp = fp;
                ComputeVarianceDelegate = fp.Invoke;
            }
            
            fixed(float* pInSpectrum01 = inSpectrum01)
            fixed(float* pInSpectrum23 = inSpectrum23)
            {                
                ComputeVarianceDelegate(
                    ref this,
                    slopeVarianceDelta,
                    pInSpectrum01,
                    pInSpectrum23,
                    idxX,
                    idxY,
                    idxZ,
                    out var variance
                );

                return variance;
            }
        }
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    internal unsafe static class OceanFFTExt
    {
        // This needs to live in a class or else the mono will hard-crash when
        // attempting to call the burst function pointer.

        [BurstCompile]
        internal static void ComputeVarianceBurst(
            ref OceanFFT fft,
            float slopeVarianceDelta,
            float* inSpectrum01,
            float* inSpectrum23,
            float idxX,
            float idxY,
            float idxZ,
            out Vector2 variance
        )
        {
            if (!BurstUtil.IsBurstCompiled)
            {
                // Unity.Mathematics performs terribly outside of burst so we
                // just use the reference version if not burst-compiled.
                variance = fft.ComputeVariance(
                    slopeVarianceDelta,
                    inSpectrum01,
                    inSpectrum23,
                    idxX,
                    idxY,
                    idxZ
                );
            }
            else
            {
                variance = fft.ComputeVarianceBurst(
                    slopeVarianceDelta,
                    inSpectrum01,
                    inSpectrum23,
                    idxX,
                    idxY,
                    idxZ
                );
            }
        }
    }
}