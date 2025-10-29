using System.IO;
using System.Windows.Media.Imaging;

namespace sambar;

/// <summary>
/// Some common utilities and ease of life functions to use
/// </summary>
public partial class Api
{
	public BitmapImage? GetImageSource(string imageFile)
	{
		try
		{
			BitmapImage bi = new();
			bi.BeginInit();
			bi.UriSource = new(imageFile);
			bi.CacheOption = BitmapCacheOption.OnLoad;
			bi.EndInit();
			bi.Freeze();
			return bi;
		}
		catch (Exception ex)
		{
			Logger.Log(ex.Message);
			return null;
		}
	}
}
