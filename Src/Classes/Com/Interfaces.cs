using System.Runtime.InteropServices;

namespace sambar;

// shlobj_core.h
[ComImport]
[Guid("f490eb00-1240-11d1-9888-006097deacf9")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IActiveDesktop
{
	[PreserveSig]
	int ApplyChanges(AD_Apply dwFlags);
	[PreserveSig]
	int GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszWallpaper,
		  int cchWallpaper,
		  int dwReserved);
	[PreserveSig]
	int SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string pwszWallpaper, int dwReserved);
	[PreserveSig]
	int GetWallpaperOptions(ref WALLPAPEROPT pwpo, int dwReserved);
	[PreserveSig]
	int SetWallpaperOptions(ref WALLPAPEROPT pwpo, int dwReserved);
	[PreserveSig]
	int GetPattern([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszPattern, int cchPattern, int dwReserved);
	[PreserveSig]
	int SetPattern([MarshalAs(UnmanagedType.LPWStr)] string pwszPattern, int dwReserved);
	[PreserveSig]
	int GetDesktopItemOptions(ref COMPONENTSOPT pco, int dwReserved);
	[PreserveSig]
	int SetDesktopItemOptions(ref COMPONENTSOPT pco, int dwReserved);
}
