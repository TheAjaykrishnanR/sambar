# Wallpaper changer

widget file: https://github.com/TheAjaykrishnanR/sambar/blob/master/Src/WidgetPacks/Base/Wallpapers.widget.cs

## Configuration parameters:

You must specify the folder where sambar checks for your pictures to set wallpapers.

If you look at the widget file you will notice that its specified as follows:

```
wallpapersFolder = Path.Join(ENV.HOME, "Pictures", "Wallpapers");
```

It will be set inside the `Init()` function

This sets the `wallpapersFolder` at `C:\Users\<Username>\Pictures\Wallpapers`
