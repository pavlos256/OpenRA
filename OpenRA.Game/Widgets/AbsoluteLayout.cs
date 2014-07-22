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
using OpenRA.Support;

namespace OpenRA.Widgets
{
	public class AbsoluteLayout : WidgetLayout
	{
		public override WidgetLayout Clone()
		{
			return new AbsoluteLayout();
		}

		public override void PerformLayout(Widget widget)
		{
			var parentBounds = widget.Bounds;

			var parentSubs = new Dictionary<string, int>();
			parentSubs.Add("WINDOW_RIGHT", Game.Renderer.Resolution.Width);
			parentSubs.Add("WINDOW_BOTTOM", Game.Renderer.Resolution.Height);
			parentSubs.Add("PARENT_RIGHT", parentBounds.Width);
			parentSubs.Add("PARENT_LEFT", parentBounds.Left);
			parentSubs.Add("PARENT_TOP", parentBounds.Top);
			parentSubs.Add("PARENT_BOTTOM", parentBounds.Height);
			parentSubs.Add("PARENT_WIDTH", parentBounds.Width);
			parentSubs.Add("PARENT_HEIGHT", parentBounds.Height);

			foreach (var child in widget.Children)
			{
				// Console.WriteLine("{0} {1}", child.Id, child.Width);

				if (child.LegacyLayout)
					continue;

				var subs = parentSubs;
				if (child.LayoutVariables != null && child.LayoutVariables.Count > 0)
				{
					subs = new Dictionary<string, int>(parentSubs);
					foreach (var kvp in child.LayoutVariables)
						subs[kvp.Key] = kvp.Value;
				}

				var width = Evaluator.Evaluate(child.Width, subs);
				var height = Evaluator.Evaluate(child.Height, subs);

				subs["WIDTH"] = width;
				subs["HEIGHT"] = height;

				child.Bounds = new Rectangle(
					Evaluator.Evaluate(child.X, subs),
					Evaluator.Evaluate(child.Y, subs),
					width,
					height);
			}
		}
	}
}
