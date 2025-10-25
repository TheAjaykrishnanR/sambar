using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace sambar;

public partial class Api
{
	IActiveDesktop activeDesktop;

	public void SystemInit()
	{
		Type? activeDesktopClass = Type.GetTypeFromCLSID(CLSID.ActiveDesktop);
		activeDesktop = (IActiveDesktop)Activator.CreateInstance(activeDesktopClass);
		Logger.Log($"ad_class_null: {activeDesktopClass == null}, ad_null: {activeDesktop == null}");
	}

	public string GetWallpaper()
	{
		StringBuilder str = new(512);
		activeDesktop?.GetWallpaper(str, str.Capacity, (int)AD_GETWP.IMAGE);
		Logger.Log($"the wallpaper is {str.ToString()}");
		return str.ToString();
	}

	public void SetWallpaper(string imageFile)
	{
		int? res = activeDesktop?.SetWallpaper(imageFile, 0 /* dwReserved = 0 */);
		Logger.Log($"Setting Wallpaper: {imageFile}, res: {res}, iadnull: {activeDesktop == null}");
		activeDesktop?.ApplyChanges(AD_Apply.ALL);
	}

	public void SetWallpaper(string imageFile, WallpaperAnimation animation)
	{
		Window? wnd = Sambar.api?.CreateDesktopOverlay();
		wnd!.Background = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
		Canvas canvas = new();

		Image img = new() { Source = GetImageSource(imageFile) };

		(int imgWidth, int imgHeight) = Utils.GetImageDimensions(imageFile);
		(img.Width, img.Height) = Utils.ScaleImage(imgWidth, imgHeight, (int)wnd.Width, (int)wnd.Height);

		Logger.Log($"img.Width: {img.Width}, img.Height: {img.Height}, actual => W: {imgWidth}, H: {imgHeight}, [screen, x: {Sambar.screenWidth}, y: {Sambar.screenHeight}]");

		// register a name for the ellipse so it can be targetted for animations
		NameScope.SetNameScope(wnd, new NameScope());
		wnd!.RegisterName(animation.maskShapeIdentifier, animation.maskShape);

		GeometryDrawing geometryDrawing = new() { Geometry = animation.maskShape, Brush = new SolidColorBrush(Colors.Black) };

		//https://stackoverflow.com/questions/14283528/positioning-an-opacitymask-in-wpf
		DrawingBrush drawingBrush = new()
		{
			Drawing = geometryDrawing,
			Stretch = Stretch.None,
			ViewboxUnits = BrushMappingMode.Absolute,
			AlignmentX = AlignmentX.Left,
			AlignmentY = AlignmentY.Top
		};

		img.OpacityMask = drawingBrush;

		// triggers animation at window load
		wnd.Loaded += (s, e) => { animation.sequence.Begin(wnd); };
		canvas.Children.Add(img);

		// close the window once animation is complete and wallpaper is set
		animation.sequence.Completed += (s, e) =>
		{
			SetWallpaper(imageFile);
			Thread.Sleep(500);
			Logger.Log("closing overlay window");
			wnd.Close();
		};

		wnd!.Content = canvas;
		wnd.Show();
	}

	// execute shell commands from widgets
	public string? ExecuteShellCommand(string commandLine) => Utils.ExecuteShellCommand(commandLine);
}

public class WallpaperAnimation
{
	public Geometry maskShape; // shape of the moving mask
	public string maskShapeIdentifier = "maskShapeName"; // name of the mask
	public Storyboard sequence = new(); // the actual movement
}

