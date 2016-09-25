using UnityEngine;
using System.Collections;


namespace scatterer
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

	public FourierGPU(int size)
	{	
		if(size > 256)
		{
			Debug.Log("FourierGPU::FourierGPU - fourier grid size must not be greater than 256, changing to 256");
			size = 256;
		}
		
		if(!Mathf.IsPowerOfTwo(size))
		{
			Debug.Log("FourierGPU::FourierGPU - fourier grid size must be pow2 number, changing to nearest pow2 number");
			size = Mathf.NextPowerOfTwo(size);
		}
		
		Shader shader = Core.Instance.LoadedShaders[("Scatterer/Fourier")];

		if(shader == null) Debug.Log("FourierGPU::FourierGPU - Could not find shader Math/Fourier");
	
		m_fourier = new Material(shader);

		m_size = size; //must be pow2 num
		m_fsize = (float)m_size;
		m_passes = (int)(Mathf.Log(m_fsize)/Mathf.Log(2.0f));
		
		m_butterflyLookupTable = new Texture2D[m_passes];
		
		ComputeButterflyLookupTable();
		
		m_fourier.SetFloat("_Size", m_fsize);
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
		Texture2D tex = new Texture2D(m_size, 1, TextureFormat.ARGB32, false, true);
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
					
					m_butterflyLookupTable[i].SetPixel(i1, 0, new Color( (float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k*nBlocks) / 255.0f, 0));
					
					m_butterflyLookupTable[i].SetPixel(i2, 0, new Color( (float)j1 / 255.0f, (float)j2 / 255.0f, (float)(k*nBlocks) / 255.0f, 1));
					
				}
			}
			
			m_butterflyLookupTable[i].Apply();
		}
	}
	
	public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1)
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
			
			m_fourier.SetTexture("_ButterFlyLookUp", m_butterflyLookupTable[i]);
			
			m_fourier.SetTexture("_ReadBuffer0", data0[idx1]);
			m_fourier.SetTexture("_ReadBuffer1", data1[idx1]);

			if(idx == 0)
				RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_X_2);
			else
				RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_X_2);
		}

		for(i = 0; i < m_passes; i++, j++) 
		{
			idx = j%2;
			idx1 = (j+1)%2;
			
			m_fourier.SetTexture("_ButterFlyLookUp", m_butterflyLookupTable[i]);
			
			m_fourier.SetTexture("_ReadBuffer0", data0[idx1]);
			m_fourier.SetTexture("_ReadBuffer1", data1[idx1]);

			if(idx == 0)
				RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_Y_2);
			else
				RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_Y_2);
		}

		return idx;
	}

	public int PeformFFT(RenderTexture[] data0, RenderTexture[] data1, RenderTexture[] data2)
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
			
			m_fourier.SetTexture("_ButterFlyLookUp", m_butterflyLookupTable[i]);
			
			m_fourier.SetTexture("_ReadBuffer0", data0[idx1]);
			m_fourier.SetTexture("_ReadBuffer1", data1[idx1]);
			m_fourier.SetTexture("_ReadBuffer2", data2[idx1]);
			
			if(idx == 0)
				RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_X_3);
			else
				RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_X_3);
		}

		for(i = 0; i < m_passes; i++, j++) 
		{
			idx = j%2;
			idx1 = (j+1)%2;
			
			m_fourier.SetTexture("_ButterFlyLookUp", m_butterflyLookupTable[i]);
			
			m_fourier.SetTexture("_ReadBuffer0", data0[idx1]);
			m_fourier.SetTexture("_ReadBuffer1", data1[idx1]);
			m_fourier.SetTexture("_ReadBuffer2", data2[idx1]);
			
			if(idx == 0)
				RTUtility.MultiTargetBlit(pass0, m_fourier, PASS_Y_3);
			else
				RTUtility.MultiTargetBlit(pass1, m_fourier, PASS_Y_3);
		}

		return idx;
	}

}
}

















