class BaseLayout : Layout
{
	/// <summary>
	/// Create placeholder borders for each widget. These borders must be
	/// declared as fields of the Layout class and their names must match
	/// the name of their respective widget class
	/// </summary>

	Border Workspaces = new();
	Border Tray = new();
	Border NetworkManager = new();
	Border HideTaskbar = new();
	Border Clock = new();
	Border ScribblePad = new();
	Border StartButton = new();
	Border Counters = new();
	Border TaskbarApps = new();
	Border Wallpapers = new();

	public BaseLayout()
	{
		/// <summary>
		/// Build the layout after choosing a layout type
		/// <summary>
		Grid grid = new();

		ColumnDefinition _col1 = new() { Width = new GridLength(1, GridUnitType.Star) };
		ColumnDefinition _col2 = new() { Width = new GridLength(1, GridUnitType.Auto) };
		ColumnDefinition _col3 = new() { Width = new GridLength(1, GridUnitType.Star) };

		grid.ColumnDefinitions.Add(_col1);
		grid.ColumnDefinitions.Add(_col2);
		grid.ColumnDefinitions.Add(_col3);

		StackPanel col1 = new();
		StackPanel col2 = new();
		StackPanel col3 = new();

		/// <summary>
		/// In WPF VerticalAlignment and HorizontalAlignment are
		/// properties of an objects that specifies how it aligns
		/// itself in its parent container. It is NOT about specifying
		/// to a container element how to align its children
		/// </summary>

		col1.Orientation = Orientation.Horizontal;
		col1.VerticalAlignment = VerticalAlignment.Center;

		col2.Orientation = Orientation.Horizontal;
		col2.HorizontalAlignment = HorizontalAlignment.Center;
		col2.VerticalAlignment = VerticalAlignment.Center;

		col3.Orientation = Orientation.Horizontal;
		col3.HorizontalAlignment = HorizontalAlignment.Right;
		col3.VerticalAlignment = VerticalAlignment.Center;
		col3.FlowDirection = FlowDirection.RightToLeft;
		col3.Margin = new(0, 0, 6, 0);

		Grid.SetColumn(col1, 0);
		Grid.SetColumn(col2, 1);
		Grid.SetColumn(col3, 2);

		// col1
		col1.Children.Add(Workspaces);
		TaskbarApps.Margin = new(5, 0, 0, 0);
		col1.Children.Add(TaskbarApps);
		// col2
		Clock.HorizontalAlignment = HorizontalAlignment.Center;
		col2.Children.Add(Clock);
		// col3
		List<Border> systemTray = new();
		systemTray.Add(StartButton);
		systemTray.Add(Counters);
		systemTray.Add(NetworkManager);
		systemTray.Add(Tray);
		systemTray.Add(Wallpapers);
		//systemTray.Add(HideTaskbar);
		//systemTray.Add(ScribblePad);
		systemTray.ForEach(border =>
		{
			border.Margin = new(0, 0, 5, 0);
			border.VerticalAlignment = VerticalAlignment.Center;
			col3.Children.Add(border);
		});
		Counters.Margin = new(10, 0, 10, 0);

		/// <summary>
		/// Add all borders to the layout type [Grid || StackPanel]
		/// </summary>
		grid.Children.Add(col1);
		grid.Children.Add(col2);
		grid.Children.Add(col3);
		/// <summary>
		/// Set the finished layout type as the container
		/// <summary>
		//this.Container = panel;
		this.Container = grid;
	}
}
