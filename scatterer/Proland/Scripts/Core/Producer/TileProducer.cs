
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
using System.Collections.Generic;

namespace scatterer
{
	
	/*
	* An abstract producer of tiles. A TileProducer must be inherited from and overide the DoCreateTile
	* function to create the tiles data.
	* Note that several TileProducer can share the same TileCache, and hence the
	* same TileStorage.
	*/
	[RequireComponent(typeof(TileSampler))]
	public abstract class TileProducer : Node 
	{

		//The tile cache game object that stores the tiles produced by this producer.
		[SerializeField]
		GameObject m_cacheGO;
		TileCache m_cache;

		//The name of the uniforms this producers data will be bound if used in a shader
		[SerializeField]
		string m_name;

		//Does this producer use the gpu 
		[SerializeField]
		bool m_isGPUProducer = true;

		//layers that may modify the tile created by this producer and are optional
		TileLayer[] m_layers;

		//The tile sampler associated with this producer
		TileSampler m_sampler;

		//The id of this producer. This id is local to the TileCache used by this
		//producer, and is used to distinguish all the producers that use this cache.
		int m_id;

		// Use this for initialization
		public override void Start() 
		{
			base.Start();
			InitCache();

			//Get any layers attached to same game object. May have 0 to many attached.
			m_layers = GetComponents<TileLayer>();
			//Get the samplers attached to game object. Must have one sampler attahed.
			m_sampler = GetComponent<TileSampler>();

		}

		public override void OnDestroy()
		{
			base.OnDestroy();
		}

		//It is posible that a producer will have a call to get its cache before
		//its start fuction has been called. Call InitCache in the start and get functions
		//to ensure that the cache is always init before being returned.
		void InitCache() 
		{
			if(m_cache == null) 
			{
				m_cache = m_cacheGO.GetComponent<TileCache>();
				m_id = m_cache.NextProducerId();
				m_cache.InsertProducer(m_id, this);
			}
		}

		//Returns the TileCache that stores the tiles produced by this producer.
		public TileCache GetCache() { 
			InitCache();
			return m_cache;
		}

		public bool IsGPUProducer() {
			return m_isGPUProducer;
		}

		public int GetId() {
			return m_id;
		}

		public string GetName() {
			return m_name;
		}

		public TileSampler GetSampler() {
			return m_sampler;
		}

		public TerrainNode GetTerrainNode() {
			return m_sampler.GetTerrainNode();
		}

		public int GetTileSize(int i) {
			return GetCache().GetStorage(i).GetTileSize();
		}

		public int GetTileSizeMinBorder(int i) {
			int s = GetCache().GetStorage(i).GetTileSize();
			return s - GetBorder()*2;
		}

		/*
		* Returns the size in pixels of the border of each tile. Tiles made of
		* raster data may have a border that contains the value of the neighboring
		* pixels of the tile. For instance if the tile size (returned by
		* TileStorage.GetTileSize) is 196, and if the tile border is 2, this means
		* that the actual tile data is 192x192 pixels, with a 2 pixel border that
		* contains the value of the neighboring pixels. Using a border introduces
		* data redundancy but is usefull to get the value of the neighboring pixels
		* of a tile without needing to load the neighboring tiles.
		*/
		public virtual int GetBorder() {
			return 0;
		}

		/*
		* Returns true if this producer can produce the given tile.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		*/
		public virtual bool HasTile(int level, int tx, int ty) {
			return true;
		}

		/*
		* Returns true if this producer can produce the children of the given tile.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		*/
		public virtual bool HasChildren(int level, int tx, int ty){
			return HasTile(level + 1, 2 * tx, 2 * ty);
		}

		/*
		* Decrements the number of users of this tile by one. If this number
		* becomes 0 the tile is marked as unused, and so can be evicted from the
		* cache at any moment.
		*
		* param tile a tile currently in use.
		*/
		public virtual void PutTile(Tile tile) {
			m_cache.PutTile(tile);
		}

		/*
		* Returns the requested tile, creating it if necessary. If the tile is
		* currently in use it is returned directly. If it is in cache but unused,
		* it marked as used and returned. Otherwise a new tile is created, marked
		* as used and returned. In all cases the number of users of this tile is
		* incremented by one.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		* 
		* return the requested tile
		*/
		public virtual Tile GetTile(int level, int tx, int ty) 
		{
			return m_cache.GetTile(m_id, level, tx, ty);
		}

		/*
		* Looks for a tile in the TileCache of this TileProducer.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		* param includeUnusedCache true to include both used and unused tiles in the
		* search, false to include only the used tiles.
		* param done true to check that the tile's creation task is done.
		* 
		* return the requested tile, or NULL if it is not in the TileCache or
		* if 'done' is true, if it is not ready. This method does not change the
		* number of users of the returned tile.
		*/
		public virtual Tile FindTile(int level, int tx, int ty, bool includeUnusedCache, bool done) 
		{
			Tile tile = m_cache.FindTile(m_id, level, tx, ty, includeUnusedCache);

			if (done && tile != null && !tile.GetTask().IsDone()) {
				tile = null;
			}

			return tile;
		}

		/*
		* Creates a Task to produce the data of the given tile.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		* param slot where the produced tile data must be stored.
		* 
		* return the task to create this tile.
		*/
		public virtual CreateTileTask CreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot) {
			return new CreateTileTask(this, level, tx, ty, slot);
		}
	
		/*
		* Creates the given tile. If this task requires tiles produced by other
		* The default implementation of this method calls DoCreateTile on
		* each Layer of this producer.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		* param slot where the created tile data must be stored.
		*/
		public virtual void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot) {

			if(m_layers == null) return;

			foreach (TileLayer layer in m_layers) {
				layer.DoCreateTile(level, tx, ty, slot);
			}
		}

		//Not currently used and maybe not working correctly

//		public Vector4 GetGpuTileCoords(int level, int tx, int ty, ref Tile tile)
//		{
//			int s = GetCache().GetStorage().GetTileSize();
//			int b = GetBorder();
//			float dx = 0.0f;
//			float dy = 0.0f;
//			float dd = 1.0f;
//			float ds0 = ((float)s / 2.0f) * 2.0f - 2.0f * (float)b;
//			float ds = ds0;
//
//			while (!HasTile(level, tx, ty)) 
//			{
//				dx += (tx % 2) * dd;
//				dy += (ty % 2) * dd;
//				dd *= 2;
//				ds /= 2;
//				level -= 1;
//				tx /= 2;
//				ty /= 2;
//
//				if(level < 0) {
//					Debug.Log("Proland::TileProducer::GetGpuTileCoords - invalid level (A)");
//					Debug.Break();
//				}
//			}
//
//			Tile t = tile == null ? FindTile(level, tx, ty, true, true) : null;
//
//			while (tile == null ? t == null : level != tile.GetLevel()) 
//			{
//				dx += (tx % 2) * dd;
//				dy += (ty % 2) * dd;
//				dd *= 2;
//				ds /= 2;
//				level -= 1;
//				tx /= 2;
//				ty /= 2;
//
//				if(level < 0) {
//					Debug.Log("Proland::TileProducer::GetGpuTileCoords - invalid level (B)");
//					Debug.Break();
//				}
//
//				t = tile == null ? FindTile(level, tx, ty, true, true) : null;
//			}
//
//			dx = dx * ((s / 2) * 2 - 2 * b) / dd;
//			dy = dy * ((s / 2) * 2 - 2 * b) / dd;
//
//			if (tile == null) {
//				tile = t;
//			} 
//			else {
//				t = tile;
//			}
//
//			float w = (float)s;
//
//			if (s%2 == 0) {
//				return new Vector4((dx + b) / w, (dy + b) / w, 0.0f, ds / w);
//			} else {
//				return new Vector4((dx + b + 0.5f) / w, (dy + b + 0.5f) / w, 0.0f, ds / w);
//			}
//		}

	}
}
























