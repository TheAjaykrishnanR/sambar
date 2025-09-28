public class NetworkManager : Widget
{
	public RoundedButton btn = new();
	public string iconFile = "wifi.svg";
	public Theme theme = new();

	public NetworkManager(WidgetEnv ENV) : base(ENV) { }

	public override void Init()
	{
		if (File.Exists(Path.Join(ENV.ASSETS_FOLDER, iconFile)))
		{
			btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, iconFile);
		}
		else
		{
			btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, iconFile);
		}
		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.Margin = theme.BUTTON_MARGIN;
		btn.Height = theme.BUTTON_HEIGHT;
		btn.Width = theme.BUTTON_WIDTH;
		btn.IconWidth = 16;
		btn.IconHeight = 16;
		btn.FontFamily = theme.FONT_FAMILY;
		btn.HoverColor = theme.BUTTON_HOVER_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.HoverEffect = false;
		btn.MouseDown += ButtonMouseDown;

		this.Background = theme.WIDGET_BACKGROUND;
		this.Content = btn;
	}

	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		Task.Run(() => { Sambar.api.LaunchUri("ms-actioncenter:controlcenter/"); });
	}
}

