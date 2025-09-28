public class Workspaces : Widget
{
	List<Workspace> workspaces = new();
	public List<Button> buttons = new();
	public Theme theme = new();

	public Workspaces(WidgetEnv ENV) : base(ENV)
	{
		workspaces = Sambar.api.workspaces;

		//this.CornerRadius = theme.WIDGET_CORNER_RADIUS;
		StackPanelWithGaps panel = new(2, workspaces.Count);
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;
		panel.ClipToBounds = true;
		for (int i = 1; i <= workspaces.Count; i++)
		{
			Button btn = new();
			btn.Content = new TextBlock() { Text = $"{i}" };
			btn.FontFamily = theme.FONT_FAMILY;
			btn.Width = theme.BUTTON_WIDTH;
			btn.Height = theme.BUTTON_HEIGHT;
			btn.Foreground = theme.TEXT_COLOR;
			btn.Background = theme.BUTTON_BACKGROUND;
			btn.Click += WorkspaceButtonClicked;
			buttons.Add(btn);
			panel.Add(btn);
		}
		Sambar.api.Print($"workspaces: {workspaces.Count}, buttons: {buttons.Count}, index: {Sambar.api.currentWorkspace.index}");
		if (buttons.Count > 0)
		{
			var selectedButton = buttons[Sambar.api.currentWorkspace.index];
			selectedButton.Background = theme.BUTTON_PRESSED_BACKGROUND;
			selectedButton.Foreground = new SolidColorBrush(Colors.White);
		}
		Sambar.api.GLAZE_WORKSPACE_CHANGED += (workspace) =>
		{
			RedrawButtons(workspace.index);
		};
		this.Content = panel;
	}

	public void RedrawButtons(int index)
	{
		this.Thread.Invoke(() =>
		{
			foreach (var button in buttons)
			{
				button.Foreground = theme.TEXT_COLOR;
				button.Background = theme.BUTTON_BACKGROUND;
			}
			buttons[index].Foreground = new SolidColorBrush(Colors.White);
			buttons[index].Background = theme.BUTTON_PRESSED_BACKGROUND;
		});
	}

	// for updating Glaze when buttons pressed
	bool buttonRedrawing = false;
	public void WorkspaceButtonClicked(object? sender, RoutedEventArgs e)
	{
		buttonRedrawing = true;
		var btn = sender as Button;
		string clickedBtnName = (btn.Content as TextBlock).Text.ToString();
		Sambar.api.Print($"clickedBtnName: {clickedBtnName}");
		Workspace clickedWorkspace = workspaces.Where(wksp => wksp.name == clickedBtnName).First();
		int clickedBtnIndex = clickedWorkspace.index;
		RedrawButtons(clickedBtnIndex);
		Task.Run(async () =>
		{
			await Sambar.api.ChangeWorkspace(clickedWorkspace);
			await Task.Delay(3000);
			buttonRedrawing = false;
		});
	}
}

