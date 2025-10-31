public class AviyalWorkspaces : Widget
{
	public List<RoundedButton> buttons = new();
	public Theme theme = new();
	int workspaceCount = 0;
	int focusedWorkspaceIndex = 0;
	int aviyalPort = 6969;

	public AviyalWorkspaces(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		Sambar.api.StartAviyalClient(aviyalPort);
		Sambar.api.AVIYAL_MESSAGE_RECEIVED += AviyalMessageReceived;
		Sambar.api.AVIYAL_CONNECTED += AviyalConnected;
		AviyalConnected();
	}

	void AviyalConnected()
	{
		Sambar.api.AviyalSend("get state");
	}

	void AviyalMessageReceived(string message)
	{
		Sambar.api.Print($"aviyal_message_recieved: {message}");
		JsonNode node = JsonNode.Parse(message);
		focusedWorkspaceIndex = Convert.ToInt32(node["focusedWorkspaceIndex"].ToString());
		int _workspaceCount = Convert.ToInt32(node["workspaceCount"].ToString());
		if (_workspaceCount > workspaceCount)
		{
			workspaceCount = _workspaceCount;
			this.Thread.Invoke(() =>
			{
				this.Content = BuildUI();
			});
		}
		RedrawButtons(focusedWorkspaceIndex);
		Sambar.api.Print($"focused: {focusedWorkspaceIndex}");
	}

	Panel BuildUI()
	{
		StackPanelWithGaps panel = new(theme.WIDGET_GAP, workspaceCount);
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;
		panel.ClipToBounds = true;
		panel.Height = Sambar.api.config.height;

		for (int i = 1; i <= workspaceCount; i++)
		{
			RoundedButton btn = new();
			btn.Text = $"{i}";
			btn.FontFamily = theme.FONT_FAMILY;
			btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
			btn.Width = theme.BUTTON_WIDTH;
			btn.Height = theme.BUTTON_HEIGHT;
			btn.BorderThickness = theme.BUTTON_BORDER_THICKNESS;
			btn.BorderBrush = theme.BUTTON_BORDER_COLOR;
			btn.Foreground = theme.TEXT_COLOR;
			btn.HoverColor = theme.BUTTON_HOVER_COLOR;
			btn.Background = theme.BUTTON_BACKGROUND;
			btn.HoverEffect = true;
			btn.MouseDown += WorkspaceButtonClicked;
			buttons.Add(btn);
			panel.Add(btn);
		}

		if (buttons.Count > 0)
			buttons[focusedWorkspaceIndex].Background = theme.BUTTON_PRESSED_BACKGROUND;

		return panel;
	}

	public void RedrawButtons(int index)
	{
		this.Thread.Invoke(() =>
		{
			foreach (var button in buttons)
			{
				button.Background = theme.BUTTON_BACKGROUND;
			}
			buttons[index].Background = theme.BUTTON_PRESSED_BACKGROUND;
			buttons[index].HoverEffect = false;
		});
	}

	bool buttonRedrawing = false;
	public void WorkspaceButtonClicked(object? sender, RoutedEventArgs e)
	{
		buttonRedrawing = true;
		var btn = sender as RoundedButton;
		string clickedBtnName = Convert.ToString(btn.Text);
		int clickedBtnIndex = Convert.ToInt32(clickedBtnName) - 1;
		RedrawButtons(clickedBtnIndex);
		Task.Run(async () =>
		{
			Sambar.api.AviyalSend($"set focusedWorkspaceIndex {clickedBtnIndex}");
			await Task.Delay(500);
			buttonRedrawing = false;
		});
	}
}

public class AviyalMessage
{
	public int workspaceCount { get; set; }
	public int focusedWorkspaceIndex { get; set; }
}
