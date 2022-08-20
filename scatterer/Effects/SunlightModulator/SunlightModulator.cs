using UnityEngine;
using System.Collections.Generic;

namespace Scatterer
{
	public class SunlightModulatorsManager
	{
		public void ModulateByAttenuation(Light light, float inAttenuation)
		{
			FindOrCreateModulator (light).ModulateByAttenuation (inAttenuation);
		}
		
		public void ModulateByColor(Light light, Color inColor)
		{
			FindOrCreateModulator (light).ModulateByColor (inColor);
		}
		
		public Color GetLastModulateColor(Light light)
		{
			return FindOrCreateModulator (light).lastModulateColor;
		}

		public Color GetOriginalLightColor(Light light)
		{
			return FindOrCreateModulator (light).getOriginalColor();
		}
		
		private SunlightModulator FindOrCreateModulator(Light light)
		{
			if (modulatorsDictionary.ContainsKey (light))
			{
				return modulatorsDictionary [light];
			}
			else
			{
				modulatorsDictionary[light] = (SunlightModulator) Scatterer.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(SunlightModulator));
				modulatorsDictionary[light].Init(light);
				return modulatorsDictionary[light];
			}
		}
		
		private Dictionary<Light, SunlightModulator> modulatorsDictionary = new Dictionary<Light, SunlightModulator> ();

		public void Cleanup()
		{
			foreach (SunlightModulator modulator in modulatorsDictionary.Values)
			{
				Component.DestroyImmediate(modulator);
			}
		}
	}

	public class SunlightModulator : MonoBehaviour
	{
		Color originalColor = Color.white, modulateColor;
		public Color lastModulateColor;

		Light sunLight;
		bool applyModulation = false;
		bool originalColorStored = false;
		public SunlightModulatorPreRenderHook  preRenderHook;
		public SunlightModulatorPostRenderHook postRenderHook;
		
		public void Init(Light light)
		{
			sunLight = light;
			preRenderHook = (SunlightModulatorPreRenderHook) Utils.getEarliestLocalCamera().gameObject.AddComponent(typeof(SunlightModulatorPreRenderHook));
			preRenderHook.Init (this);
			postRenderHook = (SunlightModulatorPostRenderHook) Scatterer.Instance.nearCamera.gameObject.AddComponent(typeof(SunlightModulatorPostRenderHook)); //less than optimal, doesn't affect internalCamera
			postRenderHook.Init (this);	
		}

		public void OnPreCull() //added to scaledSpaceCamera, called before any calls from skyNode or oceanNode
		{
			storeOriginalColor ();
		}

		private void storeOriginalColor() //may not be necessary every frame?
		{
			if (sunLight.color != Color.black)
			{
				originalColor = sunLight.color;
				originalColorStored = true;
			}
		}

		public Color getOriginalColor()
		{
			return originalColor;
		}

		public void ModulateByAttenuation(float inAttenuation) //called by skynode, ie scaledSpaceCamera onPreCull
		{
			modulateColor *= inAttenuation;
			applyModulation = true;
		}

		public void ModulateByColor(Color inColor)
		{
			modulateColor *= inColor;
			applyModulation = true;
		}

		public void applyColorModulation()  //called by hook on farCamera onPreRender
		{
			if (applyModulation && originalColorStored)
			{
				sunLight.color = modulateColor * originalColor;
				lastModulateColor = sunLight.color;
				modulateColor = Color.white;
			}
		}
		
		public void restoreOriginalColor()  //called by hook on nearCamera/IVAcamera onPostRender  //may not be necessary every frame?
		{
			if (applyModulation && originalColorStored)
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

