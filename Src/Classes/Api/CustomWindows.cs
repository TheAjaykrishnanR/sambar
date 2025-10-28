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
using SkiaSharp;
using SkiaSharp.Views.WPF;

namespace sambar;

public partial class Api
{
	private void CustomWindowsInit()
	{
	}

	public (WindowThread<WidgetWindow>, WpfPlot, FilledSignal) CreateAudioVisualizer(
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

		WindowThread<WidgetWindow> threadWnd = new(x, y, width, height, init);
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

	private Window _overlayWnd;
	public Window activeOverlayWnd
	{
		get { return _overlayWnd; }
		set
		{
			_overlayWnd?.Close();
			_overlayWnd = value;
		}
	}

	/// <summary>
	/// Creates a background overlay window that fills the desktop
	/// </summary>
	public Window CreateDesktopOverlay()
	{
		activeOverlayWnd = new WidgetWindow()
		{
			Title = "sambarDesktopOverlay",
			Background = new SolidColorBrush(System.Windows.Media.Colors.Black),
			Left = 0,
			Top = 0,
			Width = Sambar.screenWidth,
			Height = Sambar.screenHeight,
		};
		return activeOverlayWnd;
	}

	/// <summary>
	/// normal win32 messagebox
	/// </summary>
	public void MessageBox(string message)
	{
		Task.Run(() => User32.MessageBox(Sambar.api!.bar.hWnd, message, "SambarMessageWindow", 0));
	}
}

/// <summary>
/// Non focusable window that always remains bottom most (also hidden in alt-tab).
/// </summary>
public class WidgetWindow : Window
{
	public nint hWnd;
	public WidgetWindow()
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
public class WindowThread<T> where T : Window, new()
{
	public T? wnd;
	public Thread thread;
	public nint hWnd;
	public FrameworkElement? content;
	bool initialized = false;
	internal WindowThread(
		int x = 100,
		int y = 100,
		int width = 800,
		int height = 400,
		Action<Window>? init = null
	)
	{
		thread = new(() =>
		{
			wnd = new();
			wnd.Title = "sambarThreadedWindow";
			wnd.Left = x;
			wnd.Top = y;
			wnd.Width = width;
			wnd.Height = height;
			wnd.Closed += (s, e) => wnd.Dispatcher.InvokeShutdown();
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

	public WindowThread<T> EnsureInitialized()
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
