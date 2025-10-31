public class Clock : Widget
{
	public Theme theme = new();
	public TextBlock textBlock = new();

	public Clock(WidgetEnv ENV) : base(ENV)
	{
		textBlock.Foreground = theme.TEXT_COLOR;
		textBlock.FontFamily = theme.FONT_FAMILY;
		textBlock.VerticalAlignment = VerticalAlignment.Center;
		textBlock.Margin = new(5);
		Sambar.api.CLOCK_TICKED += ClockTicked;

		this.Height = Sambar.api.config.height;
		this.Content = textBlock;
	}

	void ClockTicked(Time time)
	{
		Thread.Invoke(() => textBlock.Text = timeString(time));
	}

	Func<Time, string> timeString = (time) =>
	{
		return $"{time.hours}:{time.minutes}:{time.seconds} {time.day}-{time.month}-{time.year}";
	};
}
