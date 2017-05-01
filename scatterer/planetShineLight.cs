using System;
using UnityEngine;
namespace scatterer
{
	public class PlanetShineLight : MonoBehaviour
	{
		public GameObject scaledLight, localLight;
		public bool isSun;
		public CelestialBody source, sunCelestialBody;

		public void updateLight()
		{
			scaledLight.gameObject.transform.position=ScaledSpace.LocalToScaledSpace(source.transform.position);						
			localLight.gameObject.transform.position=(source.transform.position);

			if (!isSun)
			{
				localLight.gameObject.transform.LookAt(sunCelestialBody.transform.position);
				scaledLight.gameObject.transform.LookAt (ScaledSpace.LocalToScaledSpace (sunCelestialBody.transform.position));
			}
		}

		public void OnDestroy()
		{
			UnityEngine.Object.Destroy (scaledLight);
			UnityEngine.Object.Destroy (localLight);
		}
	}
}

