namespace sambar;

public delegate bool EnumWindowProc(nint hWnd, nint lParam);
public delegate nint WNDPROC(nint hWnd, WINDOWMESSAGE uMsg, nint wParam, nint lParam);
public delegate nint WndProc(nint hWnd, int uMsg, nint wParam, nint lParam, ref bool handled);
public delegate void TIMERPROC(nint hWnd, uint param2, nint param3, ulong param4);


