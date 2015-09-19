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
 * Modified and adapted for use with Kerbal Space Program by Ghassen Lahmar 2015
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{
	/*
	* A cache of tiles to avoid recomputing recently produced tiles. A tile cache
	* keeps track of which tiles (identified by their level,tx,ty coordinates) are
	* currently stored in an associated TileStorage. It also keeps track of which
	* tiles are in use, and which are not. Unused tiles are kept in the TileStorage
	* as long as possible, in order to avoid re creating them if they become needed
	* again. But the storage associated with unused tiles can be reused to store
	* other tiles at any moment (in this case we say that a tile is evicted from
	* the cache of unused tiles).
	* Conversely, the storage associated with tiles currently in use cannot be
	* reaffected until these tiles become unused. A tile is in use when it is
	* returned by GetTile, and becomes unused when PutTile is called (more
	* precisely when the number of users of this tile becomes 0, this number being
	* incremented and decremented by GetTile and PutTile, respectively). The
	* tiles that are needed to render the current frame should be declared in use,
	* so that they are not evicted between their creation and their actual
	* rendering.
	* 
	* A cache can have multiple TileStorages attached to it and the slot created is made up
	* of a slot from each of the TileStorages. This is so producer can generate tiles that contain 
	* multiple types of data associated with the same tile. For example the PlantsProducer uses a cache with 2 CBTileStorages,
	* one slot for the plants position and one for the plants other parameters.
	*/
	public class TileCache : MonoBehaviour 
	{
	
     	//Next local identifier to be used for a TileProducer using this cache.
		static int s_nextProducerId = 0;

		//The total number of slots managed by the TileStorage attached to the cache.
		[SerializeField]
		int m_capacity;

     	//The storage to store the tiles data.
		TileStorage[] m_tileStorage;

		/*
		* The tiles currently in use. These tiles cannot be evicted from the cache
		* and from the TileStorage, until they become unused. Maps tile identifiers
		* to actual tiles.
		*/
		Dictionary<Tile.TId, Tile> m_usedTiles;

		/*
		* The unused tiles. These tiles can be evicted from the cache at any moment.
		* Uses a custom container (DictionaryQueue) that can store tiles by there Tid for fast look up
		* and also keeps track of the order the tiles were inserted so it can also act as a queue
		*/
		DictionaryQueue<Tile.TId, Tile> m_unusedTiles;

		//The producers that use this TileCache. Maps local producer identifiers to actual producers.
		Dictionary<int, TileProducer> m_producers;

		int m_maxUsedTiles = 0;

		void Awake()
		{
			m_tileStorage = GetComponents<TileStorage>();
			m_producers = new Dictionary<int, TileProducer>();
			m_usedTiles = new Dictionary<Tile.TId, Tile>(new Tile.EqualityComparerTID());
			m_unusedTiles = new DictionaryQueue<Tile.TId, Tile>(new Tile.EqualityComparerTID());

		}

		public int NextProducerId() {
			return s_nextProducerId++;
		}

		public void InsertProducer(int id, TileProducer producer) 
		{
			if(m_producers.ContainsKey(id)) {
				Debug.Log("Proland::TileCache::InsertProducer - Producer id already inserted");
			}
			else {
				m_producers.Add(id, producer);
			}
		}

     	//Returns the storage used to store the actual tiles data.
		public TileStorage GetStorage(int i) 
		{
			if(i >= m_tileStorage.Length) {
				Debug.Log("Proland::TileCache::GetStorage - tile storage at location " + i + " does not exist");
			}
			return m_tileStorage[i];
		}

		public int GetCapacity() {
			return m_capacity;
		}

		public int GetTileStorageCount() {
			return m_tileStorage.Length;
		}

		//Returns the number of tiles currently in use in this cache.
		public int GetUsedTilesCount() {
			return m_usedTiles.Count;
		}

     	//Returns the number of tiles currently unused in this cache.
		public int GetUnusedTilesCount() {
			return m_unusedTiles.Count();
		}

		public int GetMaxUsedTiles() {
			return m_maxUsedTiles;
		}

		/*
		 * Call this when a tile is no longer needed.
		 * If the number of users of the tile is 0 then the tile will be moved from the used to the unused cache
		 */
		public void PutTile(Tile tile)
		{

			if(tile == null) return;

			tile.DecrementUsers();

			//if there are no more users of this tile move the tile from the used cahce to the unused cache
			if(tile.GetUsers() <= 0)
			{
				Tile.TId id = tile.GetTId();

				if(m_usedTiles.ContainsKey(id))
					m_usedTiles.Remove(id);
				
				if(!m_unusedTiles.ContainsKey(id))
					m_unusedTiles.AddLast(id, tile);
			}

		}

		/*
		 * Creates a new slot for a tile. A slot is made up of a slot from
		 * each of the TileStorages attached to the TileCache.
		 * If anyone of the storages runs out of slots then null will be returned and the program 
		 * should abort if this happens.
		 */
		List<TileStorage.Slot> NewSlot()
		{
			List<TileStorage.Slot> slot = new List<TileStorage.Slot>();

			foreach(TileStorage storage in m_tileStorage)
			{
				TileStorage.Slot s = storage.NewSlot();
				if(s == null) return null;
				slot.Add(s);
			}

			return slot;
		}

		/*
		 * Call this if a tile is needed. Will move the tile from the unused to the used cache if its is found there.
		 * If the tile is not found then a new tile will be created with a new slot. If there are no more free
		 * slots then the cache capacity has not been set to a high enough value and the program must abort.
		 */
		public Tile GetTile(int producerId, int level, int tx, int ty)
		{
			//If this producer id does not exist can not create tile.
			if(!m_producers.ContainsKey(producerId)) {
				Debug.Log("Proland::TileCache::GetTile - Producer id not been inserted into cache");
				return null;
			}
			
			Tile.TId id = Tile.GetTId(producerId, level, tx, ty);
			Tile tile = null;

			//If tile is not in the used cache
			if (!m_usedTiles.ContainsKey(id)) 
			{
				//If tile is also not in the unused cache
				if (!m_unusedTiles.ContainsKey(id)) 
				{
					List<TileStorage.Slot> slot = NewSlot();
					
					//if there are no more free slots then start recyling slots from the unused tiles
					if (slot == null && !m_unusedTiles.Empty()) {
						//Remove the tile and recylce its slot
						slot = m_unusedTiles.RemoveFirst().GetSlot();
					}

					//If a slot is found create a new tile with a new task
					if(slot != null) 
					{
						CreateTileTask task = m_producers[producerId].CreateTile(level, tx, ty, slot);
						tile = new Tile(producerId, level, tx, ty, task);
					}

					//If a free slot is not found then program has must abort. Try setting the cache capacity to higher value.
					if(slot == null) {
						throw new CacheCapacityException("No more free slots found. Insufficient storage capacity for cache " + name);
					}
				} 
				else {
					//else if the tile is in the unused cache remove it and keep a reference to it
					tile = m_unusedTiles.Remove(id);
				}
				
				if (tile != null) {
					m_usedTiles.Add(id, tile);
				}
				
			} 
			else {
				tile = m_usedTiles[id];
			}

			//Should never be null be this stage
			if(tile == null) {
				throw new System.ArgumentNullException("Tile should not be null");
			}

			//Keep track of the max number of tiles ever used for debug purposes
			if(m_usedTiles.Count > m_maxUsedTiles)
				m_maxUsedTiles = m_usedTiles.Count;

			//inc the num of users
			tile.IncrementUsers();
			
			return tile;
		}

		/*
		 * Finds a tile based on its Tid. If includeUnusedCache is true then will also look in
		 * the unused cache but be warned that tiles in the unused cache maybe evicted and have there slot recylced as any time
		 */
		public Tile FindTile(int producerId, int level, int tx, int ty, bool includeUnusedCache)
		{

			Tile.TId id = Tile.GetTId(producerId, level, tx, ty);
			Tile tile = null;

			// looks for the requested tile in the used tiles list
			if (m_usedTiles.ContainsKey(id)) 
				tile = m_usedTiles[id];

			// looks for the requested tile in the unused tiles list (if includeUnusedCache is true)
			if (tile == null && includeUnusedCache) 
			{
				if (m_unusedTiles.ContainsKey(id)) 
					tile = m_unusedTiles.Get(id);
			}
	
			return tile;
		}

	}

}




























