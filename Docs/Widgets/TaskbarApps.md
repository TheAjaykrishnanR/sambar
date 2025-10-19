# TaskbarApps

widget file : https://github.com/TheAjaykrishnanR/sambar/blob/master/Src/WidgetPacks/Base/TaskbarApps.widget.cs 

## Configuration

In the widget file add the apps you want to pin to the `pinnedApps` list as follows :

```
List<RunningApp?> pinnedApps = new()
{
    /*
     *   Insert apps you want to pin to the status bar below:
     *
     *   new(@"C:\path\to\your\app1\app1Name.exe"),
     *   new(@"C:\path\to\your\app2\app2Name.exe"),
     *   new(@"C:\path\to\your\app3\app3Name.exe"),
     *
    */
};
```
