using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;

namespace Scatterer
{
	static public class CBUtility
	{
		
		public static ComputeBuffer CreateArgBuffer(int vertexCountPerInstance, int instanceCount, int startVertex, int startInstance)
		{
			ComputeBuffer buffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
			int[] args = new int[] { vertexCountPerInstance, instanceCount, startVertex, startInstance };
			buffer.SetData(args);
			
			return buffer;
		}
		
		public static int GetVertexCountPerInstance(ComputeBuffer buffer)
		{
			int[] args = new int[] { 0, 0, 0, 0 };
			buffer.GetData(args);
			return args[0];
		}
		
		private static string[,] readNames2D = new string[,]
		{
			{"read2DC1", "_Tex2D", "_Buffer2DC1"},
			{"read2DC2", "_Tex2D", "_Buffer2DC2"},
			{"read2DC3", "_Tex2D", "_Buffer2DC3"},
			{"read2DC4", "_Tex2D", "_Buffer2DC4"}
		};
		
		private static string[,] readNames3D = new string[,]
		{
			{"read3DC1", "_Tex3D", "_Buffer3DC1"},
			{"read3DC2", "_Tex3D", "_Buffer3DC2"},
			{"read3DC3", "_Tex3D", "_Buffer3DC3"},
			{"read3DC4", "_Tex3D", "_Buffer3DC4"}
		};
		
		public static void ReadFromRenderTexture(RenderTexture tex, int channels, ComputeBuffer buffer, ComputeShader readData)
		{
			if (tex == null)
				throw new ArgumentException("RenderTexture is null");
			
			if (buffer == null)
				throw new ArgumentException("Buffer is null");
			
			if (readData == null)
				throw new ArgumentException("Computer shader is null");
			
			if (channels < 1 || channels > 4)
				throw new ArgumentException("Channels must be 1, 2, 3, or 4");
			
			if (!tex.IsCreated())
				throw new ArgumentException("Tex has not been created (Call Create() on tex)");
			
			int kernel = -1;
			int depth = 1;
			
			if(tex.dimension == TextureDimension.Tex3D)
			{
				depth = tex.volumeDepth;
				kernel = readData.FindKernel(readNames3D[channels - 1, 0]);
				readData.SetTexture(kernel, readNames3D[channels - 1, 1], tex);
				readData.SetBuffer(kernel, readNames3D[channels - 1, 2], buffer);
			}
			else
			{
				kernel = readData.FindKernel(readNames2D[channels - 1, 0]);
				readData.SetTexture(kernel, readNames2D[channels - 1, 1], tex);
				readData.SetBuffer(kernel, readNames2D[channels - 1, 2], buffer);
			}
			
			if (kernel == -1)
				throw new ArgumentException("Could not find kernel " + readNames2D[channels - 1, 0]);
			
			int width = tex.width;
			int height = tex.height;
			
			readData.SetInt("_Width", width);
			readData.SetInt("_Height", height);
			readData.SetInt("_Depth", depth);
			
			//run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
			//some extra threadBlocks. This will result in some unneeded threads running 
			int padX = (width % 8 == 0) ? 0 : 1;
			int padY = (height % 8 == 0) ? 0 : 1;
			int padZ = (depth % 8 == 0) ? 0 : 1;
			
			readData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
			
		}
		
		public static void ReadSingleFromRenderTexture(RenderTexture tex, float x, float y, float z, ComputeBuffer buffer, ComputeShader readData, bool useBilinear)
		{
			if (tex == null)
				throw new ArgumentException("RenderTexture is null");
			
			if (buffer == null)
				throw new ArgumentException("Buffer is null");
			
			if (readData == null)
				throw new ArgumentException("Computer shader is null");
			
			if (!tex.IsCreated())
				throw new ArgumentException("Tex has not been created (Call Create() on tex)");
			
			int kernel = -1;
			int depth = 1;
			
			if (tex.dimension == TextureDimension.Tex3D)
			{
				if(useBilinear)
					kernel = readData.FindKernel("readSingleBilinear3D");
				else
					kernel = readData.FindKernel("readSingle3D");
				
				depth = tex.volumeDepth;
				readData.SetTexture(kernel, "_Tex3D", tex);
				readData.SetBuffer(kernel, "_BufferSingle3D", buffer);
			}
			else
			{
				if (useBilinear)
					kernel = readData.FindKernel("readSingleBilinear2D");
				else
					kernel = readData.FindKernel("readSingle2D");
				
				readData.SetTexture(kernel, "_Tex2D", tex);
				readData.SetBuffer(kernel, "_BufferSingle2D", buffer);
			}
			
			if (kernel == -1)
				throw new ArgumentException("Could not find kernel readSingle for " + tex.dimension);
			
			int width = tex.width;
			int height = tex.height;
			
			//used for point sampling
			readData.SetInt("_IdxX", (int)x);
			readData.SetInt("_IdxY", (int)y);
			readData.SetInt("_IdxZ", (int)z);
			//used for bilinear sampling
			readData.SetVector("_UV", new Vector4(x / (float)(width - 1), y / (float)(height - 1), z / (float)(depth - 1), 0.0f));
			
			readData.Dispatch(kernel, 1, 1, 1);
			
		}
		
		public static void WriteIntoRenderTexture(RenderTexture tex, int channels, ComputeBuffer buffer, ComputeShader writeData)
		{
			if (tex == null)
				throw new ArgumentException("RenderTexture is null");
			
			if (buffer == null)
				throw new ArgumentException("Buffer is null");
			
			if (writeData == null)
				throw new ArgumentException("Computer shader is null");
			
			if (channels < 1 || channels > 4)
				throw new ArgumentException("Channels must be 1, 2, 3, or 4");
			
			if (!tex.enableRandomWrite)
				throw new ArgumentException("You must enable random write on render texture");
			
			if (!tex.IsCreated())
				throw new ArgumentException("Tex has not been created (Call Create() on tex)");
			
			int kernel = -1;
			int depth = 1;
			string D = "2D";
			string C = "C" + channels.ToString();
			
			if (tex.dimension == TextureDimension.Tex3D)
			{
				depth = tex.volumeDepth;
				D = "3D";
			}
			
			kernel = writeData.FindKernel("write" + D + C);
			
			if (kernel == -1)
				throw new ArgumentException("Could not find kernel " + "write" + D + C);
			
			int width = tex.width;
			int height = tex.height;
			
			//set the compute shader uniforms
			writeData.SetTexture(kernel, "_Des" + D + C, tex);
			writeData.SetInt("_Width", width);
			writeData.SetInt("_Height", height);
			writeData.SetInt("_Depth", depth);
			writeData.SetBuffer(kernel, "_Buffer" + D + C, buffer);
			//run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
			//some extra threadBlocks. This will result in some unneeded threads running 
			int padX = (width % 8 == 0) ? 0 : 1;
			int padY = (height % 8 == 0) ? 0 : 1;
			int padZ = (depth % 8 == 0) ? 0 : 1;
			
			writeData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
		}
		
		public static void WriteIntoRenderTexture(RenderTexture tex, int channels, string path, ComputeBuffer buffer, ComputeShader writeData)
		{
			if (tex == null)
				throw new ArgumentException("RenderTexture is null");
			
			if (buffer == null)
				throw new ArgumentException("buffer is null");
			
			if (writeData == null)
				throw new ArgumentException("Computer shader is null");
			
			if (channels < 1 || channels > 4)
				throw new ArgumentException("Channels must be 1, 2, 3, or 4");
			
			if (!tex.enableRandomWrite)
				throw new ArgumentException("You must enable random write on render texture");
			
			if (!tex.IsCreated())
				throw new ArgumentException("Tex has not been created (Call Create() on tex)");
			
			int kernel = -1;
			int depth = 1;
			string D = "2D";
			string C = "C" + channels.ToString();
			
			if (tex.dimension == TextureDimension.Tex3D)
			{
				depth = tex.volumeDepth;
				D = "3D";
			}
			
			kernel = writeData.FindKernel("write" + D + C);
			
			if (kernel == -1)
				throw new ArgumentException("Could not find kernel " + "write" + D + C);
			
			int width = tex.width;
			int height = tex.height;
			int size = width * height * depth * channels;
			
			float[] map = new float[size];
			LoadRawFile(path, map, size);
			
			buffer.SetData(map);
			
			//set the compute shader uniforms
			writeData.SetTexture(kernel, "_Des" + D + C, tex);
			writeData.SetInt("_Width", width);
			writeData.SetInt("_Height", height);
			writeData.SetInt("_Depth", depth);
			writeData.SetBuffer(kernel, "_Buffer" + D + C, buffer);
			//run the  compute shader. Runs in threads of 8 so non divisable by 8 numbers will need
			//some extra threadBlocks. This will result in some unneeded threads running 
			int padX = (width % 8 == 0) ? 0 : 1;
			int padY = (height % 8 == 0) ? 0 : 1;
			int padZ = (depth % 8 == 0) ? 0 : 1;
			
			writeData.Dispatch(kernel, Mathf.Max(1, width / 8 + padX), Mathf.Max(1, height / 8 + padY), Mathf.Max(1, depth / 8 + padZ));
		}
		
		private static void LoadRawFile(string path, float[] map, int size)
		{
			FileInfo fi = new FileInfo(path);
			
			if (fi == null)
				throw new ArgumentException("Raw file not found (" + path + ")");
			
			FileStream fs = fi.OpenRead();
			
			byte[] data = new byte[fi.Length];
			fs.Read(data, 0, (int)fi.Length);
			fs.Close();
			
			//divide by 4 as there are 4 bytes in a 32 bit float
			if (size > fi.Length / 4)
				throw new ArgumentException("Raw file is not the required size (" + path + ")");
			
			for (int x = 0, i = 0; x < size; x++, i += 4)
			{
				//Convert 4 bytes to 1 32 bit float
				map[x] = System.BitConverter.ToSingle(data, i);
			};
			
		}
	}
}




















