/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace sambar;
public partial class Api
{
	/// <summary>
	/// Tracks active windows and exposes events
	/// that fire when focus is changed 
	/// </summary>

	public delegate void ActiveWindowChangedHandler(RunningApp app);
	/// <summary>
	/// subscribe to this event to recieve active window
	/// changed messages
	/// </summary>
	public event ActiveWindowChangedHandler ACTIVE_WINDOW_CHANGED_EVENT = (app) => { };

	public void WindowingInit()
	{
		FOCUS_CHANGED_EVENT += WindowFocusChangedHandler;
		MonitorTaskbarApps();
	}

	List<RunningApp> runningApps = new();
	public void RefreshRunningApps()
	{
		List<nint>? hWndsInTaskbar = Utils.GetAllTaskbarWindows();
		// since instantiating a RunningApp loads everything including the icon
		// and refreshing runs very fast, we only instantiate new apps

		// add new (if any)
		if (hWndsInTaskbar == null) return;
		foreach (nint hWnd in hWndsInTaskbar)
		{
			if (!runningApps.Select(app => app.hWnd).Contains(hWnd))
			{
				runningApps.Add(new(hWnd));
			}
		}
		// remove apps not in hWndsInTaskbar
		runningApps = runningApps.Where(app => hWndsInTaskbar.Contains(app.hWnd)).ToList();
	}

	public void WindowFocusChangedHandler(FocusChangedMessage msg)
	{
		// we dont use IUIAutomation's hWnd because it returns elements inner to native windows
		// which most often do not have a native win32 handle
		nint foreground_hWnd = User32.GetForegroundWindow();
		//Logger.Log($"FOCUS CHANGED, foreground_hWnd: {foreground_hWnd}, foreground_className: {Utils.GetClassNameFromHWND(foreground_hWnd)}");

		// refresh running apps every time to account for newer windows
		List<RunningApp>? apps = new();
		if ((apps = runningApps?.Where(app => app.hWnd == foreground_hWnd).ToList()).Count() > 0)
		{
			ACTIVE_WINDOW_CHANGED_EVENT(apps.First());
			//Logger.Log($"ACTIVE WINDOW CHANGED: {apps.First().title}");
		}
	}

	public delegate void TaskbarAppsEventHandler(List<RunningApp> apps);
	public event TaskbarAppsEventHandler TASKBAR_APPS_EVENT = (apps) => { };
	CancellationTokenSource _mta_cts = new();
	/// <summary>
	/// Live task for constantly monitoring current taskbar apps
	/// </summary>
	List<RunningApp> _old_runningApps = new();
	bool updateRequired = false;
	public void MonitorTaskbarApps()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				RefreshRunningApps();
				// wpf ui rendering is expensive, therefore only fire when new apps actually 
				// have appeared
				if (runningApps != null && runningApps.Count != _old_runningApps.Count || updateRequired)
				{
					//Logger.Log("MONITOR APPS TRUE", file: false);
					TASKBAR_APPS_EVENT(runningApps);
					_old_runningApps = runningApps.ToList();
				}
				//Logger.Log($"MONITORING TASKBAR APPS: {runningApps.Count}, {_old_runningApps.Count}", file: false);
				await Task.Delay(100);
			}
		}, _mta_cts.Token);
	}

	public async void FlushEvents()
	{
		updateRequired = true;
		await Task.Delay(200);
		updateRequired = false;
	}
}

public class RunningApp
{
	public nint hWnd;
	public string title;
	public string exePath;
	public string className;
	public uint processId;
	public BitmapSource icon;

	public RunningApp(nint hWnd)
	{
		this.hWnd = hWnd;
		StringBuilder str = new(256);
		User32.GetWindowText(hWnd, str, str.Capacity);
		title = str.ToString();
		exePath = Utils.GetExePathFromHWND(hWnd)!;
		className = Utils.GetClassNameFromHWND(hWnd);
		User32.GetWindowThreadProcessId(hWnd, out processId);

		Shell32.ExtractIconEx(exePath, 0, out nint largeIcon, out nint smallIcon, 1);
		if (largeIcon != 0)
		{
			icon = Imaging.CreateBitmapSourceFromHIcon(largeIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			icon.Freeze();
		}
	}

	public void FocusWindow()
	{
		Logger.Log($"App requested focus");
		WINDOWPLACEMENT wndPlcmnt = new();
		User32.GetWindowPlacement(hWnd, ref wndPlcmnt);
		if ((SHOWWINDOW)wndPlcmnt.showCmd == SHOWWINDOW.SW_SHOWMINIMIZED)
		{
			User32.ShowWindow(hWnd, SHOWWINDOW.SW_RESTORE);
		}
		User32.SetForegroundWindow(hWnd);
	}
	public void Kill()
	{
		//Process.GetProcessById((int)processId).Kill();
		User32.SendMessage(hWnd, (uint)WINDOWMESSAGE.WM_CLOSE, 0, 0);
	}
}

