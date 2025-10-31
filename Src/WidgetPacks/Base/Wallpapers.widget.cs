public class Wallpapers : Widget
{
	public RoundedButton btn = new();
	public string iconFile = "image.svg";
	public Theme theme = new();
	string wallpapersFolder;

	public Wallpapers(WidgetEnv ENV) : base(ENV) { }
	public override void Init()
	{
		wallpapersFolder = Path.Join(ENV.HOME, "Pictures", "Wallpapers");

		if (File.Exists(Path.Join(ENV.ASSETS_FOLDER, iconFile)))
			btn.ImageSrc = Path.Join(ENV.ASSETS_FOLDER, iconFile);
		else
			btn.ImageSrc = Path.Join(ENV.IMPORTS_ASSETS_FOLDER, iconFile);
		btn.Height = theme.BUTTON_HEIGHT;
		btn.Width = theme.BUTTON_WIDTH;
		btn.Margin = theme.BUTTON_MARGIN;
		btn.IconWidth = 13;
		btn.IconHeight = 13;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.MouseDown += ButtonMouseDown;
		this.Content = btn;
	}
	public void ButtonMouseDown(object? sender, MouseEventArgs e)
	{
		//sambar.Menu menu = Sambar.api.CreateMenu(0, 0, 500, 300, centerOffset: true);
		string[] walls = Directory.GetFiles(wallpapersFolder).Where(path => path.EndsWith(".jpg") || path.EndsWith(".png") || path.EndsWith(".jpeg")).ToArray();

		sambar.Menu menu = Sambar.api.CreateMenu(0, 0, 500, 300, centerOffset: true);

		ImageSelector imageSelector = new();
		Task.Run(() =>
			this.Thread.Invoke(() =>
				imageSelector.Load(walls)
			)
		);
		imageSelector.IMAGE_SELECTED += ImageSelected;
		menu.Closing += (s, e) => imageSelector.Dispose();
		menu.Content = imageSelector;
	}

	void ImageSelected(string imgFile)
	{
		Sambar.api.SetWallpaper(imgFile, CreateAnimation());
	}

	public WallpaperAnimation CreateAnimation()
	{
		WallpaperAnimation animation = new();

		double final_radius = Math.Max(Sambar.screenWidth, Sambar.screenHeight);
		final_radius += 0.25 * final_radius;
		double radiusX_initial = 0, radiusX_final = final_radius;
		double radiusY_initial = 0, radiusY_final = final_radius;

		int duration = 2;
		DoubleAnimation doubleAnimationX = new()
		{
			From = radiusX_initial,
			To = radiusX_final,
			Duration = TimeSpan.FromSeconds(duration),
			AutoReverse = false
		};
		DoubleAnimation doubleAnimationY = new()
		{
			From = radiusX_initial,
			To = radiusY_final,
			Duration = TimeSpan.FromSeconds(duration),
			AutoReverse = false
		};

		Storyboard.SetTargetName(doubleAnimationX, animation.maskShapeIdentifier);
		Storyboard.SetTargetProperty(doubleAnimationX, new PropertyPath(EllipseGeometry.RadiusXProperty));
		Storyboard.SetTargetName(doubleAnimationY, animation.maskShapeIdentifier);
		Storyboard.SetTargetProperty(doubleAnimationY, new PropertyPath(EllipseGeometry.RadiusYProperty));

		animation.maskShape = new EllipseGeometry(new Point(0, 0), radiusX_initial, radiusY_initial);
		animation.sequence.Children.Add(doubleAnimationX);
		animation.sequence.Children.Add(doubleAnimationY);

		return animation;
	}
}
