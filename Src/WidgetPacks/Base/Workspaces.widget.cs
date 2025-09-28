public class Workspaces : Widget
{
	List<Workspace> workspaces = new();
	public List<RoundedButton> buttons = new();
	public Theme theme = new();

	/*
	 * Made public so that they can be edited in a mod file (Workspaces.mod.cs)
	 * */

	//public int BUTTON_HEIGHT = theme.BUTTON_HEIGHT;
	//public int BUTTON_WIDTH = theme.BUTTON_WIDTH;

	//FONT_FAMILY = theme.FONT_FAMILY;
	//BUTTON_CORNER_RADIUS = theme.BUTTON_CORNER_RADIUS;
	//BUTTON_WIDTH = theme.BUTTON_WIDTH;
	//BUTTON_HEIGHT = theme.BUTTON_HEIGHT;
	//BUTTON_BORDER_THICKNESS = theme.BUTTON_BORDER_THICKNESS;
	//BUTTON_BORDER_COLOR = theme.BUTTON_BORDER_COLOR;
	//TEXT_COLOR = theme.TEXT_COLOR;
	//BUTTON_HOVER_COLOR = theme.BUTTON_HOVER_COLOR;
	//BUTTON_BACKGROUND = theme.BUTTON_BACKGROUND;

	//public Brush BUTTON_BACKGROUND = theme.BUTTON_BACKGROUND;
	//public Brush BUTTON_PRESSED_BACKGROUND = theme.BUTTON_PRESSED_BACKGROUND;

	public Workspaces(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		workspaces = Sambar.api.workspaces;

		//this.CornerRadius = theme.WIDGET_CORNER_RADIUS;
		StackPanelWithGaps panel = new(theme.WIDGET_GAP, workspaces.Count);
		panel.Orientation = Orientation.Horizontal;
		panel.VerticalAlignment = VerticalAlignment.Center;
		panel.ClipToBounds = true;
		panel.Height = Sambar.api.config.height;

		for (int i = 1; i <= workspaces.Count; i++)
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
		Sambar.api.Print($"workspaces: {workspaces.Count}, buttons: {buttons.Count}, index: {Sambar.api.currentWorkspace.index}");
		if (buttons.Count > 0)
			buttons[Sambar.api.currentWorkspace.index].Background = theme.BUTTON_PRESSED_BACKGROUND;
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
				button.Background = theme.BUTTON_BACKGROUND;
			}
			buttons[index].Background = theme.BUTTON_PRESSED_BACKGROUND;
			buttons[index].HoverEffect = false;
		});
	}

	// for updating Glaze when buttons pressed
	bool buttonRedrawing = false;
	public void WorkspaceButtonClicked(object? sender, RoutedEventArgs e)
	{
		buttonRedrawing = true;
		var btn = sender as RoundedButton;
		string clickedBtnName = Convert.ToString(btn.Text);
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

