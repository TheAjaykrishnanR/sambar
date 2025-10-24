/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Windows;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Media;

namespace sambar;

public class _Main
{
	public static Application app;

	[STAThread]
	static void Main(string[] args)
	{
		// for logger
		Kernel32.AttachConsole(-1);
		Logger.Log($"Logging into {Paths.logFile}");

		// check for already running instances 
		if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
		{
			Logger.Log("An instance is already running, exiting ...");
			return;
		}

		AppDomain.CurrentDomain.UnhandledException += (s, e) =>
		{
			Exception? ex = e.ExceptionObject as Exception;
			string text = $"{ex?.Message}\n{ex?.StackTrace}";
			Logger.Log(text);
		};

		Paths.CreateIfAbsent();

		// evaluate the .init.cs to get the widget pack name
		string? widgetPackName = WidgetLoader.GetObjectFromScript<string>(Paths.initCsFile);
		if (widgetPackName == null) return;

		Logger.Log($"Compiling config");
		string configFile = Path.Join(Paths.widgetPacksFolder, widgetPackName, ".config.cs");
		if (!File.Exists(configFile))
		{
			Logger.Log("widget pack does not contain .config.cs file");
			return;
		}
		Utils.CompileFileToDll(configFile, ".config");
		Assembly configAssembly = Assembly.LoadFile(Paths.configDll);
		Type configType = configAssembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Config))).First();
		Config? config = (Config?)Activator.CreateInstance(configType);

		if (!config!.hardwareRendering)
			RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

		// start the wpf bar window
		app = new();
		Sambar sambar = new(widgetPackName, config);
		app.Run(sambar);
	}
}
