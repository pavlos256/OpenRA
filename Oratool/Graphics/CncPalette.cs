//
// Author(s):
//     Pavlos Touboulidis <pav@pav.gr>
//
// Created on 2014-6-26
//
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Oratool.Graphics
{
	public class CncPalette
	{
		public readonly Color[] Colors;

		public CncPalette()
		{
			this.Colors = new Color[256];
		}

		public CncPalette(byte[] paletteData)
			: this()
		{
			this.Colors = LoadPaletteData(paletteData);
		}

		public CncPalette(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			using (var fs = File.OpenRead(path))
			{
				#if DEBUG
				if (fs.Length != 256 * 3)
					Console.Error.WriteLine("Warning: file '{0}' does not look like a PAL file.", path);
				#endif

				byte[] data = new byte[256 * 3];
				int offset = 0;
				while ((offset += fs.Read(data, offset, data.Length - offset)) < data.Length);

				this.Colors = LoadPaletteData(data);
			}
		}

		private static Color[] LoadPaletteData(byte[] paletteData)
		{
			if (paletteData == null)
				throw new ArgumentNullException("paletteData");

			if (paletteData.Length != 256 * 3)
				throw new ArgumentException(string.Format("Expected 768 bytes but found {0} bytes.", paletteData.Length));

			var colors = new Color[256];
			colors[0] = Color.Empty;

			for (int i = 1, k = i * 3; i < 256; i++)
			{
				colors[i] = Color.FromArgb(255, paletteData[k++] << 2, paletteData[k++] << 2, paletteData[k++] << 2);
			}

			return colors;
		}
	}
}
