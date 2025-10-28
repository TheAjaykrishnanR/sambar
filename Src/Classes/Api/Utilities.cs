using System.IO;
using System.Windows.Media.Imaging;

namespace sambar;

/// <summary>
/// Some common utilities and ease of life functions to use
/// </summary>
public partial class Api
{
	public List<FileStream> imgFiles = new();
	public BitmapImage GetImageSource(string imageFile)
	{
		BitmapImage bi = new();
		bi.BeginInit();
		FileStream fs = new(imageFile, FileMode.Open);
		imgFiles.Add(fs);
		bi.StreamSource = fs;
		bi.CacheOption = BitmapCacheOption.OnLoad;
		bi.EndInit();
		bi.Freeze();
		return bi;
	}
}
