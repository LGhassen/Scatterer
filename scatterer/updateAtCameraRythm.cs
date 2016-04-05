//The only way to get my atmosphere to draw behind the planet was to use a regular meshrenderer because using PostRender in drawSky 
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
//		Mesh m_mesh;
		Manager m_manager;
		SkyNode m_skynode;
		Transform parentTransform;
		public Material skyMat;
		
		
		
//		public void settings(Mesh inmesh,Material inSkyMat, Manager inManager, SkyNode inSkyNode,
//		                     GameObject inSkyObject,Transform inparentTransform, Transform inCelestialTransform)

		public void settings(Material inSkyMat, Manager inManager, SkyNode inSkyNode,
		                     Transform inparentTransform)

		{
			skyMat = inSkyMat;
			m_manager = inManager;
			m_skynode = inSkyNode;
			parentTransform = inparentTransform;
//			m_mesh = inmesh;	
		}
		

		public void OnPreRender()
		{

//			var munCelestialTransform =(Transform) ScaledSpace.Instance.transform.FindChild ("Mun");
//			if (munCelestialTransform)
//			{
//				m_manager.GetCore().copiedScaledSunLight.transform.position=munCelestialTransform.position;
//			}
//			else
//			{
//				Debug.Log("muncelestial not found");
//			}
//			
//			m_manager.GetCore().copiedScaledSunLight.light.type=LightType.Point;
//			m_manager.GetCore().copiedScaledSunLight.light.range=1E9f;

			skyMat.SetMatrix ("_Sun_WorldToLocal", m_manager.GetSunWorldToLocalRotation ()); //don't touch this

//			if (!(HighLogic.LoadedScene == GameScenes.TRACKSTATION))
			{
				m_skynode.UpdateStuff ();

				m_skynode.InitUniforms (skyMat); //desn't need to be done every frame, take a look at it and clean it up later
				m_skynode.SetUniforms (skyMat);
			}

		}		
	}
}