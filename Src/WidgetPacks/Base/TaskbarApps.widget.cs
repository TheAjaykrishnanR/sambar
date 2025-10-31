public class TaskbarApps : Widget
{
	StackPanel panel = new();
	List<RunningApp?> pinnedApps = new()
	{
		/*
		 *   Insert apps you want to pin to the status bar below:
		 *
		 *   new(@"C:\path\to\your\app1\app1Name.exe"),
		 *   new(@"C:\path\to\your\app2\app2Name.exe"),
		 *   new(@"C:\path\to\your\app3\app3Name.exe"),
		 *
		*/
	};
	List<RoundedButton> btns = new();
	public Theme theme = new();

	public TaskbarApps(WidgetEnv ENV) : base(ENV)
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Sambar.api.TASKBAR_APPS_EVENT += UpdateTaskbarApps;
		this.Content = panel;
	}

	public void UpdateTaskbarApps(List<RunningApp> apps, RunningApp focusedApp)
	{
		Sambar.api.Print($"UpdateTaskbarApps fired!");

		// the final list of taskbar apps that will be displayed
		List<RunningApp> allApps = new();
		allApps.AddRange(pinnedApps);
		allApps.AddRange(apps);

		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();

			btns.ForEach(btn => btn.Dispose());
			btns = new();

			foreach (var app in allApps)
			{
				RoundedButton btn = new();
				btn.Id = app.hWnd.ToString();
				btn.Icon = app.icon;
				btn.Height = theme.BUTTON_HEIGHT;
				btn.Width = theme.BUTTON_WIDTH;
				btn.IconHeight = theme.BUTTON_HEIGHT - 2;
				btn.IconWidth = theme.BUTTON_WIDTH - 2;
				btn.Margin = new(0, 0, 5, 0);
				btn.HoverEffect = false;
				List<MenuButton> menuItems = new()
				{
				   new("close")
				};
				menuItems.ForEach(item =>
				{
					item.MouseDown += (s, e) => app.Kill();
				});
				btn.MouseDown += (s, e) =>
				{
					switch (e.ChangedButton)
					{
						case MouseButton.Left:
							app.FocusWindow();
							break;
						case MouseButton.Right:
							Sambar.api.CreateContextMenu(menuItems);
							break;
					}
				};
				panel.Children.Add(btn);
				btns.Add(btn);
			}
			UpdateFocusedApp(focusedApp);
		});
	}

	public void UpdateFocusedApp(RunningApp focusedApp)
	{
		Sambar.api.Print($"UpdateFocusedApp(): {focusedApp.title}");
		this.Thread.Invoke(() =>
		{
			if (btns == null) return;
			foreach (var btn in btns)
			{
				if (btn.Id == focusedApp.hWnd.ToString())
				{
					btn.Background = theme.BUTTON_PRESSED_BACKGROUND;
					btn.BorderBrush = theme.BUTTON_PRESSED_BORDER_COLOR;
					btn.BorderThickness = theme.BUTTON_PRESSED_BORDER_THICKNESS;
				}
				else
				{
					btn.Background = theme.BUTTON_BACKGROUND;
					btn.BorderBrush = theme.BUTTON_BORDER_COLOR;
					btn.BorderThickness = theme.BUTTON_BORDER_THICKNESS;
				}
			}
		});
	}
}
