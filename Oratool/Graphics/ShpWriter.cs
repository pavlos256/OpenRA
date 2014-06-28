#region Copyright & License Information
/*
 * Copyright 2007-2013 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Oratool.Graphics
{
	public class ShpWriter
	{
		enum Format
		{
			Null = 0x00,
			Format20 = 0x20,
			Format40 = 0x40,
			Format80 = 0x80
		}

		private static void WriteImageHeader(BinaryWriter writer, uint fileOffset, Format format)
		{
			writer.Write(fileOffset | ((uint)format << 24));
			writer.Write((ushort)0); // RefOffset
			writer.Write((ushort)0); // RefFormat
		}

		public static void Write(Stream outputStream, Size size, IEnumerable<byte[]> frames)
		{
			var compressedFrames = frames.Select(f => Format80.Encode(f)).ToArray();

			// note: end-of-file and all-zeroes headers
			var dataOffset = 14 + (compressedFrames.Length + 2) * 8;

			using (var writer = new BinaryWriter(outputStream))
			{
				writer.Write((ushort)compressedFrames.Length);
				writer.Write((ushort)0);
				writer.Write((ushort)0);
				writer.Write((ushort)size.Width);
				writer.Write((ushort)size.Height);
				writer.Write((uint)0);

				foreach (var f in compressedFrames)
				{
					WriteImageHeader(writer, (uint)dataOffset, Format.Format80);

					dataOffset += f.Length;
				}

				// EOF
				WriteImageHeader(writer, (uint)dataOffset, Format.Null);
				// All zeroes
				WriteImageHeader(writer, 0, 0);

				foreach (var f in compressedFrames)
					writer.Write(f);
			}
		}
	}
}
