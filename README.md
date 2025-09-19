# Sambar

![showcase_1](https://github.com/TheAjaykrishnanR/sambar/blob/master/Imgs/explorer_fi1Oz9MPqd.gif)

> **A native status bar for Windows 11 written in C# (.NET) with love ❤️. Utilizes the WPF technology and native interop to 
provide a rich set of functionalities which can be consumed through widgets. You can write your own widgets utilizing
the functions and events provided throug the API to spare yourself from reinventing the wheel everytime. Or you can even 
leverage the C# standard library and build features from scratch. Everything is configurable: The layout, dimensions,
positions and colors are <ins>fully customizable</ins>. [dive in?](https://github.com/TheAjaykrishnanR/sambar/blob/master/Docs/Landing.md)**

<ins>Sources and inspirations</ins>:

 - [yasb](https://github.com/amnweb/yasb)
 - [Seelen-UI](https://github.com/eythaann/Seelen-UI)
 - [zebar](https://github.com/glzr-io/zebar)

## Features

 - Native (WPF), lightweight and less resource intensive
 - Widget support

 <ins>currently available widgets</ins>:

 1. GlazeWM workspaces
 2. Tray icons
 3. Taskbar apps
 4. Buttons (Start, Action Center)
 5. Toggle native taskbar
 6. Performance counters (CPU, Memory, Network)
 7. Audio visualizer

## Requirements

 1. Windows 11 build 26100+
 2. .NET 9 Desktop Runtime (if you aren't running the self-contained version), download and install it from [here](https://dotnet.microsoft.com/en-us/download/dotnet/9.0/runtime)

## Building

 1. Download and Install .NET 9 SDK
 2. `git clone https://github.com/TheAjaykrishnanR/sambar`
 3. `cd Src`
 4. `dotnet build`

### To publish a self contained executable:

 1. `cd Src`
 2. `dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained -c Release`

You can find the executable at `bin\Release\net*\win-x64\publish`

## Documentation

Read the [docs](https://github.com/TheAjaykrishnanR/sambar/blob/master/Docs/Landing.md) here.

## Acknowledgements

Sambar wouldnt have been possible without the existence of all the libraries it depends on.
Thanks to :
 1. `NAudio`
 2. `Newtonsoft.Json`
 3. `ScottPlot`
 4. `SkiaSharp`

## Contributing

PRs welcome !

## License

This project is free to use, modify and distribute according to the MIT License.

