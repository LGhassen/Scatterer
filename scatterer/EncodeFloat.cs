using UnityEngine;
using System.Collections;
using System.IO;
using System;

// This class is designed to take 32 bit floating point data and get it into or out of a 2D render texture.
// As there is no way in Unity to load floating point data straight into a render texture (with out dx11) the data for each
// channel must be encoded into a ARGB32 format texture and then decoded via a shader into the render texture.
//
// At the moment there are some conditions that must be meet for this to work
//
// 1 - The data must be 32 bit floating point but the render texture format can be float or half.
//
// 2 - The encode/decode step only works on data in the range 0 - 0.9999 but the function will find the highest number and normalize the 
// the data if its over 1 and then un-normalize it in the shader. This way you can have numbers greater than 1. The function will also find 
// the lowest number and if its below 0 it will add this value to all the data so the lowest number is 0. This way you can have numbers lower than 0.
// This only works when copying data into a render texture. When trying to get it out of a render texture you will need to make sure the data is in the range 0 - 0.9999
// as there is not easy way to iterate over the texture and find the min and max values. 
//
// 3 - When trying encode/decode values it not seem to work on values equal to 1 so Ive stated the max range as 0.9999.
//
// 4 - Ive added the ability to load a raw file and copy the data into a render texture. You can load 32 bit or 16 bit data. 16 bit data can be big endian or little endian

namespace scatterer{
static public class EncodeFloat
{
	static Material m_decodeToFloat, m_encodeToFloat;
	
	//This will write the values in data array into tex
	static public void WriteIntoRenderTexture(RenderTexture tex, int channels, float[] data)
	{
		if(tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - RenderTexture is null");
			return;
		}
		
		if(data == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Data is null");
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
		
		Color[] map = new Color[size];
		
		float max = 1.0f;
		float min = 0.0f;
		LoadData(data, map, size, ref min, ref max);
		
		DecodeFloat(w, h, channels, min, max, tex, map);
	}
	
	//Load 32 bit float data from raw file and write into render texture with option of returning raw loaded data
		//static public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, float[] fdata = null)
		static public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, float[] fdata)
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
		
		Color[] map = new Color[size];
		if(fdata == null) fdata = new float[size];
		
		float max = 1.0f;
		float min = 0.0f;
		if(!LoadRawFile(path, map, size, ref min, ref max, fdata))
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture - Error loading raw file " + path);
			return;
		}
		
		DecodeFloat(w, h, channels, min, max, tex, map);
		
	}
	
	//Load 16 bit float data from raw file and write into render texture with option of returning raw loaded data
		//static public void WriteIntoRenderTexture16bit(RenderTexture tex, int channels, string path, bool bigEndian, float[] fdata = null)
		static public void WriteIntoRenderTexture16bit(RenderTexture tex, int channels, string path, bool bigEndian, float[] fdata)
	{
		if(tex == null)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit- RenderTexture is null");
			return;
		}
		
		if(channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit - Channels must be 1, 2, 3, or 4");
			return;
		}
		
		int w = tex.width;
		int h = tex.height;
		int size = w*h*channels;
		
		Color[] map = new Color[size];
		if(fdata == null) fdata = new float[size];
		
		float max = 1.0f;
		float min = 0.0f;
		if(!LoadRawFile16(path, map, size, ref min, ref max, fdata, bigEndian))
		{
			Debug.Log("EncodeFloat::WriteIntoRenderTexture16bit - Error loading raw file " + path);
			return;
		}
		
		DecodeFloat(w, h, channels, min, max, tex, map);
	}
	
	//This will read the values in tex into data array. Data must be in the range 0 - 0.9999
	static public void ReadFromRenderTexture(RenderTexture tex, int channels, float[] data)
	{
		if(tex == null)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - RenderTexture is null");
			return;
		}
		
		if(data == null)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - Data is null");
			return;
		}
		
		if(channels < 1 || channels > 4)
		{
			Debug.Log("EncodeFloat::ReadFromRenderTexture - Channels must be 1, 2, 3, or 4");
			return;
		}
		
		if(m_encodeToFloat == null)
		{
			Shader shader = Shader.Find("EncodeFloat/EncodeToFloat");
			
			if(shader == null)
			{
				Debug.Log("EncodeFloat::ReadFromRenderTexture - could not find shader EncodeFloat/EncodeToFloat. Did you change the shaders name?");
				return;
			}
			
			m_encodeToFloat = new Material(shader);
		}
		
		int w = tex.width;
		int h = tex.height;
		
		RenderTexture encodeTex = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		encodeTex.filterMode = FilterMode.Point;
		Texture2D readTex = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
		
		Vector4 factor = new Vector4(1.0f, 1.0f/255.0f, 1.0f/65025.0f, 1.0f/160581375.0f);
		
		for(int i = 0; i < channels; i++)
		{
			//enocde data in tex into encodeTex
			Graphics.Blit(tex, encodeTex, m_encodeToFloat, i);
			//Read encoded values into a normal texture where we can retrive them
			RenderTexture.active = encodeTex;
			readTex.ReadPixels(new Rect(0,0,w,h),0,0);
			readTex.Apply();
			RenderTexture.active = null;
			
			//decode each pixel in readTex into a single float for the current channel
			for(int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					data[(x+y*w)*channels+i] = Vector4.Dot(readTex.GetPixel(x,y), factor);
				}
			}
		}
			encodeTex.Release ();
			UnityEngine.Object.Destroy (encodeTex);
			UnityEngine.Object.Destroy (readTex);
	}
	
	static void DecodeFloat(int w, int h, int c, float min, float max, RenderTexture tex, Color[] map)
	{
		Color[] encodedR = new Color[w*h];
		Color[] encodedG = new Color[w*h];
		Color[] encodedB = new Color[w*h];
		Color[] encodedA = new Color[w*h];
		
		for(int x = 0; x < w; x++)
		{
			for(int y = 0; y < h; y++)
			{
				encodedR[x+y*w] = new Color(0,0,0,0);
				encodedG[x+y*w] = new Color(0,0,0,0);
				encodedB[x+y*w] = new Color(0,0,0,0);
				encodedA[x+y*w] = new Color(0,0,0,0);
				
				if(c > 0) encodedR[x+y*w] = map[(x+y*w)*c+0];
				if(c > 1) encodedG[x+y*w] = map[(x+y*w)*c+1];
				if(c > 2) encodedB[x+y*w] = map[(x+y*w)*c+2];
				if(c > 3) encodedA[x+y*w] = map[(x+y*w)*c+3];
			}
		}
		
		Texture2D mapR = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
		mapR.filterMode = FilterMode.Point;
		mapR.wrapMode = TextureWrapMode.Clamp;
		mapR.SetPixels(encodedR);
		mapR.Apply();
		
		Texture2D mapG = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
		mapG.filterMode = FilterMode.Point;
		mapG.wrapMode = TextureWrapMode.Clamp;
		mapG.SetPixels(encodedG);
		mapG.Apply();
		
		Texture2D mapB = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
		mapB.filterMode = FilterMode.Point;
		mapB.wrapMode = TextureWrapMode.Clamp;
		mapB.SetPixels(encodedB);
		mapB.Apply();
		
		Texture2D mapA = new Texture2D(w, h,TextureFormat.ARGB32, false, true);
		mapA.filterMode = FilterMode.Point;
		mapA.wrapMode = TextureWrapMode.Clamp;
		mapA.SetPixels(encodedA);
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
	}
	
	static float[] EncodeFloatRGBA(float val)
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
	
	static void LoadData(float[] data, Color[] map, int size, ref float min, ref float max) 
	{	
		
		for(int x = 0 ; x < size; x++) 
		{
			//Find the min and max range of data
			if(data[x] > max) max = data[x];
			if(data[x] < min) min = data[x];
		};
		
		min = Mathf.Abs(min);
		max += min;
		
		for(int x = 0 ; x < size; x++) 
		{
			float normalizedData = (data[x] + min) / max;
			
			
			//does not work on value of one
			if(normalizedData >= 1.0f) normalizedData = 0.999999f;
			
			//I was expecting to convert the float to 4 bytes using System.BitConverter.GetBytes() and store each in a color32 object
			// but this did not work for some reason. Instead you need to split the float into four floats 
			// using the EcodeFloatRGBA function  and store in acolor object. I suspect the decode function in the 
			//shader with only work on data that has been encode using this function.
			
			float[] farray = EncodeFloatRGBA(normalizedData);
			
			map[x] = new Color(farray[0], farray[1], farray[2], farray[3]);
		};
	}
	
	static bool LoadRawFile(string path, Color[] map, int size, ref float min, ref float max, float[] fdata) 
	{	
		FileInfo fi = new FileInfo(path);
		
		if(fi == null)
		{
			Debug.Log("EncodeFloat::LoadRawFile - Raw file not found");
			return false;
		}
		
		FileStream fs = fi.OpenRead();
		
		byte[] data = new byte[fi.Length];
		fs.Read(data, 0, (int)fi.Length);
		fs.Close();
		
		//divide by 4 as there are 4 bytes in a 32 bit float
		if(size > fi.Length/4)
		{
			Debug.Log("EncodeFloat::LoadRawFile - Raw file is not the required size");
			return false;
		}
		
		int i = 0;
		for(int x = 0 ; x < size; x++) 
		{
			//Convert 4 bytes to 1 32 bit float
			fdata[x] = System.BitConverter.ToSingle(data, i);
			
			//Find the min and max range of data
			if(fdata[x] > max) max = fdata[x];
			if(fdata[x] < min) min = fdata[x];
			
			i += 4; // theres 4 bytes in 32 bits so increment i by 4
		};
		
		min = Mathf.Abs(min);
		max += min;
		
		for(int x = 0 ; x < size; x++) 
		{
			float normalizedData = (fdata[x] + min) / max;
			
			//does not work on value of one
			if(normalizedData >= 1.0f) normalizedData = 0.999999f;
			
			//I was expecting to convert the float to 4 bytes using System.BitConverter.GetBytes() and store each in a color32 object
			// but this did not work for some reason. Instead you need to split the float into four floats 
			// using the EcodeFloatRGBA function  and store in a color object. I suspect the decode function in the 
			//shader with only work on data that has been encode using this function.
			
			float[] farray = EncodeFloatRGBA(normalizedData);
			
			//By changing the order of the data in the fdata array you maybe able to 
			//using this function on data with a different byte order.
			map[x] = new Color(farray[0], farray[1], farray[2], farray[3]);
		};
		
		return true;
	}
	
	static bool LoadRawFile16(string path, Color[] map, int size, ref float min, ref float max, float[] fdata, bool bigendian) 
	{	
		FileInfo fi = new FileInfo(path);
		
		if(fi == null)
		{
			Debug.Log("EncodeFloat::LoadRawFile16 - Raw file not found");
			return false;
		}
		
		FileStream fs = fi.OpenRead();
		
		byte[] data = new byte[fi.Length];
		fs.Read(data, 0, (int)fi.Length);
		fs.Close();
		
		//divide by 2 as there are 2 bytes in a 16 bit float
		if(size > fi.Length/2)
		{
			Debug.Log("EncodeFloat::LoadRawFile16 - Raw file is not the required size");
			return false;
		}
		
		int i = 0;
		for(int x = 0 ; x < size; x++) 
		{
			//Extract 16 bit data and normalize.
			fdata[x] = (bigendian) ? (data[i++]*256.0f + data[i++]) : (data[i++] + data[i++]*256.0f);
			fdata[x] /= 65535.0f;
			
			//Find the min and max range of data
			if(fdata[x] > max) max = fdata[x];
			if(fdata[x] < min) min = fdata[x];
		};
		
		min = Mathf.Abs(min);
		max += min;
		
		for(int x = 0 ; x < size; x++) 
		{
			float normalizedData = (fdata[x] + min) / max;
			
			//I was expecting to convert the float to 4 bytes using System.BitConverter.GetBytes() and store each in a color32 object
			// but this did not work for some reason. Instead you need to split the float into four floats 
			// using the EcodeFloatRGBA function  and store in a color object. I suspect the decode function in the 
			//shader with only work on data that has been encode using this function.
			
			float[] farray = EncodeFloatRGBA(normalizedData);
			
			//By changing the order of the data in the fdata array you maybe able to 
			//using this function on data with a different byte order.
			map[x] = new Color(farray[0], farray[1], farray[2], farray[3]);
		};
		
		return true;
	}
	
}
}
