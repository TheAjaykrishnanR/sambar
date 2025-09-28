public class StartButton : Widget
{
	public Theme theme = new();
	public RoundedButton btn = new();

	public StartButton(WidgetEnv ENV) : base(ENV)
	{
		btn.Height = 16;
		btn.Width = 16;
		btn.Foreground = theme.TEXT_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.HoverColor = theme.BUTTON_HOVER_COLOR;
		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.FontFamily = theme.FONT_FAMILY;
		btn.HoverEffect = false;
		if (!ENV.IS_IMPORTED) btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, "start.svg");
		else btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, "start.svg");
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
	}

	public void ButtonMouseDown(object sender, MouseEventArgs e)
	{
		Sambar.api.StartMenu();
	}
}
