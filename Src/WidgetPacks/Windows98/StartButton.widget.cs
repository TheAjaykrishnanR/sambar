public class StartButton : Widget
{
	public Theme theme = new();
	public Button btn = new();

	public StartButton(WidgetEnv ENV) : base(ENV)
	{
		Image icon = new() { Source = Sambar.api.GetImageSource(Path.Join(ENV.ASSETS_FOLDER, "windows.ico")) };
		icon.Width = 15;

		btn.Height = 19;
		btn.Width = 19;
		btn.Foreground = theme.TEXT_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.FontFamily = theme.FONT_FAMILY;
		btn.Content = icon;
		btn.Click += Clicked;
		this.Content = btn;
	}

	public void Clicked(object sender, RoutedEventArgs e)
	{
		Sambar.api.StartMenu();
	}
}
