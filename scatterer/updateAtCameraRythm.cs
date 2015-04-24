//The only way to get my atmosphere to draw behind the planet was to sue a regular meshrenderer because using PostRender in drawSky 
//causes it to be drawn after the whole scene is done
//

using UnityEngine;
using System.Collections;

namespace scatterer
{
	
	public class updateAtCameraRythm : MonoBehaviour
	{
		Manager m_manager;
		SkyNode m_skynode;

		public Material skyMat;



		public void settings(Material inSkyMat, Manager inManager, SkyNode inSkyNode)
		{

			skyMat = inSkyMat;
			m_manager = inManager;
			m_skynode = inSkyNode;

		}


		public void OnPreRender()
		{

			skyMat.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this
			m_skynode.InitUniforms(skyMat);
			m_skynode.SetUniforms (skyMat);
//			skyMat.SetPass(0);
//
//			Graphics.DrawMeshNow(m_mesh, position, Quaternion.identity);
//			//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cam);

		}		
	}
}