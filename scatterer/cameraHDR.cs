
using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer
{
	public class cameraHDR: MonoBehaviour
	{	
		public Material toneMappingMaterial;
		SkyNode m_skynode;
		float HDR=0.25f;
		
		void Start()
		{	
			toneMappingMaterial= ShaderTool.GetMatFromShader2 ("CompiledToneMapper.shader");
			//		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
			GetComponent<Camera> ().hdr = true;
		}
		
		public void settings(SkyNode inSkyNode)
		{
			m_skynode = inSkyNode;
			HDR = m_skynode.farCameraHDR;
			
		}
		
		
		//		public void OnRender()
		//public void OnPreRender()
		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			//insert bloom here
			//toneMappingMaterial.SetFloat("_ExposureAdjustment", m_skynode.m_HDRExposure);
			toneMappingMaterial.SetFloat("_ExposureAdjustment", HDR);
			print ("HDR in farcamera cameraHDRscript");
			print (HDR);
			Graphics.Blit(source, destination, toneMappingMaterial, 8); //tonemapping, 8 is choosing the photographic preset/pass
		}		
	}
}