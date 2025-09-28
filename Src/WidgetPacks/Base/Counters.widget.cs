public class Counters : Widget
{
	public Theme theme = new();
	public TextBlock textBlock = new();

	public Counters(WidgetEnv ENV) : base(ENV)
	{
		textBlock.Foreground = theme.TEXT_COLOR;
		textBlock.FontFamily = theme.FONT_FAMILY;
		textBlock.VerticalAlignment = VerticalAlignment.Center;
		textBlock.Margin = new(0);

		Sambar.api.CPU_PERFORMANCE_NOTIFIED += (_usage) => { cpuUsage = _usage[0]; };
		Sambar.api.NETWORK_SPEED_NOTIFIED += (_speeds) => { downSpeed = _speeds[0]; upSpeed = _speeds[1]; };
		Sambar.api.MEMORY_USAGE_NOTIFIED += (_usage) => { memUsage = _usage[1] - _usage[0]; };
		UpdateText();

		this.Content = textBlock;
	}

	long cpuUsage = 0;
	float downSpeed = 0f, upSpeed = 0f;
	float memUsage = 0;

	public void UpdateText()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				(float _down, string _down_flag) = NormalizeSpeed(downSpeed);
				(float _up, string _up_flag) = NormalizeSpeed(upSpeed);

				// Dont add special characters such as '[' or ']' as these have special
				// meaning related to wpf databinding
				string text = $"NET: {_down:00.0} ↓{_down_flag} {_up:00.0} ↑{_up_flag}, MEM: {memUsage:00.00} Gb, CPU: {cpuUsage:00}%";
				this.Thread.Invoke(() => { textBlock.Text = text; });
				await Task.Delay(1000);
			}
		});
	}

	/// <summary>
	/// Normalize speed in Kbps to 00.0 format with appropriate flags
	/// </summary>
	public (float, string) NormalizeSpeed(float speed)
	{
		string unit = "Kb/s";
		if (speed > 100) unit = "Mb/s";
		if (speed > 100000) unit = "Gb/s";
		while (speed.ToString().Split(".")[0].Count() > 2) { speed /= 1024; }
		return (speed, unit);
	}
}
