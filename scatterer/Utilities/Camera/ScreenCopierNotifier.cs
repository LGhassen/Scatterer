// Exists only to notify the screenCopy commandBuffer that a screen copy is needed for this frame

using System;
using UnityEngine;

namespace scatterer
{
	public class ScreenCopierNotifierObject
	{
		GameObject backgroundCopierGO;
		ScreenCopierNotifier backgroundCopier;


		public ScreenCopierNotifierObject (CelestialBody cb)
		{

			GameObject oldGO = GameObject.Find("Scatterer background Copier "+cb.name);

			backgroundCopierGO = new GameObject ("Scatterer background Copier "+cb.name);
			backgroundCopierGO.transform.parent = cb.transform;

			MeshRenderer mr = backgroundCopierGO.AddComponent<MeshRenderer> ();
			mr.material = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/invisible")]);
			mr.receiveShadows = false;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.enabled = true;

			MeshFilter mf   = backgroundCopierGO.AddComponent<MeshFilter> ();
			mf.mesh.Clear ();
			mf.mesh = MeshFactory.MakePlane (2, 2, MeshFactory.PLANE.XY, false, false);
			mf.mesh.bounds = new Bounds (Vector4.zero, new Vector3 (Mathf.Infinity, Mathf.Infinity, Mathf.Infinity));

			backgroundCopierGO.layer = 15;

			backgroundCopier = backgroundCopierGO.AddComponent<ScreenCopierNotifier> ();
		}

		public void Cleanup ()
		{
			backgroundCopier.isEnabled = false;
			Component.Destroy (backgroundCopier);
			backgroundCopierGO.DestroyGameObject ();
			backgroundCopierGO = null;
		}

		public void Enable(bool enable)
		{
			backgroundCopierGO.SetActive (enable);
			backgroundCopier.isEnabled = enable;
		}
	}

	public class ScreenCopierNotifier : MonoBehaviour
	{
		public bool isEnabled = false;

		public ScreenCopierNotifier ()
		{
		}

		void OnWillRenderObject()
		{
			if (isEnabled)
			{
				ScreenCopyCommandBuffer.EnableForThisFrame (Camera.current);
			}
		}
	}
}

