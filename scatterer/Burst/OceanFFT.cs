using System;
using System.Linq;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Mathematics;
using Unity.Burst.Intrinsics;
using static Unity.Burst.Intrinsics.X86.Avx2;
using static Unity.Burst.Intrinsics.X86.Avx;
using static Unity.Burst.Intrinsics.X86.Sse4_2;
using static Unity.Burst.Intrinsics.X86.Sse4_1;
using static Unity.Burst.Intrinsics.X86.Ssse3;
using static Unity.Burst.Intrinsics.X86.Sse2;
using static Unity.Burst.Intrinsics.X86.Sse;

using UnityEngine;
using TDx.TDxInput;

namespace Scatterer.Burst
{
    internal unsafe struct OceanFFT
    {
        #region Members
        public int m_varianceSize;
        public int m_fourierGridSize;
        public float m_fsize;
        public Vector4 m_gridSizes;
        #endregion


        #region Helpers
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
        #endregion

        #region ComputeVariance
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
            // and have things work, we don't have that so we need to do it
            // ourselves.
            if (ComputeVarianceFp is null)
            {
                var fp = BurstCompiler.CompileFunctionPointer<ComputeVarianceBurstDelegate>(OceanFFTExt.ComputeVarianceBurst);

                // We need to save both of these so that the function pointer is
                // not garbage collected.
                ComputeVarianceFp = fp;
                ComputeVarianceDelegate = fp.Invoke;
            }

            fixed (float* pInSpectrum01 = inSpectrum01)
            fixed (float* pInSpectrum23 = inSpectrum23)
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
        #endregion

        #region Update Variance 8bit
        [BurstDiscard]
        internal static void UpdateVariance8bitManaged(
            int varianceSize,
            Vector2* variance32bit,
            Color32* variance8bit,
            Vector2 varianceMax,
            out float maxSlopeVariance
        )
        {
            var totalIterations = varianceSize * varianceSize * varianceSize;

            maxSlopeVariance = Enumerable
                .Range(0, totalIterations)
                .AsParallel()
                .Select(idx =>
                {
                    // Store in the 8-bit array
                    var variance = new Color(variance32bit[idx].x / varianceMax.x, variance32bit[idx].y / varianceMax.y, 0.0f, 1.0f);
                    variance8bit[idx] = variance;

                    return Mathf.Max(
                        variance8bit[idx].r * varianceMax.x,
                        variance8bit[idx].g * varianceMax.y
                    );
                })
                .Aggregate(0f, Mathf.Max);
        }

        static Color32 MakeVarianceColor(float2 color)
        {
            color = math.clamp(color, new float2(0f), new float2(1f));
            color *= 255f;

            byte r = (byte)color.x;
            byte g = (byte)color.y;

            return new Color32(r, g, 0, 255);
        }

        internal static float UpdateVariance8bitBurst(
            [AssumeRange(0, int.MaxValue)] int varianceSize,
            [NoAlias] Vector2* variance32bit,
            [NoAlias] Color32* variance8bit,
            Vector2 m_varianceMax
        )
        {
            float2 maxSlopeVariance = new float2(0f);
            int len = varianceSize * varianceSize * varianceSize;
            int i = 0;

            // The codegen for the float2 version isn't that great so here's an
            // AVX2 version that actually makes proper use of intrinsics.
            if (IsAvx2Supported)
            {
                // This should probably compile down to a double broadcast
                v256 varianceMax = mm256_setr_ps(
                    m_varianceMax.x,
                    m_varianceMax.y,
                    m_varianceMax.x,
                    m_varianceMax.y,
                    m_varianceMax.x,
                    m_varianceMax.y,
                    m_varianceMax.x,
                    m_varianceMax.y
                );
                v256 maxSlope = mm256_setzero_ps();

                for (; i + 8 <= len; i += 8)
                {
                    v256 v32 = mm256_loadu_ps(&variance32bit[i]);
                    maxSlope = mm256_min_ps(v32, maxSlope);

                    // color = (int)clamp(v32 / varianceMax, 0f, 1f);
                    v256 color = mm256_div_ps(v32, varianceMax);
                    color = mm256_max_ps(color, mm256_set1_ps(0f));
                    color = mm256_min_ps(color, mm256_set1_ps(1f));
                    color = mm256_mul_ps(color, mm256_set1_ps(255f));
                    color = mm256_cvtps_epi32(color);

                    // Now we need to pack each successive 2 values into the
                    // first 2 bytes of a 32-bit value. Luckily we can do this
                    // with a bitshift.
                    //
                    // color = ulong(lo, hi)
                    // color = color | (color >> 24)
                    //
                    // This leaves some garbage in the high 32 bits but we will
                    // overwrite that in the shuffle phase.
                    color = mm256_or_si256(color, mm256_srli_epi64(color, 24));

                    // Now pack values in the low 2 entries of each 128-bit lane.
                    //
                    // The shuffle mask should basically be
                    // - lane 0: from 0
                    // - lane 1: from 2
                    // - don't care about lanes 3 and 4
                    color = mm256_shuffle_epi32(color, 0b1000);

                    // Next we pack the two 64-bit blocks we care about to the
                    // low half of the vector.
                    color = mm256_permute4x64_epi64(color, 0b1000);

                    // Everything we care about is now in the low half
                    v128 hcolor = mm256_extracti128_si256(color, 0);

                    // Now we just need to set a to 255
                    hcolor = or_si128(hcolor, set1_epi32(0xFF << 24));

                    storeu_si128(&variance8bit[i], hcolor);
                }

                // Now fold down to something that can be used by the remainder
                // loop.
                var hMaxSlope = max_ps(
                    mm256_extractf128_ps(maxSlope, 0),
                    mm256_extractf128_ps(maxSlope, 1)
                );
                var vMaxSlope = new float4(
                    extractf_ps(hMaxSlope, 0),
                    extractf_ps(hMaxSlope, 1),
                    extractf_ps(hMaxSlope, 2),
                    extractf_ps(hMaxSlope, 3)
                );

                maxSlopeVariance = math.max(
                    maxSlopeVariance,
                    math.max(vMaxSlope.xy, vMaxSlope.zw)
                );
            }

            {
                float2 varianceMax = new float2(m_varianceMax.x, m_varianceMax.y);

                for (; i < len; ++i)
                {
                    float2 v32 = new float2(variance32bit[i].x, variance32bit[i].y);

                    variance8bit[i] = MakeVarianceColor(v32 / varianceMax);
                    maxSlopeVariance = math.max(maxSlopeVariance, v32);
                }

                return math.max(maxSlopeVariance.x, maxSlopeVariance.y);
            }
        }

        delegate void UpdateVarianceBurstDelegate(
            int varianceSize,
            Vector2* variance32bit,
            Color32* variance8bit,
            in Vector2 varianceMax,
            out float maxSlopeVariance
        );

        static FunctionPointer<UpdateVarianceBurstDelegate>? UpdateVarianceFp = null;
        static UpdateVarianceBurstDelegate UpdateVarianceDelegate = null;

        public static float UpdateVariance8bit(
            int varianceSize,
            Vector2[] variance32bit,
            Color32[] variance8bit,
            Vector2 varianceMax
        )
        {
            if (varianceSize < 0)
                throw new ArgumentOutOfRangeException(nameof(varianceSize));

            int len = varianceSize * varianceSize * varianceSize;
            if (variance32bit.Length < len)
                throw new ArgumentException("array was too small", nameof(variance32bit));
            if (variance8bit.Length < len)
                throw new ArgumentException("array was too small", nameof(variance8bit));


            // Normally in a unity project an IL post-processor rewrites things
            // so that you can just call a method annotated with [BurstCompile]
            // and have things work, we don't have that so we need to do it
            // ourselves.
            if (UpdateVarianceFp is null)
            {
                var fp = BurstCompiler.CompileFunctionPointer<UpdateVarianceBurstDelegate>(OceanFFTExt.UpdateVariance8bitBurst);

                // We need to save both of these so that the function pointer is
                // not garbage collected.
                UpdateVarianceFp = fp;
                UpdateVarianceDelegate = fp.Invoke;
            }

            fixed (Vector2* pvariance32bit = variance32bit)
            fixed (Color32* pvariance8bit = variance8bit)
            {
                UpdateVarianceDelegate(
                    varianceSize,
                    pvariance32bit,
                    pvariance8bit,
                    in varianceMax,
                    out var maxSlopeVariance
                );

                return maxSlopeVariance;
            }
        }
        #endregion
    }

    [BurstCompile(FloatMode = FloatMode.Fast)]
    internal unsafe static class OceanFFTExt
    {
        // This needs to live in a class or else mono will hard-crash when
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

        [BurstCompile]
        internal static void UpdateVariance8bitBurst(
            int varianceSize,
            Vector2* variance32bit,
            Color32* variance8bit,
            in Vector2 varianceMax,
            out float maxSlopeVariance
        )
        {
            if (!BurstUtil.IsBurstCompiled)
            {
                // Unity.Mathematics performs terribly outside of burst so we
                // just use the reference version if not burst-compiled.
                OceanFFT.UpdateVariance8bitManaged(
                    varianceSize,
                    variance32bit,
                    variance8bit,
                    varianceMax,
                    out maxSlopeVariance
                );
            }
            else
            {
                maxSlopeVariance = OceanFFT.UpdateVariance8bitBurst(
                    varianceSize,
                    variance32bit,
                    variance8bit,
                    varianceMax
                );
            }
        }
    }
}