// Shamelessly stolen from rbray's wip-EVE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class SimpleRenderingShape
	{
		GameObject meshContainer;

		public GameObject GameObject { get { return meshContainer; } }
		
		public SimpleRenderingShape(float size, Material material, bool sphere)
		{

			GameObject cube;
			if (!sphere)
				cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			else
				cube = GameObject.CreatePrimitive(PrimitiveType.Sphere);


			GameObject.Destroy (cube.GetComponent<Collider> ());

			cube.transform.localScale = Vector3.one;
			meshContainer = cube;
			
			MeshFilter mf = cube.GetComponent<MeshFilter>();
			Vector3[] verts = mf.mesh.vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				verts[i] *= size;
			}
			mf.mesh.vertices = verts;
			mf.mesh.RecalculateBounds();
			mf.mesh.RecalculateNormals();
			
			var mr = cube.GetComponent<MeshRenderer>();
			mr.material = material;
			
//			mr.castShadows = false;
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			mr.receiveShadows = false;
			mr.enabled = true;
		}

		public void resize(float size)
		{
			meshContainer.transform.localScale = new Vector3 (size, size, size);
		}
	}
}