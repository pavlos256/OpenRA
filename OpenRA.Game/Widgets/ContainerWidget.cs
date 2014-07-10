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

namespace OpenRA.Widgets
{
	public class ContainerWidget : Widget
	{
		public ContainerWidget()
		{
			IgnoreMouseOver = true;
		}

		public ContainerWidget(ContainerWidget other)
			: base(other)
		{
			IgnoreMouseOver = true;
		}

		public override string GetCursor(int2 pos)
		{
			return null;
		}

		public override Widget Clone()
		{
			return new ContainerWidget(this);
		}

		public Func<KeyInput, bool> OnKeyPress = _ => false;

		public override bool HandleKeyPress(KeyInput e)
		{
			return OnKeyPress(e);
		}
	}
}
