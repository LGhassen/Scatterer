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
	public class ScreenSpaceScattering : MonoBehaviour
	{
		public Material material;
		
		MeshRenderer scatteringMR;
		
		public void Init()
		{
			scatteringMR = gameObject.GetComponent<MeshRenderer>();
			material.SetOverrideTag("IgnoreProjector", "True");
			scatteringMR.sharedMaterial = material;
			
			scatteringMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			scatteringMR.receiveShadows = false;
			scatteringMR.enabled = true;

			GetComponent<MeshFilter>().mesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));
			
			gameObject.layer = (int) 15;
		}
		
		public void SetActive(bool active)
		{
			scatteringMR.enabled = active;
		}

		void OnWillRenderObject()
		{
			if (material != null)
			{
				Camera cam = Camera.current;
				
				if (!cam)
					return;

				material.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, cam.cameraToWorldMatrix);

				ScreenCopyCommandBuffer.EnableScatteringScreenAndDepthCopyForFrame(cam); //this needs to be modified to do the screen copy if there is no ocean, but that's easy I guess
			}
		}
	}

	public class ScreenSpaceScatteringContainer : AbstractLocalAtmosphereContainer
	{
		ScreenSpaceScattering screenSpaceScattering;

		public ScreenSpaceScatteringContainer (Material atmosphereMaterial, Transform parentTransform, float Rt, ProlandManager parentManager) : base (atmosphereMaterial, parentTransform, Rt, parentManager)
		{
			scatteringGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
			scatteringGO.name = "Scatterer screenspace scattering " + atmosphereMaterial.name;
			GameObject.Destroy (scatteringGO.GetComponent<Collider> ());
			scatteringGO.transform.localScale = Vector3.one;

			//for now just disable this from reflection probe because no idea how to add the effect on it, no access to depth buffer and I don't feel like the perf hit would be worth to enable it
			//this will be handled by the ocean if it is present
			if (!manager.hasOcean || !Scatterer.Instance.mainSettings.useOceanShaders)
			{
				DisableEffectsChecker disableEffectsChecker = scatteringGO.AddComponent<DisableEffectsChecker> ();
				disableEffectsChecker.manager = this.manager;
			}

			screenSpaceScattering = scatteringGO.AddComponent<ScreenSpaceScattering>();

			scatteringGO.transform.position = parentTransform.position;
			scatteringGO.transform.parent   = parentTransform;
			
			screenSpaceScattering.material = atmosphereMaterial;
			screenSpaceScattering.material.CopyKeywordsFrom (atmosphereMaterial);

			screenSpaceScattering.Init();
		}

		public override void updateContainer ()
		{
			bool isEnabled = !underwater && !inScaledSpace && activated;
			screenSpaceScattering.SetActive(isEnabled);
			scatteringGO.SetActive(isEnabled);
		}

		~ScreenSpaceScatteringContainer()
		{
			setActivated (false);
			if(!ReferenceEquals(scatteringGO,null))
			{
				if(!ReferenceEquals(scatteringGO.transform,null))
				{
					if(!ReferenceEquals(scatteringGO.transform.parent,null))
					{
						scatteringGO.transform.parent = null;
					}
				}
				
				Component.Destroy(screenSpaceScattering);
				GameObject.DestroyImmediate(scatteringGO);
				screenSpaceScattering = null;
				scatteringGO = null;
			}
		}
	}
}
