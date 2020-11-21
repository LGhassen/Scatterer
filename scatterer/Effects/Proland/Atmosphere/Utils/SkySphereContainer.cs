using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class SkySphereContainer : MonoBehaviour
	{
		GameObject skySphereGO;
		MeshRenderer skySphereMR;

		public GameObject GameObject { get { return skySphereGO; } }
		public MeshRenderer MeshRenderer { get { return skySphereMR; } }

		Transform parentLocalTransform, parentScaledTransform;
		
		public SkySphereContainer(float size, Material material, Transform inParentLocalTransform, Transform inParentScaledTransform)
		{
			skySphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			GameObject.Destroy (skySphereGO.GetComponent<Collider> ());

			skySphereGO.transform.localScale = Vector3.one;
			
			MeshFilter skySphereMF = skySphereGO.GetComponent<MeshFilter>();
			Vector3[] verts = skySphereMF.mesh.vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] *= size;
			}
			skySphereMF.mesh.vertices = verts;
			skySphereMF.mesh.RecalculateBounds();
			skySphereMF.mesh.RecalculateNormals();
			
			skySphereMR = skySphereGO.GetComponent<MeshRenderer>();
			skySphereMR.sharedMaterial = material;
			Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);

			skySphereMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			skySphereMR.receiveShadows = false;
			skySphereMR.enabled = true;

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				skySphereGO.layer = 15;
			else
				skySphereGO.layer = 9;

			//I think this won't be needed anymore, but test with kerbal konstructs to make sure tsunami bug doesn't come back
//			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
//			{
//				SkySphereKSCUpdater updater = (SkySphereKSCUpdater)skySphereGO.AddComponent (typeof(SkySphereKSCUpdater));
//				updater.parentLocalTransform = inParentLocalTransform;
//			}
//			else
			{
				skySphereGO.transform.position = inParentScaledTransform.position;
				skySphereGO.transform.parent = inParentScaledTransform;
			}

			parentScaledTransform = inParentScaledTransform;
			parentLocalTransform = inParentLocalTransform;
		}
		
		public void SwitchLocalMode()
		{
			skySphereGO.layer = 15;

			skySphereGO.transform.parent = null;

			skySphereGO.transform.position = parentLocalTransform.position;
			skySphereGO.transform.localScale = new Vector3(ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor, ScaledSpace.ScaleFactor);
			skySphereGO.transform.parent = parentLocalTransform;

			Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", true);
		}
		
		public void SwitchScaledMode()
		{
			skySphereGO.layer = 9;

			skySphereGO.transform.parent = null;

			skySphereGO.transform.position = parentScaledTransform.position;
			skySphereGO.transform.localScale = Vector3.one;
			skySphereGO.transform.parent = parentScaledTransform;

			Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);
		}

		public void Resize(float size)
		{
			skySphereGO.transform.localScale = new Vector3 (size, size, size);
		}

		public void Cleanup()
		{
			if (!ReferenceEquals (skySphereGO, null))
				skySphereGO.DestroyGameObject ();
		}

		public void OnDestroy()
		{
			Cleanup ();
		}
	}
}