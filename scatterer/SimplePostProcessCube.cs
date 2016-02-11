// Shamelessly stolen from rbray's wip-EVE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace scatterer
{
	public class SimplePostProcessCube
	{
		
		GameObject meshContainer;
		public GameObject GameObject { get { return meshContainer; } }
		
		public SimplePostProcessCube(float size, Material material)
		{
			
			GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObject.Destroy(cube.collider);
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
			
			mr.castShadows = false;
			mr.receiveShadows = false;
			mr.enabled = true;
		}
	}
}