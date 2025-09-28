/*
 * Combines NetworkManager, Tray and Wallpaper widgets from Base
 * */

public class ActionCenter : Widget
{
	public ActionCenter(WidgetEnv ENV) : base(ENV)
	{
		StackPanel panel = new();
		panel.Orientation = Orientation.Horizontal;

		NetworkManager wifi = new(ENV);
		Tray tray = new(ENV);
		Wallpapers wall = new(ENV);

		wifi.Init();
		tray.Init();
		wall.Init();

		panel.Children.Add(wifi);
		panel.Children.Add(tray);
		panel.Children.Add(wall);

		this.CornerRadius = new(8);
		this.Background = Utils.BrushFromHex("#1a1a1a");
		this.Content = panel;
	}
}
