using UnityEngine;

namespace Scatterer
{
    public class FourierGPU
    {
        const int PASS_X_1 = 0, PASS_Y_1 = 1;
        const int PASS_X_2 = 2, PASS_Y_2 = 3;
        const int PASS_X_3 = 4, PASS_Y_3 = 5;

        int m_size;
        float m_fsize;
        int m_passes;
        Texture2D[] m_butterflyLookupTable = null;
        Material m_fourier;

        ComputeShader computeShader;
        int kernelInputCount1, kernelInputCount2, kernelInputCount3;

        bool useCompute = false;

        public FourierGPU(int size)
        {
            // The size is limited by the groupshared memory size for single-pass compute
            // Any size above 256 is already overkill / diminishing returns regardless
            if (size > 512)
            {
                Utils.LogDebug("FourierGPU::FourierGPU - fourier grid size must not be greater than 512, changing to 512");
                size = 512;
            }

            if (!Mathf.IsPowerOfTwo(size))
            {
                Utils.LogDebug("FourierGPU::FourierGPU - fourier grid size must be pow2 number, changing to nearest pow2 number");
                size = Mathf.NextPowerOfTwo(size);
            }

            m_size = size;

            useCompute = SystemInfo.supportsComputeShaders && !SystemInfo.graphicsDeviceVersion.Contains("OpenGL");

            if (useCompute)
            {
                computeShader = ShaderReplacer.Instance.LoadedComputeShaders["Fourier"];

                var kernel1 = $"FFT_{size}_1";
                var kernel2 = $"FFT_{size}_2";
                var kernel3 = $"FFT_{size}_3";

                if(!computeShader.HasKernel(kernel1) || !computeShader.HasKernel(kernel2) || !computeShader.HasKernel(kernel3))
                {
                    Utils.LogError($"Compute kernels for FFT size {size} not found");
                }

                kernelInputCount1 = computeShader.FindKernel(kernel1);
                kernelInputCount2 = computeShader.FindKernel(kernel2);
                kernelInputCount3 = computeShader.FindKernel(kernel3);
            }
            else
            {
                Shader shader = ShaderReplacer.Instance.LoadedShaders[("Scatterer/Fourier")];

                if (shader == null)
                    Utils.LogError("FourierGPU::FourierGPU - Could not find shader Scatterer/Fourier");

                m_fourier = new Material(shader);

                m_fsize = (float)m_size;
                m_passes = (int)(Mathf.Log(m_fsize) / Mathf.Log(2.0f));

                m_butterflyLookupTable = new Texture2D[m_passes];

                ComputeButterflyLookupTable();

                m_fourier.SetFloat("_Size", m_fsize);
            }
        }

        int BitReverse(int i)
        {
            int j = i;
            int Sum = 0;
            int W = 1;
            int M = m_size / 2;
            while(M != 0) 
            {
                j = ((i&M) > M-1) ? 1 : 0;
                Sum += j * W;
                W *= 2;
                M /= 2;
            }
            return Sum;
        }
        
        Texture2D Make1DTex(int i)
        {
            Texture2D tex = new Texture2D(m_size, 1, TextureFormat.RGBAHalf, false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        void ComputeButterflyLookupTable()
        {
            for(int i = 0; i < m_passes; i++) 
            {
                int nBlocks  = (int) Mathf.Pow(2, m_passes - 1 - i);
                int nHInputs = (int) Mathf.Pow(2, i);
                
                m_butterflyLookupTable[i] = Make1DTex(i);
                
                for (int j = 0; j < nBlocks; j++)
                {
                    for (int k = 0; k < nHInputs; k++) 
                    {
                        int i1, i2, j1, j2;
                        if (i == 0) 
                        {
                            i1 = j * nHInputs * 2 + k;
                            i2 = j * nHInputs * 2 + nHInputs + k;
                            j1 = BitReverse(i1);
                            j2 = BitReverse(i2);
                        } 
                        else 
                        {
                            i1 = j * nHInputs * 2 + k;
                            i2 = j * nHInputs * 2 + nHInputs + k;
                            j1 = i1;
                            j2 = i2;
                        }
                        
                        m_butterflyLookupTable[i].SetPixel(i1, 0, new Color( (float)j1, (float)j2, (float)(k * nBlocks), 0));
                        m_butterflyLookupTable[i].SetPixel(i2, 0, new Color( (float)j1, (float)j2, (float)(k * nBlocks), 1));
                    }
                }
                
                m_butterflyLookupTable[i].Apply();
            }
        }


        public int PerformFFT(RenderTexture[] data0)
        {
            if (useCompute)
            {
                return PerformFFTCompute(data0);
            }
            else
            {
                return PerformFFTGraphics(data0);
            }
        }

        public int PerformFFTCompute(RenderTexture[] data0)
        {
            int kernel = kernelInputCount1;
            int readWriteIndex = 1;

            computeShader.SetTexture(kernel, "input0", data0[readWriteIndex]);

            // Horizontal pass
            computeShader.SetInt("verticalPass", 0);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            // Vertical pass
            computeShader.SetInt("verticalPass", 1);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            return readWriteIndex;
        }

        public int PerformFFTGraphics(RenderTexture[] data0)
        {
            RenderTexture[] pass0 = new RenderTexture[] { data0[0] };
            RenderTexture[] pass1 = new RenderTexture[] { data0[1] };

            int i;
            int idx = 0; int idx1;
            int j = 0;

            for (i = 0; i < m_passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);

                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_X_1);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_X_1);
            }

            for (i = 0; i < m_passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);

                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);

                if (idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_Y_1);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_Y_1);
            }

            return idx;
        }

        public int PerformFFT(RenderTexture[] data0, RenderTexture[] data1)
        {
            if (useCompute)
            {
                return PerformFFTCompute(data0, data1);
            }
            else
            {
                return PerformFFTGraphics(data0, data1);
            }
        }

        public int PerformFFTCompute(RenderTexture[] data0, RenderTexture[] data1)
        {
            int kernel = kernelInputCount2;
            int readWriteIndex = 1;

            computeShader.SetTexture(kernel, "input0", data0[readWriteIndex]);
            computeShader.SetTexture(kernel, "input1", data1[readWriteIndex]);

            // Horizontal pass
            computeShader.SetInt("verticalPass", 0);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            // Vertical pass
            computeShader.SetInt("verticalPass", 1);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            return readWriteIndex;
        }

        public int PerformFFTGraphics(RenderTexture[] data0, RenderTexture[] data1)
        {
            RenderTexture[] pass0 = new RenderTexture[]{ data0[0], data1[0] };
            RenderTexture[] pass1 = new RenderTexture[]{ data0[1], data1[1] };
            
            int i;
            int idx = 0; int idx1;
            int j = 0;
            
            for(i = 0; i < m_passes; i++, j++) 
            {
                idx = j%2;
                idx1 = (j+1)%2;
                
                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);
                
                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer1_PROPERTY, data1[idx1]);

                if(idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_X_2);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_X_2);
            }

            for(i = 0; i < m_passes; i++, j++) 
            {
                idx = j%2;
                idx1 = (j+1)%2;
                
                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);
                
                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer1_PROPERTY, data1[idx1]);

                if(idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_Y_2);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_Y_2);
            }

            return idx;
        }

        public int PerformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
        {
            if (useCompute)
            {
                return PerformFFTCompute(data0, data1, data2);
            }
            else
            {
                return PerformFFTGraphics(data0, data1, data2);
            }
        }

        public int PerformFFTCompute(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
        {
            int kernel = kernelInputCount3;
            int readWriteIndex = 1;

            computeShader.SetTexture(kernel, "input0", data0[readWriteIndex]);
            computeShader.SetTexture(kernel, "input1", data1[readWriteIndex]);
            computeShader.SetTexture(kernel, "input2", data2[readWriteIndex]);

            // Horizontal pass
            computeShader.SetInt("verticalPass", 0);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            // Vertical pass
            computeShader.SetInt("verticalPass", 1);
            computeShader.Dispatch(kernel, 1, m_size, 1);

            return readWriteIndex;
        }

        public int PerformFFTGraphics(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
        {
            RenderTexture[] pass0 = new RenderTexture[]{ data0[0], data1[0], data2[0] };
            RenderTexture[] pass1 = new RenderTexture[]{ data0[1], data1[1], data2[1] };
            
            int i;
            int idx = 0; int idx1;
            int j = 0;
            
            for(i = 0; i < m_passes; i++, j++) 
            {
                idx = j%2;
                idx1 = (j+1)%2;
                
                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);
                
                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer1_PROPERTY, data1[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer2_PROPERTY, data2[idx1]);
                
                if(idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_X_3);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_X_3);
            }

            for(i = 0; i < m_passes; i++, j++) 
            {
                idx = j%2;
                idx1 = (j+1)%2;
                
                m_fourier.SetTexture(ShaderProperties._ButterFlyLookUp_PROPERTY, m_butterflyLookupTable[i]);
                
                m_fourier.SetTexture(ShaderProperties._ReadBuffer0_PROPERTY, data0[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer1_PROPERTY, data1[idx1]);
                m_fourier.SetTexture(ShaderProperties._ReadBuffer2_PROPERTY, data2[idx1]);
                
                if(idx == 0)
                    RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_Y_3);
                else
                    RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_Y_3);
            }

            return idx;
        }

    }
}

















