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
using System.IO;

namespace Oratool.Graphics
{
	class FastByteReader
	{
		readonly byte[] src;
		int offset;

		public FastByteReader(byte[] src, int offset = 0)
		{
			this.src = src;
			this.offset = offset;
		}

		public bool Done() { return offset >= src.Length; }
		public byte ReadByte() { return src[offset++]; }
		public int ReadWord()
		{
			var x = ReadByte();
			return x | (ReadByte() << 8);
		}

		public void CopyTo(byte[] dest, int offset, int count)
		{
			Array.Copy(src, this.offset, dest, offset, count);
			this.offset += count;
		}

		public int Remaining() { return src.Length - offset; }
	}

	public static class Format80
	{
		static void ReplicatePrevious(byte[] dest, int destIndex, int srcIndex, int count)
		{
			if (srcIndex > destIndex)
				throw new NotImplementedException(string.Format("srcIndex > destIndex {0} {1}", srcIndex, destIndex));

			if (destIndex - srcIndex == 1)
			{
				for (var i = 0; i < count; i++)
					dest[destIndex + i] = dest[destIndex - 1];
			}
			else
			{
				for (var i = 0; i < count; i++)
					dest[destIndex + i] = dest[srcIndex + i];
			}
		}

		public static int DecodeInto(byte[] src, byte[] dest, int srcOffset = 0)
		{
			var ctx = new FastByteReader(src, srcOffset);
			var destIndex = 0;

			while (true)
			{
				var i = ctx.ReadByte();
				if ((i & 0x80) == 0)
				{
					// case 2
					var secondByte = ctx.ReadByte();
					var count = ((i & 0x70) >> 4) + 3;
					var rpos = ((i & 0xf) << 8) + secondByte;

					ReplicatePrevious(dest, destIndex, destIndex - rpos, count);
					destIndex += count;
				}
				else if ((i & 0x40) == 0)
				{
					// case 1
					var count = i & 0x3F;
					if (count == 0)
						return destIndex;

					ctx.CopyTo(dest, destIndex, count);
					destIndex += count;
				}
				else
				{
					var count3 = i & 0x3F;
					if (count3 == 0x3E)
					{
						// case 4
						var count = ctx.ReadWord();
						var color = ctx.ReadByte();

						for (var end = destIndex + count; destIndex < end; destIndex++)
							dest[destIndex] = color;
					}
					else if (count3 == 0x3F)
					{
						// case 5
						var count = ctx.ReadWord();
						var srcIndex = ctx.ReadWord();
						if (srcIndex >= destIndex)
							throw new NotImplementedException(string.Format("srcIndex >= destIndex {0} {1}", srcIndex, destIndex));

						for (var end = destIndex + count; destIndex < end; destIndex++)
							dest[destIndex] = dest[srcIndex++];
					}
					else
					{
						// case 3
						var count = count3 + 3;
						var srcIndex = ctx.ReadWord();
						if (srcIndex >= destIndex)
							throw new NotImplementedException(string.Format("srcIndex >= destIndex {0} {1}", srcIndex, destIndex));

						for (var end = destIndex + count; destIndex < end; destIndex++)
							dest[destIndex] = dest[srcIndex++];
					}
				}
			}
		}

		private static int CountSame(byte[] src, int offset, int maxCount)
		{
			maxCount = Math.Min(src.Length - offset, maxCount);
			if (maxCount <= 0)
				return 0;

			byte first = src[offset++];
			int i;

			for (i = 1; i < maxCount && src[offset] == first; i++, offset++);

			return i;
		}

		private static void WriteCopyBlocks(byte[] src, int offset, int count, MemoryStream output)
		{
			while (count > 0)
			{
				int writeNow = Math.Min(count, 0x3F);
				output.WriteByte((byte)(0x80 | writeNow));
				output.Write(src, offset, writeNow);

				count -= writeNow;
				offset += writeNow;
			}
		}

		// Quick and dirty Format80 encoder version 2
		// Uses raw copy and RLE compression
		public static byte[] Encode(byte[] src)
		{
			using (var ms = new MemoryStream())
			{
				int offset = 0, left = src.Length;
				int blockStart = 0;

				while (offset < left)
				{
					int repeatCount = CountSame(src, offset, 0xFFFF);
					if (repeatCount >= 4)
					{
						// Write what we haven't written up to now
						{
							int blockLen = offset - blockStart;
							WriteCopyBlocks(src, blockStart, blockLen, ms);
						}

						// Command 4: Repeat byte n times
						ms.WriteByte(0xFE);
						// Low byte
						ms.WriteByte((byte)(repeatCount & 0xFF));
						// High byte
						ms.WriteByte((byte)(repeatCount >> 8));
						// Value to repeat
						ms.WriteByte(src[offset]);

						offset += repeatCount;
						blockStart = offset;
					}
					else
					{
						offset++;
					}
				}

				// Write what we haven't written up to now
				{
					int blockLen = offset - blockStart;
					WriteCopyBlocks(src, blockStart, blockLen, ms);
				}

				// Write terminator
				ms.WriteByte(0x80);

				return ms.ToArray();
			}
		}
	}
}
