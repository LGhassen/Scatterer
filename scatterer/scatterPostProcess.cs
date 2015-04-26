using UnityEngine;
using System.Collections;

namespace scatterer
{

public class scatterPostprocess : MonoBehaviour
{
//		int nearPlane=299;
//		int farPlane=750000;
	
	public Material m_atmosphereImageEffect;
	
	
	public void setMaterial(Material mat)
	{
		m_atmosphereImageEffect = mat;					
	}

//	public void setFarPlane(int far)
//	{
//		farPlane = far;					
//	}
//
//	public void setNearPlane(int near)
//	{
//		nearPlane = near;					
//	}

		
	void Start()
	{
		
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
	}
	
	void OnRenderImage(RenderTexture source, RenderTexture destination) 
	{	
		
		//Graphics.Blit(source, destination);
		//return;
		
		//This will apply the atmospheric scattering to all objects in the scenne that have written to the depth buffer
		//The world pos is reconstructed from the depth values. To do the some information about the frustum must be passed
		//in to the shader. The code below calculates the position of the frustum corners
		//This method has been adapted from the global fog image effect
		
		float CAMERA_NEAR = GetComponent<Camera>().nearClipPlane;
//			float CAMERA_NEAR = nearPlane;
//			print ("NEAR CLIP PLANE");
//			print(CAMERA_NEAR);

		float CAMERA_FAR = GetComponent<Camera>().farClipPlane;
//			float CAMERA_FAR = farPlane;

//			print ("NEAR CLIP PLANE");
//			print(CAMERA_FAR);



		float CAMERA_FOV = GetComponent<Camera>().fieldOfView;
		float CAMERA_ASPECT_RATIO = GetComponent<Camera>().aspect;
		
		Matrix4x4 frustumCorners = Matrix4x4.identity;		
		
		float fovWHalf = CAMERA_FOV * 0.5f;
		
		Vector3 toRight = GetComponent<Camera>().transform.right * CAMERA_NEAR * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * CAMERA_ASPECT_RATIO;
		Vector3 toTop = GetComponent<Camera>().transform.up * CAMERA_NEAR * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);
		
		Vector3 topLeft = (GetComponent<Camera>().transform.forward * CAMERA_NEAR - toRight + toTop);
		float CAMERA_SCALE = topLeft.magnitude * CAMERA_FAR/CAMERA_NEAR;	

//			print ("CAMERA SCALE=");
//			print (CAMERA_SCALE);
		
		topLeft.Normalize();
		topLeft *= CAMERA_SCALE;
		
		Vector3 topRight = (GetComponent<Camera>().transform.forward * CAMERA_NEAR + toRight + toTop);
		topRight.Normalize();
		topRight *= CAMERA_SCALE;
		
		Vector3 bottomRight = (GetComponent<Camera>().transform.forward * CAMERA_NEAR + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= CAMERA_SCALE;
		
		Vector3 bottomLeft = (GetComponent<Camera>().transform.forward * CAMERA_NEAR - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= CAMERA_SCALE;

		frustumCorners.SetRow (0, topLeft); 
		frustumCorners.SetRow (1, topRight);		
		frustumCorners.SetRow (2, bottomRight);
		frustumCorners.SetRow (3, bottomLeft);	

		m_atmosphereImageEffect.SetMatrix ("_FrustumCorners", frustumCorners);
		
		//CustomGraphicsBlit(source, destination, m_atmosphereImageEffect, 0);
		CustomGraphicsBlit(source, destination, m_atmosphereImageEffect, 0);
	}
	
	static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial, int passNr) 
	{
		RenderTexture.active = dest;
		
		fxMaterial.SetTexture ("_MainTex", source);	        
		
		GL.PushMatrix ();
		GL.LoadOrtho ();
		
		fxMaterial.SetPass (passNr);	
		
		GL.Begin (GL.QUADS);
		
		//This custom blit is needed as infomation about what corner verts relate to what frustum corners is needed
		//A index to the frustum corner is store in the z pos of vert
		
		GL.MultiTexCoord2 (0, 0.0f, 0.0f); 
		GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
		
		GL.MultiTexCoord2 (0, 1.0f, 0.0f); 
		GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
		
		GL.MultiTexCoord2 (0, 1.0f, 1.0f); 
		GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
		
		GL.MultiTexCoord2 (0, 0.0f, 1.0f); 
		GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
		
		GL.End ();
		GL.PopMatrix ();
		
	}	
}
}
