/*
	MIT License
    Copyright (c) 2025 Ajaykrishnan R	
*/

using Newtonsoft.Json;
using System.Windows;

namespace sambar;

public class Config
{
	public int height = 40;
	public int width = 0;
	public string dockEdge = "top";
	public int marginXLeft = 10;
	public int marginXRight = 10;
	public int marginYTop = 10;
	public int marginYBottom = 10;
	public int paddingXLeft = 0;
	public int paddingXRight = 0;
	public int paddingYTop = 0;
	public int paddingYDown = 0;
	public string backgroundColor = "";
	public bool roundedCorners = true;
	public string borderColor = "";
	public Thickness borderThickness = new(0);
	public bool hardwareRendering = true;

	[JsonConstructor]
	public Config(
		int? height,
		int? width,
		int? marginXLeft,
		int? marginXRight,
		int? marginYTop,
		int? paddingXLeft,
		int? paddingXRight,
		int? paddingYTop,
		int? paddingYDown,
		string? backgroundColor,
		bool? roundedCorners,
		string? borderColor,
		Thickness? borderThickness,
		bool? hardwareRendering
	)
	{

		this.height = height ?? 40;
		this.width = width ?? 0;
		this.marginXLeft = marginXLeft ?? 10;
		this.marginXRight = marginXRight ?? 10;
		this.marginYTop = marginYTop ?? 10;
		this.paddingXLeft = paddingXLeft ?? 0;
		this.paddingXRight = paddingXRight ?? 0;
		this.paddingYTop = paddingYTop ?? 0;
		this.paddingYDown = paddingYDown ?? 0;
		this.backgroundColor = backgroundColor ?? "#ffffff";
		this.borderColor = borderColor ?? "#ffffff";
		this.borderThickness = borderThickness ?? new(0);
		this.hardwareRendering = hardwareRendering ?? true;
	}

	public Config(int screenWidth)
	{
		width = screenWidth - (marginXLeft + marginXRight);
	}

	public Config() { }
}

// .imports.cs
public class WidgetImports
{
	public string importsPack = "";
	public List<string> widgets = new();
	//                widget        pack    file
	public Dictionary<string, List<(string, string)>> usings = new();
}


