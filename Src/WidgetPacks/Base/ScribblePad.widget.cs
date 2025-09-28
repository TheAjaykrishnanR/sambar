public class ScribblePad : Widget
{
	public Theme theme = new();
	public RoundedButton btn = new();
	InkCanvas inkCanvas = new();

	public ScribblePad(WidgetEnv ENV) : base(ENV)
	{
		DrawingAttributes drawingAttributes = new();
		drawingAttributes.Color = Colors.White;
		inkCanvas.DefaultDrawingAttributes = drawingAttributes;

		btn.Height = theme.BUTTON_HEIGHT;
		btn.Width = theme.BUTTON_WIDTH;
		btn.Text = "S";
		btn.Foreground = theme.TEXT_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.HoverColor = theme.BUTTON_HOVER_COLOR;
		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.FontFamily = theme.FONT_FAMILY;
		btn.HoverEffect = true;
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
	}

	public void ButtonMouseDown(object sender, MouseEventArgs e)
	{
		sambar.Menu menu = Sambar.api.CreateMenu(btn, 300, 300);
		menu.Content = inkCanvas;
	}
}
