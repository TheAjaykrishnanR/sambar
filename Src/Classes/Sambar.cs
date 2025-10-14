/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.WebSockets;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace sambar;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class Sambar : Window
{
	public nint hWnd;
	public static Api? api;
	Config config;
	public string widgetPackName;
	bool firstShow = true;
	public Action<Action> UIThread;
	internal Sambar(string widgetPackName, Config config)
	{
		// Initialize the following in order
		// 1. window 
		// 2. api
		// 3. widgets

		this.Title = "Bar";
		this.WindowStyle = WindowStyle.None;
		this.AllowsTransparency = true;
		this.Topmost = true;
		this.widgetPackName = widgetPackName;
		this.config = config;
		this.UIThread = this.Dispatcher.Invoke;
		this.ShowActivated = false;

		// WPF event sequence
		// https://memories3615.wordpress.com/2017/03/24/wpf-window-events-sequence/
		SourceInitialized += (s, e) =>
		{
			hWnd = new WindowInteropHelper(this).Handle;
			WindowInit(); // needs hWnd
			this.Hook(WndProc);
		};

		Loaded += (s, e) =>
		{
			if (firstShow)
			{
				RegisterAsAppbar(); // do this before AddWidgets atleast because for some reason
									// calling this after does not guarantee ABM_SETPOS, maybe because of ToggleTaskbar ?
									// ToggleTaskbar has a call to SHAppbarMessage() to hide the real taskbar
									// maybe thats whats messing with it ?

				api = new(this);
				api.config = config; //setting a copy of the config to the API 
				AddWidgets();
				firstShow = false;
			}
		};
	}

	//public static double scale;
	bool barTransparent = false;
	public static int screenWidth;
	public static int screenHeight;
	public void WindowInit()
	{
		Shcore.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);

		screenWidth = User32.GetSystemMetrics(0);
		screenHeight = User32.GetSystemMetrics(1);

		// get the scalefactor of the primary monitor
		//scale = Utils.GetDisplayScaling();
		//Logger.Log($"Scale factor: {scale}");
		//screenWidth = (int)(screenWidth / scale);
		//screenHeight = (int)(screenHeight / scale);

		if (config.width == 0) { config.width = screenWidth - (config.marginXLeft + config.marginXRight); }

		this.Background = Utils.BrushFromHex(config.backgroundColor);
		if (this.Background.Equals(Colors.Transparent)) { barTransparent = true; }

		// Make bar a toolwindow (appear always on top)
		// TODO: loses topmost to other windows when task manager is open
		uint exStyles = User32.GetWindowLong(hWnd, GETWINDOWLONG.GWL_EXSTYLE);
		User32.SetWindowLong(hWnd, (int)GETWINDOWLONG.GWL_EXSTYLE, (int)(exStyles | (uint)sambar.WINDOWSTYLE.WS_EX_TOOLWINDOW));

		Utils.HideWindowInAltTab(hWnd);

		this.Width = config.width;
		this.Height = config.height;
		this.Left = config.marginXLeft;
		this.Top = config.marginYTop;

		this.BorderBrush = Utils.BrushFromHex(config.borderColor);
		this.BorderThickness = config.borderThickness;

		Logger.Log($"this.Width: {config.width}");

		int cornerPreference = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
		if (!barTransparent && config.roundedCorners)
			Dwmapi.DwmSetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

		// bar right click context menu
		this.SetContextMenu([
			("Exit", (s, e) => { Exit(); }),
		]);
	}

	public async void AddWidgets()
	{
		// the api has some blocking init tasks (looking at you glazewm) in the constructor that widgets might request, so only load the widgets once they are finished
		await Task.WhenAll(api!.initTasks);
		// way down the line in widget loader we call GetObjectFromString<T>() for 
		// all the .mod.cs files, the EvaluateAsync inside is run on a newly spawned 
		// thread for each script. After a couple of scripts (6 - 7) the next script's
		// thread for some reason goes into ThreadState.WaitSleepJoin. So i guess in short
		// _t.Join() doesnt work reliably when called from the UIThread (a deadlock ?).
		// But why would EvaluateAsync wait for UIThread ? it doesnt have anything to do with it.
		await Task.Run(() => { WidgetLoader widgetLoader = new(this); });
		// since the api starts much earlier than the widgets, fire events to update state once
		// widgets are loaded
		Sambar.api.FlushEvents();
	}

	// cleanup and exit
	public void Exit()
	{
		UnregisterAsAppbar();
		Sambar.api?.Cleanup();
		_Main.app.Shutdown();
	}

	/// <summary>
	/// Allows us to claim desktop real estate
	/// Does not work in SourceInitialized, needs at lest Loaded
	/// </summary>
	APPBARDATA abd = new();
	public void RegisterAsAppbar()
	{
		abd.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
		abd.hWnd = this.hWnd;
		abd.uCallbackMessage = User32.RegisterWindowMessage("SambarMessage");

		uint res = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.New, ref abd);

		AppbarSetPos();
	}

	public void AppbarSetPos()
	{

		const int ABE_LEFT = 0;
		const int ABE_TOP = 1;
		const int ABE_RIGHT = 2;
		const int ABE_BOTTOM = 3;

		abd.uEdge = ABE_TOP;

		switch (abd.uEdge)
		{
			case ABE_LEFT or ABE_RIGHT:
				abd.rc = new() { Top = 0, Bottom = screenHeight };
				break;
			case ABE_TOP or ABE_BOTTOM:
				abd.rc = new() { Left = 0, Right = screenWidth };
				break;
		}

		uint res2 = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.QueryPos, ref abd);
		Logger.Log($"APPBAR RECT, L: {abd.rc.Left}, T: {abd.rc.Top}, R: {abd.rc.Right}, B: {abd.rc.Bottom}");

		// adjust
		switch (abd.uEdge)
		{
			case ABE_LEFT:
				abd.rc.Right = abd.rc.Left + config.width;
				break;
			case ABE_TOP:
				abd.rc.Bottom = abd.rc.Top + config.height;
				break;
			case ABE_RIGHT:
				abd.rc.Left = abd.rc.Right - config.width;
				break;
			case ABE_BOTTOM:
				abd.rc.Top = abd.rc.Bottom - config.height;
				break;
		}

		uint res3 = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.SetPos, ref abd);
		Logger.Log($"REGISTERED AS APPBAR, abd.qpos: {res2}, abm.setpos: {res3}");
		Logger.Log($"win32: {Marshal.GetLastWin32Error()}");
	}

	public void UnregisterAsAppbar()
	{
		uint res = Shell32.SHAppBarMessage((uint)APPBARMESSAGE.Remove, ref abd);
		Logger.Log($"UNREGISTERED AS APPBAR: {res}");
	}

	private nint WndProc(nint hWnd, int msg, nint wparam, nint lparam, ref bool handled)
	{
		switch (msg)
		{
			case (int)WINDOWMESSAGE.WM_ACTIVATE:
				Shell32.SHAppBarMessage((int)APPBARMESSAGE.Activate, ref abd);
				break;
		}

		switch (wparam)
		{
			case (int)APPBARNOTIFY.ABN_POSCHANGED:
				AppbarSetPos();
				break;
			case (int)APPBARNOTIFY.ABN_FULLSCREENAPP:
				if (lparam > 0) // fullscreen app is opening
				{
					this.Topmost = false;
					User32.SetWindowPos(this.hWnd, (nint)SWPZORDER.HWND_BOTTOM, 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
				}
				else // revert back to topmost once fullscreen app closes
				{
					User32.SetWindowPos(this.hWnd, (nint)SWPZORDER.HWND_TOPMOST, 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
					this.Topmost = true;
				}
				break;
		}
		return 0;
	}
}
