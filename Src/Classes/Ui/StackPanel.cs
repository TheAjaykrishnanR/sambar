using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace sambar;

public class StackPanelWithGaps : StackPanel
{
	int gap;
	int capacity;
	public StackPanelWithGaps(int gap, int capacity)
	{
		this.gap = gap;
		this.capacity = capacity;

		if (this.Orientation == Orientation.Horizontal)
		{
			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.VerticalAlignment = VerticalAlignment.Center;
		}
		else
		{
			this.HorizontalAlignment = HorizontalAlignment.Center;
			this.VerticalAlignment = VerticalAlignment.Top;
		}
	}

	int currentElement = 0;
	public void Add(Control control)
	{
		Logger.Log($"currentElement: {currentElement}, capacity: {capacity}");
		if (currentElement < capacity - 1)
		{
			if (this.Orientation == Orientation.Horizontal)
			{
				control.Margin = new(gap, 0, 0, 0);
			}
			else
			{
				control.Margin = new(0, gap, 0, 0);
			}
		}
		else if (currentElement == capacity - 1)
		{
			if (this.Orientation == Orientation.Horizontal)
			{
				Logger.Log("LAST ELEMENT");
				control.Margin = new(gap, 0, gap, 0);
			}
			else
			{
				control.Margin = new(0, gap, 0, gap);
			}
		}
		else
		{
			var last = this.Children[^1] as UserControl;
			if (this.Orientation == Orientation.Horizontal)
			{
				last.Margin = new(gap, 0, 0, 0);
				control.Margin = new(gap, 0, gap, 0);
			}
			else
			{
				last.Margin = new(0, gap, 0, 0);
				control.Margin = new(0, gap, 0, gap);
			}
			capacity++;
		}
		this.Children.Add(control);
		currentElement++;
	}
}
