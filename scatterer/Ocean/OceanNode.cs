/*
 * Proland: a procedural landscape rendering library.
 * Copyright (c) 2008-2011 INRIA
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * Proland is distributed under a dual-license scheme.
 * You can obtain a specific license from Inria: proland-licensing@inria.fr.
 *
 * Authors: Eric Bruneton, Antoine Begault, Guillaume Piolat.
 * Modified and ported to Unity by Justin Hawkins 2014
 *
 *
 */

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
	
	/**
 * An AbstractTask to draw a flat or spherical ocean.
 * This class provides the functions and data to draw a flat projected grid but nothing else
 */
	public abstract class OceanNode : MonoBehaviour
	{
		
		Manager m_manager;
		Core m_core;
		
		[SerializeField]
		protected Material m_oceanMaterial;
		
		[SerializeField]
		protected Color m_oceanUpwellingColor = new Color(0.039f, 0.156f, 0.47f);
		
		//Sea level in meters
		[SerializeField]
		protected float m_oceanLevel = 5.0f;
		double h=0;
		//The maximum altitude at which the ocean must be displayed.
		[SerializeField]
		protected float m_zmin = 20000.0f;
		//Size of each grid in the projected grid. (number of pixels on screen)
		[SerializeField]
		protected int m_resolution = 4;
		
		Mesh[] m_screenGrids;
		Matrix4x4d m_oldlocalToOcean;
		Matrix4x4d m_oldworldToOcean;
		Vector3d2 m_offset;
		
		//If the ocean should be draw. To minimize depth fighting the ocean is not draw
		//when the camera is far away. Instead the terrain shader should render the ocean areas directly on the terrain
		bool m_drawOcean;
		
		//Concrete classes must provide a function that returns the
		//variance of the waves need for the BRDF rendering of waves
		public abstract float GetMaxSlopeVariance();
		
		public bool GetDrawOcean()
		{
			return m_drawOcean;
		}
		
		// Use this for initialization
		public virtual void Start ()
		{
			//			base.Start();
			
			m_oceanMaterial=new Material(ShaderTool.GetMatFromShader2("CompiledOceanWhiteCaps.shader"));
			
			m_manager.GetSkyNode().InitUniforms(m_oceanMaterial);
			
			m_oldlocalToOcean = Matrix4x4d.Identity();
			m_oldworldToOcean = Matrix4x4d.Identity();
			m_offset = Vector3d2.Zero();
			
			//Create the projected grid. The resolution is the size in pixels
			//of each square in the grid. If the squares are small the size of
			//the mesh will exceed the max verts for a mesh in Unity. In this case
			//split the mesh up into smaller meshes.
			
			m_resolution = Mathf.Max(1, m_resolution);
			//The number of squares in the grid on the x and y axis
			int NX = Screen.width / m_resolution;
			int NY = Screen.height / m_resolution;
			int numGrids = 1;
			
			const int MAX_VERTS = 65000;
			//The number of meshes need to make a grid of this resolution
			if(NX*NY > MAX_VERTS)
			{
				numGrids += (NX*NY) / MAX_VERTS;
			}
			
			m_screenGrids = new Mesh[numGrids];
			//Make the meshes. The end product will be a grid of verts that cover
			//the screen on the x and y axis with the z depth at 0. This grid is then
			//projected as the ocean by the shader
			for(int i = 0; i < numGrids; i++)
			{
				NY = Screen.height / numGrids / m_resolution;
				
				m_screenGrids[i] = MakePlane(NX, NY, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				m_screenGrids[i].bounds = new Bounds(Vector3.zero, new Vector3(1e8f, 1e8f, 1e8f));
			}			
		}
		
		public virtual void OnDestroy()
		{
			//			base.OnDestroy();
		}
		
		Mesh MakePlane(int w, int h, float offset, float scale)
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
					
					uv.y *= scale;
					uv.y += offset;
					
					Vector2 p = new Vector2();
					p.x = (uv.x-0.5f)*2.0f;
					p.y = (uv.y-0.5f)*2.0f;
					
					Vector3 pos = new Vector3(p.x, p.y, 0.0f);
					Vector3 norm = new Vector3(0.0f, 0.0f, 1.0f);
					
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
					indices[num++] = x + y * w;
					indices[num++] = x + (y+1) * w;
					indices[num++] = (x+1) + y * w;
					
					indices[num++] = x + (y+1) * w;
					indices[num++] = (x+1) + (y+1) * w;
					indices[num++] = (x+1) + y * w;
				}
			}
			
			Mesh mesh = new Mesh();
			
			mesh.vertices = vertices;
			mesh.uv = texcoords;
			mesh.triangles = indices;
			mesh.normals = normals;
			
			return mesh;
		}
		
		public virtual void UpdateNode()
		{
			
			//			GameObject lineObj = new GameObject("Line");
			//			GameObject hatObj  = new GameObject("Line");
			//			
			////			if (onMap)
			////			{
			//				lineObj.layer = 9;
			//				hatObj.layer = 9;
			////			}
			//
			//			LineRenderer _line;
			//			LineRenderer _hat;
			//
			//			if (lineObj.GetComponent<LineRenderer> == null) {			
			//				 _line = lineObj.AddComponent<LineRenderer> ();
			//				 _hat = hatObj.AddComponent<LineRenderer> ();
			//
			//			}
			//			
			//			_line.useWorldSpace = false;
			//			_hat.useWorldSpace  = false;
			
			// Default the vector's origin to the current ship.
			// (user can modify this using VECTOR:SHOWSTART)
			//			Vector3d shipPosFromOrigin = FlightGlobals.ActiveVessel.findWorldCenterOfMass();
			//			_line.transform.localPosition = shipPosFromOrigin;
			//			_hat.transform.localPosition  = shipPosFromOrigin;
			//			
			//			_line.material = new Material(Shader.Find("Particles/Additive"));
			//			_hat.material  = new Material(Shader.Find("Particles/Additive"));

//			print ("widtth, height");
//			print (Screen.width);
//			print (Screen.height);
			
			
			if ((m_manager.m_skyNode.farCamera) != (null)) {
				if (m_manager.m_skyNode.farCamera.gameObject.GetComponent<OceanUpdateAtCameraRythm> () == null) {
					m_manager.m_skyNode.farCamera.gameObject.AddComponent (typeof(OceanUpdateAtCameraRythm));
				}
				
				if (m_manager.m_skyNode.farCamera.gameObject.GetComponent<OceanUpdateAtCameraRythm> () != null) {
					m_manager.m_skyNode.farCamera.gameObject.GetComponent<OceanUpdateAtCameraRythm> ().m_oceanNode = this;
				}
				
				else print ("NULL OCEAN NODE");
				
			}
			
			else print ("NULL FAR CAM");
			
			
			foreach (Mesh mesh in m_screenGrids) {
				m_oceanMaterial.renderQueue=m_manager.GetCore().renderQueue;
				//				print ("RENDERQUEUE");
				//				print(m_oceanMaterial.renderQueue);
				mesh.bounds = new Bounds (Vector3.zero, new Vector3 (1e30f, 1e30f, 1e30f));
				Graphics.DrawMesh (mesh, Vector3.zero,Quaternion.identity, m_oceanMaterial, m_core.layer, m_core.chosenCamera);
				
			}	
			
			PQS pqs = m_manager.parentCelestialBody.pqsController;
			PQS ocean = pqs.ChildSpheres [0];
						ocean.isDisabled = true;
						ocean.DisableSphere ();
		}
		
		
		public void updateStuff(){						
			//Calculates the required data for the projected grid
			
			// compute ltoo = localToOcean transform, where ocean frame = tangent space at
			// camera projection on sphere radius in local space
									
//			m_core.chosenCamera.ResetProjectionMatrix();
//			m_core.chosenCamera.ResetWorldToCameraMatrix();

			Matrix4x4 ctol1 = m_core.chosenCamera.cameraToWorldMatrix;
						
			//position relative to kerbin
			Vector3d tmp = (m_core.chosenCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			print ("TMP");
			print (tmp);
			print (tmp.magnitude);

			Vector3d2 cl = new Vector3d2 ();
//			cl.x = tmp.x;
//			cl.y = tmp.y;
//			cl.z = tmp.z;
			
			
//			Matrix4x4d cameraToWorld = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, tmp.x,
//			                                           ctol1.m10, ctol1.m11, ctol1.m12, tmp.y,
//			                                           ctol1.m20, ctol1.m21, ctol1.m22, tmp.z,
//			                                           ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);

			Matrix4x4d cameraToWorld = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, ctol1.m03,
			                                           ctol1.m10, ctol1.m11, ctol1.m12, ctol1.m13,
			                                           ctol1.m20, ctol1.m21, ctol1.m22, ctol1.m23,
			                                           ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);



//			Matrix4x4d cameraToWorld = new Matrix4x4d (0,0.906534175092856,  0.422132431102747,    6365631.33384793,
//			                                           -0.997188818006091 ,  0.0316302600890084,   -0.0679263416526804,  -906.151431945729,
//			                                           0.0749297086849734,0.420945740013386,-0.903985742542972,-12059.3565785169,
//			                                           0,0,0,1);


			//Vector3d planetPos = m_manager.parentCelestialBody.transform.position;

			Matrix4x4d worldToLocal = new Matrix4x4d( m_manager.parentCelestialBody.transform.worldToLocalMatrix);

			Vector4 translation = m_manager.parentCelestialBody.transform.worldToLocalMatrix.inverse.GetColumn (3);

			Matrix4x4d worldToLocal2= new Matrix4x4d (1, 0, 0, -translation.x ,
			                                          0, 1, 0, -translation.y,
			                                          0, 0, 1, -translation.z,
			                                          0, 0, 0, 1);

			print ("translation");
			print (translation);


			//Matrix4x4d camToLocal = cameraToWorld.Inverse ();// * worldToLocal.Inverse();
			Matrix4x4d camToLocal =  worldToLocal2 * cameraToWorld ;
//			Matrix4x4d camToLocal = cameraToWorld ;

//			print ("WORLD TO LOCAL");
//			print (worldToLocal);

//			print ("CAM TO LOCAL");
//			print (camToLocal);

			//cl = camToLocal * Vector3d2.Zero ();
			cl = camToLocal * Vector3d2.Zero ();

			print ("CL");
			print (cl);
			print (cl.Magnitude ());
						
			// camera in local space relative to planet's origin
			
			double radius = m_manager.GetRadius ();
			
			m_drawOcean = true;
			Vector3d2 ux, uy, uz, oo;
			
			uz = cl.Normalized() ; // unit z vector of ocean frame, in local space
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity())
			{
				ux = (new Vector3d2(m_oldlocalToOcean.m[1,0], m_oldlocalToOcean.m[1,1], m_oldlocalToOcean.m[1,2])).Cross(uz).Normalized();
			}
			
			else
			{
				ux = Vector3d2.UnitZ().Cross(uz).Normalized();
			}
			
			uy = uz.Cross(ux); // unit y vector
			
			h = cl.Magnitude() - radius;
			Vector3d tmp2=m_core.chosenCamera.transform.position-m_manager.parentCelestialBody.transform.position;
			
			oo = uz * (radius); // origin of ocean frame, in local space
			oo.x = oo.x;
			oo.y = oo.y;
			oo.z = oo.z;
			
			//local to ocean transform
			//computed from oo and ux, uy, uz should be correct
			Matrix4x4d localToOcean = new Matrix4x4d(
				ux.x, ux.y, ux.z, -ux.Dot(oo),
				uy.x, uy.y, uy.z, -uy.Dot(oo),
				uz.x, uz.y, uz.z, -uz.Dot(oo),
				0.0,  0.0,  0.0,  1.0);


			Matrix4x4d cameraToOcean = localToOcean * camToLocal;
//			Matrix4x4d cameraToOcean = OceanToWorld.Inverse() * cameraToWorld;
			Vector3d2 delta=new Vector3d2(0,0,0);
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity())
			{
				/*Vector3d2 */delta = localToOcean * (m_oldlocalToOcean.Inverse() * Vector3d2.Zero());
				m_offset += delta;
			}
			
			m_oldlocalToOcean = localToOcean;



			
			Matrix4x4d stoc = ModifiedProjectionMatrix (m_core.chosenCamera).Inverse();

//			Matrix4x4d stoc = new Matrix4x4d(3.57253843540205,0,0,0,
//			                             0,3.577350279552042,0,0,
//			                             0,0,0,-2,
//			                             0,0,-0.00178426976276966,0.00178426997547119);




						
			Vector3d2 oc = cameraToOcean * Vector3d2.Zero();
			

			//			h = tmp2.magnitude - radius;
			h = oc.z;
			print ("h=");
			print (h);

			print ("tmp2.magnitude - radius");
			print (tmp2.magnitude - radius);
			
			Vector4d stoc_w = (stoc * Vector4d.UnitW()).XYZ0();
			Vector4d stoc_x = (stoc * Vector4d.UnitX()).XYZ0();
			Vector4d stoc_y = (stoc * Vector4d.UnitY()).XYZ0();
			
			Vector3d2 A0 = (cameraToOcean * stoc_w).XYZ();
			Vector3d2 dA = (cameraToOcean * stoc_x).XYZ();
			Vector3d2 B =  (cameraToOcean * stoc_y).XYZ();
			
			Vector3d2 horizon1, horizon2;
			
			Vector3d2 offset = new Vector3d2(-m_offset.x, -m_offset.y, h);
			
			
			//			print ("offset");
			//			print (offset);
			
			
			double h1 = h * (h + 2.0 * radius);
			double h2 = (h + radius) * (h + radius);
			double alpha = B.Dot(B) * h1 - B.z * B.z * h2;
			
			double beta0 = (A0.Dot(B) * h1 - B.z * A0.z * h2) / alpha;
			double beta1 = (dA.Dot(B) * h1 - B.z * dA.z * h2) / alpha;
			
			double gamma0 = (A0.Dot(A0) * h1 - A0.z * A0.z * h2) / alpha;
			double gamma1 = (A0.Dot(dA) * h1 - A0.z * dA.z * h2) / alpha;
			double gamma2 = (dA.Dot(dA) * h1 - dA.z * dA.z * h2) / alpha;
			
			horizon1 = new Vector3d2(-beta0, -beta1, 0.0);
			horizon2 = new Vector3d2(beta0 * beta0 - gamma0, 2.0 * (beta0 * beta1 - gamma1), beta1 * beta1 - gamma2);
			
			Vector3d2 sunDir = new Vector3d2 (m_manager.getDirectionToSun ().normalized);
			Vector3d2 oceanSunDir = localToOcean.ToMatrix3x3d() * sunDir;
			
			m_oceanMaterial.SetVector("_Ocean_SunDir", oceanSunDir.ToVector3());
			
			m_oceanMaterial.SetVector("_Ocean_Horizon1", horizon1.ToVector3());
			m_oceanMaterial.SetVector("_Ocean_Horizon2", horizon2.ToVector3());
			
			m_oceanMaterial.SetMatrix("_Ocean_CameraToOcean", cameraToOcean.ToMatrix4x4());
			m_oceanMaterial.SetMatrix("_Ocean_OceanToCamera", cameraToOcean.Inverse().ToMatrix4x4());
			
			m_oceanMaterial.SetVector("_Ocean_CameraPos", offset.ToVector3());
			
			m_oceanMaterial.SetVector("_Ocean_Color", m_oceanUpwellingColor * 0.1f);
			m_oceanMaterial.SetVector("_Ocean_ScreenGridSize", new Vector2((float)m_resolution / (float)Screen.width, (float)m_resolution / (float)Screen.height));
			m_oceanMaterial.SetFloat("_Ocean_Radius", (float)radius);
			
			m_manager.GetSkyNode().SetUniforms(m_oceanMaterial);
			
		}
					

		public void SetUniforms(Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if(mat == null) return;
			
			mat.SetFloat("_Ocean_Sigma", GetMaxSlopeVariance());
			mat.SetVector("_Ocean_Color", m_oceanUpwellingColor * 0.1f);
			mat.SetFloat("_Ocean_DrawBRDF", (m_drawOcean) ? 0.0f : 1.0f);
			
			
//			mat.SetFloat("_Ocean_Level", m_oceanLevel-(float)h);
		}
		
		public void setManager(Manager manager)
		{
			m_manager=manager;
		}
		
		public void setCore(Core core)
		{
			m_core=core;
		}
		
		public static Quaternion QuaternionFromMatrix(Matrix4x4 m) {
			// Adapted from: http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
			Quaternion q = new Quaternion();
			q.w = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] + m[1,1] + m[2,2] ) ) / 2; 
			q.x = Mathf.Sqrt( Mathf.Max( 0, 1 + m[0,0] - m[1,1] - m[2,2] ) ) / 2; 
			q.y = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] + m[1,1] - m[2,2] ) ) / 2; 
			q.z = Mathf.Sqrt( Mathf.Max( 0, 1 - m[0,0] - m[1,1] + m[2,2] ) ) / 2; 
			q.x = Mathf.Sign( q.x * ( m[2,1] - m[1,2] ) );
			q.y = Mathf.Sign( q.y * ( m[0,2] - m[2,0] ) );
			q.z = Mathf.Sign( q.z * ( m[1,0] - m[0,1] ) );
			return q;
		}

		public Matrix4x4d ModifiedProjectionMatrix(Camera inCam)
		{ 
			
//			float h = (float)(GetHeight() - m_groundHeight);
//			camera.nearClipPlane = 0.1f * h;
//			camera.farClipPlane = 1e6f * h;
			
			inCam.ResetProjectionMatrix();
			
			Matrix4x4 p = inCam.projectionMatrix;
			bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
			
			if(d3d) 
			{
				if(inCam.actualRenderingPath == RenderingPath.DeferredLighting)
				{
					// Invert Y for rendering to a render texture
					for (int i = 0; i < 4; i++) {
						p[1,i] = -p[1,i];
					}
				}
				
				// Scale and bias depth range
				for (int i = 0; i < 4; i++) {
					p[2,i] = p[2,i]*0.5f + p[3,i]*0.5f;
				}
			}
			
			Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d(p);
			inCam.projectionMatrix = m_cameraToScreenMatrix.ToMatrix4x4();
			return m_cameraToScreenMatrix;
			//m_screenToCameraMatrix = m_cameraToScreenMatrix.Inverse();
		}
		
	}
	
}