using UnityEngine;
using System.Collections;

namespace scatterer
{
	
    public class FourierCPU
    {
        int m_size;
        float m_fsize;
        int m_passes;
		public float[] m_butterflyLookupTable = null;

        public FourierCPU(int size)
        {
            if (!Mathf.IsPowerOfTwo(size))
            {
                Utils.LogDebug("Fourier grid size must be pow2 number, changing to nearest pow2 number");
                size = Mathf.NextPowerOfTwo(size);
            }

            m_size = size; //must be pow2 num
            m_fsize = (float)m_size;
            m_passes = (int)(Mathf.Log(m_fsize) / Mathf.Log(2.0f));
        }

        //Performs two FFTs on two complex numbers packed in a vector4
        Vector4 FFT(Vector2 w, Vector4 input1, Vector4 input2)
        {
            input1.x += w.x * input2.x - w.y * input2.y;
            input1.y += w.y * input2.x + w.x * input2.y;
            input1.z += w.x * input2.z - w.y * input2.w;
            input1.w += w.y * input2.z + w.x * input2.w;

            return input1;
        }

        //Performs one FFT on a complex number
        Vector2 FFT(Vector2 w, Vector2 input1, Vector2 input2)
        {
            input1.x += w.x * input2.x - w.y * input2.y;
            input1.y += w.y * input2.x + w.x * input2.y;

            return input1;
        }

        public int PeformFFT(int startIdx, Vector4[,] data0, Vector4[,] data1, Vector4[,] data2)
        {
            int x; int y; int i;
            int idx = 0; int idx1; int bftIdx;
            int X; int Y;
            Vector2 w;

            int j = startIdx;



            for (i = 0; i < m_passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                for (x = 0; x < m_size; x++)
                {
                    for (y = 0; y < m_size; y++)
                    {
                        bftIdx = 4 * (x + i * m_size);

                        X = (int)m_butterflyLookupTable[bftIdx + 0];
                        Y = (int)m_butterflyLookupTable[bftIdx + 1];
                        w.x = m_butterflyLookupTable[bftIdx + 2];
                        w.y = m_butterflyLookupTable[bftIdx + 3];

                        data0[idx, x + y * m_size] = FFT(w, data0[idx1, X + y * m_size], data0[idx1, Y + y * m_size]);
                        data1[idx, x + y * m_size] = FFT(w, data1[idx1, X + y * m_size], data1[idx1, Y + y * m_size]);
                        data2[idx, x + y * m_size] = FFT(w, data2[idx1, X + y * m_size], data2[idx1, Y + y * m_size]);
                    }
                }
            }

            for (i = 0; i < m_passes; i++, j++)
            {
                idx = j % 2;
                idx1 = (j + 1) % 2;

                for (x = 0; x < m_size; x++)
                {
                    for (y = 0; y < m_size; y++)
                    {
                        bftIdx = 4 * (y + i * m_size);

                        X = (int)m_butterflyLookupTable[bftIdx + 0];
                        Y = (int)m_butterflyLookupTable[bftIdx + 1];
                        w.x = m_butterflyLookupTable[bftIdx + 2];
                        w.y = m_butterflyLookupTable[bftIdx + 3];

                        data0[idx, x + y * m_size] = FFT(w, data0[idx1, x + X * m_size], data0[idx1, x + Y * m_size]);
                        data1[idx, x + y * m_size] = FFT(w, data1[idx1, x + X * m_size], data1[idx1, x + Y * m_size]);
                        data2[idx, x + y * m_size] = FFT(w, data2[idx1, x + X * m_size], data2[idx1, x + Y * m_size]);

                    }
                }
            }


			return idx;
        }
	

		public int PeformFFT(int startIdx, Vector4[,] data0, Vector4[,] data1)
		{
			int x; int y; int i;
			int idx = 0; int idx1; int bftIdx;
			int X; int Y;
			Vector2 w;
			
			int j = startIdx;
			
			
			
			for (i = 0; i < m_passes; i++, j++)
			{
				idx = j % 2;
				idx1 = (j + 1) % 2;
				
				for (x = 0; x < m_size; x++)
				{
					for (y = 0; y < m_size; y++)
					{
						bftIdx = 4 * (x + i * m_size);
						
						X = (int)m_butterflyLookupTable[bftIdx + 0];
						Y = (int)m_butterflyLookupTable[bftIdx + 1];
						w.x = m_butterflyLookupTable[bftIdx + 2];
						w.y = m_butterflyLookupTable[bftIdx + 3];
						
						data0[idx, x + y * m_size] = FFT(w, data0[idx1, X + y * m_size], data0[idx1, Y + y * m_size]);
						data1[idx, x + y * m_size] = FFT(w, data1[idx1, X + y * m_size], data1[idx1, Y + y * m_size]);

					}
				}
			}
			
			for (i = 0; i < m_passes; i++, j++)
			{
				idx = j % 2;
				idx1 = (j + 1) % 2;
				
				for (x = 0; x < m_size; x++)
				{
					for (y = 0; y < m_size; y++)
					{
						bftIdx = 4 * (y + i * m_size);
						
						X = (int)m_butterflyLookupTable[bftIdx + 0];
						Y = (int)m_butterflyLookupTable[bftIdx + 1];
						w.x = m_butterflyLookupTable[bftIdx + 2];
						w.y = m_butterflyLookupTable[bftIdx + 3];
						
						data0[idx, x + y * m_size] = FFT(w, data0[idx1, x + X * m_size], data0[idx1, x + Y * m_size]);
						data1[idx, x + y * m_size] = FFT(w, data1[idx1, x + X * m_size], data1[idx1, x + Y * m_size]);

						
					}
				}
			}
			
			
			return idx;
		}


		public int PeformFFT(int startIdx, Vector4[,] data0)
		{
			int x; int y; int i;
			int idx = 0; int idx1; int bftIdx;
			int X; int Y;
			Vector2 w;
			
			int j = startIdx;
			
			
			
			for (i = 0; i < m_passes; i++, j++)
			{
				idx = j % 2;
				idx1 = (j + 1) % 2;
				
				for (x = 0; x < m_size; x++)
				{
					for (y = 0; y < m_size; y++)
					{
						bftIdx = 4 * (x + i * m_size);
						
						X = (int)m_butterflyLookupTable[bftIdx + 0];
						Y = (int)m_butterflyLookupTable[bftIdx + 1];
						w.x = m_butterflyLookupTable[bftIdx + 2];
						w.y = m_butterflyLookupTable[bftIdx + 3];
						
						data0[idx, x + y * m_size] = FFT(w, data0[idx1, X + y * m_size], data0[idx1, Y + y * m_size]);
					}
				}
			}
			
			for (i = 0; i < m_passes; i++, j++)
			{
				idx = j % 2;
				idx1 = (j + 1) % 2;
				
				for (x = 0; x < m_size; x++)
				{
					for (y = 0; y < m_size; y++)
					{
						bftIdx = 4 * (y + i * m_size);
						
						X = (int)m_butterflyLookupTable[bftIdx + 0];
						Y = (int)m_butterflyLookupTable[bftIdx + 1];
						w.x = m_butterflyLookupTable[bftIdx + 2];
						w.y = m_butterflyLookupTable[bftIdx + 3];
						
						data0[idx, x + y * m_size] = FFT(w, data0[idx1, x + X * m_size], data0[idx1, x + Y * m_size]);
					}
				}
			}

			return idx;
		}
    }
}
