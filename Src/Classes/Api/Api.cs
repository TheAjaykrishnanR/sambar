/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;
using System.Windows;
using Interop.UIAutomationClient;

namespace sambar;

/// <summary>
/// The entire Api class provides core functionalities that a widget developer
/// can use in their plugins so that they dont have to reinvent the wheel.
/// </summary>
public partial class Api
{

	public List<Task> initTasks = new();
	internal Api(Sambar bar)
	{
		this.bar = bar;

		EventsInit();
		ToggleTaskbarInit();
		WindowingInit();
		SystemInit();
		//SystemTrayInit();
		TaskbarInterceptorInit();
		CustomWindowsInit();
		ClockInit();
		CountersInit();
		AviyalInit();
		initTasks.AddRange([
			Task.Run(GlazeInit),
			Task.Run(AudioInit)
		]);
	}

	public void Print(string text)
	{
		Logger.Log(text);
	}

	// so that widgets and scripts can use it
	public Config config;

	// instance of mainWindow
	public Sambar bar;

	// IUIAutomation
	CUIAutomation ui = new();

	internal void Cleanup()
	{
		ToggleTaskbarCleanup();
		GlazeCleanup();
	}
}

