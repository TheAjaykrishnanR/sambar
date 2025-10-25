using System.Runtime.InteropServices;

namespace sambar;

[StructLayout(LayoutKind.Sequential)]
public struct WALLPAPEROPT
{
	public static readonly int SizeOf = Marshal.SizeOf(typeof(WALLPAPEROPT));
	public int dwSize;
	public WallPaperStyle dwStyle;
}

[StructLayout(LayoutKind.Sequential)]
public struct COMPONENTSOPT
{
	public static readonly int SizeOf = Marshal.SizeOf(typeof(COMPONENTSOPT));
	public int dwSize;
	[MarshalAs(UnmanagedType.Bool)]
	public bool fEnableComponents;
	[MarshalAs(UnmanagedType.Bool)]
	public bool fActiveDesktop;
}
