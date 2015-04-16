
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
 */

using UnityEngine;
using System;
using System.Collections;

namespace scatterer
{	
	/**
	* A deformation of space. Such a deformation maps a 3D source point to a 3D
	* destination point. The source space is called the local space, while
	* the destination space is called the deformed space. Source and
	* destination points are defined with their x,y,z coordinates in an orthonormal
	* reference frame. A Deformation is also responsible to set the shader uniforms
	* that are necessary to project a TerrainQuad on screen, taking the deformation
	* into account. The default implementation of this class implements the
	* identity deformation, i.e. the deformed point is equal to the local one.
	*/
	public class Deformation 
	{
		public class Uniforms
		{
			public int blending, localToWorld, localToScreen;
			public int offset, camera, screenQuadCorners;
			public int screenQuadVerticals, radius, screenQuadCornerNorms;
			public int tangentFrameToWorld, tileToTangent; 
			
			public Uniforms()
			{
				blending = Shader.PropertyToID("_Deform_Blending");
				localToWorld = Shader.PropertyToID("_Deform_LocalToWorld");
				localToScreen = Shader.PropertyToID("_Deform_LocalToScreen");
				offset = Shader.PropertyToID("_Deform_Offset");
				camera = Shader.PropertyToID("_Deform_Camera");
				screenQuadCorners = Shader.PropertyToID("_Deform_ScreenQuadCorners");
				screenQuadVerticals = Shader.PropertyToID("_Deform_ScreenQuadVerticals");
				radius = Shader.PropertyToID("_Deform_Radius");
				screenQuadCornerNorms = Shader.PropertyToID("_Deform_ScreenQuadCornerNorms");
				tangentFrameToWorld = Shader.PropertyToID("_Deform_TangentFrameToWorld");
				tileToTangent = Shader.PropertyToID("_Deform_TileToTangent");
			}
		}
		
		protected Uniforms m_uniforms;
		protected Matrix4x4d m_localToCamera;
		protected Matrix4x4d m_localToScreen;
		protected Matrix3x3d m_localToTangent;
		
		public Deformation()
		{
			m_uniforms = new Uniforms();
			m_localToCamera = new Matrix4x4d();
			m_localToScreen = new Matrix4x4d();
			m_localToTangent = new Matrix3x3d();
		}
		
		/**
		* Returns the deformed point corresponding to the given source point.
		*
		* param localPt a point in the local (i.e., source) space.
		* return the corresponding point in the deformed (i.e., destination) space.
		*/
		public virtual Vector3d2 LocalToDeformed(Vector3d2 localPt)
		{
			return localPt;
		}
		
		/**
		* Returns the differential of the deformation function at the given local
		* point. This differential gives a linear approximation of the deformation
		* around a given point, represented with a matrix. More precisely, if p
		* is near localPt, then the deformed point corresponding to p can be
		* approximated with localToDeformedDifferential(localPt) * (p - localPt).
		*
		* param localPt a point in the local (i.e., source) space. The z
		* coordinate of this point is ignored, and considered to be 0.
		* return the differential of the deformation function at the given local
		* point.
		*/
		public virtual Matrix4x4d LocalToDeformedDifferential(Vector3d2 localPt, bool clamp = false)
		{
			return Matrix4x4d.Translate(new Vector3d2(localPt.x, localPt.y, 0.0));
		}
		
		/**
		* Returns the local point corresponding to the given source point.
		*
		* param deformedPt a point in the deformed (i.e., destination) space.
		* return the corresponding point in the local (i.e., source) space.
		*/
		public virtual Vector3d2 DeformedToLocal(Vector3d2 deformedPt)
		{
			return deformedPt;
		}
		
		/**
		* Returns the local bounding box corresponding to the given source disk.
		*
		* param deformedPt the source disk center in deformed space.
		* param deformedRadius the source disk radius in deformed space.
		* return the local bounding box corresponding to the given source disk.
		*/
		public virtual Box2d DeformedToLocalBounds(Vector3d2 deformedCenter, double deformedRadius)
		{
			return new Box2d(deformedCenter.x - deformedRadius, deformedCenter.x + deformedRadius,
			                 deformedCenter.y - deformedRadius, deformedCenter.y + deformedRadius);
		}
		
		/**
		* Returns an orthonormal reference frame of the tangent space at the given
		* deformed point. This reference frame is such that its xy plane is the
		* tangent plane, at deformedPt, to the deformed surface corresponding to
		* the local plane z=0. Note that this orthonormal reference frame does
		* not give the differential of the inverse deformation funtion,
		* which in general is not an orthonormal transformation. If p is a deformed
		* point, then deformedToLocalFrame(deformedPt) * p gives the coordinates of
		* p in the orthonormal reference frame defined above.
		*
		* param deformedPt a point in the deformed (i.e., destination) space.
		* return the orthonormal reference frame at deformedPt defined above.
		*/
		public virtual Matrix4x4d DeformedToTangentFrame(Vector3d2 deformedPt)
		{
			return Matrix4x4d.Translate(new Vector3d2(-deformedPt.x, -deformedPt.y, 0.0));
		}
		
		/**
		* Returns the distance in local (i.e., source) space between a point and a
		* bounding box.
		*
		* param localPt a point in local space.
		* param localBox a bounding box in local space.
		*/
		public virtual double GetLocalDist(Vector3d2 localPt, Box3d localBox)
		{
			return Math.Max(Math.Abs(localPt.z - localBox.zmax),
			                Math.Max(Math.Min(Math.Abs(localPt.x - localBox.xmin), Math.Abs(localPt.x - localBox.xmax)),
			         Math.Min(Math.Abs(localPt.y - localBox.ymin), Math.Abs(localPt.y - localBox.ymax))));
		}
		
		/**
		 * Returns the visibility of a bounding box in local space, in a view
		 * frustum defined in deformed space.
		 *
		 * param node a TerrainNode. This is node is used to get the camera position
		 * in local and deformed space with TerrainNode::GetLocalCamera and
		 * TerrainNode::GetDeformedCamera, as well as the view frustum planes
		 * in deformed space with TerrainNode::GetDeformedFrustumPlanes.
		 * param localBox a bounding box in local space.
		 * return the visibility of the bounding box in the view frustum.
		 */
		public virtual Frustum.VISIBILTY GetVisibility(TerrainNode node, Box3d localBox)
		{
			// localBox = deformedBox, so we can compare the deformed frustum with it
			return Frustum.GetVisibility(node.GetDeformedFrustumPlanes(), localBox);
		}
		
		/**
		* Sets the shader uniforms that are necessary to project on screen the
		* TerrainQuad of the given TerrainNode. This method can set the uniforms
		* that are common to all the quads of the given terrain.
		*/
		public virtual void SetUniforms(TerrainNode node, Material mat)
		{
			if(mat == null || node == null) return;
			
			float d1 = node.GetSplitDist() + 1.0f;
			float d2 = 2.0f * node.GetSplitDist();
			mat.SetVector(m_uniforms.blending, new Vector2(d1, d2 - d1));
			
			m_localToCamera = node.GetView().GetWorldToCamera() * node.GetLocalToWorld();
			m_localToScreen = node.GetView().GetCameraToScreen() * m_localToCamera;
			
			Vector3d2 localCameraPos = node.GetLocalCameraPos();
			Vector3d2 worldCamera = node.GetView().GetWorldCameraPos();
			
			Matrix4x4d A = LocalToDeformedDifferential(localCameraPos);
			Matrix4x4d B = DeformedToTangentFrame(worldCamera);
			
			Matrix4x4d ltot = B * node.GetLocalToWorld() * A;
			
			m_localToTangent = new Matrix3x3d(	ltot.m[0,0], ltot.m[0,1], ltot.m[0,3],
			                                  ltot.m[1,0], ltot.m[1,1], ltot.m[1,3],
			                                  ltot.m[3,0], ltot.m[3,1], ltot.m[3,3]);
			
			mat.SetMatrix(m_uniforms.localToScreen, m_localToScreen.ToMatrix4x4());
			mat.SetMatrix(m_uniforms.localToWorld, node.GetLocalToWorld().ToMatrix4x4());
			
		}
		
		/**
		* Sets the shader uniforms that are necessary to project on screen the
		* given TerrainQuad. This method can set the uniforms that are specific to
		* the given quad.
		*/
		public virtual void SetUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
		{
			
			if(matPropertyBlock == null || node == null || quad == null) return;
			
			double ox = quad.GetOX();
			double oy = quad.GetOY();
			double l = quad.GetLength();
			double distFactor = (double)node.GetDistFactor();
			int level = quad.GetLevel();
			
			matPropertyBlock.AddVector(m_uniforms.offset, new Vector4((float)ox, (float)oy, (float)l, (float)level));
			
			Vector3d2 camera = node.GetLocalCameraPos();
			
			matPropertyBlock.AddVector(m_uniforms.camera, new Vector4(	(float)((camera.x - ox) / l), (float)((camera.y - oy) / l),
			                                                          (float)((camera.z - node.GetView().GetGroundHeight()) / (l * distFactor)),
			                                                          (float)camera.z));
			
			Vector3d2 c = node.GetLocalCameraPos();
			
			Matrix3x3d m = m_localToTangent * (new Matrix3x3d(l, 0.0, ox - c.x, 0.0, l, oy - c.y, 0.0, 0.0, 1.0));
			
			matPropertyBlock.AddMatrix(m_uniforms.tileToTangent, m.ToMatrix4x4());
			
			SetScreenUniforms(node, quad, matPropertyBlock);
		}
		
		protected virtual void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
		{
			
			double ox = quad.GetOX();
			double oy = quad.GetOY();
			double l = quad.GetLength();
			
			Vector3d2 p0 = new Vector3d2(ox, oy, 0.0);
			Vector3d2 p1 = new Vector3d2(ox + l, oy, 0.0);
			Vector3d2 p2 = new Vector3d2(ox, oy + l, 0.0);
			Vector3d2 p3 = new Vector3d2(ox + l, oy + l, 0.0);
			
			Matrix4x4d corners = new Matrix4x4d(p0.x, p1.x, p2.x, p3.x,
			                                    p0.y, p1.y, p2.y, p3.y,
			                                    p0.z, p1.z, p2.z, p3.z,
			                                    1.0, 1.0, 1.0, 1.0);
			
			matPropertyBlock.AddMatrix(m_uniforms.screenQuadCorners, (m_localToScreen * corners).ToMatrix4x4());
			
			Matrix4x4d verticals = new Matrix4x4d(	0.0, 0.0, 0.0, 0.0,
			                                      0.0, 0.0, 0.0, 0.0,
			                                      1.0, 1.0, 1.0, 1.0,
			                                      0.0, 0.0, 0.0, 0.0);
			
			matPropertyBlock.AddMatrix(m_uniforms.screenQuadVerticals, (m_localToScreen * verticals).ToMatrix4x4());
			
		}
		
	}	
}














