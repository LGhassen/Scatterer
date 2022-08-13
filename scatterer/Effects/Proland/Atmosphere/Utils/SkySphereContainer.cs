using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class SkySphereContainer
	{
		GameObject skySphereGO;
		MeshRenderer skySphereMR;

		public GameObject GameObject { get { return skySphereGO; } }
		public MeshRenderer MeshRenderer { get { return skySphereMR; } }

		Transform parentLocalTransform, parentScaledTransform;

		MeshFilter skySphereMF;
		
		public SkySphereContainer(float size, Material material, Transform inParentLocalTransform, Transform inParentScaledTransform)
		{
			skySphereGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			GameObject.Destroy (skySphereGO.GetComponent<Collider> ());

			skySphereGO.transform.localScale = Vector3.one;
			
			skySphereMF = skySphereGO.GetComponent<MeshFilter>();
			Vector3[] verts = skySphereMF.mesh.vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = verts[i].normalized * size;
			}
			skySphereMF.mesh.vertices = verts;
			skySphereMF.mesh.RecalculateBounds();
			skySphereMF.mesh.RecalculateNormals();
			
			skySphereMR = skySphereGO.GetComponent<MeshRenderer>();
			skySphereMR.sharedMaterial = material;
			Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);

			skySphereMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			skySphereMR.receiveShadows = false;
			skySphereMR.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
			skySphereMR.enabled = true;

			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				skySphereGO.layer = 15;
			else
				skySphereGO.layer = 9;

			skySphereGO.transform.position = inParentScaledTransform.position;
			skySphereGO.transform.parent = inParentScaledTransform;

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

			skySphereGO.AddComponent<SkySphereScreenCopy> ();
		}
		
		public void SwitchScaledMode()
		{
			skySphereGO.layer = 9;

			skySphereGO.transform.parent = null;

			skySphereGO.transform.position = parentScaledTransform.position;
			skySphereGO.transform.localScale = Vector3.one;
			skySphereGO.transform.parent = parentScaledTransform;

			Utils.EnableOrDisableShaderKeywords (skySphereMR.sharedMaterial, "LOCAL_SKY_ON", "LOCAL_SKY_OFF", false);

			var scrCopy = skySphereGO.GetComponent<SkySphereScreenCopy> ();

			if (!ReferenceEquals (null, scrCopy))
				UnityEngine.Component.DestroyImmediate (scrCopy);

		}

		public void Resize(float size)
		{	
			Vector3[] verts = skySphereMF.mesh.vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] = verts[i].normalized * size;
			}
			skySphereMF.mesh.vertices = verts;
			skySphereMF.mesh.RecalculateBounds();
			skySphereMF.mesh.RecalculateNormals();
		}

		public void Cleanup()
		{
			if (skySphereMR != null)
			{
				skySphereMR.enabled = false;
				UnityEngine.Component.DestroyImmediate (skySphereMR);
			}

			if (skySphereGO != null)
			{
				UnityEngine.Object.DestroyImmediate(skySphereGO);
			}
		}
	}

	public class SkySphereScreenCopy : MonoBehaviour
	{
		void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			
			if (!cam)
				return;
			
			ScreenCopyCommandBuffer.EnableScreenCopyForFrame (cam);
		}
	}
}