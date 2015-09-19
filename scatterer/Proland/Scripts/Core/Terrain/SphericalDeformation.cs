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
	/*
	 * A Deformation of space transforming planes to spheres. This deformation
	 * transforms the plane z=0 into a sphere of radius R centered at (0,0,-R).
	 * The plane z=h is transformed into the sphere of radius R+h. The
	 * deformation of p=(x,y,z) in local space is q=(R+z) P /\norm P\norm,
	 * where P=(x,y,R).
	 * 
	 * NOTE - See Deformation.cs script for more info on what each function does
	 */
	public class SphericalDeformation : Deformation
	{
		
		//The radius of the sphere into which the plane z=0 must be deformed.
		double R;
		
		public SphericalDeformation(double R)
		{
			this.R = R;
		}
		
		public override Vector3d2 LocalToDeformed(Vector3d2 localPt)
		{
			return (new Vector3d2(localPt.x, localPt.y, R)).Normalized(localPt.z + R);
		}
		
		public override Matrix4x4d LocalToDeformedDifferential(Vector3d2 localPt, bool clamp = false)
		{
			if (!MathUtility.IsFinite(localPt.x) || !MathUtility.IsFinite(localPt.y) || !MathUtility.IsFinite(localPt.z)) {
				return Matrix4x4d.Identity();
			}
			
			Vector3d2 pt = new Vector3d2(localPt);
			
			if (clamp) {
				pt.x = pt.x - Math.Floor((pt.x + R) / (2.0 * R)) * 2.0 * R;
				pt.y = pt.y - Math.Floor((pt.y + R) / (2.0 * R)) * 2.0 * R;
			}
			
			double l = pt.x*pt.x + pt.y*pt.y + R*R;
			double c0 = 1.0 / Math.Sqrt(l);
			double c1 = c0 * R / l;
			
			return new Matrix4x4d((	pt.y*pt.y + R*R)*c1, -pt.x*pt.y*c1, pt.x*c0, R*pt.x*c0,
			                      -pt.x*pt.y*c1, (pt.x*pt.x + R*R)*c1, pt.y*c0, R*pt.y*c0,
			                      -pt.x*R*c1, -pt.y*R*c1, R*c0, (R*R)*c0,
			                      0.0, 0.0, 0.0, 1.0);
		}
		
		public override Vector3d2 DeformedToLocal(Vector3d2 deformedPt)
		{
			double l = deformedPt.Magnitude();
			
			if (deformedPt.z >= Math.Abs(deformedPt.x) && deformedPt.z >= Math.Abs(deformedPt.y)) {
				return new Vector3d2(deformedPt.x / deformedPt.z * R, deformedPt.y / deformedPt.z * R, l - R);
			}
			if (deformedPt.z <= -Math.Abs(deformedPt.x) && deformedPt.z <= -Math.Abs(deformedPt.y)) {
				return new Vector3d2(double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity);
			}
			if (deformedPt.y >= Math.Abs(deformedPt.x) && deformedPt.y >= Math.Abs(deformedPt.z)) {
				return new Vector3d2(deformedPt.x / deformedPt.y * R, (2.0 - deformedPt.z / deformedPt.y) * R, l - R);
			}
			if (deformedPt.y <= -Math.Abs(deformedPt.x) && deformedPt.y <= -Math.Abs(deformedPt.z)) {
				return new Vector3d2(-deformedPt.x / deformedPt.y * R, (-2.0 - deformedPt.z / deformedPt.y) * R, l - R);
			}
			if (deformedPt.x >= Math.Abs(deformedPt.y) && deformedPt.x >= Math.Abs(deformedPt.z)) {
				return new Vector3d2((2.0 - deformedPt.z / deformedPt.x) * R, deformedPt.y / deformedPt.x * R, l - R);
			}
			if (deformedPt.x <= -Math.Abs(deformedPt.y) && deformedPt.x <= -Math.Abs(deformedPt.z)) {
				return new Vector3d2((-2.0 - deformedPt.z / deformedPt.x) * R, -deformedPt.y / deformedPt.x * R, l - R);
			}
			//should never reach here
			Debug.Log("Proland::SpericalDeformation::DeformToLocal - fail");
			return new Vector3d2();
		}
		
		public override Box2d DeformedToLocalBounds(Vector3d2 deformedCenter, double deformedRadius)
		{
			Vector3d2 p = DeformedToLocal(deformedCenter);
			double r = deformedRadius;
			
			if (double.IsInfinity(p.x) || double.IsInfinity(p.y)) {
				return new Box2d();
			}
			
			double k = (1.0 - r * r / (2.0 * R * R)) * (new Vector3d2(p.x, p.y, R)).Magnitude();
			double A = k * k - p.x * p.x;
			double B = k * k - p.y * p.y;
			double C = -2.0 * p.x * p.y;
			double D = -2.0 * R * R * p.x;
			double E = -2.0 * R * R * p.y;
			double F = R * R * (k * k - R * R);
			
			double a = C * C - 4.0 * A * B;
			double b = 2.0 * C * E - 4.0 * B * D;
			double c = E * E - 4.0 * B * F;
			double d = Math.Sqrt(b * b - 4.0 * a * c);
			double x1 = (- b - d) / (2.0 * a);
			double x2 = (- b + d) / (2.0 * a);
			
			b = 2.0 * C * D - 4.0 * A * E;
			c = D * D - 4.0 * A * F;
			d = Math.Sqrt(b * b - 4.0 * a * c);
			double y1 = (- b - d) / (2.0 * a);
			double y2 = (- b + d) / (2.0 * a);
			
			return new Box2d(new Vector2d(x1, y1), new Vector2d(x2, y2));
		}
		
		public override Matrix4x4d DeformedToTangentFrame(Vector3d2 deformedPt)
		{
			Vector3d2 Uz = deformedPt.Normalized();
			Vector3d2 Ux = (new Vector3d2(0,1,0)).Cross(Uz).Normalized();
			Vector3d2 Uy = Uz.Cross(Ux);
			
			return new Matrix4x4d(	Ux.x, Ux.y, Ux.z, 0.0,
			                      Uy.x, Uy.y, Uy.z, 0.0,
			                      Uz.x, Uz.y, Uz.z, -R,
			                      0.0, 0.0, 0.0, 1.0);
		}
		
		public override Frustum.VISIBILTY GetVisibility(TerrainNode t, Box3d localBox)
		{
			Vector3d2[] deformedBox = new Vector3d2[4];
			deformedBox[0] = LocalToDeformed(new Vector3d2(localBox.xmin, localBox.ymin, localBox.zmin));
			deformedBox[1] = LocalToDeformed(new Vector3d2(localBox.xmax, localBox.ymin, localBox.zmin));
			deformedBox[2] = LocalToDeformed(new Vector3d2(localBox.xmax, localBox.ymax, localBox.zmin));
			deformedBox[3] = LocalToDeformed(new Vector3d2(localBox.xmin, localBox.ymax, localBox.zmin));
			
			double a = (localBox.zmax + R) / (localBox.zmin + R);
			double dx = (localBox.xmax - localBox.xmin) / 2 * a;
			double dy = (localBox.ymax - localBox.ymin) / 2 * a;
			double dz = localBox.zmax + R;
			double f = Math.Sqrt(dx * dx + dy * dy + dz * dz) / (localBox.zmin + R);
			
			Vector4d[] deformedFrustumPlanes = t.GetDeformedFrustumPlanes();
			
			Frustum.VISIBILTY v0 = GetVisibility(deformedFrustumPlanes[0], deformedBox, f);
			if (v0 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			Frustum.VISIBILTY v1 = GetVisibility(deformedFrustumPlanes[1], deformedBox, f);
			if (v1 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			Frustum.VISIBILTY v2 = GetVisibility(deformedFrustumPlanes[2], deformedBox, f);
			if (v2 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			Frustum.VISIBILTY v3 = GetVisibility(deformedFrustumPlanes[3], deformedBox, f);
			if (v3 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			Frustum.VISIBILTY v4 = GetVisibility(deformedFrustumPlanes[4], deformedBox, f);
			if (v4 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			Vector3d2 c = t.GetDeformedCameraPos();
			double lSq = c.SqrMagnitude();
			double rm = R + Math.Min(0.0, localBox.zmin);
			double rM = R + localBox.zmax;
			double rmSq = rm * rm;
			double rMSq = rM * rM;
			Vector4d farPlane = new Vector4d(c.x, c.y, c.z, Math.Sqrt((lSq - rmSq) * (rMSq - rmSq)) - rmSq);
			
			Frustum.VISIBILTY v5 = GetVisibility(farPlane, deformedBox, f);
			if (v5 == Frustum.VISIBILTY.INVISIBLE) {
				return Frustum.VISIBILTY.INVISIBLE;
			}
			
			if (v0 == Frustum.VISIBILTY.FULLY && v1 == Frustum.VISIBILTY.FULLY &&
			    v2 == Frustum.VISIBILTY.FULLY && v3 == Frustum.VISIBILTY.FULLY &&
			    v4 == Frustum.VISIBILTY.FULLY && v5 == Frustum.VISIBILTY.FULLY)
			{
				return Frustum.VISIBILTY.FULLY;
			}
			return Frustum.VISIBILTY.PARTIALLY;
		}
		
		public static Frustum.VISIBILTY GetVisibility(Vector4d clip, Vector3d2[] b, double f)
		{
			double o = b[0].x * clip.x + b[0].y * clip.y + b[0].z * clip.z;
			bool p = o + clip.w > 0.0;
			
			if ((o * f + clip.w > 0.0) == p) 
			{
				o = b[1].x * clip.x + b[1].y * clip.y + b[1].z * clip.z;
				
				if ((o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p) 
				{
					o = b[2].x * clip.x + b[2].y * clip.y + b[2].z * clip.z;
					
					if ((o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p) 
					{
						o = b[3].x * clip.x + b[3].y * clip.y + b[3].z * clip.z;
						
						return 	(o + clip.w > 0.0) == p && (o * f + clip.w > 0.0) == p ? 
							(p ? Frustum.VISIBILTY.FULLY : Frustum.VISIBILTY.INVISIBLE) : 
								Frustum.VISIBILTY.PARTIALLY;
					}
				}
			}
			
			return Frustum.VISIBILTY.PARTIALLY;
		}
		
		public override void SetUniforms(TerrainNode node, Material mat)
		{
			if(mat == null || node == null) return;
			
			base.SetUniforms(node, mat);
			
			mat.SetFloat(m_uniforms.radius, (float)R);
		}
		
		protected override void SetScreenUniforms(TerrainNode node, TerrainQuad quad, MaterialPropertyBlock matPropertyBlock)
		{
			
			double ox = quad.GetOX();
			double oy = quad.GetOY();
			double l = quad.GetLength();
			
			Vector3d2 p0 = new Vector3d2(ox, oy, R);
			Vector3d2 p1 = new Vector3d2(ox + l, oy, R);
			Vector3d2 p2 = new Vector3d2(ox, oy + l, R);
			Vector3d2 p3 = new Vector3d2(ox + l, oy + l, R);
			Vector3d2 pc = (p0 + p3) * 0.5;
			
			double l0 = 0.0, l1 = 0.0, l2 = 0.0, l3 = 0.0;
			Vector3d2 v0 = p0.Normalized(ref l0);
			Vector3d2 v1 = p1.Normalized(ref l1);
			Vector3d2 v2 = p2.Normalized(ref l2);
			Vector3d2 v3 = p3.Normalized(ref l3);
			
			Matrix4x4d deformedCorners = new Matrix4x4d(v0.x * R, v1.x * R, v2.x * R, v3.x * R,
			                                            v0.y * R, v1.y * R, v2.y * R, v3.y * R,
			                                            v0.z * R, v1.z * R, v2.z * R, v3.z * R,
			                                            1.0, 1.0, 1.0, 1.0);
			
			matPropertyBlock.AddMatrix(m_uniforms.screenQuadCorners, (m_localToScreen * deformedCorners).ToMatrix4x4());
			
			Matrix4x4d deformedVerticals = new Matrix4x4d(	v0.x, v1.x, v2.x, v3.x,
			                                              v0.y, v1.y, v2.y, v3.y,
			                                              v0.z, v1.z, v2.z, v3.z,
			                                              0.0, 0.0, 0.0, 0.0);
			
			matPropertyBlock.AddMatrix(m_uniforms.screenQuadVerticals, (m_localToScreen * deformedVerticals).ToMatrix4x4());
			matPropertyBlock.AddVector(m_uniforms.screenQuadCornerNorms, new Vector4((float)l0, (float)l1, (float)l2, (float)l3));
			
			Vector3d2 uz = pc.Normalized();
			Vector3d2 ux = (new Vector3d2(0,1,0)).Cross(uz).Normalized();
			Vector3d2 uy = uz.Cross(ux);
			
			Matrix4x4d ltow = node.GetLocalToWorld();
			
			Matrix3x3d tangentFrameToWorld = new Matrix3x3d(ltow.m[0,0], ltow.m[0,1], ltow.m[0,2],
			                                                ltow.m[1,0], ltow.m[1,1], ltow.m[1,2],
			                                                ltow.m[2,0], ltow.m[2,1], ltow.m[2,2]);
			
			Matrix3x3d m = new Matrix3x3d(	ux.x, uy.x, uz.x,
			                              ux.y, uy.y, uz.y,
			                              ux.z, uy.z, uz.z);
			
			matPropertyBlock.AddMatrix(m_uniforms.tangentFrameToWorld, (tangentFrameToWorld * m).ToMatrix4x4());
			
		}
		
	}
}



























