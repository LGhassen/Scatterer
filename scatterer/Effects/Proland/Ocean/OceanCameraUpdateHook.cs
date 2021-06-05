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
	public class OceanCameraUpdateHook : MonoBehaviour
	{
		public OceanNode oceanNode;

		Matrix4x4 cameraToScreen,screenToCamera;
		Matrix4x4d m_oldlocalToOcean = Matrix4x4d.Identity ();

		// Whenever any camera will render us, call the method which updates the material with the right params
		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam || MapView.MapIsEnabled || !oceanNode.m_manager.m_skyNode.simulateOceanInteraction)
				return;

			updateCameraSpecificUniforms (oceanNode.m_oceanMaterial, cam);

			Utils.EnableOrDisableShaderKeywords (oceanNode.m_oceanMaterial, "REFRACTIONS_AND_TRANSPARENCY_ON", "REFRACTIONS_AND_TRANSPARENCY_OFF",
			                                     Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions && (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera));
		}

		public void updateCameraSpecificUniforms (Material oceanMaterial, Camera inCamera)
		{
			cameraToScreen = GL.GetGPUProjectionMatrix (inCamera.projectionMatrix, false);
			screenToCamera = cameraToScreen.inverse;
			
			oceanNode.m_oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, cameraToScreen);
			oceanNode.m_oceanMaterial.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, screenToCamera);
			
			
			//Calculates the required data for the projected grid
			
			// compute ltoo = localToOcean transform, where ocean frame = tangent space at
			// camera projection on sphere radius in local space
			
			//move these to dedicated projected grid class?
			
			Matrix4x4 ctol1 = inCamera.cameraToWorldMatrix;
			
			Matrix4x4d cameraToWorld = new Matrix4x4d (ctol1.m00, ctol1.m01, ctol1.m02, ctol1.m03,
			                                           ctol1.m10, ctol1.m11, ctol1.m12, ctol1.m13,
			                                           ctol1.m20, ctol1.m21, ctol1.m22, ctol1.m23,
			                                           ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);

			Vector3d translation;

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				translation = oceanNode.m_manager.parentLocalTransform.position;	//have to use this in space center or get the tsunami bug
			}
			else
			{
				translation = oceanNode.m_manager.parentCelestialBody.position;		//more precise, especially with RSS, but breaks a bit in KSC
			}

			Matrix4x4d worldToLocal = new Matrix4x4d(1, 0, 0, -translation.x,
			                                         0, 1, 0, -translation.y,
			                                         0, 0, 1, -translation.z,
			                                         0, 0, 0, 1);
			
			Matrix4x4d camToLocal = worldToLocal * cameraToWorld;
			Matrix4x4d localToCam = camToLocal.Inverse ();
			
			// camera in local space relative to planet's origin
			Vector3d2 cl = new Vector3d2 ();
			cl = camToLocal * Vector3d2.Zero ();
			
			double radius = oceanNode.m_manager.GetRadius ();
			
			oceanNode.uz = cl.Normalized (); // unit z vector of ocean frame, in local space
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ())
			{
				oceanNode.ux = (new Vector3d2 (m_oldlocalToOcean.m [1, 0], m_oldlocalToOcean.m [1, 1], m_oldlocalToOcean.m [1, 2])).Cross (oceanNode.uz).Normalized ();
			}
			else 
			{
				oceanNode.ux = Vector3d2.UnitZ ().Cross (oceanNode.uz).Normalized ();
			}
			
			oceanNode.uy = oceanNode.uz.Cross (oceanNode.ux); // unit y vector

			//Wind moves in -Ux direction, which by default points north for some reason, can rotate it to any desired direction this way

			oceanNode.oo = oceanNode.uz * (radius); // origin of ocean frame, in local space
			
			//local to ocean transform
			//computed from oo and ux, uy, uz should be correct
			Matrix4x4d localToOcean = new Matrix4x4d (
				oceanNode.ux.x, oceanNode.ux.y, oceanNode.ux.z, -oceanNode.ux.Dot (oceanNode.oo),
				oceanNode.uy.x, oceanNode.uy.y, oceanNode.uy.z, -oceanNode.uy.Dot (oceanNode.oo),
				oceanNode.uz.x, oceanNode.uz.y, oceanNode.uz.z, -oceanNode.uz.Dot (oceanNode.oo),
				0.0, 0.0, 0.0, 1.0);
			
			Matrix4x4d cameraToOcean = localToOcean * camToLocal;
			Matrix4x4d worldToOcean = localToOcean * worldToLocal;
			
			Vector3d2 delta = new Vector3d2 (0, 0, 0);
			
			if (m_oldlocalToOcean != Matrix4x4d.Identity ())
			{
				delta = localToOcean * (m_oldlocalToOcean.Inverse () * Vector3d2.Zero ());
				oceanNode.m_Offset += delta;
			}
			
			//reset offset when bigger than 20000 to  avoid floating point issues when later casting the offset to float
			if (Mathf.Max (Mathf.Abs ((float)oceanNode.m_Offset.x), Mathf.Abs ((float)oceanNode.m_Offset.y)) > 20000f)
			{
				oceanNode.m_Offset.x=0.0;
				oceanNode.m_Offset.y=0.0;
			}
			
			m_oldlocalToOcean = localToOcean;
			
			//			Matrix4x4d ctos = ModifiedProjectionMatrix (inCamera); //moved to command buffer
			//			Matrix4x4d stoc = ctos.Inverse ();
			
			Vector3d2 oc = cameraToOcean * Vector3d2.Zero ();
			oceanNode.height = oc.z;					
			
			oceanNode.offset = new Vector3d2 (-oceanNode.m_Offset.x, -oceanNode.m_Offset.y, oceanNode.height);
			
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
			
			Vector3d2 sunDir = new Vector3d2 (oceanNode.m_manager.getDirectionToSun ());
			Vector3d2 oceanSunDir = localToOcean.ToMatrix3x3d () * sunDir;
			
			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, cameraToWorld .ToMatrix4x4());
			
			oceanMaterial.SetVector (ShaderProperties._Ocean_SunDir_PROPERTY, oceanSunDir.ToVector3 ());
			
			oceanMaterial.SetMatrix (ShaderProperties._Ocean_CameraToOcean_PROPERTY, cameraToOcean.ToMatrix4x4 ());
			oceanMaterial.SetMatrix (ShaderProperties._Ocean_OceanToCamera_PROPERTY, cameraToOcean.Inverse ().ToMatrix4x4 ());
			
			//			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, ctos.ToMatrix4x4 ());
			//			oceanMaterial.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, stoc.ToMatrix4x4 ());
			
			oceanMaterial.SetMatrix (ShaderProperties._Globals_WorldToOcean_PROPERTY, worldToOcean.ToMatrix4x4 ());
			oceanMaterial.SetMatrix (ShaderProperties._Globals_OceanToWorld_PROPERTY, worldToOcean.Inverse ().ToMatrix4x4 ());
			
			oceanMaterial.SetVector (ShaderProperties._Ocean_CameraPos_PROPERTY, oceanNode.offset.ToVector3 ());
			
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
			//I think these should be moved to NonCameraSpecificUniforms
			if (Scatterer.Instance.mainSettings.usePlanetShine)
			{
				Matrix4x4 planetShineSourcesMatrix=oceanNode.m_manager.m_skyNode.planetShineSourcesMatrix;
				
				Vector3d2 oceanSunDir2;
				for (int i=0;i<4;i++)
				{
					Vector4 row = planetShineSourcesMatrix.GetRow(i);
					oceanSunDir2=localToOcean.ToMatrix3x3d () * new Vector3d2(row.x,row.y,row.z);
					planetShineSourcesMatrix.SetRow(i,new Vector4((float)oceanSunDir2.x,(float)oceanSunDir2.y,(float)oceanSunDir2.z,row.w));
				}
				oceanMaterial.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, planetShineSourcesMatrix); //this can become shared code to not recompute
				
				oceanMaterial.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, oceanNode.m_manager.m_skyNode.planetShineRGBMatrix);
			}

			Matrix4x4 worldToLightMatrix = oceanNode.m_manager.mainSunLight.transform.worldToLocalMatrix;
			if (oceanNode.m_manager.parentCelestialBody.transform.position.sqrMagnitude < oceanNode.m_manager.mainSunLight.transform.position.sqrMagnitude)
			{
				worldToLightMatrix.m03 = oceanNode.m_manager.parentCelestialBody.transform.position.x;
				worldToLightMatrix.m13 = oceanNode.m_manager.parentCelestialBody.transform.position.y;
				worldToLightMatrix.m23 = oceanNode.m_manager.parentCelestialBody.transform.position.z;
			}

			if (!ReferenceEquals (oceanNode.causticsShadowMaskModulator, null))
			{
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetMatrix (ShaderProperties.CameraToWorld_PROPERTY, inCamera.cameraToWorldMatrix);
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetMatrix (ShaderProperties.WorldToLight_PROPERTY, worldToLightMatrix);
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetVector (ShaderProperties.PlanetOrigin_PROPERTY, oceanNode.m_manager.parentLocalTransform.position);
			}

			if (!ReferenceEquals (oceanNode.causticsLightRaysRenderer, null))
			{
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetMatrix (ShaderProperties.CameraToWorld_PROPERTY, inCamera.cameraToWorldMatrix);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetMatrix (ShaderProperties.WorldToLight_PROPERTY, worldToLightMatrix);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetVector (ShaderProperties.LightDir_PROPERTY, oceanNode.m_manager.mainSunLight.transform.forward);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetVector (ShaderProperties.PlanetOrigin_PROPERTY, oceanNode.m_manager.parentLocalTransform.position);
			}
		}

		public void OnDestroy()
		{
		}
	}
}