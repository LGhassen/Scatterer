//Simple class that allows to display the depth buffer to abstract camera
//
//
//

using UnityEngine;
using System.Collections;

namespace scatterer
{

public class ViewDepthBuffer : MonoBehaviour
{
	[SerializeField]
	public Material m_depthBufferShader;
	
	void Start()
	{

		m_depthBufferShader = ShaderTool.GetMatFromShader2 ("CompiledViewDepthBuffer.shader");
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination) 
	{	
		
		Graphics.Blit (source, destination, m_depthBufferShader, 0);
	}
	
}
}