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
using System.Drawing.Imaging;
using OpenRA.Graphics;
using OpenRA.Support;
using OpenRA.FileFormats;

namespace OpenRA
{
	public class HeightmapTerrain
	{
		// The heightmap bitmap
		public readonly string HeightmapFile;

		// The number of usable height bits-per-pixel in the heightmap bitmap
		public readonly int HeightmapBits;

		// How many height units to get a length equal to the width of the tile
		public readonly int HeightResolution;

		// This value will be mapped to height 0.
		// Lower values will become negative heights (below water)
		public readonly int WaterLevel;

		// The file that defines the surface's materials
		public readonly string TilesFile;

		// Runtime
		// state
		// members
		public readonly Size Size;
		Vertex[] vertices;
		CellLayer<byte> tileMap;

		public HeightmapTerrain()
		{
		}

		void InitializeRuntime(TileMapTileSet tileSet, MersenneTwister random)
		{
			var heightmap = LoadHeightmap(HeightmapFile, HeightmapBits, WaterLevel, out Size);
			// The size is in corners, we want tiles now
			Size.Width--;
			Size.Height--;

			tileMap = LoadTileMap(TilesFile);
			if (tileMap.Size != Size)
				throw new ArgumentException(
					"The tileMap size ({0}x{1}) is not the one expected ({2}x{3}).".F(
						tileMap.Size.Width, tileMap.Size.Height, Size.Width, Size.Height));

			vertices = BuiltHeightmapVertices(heightmap, Size);

			for (int y = 0, i = 0; y < Size.Height; y++)
				for (var x = 0; x < Size.Width; x++, i += 4)
					tileSet.SetQuadAttributes(vertices, i, tileMap[x, y], random);
		}

		static int[] LoadHeightmap(string path, int bpp, int waterLevel, out Size size)
		{
			using (var bitmap = new Bitmap(path))
			{
				if (bitmap.Size.Width < 2 || bitmap.Size.Height < 2)
					throw new InvalidOperationException("None of the heightmap's dimensions can be smaller than 2");

				size = bitmap.Size;

				var map = new int[bitmap.Size.Width * bitmap.Size.Height];
				var bits = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
				try
				{
					unsafe
					{
						var mask = (1 << bpp) - 1;

						for (int y = 0, t = 0; y < bitmap.Size.Height; y++)
						{
							int* p = (int*)(bits.Scan0 + bits.Stride * y);

							for (int x = 0; x < bitmap.Size.Width; x++, t++, p++)
							{
								int value = (*p & mask) - waterLevel;

								map[t] = value;
							}
						}
					}

					return map;
				}
				finally
				{
					bitmap.UnlockBits(bits);
				}
			}
		}

		static Vertex[] BuiltHeightmapVertices(int[] heightmap, Size size)
		{
			// The heightmap array has one extra column and one extra row

			var hStride = size.Width + 1;
			var vertices = new Vertex[4 * size.Width * size.Height];
			for (int y = 0, t = 0; y < size.Height; y++)
			{
				int hIdx = y * hStride;

				for (int x = 0; x < size.Width; x++, hIdx++)
				{
					// x -> x, y -> z, height -> y
					vertices[t++] = new Vertex(new float[3] { x + 0, heightmap[hIdx + 0], y + 0 }, float2.Zero, float2.Zero);
					vertices[t++] = new Vertex(new float[3] { x + 1, heightmap[hIdx + 1], y + 0 }, float2.Zero, float2.Zero);
					vertices[t++] = new Vertex(new float[3] { x + 1, heightmap[hIdx + 1 + hStride], y + 1 }, float2.Zero, float2.Zero);
					vertices[t++] = new Vertex(new float[3] { x + 0, heightmap[hIdx + 0 + hStride], y + 1 }, float2.Zero, float2.Zero);
				}
			}

			return vertices;
		}

		static CellLayer<byte> LoadTileMap(string path)
		{
			using (var bitmap = PngLoader.Load(path))
			{
				var map = new byte[bitmap.Size.Width * bitmap.Size.Height];
				var bits = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);
				try
				{
					unsafe
					{
						for (int y = 0, t = 0; y < bitmap.Size.Height; y++)
						{
							byte* p = (byte*)(bits.Scan0 + bits.Stride * y);

							for (int x = 0; x < bitmap.Size.Width; x++, t++, p++)
							{
								map[t] = *p;
							}
						}
					}

					return new CellLayer<byte>(bitmap.Size, map);
				}
				finally
				{
					bitmap.UnlockBits(bits);
				}
			}
		}
	}
}
