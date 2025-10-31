public class Workspaces : Widget
{
	public List<RoundedButton> btns = new();
	public Theme theme = new();
	int workspaceCount = 0;
	int focusedWorkspaceIndex = 0;

	public Workspaces(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		Sambar.api.StartGlazeClient("ws://localhost:6123");

		Sambar.api.GLAZE_CONNECTED += GlazeConnected;
		Sambar.api.GLAZE_WORKSPACE_CHANGED += GlazeWorkspaceChanged;
	}

	void GlazeConnected(int count)
	{
		btns?.ForEach(btn => btn.Dispose());
		btns = new();

		workspaceCount = count;
		focusedWorkspaceIndex = Sambar.api.currentWorkspace.index;
		this.Thread.Invoke(() =>
		{
			this.Content = BuildUI();
		});
	}

	void GlazeWorkspaceChanged(int index)
	{
		focusedWorkspaceIndex = index;
		RedrawButtons(index);
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
			btns.Add(btn);
			panel.Add(btn);
		}

		if (btns.Count > 0)
			btns[focusedWorkspaceIndex].Background = theme.BUTTON_PRESSED_BACKGROUND;
		return panel;
	}

	public void RedrawButtons(int index)
	{
		this.Thread.Invoke(() =>
		{
			foreach (var btn in btns)
			{
				btn.Background = theme.BUTTON_BACKGROUND;
			}
			btns[index].Background = theme.BUTTON_PRESSED_BACKGROUND;
			btns[index].HoverEffect = false;
		});
	}

	// for updating Glaze when buttons pressed 
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
			await Sambar.api.ChangeWorkspace(clickedBtnIndex);
			await Task.Delay(3000);
			buttonRedrawing = false;
		});
	}
}

