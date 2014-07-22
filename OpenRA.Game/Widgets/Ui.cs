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
using OpenRA.Graphics;
using OpenRA;

namespace OpenRA.Widgets
{
	public static class Ui
	{
		static Lazy<RootWidget> root = Exts.Lazy(() =>
		{
			var r = new RootWidget();
			r.Bounds = new Rectangle(Point.Empty, Game.Renderer.Resolution);
			r.Layout = new AbsoluteLayout();
			return r;
		});
		public static Widget Root { get { return root.Value; } }

		public static int LastTickTime = Environment.TickCount;

		static Stack<Widget> WindowList = new Stack<Widget>();

		public static Widget MouseFocusWidget;
		public static Widget KeyboardFocusWidget;
		public static Widget MouseOverWidget;

		public static void CloseWindow()
		{
			if (WindowList.Count > 0)
				Root.RemoveChild(WindowList.Pop());
			if (WindowList.Count > 0)
				Root.AddChild(WindowList.Peek());
		}

		public static Widget OpenWindow(string id)
		{
			return OpenWindow(id, new WidgetArgs());
		}

		public static Widget OpenWindow(string id, WidgetArgs args)
		{
			var window = Game.modData.WidgetLoader.LoadWidget(args, Root, id);
			if (WindowList.Count > 0)
				Root.RemoveChild(WindowList.Peek());
			WindowList.Push(window);
			return window;
		}

		public static T LoadWidget<T>(string id, Widget parent, WidgetArgs args) where T : Widget
		{
			var widget = LoadWidget(id, parent, args) as T;
			if (widget == null)
				throw new InvalidOperationException(
					"Widget {0} is not of type {1}".F(id, typeof(T).Name));
			return widget;
		}

		public static Widget LoadWidget(string id, Widget parent, WidgetArgs args)
		{
			return Game.modData.WidgetLoader.LoadWidget(args, parent, id);
		}

		public static void Tick()
		{
			Root.TickOuter();
		}

		public static void Draw()
		{
			Root.DrawOuter();
		}

		public static bool HandleInput(MouseInput mi)
		{
			var wasMouseOver = MouseOverWidget;

			if (mi.Event == MouseInputEvent.Move)
				MouseOverWidget = null;

			var handled = false;
			if (MouseFocusWidget != null && MouseFocusWidget.HandleMouseInputOuter(mi))
				handled = true;

			if (!handled && Root.HandleMouseInputOuter(mi))
				handled = true;

			if (mi.Event == MouseInputEvent.Move)
			{
				Viewport.LastMousePos = mi.Location;
				Viewport.TicksSinceLastMove = 0;
			}

			if (wasMouseOver != MouseOverWidget)
			{
				if (wasMouseOver != null)
					wasMouseOver.MouseExited();

				if (MouseOverWidget != null)
					MouseOverWidget.MouseEntered();
			}

			return handled;
		}

		public static bool HandleKeyPress(KeyInput e)
		{
			if (KeyboardFocusWidget != null)
				return KeyboardFocusWidget.HandleKeyPressOuter(e);

			return Root.HandleKeyPressOuter(e);
		}

		public static bool HandleTextInput(string text)
		{
			if (KeyboardFocusWidget != null)
				return KeyboardFocusWidget.HandleTextInputOuter(text);

			return Root.HandleTextInputOuter(text);
		}

		public static void ResetAll()
		{
			Root.RemoveChildren();

			while (WindowList.Count > 0)
				CloseWindow();
		}
	}
}
