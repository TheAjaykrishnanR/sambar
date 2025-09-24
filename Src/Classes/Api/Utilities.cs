/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using System.Windows.Media.Imaging;

namespace sambar;

/// <summary>
/// Some common utilities and ease of life functions to use
/// </summary>
public partial class Api
{
	public BitmapImage GetImageSource(string imageFile)
	{
		return new BitmapImage(new Uri(imageFile));
	}
}
