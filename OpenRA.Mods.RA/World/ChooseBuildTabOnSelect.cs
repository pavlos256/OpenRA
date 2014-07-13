#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Mods.RA.Widgets;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.RA
{
	class ChooseBuildTabOnSelectInfo : ITraitInfo
	{
		public object Create(ActorInitializer init) { return new ChooseBuildTabOnSelect(init); }
	}

	class ChooseBuildTabOnSelect : INotifySelection
	{
		readonly World world;

		public ChooseBuildTabOnSelect(ActorInitializer init)
		{
			world = init.world;
		}

		public void SelectionChanged()
		{
			var palette = Ui.Root.GetOrNull<ProductionPaletteWidget>("PRODUCTION_PALETTE");
			if (palette == null)
				return;

			// Queue-per-structure
			var perqueue = world.Selection.Actors.FirstOrDefault(a => a.IsInWorld && a.World.LocalPlayer == a.Owner
				&& a.TraitsImplementing<ProductionQueue>().Any(q => q.Enabled));

			if (perqueue != null)
			{
				palette.CurrentQueue = perqueue.TraitsImplementing<ProductionQueue>().First(q => q.Enabled);
				return;
			}

			// Queue-per-player
			var types = world.Selection.Actors.Where(a => a.IsInWorld && (a.World.LocalPlayer == a.Owner))
				.SelectMany(a => a.TraitsImplementing<Production>())
				.SelectMany(t => t.Info.Produces)
				.ToArray();

			if (types.Length == 0)
				return;

			palette.CurrentQueue = world.LocalPlayer.PlayerActor.TraitsImplementing<ProductionQueue>()
				.FirstOrDefault(q => q.Enabled && types.Contains(q.Info.Type));
		}
	}
}
