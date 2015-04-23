//Simple class that draws the sky
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


		public void OnPostRender() 
		{
			m_skyMaterial.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
			m_skynode.SetUniforms (m_skyMaterial);
			m_skyMaterial.SetPass(0);

			Graphics.DrawMeshNow(m_mesh, position, Quaternion.identity);

		}		
	}
}