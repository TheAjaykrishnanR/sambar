using System.Windows;
using System.Windows.Media;

return (dynamic taskbarApps, dynamic ENV) =>
{
	taskbarApps.theme.BUTTON_PRESSED_BORDER_COLOR = new SolidColorBrush(Colors.Black);
	taskbarApps.theme.BUTTON_PRESSED_BACKGROUND = new SolidColorBrush(Colors.Transparent);
	taskbarApps.theme.BUTTON_WIDTH = 18;
	taskbarApps.theme.BUTTON_HEIGHT = 18;
};
