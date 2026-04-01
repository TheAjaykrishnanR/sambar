public class Pinger : Widget
{
	System.Net.NetworkInformation.Ping pinger = new();
	System.Windows.Shapes.Ellipse circle = new();

	public Pinger(WidgetEnv ENV) : base(ENV)
	{
		circle.Width = 5;
		circle.Height = 5;
		circle.Fill = System.Windows.Media.Brushes.Red;
		this.Content = circle;

		Task.Run(pingTask);
	}

	public async Task pingTask()
	{
		while (true)
		{
			bool success = pinger.Send(System.Net.IPAddress.Parse("1.1.1.1")).Status == IPStatus.Success;
			this.Thread.Invoke(() =>
			{
				if (success) circle.Fill = System.Windows.Media.Brushes.Green;
				else circle.Fill = System.Windows.Media.Brushes.Red;
			});
			await Task.Delay(5000);
		}
	}
}
