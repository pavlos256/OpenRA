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
using System.Drawing;
using OpenRA.Support;

namespace OpenRA
{
	public class TileMapTile
	{
		public readonly string Type;
		public readonly Color MinimapColor;
		public int2[] Variants { get; private set; }

		public TileMapTile()
		{
		}

		public int2 GetRandomVariant(MersenneTwister random)
		{
			return Variants[random.Next(Variants.Length)];
		}
	}
}
