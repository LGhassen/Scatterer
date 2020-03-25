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
	public class SunlightModulator : MonoBehaviour
	{
		Color originalColor;
		Color modulateColor;
		Light sunLight;
		bool applyModulation = false;
		SunlightModulatorPreRenderHook  preRenderHook;
		SunlightModulatorPostRenderHook postRenderHook;
		
		private void Awake()
		{
			sunLight = Scatterer.Instance.sunLight.GetComponent < Light > ();
			preRenderHook = (SunlightModulatorPreRenderHook)Scatterer.Instance.ReturnProperCamera(true, false).gameObject.AddComponent(typeof(SunlightModulatorPreRenderHook));
			postRenderHook = (SunlightModulatorPostRenderHook) Scatterer.Instance.ReturnProperCamera(false, false).gameObject.AddComponent(typeof(SunlightModulatorPostRenderHook)); //less than optimal, doesn't affect internalCamera
																																						  //but also the issue is that internalCamera
		}

		public void OnPreCull() //added to scaledSpaceCamera, called before any calls from skyNode or oceanNode
		{
			storeOriginalColor ();
		}

		public void storeOriginalColor()    //may not be necessary every frame?
		{
			originalColor = sunLight.color;
			//Utils.Log ("store original color " + originalColor.ToString ());
		}

		public void modulateByAttenuation(float inAttenuation) //called by skynode, ie scaledSpaceCamera onPreCull
		{
			modulateColor *= inAttenuation;
			applyModulation = true;
		}

		public void modulateByColor(Color inColor)
		{
			modulateColor *= inColor;
			applyModulation = true;
		}

		public void applyColorModulation()  //called by hook on farCamera onPreRender
		{
			if (applyModulation)
			{
				sunLight.color = modulateColor * originalColor;
				modulateColor = Color.white;
				if (Scatterer.Instance.mainSettings.integrateWithEVEClouds)	//preserve original directional light color to not have double extinction
					Shader.SetGlobalColor("scattererOrigDirectionalColor",originalColor);
			}
		}
		
		public void restoreOriginalColor()  //called by hook on unifiedCamera/IVAcamera onPostRender  //may not be necessary every frame?
		{
			if (applyModulation)
			{
				sunLight.color = originalColor;
				applyModulation = false;
			}
		}

		public void OnDestroy()
		{
			Component.Destroy (preRenderHook);
			Component.Destroy (postRenderHook);
		}
	}
}

