//This updates the meshrenderer settings before the camera renders the scene

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
		Transform parentTransform;
		public Material skyMat;

		public void settings(Material inSkyMat, Manager inManager, SkyNode inSkyNode,Transform inparentTransform)
		{
			skyMat = inSkyMat;
			m_manager = inManager;
			m_skynode = inSkyNode;
			parentTransform = inparentTransform;
		}

//		public void OnPreRender()
		public void OnPreCull()
		{	
			m_skynode.UpdateStuff ();			
			m_skynode.SetUniforms (skyMat);
		}
	}
}