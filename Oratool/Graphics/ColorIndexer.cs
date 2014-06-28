//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-28
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Drawing.Imaging;

namespace Oratool.Graphics
{
	public class ColorIndexer
	{
		private Tuple<byte, D3>[] availableColors;
		private byte transparentIndex;
		private byte alphaThreshold;
		private int[] cache;

		public ColorIndexer(IEnumerable<IndexedColor> availableColors, byte transparentIndex = 0, byte alphaThreshold = 128)
		{
			this.availableColors = availableColors.Select(ic => Tuple.Create(ic.Index, ic.Color.ToCIELab())).ToArray();
			this.transparentIndex = 0;
			this.alphaThreshold = alphaThreshold;

			this.cache = new int[256 * 256 * 256];
			for (int i = 0; i < this.cache.Length; i++)
				this.cache[i] = -1;
		}

		private byte FindBestIndex(Color c)
		{
			if (c.A > this.alphaThreshold)
			{
				byte bestIndex = 0;

				int cacheIndex = c.ToArgb() & 0xFFFFFF;
				if (this.cache[cacheIndex] >= 0)
					return (byte)this.cache[cacheIndex];

				D3 lab = c.ToCIELab();
				double minDelta = double.MaxValue;

				foreach (var av in this.availableColors)
				{
					double d = ColorDelta.DeltaE(lab, av.Item2);
					if (d >= minDelta)
						continue;

					minDelta = d;
					bestIndex = av.Item1;
				}

				this.cache[cacheIndex] = bestIndex;

				return bestIndex;
			}
			else
			{
				return this.transparentIndex;
			}
		}

		public byte[] Index(Bitmap bitmap)
		{
			byte[] indexed = new byte[bitmap.Size.Width * bitmap.Size.Height];

			var bits = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				unsafe
				{
					for (int y = 0, t = 0; y < bitmap.Size.Height; y++)
					{
						int* p = (int*)(bits.Scan0 + bits.Stride * y);

						for (int x = 0; x < bitmap.Size.Width; x++, t++, p++)
						{
							Color c = Color.FromArgb(*p);

							indexed[t] = FindBestIndex(c);
						}
					}
				}
			}
			finally
			{
				bitmap.UnlockBits(bits);
			}

			return indexed;
		}
	}

	public struct IndexedColor
	{
		public readonly byte Index;
		public readonly Color Color;

		public IndexedColor(byte index, Color color)
		{
			this.Index = index;
			this.Color = color;
		}
	}
}
