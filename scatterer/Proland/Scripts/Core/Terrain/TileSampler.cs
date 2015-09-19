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
using System.Collections.Generic;

namespace scatterer
{
	/*
	* This class can set the uniforms necessary to access a given texture tile on GPU, stored
	* in a GPUTileStorage. This class also manages the creation
	* of new texture tiles when a terrain quadtree is updated, via a TileProducer.
	*/
	public class TileSampler : Node
	{
		//class used to sort a TileSampler based on its priority
		public class Sort : IComparer<TileSampler>
		{
			int IComparer<TileSampler>.Compare(TileSampler a, TileSampler b)
			{           
				if (a.GetPriority() > b.GetPriority())
					return 1;
				if (a.GetPriority() < b.GetPriority())
					return -1;
				else
					return 0; 
			}
		}

		public class Uniforms
		{
			public int tile, tileSize, tileCoords;

			public Uniforms(string name)
			{
				tile = Shader.PropertyToID("_" + name + "_Tile");
				tileSize = Shader.PropertyToID("_" + name + "_TileSize");
				tileCoords = Shader.PropertyToID("_" + name + "_TileCoords");
			}
		}

		//The terrain node associated with this sampler
		[SerializeField]
		GameObject m_terrainNodeGO;
		TerrainNode m_terrainNode;
		//True to store texture tiles for leaf quads.
		[SerializeField]
		bool m_storeLeaf = true;
		//True to store texture tiles for non leaf quads.
		[SerializeField]
		bool m_storeParent = true;
		//True to store texture tiles for invisible quads.
		//[SerializeField]
		bool m_storeInvisible = false;
		//The order in which to update samplers
		[SerializeField]
		int m_priority = -1;

		//An internal quadtree to store the texture tiles associated with each quad.
		QuadTree m_root = null;

		Uniforms m_uniforms;

		//The producer to be used to create texture tiles for newly created quads.
		TileProducer m_producer;
		TileFilter[] m_tileFilters;

		public override void Start() 
		{
			base.Start();

			m_producer = GetComponent<TileProducer>();
			m_terrainNode = m_terrainNodeGO.GetComponent<TerrainNode>();
			m_uniforms = new Uniforms(m_producer.GetName());
			m_tileFilters = GetComponents<TileFilter>();
		}

		public override void OnDestroy() 
		{
			base.OnDestroy();
			//Debug.Log("Max used tiles for producer " + m_producer.GetName() + " = " + m_producer.GetCache().GetMaxUsedTiles());
		}

		public TileProducer GetProducer() {
			return m_producer;
		}

		public bool GetStoreLeaf() {
			return m_storeLeaf;
		}

		public TerrainNode GetTerrainNode() {
			return m_terrainNode;
		}

		public int GetPriority() {
			return m_priority;
		}

		public virtual void UpdateSampler() 
		{
			if(m_storeInvisible) m_terrainNode.SetSplitInvisibleQuads(true);

			PutTiles(m_root, m_terrainNode.GetRoot());
			GetTiles(null, ref m_root, m_terrainNode.GetRoot());

			//Debug.Log("used = " + GetProducer().GetCache().GetUsedTilesCount() + " unused = " + GetProducer().GetCache().GetUnusedTilesCount());
		}

		/*
		* Returns true if a tile is needed for the given terrain quad.
		*/
		protected virtual bool NeedTile(TerrainQuad quad)
		{
			bool needTile = m_storeLeaf;

			//if the quad is not a leaf and producer has children 
			//and if have been asked not to store parent then dont need tile
			if (!m_storeParent && !quad.IsLeaf() && m_producer.HasChildren(quad.GetLevel(), quad.GetTX(), quad.GetTY())) {
				needTile = false;
			}

			//Check if any of the filters have determined that this tile is not needed
			foreach(TileFilter filter in m_tileFilters) {
				if (filter.DiscardTile(quad)) {
					needTile = false;
					break;
				}
			}

			//if this quad is not visilbe and have not been asked to store invisilbe quads dont need tile
			if (!m_storeInvisible && !quad.IsVisible()) {
				needTile = false;
			}

			return needTile;
		}

		/*
		* Updates the internal quadtree to make it identical to the given terrain
		* quadtree. This method releases the texture tiles corresponding to
		* deleted quads.
		*/
		protected virtual void PutTiles(QuadTree tree, TerrainQuad quad)
		{
			if (tree == null) return;

			//Check if this tile is needed, if not put tile.
			tree.needTile = NeedTile(quad);
			
			if (!tree.needTile && tree.tile != null) {
				m_producer.PutTile(tree.tile);
				tree.tile = null;
			}

			//If this qiad is a leaf then all children of the tree are not needed
			if (quad.IsLeaf()) {
				if (!tree.IsLeaf()) {
					tree.RecursiveDeleteChildren(this);
				}
			}
			else if(m_producer.HasChildren(quad.GetLevel(), quad.GetTX(), quad.GetTY())) {
				for (int i = 0; i < 4; ++i) 
					PutTiles(tree.children[i], quad.GetChild(i));
			}

		}

		/*
		* Updates the internal quadtree to make it identical to the given terrain
		* quadtree. Collects the tasks necessary to create the missing texture
		* tiles, corresponding to newly created quads.
		*/
		protected virtual void GetTiles(QuadTree parent, ref QuadTree tree, TerrainQuad quad)
		{
			//if tree not created, create a new tree and check if its tile is needed
			if (tree == null) 
			{
				tree = new QuadTree(parent);
				tree.needTile = NeedTile(quad);
			}

			//If this trees tile is needed get a tile and add its task to the schedular if the task is not already done
			if (tree.needTile && tree.tile == null) 
			{
				tree.tile = m_producer.GetTile(quad.GetLevel(), quad.GetTX(), quad.GetTY());

				if(!tree.tile.GetTask().IsDone()) 
				{
					//if task not done schedule task
					m_manager.GetSchedular().Add(tree.tile.GetTask());
				}
			}
			
			if (!quad.IsLeaf() && m_producer.HasChildren(quad.GetLevel(), quad.GetTX(), quad.GetTY())) {
				for (int i = 0; i < 4; ++i) {
					GetTiles(tree, ref tree.children[i], quad.GetChild(i));
				}
			}
		}

		public void SetTile(MaterialPropertyBlock matPropertyBlock, int level, int tx, int ty)
		{
			if(!m_producer.IsGPUProducer()) return;

			RenderTexture tex = null;
			Vector3 coords = Vector3.zero, size = Vector3.zero;

			SetTile(ref tex, ref coords, ref size, level, tx, ty);

			matPropertyBlock.AddTexture(m_uniforms.tile, tex);
			matPropertyBlock.AddVector(m_uniforms.tileCoords, coords);
			matPropertyBlock.AddVector(m_uniforms.tileSize, size);
		}

		public void SetTile(Material mat, int level, int tx, int ty)
		{
			if(!m_producer.IsGPUProducer()) return;

			RenderTexture tex = null;
			Vector3 coords = Vector3.zero, size = Vector3.zero;
			
			SetTile(ref tex, ref coords, ref size, level, tx, ty);
			
			mat.SetTexture(m_uniforms.tile, tex);
			mat.SetVector(m_uniforms.tileCoords, coords);
			mat.SetVector(m_uniforms.tileSize, size);
		}

		/*
		* Sets the uniforms necessary to access the texture tile for
		* the given quad. The samplers producer must be using a GPUTileStorage at the first slot
		* for this function to work
		*/
		void SetTile(ref RenderTexture tex, ref Vector3 coord, ref Vector3 size, int level, int tx, int ty)
		{

			if(!m_producer.IsGPUProducer()) return;

			Tile t = null;
			int b = m_producer.GetBorder();
			int s = m_producer.GetCache().GetStorage(0).GetTileSize();

			float dx = 0;
			float dy = 0;
			float dd = 1;
			float ds0 = (s / 2) * 2.0f - 2.0f * b;
			float ds = ds0;

			while (!m_producer.HasTile(level, tx, ty)) {
				dx += (tx % 2) * dd;
				dy += (ty % 2) * dd;
				dd *= 2;
				ds /= 2;
				level -= 1;
				tx /= 2;
				ty /= 2;

				if(level < 0) {
					Debug.Log("Proland::TileSampler::SetTile - invalid level");
					return;
				}
			}
			
			QuadTree tt = m_root;
			QuadTree tc;
			int tl = 0;
			while (tl != level && (tc = tt.children[((tx >> (level - tl - 1)) & 1) | ((ty >> (level - tl - 1)) & 1) << 1]) != null) {
				tl += 1;
				tt = tc;
			}

			while (level > tl) {
				dx += (tx % 2) * dd;
				dy += (ty % 2) * dd;
				dd *= 2;
				ds /= 2;
				level -= 1;
				tx /= 2;
				ty /= 2;
			}
			t = tt.tile;
			
			while (t == null) {
				dx += (tx % 2) * dd;
				dy += (ty % 2) * dd;
				dd *= 2;
				ds /= 2;
				level -= 1;
				tx /= 2;
				ty /= 2;
				tt = tt.parent;

				if(tt == null) {
					Debug.Log("Proland::TileSampler::SetTile - null tile");
					return;
				}

				t = tt.tile;
			}
			
			dx = dx * ((s / 2) * 2 - 2 * b) / dd;
			dy = dy * ((s / 2) * 2 - 2 * b) / dd;

			if(t == null) {
				Debug.Log("Proland::TileSampler::SetTile - tile is null");
				return;
			}

			GPUTileStorage.GPUSlot gpuSlot = t.GetSlot(0) as GPUTileStorage.GPUSlot;

			if(gpuSlot == null) {
				Debug.Log("Proland::TileSampler::SetTile - gpuSlot is null");
				return;
			}

			float w = gpuSlot.GetTexture().width;
			float h = gpuSlot.GetTexture().height;
		
			Vector4 coords;
			if (s%2 == 0) {
				coords = new Vector4((dx + b) / w, (dy + b) / h, 0.0f, ds / w);
			} else {
				coords = new Vector4((dx + b + 0.5f) / w, (dy + b + 0.5f) / h, 0.0f, ds / w);
			}

			tex = gpuSlot.GetTexture();
			coord = new Vector3(coords.x, coords.y, coords.z);
			size = new Vector3(coords.w, coords.w, (s / 2) * 2.0f - 2.0f * b);
		}

	}
}























