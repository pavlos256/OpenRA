#region Copyright & License Information
/*
 * Copyright 2007-2014 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Drawing;
using System.Linq;
using OpenRA.Mods.RA;
using OpenRA.Mods.RA.Buildings;
using OpenRA.Mods.RA.Widgets;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Cnc.Widgets.Logic
{
	public class CncIngameChromeLogic
	{
		readonly Widget ingameRoot;
		readonly World world;

		[ObjectCreator.UseCtor]
		public CncIngameChromeLogic(Widget widget, World world)
		{
			this.world = world;
			ingameRoot = widget.Get("INGAME_ROOT");
			var playerRoot = ingameRoot.Get("PLAYER_ROOT");

			// Observer
			if (world.LocalPlayer == null)
				InitObserverWidgets(world, playerRoot);
			else
				InitPlayerWidgets(world, playerRoot);

			Game.LoadWidget(world, "CHAT_PANEL", ingameRoot, new WidgetArgs());
		}

		public void OptionsClicked()
		{
			var cachedPause = world.PredictedPaused;

			ingameRoot.IsVisible = () => false;
			if (world.LobbyInfo.IsSinglePlayer)
				world.SetPauseState(true);

			Game.LoadWidget(world, "INGAME_MENU", Ui.Root, new WidgetArgs()
			{
				{ "onExit", () =>
					{
						ingameRoot.IsVisible = () => true;
						if (world.LobbyInfo.IsSinglePlayer)
							world.SetPauseState(cachedPause);
					}
				}
			});
		}

		public void InitObserverWidgets(World world, Widget playerRoot)
		{
			var observerWidgets = Game.LoadWidget(world, "OBSERVER_WIDGETS", playerRoot, new WidgetArgs());
			observerWidgets.Get<ButtonWidget>("OPTIONS_BUTTON").OnClick = OptionsClicked;
		}

		public void InitPlayerWidgets(World world, Widget playerRoot)
		{
			// Real player
			var playerWidgets = Game.LoadWidget(world, "PLAYER_WIDGETS", playerRoot, new WidgetArgs());
			playerWidgets.IsVisible = () => true;
		}
	}
}
