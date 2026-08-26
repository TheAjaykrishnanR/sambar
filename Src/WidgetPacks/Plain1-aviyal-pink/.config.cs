public class BaseConfig : Config
{
	public BaseConfig()
	{
		this.height = 20;
		this.width = 0;
		this.dockEdge = "top";
		this.marginXLeft = 0;
		this.marginXRight = 0;
		this.marginYTop = 0;
		this.marginYBottom = 0;
		this.paddingXLeft = 0;
		this.paddingXRight = 0;
		this.paddingYTop = 0;
		this.paddingYDown = 0;
		//this.backgroundColor = "#1a1a1a";
		this.backgroundColor = "#312124";
		this.borderColor = "#ffffff";
		this.borderThickness = new(0);
		this.roundedCorners = false;
	}
}
