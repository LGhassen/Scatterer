using UnityEngine;
using System.Collections;

namespace Scatterer
{
	static public class RTUtility
	{
		static public void MultiTargetBlit(RenderTexture[] des, Material mat, int pass)
		{			
			RenderBuffer[] rb = new RenderBuffer[des.Length];
			
			for(int i = 0; i < des.Length; i++)
				rb[i] = des[i].colorBuffer;
			
			Graphics.SetRenderTarget(rb, des[0].depthBuffer);
			
			GL.PushMatrix();
			GL.LoadOrtho();
			
			mat.SetPass(pass);
			
			GL.Begin(GL.QUADS);
			GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
			GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
			GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
			GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
			GL.End();
			
			GL.PopMatrix();
		}
		
		static public void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass)
		{
			Graphics.SetRenderTarget(des_rb, des_db);
			
			GL.PushMatrix();
			GL.LoadOrtho();
			
			mat.SetPass(pass);
			
			GL.Begin(GL.QUADS);
			GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
			GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
			GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
			GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
			GL.End();
			
			GL.PopMatrix();
		}
		
		static public void Swap(RenderTexture[] texs)
		{
			RenderTexture temp = texs[0];	
			texs[0] = texs[1];
			texs[1] = temp;
		}
		
		static public void ClearColor(RenderTexture[] texs)
		{
			for(int i = 0; i < texs.Length; i++)
			{
				Graphics.SetRenderTarget(texs[i]);
				GL.Clear(false,true, Color.clear);
			}
		}
	}
}
