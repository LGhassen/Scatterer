using UnityEngine;
using System.Collections;

namespace scatterer
{
	static public class RTUtility
	{
		
		//	static public void Blit(RenderTexture src, RenderTexture des, Material mat, int pass = 0, bool clear = true)
		static public void Blit(RenderTexture src, RenderTexture des, Material mat, int pass, bool clear)
		{
			mat.SetTexture(ShaderProperties._MainTex_PROPERTY, src);
			
			RenderTexture oldRT = RenderTexture.active;
			
			Graphics.SetRenderTarget(des);
			
			if(clear) GL.Clear(true, true, Color.clear);
			
			GL.PushMatrix();
			GL.LoadOrtho();
			
			mat.SetPass(pass);
			
			GL.Begin(GL.QUADS);
			GL.TexCoord3(0.0f, 0.0f, 0.0f); GL.Vertex3(0.0f, 0.0f, 0.1f);
			GL.TexCoord3(1.0f, 0.0f, 0.0f); GL.Vertex3(1.0f, 0.0f, 0.1f);
			GL.TexCoord3(1.0f, 1.0f, 0.0f); GL.Vertex3(1.0f, 1.0f, 0.1f);
			GL.TexCoord3(0.0f, 1.0f, 0.0f); GL.Vertex3(0.0f, 1.0f, 0.1f);
			GL.End();
			
			GL.PopMatrix();
			
			RenderTexture.active = oldRT;
		}
		
		//	static public void Blit(RenderTexture src, RenderTexture des, Material mat, Rect rect, int pass = 0, bool clear = true)
		static public void Blit(RenderTexture src, RenderTexture des, Material mat, Rect rect, int pass, bool clear)
		{
			//rect must have normalized coords, ie 0 - 1
			
			mat.SetTexture(ShaderProperties._MainTex_PROPERTY, src);
			
			RenderTexture oldRT = RenderTexture.active;
			
			Graphics.SetRenderTarget(des);
			
			if(clear) GL.Clear(true, true, Color.clear);
			
			GL.PushMatrix();
			GL.LoadOrtho();
			
			mat.SetPass(pass);
			
			GL.Begin(GL.QUADS);
			GL.TexCoord2(rect.x, rect.y); GL.Vertex3(rect.x, rect.y, 0.1f);
			GL.TexCoord2(rect.x+rect.width, rect.y); GL.Vertex3(rect.x+rect.width, rect.y, 0.1f);
			GL.TexCoord2(rect.x+rect.width, rect.y+rect.height); GL.Vertex3(rect.x+rect.width, rect.y+rect.height, 0.1f);
			GL.TexCoord2(rect.x, rect.y+rect.height); GL.Vertex3(rect.x, rect.y+rect.height, 0.1f);
			GL.End();
			
			GL.PopMatrix();
			
			RenderTexture.active = oldRT;
		}
		
		//	static public void MultiTargetBlit(RenderTexture[] des, Material mat, int pass = 0)
		static public void MultiTargetBlit(RenderTexture[] des, Material mat, int pass)
		{
			//RenderTexture oldRT = RenderTexture.active;
			
			RenderBuffer[] rb = new RenderBuffer[des.Length];
			
			for(int i = 0; i < des.Length; i++)
				rb[i] = des[i].colorBuffer;
			
			Graphics.SetRenderTarget(rb, des[0].depthBuffer);
			
			GL.Clear(true, true, Color.clear);
			
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
			
			//RenderTexture.active = oldRT;
		}
		
		//	static public void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass = 0)
		static public void MultiTargetBlit(RenderBuffer[] des_rb, RenderBuffer des_db, Material mat, int pass)
		{
			//RenderTexture oldRT = RenderTexture.active;
			
			Graphics.SetRenderTarget(des_rb, des_db);
			
			GL.Clear(true, true, Color.clear);
			
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
			
			//RenderTexture.active = oldRT;
		}
		
		static public void Swap(RenderTexture[] texs)
		{
			RenderTexture temp = texs[0];	
			texs[0] = texs[1];
			texs[1] = temp;
		}
		
		static public void Swap(ref RenderTexture tex0, ref RenderTexture tex1)
		{
			RenderTexture temp = tex0;	
			tex0 = tex1;
			tex1 = temp;
		}
		
		static public void ClearColor(RenderTexture tex)
		{
			Graphics.SetRenderTarget(tex);
			GL.Clear(false,true, Color.clear);
		}
		
		static public void ClearColor(RenderTexture[] texs)
		{
			for(int i = 0; i < texs.Length; i++)
			{
				Graphics.SetRenderTarget(texs[i]);
				GL.Clear(false,true, Color.clear);
			}
		}
		
		static public void SetToPoint(RenderTexture[] texs)
		{
			for(int i = 0; i < texs.Length; i++)
			{
				texs[i].filterMode = FilterMode.Point;
			}
		}
		
		static public void SetToBilinear(RenderTexture[] texs)
		{
			for(int i = 0; i < texs.Length; i++)
			{
				texs[i].filterMode = FilterMode.Bilinear;
			}
		}
		
		static public void Destroy(RenderTexture[] texs)
		{
			if(texs == null) return;
			
			for(int i = 0; i < texs.Length; i++)
				RenderTexture.DestroyImmediate(texs[i]);
		}
	}
}
