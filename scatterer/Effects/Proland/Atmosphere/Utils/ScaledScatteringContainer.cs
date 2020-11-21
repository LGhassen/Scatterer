using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class ScaledScatteringContainer : MonoBehaviour
	{
		GameObject scaledScatteringGO;
		MeshRenderer scaledScatteringMR;
		
		public GameObject GameObject { get { return scaledScatteringGO; } }
		public MeshRenderer MeshRenderer { get { return scaledScatteringMR; } }
		
		Transform parentLocalTransform, parentScaledTransform;
		
		public ScaledScatteringContainer(Mesh planetMesh, Material material, Transform inParentLocalTransform, Transform inParentScaledTransform)
		{
			parentScaledTransform = inParentScaledTransform;
			parentLocalTransform = inParentLocalTransform;

			scaledScatteringGO = new GameObject ();

			scaledScatteringGO.transform.localScale = parentScaledTransform.localScale;
			scaledScatteringGO.transform.position = parentScaledTransform.position;
			scaledScatteringGO.transform.rotation = parentScaledTransform.localRotation;
			scaledScatteringGO.transform.parent = parentScaledTransform;
			
			MeshFilter skySphereMF = scaledScatteringGO.AddComponent<MeshFilter>();
			skySphereMF.mesh = (Mesh)Instantiate (planetMesh);
			
			scaledScatteringMR = scaledScatteringGO.AddComponent<MeshRenderer>();
			scaledScatteringMR.sharedMaterial = material;
			Utils.EnableOrDisableShaderKeywords (scaledScatteringMR.sharedMaterial, "LOCAL_MODE_ON", "LOCAL_MODE_OFF", false);
			
			scaledScatteringMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			scaledScatteringMR.receiveShadows = false;
			scaledScatteringMR.enabled = true;
			
			if (HighLogic.LoadedScene == GameScenes.MAINMENU)
				scaledScatteringGO.layer = 15;
			else
				scaledScatteringGO.layer = 10;
		}

		//To recheck if this renders or not on localCamera, I think it does but position, scale and renderqueue might be wrong
		public void SwitchLocalMode()
		{
			scaledScatteringGO.layer = 15;

			scaledScatteringGO.transform.parent = null;

			scaledScatteringGO.transform.localScale = parentScaledTransform.localScale * ScaledSpace.ScaleFactor;

			scaledScatteringGO.transform.position = parentLocalTransform.position;
			scaledScatteringGO.transform.rotation = parentScaledTransform.localRotation;
			scaledScatteringGO.transform.parent = parentLocalTransform;
			
			Utils.EnableOrDisableShaderKeywords (scaledScatteringMR.sharedMaterial, "LOCAL_MODE_ON", "LOCAL_MODE_OFF", true);
		}
		
		public void SwitchScaledMode()
		{
			scaledScatteringGO.layer = 10;

			scaledScatteringGO.transform.parent = null;

			scaledScatteringGO.transform.localScale = parentScaledTransform.localScale;
			scaledScatteringGO.transform.position = parentScaledTransform.position;
			scaledScatteringGO.transform.rotation = parentScaledTransform.localRotation;
			scaledScatteringGO.transform.parent = parentScaledTransform;
			
			Utils.EnableOrDisableShaderKeywords (scaledScatteringMR.sharedMaterial, "LOCAL_MODE_ON", "LOCAL_MODE_OFF", false);
		}
		
		public void Cleanup()
		{
			if (!ReferenceEquals (scaledScatteringGO, null))
				scaledScatteringGO.DestroyGameObject ();
		}
		
		public void OnDestroy()
		{
			Cleanup ();
		}
	}
}