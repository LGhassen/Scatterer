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
	 * A tile storage that can contain compute buffers
	 */
	public class CBTileStorage : TileStorage 
	{
		
		//A slot managed by a CBTileStorage containing the buffer
		public class CBSlot : Slot
		{
			ComputeBuffer m_buffer;
			
			public ComputeBuffer GetBuffer() {
				return m_buffer;
			}

			public override void Release()
			{
				if(m_buffer != null) 
					m_buffer.Release();
			}
			
			public CBSlot(TileStorage owner, ComputeBuffer buffer) : base(owner) {
				m_buffer = buffer;
			}
		};

		public enum DATA_TYPE { FLOAT, INT, BYTE };

		//what type of data is held in the buffer. ie float, int, etc
		[SerializeField]
		DATA_TYPE m_dataType = DATA_TYPE.FLOAT;

		//How many channels has the data, ie a float1, float2, etc
		[SerializeField]
		int m_channels = 1;

		[SerializeField]
		ComputeBufferType m_bufferType = ComputeBufferType.Default;

		public ComputeBufferType GetBufferType() {
			return m_bufferType;
		}

		protected override void Awake() 
		{
			base.Awake();
			
			int tileSize = GetTileSize();
			int capacity = GetCapacity();

			for(int i = 0; i < capacity; i++)
			{
				ComputeBuffer buffer;

				switch((int)m_dataType)
				{
					case (int)DATA_TYPE.FLOAT:
						buffer = new ComputeBuffer(tileSize, sizeof(float) * m_channels, m_bufferType);
						break;

					case (int)DATA_TYPE.INT:
						buffer = new ComputeBuffer(tileSize, sizeof(int) * m_channels, m_bufferType);
						break;

					case (int)DATA_TYPE.BYTE:
						buffer = new ComputeBuffer(tileSize, sizeof(byte) * m_channels, m_bufferType);
						break;

					default:
						buffer = new ComputeBuffer(tileSize, sizeof(float) * m_channels, m_bufferType);
						break;
				};

				CBSlot slot = new CBSlot(this, buffer);

				AddSlot(i, slot);
			}
		}

	}
	
}


























