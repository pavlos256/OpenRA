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
using System.ComponentModel;
using System.Globalization;

namespace OpenRA.Widgets
{
	[TypeConverter(typeof(PaddingConverter))]
	public struct Padding : IEquatable<Padding>
	{
		public static readonly Padding Empty = new Padding();

		public int Left, Top, Right, Bottom;

		public Padding(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public Padding(int horizontal, int vertical)
			: this(horizontal, vertical, horizontal, vertical) { }

		public Padding(int all)
			: this(all, all, all, all) { }

		public static bool operator ==(Padding a, Padding b)
		{
			return a.Left == b.Left
				&& a.Top == b.Top
				&& a.Right == b.Right
				&& a.Bottom == b.Bottom;
		}

		public static bool operator !=(Padding a, Padding b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Left ^ Top ^ Right ^ Bottom;
		}

		public bool Equals(Padding other)
		{
			return this == other;
		}

		public override bool Equals(object other)
		{
			if (other is Padding)
				return this == (Padding)other;

			return false;
		}

		public string ToString(IFormatProvider format)
		{
			return string.Format(format ?? CultureInfo.CurrentCulture, "{0}, {1}, {2}, {3}", Left, Top, Right, Bottom);
		}

		public override string ToString()
		{
			return ToString(null);
		}

		public static bool TryParse(string text, NumberStyles style, IFormatProvider format, out Padding result)
		{
			if (text == null)
				throw new ArgumentNullException("text");

			result = Padding.Empty;

			var parts = text.Trim().Split(',');
			if (parts.Length == 0)
				return false;

			var values = new int[parts.Length];
			for (var i = 0; i < parts.Length; i++)
			{
				int value;
				if (int.TryParse(parts[i], style, format, out value))
					values[i] = value;
				else
					return false;
			}

			if (values.Length == 1)
				result = new Padding(values[0]);
			else if (values.Length == 2)
				result = new Padding(values[0], values[1]);
			else if (values.Length == 4)
				result = new Padding(values[0], values[1], values[2], values[3]);
			else
				return false;

			return true;
		}
	}

	public class PaddingConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			Padding result;
			if (Padding.TryParse((string)value, NumberStyles.Integer, culture, out result))
				return result;

			throw new FormatException("Input string was not in the correct format");
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return ((Padding)value).ToString(culture);
		}
	}
}
