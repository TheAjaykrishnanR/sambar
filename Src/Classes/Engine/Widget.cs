using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;

namespace sambar;

public class Widget : Border
{
	public int index;

	public UIElement Content
	{
		get { return this.Child; }
		set { this.Child = value; }
	}

	public Dispatcher Thread
	{
		get { return this.Dispatcher; }
	}

	Config config;
	public WidgetEnv ENV;

	// for widgets that intend to be as extensible as possible put the initializing
	// here so that they can be called after modAction has been executed
	public virtual void Init() { }

	protected Widget(WidgetEnv ENV)
	{
		this.HorizontalAlignment = HorizontalAlignment.Left;
		config = Sambar.api.config;

		this.ENV = ENV;
	}
}

public class WidgetEnv
{
	public string ASSETS_FOLDER = "";
	public string IMPORTS_ASSETS_FOLDER = "";
	public bool IS_IMPORTED = false;
	public string HOME = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
}
