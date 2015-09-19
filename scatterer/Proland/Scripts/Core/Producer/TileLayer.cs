
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
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace scatterer
{

	/*
	 * An abstract layer for a TileProducer. Some tile producers can be
	 * customized with layers modifying the default tile production algorithm
	 * (for instance to add roads or rivers to an orthographic tile producer).
	 * For these kind of producers, each method of this class is called during
	 * the corresponding method in the TileProducer. The default implementation
	 * of these methods in this class is empty.
	 */
	public abstract class TileLayer : Node 
	{

		public override void Start () {
			base.Start();
		}

		public abstract void DoCreateTile(int level, int tx, int ty, List<TileStorage.Slot> slot);

	}

}



























