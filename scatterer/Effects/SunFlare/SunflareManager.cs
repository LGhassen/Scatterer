using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime;
using KSP;
using KSP.IO;
using UnityEngine;

namespace scatterer
{
	public class SunflareManager : MonoBehaviour
	{
		public List<string> sunflaresList=new List<string> {};
		public List<SunFlare> scattererSunFlares = new List<SunFlare>();	

		public SunflareManager ()
		{
		}

		public void Init()
		{
			Utils.DisableStockSunflares ();

			foreach (string sunflareBody in Core.Instance.planetsConfigsReader.sunflares)
			{
				SunFlare customSunFlare = (SunFlare)Core.Instance.scaledSpaceCamera.gameObject.AddComponent (typeof(SunFlare));
				try
				{
					customSunFlare.Configure(Core.Instance.CelestialBodies.SingleOrDefault (_cb => _cb.GetName () == sunflareBody),
					                         sunflareBody,Utils.GetScaledTransform (sunflareBody));
					customSunFlare.start ();
					scattererSunFlares.Add (customSunFlare);
				}
				catch (Exception exception)
				{
					Utils.LogDebug ("Custom sunflare cannot be added to " + sunflareBody + " " + exception.ToString ());
					Component.Destroy (customSunFlare);
					UnityEngine.Object.Destroy (customSunFlare);
					if (scattererSunFlares.Contains (customSunFlare))
					{
						scattererSunFlares.Remove (customSunFlare);
					}
					continue;
				}
			}
		}
		
		//TODO: decouple and let every sunflare update itself, based on the GameObject it is linked to?
		public void UpdateFlares()
		{
			foreach (SunFlare customSunFlare in scattererSunFlares)
			{
				customSunFlare.Update();
			}
		}

		public void Cleanup()
		{
			if (Core.Instance.mainSettings.fullLensFlareReplacement)
			{
				foreach (SunFlare customSunFlare in scattererSunFlares)
				{
					customSunFlare.CleanUp();
					Component.Destroy (customSunFlare);
				}
			}

			Utils.ReenableStockSunflares ();
		}
	}
}

