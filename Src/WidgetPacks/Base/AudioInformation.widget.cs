public class AudioInformation : Widget
{
	public Theme theme = new();

	public AudioInformation(WidgetEnv ENV) : base(ENV)
	{
		TextBlock textBlock = new();
		textBlock.Foreground = new SolidColorBrush(System.Windows.Media.Colors.Black);
		textBlock.FontFamily = theme.FONT_FAMILY;
		textBlock.TextWrapping = TextWrapping.Wrap;
		textBlock.TextAlignment = TextAlignment.Right;
		//textBlock.FontWeight = FontWeights.Bold;

		Border border = new();
		//border.BorderBrush = new SolidColorBrush(Colors.Red);
		//border.BorderThickness = new(2);
		border.Child = textBlock;

		Window wnd = Sambar.api.CreateWidgetWindow(425, -210, 400, 50, centerOffset: true);
		wnd.Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
		wnd.Content = border;
		wnd.Show();

		Sambar.api.MEDIA_INFO_EVENT += (MediaInfo info) =>
		{
			wnd.Dispatcher.Invoke(() => textBlock.Text = info.Title);
		};
		Sambar.api.MEDIA_STOPPED_EVENT += () =>
		{
			wnd.Dispatcher.Invoke(() => textBlock.Text = "");
		};
	}
}
