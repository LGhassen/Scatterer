﻿using UnityEngine;
using System.Collections;

public static class MeshFactory  
{
	public enum PLANE { XY, XZ, YZ };

	//public static Mesh MakePlane(int w, int h, PLANE plane = PLANE.XY, bool _01 = true, bool cw = false)
	//public static Mesh MakePlane(int w, int h, PLANE plane = PLANE.XY, bool _01 = true, bool cw) 
	public static Mesh MakePlane(int w, int h, PLANE plane, bool _01, bool cw) 
	{
		
		Vector3[] vertices = new Vector3[w*h];
		Vector2[] texcoords = new Vector2[w*h];
		Vector3[] normals = new Vector3[w*h];
		int[] indices = new int[w*h*6];
		
		for(int x = 0; x < w; x++)
		{
			for(int y = 0; y < h; y++)
			{
				Vector2 uv = new Vector3((float)x / (float)(w-1), (float)y / (float)(h-1));
				Vector2 p = new Vector2();

				if(_01)
					p = uv;
				else {
					p.x = (uv.x-0.5f)*2.0f;
					p.y = (uv.y-0.5f)*2.0f;
				}

				Vector3 pos, norm;

				switch((int)plane)
				{
				case (int)PLANE.XY:
					pos = new Vector3(p.x, p.y, 0.0f);
					norm = new Vector3(0.0f, 0.0f, 1.0f);
					break;

				case (int)PLANE.XZ:
					pos = new Vector3(p.x, 0.0f, p.y);
					norm = new Vector3(0.0f, 1.0f, 0.0f);
					break;

				case (int)PLANE.YZ:
					pos = new Vector3(0.0f, p.x, p.y);
					norm = new Vector3(1.0f, 0.0f, 0.0f);
					break;

				default:
					pos = new Vector3(p.x, p.y, 0.0f);
					norm = new Vector3(0.0f, 0.0f, 1.0f);
					break;
				}
				
				texcoords[x+y*w] = uv;
				vertices[x+y*w] = pos;
				normals[x+y*w] = norm;
			}
		}
		
		int num = 0;
		for(int x = 0; x < w-1; x++)
		{
			for(int y = 0; y < h-1; y++)
			{
				if(cw)
				{
					indices[num++] = x + y * w;
					indices[num++] = x + (y+1) * w;
					indices[num++] = (x+1) + y * w;
					
					indices[num++] = x + (y+1) * w;
					indices[num++] = (x+1) + (y+1) * w;
					indices[num++] = (x+1) + y * w;
				}
				else
				{
					indices[num++] = x + y * w;
					indices[num++] = (x+1) + y * w;
					indices[num++] = x + (y+1) * w;
					
					indices[num++] = x + (y+1) * w;
					indices[num++] = (x+1) + y * w;
					indices[num++] = (x+1) + (y+1) * w;
				}
			}
		}
		
		Mesh mesh = new Mesh();
		
		mesh.vertices = vertices;
		mesh.uv = texcoords;
		mesh.triangles = indices;
		mesh.normals = normals;
		
		return mesh;
	}
}












