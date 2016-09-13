using UnityEngine;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using KSP.IO;

namespace scatterer {
	
	public class OceanUpdateAtCameraRythm: MonoBehaviour {
		public OceanNode m_oceanNode;
		public Material oceanMaterialFar;
		public Camera farCamera;
		public Camera nearCamera;
		public Manager m_manager=null;
		
		//public void OnPreRender() {
		public void OnPreCull(){
			if (!m_manager)
				Destroy (this);

			if (!MapView.MapIsEnabled && farCamera && nearCamera && !m_manager.m_skyNode.inScaledSpace && m_oceanNode.GetDrawOcean() ) {
				m_oceanNode.updateStuff(oceanMaterialFar, farCamera);
			}
		}
	}
}