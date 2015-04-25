//The only way to get my atmosphere to draw behind the planet was to sue a regular meshrenderer because using PostRender in drawSky 
//causes it to be drawn after the whole scene is done
//

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
	
	public class updateAtCameraRythm : MonoBehaviour
	{
		Manager m_manager;
		SkyNode m_skynode;
		GameObject tester;

		CelestialBody parentCelestialBody;

		bool debug6;

		public Material skyMat;



		public void settings(Material inSkyMat, Manager inManager, SkyNode inSkyNode, GameObject intester, bool indebug6, CelestialBody inparent)
		{

			skyMat = inSkyMat;
			m_manager = inManager;
			m_skynode = inSkyNode;
			tester = intester;
			debug6 = indebug6;
			parentCelestialBody = inparent;

		}


		public void OnPreRender()
		{

			skyMat.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this

//			if (debug6){
				tester.transform.parent = parentCelestialBody.transform;
//			}
//			
//			else{
//				Transform celestialTransform = ScaledSpace.Instance.scaledSpaceTransforms.Single(t => t.name == parentCelestialBody.name);
//				tester.transform.parent = celestialTransform;
//			}


			m_skynode.InitUniforms(skyMat);
			m_skynode.SetUniforms (skyMat);
//			skyMat.SetPass(0);
//
//			Graphics.DrawMeshNow(m_mesh, position, Quaternion.identity);
//			//			Graphics.DrawMesh(m_mesh, position, Quaternion.identity,m_skyMaterial,layer,cam);

		}		
	}
}