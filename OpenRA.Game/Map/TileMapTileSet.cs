#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using OpenRA.Graphics;
using OpenRA.Support;

namespace OpenRA
{
	public class TileMapTileSet
	{
		public readonly string Id;
		public readonly string Name;
		public readonly string AtlasFile;
		public readonly int2 TileSize;
		public TileMapTile[] Tiles { get; private set; }

		// Runtime
		public readonly float2 AtlasSize;

		public TileMapTileSet()
		{
		}

		public void SetQuadAttributes(Vertex[] vertices, int vertexOffset, int tileIndex, MersenneTwister random)
		{
			var tile = Tiles[tileIndex];
			var tileXy = tile.GetRandomVariant(random);

			var v = vertices[vertexOffset++];
			v.u = (tileXy.X + 0) * TileSize.X / AtlasSize.X;
			v.v = (tileXy.Y + 0) * TileSize.Y / AtlasSize.Y;

			v = vertices[vertexOffset++];
			v.u = (tileXy.X + 1) * TileSize.X / (float)TileSize.X;
			v.v = (tileXy.Y + 0) * TileSize.Y / AtlasSize.Y;

			v = vertices[vertexOffset++];
			v.u = (tileXy.X + 1) * TileSize.X / (float)TileSize.X;
			v.v = (tileXy.Y + 1) * TileSize.Y / AtlasSize.Y;

			v = vertices[vertexOffset++];
			v.u = (tileXy.X + 0) * TileSize.X / (float)TileSize.X;
			v.v = (tileXy.Y + 1) * TileSize.Y / AtlasSize.Y;
		}
	}
}
