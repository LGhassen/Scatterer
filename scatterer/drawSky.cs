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

		public Material skyMat;
		Vector3 position;
		Mesh m_mesh;

		Camera cam;
		int layer;


		public void settings(Material inSkyMat, Vector3 inPos, Mesh inMesh, Manager inManager, SkyNode inSkyNode, Camera incam,int inlayer)
		{
			position = inPos;
			skyMat = inSkyMat;
			m_mesh = inMesh;
			m_skynode = inSkyNode;
			m_manager = inManager;
			cam = incam;
			layer = inlayer;
		}


		public void OnPostRender()
		{

			skyMat.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
			m_skynode.InitUniforms(skyMat);
			m_skynode.SetUniforms (skyMat);
			skyMat.SetPass(0);

			Graphics.DrawMeshNow(m_mesh, position, Quaternion.identity);
			//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cam);

		}		
	}
}