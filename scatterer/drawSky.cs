//Simple class that allows to display the depth buffer to abstract camera
//
//
//

using UnityEngine;
using System.Collections;

namespace scatterer
{
	
	public class drawSky : MonoBehaviour
	{
		Manager m_manager;
		SkyNode m_skynode;

		public Material m_skyMaterial;
		Vector3 position;
		Mesh m_mesh;

		Camera cam;
		int layer;


		public void settings(Material inSkyMat, Vector3 inPos, Mesh inMesh, Manager inManager, SkyNode inSkyNode, Camera incam,int inlayer)
		{
			position = inPos;
			m_skyMaterial = inSkyMat;
			m_mesh = inMesh;
			m_skynode = inSkyNode;
			m_manager = inManager;
			cam = incam;
			layer = inlayer;
		}
		
		void Start()
		{

		}

//		public void OnRenderimage()
//		{
//			m_manager.SetUniforms (m_skyMaterial);
//			m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
//			m_skynode.SetUniforms (m_skyMaterial);
//			
//			//Mesh m_mesh = new Mesh();
//			
//			
////			Mesh m_mesh;
////			
////			//			if (m_mesh == null) 
////			m_mesh = isoSphere.Create ();
////			
////			m_mesh.bounds = new Bounds (position, new Vector3 (1e8f, 1e8f, 1e8f));
//			
//			
//			Graphics.DrawMesh (m_mesh, position, new Quaternion (0, 1, 0, 0), m_skyMaterial,layer, cam);
//
//
//		}

		public void OnPostRender() {
			// set first shader pass of the material

			m_manager.SetUniforms (m_skyMaterial);
			m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
			m_skynode.SetUniforms (m_skyMaterial);

			m_skyMaterial.SetPass(0);





			Graphics.DrawMeshNow(m_mesh, position, Quaternion.identity);

			print ("DRAWMESHNOW CALLED");
		}
//		
////		void OnRenderImage(RenderTexture source, RenderTexture destination) 
////		{	
////			
////			Graphics.Blit (source, destination, m_depthBufferShader, 0);
////		}
		
	}
}