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
	/*
	 * An AbstractTask to draw a flat or spherical ocean.
	 * This class provides the functions and data to draw a flat projected grid but nothing else
	 */
	public abstract class OceanNode: MonoBehaviour
	{
		Matrix4x4d m_cameraToWorldMatrix;
		public Manager m_manager;
		Core m_core;
		
		//		public float theta =1.0f;
		//		public float phi=1.0f;
		
		public Material m_oceanMaterialNear;
		public Material m_oceanMaterialFar;
		OceanUpdateAtCameraRythm oceanupdater;

		[Persistent] public Vector3 m_oceanUpwellingColor = new Vector3 (0.0039f, 0.0156f, 0.047f);
		
		//Sea level in meters
		[Persistent]
		public float m_oceanLevel = 0.0f;

		bool stockOceanExists = true;
		PQS ocean;
		double h = 0;
		//The maximum altitude at which the ocean must be displayed.
//		[Persistent]
		protected float m_zmin = 20000.0f;
		
		//Size of each grid in the projected grid. (number of pixels on screen)
		
		[Persistent]
		public int m_resolution = 4;
//		[Persistent]
		public int MAX_VERTS = 65000;
//		[Persistent]
		public float oceanScale = 1f;
		[Persistent]
		public float oceanAlpha = 1f;

		[Persistent]
		public float alphaRadius = 3000f;


//		[Persistent]
		public float sunReflectionMultiplier = 1f;

//		[Persistent]
		public float skyReflectionMultiplier = 1f;
		
//		[Persistent]
		public float seaRefractionMultiplier = 1f;





		int numGrids;
		Mesh[] m_screenGrids;
		Material emptyMaterial;
		Material fakeOceanMaterial;
		SimplePostProcessCube oceanPC;
		GameObject fakeOceanObject;

		Mesh fakeOceanMesh;
		MeshFilter fakeOceanMF;
		MeshRenderer fakeOceanMR;
//		Material newMat;

		[Persistent] public float fakeOceanAltitude = 15000;

//		GameObject[] waterGameObjectsNear;
//		MeshRenderer[] waterMeshRenderersNear;
//		MeshFilter[] waterMeshFiltersNear;
//
//		GameObject[] waterGameObjectsFar;
//		MeshRenderer[] waterMeshRenderersFar;
//		MeshFilter[] waterMeshFiltersFar;

		Matrix4x4d m_oldlocalToOcean;
		Matrix4x4d m_oldworldToOcean;

		public Vector3 offsetVector3{
			get {
				return offset.ToVector3();
			}
		}

		


		Vector3d2 m_offset;
		public Vector3d2 offset;

		public Vector3d2 ux, uy, uz, oo;
		
		//If the ocean should be drawn. To minimize depth fighting
		bool m_drawOcean;
		
		//Concrete classes must provide a function that returns the
		//variance of the waves need for the BRDF rendering of waves
		public abstract float GetMaxSlopeVariance ();
		
		public bool GetDrawOcean ()
		{
			return m_drawOcean;
		}
		
		// Use this for initialization
		public virtual void Start ()
		{
			m_cameraToWorldMatrix = Matrix4x4d.Identity ();
			
			//using different materials for both the far and near cameras because they have different projection matrixes
			//the projection matrix in the shader has to match that of the camera or the projection will be wrong and the ocean will
			//appear to "shift around"
			m_oceanMaterialNear = new Material (ShaderTool.GetMatFromShader2 ("CompiledOceanWhiteCaps.shader"));
			m_oceanMaterialFar = new Material (ShaderTool.GetMatFromShader2 ("CompiledOceanWhiteCaps.shader"));
			
			m_manager.GetSkyNode ().InitUniforms (m_oceanMaterialNear);
			m_manager.GetSkyNode ().InitUniforms (m_oceanMaterialFar);
			
			m_oldlocalToOcean = Matrix4x4d.Identity ();
			m_oldworldToOcean = Matrix4x4d.Identity ();
			m_offset = Vector3d2.Zero ();
			
			//Create the projected grid. The resolution is the size in pixels
			//of each square in the grid. If the squares are small the size of
			//the mesh will exceed the max verts for a mesh in Unity. In this case
			//split the mesh up into smaller meshes.
			
			m_resolution = Mathf.Max (1, m_resolution);
			//The number of squares in the grid on the x and y axis
			int NX = Screen.width / m_resolution;
			int NY = Screen.height / m_resolution;
			numGrids = 1;
			
			//			const int MAX_VERTS = 65000;
			//The number of meshes need to make a grid of this resolution
			if (NX * NY > MAX_VERTS) {
				numGrids += (NX * NY) / MAX_VERTS;
			}
			
			m_screenGrids = new Mesh[numGrids];

//			waterGameObjectsNear = new GameObject[numGrids];
//			waterMeshRenderersNear = new MeshRenderer[numGrids];
//			waterMeshFiltersNear = new MeshFilter[numGrids];
//
//			waterGameObjectsFar = new GameObject[numGrids];
//			waterMeshRenderersFar = new MeshRenderer[numGrids];
//			waterMeshFiltersFar = new MeshFilter[numGrids];

			//Make the meshes. The end product will be a grid of verts that cover
			//the screen on the x and y axis with the z depth at 0. This grid is then
			//projected as the ocean by the shader
			for (int i = 0; i < numGrids; i++) {
				NY = Screen.height / numGrids / m_resolution;
				
				m_screenGrids [i] = MakePlane (NX, NY, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				m_screenGrids [i].bounds = new Bounds (Vector3.zero, new Vector3 (1e8f, 1e8f, 1e8f));
				

				//bad idea, the meshes still render arbitrarily to the near and far camera and end up drawing over everything
				//to get around this I use drawmesh further down
				//seems to have better performance also

//								waterGameObjectsNear[i] = new GameObject();
//								waterGameObjectsNear[i].transform.parent=m_manager.parentCelestialBody.transform;
//								waterMeshFiltersNear[i] = waterGameObjectsNear[i].AddComponent<MeshFilter>();
//								waterMeshFiltersNear[i].mesh.Clear ();
//								waterMeshFiltersNear[i].mesh = m_screenGrids[i];
//								waterGameObjectsNear[i].layer = 15;
//
//								waterMeshRenderersNear[i] = waterGameObjectsNear[i].AddComponent<MeshRenderer>();
//
//								waterMeshRenderersNear[i].sharedMaterial = m_oceanMaterialNear;
//								waterMeshRenderersNear[i].material =m_oceanMaterialNear;
//								
//								waterMeshRenderersNear[i].castShadows = false;
//								waterMeshRenderersNear[i].receiveShadows = false;
//				
//								waterMeshRenderersNear[i].enabled=true;
//
//
//				waterGameObjectsFar[i] = new GameObject();
//				waterGameObjectsFar[i].transform.parent=m_manager.parentCelestialBody.transform;
//				waterMeshFiltersFar[i] = waterGameObjectsFar[i].AddComponent<MeshFilter>();
//				waterMeshFiltersFar[i].mesh.Clear ();
//				waterMeshFiltersFar[i].mesh = m_screenGrids[i];
//				waterGameObjectsFar[i].layer = 15;
//				
//				waterMeshRenderersFar[i] = waterGameObjectsFar[i].AddComponent<MeshRenderer>();
//				
//				waterMeshRenderersFar[i].sharedMaterial = m_oceanMaterialFar;
//				waterMeshRenderersFar[i].material =m_oceanMaterialFar;
//				
//				waterMeshRenderersFar[i].castShadows = false;
//				waterMeshRenderersFar[i].receiveShadows = false;
//				
//				waterMeshRenderersFar[i].enabled=true;
				
			}

//			PQS pqs = m_manager.parentCelestialBody.pqsController;
//
//			if (pqs.ChildSpheres[0])
//				UnityEngine.Object.Destroy (pqs.ChildSpheres [0]);

//			if (ocean)
//			{
//				UnityEngine.Object.Destroy (ocean);
//			}



			
			//				Debug.Log("PQS.childspheres count"+pqs.ChildSpheres.Length);
			
//			PQS pqs = m_manager.parentCelestialBody.pqsController;
//			if (pqs.ChildSpheres [0]) {
//				ocean = pqs.ChildSpheres [0];
//				//					ocean.surfaceMaterial = new Material (ShaderTool.GetMatFromShader2 ("EmptyShader.shader"));
//				
//				///Thanks to rbray89 for this snippet that disables the stock ocean in a clean way
//				GameObject container = ocean.gameObject;
//				
//				FakeOceanPQS fakeOcean1 = new GameObject ().AddComponent<FakeOceanPQS> ();
//				
//				fakeOcean1.CloneFrom (ocean);
//				Destroy (ocean);
//				
//				FakeOceanPQS fakeOcean = container.AddComponent<FakeOceanPQS> ();
//				fakeOcean.CloneFrom (fakeOcean1);
//				
//				Destroy (fakeOcean1);
//				
//				FieldInfo field = typeof(PQS).GetFields (BindingFlags.Instance | BindingFlags.NonPublic).First (
//					f => f.FieldType == typeof(PQS[]));
//				field.SetValue (pqs, new PQS[] {fakeOcean });
//				
//				PQSMod_CelestialBodyTransform cbt = pqs.GetComponentsInChildren<PQSMod_CelestialBodyTransform> () [0];
//				cbt.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[] { };
//			}


//			fakeOceanMesh = isosphere.Create (m_manager.GetRadius());
//
//
//			fakeOcean = new GameObject ();
//			fakeOceanMF = fakeOcean.AddComponent<MeshFilter>();
//			fakeOceanMF.mesh = fakeOceanMesh;
//			fakeOcean.layer = 15;
//
//
//			fakeOcean.transform.parent = m_manager.parentCelestialBody.transform;
//			
//			fakeOceanMR = fakeOcean.AddComponent<MeshRenderer>();
//
//			newMat = new Material (ShaderTool.GetMatFromShader2 ("BlackShader.shader"));
//			fakeOceanMR.sharedMaterial = newMat;
//			fakeOceanMR.material =newMat;
//
//			fakeOceanMR.castShadows = false;
//			fakeOceanMR.receiveShadows = false;

		}
		
		public virtual void OnDestroy ()
		{
			//			base.OnDestroy();
			for (int i = 0; i < numGrids; i++) {
//				Destroy(waterGameObjectsNear[i]);
//				Destroy(waterMeshFiltersNear[i]);
//				Destroy(waterMeshRenderersNear[i]);
//
//				Destroy(waterGameObjectsFar[i]);
//				Destroy(waterMeshFiltersFar[i]);
//				Destroy(waterMeshRenderersFar[i]);

				UnityEngine.Object.Destroy (m_screenGrids [i]);
			}
			UnityEngine.Object.Destroy (m_oceanMaterialNear);
			UnityEngine.Object.Destroy (m_oceanMaterialFar);

			Component.Destroy (oceanupdater);
			UnityEngine.Object.Destroy (oceanupdater);
		}
		
		Mesh MakePlane (int w, int h, float offset, float scale)
		{
			Vector3[] vertices = new Vector3[w * h];
			Vector2[] texcoords = new Vector2[w * h];
			Vector3[] normals = new Vector3[w * h];
			int[] indices = new int[w * h * 6];
			
			for (int x = 0; x < w; x++) {
				for (int y = 0; y < h; y++) {
					Vector2 uv = new Vector3 ((float)x / (float)(w - 1), (float)y / (float)(h - 1));
					
					uv.y *= scale;
					uv.y += offset;
					
					Vector2 p = new Vector2 ();
					p.x = (uv.x - 0.5f) * 2.0f;
					p.y = (uv.y - 0.5f) * 2.0f;
					
					Vector3 pos = new Vector3 (p.x, p.y, 0.0f);
					Vector3 norm = new Vector3 (0.0f, 0.0f, 1.0f);
					
					texcoords [x + y * w] = uv;
					vertices [x + y * w] = pos;
					normals [x + y * w] = norm;
				}
			}
			
			int num = 0;
			for (int x = 0; x < w - 1; x++) {
				for (int y = 0; y < h - 1; y++) {
					indices [num++] = x + y * w;
					indices [num++] = x + (y + 1) * w;
					indices [num++] = (x + 1) + y * w;
					
					indices [num++] = x + (y + 1) * w;
					indices [num++] = (x + 1) + (y + 1) * w;
					indices [num++] = (x + 1) + y * w;
				}
			}
			
			Mesh mesh = new Mesh ();
			
			mesh.vertices = vertices;
			mesh.uv = texcoords;
			mesh.triangles = indices;
			mesh.normals = normals;
			
			return mesh;
		}
		
		public virtual void UpdateNode ()
		{
			
			if ((m_manager.m_skyNode.farCamera)) {
				
				if (!oceanupdater) {
					oceanupdater = (OceanUpdateAtCameraRythm)m_manager.m_skyNode.farCamera.gameObject.AddComponent (typeof(OceanUpdateAtCameraRythm));
					oceanupdater.m_oceanNode = this;
					oceanupdater.farCamera = m_manager.m_skyNode.farCamera;
					oceanupdater.nearCamera = m_manager.m_skyNode.nearCamera;
					oceanupdater.oceanMaterialFar = m_oceanMaterialFar;
					oceanupdater.oceanMaterialNear = m_oceanMaterialNear;
					oceanupdater.m_manager = m_manager;
				}
			}


			m_drawOcean = m_manager.m_skyNode.trueAlt < fakeOceanAltitude;

//			if (!MapView.MapIsEnabled && !m_core.stockOcean && !m_manager.m_skyNode.inScaledSpace && (m_manager.m_skyNode.trueAlt < fakeOceanAltitude)) {
			if (!MapView.MapIsEnabled && !m_core.stockOcean && !m_manager.m_skyNode.inScaledSpace && m_drawOcean) {
				foreach (Mesh mesh in m_screenGrids) {
//					Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, m_oceanMaterialFar, 15, m_manager.m_skyNode.farCamera);
//					Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, m_oceanMaterialNear, 15, m_manager.m_skyNode.nearCamera);


					Graphics.DrawMesh (mesh, Vector3.zero, Quaternion.identity, m_oceanMaterialFar, 15,
					                  m_manager.m_skyNode.farCamera, 0, null, false, false);
					
					Graphics.DrawMesh (mesh, Vector3.zero, Quaternion.identity, m_oceanMaterialNear, 15,
					                  m_manager.m_skyNode.nearCamera, 0, null, false, false);

				}

//				Graphics.DrawMesh (fakeOceanMesh, Vector3.zero, Quaternion.identity, newMat, 15,
//				                   m_manager.m_skyNode.farCamera, 0, null, false, false);

			}


			if (!ocean && stockOceanExists) {  
				PQS pqs = m_manager.parentCelestialBody.pqsController;
//
////				Debug.Log("PQS.childspheres count"+pqs.ChildSpheres.Length);
//
//				
//				Debug.Log ("childspheres length"+pqs.ChildSpheres.Length.ToString());

				if (pqs.ChildSpheres [0]) {
//					Debug.Log("pqs.ChildSpheres [0] found");
					ocean = pqs.ChildSpheres [0];
					if (m_core.oceanCloudShadows)
					{
						emptyMaterial = new Material (ShaderTool.GetMatFromShader2 ("EmptyShader.shader"));
					}
					else
					{
						emptyMaterial = new Material (ShaderTool.GetMatFromShader2 ("EmptyShaderIgnoreProj.shader"));	
					}

//					fakeOceanMaterial = new Material (ShaderTool.GetMatFromShader2 ("CompiledFakeOcean.shader"));


//					m_manager.m_skyNode.InitUniforms(fakeOceanMaterial);
//					m_manager.m_skyNode.InitPostprocessMaterial(fakeOceanMaterial);

//					Debug.Log ("fake ocean mat set");
					ocean.surfaceMaterial = emptyMaterial;
					ocean.fallbackMaterial = emptyMaterial;

//					ocean.surfaceMaterial = fakeOceanMaterial;
//					ocean.fallbackMaterial = fakeOceanMaterial;

					ocean.useSharedMaterial=false;


//					oceanPC = new SimplePostProcessCube (20000, fakeOceanMaterial);
//					fakeOceanObject = oceanPC.GameObject;
//					fakeOceanObject.layer = 15;
//					fakeOceanMR = oceanPC.GameObject.GetComponent < MeshRenderer > ();
//					fakeOceanMR.material = fakeOceanMaterial;
//					oceanPC.GameObject.GetComponent < MeshFilter > ().mesh.Clear();
//					oceanPC.GameObject.GetComponent < MeshFilter > ().mesh = isosphere.Create(10000);
//
//					///Thanks to rbray89 for this snippet that disables the stock ocean in a clean way
//					GameObject container = ocean.gameObject;
//					
//					FakeOceanPQS fakeOcean1 = new GameObject().AddComponent<FakeOceanPQS>();
//
//					fakeOcean1.CloneFrom(ocean);
//					Destroy(ocean);
//
//					FakeOceanPQS fakeOcean = container.AddComponent<FakeOceanPQS>();
//					fakeOcean.CloneFrom(fakeOcean1);
//
//					Destroy(fakeOcean1);
//
//					FieldInfo field = typeof(PQS).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).First(
//						f => f.FieldType == typeof(PQS[]));
//					field.SetValue(pqs, new PQS[] {fakeOcean });
//					
//					PQSMod_CelestialBodyTransform cbt = pqs.GetComponentsInChildren<PQSMod_CelestialBodyTransform>()[0];
//					cbt.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[] { };

					stockOceanExists = false;


				} else {
					stockOceanExists = false;
					Debug.Log ("[Scatterer] Stock ocean doesn't exist for " + m_manager.parentCelestialBody.name);
				}
			}

			if (ocean)
			{
				ocean.surfaceMaterial = emptyMaterial;
				ocean.fallbackMaterial = emptyMaterial;
				ocean.useSharedMaterial = false;

//				ocean.surfaceMaterial = fakeOceanMaterial;
//				ocean.fallbackMaterial = fakeOceanMaterial;
//				ocean.useSharedMaterial = false;
				
//				fakeOceanMaterial.SetVector ("_planetPos", m_manager.parentCelestialBody.transform.position);
//				fakeOceanMaterial.SetVector ("_cameraPos", m_manager.GetCore().farCamera.transform.position - m_manager.parentCelestialBody.transform.position);
//				fakeOceanMaterial.SetVector ("_Ocean_Color", new Color (m_oceanUpwellingColor.x, m_oceanUpwellingColor.y, m_oceanUpwellingColor.z) * 0.1f);
//				fakeOceanMaterial.SetFloat ("_Ocean_Sigma", GetMaxSlopeVariance ());
////				fakeOceanMaterial.SetMatrix ("_PlanetToWorld", m_manager.parentCelestialBody.transform.localToWorldMatrix);
//
//
//
//
//
////				fakeOceanMaterial.SetMatrix ("_PlanetToWorld", camToLocal.ToMatrix4x4());
//				fakeOceanMaterial.SetMatrix ("_WorldToPlanet", m_manager.parentCelestialBody.transform.worldToLocalMatrix);
//
//
////				fakeOceanMaterial.SetVector ("SUN_DIR", m_manager.GetSunNodeDirection ());
//				m_manager.m_skyNode.SetUniforms(fakeOceanMaterial);
//				m_manager.m_skyNode.InitPostprocessMaterial(fakeOceanMaterial);
//				m_manager.m_skyNode.UpdatePostProcessMaterial(fakeOceanMaterial);
//
//
//				fakeOceanObject.transform.position= FlightGlobals.ActiveVessel.transform.position;
			}





			m_oceanMaterialNear.renderQueue = m_manager.GetCore ().oceanRenderQueue;
			m_oceanMaterialFar.renderQueue=m_manager.GetCore ().oceanRenderQueue;




			if (stockOceanExists) {

				//This causes problems later on
//				ocean.quadAllowBuild = false;
//				int deletedQuads=ocean.quads.Length;
//
//				if (deletedQuads>0){
//
//					for (int i=0;i<ocean.quads.Length;i++)
//					{
//						if (ocean.quads[i])
//							UnityEngine.Object.Destroy(ocean.quads[i]);
//					}
//
//					ocean.quads = Array.FindAll(ocean.quads, PQisNotNull);
////					deletedQuads-=ocean.quads.Length;
//
//						Debug.Log("[Scatterer] Destroyed "+deletedQuads.ToString()+" stock ocean quads on "
//					    	      +m_manager.parentCelestialBody.name);
//
//				}


//				ocean.quads= new PQ[10];

//				ocean.DeactivateSphere();
//				ocean.DisableSphere();
//				ocean.surfaceMaterial=new Material(ShaderTool.GetMatFromShader2("EmptyShader.shader"));
//				Debug.Log("ocean.subdivisionThreshold"+ ocean.subdivisionThreshold.ToString());
//				Debug.Log("ocean.maxDetailDistance"+ ocean.maxDetailDistance.ToString());



			}

//
////				ocean.quadAllowBuild=false;
//				Debug.Log("ocean.visRad"+ ocean.visRad);
//				Debug.Log("ocean.visibleRadius"+ ocean.visibleRadius.ToString());
//				Debug.Log("ocean.visibleAltitude"+ ocean.visibleAltitude.ToString());
////				Debug.Log("ocean.useSharedMaterial"+ ocean.useSharedMaterial.ToString());
////				Debug.Log("ocean.surfaceMaterial"+ ocean.surfaceMaterial.ToString());
//				Debug.Log("ocean.subdivisionThresholds"+ ocean.subdivisionThresholds.ToString());
//				Debug.Log("ocean.subdivisionThreshold"+ ocean.subdivisionThreshold.ToString());
////				Debug.Log("ocean.radiusMi"+ ocean.radiusMin.ToString());
////				Debug.Log("ocean.radiusMax"+ ocean.radiusMax.ToString());
////				Debug.Log("ocean.quadAllowBuild"+ ocean.quadAllowBuild.ToString());
//				Debug.Log("ocean.maxDetailDistance"+ ocean.maxDetailDistance.ToString());
//				Debug.Log("ocean.detailAltitudeMax"+ ocean.detailAltitudeMax.ToString());
////						}
						
						

//						ocean.isDisabled = true;
//						ocean.DisableSphere ();
//						ocean.isDisabled = !m_core.stockOcean;
//			
//						if (!m_core.stockOcean)
//						{
//							ocean.DisableSphere ();
//						}
//						else
//						{
//							ocean.EnableSphere ();
//						}
			
			
//						for(int i = 0; i < numGrids; i++)
//						{
//							waterMeshRenderersNear[i].enabled=!m_core.stockOcean && !MapView.MapIsEnabled;
//							waterMeshRenderersFar[i].enabled=!m_core.stockOcean && !MapView.MapIsEnabled ;
//						}





		}
		
		public void updateStuff (Material oceanMaterial, Camera inCamera)
		{
			//Calculates the required data for the projected grid
			
			// compute ltoo = localToOcean transform, where ocean frame = tangent space at
			// camera projection on sphere radius in local space
			
			Matrix4x4 ctol1 = inCamera.cameraToWorldMatrix;
			
			//position relative to kerbin
//			Vector3d tmp = (inCamera.transform.position) - m_manager.parentCelestialBody.transform.position;
			
			
			Matrix4x4d cameraToWorld = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, ctol1.m03,
			                                          ctol1.m10, ctol1.m11, ctol1.m12, ctol1.m13,
			                                          ctol1.m20, ctol1.m21, ctol1.m22, ctol1.m23,
			                                          ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);
			
			
			//Looking back, I have no idea how I figured this crap out
			Vector4 translation = m_manager.parentCelestialBody.transform.worldToLocalMatrix.inverse.GetColumn (3);
			
			Matrix4x4d worldToLocal = new Matrix4x4d (1, 0, 0, -translation.x,
			                                         0, 1, 0, -translation.y,
			                                         0, 0, 1, -translation.z,
			                                         0, 0, 0, 1);

			Matrix4x4d camToLocal = worldToLocal * cameraToWorld;


			// camera in local space relative to planet's origin
			Vector3d2 cl = new Vector3d2 ();
			cl = camToLocal * Vector3d2.Zero ();
			
//			double radius = m_manager.GetRadius ();
			double radius = m_manager.GetRadius ()+m_oceanLevel;

//			Vector3d2 ux, uy, uz, oo;
			
			uz = cl.Normalized (); // unit z vector of ocean frame, in local space
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ()) {
				ux = (new Vector3d2 (m_oldlocalToOcean.m [1, 0], m_oldlocalToOcean.m [1, 1], m_oldlocalToOcean.m [1, 2])).Cross (uz).Normalized ();
			} else {
				ux = Vector3d2.UnitZ ().Cross (uz).Normalized ();
			}

			uy = uz.Cross (ux); // unit y vector
			
			oo = uz * (radius); // origin of ocean frame, in local space
			
			
			//local to ocean transform
			//computed from oo and ux, uy, uz should be correct
			Matrix4x4d localToOcean = new Matrix4x4d (
				ux.x, ux.y, ux.z, -ux.Dot (oo),
				uy.x, uy.y, uy.z, -uy.Dot (oo),
				uz.x, uz.y, uz.z, -uz.Dot (oo),
				0.0, 0.0, 0.0, 1.0);


			Matrix4x4d cameraToOcean = localToOcean * camToLocal;


			//Couldn't figure out how to change the wind's direction in all that math so I tried to do the easy thing
			//And Rotated the ocean and the sun
			//This didn't work

			//deleted rotation code here



			
			Vector3d2 delta = new Vector3d2 (0, 0, 0);
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ()) {
				delta = localToOcean * (m_oldlocalToOcean.Inverse () * Vector3d2.Zero ());
				m_offset += delta;
			}
			
			m_oldlocalToOcean = localToOcean;
			
			Matrix4x4d ctos = ModifiedProjectionMatrix (inCamera);
			Matrix4x4d stoc = ctos.Inverse ();
			
			Vector3d2 oc = cameraToOcean * Vector3d2.Zero ();
			
			h = oc.z;
			
			Vector4d stoc_w = (stoc * Vector4d.UnitW ()).XYZ0 ();
			Vector4d stoc_x = (stoc * Vector4d.UnitX ()).XYZ0 ();
			Vector4d stoc_y = (stoc * Vector4d.UnitY ()).XYZ0 ();
			
			Vector3d2 A0 = (cameraToOcean * stoc_w).XYZ ();
			Vector3d2 dA = (cameraToOcean * stoc_x).XYZ ();
			Vector3d2 B = (cameraToOcean * stoc_y).XYZ ();
			
			Vector3d2 horizon1, horizon2;
			
//			Vector3d2 offset = new Vector3d2 (-m_offset.x, -m_offset.y, h);
			offset = new Vector3d2 (-m_offset.x, -m_offset.y, h);
//			Vector3d2 offset = new Vector3d2 (0f, 0f, h);
			
			double h1 = h * (h + 2.0 * radius);
			double h2 = (h + radius) * (h + radius);
			double alpha = B.Dot (B) * h1 - B.z * B.z * h2;
			
			double beta0 = (A0.Dot (B) * h1 - B.z * A0.z * h2) / alpha;
			double beta1 = (dA.Dot (B) * h1 - B.z * dA.z * h2) / alpha;
			
			double gamma0 = (A0.Dot (A0) * h1 - A0.z * A0.z * h2) / alpha;
			double gamma1 = (A0.Dot (dA) * h1 - A0.z * dA.z * h2) / alpha;
			double gamma2 = (dA.Dot (dA) * h1 - dA.z * dA.z * h2) / alpha;
			
			horizon1 = new Vector3d2 (-beta0, -beta1, 0.0);
			horizon2 = new Vector3d2 (beta0 * beta0 - gamma0, 2.0 * (beta0 * beta1 - gamma1), beta1 * beta1 - gamma2);
			
			Vector3d2 sunDir = new Vector3d2 (m_manager.getDirectionToSun ().normalized);
			Vector3d2 oceanSunDir = localToOcean.ToMatrix3x3d () * sunDir;
			
			oceanMaterial.SetVector ("_Ocean_SunDir", oceanSunDir.ToVector3 ());
			
			oceanMaterial.SetVector ("_Ocean_Horizon1", horizon1.ToVector3 ());
			oceanMaterial.SetVector ("_Ocean_Horizon2", horizon2.ToVector3 ());
			
			oceanMaterial.SetMatrix ("_Ocean_CameraToOcean", cameraToOcean.ToMatrix4x4 ());
			oceanMaterial.SetMatrix ("_Ocean_OceanToCamera", cameraToOcean.Inverse ().ToMatrix4x4 ());
			
			oceanMaterial.SetMatrix ("_Globals_CameraToScreen", ctos.ToMatrix4x4 ());
			oceanMaterial.SetMatrix ("_Globals_ScreenToCamera", stoc.ToMatrix4x4 ());
			
			oceanMaterial.SetVector ("_Ocean_CameraPos", offset.ToVector3 ());
			
			oceanMaterial.SetVector ("_Ocean_Color", new Color(m_oceanUpwellingColor.x,m_oceanUpwellingColor.y,m_oceanUpwellingColor.z) /*  *0.1f   */);
			oceanMaterial.SetVector ("_Ocean_ScreenGridSize", new Vector2 ((float)m_resolution / (float)Screen.width, (float)m_resolution / (float)Screen.height));
			oceanMaterial.SetFloat ("_Ocean_Radius", (float)radius);
			
			//			oceanMaterial.SetFloat("scale", 1);
			oceanMaterial.SetFloat ("scale", oceanScale);

			oceanMaterial.SetFloat ("_OceanAlpha", oceanAlpha);
			oceanMaterial.SetFloat ("alphaRadius", alphaRadius);


			oceanMaterial.SetFloat ("sunReflectionMultiplier", sunReflectionMultiplier);
			oceanMaterial.SetFloat ("skyReflectionMultiplier", skyReflectionMultiplier);
			oceanMaterial.SetFloat ("seaRefractionMultiplier", seaRefractionMultiplier);


			m_manager.GetSkyNode ().SetOceanUniforms (oceanMaterial);
			
		}
		
		public void SetUniforms (Material mat)
		{
			//Sets uniforms that this or other gameobjects may need
			if (mat == null)
				return;
			
			mat.SetFloat ("_Ocean_Sigma", GetMaxSlopeVariance ());
			mat.SetVector ("_Ocean_Color", new Color(m_oceanUpwellingColor.x,m_oceanUpwellingColor.y,m_oceanUpwellingColor.z) * 0.1f);
			mat.SetFloat ("fakeOcean", (m_drawOcean) ? 0.0f : 1.0f);
			
			
			mat.SetFloat ("_Ocean_Level", m_oceanLevel);
		}
		
		public void setManager (Manager manager)
		{
			m_manager = manager;
		}
		
		public void setCore (Core core)
		{
			m_core = core;
		}
		
		public Matrix4x4d ModifiedProjectionMatrix (Camera inCam)
		{
			/*
//			float h = (float)(GetHeight() - m_groundHeight);
//			camera.nearClipPlane = 0.1f * h;
//			camera.farClipPlane = 1e6f * h;
			
//			inCam.ResetProjectionMatrix();
			
			Matrix4x4 p = inCam.projectionMatrix;
//			bool d3d = SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
			
//			if(d3d) 
//			{
//				if(inCam.actualRenderingPath == RenderingPath.DeferredLighting)
//				{
//					// Invert Y for rendering to a render texture
//					for (int i = 0; i < 4; i++) {
//						p[1,i] = -p[1,i];
//					}
//				}
//				
//				// Scale and bias depth range
//				for (int i = 0; i < 4; i++) {
//					p[2,i] = p[2,i]*0.5f + p[3,i]*0.5f;
//				}
//			}
			
			Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d(p);
			inCam.projectionMatrix = m_cameraToScreenMatrix.ToMatrix4x4(); */
			
			Matrix4x4 p;
			//			if (debugSettings [2])
			//			if(!MapView.MapIsEnabled)
			//			{
			//			float tmpNearclip = inCam.nearClipPlane;
			//			float tmpFarclip = inCam.farClipPlane;
			//			
			//			inCam.nearClipPlane = m_manager.m_skyNode.oceanNearPlane;
			//			inCam.farClipPlane = m_manager.m_skyNode.oceanFarPlane;
			
			//			float h = (float)(GetHeight() - m_groundHeight);
			//			m_manager.GetCore ().chosenCamera.nearClipPlane = 0.1f * (alt - m_radius);
			//			m_manager.GetCore ().chosenCamera.farClipPlane = 1e6f * (alt - m_radius);
			
			p = inCam.projectionMatrix;
			
			{
				//				if(camera.actualRenderingPath == RenderingPath.DeferredLighting)
				//				{
				//					// Invert Y for rendering to a render texture
				//										for (int i = 0; i < 4; i++) {
				//											p[1,i] = -p[1,i];
				//										}
				//				}

				//if OpenGL isn't detected
				// Scale and bias depth range
				if (!m_core.opengl)
					for (int i = 0; i < 4; i++) {
						p [2, i] = p [2, i] * 0.5f + p [3, i] * 0.5f;
					}
			}
			
			//			p = scaledSpaceCamera.projectionMatrix;
			
			//			inCam.nearClipPlane=tmpNearclip;
			//			inCam.farClipPlane=tmpFarclip;
			
			//			p = scaledSpaceCamera.projectionMatrix;
			//			}
			//			else
			//			{
			//				p = scaledSpaceCamera.projectionMatrix;
			//			}
			
			
			Matrix4x4d m_cameraToScreenMatrix = new Matrix4x4d (p);
			
			
			return m_cameraToScreenMatrix;
		}

		public void saveToConfigNode ()
		{
			ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
			cnTemp.Save (m_manager.m_skyNode.assetDir + "/OceanSettings.cfg");
		}
		
		public void loadFromConfigNode (bool loadBackup)
		{
			ConfigNode cnToLoad;

			if (loadBackup)
			{
				cnToLoad = ConfigNode.Load (m_manager.m_skyNode.assetDir + "/OceanSettingsBackup.cfg");
			}

			else
			{
				cnToLoad = ConfigNode.Load (m_manager.m_skyNode.assetDir + "/OceanSettings.cfg");
			}
			ConfigNode.LoadObjectFromConfig (this, cnToLoad);
		}
		
		static bool PQisNotNull (PQ pq)
		{
			return pq;
		}

		static bool PQSisNotNull (PQS pqs)
		{
			return pqs;
		}
	}
}