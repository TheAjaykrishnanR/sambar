﻿using System.Runtime.InteropServices;
using System.Drawing;

namespace sambar;

/// <summary>
/// DWORD := uint
/// HWND  := nint
/// PVOID := nint
/// </summary>
/// ------------------------



/// <summary>
/// For controlling the visibility and autohide behaviours of the taksbar
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct APPBARDATA
{
    public uint cbSize;
    public nint hWnd;
    public uint uCallbackMessage;
    public uint uEdge;
    public Rectangle rc;
    public uint lParam;
}

[StructLayout(LayoutKind.Sequential)]
public struct POINT 
{
	public int X;
	public int Y;
}

[StructLayout(LayoutKind.Sequential)]
public struct RECT
{
	public int Left;
	public int Top;
	public int Right;
	public int Bottom;
}

[StructLayout(LayoutKind.Sequential)]
public struct WINDOWPLACEMENT
{
	public uint length;
	public uint flags;
	public uint showCmd;
	public POINT ptMinPosition;
	public POINT ptMaxPosition;
	public RECT rcNormalPosition;
	public RECT rcDevice;
}

/// <summary>
/// struct that applications use to query its tray icon information using
/// Shell_NotifyIcon() and Shell_NotifyIconGetRect(), these functions would then send
/// another internal struct [_NOTIFYICONIDENTIFIERINTERNAL] containing additional items 
/// to Shell_TrayWnd
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct _NOTIFYICONIDENTIFIER {
	public uint cbSize;
	public nint hWnd;
	public uint UID;
	public Guid guidItem;
}

[StructLayout(LayoutKind.Sequential)]
public struct _NOTIFYICONIDENTIFIERINTERNAL
{
	//--------------------
	public int magicNumber;
	public int msg;
	//---------------------
	public int callbackSize;
	//---------------------
	public int padding;
	//---------------------
	public nint hWnd;
	public uint UID;
	public Guid guidItem;
}

[StructLayout(LayoutKind.Sequential)]
public struct WNDCLASSEX
{
	public uint cbSize;
	public uint style;
	public WNDPROC lpfnWndProc;
	public int cbClsExtra;
	public int cbWndExtra;
	public nint hInstance;
	public nint hIcon;
	public nint hCurosr;
	public nint hbrBackground;
	public string lpszMenuName;
	public string lpszClassName;
	public nint hIconSm;
}

[StructLayout(LayoutKind.Sequential)]
public struct COPYDATASTRUCT
{
	public ulong dwData;
	public ulong cbData;
	public nint lpData;
}

[StructLayout(LayoutKind.Explicit)]
public struct TIMEOUTVERSIONUNION
{
	[FieldOffset(0)]
	public uint uTimeout;
	[FieldOffset(0)]
	public uint uVersion;
}

/// <summary>
/// Very delicate struct, you might also notice that hWnd is an uint instead of the usual nint
/// (IntPtr)
/// </summary>

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct NOTIFYICONDATA
{
	public uint cbSize;
	/// <summary>
	/// Window handle of the message processing window for the tray icon. This is NOT
	/// the handle to the actual icon's window, the actual icon might not even have a window
	/// to begin with (which is the case with XAML elements)
	/// </summary>
	public uint hWnd;
	public uint uID;
	public uint uFlags;
	/// <summary>
	/// SendMessage(hWnd, uCallbackMessage, ..., ...) 
	/// Wait what ? ......^...
	/// isnt it supposed to be a window message defined in WINDOWMESSAGE such as WM_CONTEXTMENU 
	/// or WM_RIGHTBUTTONDOWN ? well the actual window the gets the WM_RIGHTBUTTONDOWN when 
	/// the icon is rightclicked is the window hoisting the icon TopLevelXamlOverflowWindow
	/// or even Shell_TrayWnd. It then requests the message processing window of the icon (window with handle hWnd)
	/// for a context menu.
	/// </summary>
	public uint uCallbackMessage;
	public uint hIcon;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
	public string szTip;
	public uint dwState;
	public uint dwStateMask;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
	public string szInfo;
	public TIMEOUTVERSIONUNION uTimeoutOrVersion;
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
	public string szInfoTitle;
	public uint dwInfoFlags;
	public Guid guidItem;
	public uint hBalloonIcon;
}

/// <summary>
/// Message Type recieved or send to Taskbar [Shell_TrayWnd]
/// during the WM_COPYDATA event
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct SHELLTRAYICONUPDATEDATA
{
	public int dwHz;
	public uint dwMessage;
	public NOTIFYICONDATA nid;
}

/// <summary>
/// Win32 basic window message type used by SendMessage(), GetMessage(), TranslateMessage()
/// DispatchMessage() etc
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct MSG
{
	public nint hwnd;
	public WINDOWMESSAGE message;
	public nint wParam;
	public nint lParam;
	public uint time;
	public POINT pt;
	public uint lPrivate;
}

[StructLayout(LayoutKind.Sequential)]
public struct UNICODE_STRING
{
	public ushort Length;
	public ushort MaximumLength;
	public nint Buffer;
}

/// <summary>
/// Used by NtQuerySystemInformation in ntdll to query process module paths without 
/// elevated priveleges. Part of the undocumented windows api
/// </summary>

[StructLayout(LayoutKind.Sequential)]
public struct SYSTEM_PROCESS_ID_INFORMATION
{
	public nint ProcessId;
	public UNICODE_STRING ImageName;
}



