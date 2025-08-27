﻿/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using Windows.ApplicationModel.VoiceCommands;

namespace sambar;
public partial class Api
{
	private void DropdownMenuInit()
	{

	}
	public Menu CreateMenu(UserControl callingElement, int width = 100, int height = 100)
	{

		int x = (int)callingElement.PointToScreen(new Point(callingElement.Width / 2, callingElement.Height / 2)).X - (width / 2);
		x = x < Sambar.api.config.marginXLeft ? Sambar.api.config.marginXLeft : x;
		int y = Sambar.api.config.marginYTop + Sambar.api.config.height + 5;

		Menu menu = new(x, y, width, height);
		return menu;
	}
	
	// context menu with menubuttons
	public Menu CreateContextMenu(List<MenuButton> items) 
	{
		User32.GetCursorPos(out POINT pt);
		ContextMenu menu = new(pt.X, pt.Y, 100, items.Count*30);
		StackPanel panel = new();
		panel.Orientation = Orientation.Vertical;
		foreach(var item in items)
		{
			panel.Children.Add(item);
		}
		menu.Content = panel;
		return menu;
	}
}

public class Menu : Window
{
	nint hWnd;
	int _left, _top, _right, _bottom;
	public Menu(int x, int y, int width, int height)
	{
		this.Title = "SambarContextMenu";
		this.WindowStyle = WindowStyle.None;
		this.Topmost = true;
		this.AllowsTransparency = true;
		this.ResizeMode = ResizeMode.NoResize;
		this.Width = width;
		this.Height = height;
		this.Left = x;
		this.Top = y;
		this.ShowActivated = true;

		hWnd = new WindowInteropHelper(this).EnsureHandle();
		uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
		User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)WINDOWSTYLE.WS_EX_TOOLWINDOW));
		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

		_left = (int)this.Left;
		_top = (int)this.Top;
		_right = _left + (int)this.Width;
		_bottom = _top + (int)this.Height;
		Logger.Log($"Menu, L: {_left}, T: {_top}, R: {_right}, B: {_bottom}");

		Task.Run(async () =>
		{
            Sambar.api.bar.Dispatcher.Invoke(() => this.Show());
            await Task.Delay(200);
            Api.FOCUS_CHANGED_EVENT += MenuFocusChangedHandler;
        });
	}

	public bool isClosing = false;
	public virtual async void MenuFocusChangedHandler(FocusChangedMessage msg)
	{
		if (isClosing) return;

		Logger.Log($"MenuFocusChanged, name: {msg.name}, class: {msg.className}, controlType: {msg.controlType}");
		if (msg.name == "Desktop") Sambar.api.bar.Dispatcher.Invoke(() => CustomClose());
		// if cursor inside menu
		User32.GetCursorPos(out POINT cursorPos);
		if (cursorPos.X > _left && cursorPos.X < _right)
			if (cursorPos.Y > _top && cursorPos.Y < _bottom)
				return;
		// Filter using ControlType
		if (
			msg.controlType == ControlType.MENU ||
			msg.controlType == ControlType.MENUITEM ||
			msg.controlType == ControlType.LIST ||
			msg.controlType == ControlType.LISTITEM ||
			msg.controlType == ControlType.BUTTON
		) return;
		//// if focus changed to itself ignore
		if (msg.name == "SambarContextMenu") return;
		if (msg.name == "Bar" && msg.className == "Window") return;
		if (Utils.IsContextMenu(msg.hWnd)) return;
		// wait for trayIconMenuChildren to get filled if icon children havent been retrieved
		// and also wait so that focus changed event is not consumed when menu it is opening
		await Task.Delay(Sambar.api.WINDOW_CAPTURE_DURATION);
		if (!Sambar.api.capturedWindows.Select(_msg => _msg.className).Contains(msg.className))
		{
			Logger.Log($"Closing menu by losing focus to non-menu item: {msg.name}, {msg.className}");
			Sambar.api.bar.Dispatcher.Invoke(() => CustomClose());
		}
	}

	public void CustomShow()
	{
		this.Show();
	}
	public void CustomClose()
	{
		isClosing = true;
        this.Close(); 
	}
}

public class ContextMenu : Menu
{
    public ContextMenu(int x, int y, int width, int height) : base(x, y, width, height)
    {
    }

	public override async void MenuFocusChangedHandler(FocusChangedMessage msg) 
	{
		if (isClosing) return;

        if(msg.name != "SambarContextMenu") Sambar.api.bar.Dispatcher.Invoke(() => CustomClose());
		Logger.Log($"overriden focushandler, closing due to: {msg.name}");
	}
}

public class MenuButton: RoundedButton
{
	public MenuButton(string text)
	{
		Text = text;
		HoverEffect = true;
		Margin = new(5);
		CornerRadius = new(5);
		HoverColor = Utils.BrushFromHex("#383838");
		Height = 20;
		FontFamily = new("JetBrains Mono");
	}
}
