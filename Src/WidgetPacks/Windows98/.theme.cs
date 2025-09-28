// GLOBAL RESOURCES USED BY YOUR THEME
// ALL VARIABLES MUST BE STATIC

// Fluent Icons
// https://fluenticons.co/
public class Theme
{
	public Brush TEXT_COLOR = Utils.BrushFromHex("#000000");
	public FontFamily FONT_FAMILY = new("JetBrains Mono");

	public Brush BUTTON_FOREGROUND = new SolidColorBrush(Colors.White);
	public Brush BUTTON_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	public Brush BUTTON_PRESSED_BACKGROUND = Utils.BrushFromHex("#575555");

	public Brush BUTTON_BORDER_COLOR = new SolidColorBrush(Colors.Black);
	public Brush BUTTON_PRESSED_BORDER_COLOR = Utils.BrushFromHex("#22F803");
	public Brush BUTTON_HOVER_COLOR = Utils.BrushFromHex("#828181");
	public CornerRadius BUTTON_CORNER_RADIUS = new(0);
	public Thickness BUTTON_MARGIN = new(0);
	public int ICON_WIDTH = 14;
	public int ICON_HEIGHT = 14;
	public int BUTTON_WIDTH = 19;
	public int BUTTON_HEIGHT = 19;
	public Thickness BUTTON_BORDER_THICKNESS = new(0);
	public Thickness BUTTON_PRESSED_BORDER_THICKNESS = new(1);

	public Brush WIDGET_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	public CornerRadius WIDGET_CORNER_RADIUS = new(5);
	public int WIDGET_GAP = 5;
}
