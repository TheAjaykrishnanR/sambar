using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;

namespace sambar;

public partial class Api
{

	// Constructor
	public void ToggleTaskbarInit()
	{
		taskbar_hWnd = User32.FindWindow("Shell_TrayWnd", null);
		WINDOWPLACEMENT lpwndpl = new();
		User32.GetWindowPlacement(taskbar_hWnd, ref lpwndpl);
		Logger.Log($"ToggleTaskBarInit: {lpwndpl.showCmd}");
	}

	nint taskbar_hWnd;
	public bool taskbarVisible { get; private set; } = true;
	public void ToggleTaskbar()
	{
		if (taskbar_hWnd == 0) return;
		if (taskbarVisible)
		{
			HideTaskbar();
		}
		else
		{
			ShowTaskbar();
		}
	}

	public void HideTaskbar()
	{
		SetTaskbarState(APPBARSTATE.AutoHide);
		User32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_HIDE);
		taskbarVisible = false;
	}

	public void ShowTaskbar()
	{
		SetTaskbarState(APPBARSTATE.AlwaysOnTop);
		User32.ShowWindow(taskbar_hWnd, SHOWWINDOW.SW_SHOW);
		taskbarVisible = true;
	}

	private void SetTaskbarState(APPBARSTATE state)
	{
		APPBARDATA msgData = new();
		msgData.cbSize = (uint)Marshal.SizeOf<APPBARDATA>();
		msgData.hWnd = taskbar_hWnd;
		msgData.lParam = (uint)state;
		Shell32.SHAppBarMessage((uint)APPBARMESSAGE.SetState, ref msgData);
	}

	private void ToggleTaskbarCleanup()
	{
		if (!taskbarVisible) ToggleTaskbar();
	}
}


