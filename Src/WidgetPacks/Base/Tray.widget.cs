using System.IO;

public class Tray : Widget
{
	sambar.Menu? menu = null;

	public Theme theme = new();
	public RoundedButton btn = new();
	public string iconFile = "arrow_down.svg";

	public Tray(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		if (File.Exists(Path.Join(ENV.ASSETS_FOLDER, iconFile)))
			btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, iconFile);
		else
			btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, iconFile);
		btn.IconHeight = theme.ICON_HEIGHT;
		btn.IconWidth = theme.ICON_WIDTH;
		btn.Height = theme.BUTTON_HEIGHT;
		btn.Width = theme.BUTTON_WIDTH;
		btn.Margin = theme.BUTTON_MARGIN;
		btn.Foreground = theme.TEXT_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.HoverColor = theme.BUTTON_HOVER_COLOR;
		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.HoverEffect = false;
		btn.MouseDown += (s, e) =>
		{
			menu = Sambar.api.CreateMenu(btn, 100, 100);
			UpdateTrayPanel();
		};

		this.Content = btn;
		this.Background = theme.WIDGET_BACKGROUND;
		this.CornerRadius = theme.WIDGET_CORNER_RADIUS;

		Sambar.api.TASKBAR_CHANGED += () => UpdateTrayPanel();
	}

	public void UpdateTrayPanel()
	{
		if (menu == null) return;
		menu.Dispatcher.Invoke(() =>
		{
			List<TrayIcon> trayIcons = Sambar.api.GetTrayIcons();
			WrapPanel panel = new();
			Sambar.api.Print($"UpdateTrayPanel(): {trayIcons.Count()}");
			panel.Orientation = Orientation.Horizontal;
			foreach (var trayIcon in trayIcons)
			{
				RoundedButton iconBtn = new();
				iconBtn.Height = theme.BUTTON_HEIGHT;
				iconBtn.Width = theme.BUTTON_WIDTH;
				iconBtn.Margin = theme.BUTTON_MARGIN;
				iconBtn.Foreground = theme.TEXT_COLOR;
				iconBtn.Background = theme.BUTTON_BACKGROUND;
				iconBtn.HoverColor = theme.BUTTON_HOVER_COLOR;
				iconBtn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
				iconBtn.FontFamily = theme.FONT_FAMILY;
				iconBtn.Icon = trayIcon.icon;
				iconBtn.HoverEffect = false;
				iconBtn.Margin = new(10, 10, 0, 0);
				iconBtn.MouseDown += (s, e) =>
				{
					switch (e.ChangedButton)
					{
						case MouseButton.Right:
							trayIcon.ContextMenu();
							break;
						case MouseButton.Left:
							trayIcon.Click();
							break;
					}
				};
				panel.Children.Add(iconBtn);
			}
			menu.Content = panel;
		});
	}
}
