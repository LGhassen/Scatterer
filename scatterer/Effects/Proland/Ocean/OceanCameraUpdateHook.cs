using UnityEngine;
using System;

namespace Scatterer
{
	public class OceanCameraUpdateHook : MonoBehaviour
	{
		public OceanNode oceanNode;

		Matrix4x4 cameraToScreen,screenToCamera;
		Matrix4x4d m_oldlocalToOcean = Matrix4x4d.Identity ();
		bool m_oldlocalToOceanValid = false; // replaces the per-frame value-compare against a freshly allocated Matrix4x4d.Identity()

		// Cached math objects. Matrix4x4d/Vector3d2 are classes, and this hook previously allocated
		// ~30 of them per camera per frame (matrices, inverses, operator temporaries) - measured as
		// ~4 KB/frame of pure GC pressure on a coastal scene. All formulas below are unchanged; only
		// the allocation strategy changed to reuse these instances via the non-allocating Set/Multiply/
		// InverseInto/MultiplyPoint/CrossInto variants.
		readonly Matrix4x4d cameraToWorldD = Matrix4x4d.Identity ();
		readonly Matrix4x4d worldToLocalD = Matrix4x4d.Identity ();
		readonly Matrix4x4d camToLocalD = Matrix4x4d.Identity ();
		readonly Matrix4x4d localToCamD = Matrix4x4d.Identity ();
		readonly Matrix4x4d localToOceanD = Matrix4x4d.Identity ();
		readonly Matrix4x4d cameraToOceanD = Matrix4x4d.Identity ();
		readonly Matrix4x4d worldToOceanD = Matrix4x4d.Identity ();
		readonly Matrix4x4d inverseScratchD = Matrix4x4d.Identity ();
		readonly Vector3d2 zeroD = new Vector3d2 (0, 0, 0); // stands in for Vector3d2.Zero(); never mutated
		readonly Vector3d2 scratchA = new Vector3d2 ();
		readonly Vector3d2 scratchB = new Vector3d2 ();
		readonly Vector3d2 deltaD = new Vector3d2 ();
		readonly Vector3d2 sphereDirD = new Vector3d2 ();
		readonly Vector3d2 sunDirD = new Vector3d2 ();
		readonly Vector3d2 oceanSunDirD = new Vector3d2 ();
		readonly Vector3d2 camPosLocalD = new Vector3d2 ();

		// Whenever any camera will render us, call the method which updates the material with the right params
		public void OnWillRenderObject()
		{
			Camera cam = Camera.current;
			if (!cam || MapView.MapIsEnabled || !oceanNode.prolandManager.skyNode.simulateOceanInteraction)
				return;

			updateCameraSpecificUniforms (oceanNode.m_oceanMaterial, cam);

			if (Scatterer.Instance.mainSettings.oceanTransparencyAndRefractions && (cam == Scatterer.Instance.farCamera || cam == Scatterer.Instance.nearCamera))
			{
				if (Scatterer.Instance.unifiedCameraMode)
				{
					oceanNode.m_oceanMaterial.EnableKeyword("REFRACTIONS_AND_TRANSPARENCY_ON");
					oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_MERGED_DEPTH");
				}
				else
				{
					oceanNode.m_oceanMaterial.EnableKeyword("REFRACTIONS_AND_TRANSPARENCY_MERGED_DEPTH");
					oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_ON");
				}
				oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_OFF");
			}
			else
			{
				oceanNode.m_oceanMaterial.EnableKeyword("REFRACTIONS_AND_TRANSPARENCY_OFF");
				oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_ON");
				oceanNode.m_oceanMaterial.DisableKeyword("REFRACTIONS_AND_TRANSPARENCY_MERGED_DEPTH");
			}
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

			Matrix4x4d cameraToWorld = cameraToWorldD;
			cameraToWorld.Set (ctol1.m00, ctol1.m01, ctol1.m02, ctol1.m03,
			                   ctol1.m10, ctol1.m11, ctol1.m12, ctol1.m13,
			                   ctol1.m20, ctol1.m21, ctol1.m22, ctol1.m23,
			                   ctol1.m30, ctol1.m31, ctol1.m32, ctol1.m33);

			Vector3d translation;

			if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
			{
				translation = oceanNode.prolandManager.parentLocalTransform.position;	//have to use this in space center or get the tsunami bug
			}
			else
			{
				translation = oceanNode.prolandManager.parentCelestialBody.position;		//more precise, especially with RSS, but breaks a bit in KSC
			}

			Matrix4x4d worldToLocal = worldToLocalD;
			worldToLocal.Set (1, 0, 0, -translation.x,
			                  0, 1, 0, -translation.y,
			                  0, 0, 1, -translation.z,
			                  0, 0, 0, 1);

			Matrix4x4d camToLocal = camToLocalD;
			Matrix4x4d.Multiply (worldToLocal, cameraToWorld, camToLocal);
			Matrix4x4d localToCam = localToCamD;
			camToLocal.InverseInto (localToCam);

			// camera in local space relative to planet's origin
			Vector3d2 cl = camPosLocalD;
			camToLocal.MultiplyPoint (zeroD, cl);

			double radius = oceanNode.prolandManager.GetRadius ();

			oceanNode.uz.CopyFrom (cl); // unit z vector of ocean frame, in local space
			oceanNode.uz.Normalize ();

			if (m_oldlocalToOceanValid)
			{
				scratchA.Set (m_oldlocalToOcean.m [1, 0], m_oldlocalToOcean.m [1, 1], m_oldlocalToOcean.m [1, 2]);
				scratchA.CrossInto (oceanNode.uz, oceanNode.ux);
				oceanNode.ux.Normalize ();
			}
			else
			{
				scratchA.Set (0, 0, 1); // Vector3d2.UnitZ()
				scratchA.CrossInto (oceanNode.uz, oceanNode.ux);
				oceanNode.ux.Normalize ();
			}

			oceanNode.uz.CrossInto (oceanNode.ux, oceanNode.uy); // unit y vector

			//Wind moves in -Ux direction, which by default points north for some reason, can rotate it to any desired direction this way

			oceanNode.oo.Set (oceanNode.uz.x * radius, oceanNode.uz.y * radius, oceanNode.uz.z * radius); // origin of ocean frame, in local space

			//local to ocean transform
			//computed from oo and ux, uy, uz should be correct
			Matrix4x4d localToOcean = localToOceanD;
			localToOcean.Set (
				oceanNode.ux.x, oceanNode.ux.y, oceanNode.ux.z, -oceanNode.ux.Dot (oceanNode.oo),
				oceanNode.uy.x, oceanNode.uy.y, oceanNode.uy.z, -oceanNode.uy.Dot (oceanNode.oo),
				oceanNode.uz.x, oceanNode.uz.y, oceanNode.uz.z, -oceanNode.uz.Dot (oceanNode.oo),
				0.0, 0.0, 0.0, 1.0);

			Matrix4x4d cameraToOcean = cameraToOceanD;
			Matrix4x4d.Multiply (localToOcean, camToLocal, cameraToOcean);
			Matrix4x4d worldToOcean = worldToOceanD;
			Matrix4x4d.Multiply (localToOcean, worldToLocal, worldToOcean);

			if (m_oldlocalToOceanValid)
			{
				m_oldlocalToOcean.InverseInto (inverseScratchD);
				inverseScratchD.MultiplyPoint (zeroD, scratchA);
				localToOcean.MultiplyPoint (scratchA, deltaD);
				oceanNode.m_Offset.x += deltaD.x;
				oceanNode.m_Offset.y += deltaD.y;
				oceanNode.m_Offset.z += deltaD.z;
			}
			
			//reset offset when bigger than 20000 to  avoid floating point issues when later casting the offset to float
			if (Mathf.Max (Mathf.Abs ((float)oceanNode.m_Offset.x), Mathf.Abs ((float)oceanNode.m_Offset.y)) > 20000f)
			{
				oceanNode.m_Offset.x=0.0;
				oceanNode.m_Offset.y=0.0;
			}
			
			m_oldlocalToOcean.CopyFrom (localToOcean); // copy, not reference-assign: localToOceanD is reused next frame
			m_oldlocalToOceanValid = true;

			//			Matrix4x4d ctos = ModifiedProjectionMatrix (inCamera); //moved to command buffer
			//			Matrix4x4d stoc = ctos.Inverse ();

			cameraToOcean.MultiplyPoint (zeroD, scratchA); // oc
			oceanNode.height = scratchA.z;

			oceanNode.offset.Set (-oceanNode.m_Offset.x, -oceanNode.m_Offset.y, oceanNode.height);
			
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
			
			Vector3d sunDirRaw = oceanNode.prolandManager.getDirectionToMainSun ();
			Vector3d2 sunDir = sunDirD;
			sunDir.Set (sunDirRaw.x, sunDirRaw.y, sunDirRaw.z);
			Vector3d2 oceanSunDir = oceanSunDirD;
			localToOcean.MultiplyVector3x3 (sunDir, oceanSunDir);

			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToWorld_PROPERTY, cameraToWorld .ToMatrix4x4());

			oceanMaterial.SetVector (ShaderProperties._Ocean_SunDir_PROPERTY, oceanSunDir.ToVector3 ());

			oceanMaterial.SetMatrix (ShaderProperties._Ocean_CameraToOcean_PROPERTY, cameraToOcean.ToMatrix4x4 ());
			cameraToOcean.InverseInto (inverseScratchD);
			oceanMaterial.SetMatrix (ShaderProperties._Ocean_OceanToCamera_PROPERTY, inverseScratchD.ToMatrix4x4 ());

			//			oceanMaterial.SetMatrix (ShaderProperties._Globals_CameraToScreen_PROPERTY, ctos.ToMatrix4x4 ());
			//			oceanMaterial.SetMatrix (ShaderProperties._Globals_ScreenToCamera_PROPERTY, stoc.ToMatrix4x4 ());

			oceanMaterial.SetMatrix (ShaderProperties._Globals_WorldToOcean_PROPERTY, worldToOcean.ToMatrix4x4 ());
			worldToOcean.InverseInto (inverseScratchD);
			oceanMaterial.SetMatrix (ShaderProperties._Globals_OceanToWorld_PROPERTY, inverseScratchD.ToMatrix4x4 ());
			
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
			
			Vector3d2 sphereDir = sphereDirD;
			localToCam.MultiplyPoint (zeroD, sphereDir);         //vector to center of planet
			double OHL = sphereDir.Magnitude ();         		 //distance to center of planet
			sphereDir.Normalize ();						 		 //direction to center of planet
			
			double rHorizon = Math.Sqrt(OHL * OHL - radius * radius);  //distance to the horizon, i.e distance to ocean sphere tangent
			
			//Theta=angle to horizon, now all that is left to do is check the viewdir against this angle in the shader
			double cosTheta= rHorizon / (OHL); 
			double sinTheta= Math.Sqrt (1- cosTheta*cosTheta);
			
			oceanMaterial.SetVector (ShaderProperties.sphereDir_PROPERTY, sphereDir.ToVector3 ());
			oceanMaterial.SetFloat (ShaderProperties.cosTheta_PROPERTY, (float) cosTheta);
			oceanMaterial.SetFloat (ShaderProperties.sinTheta_PROPERTY, (float) sinTheta);

			//planetshine properties
			if ((oceanNode.prolandManager.secondarySuns.Count > 0) || Scatterer.Instance.mainSettings.usePlanetShine)
			{
				Matrix4x4 planetShineSourcesMatrix=oceanNode.prolandManager.planetShineSourcesMatrix;
				
				Vector3d2 oceanSunDir2 = scratchB;
				for (int i=0;i<4;i++)
				{
					Vector4 row = planetShineSourcesMatrix.GetRow(i);
					if (row.w != 0f)
					{
						scratchA.Set (row.x, row.y, row.z);
						localToOcean.MultiplyVector3x3 (scratchA, oceanSunDir2);
						planetShineSourcesMatrix.SetRow(i,new Vector4((float)oceanSunDir2.x,(float)oceanSunDir2.y,(float)oceanSunDir2.z,row.w));
					}
				}
				oceanMaterial.SetMatrix (ShaderProperties.planetShineSources_PROPERTY, planetShineSourcesMatrix);
				oceanMaterial.SetMatrix (ShaderProperties.planetShineRGB_PROPERTY, oceanNode.prolandManager.planetShineRGBMatrix);
			}

			Matrix4x4 worldToLightMatrix = oceanNode.prolandManager.mainSunLight.transform.worldToLocalMatrix;
			if (oceanNode.prolandManager.parentCelestialBody.transform.position.sqrMagnitude < oceanNode.prolandManager.mainSunLight.transform.position.sqrMagnitude)
			{
				worldToLightMatrix.m03 = oceanNode.prolandManager.parentCelestialBody.transform.position.x;
				worldToLightMatrix.m13 = oceanNode.prolandManager.parentCelestialBody.transform.position.y;
				worldToLightMatrix.m23 = oceanNode.prolandManager.parentCelestialBody.transform.position.z;
			}

			if (oceanNode.causticsShadowMaskModulator)
			{
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetMatrix (ShaderProperties.CameraToWorld_PROPERTY, inCamera.cameraToWorldMatrix);
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetMatrix (ShaderProperties.WorldToLight_PROPERTY, worldToLightMatrix);
				oceanNode.causticsShadowMaskModulator.CausticsShadowMaskModulateMaterial.SetVector (ShaderProperties.PlanetOrigin_PROPERTY, oceanNode.prolandManager.parentLocalTransform.position);
			}

			if (oceanNode.causticsLightRaysRenderer)
			{
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetMatrix (ShaderProperties.CameraToWorld_PROPERTY, inCamera.cameraToWorldMatrix);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetMatrix (ShaderProperties.WorldToLight_PROPERTY, worldToLightMatrix);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetVector (ShaderProperties.LightDir_PROPERTY, oceanNode.prolandManager.mainSunLight.transform.forward);
				oceanNode.causticsLightRaysRenderer.CausticsLightRaysMaterial.SetVector (ShaderProperties.PlanetOrigin_PROPERTY, oceanNode.prolandManager.parentLocalTransform.position);
			}
		}

		public void OnDestroy()
		{
		}
	}
}