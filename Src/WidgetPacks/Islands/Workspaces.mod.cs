return (dynamic workspaces, dynamic ENV) =>
{
	workspaces.Background = Utils.BrushFromHex("#1a1a1a");
	workspaces.CornerRadius = new CornerRadius(8);
	workspaces.theme.BUTTON_PRESSED_BACKGROUND = Utils.BrushFromHex("#d856f5");
	workspaces.theme.BUTTON_CORNER_RADIUS = new CornerRadius(8);
};

