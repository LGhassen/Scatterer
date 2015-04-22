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
	public class isoSphere
	{
		private struct TriangleIndices
		{
			public int v1;
			public int v2;
			public int v3;
			
			public TriangleIndices(int v1, int v2, int v3)
			{
				this.v1 = v1;
				this.v2 = v2;
				this.v3 = v3;
			}
		}

		// return index of point in the middle of p1 and p2
		private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float altitude)
		{
			// first check if we have it already
			bool firstIsSmaller = p1 < p2;
			long smallerIndex = firstIsSmaller ? p1 : p2;
			long greaterIndex = firstIsSmaller ? p2 : p1;
			long key = (smallerIndex << 32) + greaterIndex;
			
			int ret;
			if (cache.TryGetValue(key, out ret))
			{
				return ret;
			}
			
			// not in cache, calculate it
			Vector3 point1 = vertices[p1];
			Vector3 point2 = vertices[p2];
			Vector3 middle = new Vector3
				(
					(point1.x + point2.x) / 2f,
					(point1.y + point2.y) / 2f,
					(point1.z + point2.z) / 2f
					);
			
			// add vertex makes sure point is on unit sphere
			int i = vertices.Count;
			vertices.Add(middle.normalized * altitude);
			
			// store it, return index
			cache.Add(key, i);
			
			return i;
		}

		public static void UpdateRadius(GameObject gameObject, float radius)
		{
			MeshFilter filter = gameObject.GetComponent<MeshFilter>();
			Mesh mesh = filter.mesh;
			Vector3[] verticies = mesh.vertices;
			for(int i = 0; i < verticies.Length; i++)
			{
				verticies[i] = verticies[i].normalized * radius;
			}
			mesh.vertices = verticies;
			mesh.RecalculateBounds();
			mesh.Optimize();
		}

		public static Mesh Create()
		{

			float radius = 1;


			Mesh mesh=new Mesh();
			mesh.Clear();
			
			List<Vector3> vertList = new List<Vector3>();
			Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
							
			int recursionLevel = 6;

			
			// create 12 vertices of a icosahedron
			float t = (1f + Mathf.Sqrt(5f)) / 2f;
			
			vertList.Add(new Vector3(-1f, t, 0f).normalized * radius);
			vertList.Add(new Vector3(1f, t, 0f).normalized * radius);
			vertList.Add(new Vector3(-1f, -t, 0f).normalized * radius);
			vertList.Add(new Vector3(1f, -t, 0f).normalized * radius);
			
			vertList.Add(new Vector3(0f, -1f, t).normalized * radius);
			vertList.Add(new Vector3(0f, 1f, t).normalized * radius);
			vertList.Add(new Vector3(0f, -1f, -t).normalized * radius);
			vertList.Add(new Vector3(0f, 1f, -t).normalized * radius);
			
			vertList.Add(new Vector3(t, 0f, -1f).normalized * radius);
			vertList.Add(new Vector3(t, 0f, 1f).normalized * radius);
			vertList.Add(new Vector3(-t, 0f, -1f).normalized * radius);
			vertList.Add(new Vector3(-t, 0f, 1f).normalized * radius);
			
			
			// create 20 triangles of the icosahedron
			List<TriangleIndices> faces = new List<TriangleIndices>();
			
			// 5 faces around point 0
			faces.Add(new TriangleIndices(0, 11, 5));
			faces.Add(new TriangleIndices(0, 5, 1));
			faces.Add(new TriangleIndices(0, 1, 7));
			faces.Add(new TriangleIndices(0, 7, 10));
			faces.Add(new TriangleIndices(0, 10, 11));
			
			// 5 adjacent faces 
			faces.Add(new TriangleIndices(1, 5, 9));
			faces.Add(new TriangleIndices(5, 11, 4));
			faces.Add(new TriangleIndices(11, 10, 2));
			faces.Add(new TriangleIndices(10, 7, 6));
			faces.Add(new TriangleIndices(7, 1, 8));
			
			// 5 faces around point 3
			faces.Add(new TriangleIndices(3, 9, 4));
			faces.Add(new TriangleIndices(3, 4, 2));
			faces.Add(new TriangleIndices(3, 2, 6));
			faces.Add(new TriangleIndices(3, 6, 8));
			faces.Add(new TriangleIndices(3, 8, 9));
			
			// 5 adjacent faces 
			faces.Add(new TriangleIndices(4, 9, 5));
			faces.Add(new TriangleIndices(2, 4, 11));
			faces.Add(new TriangleIndices(6, 2, 10));
			faces.Add(new TriangleIndices(8, 6, 7));
			faces.Add(new TriangleIndices(9, 8, 1));
			
			
			// refine triangles
			for (int i = 0; i < recursionLevel; i++)
			{
				List<TriangleIndices> faces2 = new List<TriangleIndices>();
				foreach (var tri in faces)
				{
					// replace triangle by 4 triangles
					int a = getMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
					int b = getMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
					int c = getMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);
					
					faces2.Add(new TriangleIndices(tri.v1, a, c));
					faces2.Add(new TriangleIndices(tri.v2, b, a));
					faces2.Add(new TriangleIndices(tri.v3, c, b));
					faces2.Add(new TriangleIndices(a, b, c));
				}
				faces = faces2;
			}
			
			Vector3[] normals = new Vector3[vertList.Count];
			for (int i = 0; i < normals.Length; i++)
				normals[i] = vertList[i].normalized;
			
			//if (celestialBody != null)
			//{
				for (int i = 0; i < vertList.Count; i++)
				{
					Vector3d rotVert = RotateY(vertList[i], .5 * Math.PI);
					//float value = (float)(height + celestialBody.pqsController.GetSurfaceHeight(rotVert));
//					vertList[i] *= value;
					vertList[i] *= 650000;
				}
			//}
			
			mesh.vertices = vertList.ToArray();
			
			List<int> triList = new List<int>();
			for (int i = 0; i < faces.Count; i++)
			{
				triList.Add(faces[i].v1);
				triList.Add(faces[i].v2);
				triList.Add(faces[i].v3);
			}
			mesh.triangles = triList.ToArray();
			
			float invPi2 = 1 / (2 * Mathf.PI);
			float invPi = 1 / (Mathf.PI);
			List<Vector2> uvList = new List<Vector2>();
			for (int i = 0; i < vertList.Count; i++)
			{
				Vector2 uv = new Vector2();
				Vector3 normal = vertList[i].normalized;
				uv.x = 0.5f + invPi2 * Mathf.Atan2(normal.z, normal.x);
				uv.y = 0.5f - invPi * Mathf.Asin(normal.y);
				uvList.Add(uv);
			}
			mesh.uv = uvList.ToArray();
			
			mesh.normals = normals;
			CalculateMeshTangents(mesh);
			
			mesh.RecalculateBounds();
			mesh.Optimize();

			return mesh;			
		}




		public static void CalculateMeshTangents(Mesh mesh)
		{
			//speed up math by copying the mesh arrays
			int[] triangles = mesh.triangles;
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			Vector3[] normals = mesh.normals;
			
			//variable definitions
			int triangleCount = triangles.Length;
			int vertexCount = vertices.Length;
			
			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];
			
			Vector4[] tangents = new Vector4[vertexCount];
			
			for (long a = 0; a < triangleCount; a += 3)
			{
				long i1 = triangles[a + 0];
				long i2 = triangles[a + 1];
				long i3 = triangles[a + 2];
				
				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];
				
				Vector2 w1 = uv[i1];
				Vector2 w2 = uv[i2];
				Vector2 w3 = uv[i3];
				
				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;
				
				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;
				
				float r = 1.0f / (s1 * t2 - s2 * t1);
				
				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
				
				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;
				
				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}
			
			
			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normals[a];
				Vector3 t = tan1[a];
				
				//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;
				
				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}
			
			mesh.tangents = tangents;
		}

		public static Vector3 RotateY(Vector3d v, double angle)
		{
			
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			
			double x = (cos * v.x) + (sin * v.z);
			double z = (cos * v.z) - (sin * v.x);
			return new Vector3d(x, v.y, z);
			
		}
	}
}

