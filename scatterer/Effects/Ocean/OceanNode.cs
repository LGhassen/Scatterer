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
using UnityEngine.Rendering;

using KSP.IO;

namespace scatterer
{
	/*
	 * An AbstractTask to draw a flat or spherical ocean.
	 * This class provides the functions and data to draw a flat projected grid but nothing else
	 */
	public abstract class OceanNode: MonoBehaviour
	{
		public UrlDir.UrlConfig configUrl;
		
		public Manager m_manager;

		public Material m_oceanMaterial;

		[Persistent]
		public Vector3 m_oceanUpwellingColor = new Vector3 (0.0039f, 0.0156f, 0.047f);

		[Persistent]
		public Vector3 m_UnderwaterColor = new Vector3 (0.1f, 0.75f, 0.8f);

		double h = 0;
		
		//Size of each grid in the projected grid. (number of pixels on screen)		
		[Persistent]
		public int m_resolution = 4;
		[Persistent]
		public int MAX_VERTS = 65000;
		[Persistent]
		public float oceanScale = 1f;
		[Persistent]
		public float oceanAlpha = 1f;

		[Persistent]
		public float alphaRadius = 3000f;

		[Persistent]
		public float transparencyDepth = 60f;

		[Persistent]
		public float darknessDepth = 1000f;

		[Persistent]
		public float refractionIndex = 1.33f;

		public bool isUnderwater = false;
		bool underwaterMode = false;

		public int numGrids;
		Mesh[] m_screenGrids;

		GameObject[] waterGameObjects;
		public MeshRenderer[] waterMeshRenderers;
		MeshFilter[] waterMeshFilters;

		public AtmosphereProjector underwaterProjector;
		Material underwaterMaterial;

		Matrix4x4d m_oldlocalToOcean;

		public Vector3 offsetVector3{
			get {
				return offset.ToVector3();
			}
		}

		OceanCameraUpdateHook oceanCameraProjectionMatModifier;
		CommandBuffer oceanRefractionCommandBuffer;

		Vector3d2 m_offset;
		public Vector3d2 offset;

		public Vector3d2 ux, uy, uz, oo;

		Matrix4x4 ctos;
		Matrix4x4 stoc;

		public float planetOpacity=1f; //planetOpacity to fade out the ocean when PQS is fading out

		//Concrete classes must provide a function that returns the
		//variance of the waves need for the BRDF rendering of waves
		public abstract float GetMaxSlopeVariance ();
		UnderwaterDimmingHook underwaterDimmingHook;

		// Use this for initialization
		public virtual void Init ()
		{

			if (Core.Instance.mainSettings.oceanPixelLights)
				m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[ ("Scatterer/OceanWhiteCapsPixelLights")]);
			else
				m_oceanMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[ ("Scatterer/OceanWhiteCaps")]);

			if (Core.Instance.mainSettings.oceanSkyReflections)
			{
				m_oceanMaterial.EnableKeyword ("SKY_REFLECTIONS_ON");
				m_oceanMaterial.DisableKeyword ("SKY_REFLECTIONS_OFF");
			}
			else
			{
				m_oceanMaterial.EnableKeyword ("SKY_REFLECTIONS_OFF");
				m_oceanMaterial.DisableKeyword ("SKY_REFLECTIONS_ON");
			}

			if (Core.Instance.mainSettings.usePlanetShine)
			{
				m_oceanMaterial.EnableKeyword ("PLANETSHINE_ON");
				m_oceanMaterial.DisableKeyword ("PLANETSHINE_OFF");
			}
			else
			{
				m_oceanMaterial.DisableKeyword ("PLANETSHINE_ON");
				m_oceanMaterial.EnableKeyword ("PLANETSHINE_OFF");
			}

			if (Core.Instance.mainSettings.oceanRefraction)
			{
				m_oceanMaterial.EnableKeyword ("REFRACTION_ON");
				m_oceanMaterial.DisableKeyword ("REFRACTION_OFF");
			}
			else
			{
				m_oceanMaterial.EnableKeyword ("REFRACTION_OFF");
				m_oceanMaterial.DisableKeyword ("REFRACTION_ON");
			}

			if (Core.Instance.mainSettings.shadowsOnOcean && (QualitySettings.shadows != ShadowQuality.Disable))
			{
				if (QualitySettings.shadows == ShadowQuality.HardOnly)
				{
					m_oceanMaterial.EnableKeyword ("OCEAN_SHADOWS_HARD");
					m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_SOFT");
				}
				else
				{
					m_oceanMaterial.EnableKeyword ("OCEAN_SHADOWS_SOFT");
					m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_HARD");
				}
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_OFF");
			}
			else
			{
				m_oceanMaterial.EnableKeyword ("OCEAN_SHADOWS_OFF");
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_HARD");
				m_oceanMaterial.DisableKeyword ("OCEAN_SHADOWS_SOFT");
			}

			m_oceanMaterial.SetOverrideTag ("IgnoreProjector", "True");

			m_manager.GetSkyNode ().InitUniforms (m_oceanMaterial);

			m_oceanMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Core.Instance.bufferRenderingManager.depthTexture);

			//if (Core.Instance.oceanRefraction)
			m_oceanMaterial.SetTexture ("_BackgroundTexture", Core.Instance.bufferRenderingManager.refractionTexture);
			m_oceanMaterial.renderQueue=2501;

			m_oldlocalToOcean = Matrix4x4d.Identity ();

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

			waterGameObjects = new GameObject[numGrids];
			waterMeshRenderers = new MeshRenderer[numGrids];
			waterMeshFilters = new MeshFilter[numGrids];

			//Make the meshes. The end product will be a grid of verts that cover
			//the screen on the x and y axis with the z depth at 0. This grid is then
			//projected as the ocean by the shader
			for (int i = 0; i < numGrids; i++)
			{
				NY = Screen.height / numGrids / m_resolution;
				
				m_screenGrids [i] = MakePlane (NX, NY, (float)i / (float)numGrids, 1.0f / (float)numGrids);
				m_screenGrids [i].bounds = new Bounds (Vector3.zero, new Vector3 (1e8f, 1e8f, 1e8f));	


				waterGameObjects[i] = new GameObject();
				waterGameObjects[i].transform.parent=m_manager.parentCelestialBody.transform; //might be redundant
				waterMeshFilters[i] = waterGameObjects[i].AddComponent<MeshFilter>();
				waterMeshFilters[i].mesh.Clear ();
				waterMeshFilters[i].mesh = m_screenGrids[i];

				waterGameObjects[i].layer = 15;
				waterMeshRenderers[i] = waterGameObjects[i].AddComponent<MeshRenderer>();

				waterMeshRenderers[i].sharedMaterial = m_oceanMaterial;
				waterMeshRenderers[i].material =m_oceanMaterial;

				waterMeshRenderers[i].receiveShadows = false;
				waterMeshRenderers[i].shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;

				waterMeshRenderers[i].enabled=true;
			}

			oceanCameraProjectionMatModifier = waterGameObjects[0].AddComponent<OceanCameraUpdateHook>();
			oceanCameraProjectionMatModifier.oceanNode = this;

			underwaterMaterial = new Material (ShaderReplacer.Instance.LoadedShaders[("Scatterer/UnderwaterScatterProjector")]);
			m_manager.GetSkyNode ().InitPostprocessMaterial (underwaterMaterial);
			m_manager.GetSkyNode ().InitPostprocessMaterial (m_oceanMaterial);

			underwaterMaterial.renderQueue=2502; //draw over fairings which is 2450 and over ocean which is 2501

			underwaterProjector = new AtmosphereProjector(underwaterMaterial,m_manager.parentLocalTransform,(float)m_manager.m_radius);
			underwaterProjector.setActivated(false);

			initUniforms ();

			//move this to separate class and make it work on every camera

			//refraction command buffer
			oceanRefractionCommandBuffer = new CommandBuffer();
			oceanRefractionCommandBuffer.name = "ScattererOceanGrabScreen";
			
			// copy screen
			oceanRefractionCommandBuffer.Blit (BuiltinRenderTextureType.CurrentActive, Core.Instance.bufferRenderingManager.refractionTexture);
			
			Core.Instance.farCamera.AddCommandBuffer  (CameraEvent.AfterForwardOpaque, oceanRefractionCommandBuffer);
			Core.Instance.nearCamera.AddCommandBuffer (CameraEvent.AfterForwardOpaque, oceanRefractionCommandBuffer);

			//dimming
			if (Core.Instance.mainSettings.underwaterLightDimming && (HighLogic.LoadedScene != GameScenes.MAINMENU))
			{
				underwaterDimmingHook = (UnderwaterDimmingHook) Core.Instance.scaledSpaceCamera.gameObject.AddComponent(typeof(UnderwaterDimmingHook));
				underwaterDimmingHook.oceanNode = this;
			}
		}
		
		public virtual void Cleanup ()
		{
			Utils.LogDebug ("ocean node Cleanup");

			if (oceanCameraProjectionMatModifier)
			{
				oceanCameraProjectionMatModifier.OnDestroy ();
				Component.Destroy (oceanCameraProjectionMatModifier);
				UnityEngine.Object.Destroy (oceanCameraProjectionMatModifier);
			}

			if (!ReferenceEquals(oceanRefractionCommandBuffer,null))
			{
				Core.Instance.farCamera.RemoveCommandBuffer  (CameraEvent.AfterForwardOpaque, oceanRefractionCommandBuffer);
				Core.Instance.nearCamera.RemoveCommandBuffer (CameraEvent.AfterForwardOpaque, oceanRefractionCommandBuffer);
			}

			//			base.OnDestroy();
			for (int i = 0; i < numGrids; i++)
			{
				Destroy(waterGameObjects[i]);
				Component.Destroy(waterMeshFilters[i]);
				Component.Destroy(waterMeshRenderers[i]);

				UnityEngine.Object.Destroy (m_screenGrids [i]);
			}


			UnityEngine.Object.Destroy (m_oceanMaterial);
			UnityEngine.Object.Destroy (underwaterMaterial);

			if (underwaterDimmingHook)
				Component.Destroy (underwaterDimmingHook);

			if (!ReferenceEquals(null,underwaterProjector))
			{
				UnityEngine.Object.Destroy (underwaterProjector);
			}
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
			bool oceanDraw = !MapView.MapIsEnabled && !m_manager.m_skyNode.inScaledSpace && (planetOpacity > 0f);

			foreach (MeshRenderer _mr in waterMeshRenderers)
			{
				_mr.enabled= oceanDraw;
			}

			isUnderwater = ((Core.Instance.farCamera.transform.position - m_manager.parentLocalTransform.position).magnitude -(float)m_manager.m_radius) < 0f;

			underwaterProjector.projector.enabled = isUnderwater;

			if (underwaterMode ^ isUnderwater)
			{
				toggleUnderwaterMode();
			}
		}

		public void initUniforms()
		{
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_Color_PROPERTY, m_oceanUpwellingColor);
			m_oceanMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			m_oceanMaterial.SetVector (ShaderProperties._Ocean_ScreenGridSize_PROPERTY, new Vector2 ((float)m_resolution / (float)Screen.width, (float)m_resolution / (float)Screen.height));
			//oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(radius+m_oceanLevel));
			m_oceanMaterial.SetFloat (ShaderProperties._Ocean_Radius_PROPERTY, (float)(m_manager.GetRadius()));
			
			m_oceanMaterial.SetFloat (ShaderProperties._OceanAlpha_PROPERTY, oceanAlpha);
			m_oceanMaterial.SetFloat (ShaderProperties.alphaRadius_PROPERTY, alphaRadius);
			
			m_oceanMaterial.SetFloat ("refractionIndex", refractionIndex); //these don't need to be updated every frame
			m_oceanMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			m_oceanMaterial.SetFloat ("darknessDepth", darknessDepth);					
			m_oceanMaterial.SetTexture (ShaderProperties._customDepthTexture_PROPERTY, Core.Instance.bufferRenderingManager.depthTexture);
			m_oceanMaterial.SetTexture ("_BackgroundTexture", Core.Instance.bufferRenderingManager.refractionTexture); //these don't need to be updated every frame
			
			underwaterMaterial.SetFloat ("transparencyDepth", transparencyDepth);
			underwaterMaterial.SetFloat ("darknessDepth", darknessDepth);
			underwaterMaterial.SetVector ("_Underwater_Color", m_UnderwaterColor);
			underwaterMaterial.SetFloat ("Rg",(float)m_manager.m_radius);
			
			float camerasOverlap = Core.Instance.nearCamera.farClipPlane - Core.Instance.farCamera.nearClipPlane;
			m_oceanMaterial.SetFloat("_ScattererCameraOverlap",camerasOverlap);
		}

		public void OnPreCull() //OnPreCull of OceanNode (added to farCamera) executes after OnPreCull of SkyNode (added to ScaledSpaceCamera, executes first)
		{
			if (!MapView.MapIsEnabled && Core.Instance.farCamera && !m_manager.m_skyNode.inScaledSpace)
			{
				updateNonCameraSpecificUniforms(m_oceanMaterial);
			}
		}

		public void updateNonCameraSpecificUniforms (Material oceanMaterial)
		{
			m_manager.GetSkyNode ().SetOceanUniforms (oceanMaterial);

			if (underwaterMode)
			{
				m_manager.GetSkyNode ().UpdatePostProcessMaterial (underwaterMaterial);
			}
			
			planetOpacity = 1f - m_manager.parentCelestialBody.pqsController.surfaceMaterial.GetFloat ("_PlanetOpacity");
			m_oceanMaterial.SetFloat ("_PlanetOpacity", planetOpacity);
			
			m_oceanMaterial.SetInt ("_ZwriteVariable", (planetOpacity == 1) ? 1 : 0); //if planetOpacity!=1, ie fading out the sea, disable scattering on it and enable the projector scattering, for the projector scattering to work need to disable zwrite
		}

		public void updateCameraSpecificUniforms (Material oceanMaterial, Camera inCamera)
		{
			ctos = GL.GetGPUProjectionMatrix (inCamera.projectionMatrix,false);
			stoc = ctos.inverse;
			
			m_oceanMaterial.SetMatrix ("_Globals_CameraToScreen", ctos);
			m_oceanMaterial.SetMatrix ("_Globals_ScreenToCamera", stoc);


			//Calculates the required data for the projected grid
			
			// compute ltoo = localToOcean transform, where ocean frame = tangent space at
			// camera projection on sphere radius in local space
			
			Matrix4x4 ctol1 = inCamera.cameraToWorldMatrix;

			Matrix4x4d cameraToWorld = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, ctol1.m03,
			                                          ctol1.m10, ctol1.m11, ctol1.m12, ctol1.m13,
			                                          ctol1.m20, ctol1.m21, ctol1.m22, ctol1.m23,
			                                          ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);

			Vector3d translation = m_manager.parentLocalTransform.position;


			Matrix4x4d worldToLocal = new Matrix4x4d(1, 0, 0, -translation.x,
			                                         0, 1, 0, -translation.y,
			                                         0, 0, 1, -translation.z,
			                                         0, 0, 0, 1);

			Matrix4x4d camToLocal = worldToLocal * cameraToWorld;
			Matrix4x4d localToCam = camToLocal.Inverse ();

			// camera in local space relative to planet's origin
			Vector3d2 cl = new Vector3d2 ();
			cl = camToLocal * Vector3d2.Zero ();

			double radius = m_manager.GetRadius ();

			uz = cl.Normalized (); // unit z vector of ocean frame, in local space
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ())
			{
				ux = (new Vector3d2 (m_oldlocalToOcean.m [1, 0], m_oldlocalToOcean.m [1, 1], m_oldlocalToOcean.m [1, 2])).Cross (uz).Normalized ();
			}
			else 
			{
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
			Matrix4x4d worldToOcean = localToOcean * worldToLocal;

			Vector3d2 delta = new Vector3d2 (0, 0, 0);
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ())
			{
				delta = localToOcean * (m_oldlocalToOcean.Inverse () * Vector3d2.Zero ());
				m_offset += delta;
			}

			//reset offset when bigger than 20000 to  avoid floating point issues when later casting the offset to float
			if (Mathf.Max (Mathf.Abs ((float)m_offset.x), Mathf.Abs ((float)m_offset.y)) > 20000f)
			{
				m_offset.x=0.0;
				m_offset.y=0.0;
			}

			m_oldlocalToOcean = localToOcean;
			
//			Matrix4x4d ctos = ModifiedProjectionMatrix (inCamera); //moved to command buffer
//			Matrix4x4d stoc = ctos.Inverse ();
			
			Vector3d2 oc = cameraToOcean * Vector3d2.Zero ();
			h = oc.z;					

			offset = new Vector3d2 (-m_offset.x, -m_offset.y, h);

			//old horizon code
			//This breaks down when you tilt the camera by 90 degrees in any direction
			//I made some new horizon code down, scroll down

//			Vector4d stoc_w = (stoc * Vector4d.UnitW ()).XYZ0 ();
//			Vector4d stoc_x = (stoc * Vector4d.UnitX ()).XYZ0 ();
//			Vector4d stoc_y = (stoc * Vector4d.UnitY ()).XYZ0 ();
//			
//			Vector3d2 A0 = (cameraToOcean * stoc_w).XYZ ();  
//			Vector3d2 dA = (cameraToOcean * stoc_x).XYZ ();
//			Vector3d2 B = (cameraToOcean * stoc_y).XYZ ();
//
//			Vector3d2 horizon1, horizon2;
//
//			double h1 = h * (h + 2.0 * radius);
//			double h2 = (h + radius) * (h + radius);
//			double alpha = B.Dot (B) * h1 - B.z * B.z * h2;
//
//			double beta0 = (A0.Dot (B) * h1 - B.z * A0.z * h2) / alpha;
//			double beta1 = (dA.Dot (B) * h1 - B.z * dA.z * h2) / alpha;
//			
//			double gamma0 = (A0.Dot (A0) * h1 - A0.z * A0.z * h2) / alpha;
//			double gamma1 = (A0.Dot (dA) * h1 - A0.z * dA.z * h2) / alpha;
//			double gamma2 = (dA.Dot (dA) * h1 - dA.z * dA.z * h2) / alpha;
//			
//			horizon1 = new Vector3d2 (-beta0, -beta1, 0.0);
//			horizon2 = new Vector3d2 (beta0 * beta0 - gamma0, 2.0 * (beta0 * beta1 - gamma1), beta1 * beta1 - gamma2);
			
			Vector3d2 sunDir = new Vector3d2 (m_manager.getDirectionToSun ().normalized);
			Vector3d2 oceanSunDir = localToOcean.ToMatrix3x3d () * sunDir;

			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, cameraToWorld .ToMatrix4x4());

			oceanMaterial.SetVector (ShaderProperties._Ocean_SunDir_PROPERTY, oceanSunDir.ToVector3 ());
			
			oceanMaterial.SetMatrix (ShaderProperties._Ocean_CameraToOcean_PROPERTY, cameraToOcean.ToMatrix4x4 ());
			oceanMaterial.SetMatrix (ShaderProperties._Ocean_OceanToCamera_PROPERTY, cameraToOcean.Inverse ().ToMatrix4x4 ());
			
//			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, ctos.ToMatrix4x4 ());
//			oceanMaterial.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, stoc.ToMatrix4x4 ());

			oceanMaterial.SetMatrix (ShaderProperties._Globals_WorldToOcean_PROPERTY, worldToOcean.ToMatrix4x4 ());
			oceanMaterial.SetMatrix (ShaderProperties._Globals_OceanToWorld_PROPERTY, worldToOcean.Inverse ().ToMatrix4x4 ());

			oceanMaterial.SetVector (ShaderProperties._Ocean_CameraPos_PROPERTY, offset.ToVector3 ());

			//horizon calculations
			//these are used to find where the horizon line is on screen
			//and "clamp" vertexes that are above it back to it
			//as the grid is projected on the whole screen, vertexes over the horizon need to be dealt with
			//simply passing a flag to drop fragments or moving these vertexes offscreen will cause issues
			//as the horizon line can be between two vertexes and the horizon line will appear "pixelated"
			//as whole chunks go missing

			//these need to be done here
			//1)for double precision
			//2)for speed

			Vector3d2 sphereDir=localToCam * Vector3d2.Zero ();  //vector to center of planet			
			double OHL = sphereDir.Magnitude ();         		 //distance to center of planet
			sphereDir = sphereDir.Normalized ();		 		 //direction to center of planet

			double rHorizon = Math.Sqrt( (OHL)*(OHL) - (radius * radius));  //distance to the horizon, i.e distance to ocean sphere tangent
																			//basic geometry yo

			//Theta=angle to horizon, now all that is left to do is check the viewdir against this angle in the shader
			double cosTheta= rHorizon / (OHL); 
			double sinTheta= Math.Sqrt (1- cosTheta*cosTheta);

			oceanMaterial.SetVector (ShaderProperties.sphereDir_PROPERTY, sphereDir.ToVector3 ());
			oceanMaterial.SetFloat (ShaderProperties.cosTheta_PROPERTY, (float) cosTheta);
			oceanMaterial.SetFloat (ShaderProperties.sinTheta_PROPERTY, (float) sinTheta);

			//planetshine properties
			if (Core.Instance.mainSettings.usePlanetShine)
			{
				Matrix4x4 planetShineSourcesMatrix=m_manager.m_skyNode.planetShineSourcesMatrix;

				Vector3d2 oceanSunDir2;
				for (int i=0;i<4;i++)
				{
					Vector4 row = planetShineSourcesMatrix.GetRow(i);
					oceanSunDir2=localToOcean.ToMatrix3x3d () * new Vector3d2(row.x,row.y,row.z);
					planetShineSourcesMatrix.SetRow(i,new Vector4((float)oceanSunDir2.x,(float)oceanSunDir2.y,(float)oceanSunDir2.z,row.w));
				}
				oceanMaterial.SetMatrix ("planetShineSources", planetShineSourcesMatrix); //this can become shared code to not recompute

				oceanMaterial.SetMatrix ("planetShineRGB", m_manager.m_skyNode.planetShineRGBMatrix);
			}
		}

		void toggleUnderwaterMode()
		{
			if (underwaterMode) //switch to over water
			{
				underwaterProjector.setActivated(false);
				underwaterProjector.updateProjector ();
				m_manager.GetSkyNode().localScatteringProjector.setUnderwater(false);
				m_oceanMaterial.EnableKeyword("UNDERWATER_OFF");
				m_oceanMaterial.DisableKeyword("UNDERWATER_ON");

			}
			else   //switch to underwater 
			{
				underwaterProjector.setActivated(true);
				underwaterProjector.updateProjector ();
				m_manager.GetSkyNode().localScatteringProjector.setUnderwater(true);
				m_oceanMaterial.EnableKeyword("UNDERWATER_ON");
				m_oceanMaterial.DisableKeyword("UNDERWATER_OFF");
			}

			underwaterMode = !underwaterMode;
		}

		public void applyUnderwaterDimming () //called OnPostRender of scaledSpace Camera by hook, needs to be done before farCamera onPreCull where the color is set
		{
			if (!MapView.MapIsEnabled && isUnderwater)
			{
				float underwaterDim = Mathf.Abs(Vector3.Distance (Core.Instance.farCamera.transform.position, m_manager.parentLocalTransform.position)-(float)m_manager.m_radius);
				underwaterDim = Mathf.Lerp(1.0f,0.0f,underwaterDim / darknessDepth);
				Core.Instance.sunlightModulatorInstance.modulateByAttenuation(underwaterDim);
			}	
		}

		public void setManager (Manager manager)
		{
			m_manager = manager;
		}

		public void saveToConfigNode ()
		{
			ConfigNode[] configNodeArray;
			bool found = false;
			
			configNodeArray = configUrl.config.GetNodes("Ocean");
			
			foreach(ConfigNode _cn in configNodeArray)
			{
				if (_cn.HasValue("name") && _cn.GetValue("name") == m_manager.parentCelestialBody.name)
				{
					ConfigNode cnTemp = ConfigNode.CreateConfigFromObject (this);
					_cn.ClearData();
					ConfigNode.Merge (_cn, cnTemp);
					_cn.name="Ocean";
					Utils.LogDebug("saving "+m_manager.parentCelestialBody.name+
					          " ocean config to: "+configUrl.parent.url);
					configUrl.parent.SaveConfigs ();
					found=true;
					break;
				}
			}
			
			if (!found)
			{
				Utils.LogDebug("couldn't find config file to save to");
			}
		}
		
		public void loadFromConfigNode ()
		{
			ConfigNode cnToLoad = new ConfigNode();
			ConfigNode[] configNodeArray;
			bool found = false;

			foreach (UrlDir.UrlConfig _url in Core.Instance.oceanConfigs)
			{
				configNodeArray = _url.config.GetNodes("Ocean");
				
				foreach(ConfigNode _cn in configNodeArray)
				{
					if (_cn.HasValue("name") && _cn.GetValue("name") == m_manager.parentCelestialBody.name)
					{
						cnToLoad = _cn;
						configUrl = _url;
						found = true;
						break;
					}
				}
			}
			
			if (found)
			{
				Utils.LogDebug("Ocean config found for: "+m_manager.parentCelestialBody.name);
				
				ConfigNode.LoadObjectFromConfig (this, cnToLoad);		
			}
			else
			{
				Utils.LogDebug("Ocean config not found for: "+m_manager.parentCelestialBody.name);
				Utils.LogDebug("Removing ocean for "+m_manager.parentCelestialBody.name +" from planets list");
				
				(Core.Instance.scattererCelestialBodies.Find(_cb => _cb.celestialBodyName == m_manager.parentCelestialBody.name)).hasOcean = false;
				
				this.Cleanup();
				UnityEngine.Object.Destroy (this);
			}
		}

		public void setWaterMeshrenderersEnabled (bool enabled)
		{
			for (int i=0; i < numGrids; i++)
			{
				waterMeshRenderers[i].enabled=enabled;
			}
		}
	}
}