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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Support;

namespace OpenRA.Widgets
{
	public class BoxLayout : WidgetLayout
	{
		public Padding Padding;
		public int ItemSpacing;
		public BoxOrientation Orientation;
		public BoxMainAlign MainAlign;
		public BoxCrossAlign CrossAlign;
		public Size MinSize;
		public Size MaxSize;

		Dictionary<string, int> exprSubstitutions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		public BoxLayout()
		{
		}

		public BoxLayout(BoxLayout other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			Padding = other.Padding;
			ItemSpacing = other.ItemSpacing;
			Orientation = other.Orientation;
		}

		public override WidgetLayout Clone()
		{
			return new BoxLayout(this);
		}

		class LayoutItem
		{
			public readonly Widget Widget;

			public readonly int InitialMainLength;
			public readonly int MinMainLength;
			public readonly int MaxMainLength;

			public readonly int InitialCrossLength;
			public readonly int MinCrossLength;
			public readonly int MaxCrossLength;

			public int AllocatedLength;
			public int CurrentLength;

			public LayoutItem(BoxLayout layout, Widget item, int fixedMainLength, int fixedCrossLength, BoxOrientation orientation)
			{
				Widget = item;

				var minSize = layout.MinSize;
				if (item.MinSize.Width > 0)
					minSize.Width = item.MinSize.Width;
				if (item.MinSize.Height > 0)
					minSize.Height = item.MinSize.Height;

				var maxSize = layout.MaxSize;
				if (item.MaxSize.Width > 0)
					maxSize.Width = item.MaxSize.Width;
				if (item.MaxSize.Height > 0)
					maxSize.Height = item.MaxSize.Height;

				InitialMainLength = fixedMainLength;
				MinMainLength = Math.Max(0, minSize.GetMainLength(orientation));
				MaxMainLength = maxSize.GetMainLength(orientation);
				if (InitialMainLength > 0 && MinMainLength > 0 && InitialMainLength < MinMainLength)
					InitialMainLength = MinMainLength;
				if (InitialMainLength > 0 && MaxMainLength > 0 && InitialMainLength > MaxMainLength)
					InitialMainLength = MaxMainLength;
				if (MinMainLength == MaxMainLength && MinMainLength > 0)
					InitialMainLength = MinMainLength;

				InitialCrossLength = fixedCrossLength;
				MinCrossLength = Math.Max(0, minSize.GetCrossLength(orientation));
				MaxCrossLength = maxSize.GetCrossLength(orientation);
				if (InitialCrossLength > 0 && MinCrossLength > 0 && InitialCrossLength < MinCrossLength)
					InitialCrossLength = MinCrossLength;
				if (InitialCrossLength > 0 && MaxCrossLength > 0 && InitialCrossLength > MaxCrossLength)
					InitialCrossLength = MaxCrossLength;
				if (MinCrossLength == MaxCrossLength && MinCrossLength > 0)
					InitialCrossLength = MinCrossLength;
			}
		}

		public override void PerformLayout(Widget widget)
		{
			if (widget.Children.Count == 0)
				return;

			var containerArea = new Rectangle(Point.Empty, widget.Bounds.Size);
			var area = new Rectangle(0, 0, containerArea.Width - Padding.Left - Padding.Right, containerArea.Height - Padding.Top - Padding.Bottom);
			area.Intersect(containerArea);
			var areaMainLength = area.GetMainLength(Orientation);
			var areaCrossLength = area.GetCrossLength(Orientation);

			exprSubstitutions["PARENT_WIDTH"] = area.Width;
			exprSubstitutions["PARENT_HEIGHT"] = area.Height;

			// Create a state object for each child we want to layout
			var items = widget.Children
				.Where(child =>
				{
					// Do not layout collapsed widgets
					if (child.CollapseWhenHidden && !child.IsVisible())
						return false;

					return true;
				})
				// This reuses the same dictionary instance for all children
				.Select(child =>
				{
					var mainLength = 0;
					var mainExpr = IsHorz ? child.Width : child.Height;
					if (mainExpr != "0")
					{
						exprSubstitutions["parent_dim"] = areaMainLength;
						mainExpr = mainExpr.Replace("%", "* parent_dim / 100");

						mainLength = Evaluator.Evaluate(mainExpr, exprSubstitutions);
					}

					var crossLength = 0;
					var crossExpr = IsHorz ? child.Height : child.Width;
					if (crossExpr != "0")
					{
						exprSubstitutions["parent_dim"] = areaCrossLength;
						crossExpr = crossExpr.Replace("%", "* parent_dim / 100");

						crossLength = Evaluator.Evaluate(crossExpr, exprSubstitutions);
					}

					return new LayoutItem(this, child, mainLength, crossLength, Orientation);

				}).ToArray();

			// Subtract children margins and spacing from the available area
			var availMainLength = areaMainLength;
			foreach (var item in items)
			{
				var child = item.Widget;
				var margin = IsHorz ? child.Margin.Left + child.Margin.Right : child.Margin.Top + child.Margin.Bottom;
				availMainLength -= margin;
			}
			availMainLength -= ItemSpacing * (items.Length - 1);

			AssignMainLengths(items, availMainLength);
			AssignMainPositions(items, areaMainLength);
			AssignCrossLengths(items, areaCrossLength);
			AssignCrossPositions(items, areaCrossLength);
		}

		void AssignMainLengths(LayoutItem[] allItems, int availableLength)
		{
			var items = allItems.ToList();

			// If the item has a fixed size and is not flexible,
			// allocate space for it and stop processing it.
			items.RemoveAll(item =>
			{
				if (item.InitialMainLength > 0 && item.Widget.Flex <= 0)
				{
					item.AllocatedLength = item.InitialMainLength;
					availableLength = Math.Max(0, availableLength - item.AllocatedLength);
					item.CurrentLength = item.AllocatedLength;

					return true;
				}

				return false;
			});

			// For each item that has a minimum length constraint, allocate
			// the required length.
			foreach (var item in items)
			{
				if (item.MinMainLength > 0)
				{
					item.AllocatedLength = item.MinMainLength;
					availableLength = Math.Max(0, availableLength - item.AllocatedLength);
					item.CurrentLength = item.AllocatedLength;
				}
			}

			while (items.Count > 0)
			{
				// Step 1:
				// Assign lengths according to their flex value
				var flexSum = items.Sum(item => Math.Max(1, item.Widget.Flex));

				var left = availableLength;
				foreach (var item in items)
				{
					item.CurrentLength = availableLength * Math.Max(1, item.Widget.Flex) / flexSum;
					left -= item.CurrentLength;
				}
				// Allot the remainder
				while (left > 0)
				{
					foreach (var item in items)
					{
						if (--left >= 0)
							item.CurrentLength++;
						else
							break;
					}
				}

				// Step 2:
				// If any item has been given more than it already has allocated,
				// release the preallocated portion back to the pool and restart.
				var released = 0;
				foreach (var item in items)
				{
					if (item.AllocatedLength > 0 && item.CurrentLength >= item.AllocatedLength)
					{
						released += item.AllocatedLength;
						item.AllocatedLength = 0;
					}
				}
				if (released > 0)
				{
					availableLength += released;
				}
				else
				{
					// Step 3:
					// If any item has been given more than its max,
					// limit it to the max, release excess, stop processing it,
					// then restart.
					var removed = items.RemoveAll(item =>
					{
						if (item.MaxMainLength > 0 && item.CurrentLength > item.MaxMainLength)
						{
							item.CurrentLength = item.MaxMainLength;
							availableLength -= item.CurrentLength;
							return true;
						}

						return false;
					});

					if (removed == 0)
					{
						// Step 4:
						// If the alloted length is less than the already allocated
						// length then we can't give any more.
						// Use the preallocated length to release the surplus to the
						// others, stop processing it and restart.
						var idx = items.FindIndex(item => item.AllocatedLength > 0 && item.CurrentLength < item.AllocatedLength);
						if (idx >= 0)
						{
							var item = items[idx];
							items.RemoveAt(idx);
							item.CurrentLength = item.AllocatedLength;
						}
						else
						{
							// End
							break;
						}
					}
				}
			}

			// Copy the values from the working variables
			foreach (var item in allItems)
			{
				if (IsHorz)
					item.Widget.Bounds.Width = item.CurrentLength;
				else
					item.Widget.Bounds.Height = item.CurrentLength;
			}
		}

		void AssignMainPositions(LayoutItem[] items, int availableLength)
		{
			var totalItemsLength = 0;
			foreach (var item in items)
			{
				var widget = item.Widget;

				if (IsHorz)
					totalItemsLength += widget.Margin.Left + widget.Bounds.Width + widget.Margin.Right;
				else
					totalItemsLength += widget.Margin.Top + widget.Bounds.Height + widget.Margin.Bottom;
			}
			totalItemsLength += ItemSpacing * (items.Length - 1);

			var freeSpace = availableLength - totalItemsLength;
			var start = IsHorz ? Padding.Left : Padding.Top;
			var spaceBefore = 0d;
			var spaceAfter = 0d;

			switch (MainAlign)
			{
				case BoxMainAlign.End:
					start += availableLength - totalItemsLength;
					break;

				case BoxMainAlign.Center:
					start += (availableLength - totalItemsLength) / 2;
					break;

				case BoxMainAlign.SpaceBetween:
					if (items.Length >= 2)
						spaceAfter = Math.Max(0d, ((double)freeSpace) / (items.Length - 1));
					break;

				case BoxMainAlign.SpaceAround:
					spaceBefore = Math.Max(0d, ((double)freeSpace) / items.Length / 2d);
					spaceAfter = spaceBefore;
					break;
			}

			var spaceRemainder = 0d;

			for (var i = 0; i < items.Length; i++)
			{
				var widget = items[i].Widget;

				// Space before
				var iSpaceBefore = (int)(spaceBefore + spaceRemainder);
				start += iSpaceBefore;
				spaceRemainder += spaceBefore - iSpaceBefore;

				// Margin, Set position, Body size, Margin
				if (IsHorz)
				{
					start += widget.Margin.Left;
					widget.Bounds.X = start;
					start += widget.Bounds.Width;
					start += widget.Margin.Right;
				}
				else
				{
					start += widget.Margin.Top;
					widget.Bounds.Y = start;
					start += widget.Bounds.Height;
					start += widget.Margin.Bottom;
				}

				var iSpaceAfter = (int)(spaceAfter + spaceRemainder);
				start += iSpaceAfter;
				spaceRemainder += spaceAfter - iSpaceAfter;

				start += ItemSpacing;
			}
		}

		void AssignCrossLengths(LayoutItem[] items, int availableLength)
		{
			foreach (var item in items)
			{
				var widget = item.Widget;

				if (item.InitialCrossLength == 0 || CrossAlign == BoxCrossAlign.Stretch)
				{
					var margins = IsHorz ? widget.Margin.Top + widget.Margin.Bottom : widget.Margin.Left + widget.Margin.Right;
					item.CurrentLength = availableLength - margins;
					if (item.CurrentLength < item.MinCrossLength)
						item.CurrentLength = item.MinCrossLength;
					if (item.MaxCrossLength > 0 && item.CurrentLength > item.MaxCrossLength)
						item.CurrentLength = item.MaxCrossLength;
				}
				else
					item.CurrentLength = item.InitialCrossLength;

				if (IsHorz)
					widget.Bounds.Height = item.CurrentLength;
				else
					widget.Bounds.Width = item.CurrentLength;
			}
		}

		void AssignCrossPositions(LayoutItem[] items, int availableLength)
		{
			foreach (var item in items)
			{
				var widget = item.Widget;

				int start;

				switch (CrossAlign)
				{
					case BoxCrossAlign.End:
						start = availableLength;
						if (IsHorz)
							start += Padding.Top - widget.Margin.Bottom - widget.Bounds.Height;
						else
							start += Padding.Left - widget.Margin.Right - widget.Bounds.Width;
						break;

					case BoxCrossAlign.Center:
						start = (IsHorz ? Padding.Top : Padding.Left) + (availableLength - (IsHorz ? widget.Bounds.Height : widget.Bounds.Width)) / 2;
						break;

					default:
						// Skip parent padding and child margin
						start = IsHorz ? (Padding.Top + widget.Margin.Top) : (Padding.Left + widget.Margin.Left);
						break;
				}

				if (IsHorz)
					widget.Bounds.Y = start;
				else
					widget.Bounds.X = start;
			}
		}

		bool IsHorz { get { return Orientation == BoxOrientation.Horizontal; } }
	}

	public enum BoxOrientation
	{
		Horizontal,
		Vertical
	}

	public enum BoxMainAlign
	{
		Start,
		End,
		Center,
		SpaceBetween,
		SpaceAround
	}

	public enum BoxCrossAlign
	{
		Start,
		End,
		Center,
		Stretch
	}

	public static class BoxExtensions
	{
		public static int GetMainLength(this Size size, BoxOrientation orientation)
		{
			return orientation == BoxOrientation.Horizontal ? size.Width : size.Height;
		}

		public static int GetCrossLength(this Size size, BoxOrientation orientation)
		{
			return orientation == BoxOrientation.Horizontal ? size.Height : size.Width;
		}

		public static int GetMainLength(this Rectangle rect, BoxOrientation orientation)
		{
			return orientation == BoxOrientation.Horizontal ? rect.Width : rect.Height;
		}

		public static int GetCrossLength(this Rectangle rect, BoxOrientation orientation)
		{
			return orientation == BoxOrientation.Horizontal ? rect.Height : rect.Width;
		}
	}
}
