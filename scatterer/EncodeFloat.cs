using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace scatterer{
public class EncodeFloat
{
	Material m_decodeToFloat;
	Color[] encoded;
	


	public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path)

	{
		if(tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture- RenderTexture is null");
			return;
		}
		
		if(channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}

		
		int w = tex.width;
		int h = tex.height;
		int size = w*h*channels;
		
//		Color[] map = new Color[size];
		IntPtr colorMapMemoryPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size*4*4); //sizeof(float) = 4; sizeof(Color) = 4*sizeof(float);
		
		float max = 1.0f;
		float min = 0.0f;
//		if(!LoadRawFile(path, map, size, ref min, ref max))
		if(!LoadRawFile(path, colorMapMemoryPointer, size, ref min, ref max))
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Error loading raw file " + path);
			return;
		}
		
//		DecodeFloat(w, h, channels, min, max, tex, map);
		DecodeFloat(w, h, channels, min, max, tex, colorMapMemoryPointer);

		Marshal.FreeHGlobal(colorMapMemoryPointer);

	}
	

//	void DecodeFloat(int w, int h, int c, float min, float max, RenderTexture tex, Color[] map)
	void DecodeFloat(int w, int h, int c, float min, float max, RenderTexture tex, IntPtr colorMap)
	{
			encoded = new Color[w*h];

//			loadEncodedTo2Dtex (0, w, h, c, encoded, map);
			loadEncodedTo2Dtex (0, w, h, c, encoded, colorMap);
			
			Texture2D mapR = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapR.filterMode = FilterMode.Point;
			mapR.wrapMode = TextureWrapMode.Clamp;
			mapR.SetPixels(encoded);
			mapR.Apply();
			
			
//			loadEncodedTo2Dtex (1, w, h, c, encoded, map);
			loadEncodedTo2Dtex (1, w, h, c, encoded, colorMap);

			
			Texture2D mapG = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapG.filterMode = FilterMode.Point;
			mapG.wrapMode = TextureWrapMode.Clamp;
			mapG.SetPixels(encoded);
			mapG.Apply();
			
			
//			loadEncodedTo2Dtex (2, w, h, c, encoded, map);
			loadEncodedTo2Dtex (2, w, h, c, encoded, colorMap);

			
			Texture2D mapB = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapB.filterMode = FilterMode.Point;
			mapB.wrapMode = TextureWrapMode.Clamp;
			mapB.SetPixels(encoded);
			mapB.Apply();
			
			
//			loadEncodedTo2Dtex (3, w, h, c, encoded, map);
			loadEncodedTo2Dtex (3, w, h, c, encoded, colorMap);

			//free colorMap here to limit memory spike even if slightly?

			
			Texture2D mapA = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapA.filterMode = FilterMode.Point;
			mapA.wrapMode = TextureWrapMode.Clamp;
			mapA.SetPixels(encoded);
			mapA.Apply();


			if(m_decodeToFloat == null)
			{
				//Shader shader = Shader.Find("EncodeFloat/DecodeToFloat");
				m_decodeToFloat = new Material(ShaderTool.GetMatFromShader("CompiledDecodedToFloat.shader"));
				
				
				if(m_decodeToFloat == null)
				{
					Debug.Log("EncodeFloat::WriteIntoRenderTexture2D - could not find shader EncodeFloat/DecodeToFloat. Did you change the shaders name?");
					return;
				}
			}
			
			m_decodeToFloat.SetFloat("_Max", max);
			m_decodeToFloat.SetFloat("_Min", min);
			m_decodeToFloat.SetTexture("_TexR", mapR);
			m_decodeToFloat.SetTexture("_TexG", mapG);
			m_decodeToFloat.SetTexture("_TexB", mapB);
			m_decodeToFloat.SetTexture("_TexA", mapA);
			Graphics.Blit(null, tex, m_decodeToFloat);
			
			UnityEngine.Object.Destroy (mapR);
			UnityEngine.Object.Destroy (mapG);
			UnityEngine.Object.Destroy (mapB);
			UnityEngine.Object.Destroy (mapA);

			encoded = null;
	}
	
	
//	void loadEncodedTo2Dtex(int channel, int w, int h, int c, Color[] encoded, Color[] map)
	void loadEncodedTo2Dtex(int channel, int w, int h, int c, Color[] encoded, IntPtr colorMap)
	{
		int index = 0;
		for(int x = 0; x < w; x++)
		{
			for(int y = 0; y < h; y++)
			{
				encoded[x+y*w] = new Color(0,0,0,0);
//				if(c > channel) encoded[x+y*w] = map[(x+y*w)*c+channel];
				if (c > channel)
				{
					index = (x+y*w)*c+channel;
//					encoded[x+y*w] = map[index];

					encoded[x+y*w].r = UnmanagedDecodeFloat(Marshal.ReadInt32(new IntPtr (colorMap.ToInt64 () + index*16   )));
					encoded[x+y*w].g = UnmanagedDecodeFloat(Marshal.ReadInt32(new IntPtr (colorMap.ToInt64 () + index*16  +4)));
					encoded[x+y*w].b = UnmanagedDecodeFloat(Marshal.ReadInt32(new IntPtr (colorMap.ToInt64 () + index*16  +8)));
					encoded[x+y*w].a = UnmanagedDecodeFloat(Marshal.ReadInt32(new IntPtr (colorMap.ToInt64 () + index*16  +12)));


//					Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16    ), System.BitConverter.ToInt32(farray[0]));
//					Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +4), System.BitConverter.ToInt32(farray[1]));
//					Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +8), System.BitConverter.ToInt32(farray[2]));
//					Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +12), System.BitConverter.ToInt32(farray[3]));
					
				}
			}
		}
	}


	float[] EncodeFloatRGBA(float val)
	{
		//Thanks to karljj1 for this function
		float[] kEncodeMul = new float[]{ 1.0f, 255.0f, 65025.0f, 160581375.0f };
		float kEncodeBit = 1.0f / 255.0f;            
		for( int i = 0; i < kEncodeMul.Length; ++i )
		{
			kEncodeMul[i] *= val;
			// Frac
			kEncodeMul[i] = ( float )( kEncodeMul[i] - System.Math.Truncate( kEncodeMul[i] ) );
		}
		
		// enc -= enc.yzww * kEncodeBit;
		float[] yzww = new float[] { kEncodeMul[1], kEncodeMul[2], kEncodeMul[3], kEncodeMul[3] };
		for( int i = 0; i < kEncodeMul.Length; ++i )
		{
			kEncodeMul[i] -= yzww[i] * kEncodeBit;
		}
		
		return kEncodeMul;
	}
	
	

//		bool LoadRawFile(string path, Color[] map, int size, ref float min, ref float max) 
		bool LoadRawFile(string path, IntPtr colorMap, int size, ref float min, ref float max) 
		{	
			FileInfo fi = new FileInfo(path);
			
			if(fi == null)
			{
				Debug.Log("EncodeFloat::LoadRawFile - Raw file not found");
				return false;
			}
			
			FileStream fs = fi.OpenRead();
//			
//			byte[] data = new byte[fi.Length];   //choufha mba3ed kech ta3melha bel intPtr
//			fs.Read(data, 0, (int)fi.Length);


			int byteRead;
			IntPtr fdataMemoryPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size*4); //sizeOf(float) --> 4
			int j = 0;
			while (  ((byteRead = fs.ReadByte()) != -1)    &&    (j < size*4) )
			{
				Marshal.WriteByte (new IntPtr (fdataMemoryPointer.ToInt64 () + j), BitConverter.GetBytes(byteRead)[0]);
				j++;
//				Debug.Log(j.ToString());
			}

			fs.Close();

			fs = null;
			
			//divide by 4 as there are 4 bytes in a 32 bit float
			if(size > fi.Length/4)
			{
				Debug.Log("EncodeFloat::LoadRawFile - Raw file is not the required size");
				return false;
			}

				
//			float[] fdata = new float[size];
//			IntPtr fdataMemoryPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size*4); //sizeOf(float) --> 4

			Debug.Log ("Marshal copying data");

			int i = 0;
			for(int x = 0 ; x < size; x++) 
			{
				//Convert 4 bytes to 1 32 bit float
//				fdata[x] = System.BitConverter.ToSingle(data, i);
//				Marshal.WriteInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4), System.BitConverter.ToInt32(data, i));

//				//Find the min and max range of data
//				if(fdata[x] > max) max = fdata[x];
//				if(fdata[x] < min) min = fdata[x];

				if( UnmanagedDecodeFloat(Marshal.ReadInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4))) > max) max = UnmanagedDecodeFloat(Marshal.ReadInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4)));
				if( UnmanagedDecodeFloat(Marshal.ReadInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4))) < min) min = UnmanagedDecodeFloat(Marshal.ReadInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4)));
				
				i += 4; // theres 4 bytes in 32 bits so increment i by 4
			};
			
			min = Mathf.Abs(min);
			max += min;

			float[] farray;

			for(int x = 0 ; x < size; x++) 
			{
//				float normalizedData = (fdata[x] + min) / max;
				float normalizedData = (UnmanagedDecodeFloat(Marshal.ReadInt32 (new IntPtr(fdataMemoryPointer.ToInt64() + x * 4))) + min) / max;
				
				//does not work on value of one
				if(normalizedData >= 1.0f) normalizedData = 0.999999f;

				farray = EncodeFloatRGBA(normalizedData);

//				map[x] = new Color(farray[0], farray[1], farray[2], farray[3]);

				Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16    ), UnmanagedEncodeFloat(farray[0]));
				Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +4), UnmanagedEncodeFloat(farray[1]));
				Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +8), UnmanagedEncodeFloat(farray[2]));
				Marshal.WriteInt32 (new IntPtr (colorMap.ToInt64 () + x*16  +12), UnmanagedEncodeFloat(farray[3]));


			};

			Debug.Log ("Marshal freeing unmanaged memory");
			Marshal.FreeHGlobal(fdataMemoryPointer);

			return true;
		}

		public static Int32 UnmanagedEncodeFloat(float f)	
		{
			return BitConverter.ToInt32(BitConverter.GetBytes(f),0);
		}
		
		public static float UnmanagedDecodeFloat(Int32 enc)
			
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(enc), 0);	
		}




}
}