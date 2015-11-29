
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
		/*public static Shader GetShader(String resource)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			StreamReader shaderStreamReader = new StreamReader(assembly.GetManifestResourceStream(resource));
			String shaderTxt = shaderStreamReader.ReadToEnd();
			return new Material(shaderTxt).shader;
		}
		
		
		public static Material GetMatFromShader( String resource)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			StreamReader shaderStreamReader = new StreamReader(assembly.GetManifestResourceStream(resource));
			String shaderTxt = shaderStreamReader.ReadToEnd();
			return new Material(shaderTxt);
		}*/
		
		public static Material GetMatFromShader(String resource)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			
			StreamReader shaderStream = new StreamReader(new FileStream(Path.GetDirectoryName(path) + "/" + resource, FileMode.Open, FileAccess.Read));
			string shaderContent = shaderStream.ReadToEnd();

			Material Mat1= new Material(shaderContent);
			return Mat1;
		}

		public static Material GetMatFromShader2(String resource)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			
			StreamReader shaderStream = new StreamReader(new FileStream(Path.GetDirectoryName(path) + "/" + resource, FileMode.Open, FileAccess.Read));
			string shaderContent = shaderStream.ReadToEnd();
			Material Mat2= new Material(shaderContent);
			return Mat2;
		}

		public static Shader GetShader2(String resource)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string path = Uri.UnescapeDataString(uri.Path);
			
			StreamReader shaderStream = new StreamReader(new FileStream(Path.GetDirectoryName(path) + "/" + resource, FileMode.Open, FileAccess.Read));
			string shaderContent = shaderStream.ReadToEnd();
			Material Mat2= new Material(shaderContent);
			return Mat2.shader;
		}

		
	}
	
	
	
}

