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
using System.Collections.Generic;

namespace scatterer
{
	
	/*
	* A TileSampler to be used with a ElevationProducer.
	* This class reads back the elevation data of newly created elevation tiles
	* in order to update the TerrainQuad::zmin and TerrainQuad::zmax fields. It
	* also reads back the elevation value below the current viewer position to
	* update the TerrainNode::s_groundHeightAtCamera static field.
	*/
	public class TileSamplerZ : TileSampler
	{
		/*
		* An internal quadtree to store the texture tile associated with each
		* terrain quad, and to keep track of tiles that need to be read back.
		*/
		class QuadTreeZ : QuadTree
		{
			public TerrainQuad quad;
			
			public bool readBack;
			
			public QuadTreeZ(QuadTree parent, TerrainQuad quad) 
				: base(parent) 
			{
				this.quad = quad;
				readBack = false;
			}
		}
		
		/*
		* Helper class to store the retrived height data and the min/max values
		*/
		class ElevationInfo
		{
			public float[] elevations = null;
			public float min = float.PositiveInfinity;
			public float max = float.NegativeInfinity;
		}
		
		[SerializeField]
		bool m_enableMinMaxReadBack = false;
		
		[SerializeField]
		int m_maxReadBacksPerFrame = 5;
		
		[SerializeField]
		int m_maxStoredElevations = 10000;
		
		//The terrain quad directly below the current viewer position.
		QuadTreeZ m_cameraQuad;
		
		//The relative viewer position in the #cameraQuad quad.
		Vector2 m_cameraQuadCoords;
		
		/*
		* Last camera position used to perform a readback of the camera elevation
		* above the ground. This is used to avoid reading back this value at each
		* frame when the camera does not move.
		*/
		Vector3d2 m_oldLocalCamera;
		
		//A container for the quad trees that need to have there elevations read back
		Dictionary<Tile.Id, QuadTreeZ> m_needReadBack;
		//A container of all the tiles that have had there elevations read back
		DictionaryQueue<Tile.Id, ElevationInfo> m_elevations;
		
		ComputeBuffer m_elevationsBuffer, m_groundBuffer;
		
		public override void Start () 
		{
			base.Start();
			
			m_oldLocalCamera = Vector3d2.Zero();
			
			m_needReadBack = new Dictionary<Tile.Id, QuadTreeZ>(new Tile.EqualityComparerID());
			
			m_elevations = new DictionaryQueue<Tile.Id, ElevationInfo>(new Tile.EqualityComparerID());
			
			int size = GetProducer().GetTileSize(0);
			
			m_elevationsBuffer = new ComputeBuffer(size*size, sizeof(float));
			
			m_groundBuffer = new ComputeBuffer(1, sizeof(float));
			
		}
		
		public override void OnDestroy()
		{
			base.OnDestroy();
			m_elevationsBuffer.Release();
			m_groundBuffer.Release();
		}
		
		/*
		* Override the default TileSampler NeedTile to retrive the tile 
		* that is below the camera as well as its default behaviour
	 	*/
		protected override bool NeedTile(TerrainQuad quad)
		{
			Vector3d2 c = quad.GetOwner().GetLocalCameraPos();
			int l = quad.GetLevel();
			double ox = quad.GetOX();
			double oy = quad.GetOY();
			
			if (c.x >= ox && c.x < ox + l && c.y >= oy && c.y < oy + l) {
				return true;
			}
			
			return base.NeedTile(quad);
		}
		
		public override void UpdateSampler () 
		{
			base.UpdateSampler();
			
			UpdateMinMax();
			
			UpdateGroundHeight();
		}
		
		/*
		 * Updates the ground height below camera.
		 */
		void UpdateGroundHeight()
		{
			Vector3d2 localCamPos = GetTerrainNode().GetLocalCameraPos();
			
			//If camera has moved update ground height
			if ((localCamPos - m_oldLocalCamera).Magnitude() > 1.0 && m_cameraQuad != null && m_cameraQuad.tile != null) 
			{
				GPUTileStorage.GPUSlot slot = m_cameraQuad.tile.GetSlot()[0] as GPUTileStorage.GPUSlot;
				
				if(slot != null)
				{
					int border = GetProducer().GetBorder();
					int tileSize = GetProducer().GetTileSizeMinBorder(0);
					
					float dx = m_cameraQuadCoords.x * tileSize;
					float dy = m_cameraQuadCoords.y * tileSize;
					
					//x,y are the non-normalized position in the elevations texture where the 
					//ground height below the camera is.
					float x = dx + (float)border;
					float y = dy + (float)border;
					//Read the single value from the render texture
					CBUtility.ReadSingleFromRenderTexture(slot.GetTexture(), x, y, 0, m_groundBuffer, m_manager.GetReadData(), true);
					//Get single height value from buffer
					float[] height = new float[1];
					m_groundBuffer.GetData(height);
					
					//Update the ground height. Stored as a static value in the TerrainNode script
					GetView().SetGroundHeight(Math.Max(0.0, height[0]));
					
					m_oldLocalCamera.x = localCamPos.x;
					m_oldLocalCamera.y = localCamPos.y;
					m_oldLocalCamera.z = localCamPos.z;
					
				}
			}
			
			m_cameraQuad = null;
			
		}
		
		/*
		 * Updates the terrainQuads min and max values. Used to create a better fitting bounding box.
		 * Is not essental and can be disabled if retriving the heights data from the GPU is causing 
		 * performance issues.
		 */
		void UpdateMinMax()
		{
			//if no quads need read back or if disabled return
			if(m_needReadBack.Count == 0 || !m_enableMinMaxReadBack) return;
			
			//Make a copy of all the keys of the tiles that need to be read back
			Tile.Id[] ids = new Tile.Id[m_needReadBack.Count]; 
			m_needReadBack.Keys.CopyTo(ids, 0);
			//Sort the keys by there level, lowest -> highest
			System.Array.Sort(ids, new Tile.ComparerID());
			
			int count = 0;
			
			//foreach key read back the tiles data until the maxReadBacksPerFrame limit is reached
			foreach(Tile.Id id in ids)
			{
				QuadTreeZ t = m_needReadBack[id];
				
				//If elevations container already contains key then data has been
				//read back before so just reapply the min/max values to TerranQuad
				if(m_elevations.ContainsKey(id))
				{
					ElevationInfo info = m_elevations.Get(id);
					
					t.quad.SetZMin(info.min);
					t.quad.SetZMax(info.max);
					
					m_needReadBack.Remove(id);
				}
				else
				{
					//if for some reason the tile is null remove from container and continue
					if(t.tile == null) {
						m_needReadBack.Remove(id);
						continue;
					}
					
					GPUTileStorage.GPUSlot slot = t.tile.GetSlot()[0] as GPUTileStorage.GPUSlot;
					
					//If for some reason this is not a GPUSlot remove and continue
					if(slot == null) {
						m_needReadBack.Remove(id);
						continue;
					}
					
					RenderTexture tex = slot.GetTexture();
					
					int size = tex.width*tex.height;
					
					ElevationInfo info = new ElevationInfo();
					info.elevations = new float[size];
					//Read back heights data from texture
					CBUtility.ReadFromRenderTexture(tex, 1, m_elevationsBuffer, m_manager.GetReadData());
					//Copy into elevations info
					m_elevationsBuffer.GetData(info.elevations);
					//Find the min/max values
					for(int i = 0; i < size; i++)
					{
						if(info.elevations[i] < info.min) info.min = info.elevations[i];
						if(info.elevations[i] > info.max) info.max = info.elevations[i];
					}
					//Update quad
					t.quad.SetZMin(info.min);
					t.quad.SetZMax(info.max);
					//Store elevations to prevent having to read back again soon
					//Add to end of container
					m_elevations.AddLast(id, info);
					
					m_needReadBack.Remove(id);
					
					count++;
					//If the number of rad back to do per frame has hit the limit stop loop.
					if(count >= m_maxReadBacksPerFrame) break;
				}
			}
			
			//If the number of elevation info to store has exceded limit remove from start of container
			while(m_elevations.Count() > m_maxStoredElevations)
				m_elevations.RemoveFirst();
			
		}
		
		protected override void GetTiles(QuadTree parent, ref QuadTree tree, TerrainQuad quad)
		{
			if (tree == null) 
			{
				tree = new QuadTreeZ(parent, quad);
				tree.needTile = NeedTile(quad);
			}
			
			QuadTreeZ t = tree as QuadTreeZ;
			
			//If tile needs elevation data read back add to container
			if(t.tile != null && t.tile.GetTask().IsDone() && !t.readBack && m_maxReadBacksPerFrame > 0)
			{
				if(!m_needReadBack.ContainsKey(t.tile.GetId()))
				{
					t.readBack = true;
					m_needReadBack.Add(t.tile.GetId(), t);
				}
			}
			
			base.GetTiles(parent, ref tree, quad);
			
			//Check if this quad is below the camera. If so store a reference to it.
			if (m_cameraQuad == null && t.tile != null && t.tile.GetTask().IsDone()) 
			{
				Vector3d2 c = quad.GetOwner().GetLocalCameraPos();
				
				double l = quad.GetLength();
				double ox = quad.GetOX();
				double oy = quad.GetOY();
				
				if (c.x >= ox && c.x < ox + l && c.y >= oy && c.y < oy + l) 
				{
					m_cameraQuadCoords = new Vector2((float)((c.x - ox) / l), (float)((c.y - oy) / l));
					m_cameraQuad = t;
				}
			}
		}
		
	}
	
}





































