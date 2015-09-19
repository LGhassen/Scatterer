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
	* A TileStorage that stores tiles in 2D textures
	*/
	public class GPUTileStorage : TileStorage 
	{
	
     	//A slot managed by a GPUTileStorage containing the texture
		public class GPUSlot : Slot
		{
			RenderTexture m_texture;

			public RenderTexture GetTexture() {
				return m_texture;
			}

			public override void Release() {
				if(m_texture != null) m_texture.Release();
			}

			public GPUSlot(TileStorage owner, RenderTexture texture) : base(owner) {
				m_texture = texture;
			}
		};

		[SerializeField]
		RenderTextureFormat m_internalFormat = RenderTextureFormat.ARGB32;

		[SerializeField]
		TextureWrapMode m_wrapMode = TextureWrapMode.Clamp;

		[SerializeField]
		FilterMode m_filterMode = FilterMode.Point;

		[SerializeField]
		RenderTextureReadWrite m_readWrite;

		[SerializeField]
		bool m_mipmaps;

		[SerializeField]
		bool m_enableRandomWrite;

		[SerializeField]
		int m_ansio;

		public RenderTextureFormat GetInternalFormat() {
			return m_internalFormat;
		}

		public TextureWrapMode GetWrapMode() {
			return m_wrapMode;
		}

		public FilterMode GetFilterMode() {
			return m_filterMode;
		}

		public RenderTextureReadWrite GetReadWrite() {
			return m_readWrite;
		}

		public bool HasMipMaps() {
			return m_mipmaps;
		}

		public bool RandomWriteEnabled() {
			return m_enableRandomWrite;
		}

		public int GetAnsioLevel() {
			return m_ansio;
		}

		protected override void Awake() 
		{
			base.Awake();

			int tileSize = GetTileSize();
			int capacity = GetCapacity();

			for(int i = 0; i < capacity; i++)
			{
				RenderTexture texture = new RenderTexture(tileSize, tileSize, 0, m_internalFormat, m_readWrite);
				texture.filterMode = m_filterMode;
				texture.wrapMode = m_wrapMode;
				texture.useMipMap = m_mipmaps;
				texture.anisoLevel = m_ansio;
				texture.enableRandomWrite = m_enableRandomWrite;

				GPUSlot slot = new GPUSlot(this, texture);

				AddSlot(i, slot);
			}
		}

	}

}


























