using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


namespace scatterer{
	public class EncodeFloat2D
	{
		
		byte[] byteBuffer = new byte[4];

		
		public void WriteIntoTexture2D(Texture2D tex, int channels, string path)
			
		{
			if(tex == null)
			{
				Debug.Log("EncodeFloat::WriteIntoTexture2D- Texture is null");
				return;
			}
			
			if(channels < 1 || channels > 4)
			{
				Debug.Log("EncodeFloat::WriteIntoTexture2D - Channels must be 1, 2, 3, or 4");
				return;
			}
			

			int w = tex.width;
			int h = tex.height;
			int size = w*h*channels;
			
			FileInfo fi = new FileInfo(path);
			
			if(fi == null)
			{
				Debug.Log("EncodeFloat::LoadRawFile - Raw file not found");
				//return false;
			}
			
			if(size > fi.Length/4)
			{
				Debug.Log("EncodeFloat::LoadRawFile - Raw file is not the required size");
				//return false;
			}
			
			Debug.Log ("file size " + fi.Length.ToString () + " bytes");
			Debug.Log ("Expected file size " + (size*4).ToString () + " bytes");

			
//			BinaryReader reader = new BinaryReader (fi.OpenRead ());
////			byte byteRead;
////			int j = 0;
//			Color tempColor = Color.black;

//			for (int x = 0; x < w; x++) {
//				for (int y = 0; y < h; y++) {
//					for (int c = 0; c < channels; c++)
//					{
//						byteBuffer[0] = reader.ReadByte();
//						byteBuffer[1] = reader.ReadByte();
//						byteBuffer[2] = reader.ReadByte();
//						byteBuffer[3] = reader.ReadByte();
//
//
//						if (c==0)
//						{
//							tempColor.r=BitConverter.ToSingle (byteBuffer, 0);
//						}
//						else if(c==1)
//						{
//							tempColor.g=BitConverter.ToSingle (byteBuffer, 0);
//						}
//						else if(c==2)
//						{
//							tempColor.b=BitConverter.ToSingle (byteBuffer, 0);
//						}
//						else if(c==3)
//						{
//							tempColor.a=BitConverter.ToSingle (byteBuffer, 0);
//						}
//
//					}
//
//					tex.SetPixel(x,y, tempColor);
//				}
//			}


			
//			reader.Close ();
//			reader=null;

//			byte[] byteBuffer16 = new byte[16];
//			byte byteRead;
//			int j = 0;
//			while (j < size*4)
//			{
//				byteRead = reader.ReadByte();
//
//				j++;
//			}
			
			


//			byte[] file = System.IO.File.ReadAllBytes (path);
//			int i = 0;
//			Color tempColor = Color.black;
//			Color[] colorBuffer = new Color[file.Count() / 4];
//			int len=file.Count() / 4;
//			while (i<len)
//			{
//				byteBuffer[0]=file[i];
//				byteBuffer[1]=file[i+1];
//				byteBuffer[2]=file[i+2];
//				byteBuffer[3]=file[i+3];
//				tempColor.r=BitConverter.ToSingle (byteBuffer, 0);
//
//				byteBuffer[0]=file[i*2];
//				byteBuffer[1]=file[i*2+1];
//				byteBuffer[2]=file[i*2+2];
//				byteBuffer[3]=file[i*2+3];
//				tempColor.g=BitConverter.ToSingle (byteBuffer, 0);
//
//				byteBuffer[0]=file[i*3];
//				byteBuffer[1]=file[i*3+1];
//				byteBuffer[2]=file[i*3+2];
//				byteBuffer[3]=file[i*3+3];
//				tempColor.b=BitConverter.ToSingle (byteBuffer, 0);
//
//				byteBuffer[0]=file[i*4];
//				byteBuffer[1]=file[i*4+1];
//				byteBuffer[2]=file[i*4+2];
//				byteBuffer[3]=file[i*4+3];
//				tempColor.a=BitConverter.ToSingle (byteBuffer, 0);
//
//				colorBuffer[i]=tempColor;
//				i++;
//			}
//
//			tex.SetPixels (colorBuffer);
//			tex.Apply();

	}
}
}