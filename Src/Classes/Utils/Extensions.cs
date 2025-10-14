/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Windows.Media.Imaging;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;

namespace sambar;

public static class Extensions
{
	public static bool ContainsFlag(this uint flag, uint flagToCheck)
	{
		if ((flag & flagToCheck) != 0)
		{
			return true;
		}
		return false;
	}

	public static BitmapImage FromBitmap(this BitmapImage self, Bitmap bitmap)
	{
		using (MemoryStream ms = new())
		{
			bitmap.Save(ms, ImageFormat.Png);
			ms.Position = 0;

			self.BeginInit();
			self.StreamSource = ms;
			self.CacheOption = BitmapCacheOption.OnLoad;
			self.EndInit();
			self.Freeze();

			Logger.Log($"ICON_B: {bitmap.Width}x{bitmap.Height}");
			Logger.Log($"ICON_BP: {self.Width}x{self.Height}");
			return self;
		}
	}

	/// <summary>
	/// Adds a context menu to any WPF control
	/// </summary>
	public static void SetContextMenu(this System.Windows.Controls.Control self, List<(string, Action<object, object>)> nameActionPairs)
	{
		System.Windows.Controls.ContextMenu ctxMenu = new();
		foreach (var pair in nameActionPairs)
		{
			System.Windows.Controls.MenuItem menuItem = new()
			{
				Header = pair.Item1,
			};
			menuItem.Click += (s, e) =>
			{
				pair.Item2(s, e);
			};
			ctxMenu.Items.Add(menuItem);
		}
		self.ContextMenu = ctxMenu;
	}

	/// <summary>
	/// Hooks the WndProc
	/// </summary>
	public static void Hook(this Window wnd, WndProc proc)
	{
		HwndSource hWndSrc = HwndSource.FromHwnd(new WindowInteropHelper(wnd).EnsureHandle());
		HwndSourceHook hook = new(proc);
		hWndSrc.AddHook(hook);
		wnd.Closing += (s, e) =>
		{
			hWndSrc.RemoveHook(hook);
		};
	}
}
