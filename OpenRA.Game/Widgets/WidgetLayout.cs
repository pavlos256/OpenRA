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

namespace OpenRA.Widgets
{
	public abstract class WidgetLayout
	{
		public static WidgetLayout CreateLayout(string layoutType)
		{
			return Game.modData.ObjectCreator.CreateObject<WidgetLayout>(layoutType + "Layout");
		}

		public abstract WidgetLayout Clone();

		public abstract void PerformLayout(Widget widget);
	}
}
