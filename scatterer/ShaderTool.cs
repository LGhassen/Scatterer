
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class ShaderTool
	{
		
		public static Material GetMatFromShader2(String resource)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);

			string shaderPath = "/shaders/";
			if (Core.Instance.loadAlternative_D3D11_OGL_shaders)
			{
				if (Core.Instance.d3d11)
				{
					shaderPath = "/shaders/d3d11/";
				}
			}

			StreamReader shaderStream = new StreamReader(new FileStream(Path.GetDirectoryName(path) + shaderPath + resource, FileMode.Open, FileAccess.Read));
			string shaderContent = shaderStream.ReadToEnd();
			Material Mat2= new Material(shaderContent);
			return Mat2;
//			return null;
		}

		public static Shader GetShader2(String resource)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);

			string shaderPath = "/shaders/";
			if (Core.Instance.loadAlternative_D3D11_OGL_shaders)
			{
				if (Core.Instance.d3d11)
				{
					shaderPath = "/shaders/d3d11/";
				}
			}

			StreamReader shaderStream = new StreamReader(new FileStream(Path.GetDirectoryName(path) + shaderPath + resource, FileMode.Open, FileAccess.Read));
			string shaderContent = shaderStream.ReadToEnd();
			Material Mat2= new Material(shaderContent);
			return Mat2.shader;
		}

		
	}
	
	
	
}

