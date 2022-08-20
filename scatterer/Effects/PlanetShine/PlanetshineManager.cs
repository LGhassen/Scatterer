using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scatterer
{
	public class PlanetshineManager
	{
		List<PlanetShineLight> celestialLightSources=new List<PlanetShineLight> {};
		Cubemap planetShineCookieCubeMap;

		public PlanetshineManager ()
		{
			//load planetshine "cookie" cubemap
			planetShineCookieCubeMap = new Cubemap (512, TextureFormat.ARGB32, true);
			Texture2D[] cubeMapFaces = new Texture2D[6];
			for (int i = 0; i < 6; i++) {
				cubeMapFaces [i] = new Texture2D (512, 512);
			}
			cubeMapFaces [0].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_NegativeX.png")));
			cubeMapFaces [1].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_PositiveX.png")));
			cubeMapFaces [2].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_NegativeY.png")));
			cubeMapFaces [3].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_PositiveY.png")));
			cubeMapFaces [4].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_NegativeZ.png")));
			cubeMapFaces [5].LoadImage (System.IO.File.ReadAllBytes (String.Format ("{0}/{1}", Utils.PluginPath + "/planetShineCubemap", "_PositiveZ.png")));
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [0].GetPixels (), CubemapFace.NegativeX);
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [1].GetPixels (), CubemapFace.PositiveX);
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [2].GetPixels (), CubemapFace.NegativeY);
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [3].GetPixels (), CubemapFace.PositiveY);
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [4].GetPixels (), CubemapFace.NegativeZ);
			planetShineCookieCubeMap.SetPixels (cubeMapFaces [5].GetPixels (), CubemapFace.PositiveZ);
			planetShineCookieCubeMap.Apply ();
			
			
			foreach (PlanetShineLightSource _aSource in Scatterer.Instance.planetsConfigsReader.celestialLightSourcesData)
			{
				var celBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.bodyName == _aSource.bodyName);
				if (celBody)
				{
					PlanetShineLight aPsLight = new PlanetShineLight ();
					aPsLight.isSun = _aSource.isSun;
					aPsLight.source = celBody;
					if (!_aSource.isSun)
						aPsLight.sunCelestialBody = FlightGlobals.Bodies.SingleOrDefault (_cb => _cb.GetName () == _aSource.mainSunCelestialBody);
					// GameObject ScaledPlanetShineLight = (UnityEngine.GameObject)Instantiate (Scatterer.Instance.scaledSpaceSunLight.gameObject);
					// GameObject LocalPlanetShineLight = (UnityEngine.GameObject)Instantiate (Scatterer.Instance.scaledSpaceSunLight.gameObject);

					//TODO: fix this if I ever come back to it
					GameObject ScaledPlanetShineLight = new GameObject();
					GameObject LocalPlanetShineLight = new GameObject();
					ScaledPlanetShineLight.GetComponent<Light> ().type = LightType.Point;
					if (!_aSource.isSun)
						ScaledPlanetShineLight.GetComponent<Light> ().cookie = planetShineCookieCubeMap;
					//ScaledPlanetShineLight.GetComponent<Light>().range=1E9f;
					ScaledPlanetShineLight.GetComponent<Light> ().range = _aSource.scaledRange;
					ScaledPlanetShineLight.GetComponent<Light> ().color = new Color (_aSource.color.x, _aSource.color.y, _aSource.color.z);
					ScaledPlanetShineLight.name = celBody.name + "PlanetShineLight(ScaledSpace)";
					LocalPlanetShineLight.GetComponent<Light> ().type = LightType.Point;
					if (!_aSource.isSun)
						LocalPlanetShineLight.GetComponent<Light> ().cookie = planetShineCookieCubeMap;
					//LocalPlanetShineLight.GetComponent<Light>().range=1E9f;
					LocalPlanetShineLight.GetComponent<Light> ().range = _aSource.scaledRange * ScaledSpace.ScaleFactor;
					LocalPlanetShineLight.GetComponent<Light> ().color = new Color (_aSource.color.x, _aSource.color.y, _aSource.color.z);
					LocalPlanetShineLight.GetComponent<Light> ().cullingMask = 557591;
					LocalPlanetShineLight.GetComponent<Light> ().shadows = LightShadows.Soft;
					LocalPlanetShineLight.GetComponent<Light> ().shadowCustomResolution = 2048;
					LocalPlanetShineLight.name = celBody.name + "PlanetShineLight(LocalSpace)";
					aPsLight.scaledLight = ScaledPlanetShineLight;
					aPsLight.localLight = LocalPlanetShineLight;
					celestialLightSources.Add (aPsLight);
					Utils.LogDebug ("Added celestialLightSource " + aPsLight.source.name);
				}
			}
		}

		public void UpdatePlanetshine ()
		{
			foreach (PlanetShineLight _aLight in celestialLightSources)
			{
				_aLight.updateLight();
				
			}
		}

		public void Cleanup()
		{
			foreach (PlanetShineLight _aLight in celestialLightSources)
			{
				_aLight.OnDestroy();
				UnityEngine.Object.Destroy(_aLight);
			}
		}
	}
}

