public class TaskbarApps : Widget
{
	StackPanel panel = new();
	RunningApp focusedApp;
	List<RoundedButton> btns = new();
	public Theme theme = new();

	/*
	 * Made public so that they can be edited in a mod file (TaskbarApps.mod.cs)
	 * */

	//public int BUTTON_HEIGHT = Theme.BUTTON_HEIGHT;
	//public int BUTTON_WIDTH = Theme.BUTTON_WIDTH;

	//public Brush BUTTON_BACKGROUND = Theme.BUTTON_BACKGROUND;
	//public Brush BUTTON_BORDER_COLOR = Theme.BUTTON_BORDER_COLOR;
	//public Thickness BUTTON_BORDER_THICKNESS = Theme.BUTTON_BORDER_THICKNESS;

	//public Brush BUTTON_PRESSED_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	//public Brush BUTTON_PRESSED_BORDER_COLOR = Utils.BrushFromHex("#22F803");
	//public Thickness BUTTON_PRESSED_BORDER_THICKNESS = new(0, 5, 0, 0);

	public TaskbarApps(WidgetEnv ENV) : base(ENV)
	{
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;

		Sambar.api.TASKBAR_APPS_EVENT += UpdateTaskbarApps;
		Sambar.api.ACTIVE_WINDOW_CHANGED_EVENT += UpdateFocusedApp;
		this.Content = panel;
	}

	List<RunningApp> apps = new();
	public void UpdateTaskbarApps(List<RunningApp> apps)
	{
		Sambar.api.Print($"UpdateTaskbarApps fired!");
		this.Thread.Invoke(() =>
		{
			panel.Children.Clear();
			btns = new();
			foreach (var app in apps)
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
				if (focusedApp?.hWnd == app.hWnd)
				{
					UpdateFocusedApp(app);
				}

				panel.Children.Add(btn);
				btns.Add(btn);
			}
		});
	}

	public void UpdateFocusedApp(RunningApp app)
	{
		focusedApp = app;
		this.Thread.Invoke(() =>
		{
			if (btns == null) return;
			foreach (var btn in btns)
			{
				if (btn.Id == app.hWnd.ToString())
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
