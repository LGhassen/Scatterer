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
	* A tile described by its level,tx,ty coordinates. A Tile
	* describes where the tile is stored in the TileStorage, how its data can
	* be produced, and how many users currently use it.
	* Contains the keys (Id, Tid) commonly used to store the tiles in data structures like dictionaries
	*/
	public class Tile
	{
		/*
		* A tile identifier for a given producer. Contains the tile coordinates
		* level, tx, ty.
		*/
		public class Id
		{
			int level, tx, ty;

			public Id(int level, int tx, int ty) {
				this.level = level;
				this.tx = tx;
				this.ty = ty;
			}

			public int Compare(Id id) {
				return level.CompareTo(id.level);
			}

			public bool Equals(Id id) {
				return (level == id.level && tx == id.tx && ty == id.ty);
			}

			public override int GetHashCode() {
				int code = level ^ tx ^ ty;
				return code.GetHashCode();
			}

			public override string ToString () {
				return level.ToString() + "," + tx.ToString() + "," + ty.ToString();
			}
		}

		/*
		* A tile identifier. Contains a producer id and
		* tile coordinates level,tx,ty.
		*/
		public class TId
		{
			int producerId;
			Id tileId;

			public TId(int producerId, int level, int tx, int ty) {
				this.producerId = producerId;
				this.tileId = new Id(level, tx, ty);
			}

			public bool Equals(TId id) {
				return (producerId == id.producerId && tileId.Equals(id.tileId));
			}

			public override int GetHashCode() {
				int code = producerId ^ tileId.GetHashCode();
				return code.GetHashCode();
			}

			public override string ToString () {
				return producerId.ToString() + "," + tileId.ToString();
			}
		}

		//classes used to describe how keys can the sorted and compared

		//A Id is sorted based as its level. Sorts from lowest level to highest
		public class ComparerID : IComparer<Id>
		{
			public int Compare(Id a, Id b) {
				return a.Compare(b);
			}
		}

		//A Id is compared based on its level, tx and ty
		public class EqualityComparerID : IEqualityComparer<Id>
		{
			public bool Equals(Id t1, Id t2) {
				return t1.Equals(t2);
			}
			
			public int GetHashCode(Id t) {
				return t.GetHashCode();
			}
		}

		//A Tid is compared based on its producer, level, tx and ty
		public class EqualityComparerTID : IEqualityComparer<TId>
		{
			public bool Equals(TId t1, TId t2) {
				return t1.Equals(t2);
			}

			public int GetHashCode(TId t) {
				return t.GetHashCode();
			}
		}

		//The id of the producer that manages this tile.
		int m_producerId;

		//Number of users currently using this tile
		int m_users;

		//The quadtree level of this tile.
		int m_level;

		/*
		* The quadtree x coordinate of this tile at level level.
		* Varies between 0 and 2^level - 1.
		*/
		int m_tx;

		/*
		* The quadtree y coordinate of this tile at level level.
		* Varies between 0 and 2^level - 1.
		*/
		int m_ty;

		//The task that produces or produced the actual tile data.
		CreateTileTask m_task;

		/*
		* Creates a new tile.
		*
		* param producerId the id of the producer of this tile.
		* param level the quadtree level of this tile.
		* param tx the quadtree x coordinate of this tile.
		* param ty the quadtree y coordinate of this tile.
		* param task the task that will produce the tile data.
		*/
		public Tile(int producerId, int level, int tx, int ty, CreateTileTask task) 
		{
			m_producerId = producerId;
			m_level = level;
			m_tx = tx;
			m_ty = ty;
			m_task = task;
			m_users = 0;

			if(m_task == null) {
				Debug.Log("Proland::Tile::Tile - task can not be null");
			}
		}

		public List<TileStorage.Slot> GetSlot() {
			return m_task.GetSlot();
		}

		public TileStorage.Slot GetSlot(int i) 
		{
			if(i >= m_task.GetSlot().Count) {
				Debug.Log("Proland::Tile::GetSlot - slot at location " + i + " does not exist");
			}
			return m_task.GetSlot()[i];
		}

		public Task GetTask() {
			return m_task;
		}

		public int GetLevel() {
			return m_level;
		}

		public int GetTX() {
			return m_tx;
		}

		public int GetTY() {
			return m_ty;
		}

		public int GetUsers() {
			return m_users;
		}

		public void IncrementUsers() {
			m_users++;
		}

		public void DecrementUsers() {
			m_users--;
		}

		//Returns the identifier of this tile.
		public Id GetId() {
			return GetId(m_level, m_tx, m_ty);
		}

	 	//Returns the identifier of this tile.
		public TId GetTId() {
			return GetTId(m_producerId, m_level, m_tx, m_ty);
		}

		/*
		* Returns the identifier of a tile.
		*
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		*/
		public static Id GetId(int level, int tx, int ty) {
			return new Id(level, tx, ty);
		}

		/*
		* Returns the identifier of a tile.
		*
		* param producerId the id of the tile's producer.
		* param level the tile's quadtree level.
		* param tx the tile's quadtree x coordinate.
		* param ty the tile's quadtree y coordinate.
		*/
		public static TId GetTId(int producerId, int level, int tx, int ty) {
			return new TId(producerId, level, tx, ty);
		}
	
	};

}






































