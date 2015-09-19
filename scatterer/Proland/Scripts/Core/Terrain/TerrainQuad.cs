
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
 * */

using UnityEngine;
using System.Collections;
using System;

namespace scatterer
{
	/*
	* A quad in a terrain quadtree. The quadtree is subdivided based only
	* on the current viewer position. All quads are subdivided if they
	* meet the subdivision criterion, even if they are outside the view
	* frustum. The quad visibility is stored in #visible. It can be used
	* in TileSampler to decide whether or not data must be produced
	* for invisible tiles (we recall that the terrain quadtree itself
	* does not store any terrain data).
	*/
	public class TerrainQuad 
	{
		
		//The TerrainNode to which this terrain quadtree belongs.
		TerrainNode m_owner;
		//The parent quad of this quad.
		TerrainQuad m_parent;
		//The level of this quad in the quadtree (0 for the root).
		int m_level;
		//The logical x,y coordinate of this quad (between 0 and 2^level).
		int m_tx, m_ty;
		//The physical x,y coordinate of the lower left corner of this quad (in local space).
		double m_ox, m_oy;
		//The physical size of this quad (in local space).
		double m_length;
		//local bounding box
		Box3d m_localBox;
		//Should the quad be drawn
		bool m_drawable;
		
		/*
		* The minimum/maximum terrain elevation inside this quad. This field must
		* be updated manually by users (the TileSamplerZ class can
		* do this for you).
		*/
		float m_zmin, m_zmax;
		
		/*
		* The four subquads of this quad. If this quad is not subdivided,
		* the four values are NULL. The subquads are stored in the
		* following order: bottomleft, bottomright, topleft, topright.
		*/
		TerrainQuad[] m_children = new TerrainQuad[4];
		
		/*
		* The visibility of the bounding box of this quad from the current
		* viewer position. The bounding box is computed using zmin and
		* zmax, which must therefore be up to date to get a correct culling
		* of quads out of the view frustum. This visibility only takes frustum
		* culling into account.
		*/
		Frustum.VISIBILTY m_visible;
		
		/*
		* True if the bounding box of this quad is occluded by the bounding
		* boxes of the quads in front of it.
		*/
		bool m_occluded = false;
		
		/*
		* Creates a new TerrainQuad.
		*
		* param owner the TerrainNode to which the terrain quadtree belongs.
		* param parent the parent quad of this quad.
		* param tx the logical x coordinate of this quad.
		* param ty the logical y coordinate of this quad.
		* param ox the physical x coordinate of the lower left corner of this quad.
		* param oy the physical y coordinate of the lower left corner of this quad.
		* param l the physical size of this quad.
		* param zmin the minimum %terrain elevation inside this quad.
		* param zmax the maximum %terrain elevation inside this quad.
		*/
		public TerrainQuad(TerrainNode owner, TerrainQuad parent, int tx, int ty, double ox, double oy, double length, float zmin, float zmax)
		{
			m_owner = owner;
			m_parent = parent;
			m_level = (m_parent == null) ? 0 : m_parent.GetLevel() + 1;
			m_tx = tx;
			m_ty = ty;
			m_ox = ox;
			m_oy = oy;
			m_zmax = zmax;
			m_zmin = zmin;
			m_length = length;
			m_localBox = new Box3d(m_ox, m_ox + m_length, m_oy, m_oy + m_length, m_zmin, m_zmax);
			
		}
		
		public Frustum.VISIBILTY GetVisible() { 
			return m_visible; 
		}
		
		public int GetLevel() { 
			return m_level; 
		}
		
		public float GetZMax() { 
			return m_zmax; 
		}
		
		public float GetZMin() { 
			return m_zmin; 
		}
		
		public bool GetOccluded() { 
			return m_occluded; 
		}
		
		public int GetTX() { 
			return m_tx; 
		}
		
		public int GetTY() { 
			return m_ty; 
		}
		
		public double GetOX() {
			return m_ox;
		}
		
		public double GetOY() {
			return m_oy;
		}
		
		public double GetLength() { 
			return m_length; 
		}
		
		public TerrainQuad GetChild(int i) {
			return m_children[i];
		}
		
		public bool GetDrawable() {
			return m_drawable;
		}
		
		public void SetDrawable(bool drawable) {
			m_drawable = drawable;
		}
		
		public void SetZMin(float zmin) {
			m_zmin = zmin;
		}
		
		public void SetZMax(float zmax) {
			m_zmax = zmax;
		}
		
		//Returns the TerrainNode to which the terrain quadtree belongs.
		public TerrainNode GetOwner() { 
			return m_owner; 
		}
		
		public bool IsVisible() {
			return (m_visible != Frustum.VISIBILTY.INVISIBLE);
		}
		
		//Returns true if this quad is not subdivided.
		public bool IsLeaf() { 
			return (m_children[0] == null); 
		}
		
		//Returns the number of quads in the tree below this quad.
		public int GetSize()
		{
			int s = 1;
			
			if(IsLeaf()) {
				return s;
			} 
			else {
				return 	s + m_children[0].GetSize() + m_children[1].GetSize() +
					m_children[2].GetSize() + m_children[3].GetSize();
			}
		}
		
		//Returns the depth of the tree below this quad.
		public int GetDepth()
		{
			if(IsLeaf()) {
				return m_level;
			} 
			else {
				return 	Mathf.Max(Mathf.Max(m_children[0].GetDepth(), m_children[1].GetDepth()),
				                  Mathf.Max(m_children[2].GetDepth(), m_children[3].GetDepth()));
			}
		}
		
		void Release()
		{
			
			for(int i = 0; i < 4; i++)
			{
				if(m_children[i] != null)
				{
					m_children[i].Release();
					m_children[i] = null;
				}
			}
			
		}
		
		/*
	     * Subdivides or unsubdivides this quad based on the current
	     * viewer distance to this quad, relatively to its size. This
	     * method uses the current viewer position provided by the
	     * TerrainNode to which this quadtree belongs.
	     */
		public void Update()
		{
			
			Frustum.VISIBILTY v = (m_parent == null) ? Frustum.VISIBILTY.PARTIALLY : m_parent.GetVisible();
			
			if (v == Frustum.VISIBILTY.PARTIALLY) {
				m_visible = m_owner.GetVisibility(m_localBox);
			} 
			else {
				m_visible = v;
			}
			
			// here we reuse the occlusion test from the previous frame:
			// if the quad was found unoccluded in the previous frame, we suppose it is
			// still unoccluded at this frame. If it was found occluded, we perform
			// an occlusion test to check if it is still occluded.
			if (m_visible != Frustum.VISIBILTY.INVISIBLE && m_occluded) 
			{
				m_occluded = m_owner.IsOccluded(m_localBox);
				
				if(m_occluded) {
					m_visible = Frustum.VISIBILTY.INVISIBLE;
				}
			}
			
			double ground = m_owner.GetView().GetGroundHeight();
			double dist = m_owner.GetCameraDist(new Box3d(m_ox, m_ox + m_length, m_oy, m_oy + m_length, Math.Min(0.0, ground), Math.Max(0.0, ground)));
			
			if ((m_owner.GetSplitInvisibleQuads() || m_visible != Frustum.VISIBILTY.INVISIBLE) && dist < m_length * m_owner.GetSplitDist() && m_level < m_owner.GetMaxLevel()) 
			{
				if (IsLeaf()) {
					Subdivide();
				}
				
				int[] order = new int[4];
				double ox = m_owner.GetLocalCameraPos().x;
				double oy = m_owner.GetLocalCameraPos().y;
				double cx = m_ox + m_length / 2.0;
				double cy = m_oy + m_length / 2.0;
				
				if (oy < cy) {
					if (ox < cx) {
						order[0] = 0;
						order[1] = 1;
						order[2] = 2;
						order[3] = 3;
					} else {
						order[0] = 1;
						order[1] = 0;
						order[2] = 3;
						order[3] = 2;
					}
				} else {
					if (ox < cx) {
						order[0] = 2;
						order[1] = 0;
						order[2] = 3;
						order[3] = 1;
					} else {
						order[0] = 3;
						order[1] = 1;
						order[2] = 2;
						order[3] = 0;
					}
				}
				
				m_children[order[0]].Update();
				m_children[order[1]].Update();
				m_children[order[2]].Update();
				m_children[order[3]].Update();
				
				// we compute a more precise occlusion for the next frame (see above),
				// by combining the occlusion status of the child nodes
				m_occluded = (m_children[0].GetOccluded() && m_children[1].GetOccluded() && m_children[2].GetOccluded() && m_children[3].GetOccluded());
			} 
			else 
			{
				if (m_visible != Frustum.VISIBILTY.INVISIBLE) {
					// we add the bounding box of this quad to the occluders list
					m_occluded = m_owner.AddOccluder(m_localBox);
					if (m_occluded) {
						m_visible = Frustum.VISIBILTY.INVISIBLE;
					}
				}
				
				if (!IsLeaf()) {
					Release();
				}
			}
			
		}
		
		//Creates the four subquads of this quad.
		void Subdivide()
		{
			float hl = (float) m_length / 2.0f;
			m_children[0] = new TerrainQuad(m_owner, this, 2 * m_tx, 2 * m_ty, m_ox, m_oy, hl, m_zmin, m_zmax);
			m_children[1] = new TerrainQuad(m_owner, this, 2 * m_tx + 1, 2 * m_ty, m_ox + hl, m_oy, hl, m_zmin, m_zmax);
			m_children[2] = new TerrainQuad(m_owner, this, 2 * m_tx, 2 * m_ty + 1, m_ox, m_oy + hl, hl, m_zmin, m_zmax);
			m_children[3] = new TerrainQuad(m_owner, this, 2 * m_tx + 1, 2 * m_ty + 1, m_ox + hl, m_oy + hl, hl, m_zmin, m_zmax);
		}
		
		static int[,] ORDER = new int[,]{{1,0},{2,3},{0,2},{3,1}};
		
		/*
		 * Used to draw the outline of the terrain quads bounding box.
		 * See the DrawQuadTree.cs script for more info.
		 */
		public void DrawQuadOutline(Camera camera, Material lineMaterial, Color lineColor)
		{
			if(IsLeaf())
			{
				if(m_visible == Frustum.VISIBILTY.INVISIBLE) return;
				
				Vector3[] verts = new Vector3[8];
				
				verts[0] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox, m_oy, m_zmin)).ToVector3();
				verts[1] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox+m_length, m_oy, m_zmin)).ToVector3();
				verts[2] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox, m_oy+m_length, m_zmin)).ToVector3();
				verts[3] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox+m_length, m_oy+m_length, m_zmin)).ToVector3();
				
				verts[4] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox, m_oy, m_zmax)).ToVector3();
				verts[5] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox+m_length, m_oy, m_zmax)).ToVector3();
				verts[6] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox, m_oy+m_length, m_zmax)).ToVector3();
				verts[7] = m_owner.GetDeform().LocalToDeformed(new Vector3d2(m_ox+m_length, m_oy+m_length, m_zmax)).ToVector3();
				
				GL.PushMatrix();
				
				GL.LoadIdentity();
				GL.MultMatrix(camera.worldToCameraMatrix * m_owner.GetLocalToWorld().ToMatrix4x4());
				GL.LoadProjectionMatrix(camera.projectionMatrix);
				
				lineMaterial.SetPass( 0 );
				GL.Begin( GL.LINES );
				GL.Color( lineColor );
				
				for(int i = 0; i < 4; i++) 
				{
					//Draw bottom quad
					GL.Vertex3( verts[ORDER[i,0]].x, verts[ORDER[i,0]].y, verts[ORDER[i,0]].z );
					GL.Vertex3( verts[ORDER[i,1]].x, verts[ORDER[i,1]].y, verts[ORDER[i,1]].z );
					//Draw top quad
					GL.Vertex3( verts[ORDER[i,0]+4].x, verts[ORDER[i,0]+4].y, verts[ORDER[i,0]+4].z );
					GL.Vertex3( verts[ORDER[i,1]+4].x, verts[ORDER[i,1]+4].y, verts[ORDER[i,1]+4].z );
					//Draw verticals
					GL.Vertex3( verts[ORDER[i,0]].x, verts[ORDER[i,0]].y, verts[ORDER[i,0]].z );
					GL.Vertex3( verts[ORDER[i,0]+4].x, verts[ORDER[i,0]+4].y, verts[ORDER[i,0]+4].z );
				}
				
				GL.End();
				
				GL.PopMatrix();
			}
			
			if(!IsLeaf())
			{
				m_children[0].DrawQuadOutline(camera, lineMaterial, lineColor);
				m_children[1].DrawQuadOutline(camera, lineMaterial, lineColor);
				m_children[2].DrawQuadOutline(camera, lineMaterial, lineColor);
				m_children[3].DrawQuadOutline(camera, lineMaterial, lineColor);
			}
		}
		
	}
}

























