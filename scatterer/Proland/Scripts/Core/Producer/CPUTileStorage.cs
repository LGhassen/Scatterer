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
using System.Collections;

namespace scatterer
{

	/**
	* A TileStorage that store tiles on CPU as a 2D array of values
	* T is the type of each tile pixel component (e.g. char, float, etc).
	*/
	public class CPUTileStorage : TileStorage 
	{

		//A slot managed by a GPUTileStorage and contains the array of values
		public class CPUSlot<T> : Slot
		{
			T[] m_data;
			int m_size;

			public T[] GetData() {
				return m_data;
			}

			public int GetSize() {
				return m_size;
			}

			public void ClearData() {
				m_data = new T[m_size];
			}

			public CPUSlot(TileStorage owner, int size)
			: base(owner) 
			{
				m_data = new T[size];
				m_size = size;
			}
		};

		public enum DATA_TYPE { FLOAT, INT, SHORT, BYTE };
		
		[SerializeField]
		DATA_TYPE m_dataType = DATA_TYPE.FLOAT;
		
		[SerializeField]
		int m_channels = 1;

		public int GetChannels() {
			return m_channels;
		}

		public DATA_TYPE GetDataType() { 
			return m_dataType;
		}

		protected override void Awake() 
		{
			base.Awake();
			
			int tileSize = GetTileSize();
			int capacity = GetCapacity();

			//Note size is sqaured as the array is 2D (but stored as a 1D array)
			int size = tileSize * tileSize * m_channels;
			
			for(int i = 0; i < capacity; i++) {

				switch((int)m_dataType)
				{
					case (int)DATA_TYPE.FLOAT:
						AddSlot(i, new CPUSlot<float>(this, size));
					break;

					case (int)DATA_TYPE.INT:
					AddSlot(i, new CPUSlot<int>(this, size));
					break;

					case (int)DATA_TYPE.SHORT:
					AddSlot(i, new CPUSlot<short>(this, size));
					break;

					case (int)DATA_TYPE.BYTE:
						AddSlot(i, new CPUSlot<byte>(this, size));
					break;
				}

			}
		}

	}
}




























