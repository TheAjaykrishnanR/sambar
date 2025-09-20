﻿/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

///<summary>
/// Custom windows that can be used by widgets to make detached plugins
/// such as clocks, audio visualizers or whatever one so desires. Because it
/// doesnt fit inside Menus.cs
///</summary>

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using ScottPlot;
using ScottPlot.WPF;
using WinRT;
using ScottPlot.Plottables;
using FlaUI.Core.WindowsAPI;

namespace sambar;

public partial class Api
{
	private void CustomWindowsInit()
	{

	}

	public (ThreadWindow, WpfPlot, FilledSignal) CreateAudioVisualizer(
		int x = 100,
		int y = 100,
		int width = 400,
		int height = 200,
		bool centerOffset = false,
		Action<Window>? init = null
	)
	{
		if (centerOffset)
			(x, y) = GetCenteredCoords(x, y, width, height);

		ThreadWindow threadWnd = new(x, y, width, height, init);
		threadWnd.Run(() =>
		{
			audioVisPlot = new();
			//audioSignal = audioVisPlot!.Plot.Add.Signal(this.signalData);
			audioSignal = FilledSignal.AddFilledSignalToPlot(this.audioVisPlot, this.signalData);
			// WpfPlot does not automatically get its dimensions when inside a container like
			// stackpanel or border, therefore width and height must be set manually
			audioVisPlot.Height = 100;
			audioVisPlot.Width = 200;
		});
		return (threadWnd, audioVisPlot, audioSignal);
	}

	public Window CreateWidgetWindow(int x, int y, int width, int height, bool centerOffset = false)
	{
		if (centerOffset)
			(x, y) = GetCenteredCoords(x, y, width, height);

		WidgetWindow wnd = new()
		{
			Title = "sambarWidgetWindow",
			Background = new SolidColorBrush(System.Windows.Media.Colors.Black),
			Left = x,
			Top = y,
			Width = width,
			Height = height,
		};
		return wnd;
	}

	public (int, int) GetCenteredCoords(int offsetX, int offsetY, int width, int height)
	{
		offsetX += (Sambar.screenWidth - width) / 2;
		offsetY += (Sambar.screenHeight - height) / 2;
		return (offsetX, offsetY);
	}
}

/// <summary>
/// Non focusable window that always remains bottom most (also hidden in alt-tab).
/// </summary>
public class WidgetWindow : Window
{
	public nint hWnd;
	internal WidgetWindow()
	{
		this.ShowActivated = false;
		this.AllowsTransparency = true;
		this.WindowStyle = WindowStyle.None;
		this.ResizeMode = ResizeMode.NoResize;

		hWnd = new WindowInteropHelper(this).EnsureHandle();
		Utils.HideWindowInAltTab(hWnd);
		SetBottom();

		HwndSource hWndSrc = HwndSource.FromHwnd(hWnd);
		HwndSourceHook hook = new(WndProc);
		hWndSrc.AddHook(hook);
		this.Closing += (s, e) =>
		{
			hWndSrc.RemoveHook(hook);
		};
	}

	private nint WndProc(nint hWnd, int msg, nint wparam, nint lparam, ref bool handled)
	{
		if (msg == (int)WINDOWMESSAGE.WM_SETFOCUS ||
			msg == (int)WINDOWMESSAGE.WM_ACTIVATE ||
			msg == (int)WINDOWMESSAGE.WM_MOUSEACTIVATE ||
			msg == (int)WINDOWMESSAGE.WM_WINDOWPOSCHANGING ||
			msg == (int)WINDOWMESSAGE.WM_ACTIVATEAPP
		)
		{
			SetBottom();
			handled = true;
		}

		return 0;
	}

	private void SetBottom()
	{
		User32.SetWindowPos(hWnd, (nint)SWPZORDER.HWND_BOTTOM, 0, 0, 0, 0, SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOACTIVATE);
	}
}

/// <summary>
/// A wrapper for a window that runs on a separate thread.
/// </summary>
public class ThreadWindow
{
	public WidgetWindow? wnd;
	public nint hWnd;
	public FrameworkElement? content;
	bool initialized = false;
	internal ThreadWindow(
		int x = 100,
		int y = 100,
		int width = 800,
		int height = 400,
		Action<Window>? init = null
	)
	{
		Thread thread = new(() =>
		{
			wnd = new();
			wnd.Title = "sambarThreadedWindow";
			wnd.Left = x;
			wnd.Top = y;
			wnd.Width = width;
			wnd.Height = height;
			// just do this before show()
			init?.Invoke(wnd);
			hWnd = new WindowInteropHelper(wnd).EnsureHandle();
			initialized = true;

			// make thread a "UI Thread" by starting the message pump
			System.Windows.Threading.Dispatcher.Run();
		});
		thread.SetApartmentState(ApartmentState.STA);
		thread.IsBackground = true;
		thread.Start();
	}

	public ThreadWindow EnsureInitialized()
	{
		while (!initialized) Thread.Sleep(1);
		return this;
	}

	public void Run(Action runLambda)
	{
		while (!initialized) Thread.Sleep(1);
		bool finished = false;
		wnd?.Dispatcher.Invoke(() =>
		{
			runLambda();
			finished = true;
		});
		while (!finished) Thread.Sleep(1);
	}
}
