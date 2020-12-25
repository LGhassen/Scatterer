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
		
		MeshRenderer shadowMR;
		
		public void Init()
		{
			shadowMR = gameObject.GetComponent<MeshRenderer>();
			material.SetOverrideTag("IgnoreProjector", "True");
			shadowMR.sharedMaterial = material;
			
			shadowMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			shadowMR.receiveShadows = false;
			shadowMR.enabled = true;

			GetComponent<MeshFilter>().mesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));
			
			gameObject.layer = (int) 15;
		}
		
		public void SetActive(bool active)
		{
			shadowMR.enabled = active;
		}
		
		void OnWillRenderObject()
		{
			if (material != null)
			{
				material.SetMatrix(ShaderProperties.CameraToWorld_PROPERTY, Camera.current.cameraToWorldMatrix);

				//set directions of frustum corners in world space
				//used to reconstruct world pos from view-space depth
//
//				Vector3 topLeft 	= Camera.current.ViewportToWorldPoint(new Vector3(0f,1f,Camera.current.farClipPlane)) - Camera.current.transform.position;
//				Vector3 topRight	= Camera.current.ViewportToWorldPoint(new Vector3(1f,1f,Camera.current.farClipPlane)) - Camera.current.transform.position;
//				Vector3 bottomRight = Camera.current.ViewportToWorldPoint(new Vector3(1f,0f,Camera.current.farClipPlane)) - Camera.current.transform.position;
//				Vector3 bottomLeft 	= Camera.current.ViewportToWorldPoint(new Vector3(0f,0f,Camera.current.farClipPlane)) - Camera.current.transform.position;
//				
//				Matrix4x4 _frustumCorners = Matrix4x4.identity;				
//				{
//					_frustumCorners.SetRow (0, bottomLeft); 
//					_frustumCorners.SetRow (1, bottomRight);		
//					_frustumCorners.SetRow (2, topLeft);
//					_frustumCorners.SetRow (3, topRight);	
//				}
//				
//				material.SetMatrix ("scattererFrustumCorners", _frustumCorners);
			}
		}
	}

	public class ScreenSpaceScatteringContainer : AbstractLocalAtmosphereContainer
	{
		ScreenSpaceScattering screenSpaceScattering;

		public ScreenSpaceScatteringContainer (Material atmosphereMaterial, Transform parentTransform, float Rt) : base (atmosphereMaterial, parentTransform, Rt)
		{
			scatteringGO = GameObject.CreatePrimitive(PrimitiveType.Quad);
			scatteringGO.name = "Scatterer screenspace scattering " + atmosphereMaterial.name;
			GameObject.Destroy (scatteringGO.GetComponent<Collider> ());
			scatteringGO.transform.localScale = Vector3.one;

			screenSpaceScattering = scatteringGO.AddComponent<ScreenSpaceScattering>();

			//or just parent to the near camera
			scatteringGO.transform.position = parentTransform.position;
			scatteringGO.transform.parent   = parentTransform;
			
			screenSpaceScattering.material = atmosphereMaterial;
			screenSpaceScattering.material.CopyKeywordsFrom (atmosphereMaterial);

			screenSpaceScattering.Init();
		}

		public override void updateContainer ()
		{
			bool isEnabled = !underwater && !inScaledSpace && activated;
			screenSpaceScattering.enabled = isEnabled;
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
