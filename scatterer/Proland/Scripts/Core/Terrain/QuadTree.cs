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
 */


using UnityEngine;
using System.Collections;

namespace scatterer
{

	//An internal quadtree to store the texture tile associated with each terrain quad.
	public class QuadTree 
	{
		//TODO - make members private?

		//if a tile is needed for this quad
		public bool needTile;
		//The parent quad of this quad.
		public QuadTree parent;
		//The texture tile associated with this quad.
		public Tile tile;
		//The subquads of this quad.
		public QuadTree[] children = new QuadTree[4];
		
		public QuadTree(QuadTree parent) {
			this.parent = parent;
		}
		
		public bool IsLeaf() { 
			return (children[0] == null); 
		}
		
		//Deletes All trees subelements. Releases
		//all the corresponding texture tiles.
		public void RecursiveDeleteChildren(TileSampler owner)
		{
			if (children[0] != null) {
				for(int i = 0; i < 4; i++) {
					children[i].RecursiveDelete(owner);
					children[i] = null;
				}
			}
		}
		
		//Deletes this Tree and all its subelements. Releases
		//all the corresponding texture tiles.
		public void RecursiveDelete(TileSampler owner)
		{
			if (tile != null && owner != null) {
				owner.GetProducer().PutTile(tile);
				tile = null;
			}
			if (children[0] != null) {
				for(int i = 0; i < 4; i++) {
					children[i].RecursiveDelete(owner);
					children[i] = null;
				}
			}
		}
	}

}












