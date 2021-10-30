using UnityEngine;
using System;
using System.Collections;
using System.IO;

namespace scatterer
{

	//This class is now obsolete and can be replaced with a single blit


	/// <summary>
	/// This class is designed to take 32 bit floating point data and get it into a 2D render texture.
	/// As there is no way in Unity to load floating point data straight into a render texture (with out dx11) the data for each
	/// channel must be encoded into a ARGB32 format texture and then decoded via a shader into the render texture.
	///
	/// At the moment there are some conditions that must be meet for this to work
	///
	/// 1 - The data must be 32 bit floating point but the render texture format can be float or half.
	///
	/// 2 - The encode/decode step only works on data in the range 0 - 0.9999 but the function will find the highest number and normalize the 
	///     the data if its over 1 and then un-normalize it in the shader. This way you can have numbers greater than 1. 
	///     The function will also find the lowest number and if its below 0 it will add this value to all the data so the lowest number is 0.
	///     This way you can have numbers lower than 0. This only works when copying data into a render texture. 
	///     When trying to get it out of a render texture you will need to make sure the data is in the range 0 - 0.9999
	///     as there is not easy way to iterate over the texture and find the min and max values. 
	///
	/// 3 - When trying encode/decode values it does not seem to work on values equal to 1 so Ive stated the max range as 0.9999.
	///
	/// 4 - Ive added the ability to load a raw file and copy the data into a render texture. 
	///     You can load 32 bit or 16 bit data. 16 bit data can be big endian or little endian
	/// </summary>
	public class WriteFloat
	{
		
		const float MAX_VALUE = 0.999999f;
		
		/// <summary>
		/// The material which will write the encoded 
		/// data into the render texture.
		/// </summary>
		Material m_writeToFloat;
		
		/// <summary>
		/// The textures to hold the encoded data.
		/// To increase performance Ive made it so these
		/// textures are created once and reused. 
		/// This does mean that the data must match the texture size. 
		/// You can call the resize function to recreate the 
		/// textures at a new size if needed. 
		/// </summary>
		Texture2D m_mapR, m_mapG, m_mapB, m_mapA;
		
		/// <summary>
		/// The current size of the maps.
		/// </summary>
		int m_width, m_height;
		
		/// <summary>
		/// Create a new object that can write data into a render texture of dimensions w and h.
		/// </summary>
		public WriteFloat(int w, int h)
		{

			Shader shader = ShaderReplacer.Instance.LoadedShaders[("EncodeFloat/WriteToFloat")];

			if(shader == null)
			{
				throw new InvalidOperationException("Could not find shader EncodeFloat/WriteToFloat. Did you change the shaders name?");
			}
			
			m_writeToFloat = new Material(shader);
			
			m_width = w;
			m_height = h;
			
			m_mapR = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
			m_mapR.filterMode = FilterMode.Point;
			m_mapR.wrapMode = TextureWrapMode.Clamp;
			
			m_mapG = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
			m_mapG.filterMode = FilterMode.Point;
			m_mapG.wrapMode = TextureWrapMode.Clamp;
			
			m_mapB = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
			m_mapB.filterMode = FilterMode.Point;
			m_mapB.wrapMode = TextureWrapMode.Clamp;
			
			m_mapA = new Texture2D(w, h, TextureFormat.ARGB32, false, true);
			m_mapA.filterMode = FilterMode.Point;
			m_mapA.wrapMode = TextureWrapMode.Clamp;
			
		}
		
		/// <summary>
		/// Resize to new dimensions.
		/// </summary>
		public void Resize(int w, int h)
		{
			
			m_width = w;
			m_height = h;
			
			m_mapR.Resize(w, h, TextureFormat.ARGB32, false);
			m_mapG.Resize(w, h, TextureFormat.ARGB32, false);
			m_mapB.Resize(w, h, TextureFormat.ARGB32, false);
			m_mapA.Resize(w, h, TextureFormat.ARGB32, false);
			
		}
		
		/// <summary>
		/// This will write the values in data array into tex
		/// </summary>
		/// <param name="tex">The texture to write into</param>
		/// <param name="channels">The number of channels in texture</param>
		/// <param name="data">The data to write into texture. Size must width * height * channels.</param>
		public void WriteIntoRenderTexture(RenderTexture tex, int channels, float[] data)
		{
			
			if(channels < 1 || channels > 4)
			{
				Utils.LogDebug("Channels must be 1, 2, 3, or 4");
				return;
			}
			
			int w = tex.width;
			int h = tex.height;
			
			if(w != m_width || h != m_height)
			{
				Utils.LogDebug("Render texture not the correct dimensions");
				return;
			}
			
			int size = w*h*channels;
			
			Color[] map = new Color[size];
			
			float max = 1.0f;
			float min = 0.0f;
			LoadData(data, map, size, ref min, ref max);
			
			Write(w, h, channels, min, max, tex, map);			
		}
		
		/// <summary>
		/// Load 32 bit float data from raw file and write into render texture
		/// width the option of writting the loaded data into the fdata array if it is not null.
		/// </summary>
		/// <param name="tex">The texture to write into</param>
		/// <param name="channels">The number of channels in texture</param>
		/// <param name="data">The data to write into texture. Size must width * height * channels.</param>
		/// <param name="fdata">If this is not the data in the file will be written into this.</param>
//		public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, float[] fdata = null)
		public void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, float[] fdata)
		{
			
			if(channels < 1 || channels > 4)
			{
				Utils.LogDebug("EncodeFloat::WriteIntoRenderTexture - Channels must be 1, 2, 3, or 4");
				return;
			}
			
			int w = tex.width;
			int h = tex.height;
			
			if(w != m_width || h != m_height)
			{
				Utils.LogDebug("Render texture not the correct dimensions");
				return;
			}
			
			int size = w*h*channels;
			
			Color[] map = new Color[size];
			if(fdata == null) fdata = new float[size];
			
			float max = 1.0f;
			float min = 0.0f;
			if(!LoadRawFile(path, map, size, ref min, ref max, fdata))
			{
				Utils.LogDebug("EncodeFloat::WriteIntoRenderTexture - Error loading raw file " + path);
				return;
			}
			
			Write(w, h, channels, min, max, tex, map);
		}
		
		/// <summary>
		/// Load 16 bit float data from raw file and write into render texture
		/// width the option of writting the loaded data in to the fdata array if it is not null.
		/// </summary>
		/// <param name="tex">The texture to write into</param>
		/// <param name="channels">The number of channels in texture</param>
		/// <param name="data">The data to write into texture. Size must width * height * channels.</param>
		/// <param name="fdata">If this is not the data in the file will be written into this.</param>

//		public void WriteIntoRenderTexture16bit(RenderTexture tex, int channels, string path, bool bigEndian, float[] fdata = null)
		public void WriteIntoRenderTexture16bit(RenderTexture tex, int channels, string path, bool bigEndian, float[] fdata)
		{
			
			if(channels < 1 || channels > 4)
			{
				Utils.LogDebug("Channels must be 1, 2, 3, or 4");
				return;
			}
			
			int w = tex.width;
			int h = tex.height;
			
			if(w != m_width || h != m_height)
			{
				Utils.LogDebug("Render texture not the correct dimensions");
				return;
			}
			
			int size = w*h*channels;
			
			Color[] map = new Color[size];
			if(fdata == null) fdata = new float[size];
			
			float max = 1.0f;
			float min = 0.0f;
			if(!LoadRawFile16(path, map, size, ref min, ref max, fdata, bigEndian))
			{
				Utils.LogDebug("EncodeFloat::WriteIntoRenderTexture16bit - Error loading raw file " + path);
				return;
			}
			
			Write(w, h, channels, min, max, tex, map);
		}
		
		/// <summary>
		/// Write the encoded float in map into texture.
		/// </summary>
		void Write(int w, int h, int c, float min, float max, RenderTexture tex, Color[] map)
		{
			
			for(int x = 0; x < w; x++)
			{
				for(int y = 0; y < h; y++)
				{
					
					if(c > 0)
						m_mapR.SetPixel(x, y, map[(x+y*w)*c+0]);
					else
						m_mapR.SetPixel(x, y, Color.clear);
					
					if(c > 1)
						m_mapG.SetPixel(x, y, map[(x+y*w)*c+1]);
					else
						m_mapG.SetPixel(x, y, Color.clear);
					
					if(c > 2)
						m_mapB.SetPixel(x, y, map[(x+y*w)*c+2]);
					else
						m_mapB.SetPixel(x, y, Color.clear);
					
					if(c > 3)
						m_mapA.SetPixel(x, y, map[(x+y*w)*c+3]);
					else
						m_mapA.SetPixel(x, y, Color.clear);
				}
			}
			
			m_mapR.Apply();
			m_mapG.Apply();
			m_mapB.Apply();
			m_mapA.Apply();
			
			m_writeToFloat.SetFloat("_Max", max);
			m_writeToFloat.SetFloat("_Min", min);
			m_writeToFloat.SetTexture("_TexR", m_mapR);
			m_writeToFloat.SetTexture("_TexG", m_mapG);
			m_writeToFloat.SetTexture("_TexB", m_mapB);
			m_writeToFloat.SetTexture("_TexA", m_mapA);
			Graphics.Blit(null, tex, m_writeToFloat);
		}
		
		/// <summary>
		/// Encode a float into 4 bytes as normilized floats.
		/// </summary>
		float[] EncodeFloatRGBA(float val)
		{
			
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
		
		/// <summary>
		/// Find the range of the data and then pak it into the map.
		/// </summary>
		void LoadData(float[] data, Color[] map, int size, ref float min, ref float max) 
		{	
			
			for(int x = 0 ; x < size; x++) 
			{
				//Find the min and max range of data
				if(data[x] > max) max = data[x];
				if(data[x] < min) min = data[x];
			};
			
			min = Mathf.Abs(min);
			max += min;
			
			PackData(map, size, min, max, data);
		}
		
		/// <summary>
		/// Load the data from a 32 bit file, find the range of the data and then pak it into the map.
		/// </summary>
		bool LoadRawFile(string path, Color[] map, int size, ref float min, ref float max, float[] fdata) 
		{	
			FileInfo fi = new FileInfo(path);
			
			if(fi == null)
			{
				Utils.LogDebug("Raw file not found");
				return false;
			}
			
			FileStream fs = fi.OpenRead();
			
			byte[] data = new byte[fi.Length];
			fs.Read(data, 0, (int)fi.Length);
			fs.Close();
			
			//divide by 4 as there are 4 bytes in a 32 bit float
			if(size > fi.Length/4)
			{
				Utils.LogDebug("Raw file is not the required size");
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
			
			PackData(map, size, min, max, fdata);
			
			return true;
		}
		
		/// <summary>
		/// Load the data from a 16 bit file, find the range of the data and then pak it into the map.
		/// </summary>
		bool LoadRawFile16(string path, Color[] map, int size, ref float min, ref float max, float[] fdata, bool bigendian) 
		{	
			FileInfo fi = new FileInfo(path);
			
			if(fi == null)
			{
				Utils.LogDebug("Raw file not found");
				return false;
			}
			
			FileStream fs = fi.OpenRead();
			
			byte[] data = new byte[fi.Length];
			fs.Read(data, 0, (int)fi.Length);
			fs.Close();
			
			//divide by 2 as there are 2 bytes in a 16 bit float
			if(size > fi.Length/2)
			{
				Utils.LogDebug("Raw file is not the required size");
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
			
			PackData(map, size, min, max, fdata);
			
			return true;
		}
		
		/// <summary>
		/// Encode data into the map.
		/// </summary>
		void PackData(Color[] map, int size, float min, float max, float[] data)
		{
			for(int x = 0 ; x < size; x++) 
			{
				float normalizedData = (data[x] + min) / max;
				
				//does not work on value of one
				if(normalizedData >= 1.0f) normalizedData = MAX_VALUE;
				
				float[] farray = EncodeFloatRGBA(normalizedData);
				
				map[x] = new Color(farray[0], farray[1], farray[2], farray[3]);
			};
		}


		public void OnDestroy ()
		{
			UnityEngine.Object.Destroy(m_mapA);
			UnityEngine.Object.Destroy(m_mapR);
			UnityEngine.Object.Destroy(m_mapG);
			UnityEngine.Object.Destroy(m_mapB);
			UnityEngine.Object.Destroy (m_writeToFloat);

		}
		
	}
	
}
