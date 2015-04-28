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
	
	public class OceanUpdateAtCameraRythm : MonoBehaviour
	{
		public OceanNode m_oceanNode;
			


//		public void settings(Material inSkyMat, Manager inManager, SkyNode inSkyNode, GameObject intester, bool indebug6, CelestialBody inparent)
//		{
//
//			skyMat = inSkyMat;
//			m_manager = inManager;
//			m_skynode = inSkyNode;
//			tester = intester;
//			debug6 = indebug6;
//			parentCelestialBody = inparent;
//
//		}

	
		
	public void OnPreRender()
		{
//			if (m_oceanNode != null) {
//				m_oceanNode.updateStuff ();
			m_oceanNode.updateStuff ();
//				print("UPDATED OCEAN NODE STUFF");
//			}
//
//			else print("OCEAN NODE NULL");


		}		
	}
}