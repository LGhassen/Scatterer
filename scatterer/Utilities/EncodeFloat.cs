//this loads a .raw file into a rendertexture
//previously this used to be the only way to get floating point textures into video memory
//with unity 5 however we can do the same by using a texture2D
//this class is still used the first time a new .raw file is loaded

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
	
	byte[] byteBuffer = new byte[4];
	Color tempColor = Color.black;
	int index = 0;
	
	float kEncodeBit = 1.0f / 255.0f;
	float[] yzww = new float[] { 0f, 0f, 0f, 0f};
	float[] farray = new float []{ 1.0f, 255.0f, 65025.0f, 160581375.0f };
	float[] tempFloat = new float[]{0};


	public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path)
	{
		if(tex == null)
		{
			Utils.Log("EncodeFloat::WriteIntoRenderTexture- RenderTexture is null");
			return;
		}
		
		if(channels < 1 || channels > 4)
		{
			Utils.Log("EncodeFloat::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}

		
		int w = tex.width;
		int h = tex.height;
		int size = w*h*channels;
		

		IntPtr colorMapMemoryPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size*4*4); //sizeof(float) = 4; sizeof(Color) = 4*sizeof(float);
		
		float max = 1.0f;
		float min = 0.0f;

		if(!LoadRawFile(path, colorMapMemoryPointer, size, ref min, ref max))
		{
			Utils.Log("EncodeFloat::WriteIntoRenderTexture - Error loading raw file " + path);
			return;
		}
		
		DecodeFloat(w, h, channels, min, max, tex, colorMapMemoryPointer);  //colorMapMemoryPointer is also freed inside DecodeFloat
	}

		
		bool LoadRawFile(string path, IntPtr colorMap, int size, ref float min, ref float max)
		{	
			FileInfo fi = new FileInfo(path);
			
			if(fi == null)
			{
				Utils.Log("EncodeFloat::LoadRawFile - Raw file not found");
				return false;
			}

			if(size > fi.Length/4)
			{
				Utils.Log("EncodeFloat::LoadRawFile - Raw file is not the required size");
				return false;
			}


			IntPtr fdataMemoryPointer = System.Runtime.InteropServices.Marshal.AllocHGlobal(size*4); //sizeOf(float)= 4


			BinaryReader reader = new BinaryReader (fi.OpenRead ());
			byte byteRead;
			int j = 0;
			while (j < size*4)
			{
				byteRead = reader.ReadByte();
				Marshal.WriteByte (new IntPtr (fdataMemoryPointer.ToInt64 () + j), byteRead);
				j++;
			}

			reader.Close ();
			reader=null;

			for(int x = 0 ; x < size; x++) 
			{
				//Find the min and max range of data
				if( UnmanagedReadFloat(new IntPtr(fdataMemoryPointer.ToInt64() + x * 4)) > max) max = UnmanagedReadFloat(new IntPtr(fdataMemoryPointer.ToInt64() + x * 4));
				if( UnmanagedReadFloat(new IntPtr(fdataMemoryPointer.ToInt64() + x * 4)) < min) min = UnmanagedReadFloat(new IntPtr(fdataMemoryPointer.ToInt64() + x * 4));
			};
			
			min = Mathf.Abs(min);
			max += min;
			
			for(int x = 0 ; x < size; x++) 
			{
				float normalizedData = (UnmanagedReadFloat(new IntPtr(fdataMemoryPointer.ToInt64() + x * 4)) + min) / max;

				//does not work on value of one
				if(normalizedData >= 1.0f) normalizedData = 0.999999f;

				EncodeFloatRGBA(normalizedData,ref farray);//improved, non-leaky version

				UnmanagedWriteFloat(new IntPtr (colorMap.ToInt64 () + x*16    ),farray[0]);  //non-leaky version
				UnmanagedWriteFloat(new IntPtr (colorMap.ToInt64 () + x*16  +4),farray[1]);
				UnmanagedWriteFloat(new IntPtr (colorMap.ToInt64 () + x*16  +8),farray[2]);
				UnmanagedWriteFloat(new IntPtr (colorMap.ToInt64 () + x*16  +12),farray[3]);
			};

			Marshal.FreeHGlobal(fdataMemoryPointer);
			
			return true;
		}

		
	void DecodeFloat(int w, int h, int c, float min, float max, RenderTexture tex, IntPtr colorMap)  
	{
			Texture2D mapR = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapR.filterMode = FilterMode.Point;
			mapR.wrapMode = TextureWrapMode.Clamp;
			loadEncodedChannelAndSetPixels (0, w, h, c, mapR, colorMap);
		
			Texture2D mapG = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapG.filterMode = FilterMode.Point;
			mapG.wrapMode = TextureWrapMode.Clamp;
			loadEncodedChannelAndSetPixels (1, w, h, c, mapG, colorMap);

			Texture2D mapB = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapB.filterMode = FilterMode.Point;
			mapB.wrapMode = TextureWrapMode.Clamp;
			loadEncodedChannelAndSetPixels (2, w, h, c, mapB, colorMap);
		
			Texture2D mapA = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
			mapA.filterMode = FilterMode.Point;
			mapA.wrapMode = TextureWrapMode.Clamp;
			loadEncodedChannelAndSetPixels (3, w, h, c, mapA, colorMap);

			Marshal.FreeHGlobal(colorMap);

			mapR.Apply();
			mapG.Apply();
			mapB.Apply();
			mapA.Apply();
//
//
			if(m_decodeToFloat == null)
			{
				m_decodeToFloat = new Material(ShaderReplacer.Instance.LoadedShaders[("EncodeFloat/DecodeToFloat")]);

				if(m_decodeToFloat == null)
				{
					Utils.Log("EncodeFloat::WriteIntoRenderTexture2D - could not find shader EncodeFloat/DecodeToFloat. Did you change the shaders name?");
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

	}
	

		void loadEncodedChannelAndSetPixels(int channel, int w, int h, int c, Texture2D channelMap, IntPtr colorMap) //load color directly from unmanaged buffer and set pixel
		{

			for(int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					if (c > channel)
					{
						index = (x+y*w)*c+channel;

						tempColor.r = UnmanagedReadFloat(new IntPtr(colorMap.ToInt64 () + index*16));
						tempColor.g = UnmanagedReadFloat(new IntPtr(colorMap.ToInt64 () + index*16 + 4));
						tempColor.b = UnmanagedReadFloat(new IntPtr(colorMap.ToInt64 () + index*16 + 8));
						tempColor.a = UnmanagedReadFloat(new IntPtr(colorMap.ToInt64 () + index*16 + 12));	

						channelMap.SetPixel(x,y, tempColor);
	
					}
				}
			}
		}
	
	
		//this version causes no leaks
		void EncodeFloatRGBA(float val,ref float[] kEncodeMul)
		{

			kEncodeMul [0] = 1.0f;
			kEncodeMul [1] = 255.0f;
			kEncodeMul [2] = 65025.0f;
			kEncodeMul [3] = 160581375.0f;

			for( int i = 0; i < kEncodeMul.Length; ++i )
			{
				kEncodeMul[i] *= val;
				// Frac
				kEncodeMul[i] = ( float )( kEncodeMul[i] - System.Math.Truncate( kEncodeMul[i] ) );
			}

			yzww[0] = kEncodeMul[1];
			yzww[1] = kEncodeMul[2];
			yzww[2] = kEncodeMul[3];
			yzww[3] = kEncodeMul[3];

			for( int i = 0; i < kEncodeMul.Length; ++i )
			{
				kEncodeMul[i] -= yzww[i] * kEncodeBit;
			}
		}


		public float UnmanagedReadFloat(IntPtr unmanagedPointer)
		{
			byteBuffer [0] = Marshal.ReadByte (unmanagedPointer);
			byteBuffer [1] = Marshal.ReadByte (new IntPtr(unmanagedPointer.ToInt64()+1));
			byteBuffer [2] = Marshal.ReadByte (new IntPtr(unmanagedPointer.ToInt64()+2));
			byteBuffer [3] = Marshal.ReadByte (new IntPtr(unmanagedPointer.ToInt64()+3));

			return (BitConverter.ToSingle (byteBuffer, 0));
		}



		public void UnmanagedWriteFloat(IntPtr unmanagedPointer, float f)
		{
			tempFloat [0] = f;
			Buffer.BlockCopy (tempFloat, 0, byteBuffer, 0,4);
			
			Marshal.WriteByte (unmanagedPointer, byteBuffer [0]);
			Marshal.WriteByte (new IntPtr (unmanagedPointer.ToInt64 () + 1), byteBuffer [1]);
			Marshal.WriteByte (new IntPtr (unmanagedPointer.ToInt64 () + 2), byteBuffer [2]);
			Marshal.WriteByte (new IntPtr (unmanagedPointer.ToInt64 () + 3), byteBuffer [3]);
		}
}
}