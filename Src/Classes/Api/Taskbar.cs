using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;

namespace sambar;

public partial class Api
{
	TaskbarInterceptor interceptor;

	public void TaskbarInterceptorInit()
	{
		interceptor = new();
	}
	Dictionary<ICONACTION, List<WINDOWMESSAGE>> ICONACTION_MAP_V0_3 = new()
	{
		{ ICONACTION.RIGHT_CLICK, [ WINDOWMESSAGE.WM_RBUTTONDOWN, WINDOWMESSAGE.WM_RBUTTONUP ] },
		{ ICONACTION.LEFT_CLICK, [ WINDOWMESSAGE.WM_LBUTTONDOWN, WINDOWMESSAGE.WM_LBUTTONUP ] },
	};
	Dictionary<ICONACTION, List<WINDOWMESSAGE>> ICONACTION_MAP_V3 = new()
	{
		{ ICONACTION.RIGHT_CLICK, [ WINDOWMESSAGE.WM_CONTEXTMENU ] },
		{ ICONACTION.LEFT_CLICK, [ WINDOWMESSAGE.WM_LBUTTONDOWN, WINDOWMESSAGE.WM_LBUTTONUP ] },
	};

	/// <summary>
	/// tray icons are registered on the taskbar by their respective windows inorder for them
	/// to be shown in the taskbar, upon being registered each icon has a callback message which the 
	/// taskbar uses to communicate events back to the icon's hidden backing window. These
	/// events include things like requesting the context menu when right clicked, or opening the 
	/// main ui of the tray app when clicked so on and so forth all of which has to be supplied by
	/// the icon's backing (message processing) window. Therefore interacting with the icon is 
	/// equivalent to sending/impersonating this callback communication from the taskbar to the 
	/// backing window.
	/// </summary>
	/// <param name="nid"></param>
	/// <param name="msg"></param>
	public void ImpersonateTrayEvent(NOTIFYICONDATA nid, ICONACTION msg)
	{
		User32.GetWindowThreadProcessId((nint)nid.hWnd, out uint processId);
		int result = User32.AllowSetForegroundWindow(processId);
		//Logger.Log($"ImpersonateTrayEvent(): {processId}, result: {result}, win32: {Marshal.GetLastWin32Error()}");
		if (nid.uTimeoutOrVersion.uVersion <= 3)
		{
			foreach (var winmsg in ICONACTION_MAP_V0_3[msg])
			{
				// For some reason this SendMessage call can hang for certain applications
				// such as Taskmgr (hWnd), therefore run it in a separate thread to prevent
				// the mainwindow from crashing 
				Action _send = () => User32.SendMessage(
					(nint)nid.hWnd,
					nid.uCallbackMessage,
					(nint)nid.uID,
					Utils.MAKELPARAM((short)winmsg, 0)
				);
				Task.Run(_send);
			}
		}
		else
		{
			foreach (var winmsg in ICONACTION_MAP_V3[msg])
			{
				User32.GetCursorPos(out POINT cursorPos);
				Action _send = () => User32.SendMessage(
					(nint)nid.hWnd,
					nid.uCallbackMessage,
					Utils.MAKEWPARAM((short)cursorPos.X, (short)cursorPos.Y),
					Utils.MAKELPARAM((short)winmsg, (short)nid.uID)
				);
				Task.Run(_send);
			}
		}
	}
	public List<TrayIcon> GetTrayIcons()
	{
		var icons = interceptor.trayIconsManager.GetTrayIcons();
		//Logger.Log($"GetTrayIcons(): {icons.Count()}");
		return icons;
	}

	public delegate void TaskbarChangedEventHandler();
	public event TaskbarChangedEventHandler TASKBAR_CHANGED = () => { };
	public static void TaskbarChanged()
	{
		Sambar.api?.TASKBAR_CHANGED();
	}
}
/// <summary>
/// An invisible window to intercept(or listen to) taskbar messages.
/// We create a window with the preexisting Taskbar class
/// "Shell_TrayWnd". Any messages intended for the real taskbar
/// will be captured by our invisible window
/// </summary>
public class TaskbarInterceptor
{
	nint hWnd;
	public nint originalTray_hWnd;
	CancellationTokenSource cts;
	public TaskbarInterceptor()
	{
		// capture the original taskbar hWnd before creating the interceptor window
		// with the same class name so as to forward messages to it. why is this necessary ?
		// well its not just for wanting to use the original taskbar alongside sambar but also
		// in instances where for example certain context menus are natively created by the taskbar
		// and not by the actual tray icon application
		originalTray_hWnd = User32.FindWindow("Shell_TrayWnd", null);
		cts = new();
		Task.Run(() =>
		{
			WNDCLASSEX wc = new();
			wc.cbSize = (uint)Marshal.SizeOf<WNDCLASSEX>();
			wc.lpfnWndProc = WndProc;
			// run GetModuleHandle() in the same thread as CreateWindowEx()
			wc.hInstance = Kernel32.GetModuleHandle(null);
			wc.lpszClassName = "Shell_TrayWnd";

			ushort result = User32.RegisterClassEx(ref wc);
			if (result == 0)
			{
				Logger.Log($"RegisterClassEx() failed: {Marshal.GetLastWin32Error()}");
			}
			else
			{
				Logger.Log($"RegisterClassEx() success !");
			}

			hWnd = User32.CreateWindowEx(
				WINDOWSTYLE.WS_EX_TOPMOST | WINDOWSTYLE.WS_EX_TOOLWINDOW,
				wc.lpszClassName,
				null,
				WINDOWSTYLE.WS_POPUP | WINDOWSTYLE.WS_CLIPCHILDREN | WINDOWSTYLE.WS_CLIPSIBLINGS,
				0,
				0,
				0,
				0,
				nint.Zero,
				nint.Zero,
				nint.Zero,
				nint.Zero
			);

			// Set window as topmost first and set a timer that keeps on doing just that
			if (User32.SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE) == 0)
			{
				Logger.Log($"SetWindowPos() failed: {Marshal.GetLastWin32Error()}");
			}

			User32.SetTimer(hWnd, 1, 100, null);

			RefreshTaskbar();

			while (User32.GetMessage(out MSG msg, hWnd, 0, 0) > 0)
			{
				User32.TranslateMessage(ref msg);
				User32.DispatchMessage(ref msg);
			}
		}, cts.Token);
	}

	public TrayIconsManager trayIconsManager = new();
	//public List<TrayIcon> overflowIcons = new();


	/// <summary>
	/// WndProc for our taskbar interceptor
	/// </summary>
	/// <returns>
	/// Data specific to the Window Message it recieves.
	/// The return code is sent to whoever sents the window a window message.
	/// </returns>
	nint WndProc(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam)
	{
		//Logger.Log($"Message: {uMsg}");
		switch (uMsg)
		{
			case WINDOWMESSAGE.WM_CLOSE:
				User32.DestroyWindow(hWnd);
				break;
			case WINDOWMESSAGE.WM_DESTROY:
				User32.PostQuitMessage(0);
				break;
			case WINDOWMESSAGE.WM_TIMER:
				User32.SetWindowPos(hWnd, (nint)(-1), 0, 0, 0, 0, SETWINDOWPOS.SWP_NOMOVE | SETWINDOWPOS.SWP_NOSIZE | SETWINDOWPOS.SWP_NOACTIVATE);
				break;
			case WINDOWMESSAGE.WM_COPYDATA:
				COPYDATASTRUCT copydata = Marshal.PtrToStructure<COPYDATASTRUCT>(lParam);
				if (copydata.cbData == 0) return 0;
				switch ((SHELLTRAYMESSAGE)copydata.dwData)
				{
					case SHELLTRAYMESSAGE.ICONUPDATE:
						SHELLTRAYICONUPDATEDATA iconData = Marshal.PtrToStructure<SHELLTRAYICONUPDATEDATA>(copydata.lpData);
						NOTIFYICONDATA nid = iconData.nid;
						switch ((ICONUPDATEACTION)iconData.dwMessage)
						{
							case ICONUPDATEACTION.NIM_ADD:
								trayIconsManager.Add(nid);
								break;
							case ICONUPDATEACTION.NIM_MODIFY:
							case ICONUPDATEACTION.NIM_SETVERSION:
								trayIconsManager.Update(nid);
								break;
							case ICONUPDATEACTION.NIM_DELETE:
								trayIconsManager.Delete(nid);
								break;
						}
						// for api.TASKBAR_CHANGED event
						// Api.TaskbarChanged();
						//Logger.Log($"ICONUPDATEACTION: {(ICONUPDATEACTION)(iconData.dwMessage)}, uid: {nid.uID}, hWnd: {nid.hWnd}, nids: {trayIconsManager.icons.Count}, class: {Utils.GetClassNameFromHWND((nint)nid.hWnd)}, version: {nid.uTimeoutOrVersion.uVersion}, callback: {nid.uCallbackMessage}, hIcon: {nid.hIcon}");
						break;

					// a tray icon's process is querying icon position using Shell_NotifyIconGetRect()
					// windows can only communicate by returning data through their WndProcs, so
					// assemble the data here and return it below. Certain tray apps for example glazewm
					// queries Shell_NotifyIconGetRect() right before displaying its trayicon's context
					// menu probably to get the icon coordinates for some reason, so handling it is imoportant
					case SHELLTRAYMESSAGE.TRAYICONPOSITION:
						_NOTIFYICONIDENTIFIERINTERNAL iconIdentifier = Marshal.PtrToStructure<_NOTIFYICONIDENTIFIERINTERNAL>(copydata.lpData);
						User32.GetCursorPos(out POINT cursorPos);
						switch (iconIdentifier.msg)
						{
							// request for left, top positions of the icon rect, case 1
							case 1:
								return Utils.MAKELPARAM((short)cursorPos.X, (short)cursorPos.Y);
							// request for the right, bottom of the icon rect
							case 2:
								return Utils.MAKELPARAM((short)(cursorPos.X + 1), (short)(cursorPos.Y - 1));
						}
						return 0;
				}
				break;
			default:
				User32.SendMessage(originalTray_hWnd, (uint)uMsg, wParam, lParam);
				return User32.DefWindowProc(hWnd, uMsg, wParam, lParam);
		}
		return 0;
	}

	/// <summary>
	/// Send taskbar created notification so that apps that are already on the taskbar
	/// can tell us who they are and their icon infos. This is essential to enumerate
	/// taskbar icons.
	/// </summary>
	public void RefreshTaskbar()
	{
		uint taskbarCreatedMsg = User32.RegisterWindowMessage("TaskbarCreated");
		const nint HWND_BROADCAST = 0xffff;
		User32.SendNotifyMessage(HWND_BROADCAST, taskbarCreatedMsg, 0, 0);
	}

	public void Destroy()
	{
		User32.DestroyWindow(hWnd);
		cts.Cancel();
	}
	~TaskbarInterceptor()
	{
		Destroy();
	}
}

// TODO: use IUIAutomation for Windows Defender Context Menu
public class TrayIcon
{
	public NOTIFYICONDATA nid;
	public string? className;
	public string? exePath;
	public uint old_uVersion;
	public uint processId;
	public BitmapSource icon;

	public TrayIcon(NOTIFYICONDATA nid)
	{
		this.nid = nid;
		this.className = Utils.GetClassNameFromHWND((nint)nid.hWnd);
		this.exePath = Utils.GetExePathFromHWND((nint)nid.hWnd);

		// could fail for all kinds of reasons since nid is essentially something
		// given to us and we have no control over it, for eq nid.hIcon could be null
		try
		{
			User32.GetWindowThreadProcessId((nint)nid.hWnd, out processId);
			// get actual icon from hIcon
			this.icon = Imaging.CreateBitmapSourceFromHIcon((nint)nid.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			this.icon.Freeze();
		}
		catch (Exception ex) { Logger.Log(ex.Message); }
	}

	public NOTIFYICONDATA ValidateNid(NOTIFYICONDATA nid)
	{
		NOTIFYICONDATA validatedNid = nid;

		// validate uVersion. If uVersion is exorbitantly large that means that it probably 
		// isnt uVersion but uTimeout
		if (validatedNid.uTimeoutOrVersion.uVersion > 4)
		{
			old_uVersion = nid.uTimeoutOrVersion.uVersion;
			validatedNid.uTimeoutOrVersion.uVersion = 0;
		}

		// check if uCallbackMessage is still invalid, for most user applications it wont be, but
		// for certain system tray apps it could, if so use uid as callback message
		// eg. SystemTray_Main
		if (
			validatedNid.uCallbackMessage == 0 &&
			!validatedNid.uFlags.ContainsFlag((uint)NOTIFYICONDATAVALIDITY.NIF_MESSAGE)
		)
		{
			validatedNid.uCallbackMessage = nid.uID;
		}

		// handling special classes
		// [ATL:00007FFE197FD000] SecurityHealthSystray.exe
		if (this.className == "ATL:00007FFE197FD000")
		{
			validatedNid.uTimeoutOrVersion.uVersion = 4;
		}

		return validatedNid;
	}

	public void ContextMenu()
	{
		NOTIFYICONDATA validatedNid = ValidateNid(nid);
		//RightClick, class: tray_icon_app, exe: C:\Program Files\glzr.io\GlazeWM\glazewm.exe, hWnd: 3802056, uVersion: 0, callback: 6002 
		//Logger.Log($"RightClick, class: {className}, exe: {exePath}, hWnd: {validatedNid.hWnd}, uid: {validatedNid.uID}, uVersion: {validatedNid.uTimeoutOrVersion.uVersion}, callback: {validatedNid.uCallbackMessage}, callbackValid: {(validatedNid.uFlags & 0x00000001) != 0},  old_uid: {old_uVersion}");
		Sambar.api.ImpersonateTrayEvent(validatedNid, ICONACTION.RIGHT_CLICK);
	}

	public void Click()
	{
		NOTIFYICONDATA validatedNid = ValidateNid(nid);
		//Logger.Log($"LeftClick, class: {className}, exe: {exePath}, hWnd: {validatedNid.hWnd}, uid: {validatedNid.uID}, uVersion: {validatedNid.uTimeoutOrVersion.uVersion}, callback: {validatedNid.uCallbackMessage}, callbackValid: {(validatedNid.uFlags & 0x00000001) != 0},  old_uid: {old_uVersion}");
		Sambar.api.ImpersonateTrayEvent(validatedNid, ICONACTION.LEFT_CLICK);
	}
}

/// <summary>
/// Adds nids to notifiedIcons so that it doesnt contain repeated elements and newer nids 
/// of already present tray icons when recieved are only added while removing the older one. 
/// However additional care has to be taken while adding newer nids because certain apps such
/// as Taskmgr for example wont have a valid uCallbackMessage when sending subsequent nids through
/// NIM_MODIFY, so we dont want to add the nid directly as then we would lose our uCallbackMessage
/// </summary>
public class TrayIconsManager
{
	public List<TrayIcon> icons = new();
	public TrayIconsManager()
	{
		MonitorTrayApps();
	}

	public void Add(NOTIFYICONDATA nid)
	{
		var indexedIcons = icons.Index().ToList();
		var repIndexedIcons = indexedIcons.Where(indexedIcon => indexedIcon.Item.nid.hWnd == nid.hWnd);
		if (repIndexedIcons.Count() == 0)
		{
			icons.Add(new(nid));
			Api.TaskbarChanged();
			return;
		}
		//Logger.Log($"icon already exists, use Update() to modify");
	}
	public void Update(NOTIFYICONDATA nid)
	{
		var indexedIcons = icons.Index().ToList();
		var repIndexedIcons = indexedIcons.Where(indexedIcon => indexedIcon.Item.nid.hWnd == nid.hWnd);
		if (repIndexedIcons.Count() > 0)
		{
			// if new nids contain invalid uCallbackMessage replace them with the old ones
			if (nid.uCallbackMessage == 0)
			{
				nid.uCallbackMessage = repIndexedIcons.First().Item.nid.uCallbackMessage;
			}
			// if new nids contain invalid hIcon replace them with old ones
			if (!nid.uFlags.ContainsFlag((int)NOTIFYICONDATAVALIDITY.NIF_ICON))
			{
				nid.hIcon = repIndexedIcons.First().Item.nid.hIcon;
			}

			icons[repIndexedIcons.First().Index] = new(nid);
			Api.TaskbarChanged();
			return;
		}
		//Logger.Log($"icon does not exist, use Add() instead");
	}
	public void Delete(NOTIFYICONDATA nid)
	{
		var indexedIcons = icons.Index().ToList();
		var foundIcons = indexedIcons.Where(icon => icon.Item.nid.hWnd == nid.hWnd).ToList();
		if (foundIcons.Count() == 0) return;
		var foundIcon = foundIcons.First();
		icons.RemoveAt(foundIcon.Index);
		Api.TaskbarChanged();
	}
	List<string> NON_OVERFLOW_CLASSES =
	[
		"ATL:00007FFE3066B050", // SPEAKER
        "BluetoothNotificationAreaIconWindowClass",
		"ASYNCUI_NOTIFYICON_WINDOW_CLASS"
	];
	public List<TrayIcon> GetTrayIcons()
	{
		return icons.Where(icon => !NON_OVERFLOW_CLASSES.Contains(icon.className)).ToList();
	}

	CancellationTokenSource cts = new();
	/// <summary>
	/// Because some tray apps simply dont emit NIM_DELETE
	/// when terminated
	/// </summary>
	public void MonitorTrayApps()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				var icons = GetTrayIcons();
				foreach (var icon in icons)
				{
					// check if trayicon app is running (has pid != 0 and matches the icon's pid)
					User32.GetWindowThreadProcessId((nint)icon.nid.hWnd, out uint _pid);
					if (icon.processId != _pid) Delete(icon.nid);
				}
				await Task.Delay(500);
			}
		}, cts.Token);
	}

	~TrayIconsManager()
	{
		cts.Cancel();
	}
}

public enum ICONACTION
{
	RIGHT_CLICK,
	LEFT_CLICK,
}

