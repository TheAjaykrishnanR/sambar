using System.Windows;
using System.Windows.Controls;
using System.Reflection;

namespace sambar;

public class Layout
{
	//public Panel Container;
	public UIElement Container;
	public Dictionary<string, Border> WidgetToContainerMap = new();
	public Layout()
	{
		this.GetType()
			.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			.ToList()
			.Where(fieldInfo => fieldInfo.FieldType == typeof(Border))
			.ToList()
			.ForEach(fieldInfo =>
			{
				WidgetToContainerMap[fieldInfo.Name] = (Border)fieldInfo.GetValue(this);
			});
	}
}
