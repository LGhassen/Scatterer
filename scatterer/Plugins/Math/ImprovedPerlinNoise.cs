using UnityEngine;
using System.Collections;

public class ImprovedPerlinNoise
{
	const int SIZE = 256;
	int[] m_perm = new int[SIZE+SIZE];
	Texture2D m_permTable1D, m_permTable2D, m_gradient2D, m_gradient3D, m_gradient4D;
	
	public Texture2D GetPermutationTable1D() { return m_permTable1D; }
	public Texture2D GetPermutationTable2D() { return m_permTable2D; }
	public Texture2D GetGradient2D() { return m_gradient2D; }
	public Texture2D GetGradient3D() { return m_gradient3D; }
	public Texture2D GetGradient4D() { return m_gradient4D; }

	public ImprovedPerlinNoise(int seed)
	{
		UnityEngine.Random.seed = seed;

		int i, j, k;
		for (i = 0 ; i < SIZE ; i++) 
		{
			m_perm[i] = i;
		}

		while (--i != 0) 
		{
			k = m_perm[i];
			j = UnityEngine.Random.Range(0, SIZE);
			m_perm[i] = m_perm[j];
			m_perm[j] = k;
		}
	
		for (i = 0 ; i < SIZE; i++) 
		{
			m_perm[SIZE + i] = m_perm[i];
		}
				
	}
	
	public void LoadResourcesFor2DNoise()
	{
		LoadPermTable1D();
		LoadGradient2D();
	}
	
	public void LoadResourcesFor3DNoise()
	{
		LoadPermTable2D();
		LoadGradient3D();
	}
	
	public void LoadResourcesFor4DNoise()
	{
		LoadPermTable1D();
		LoadPermTable2D();
		LoadGradient4D();
	}
	
	void LoadPermTable1D()
	{
		if(m_permTable1D) return;
		
		m_permTable1D = new Texture2D(SIZE, 1, TextureFormat.Alpha8, false, true);
		m_permTable1D.filterMode = FilterMode.Point;
		m_permTable1D.wrapMode = TextureWrapMode.Repeat;
		
		for(int x = 0; x < SIZE; x++)
		{
			m_permTable1D.SetPixel(x, 1, new Color(0,0,0,(float)m_perm[x]/(float)(SIZE-1)));
		}
		
		m_permTable1D.Apply();
	}
	
	//This is special table that has been optimesed for 3D noise. It can also be use in 4D noise for some optimisation but the 1D perm table is still needed 
	void LoadPermTable2D()
	{
		if(m_permTable2D) return;
		
		m_permTable2D = new Texture2D(SIZE, SIZE, TextureFormat.ARGB32, false, true);
		m_permTable2D.filterMode = FilterMode.Point;
		m_permTable2D.wrapMode = TextureWrapMode.Repeat;
		
		for(int x = 0; x < SIZE; x++)
		{
			for(int y = 0; y < SIZE; y++)
			{
				int A = m_perm[x] + y;
				int AA = m_perm[A];
				int AB = m_perm[A + 1];
				
			  	int B =  m_perm[x + 1] + y;
			  	int BA = m_perm[B];
			  	int BB = m_perm[B + 1];
				
				m_permTable2D.SetPixel(x, y, new Color((float)AA/255.0f, (float)AB/255.0f, (float)BA/255.0f, (float)BB/255.0f) );
			}
		}
		
		m_permTable2D.Apply();
	}
	
	void LoadGradient2D()
	{
		if(m_gradient2D) return;
		
		m_gradient2D = new Texture2D(8, 1, TextureFormat.RGB24, false, true);
		m_gradient2D.filterMode = FilterMode.Point;
		m_gradient2D.wrapMode = TextureWrapMode.Repeat;
		
		for(int i = 0; i < 8; i++)
		{
			float R = (GRADIENT2[i*2+0] + 1.0f) * 0.5f;
			float G = (GRADIENT2[i*2+1] + 1.0f) * 0.5f;
			
			m_gradient2D.SetPixel(i, 0, new Color(R, G, 0, 1));
		}
		
		m_gradient2D.Apply();
		
	}
	
	void LoadGradient3D()
	{
		if(m_gradient3D) return;
		
		m_gradient3D = new Texture2D(SIZE, 1, TextureFormat.RGB24, false, true);
		m_gradient3D.filterMode = FilterMode.Point;
		m_gradient3D.wrapMode = TextureWrapMode.Repeat;
		
		for(int i = 0; i < SIZE; i++)
		{
			int idx = m_perm[i] % 16;
			
			float R = (GRADIENT3[idx*3+0] + 1.0f) * 0.5f;
			float G = (GRADIENT3[idx*3+1] + 1.0f) * 0.5f;
			float B = (GRADIENT3[idx*3+2] + 1.0f) * 0.5f;
			
			m_gradient3D.SetPixel(i, 0, new Color(R, G, B, 1));
		}
		
		m_gradient3D.Apply();
		
	}
	
	void LoadGradient4D()
	{
		if(m_gradient4D) return;
		
		m_gradient4D = new Texture2D(SIZE, 1, TextureFormat.ARGB32, false, true);
		m_gradient4D.filterMode = FilterMode.Point;
		m_gradient4D.wrapMode = TextureWrapMode.Repeat;
		
		for(int i = 0; i < SIZE; i++)
		{
			int idx = m_perm[i] % 32;
			
			float R = (GRADIENT4[idx*4+0] + 1.0f) * 0.5f;
			float G = (GRADIENT4[idx*4+1] + 1.0f) * 0.5f;
			float B = (GRADIENT4[idx*4+2] + 1.0f) * 0.5f;
			float A = (GRADIENT4[idx*4+3] + 1.0f) * 0.5f;
			
			m_gradient4D.SetPixel(i, 0, new Color(R, G, B, A));
		}
		
		m_gradient4D.Apply();
		
	}
	
	static float[] GRADIENT2 = new float[] 
	{ 
    	0, 1, 
		1, 1,
		1, 0, 
		1, -1, 
    	0, -1, 
		-1, -1, 
		-1, 0, 
		-1, 1,
	};
	
	static float[] GRADIENT3 = new float[] 
	{
	    1,1,0,
	    -1,1,0,
	    1,-1,0,
	    -1,-1,0,
	    1,0,1,
	    -1,0,1,
	    1,0,-1,
	    -1,0,-1, 
	    0,1,1,
	    0,-1,1,
	    0,1,-1,
	    0,-1,-1,
	    1,1,0,
	    0,-1,1,
	    -1,1,0,
	    0,-1,-1,
	};
	
	static float[] GRADIENT4 = new float[]
	{
		0, -1, -1, -1,
		0, -1, -1, 1,
		0, -1, 1, -1,
		0, -1, 1, 1,
		0, 1, -1, -1,
		0, 1, -1, 1,
		0, 1, 1, -1,
		0, 1, 1, 1,
		-1, -1, 0, -1,
		-1, 1, 0, -1,
		1, -1, 0, -1,
		1, 1, 0, -1,
		-1, -1, 0, 1,
		-1, 1, 0, 1,
		1, -1, 0, 1,
		1, 1, 0, 1,
		
		-1, 0, -1, -1,
		1, 0, -1, -1,
		-1, 0, -1, 1,
		1, 0, -1, 1,
		-1, 0, 1, -1,
		1, 0, 1, -1,
		-1, 0, 1, 1,
		1, 0, 1, 1,
		0, -1, -1, 0,
		0, -1, -1, 0,
		0, -1, 1, 0,
		0, -1, 1, 0,
		0, 1, -1, 0,
		0, 1, -1, 0,
		0, 1, 1, 0,
		0, 1, 1, 0,
	};

}













