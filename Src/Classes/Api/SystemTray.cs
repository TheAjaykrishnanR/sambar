using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using Interop.UIAutomationClient;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32;

namespace sambar;

//public partial class Api {
//
//    private List<TrayIcon> trayIcons = new();
//    private RegistryKey trayIconsRegistryKeyRoot;
//    private List<TrayIconRegKey> UIOrderListRegKeys = new();
//
//    // Constructor
//    private void SystemTrayInit()
//    {
//        // for trayIcon's images
//        trayIconsRegistryKeyRoot = Registry.CurrentUser.OpenSubKey("Control Panel").OpenSubKey("NotifyIconSettings");
//        byte[] raw_UIOrderList = (byte[])trayIconsRegistryKeyRoot.GetValue("UIOrderList");
//        string fullHex = Convert.ToHexStringLower(raw_UIOrderList);
//        Logger.Log($"UIOrderList: {fullHex}");
//        fullHex.Chunk(16).ToList().ForEach(chunk => {
//            string hex = new(chunk);
//            char[][] chunks = hex.Chunk(2).ToArray();
//            chunks = chunks.Reverse().ToArray();
//            string reverse = "";
//            chunks.ToList().ForEach(_c => reverse += new string(_c));
//            ulong num = (ulong)Int64.Parse(reverse, System.Globalization.NumberStyles.HexNumber);
//            //Logger.Log($"chunk: {hex}, reverse: {reverse}, decimal: {num}");
//            TrayIconRegKey key = new();
//            key.parentKey = $"{num}";
//            key.ExecutablePath = (string)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("ExecutablePath");
//            key.IconGuid = (string)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("IconGuid");
//            key.IconSnapshot = (byte[])trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("IconSnapshot");
//            key.UID = unchecked((uint?)(int?)trayIconsRegistryKeyRoot.OpenSubKey($"{num}").GetValue("UID"));
//            UIOrderListRegKeys.Add(key);
//            Logger.Log($"{num}, {key.ExecutablePath}");
//        });
//
//        Process[] runningProcesses = Process.GetProcesses();
//        runningProcesses.ToList().ForEach(p => Logger.Log("executable path: " + p.ProcessName));
//        List<string> trayProcesses = new();
//        // filter out the non running ones
//        UIOrderListRegKeys = UIOrderListRegKeys
//            .Where(
//                key => runningProcesses
//                        .Select(_p => _p.ProcessName)
//                        .ToList()
//                        .Contains(key.ExecutablePath
//                                        .Split(@"\")
//                                        .Last()
//                                        .Replace(".exe", "")
//            ))
//            .ToList();
//
//
//        //trayIconRegKeys.ForEach(item => Logger.Log($"running: {item.ExecutablePath}"));
//
//        // from these use either IconGuid or hWnd + UID to figure out which is in overflow
//        // tray by Shell_NotifyIconGetRect()
//        /*
//        List<GUIProcess> allWindows = Utils.EnumWindowProcesses();
//        List<TrayIconRegKey> actuallyInTrayRegKeys = new();
//        foreach(var key in UIOrderListRegKeys)
//        {
//            _NOTIFYICONIDENTIFIER identifier = new();
//            identifier.cbSize = (uint)Marshal.SizeOf<_NOTIFYICONIDENTIFIER>();
//            long result;    
//            // IconGuid exists
//            if(key.IconGuid != null)
//            {
//                Logger.Log($"{key.ExecutablePath}, IconGuid, parent: {key.parentKey}");
//                identifier.guidItem = new(key.IconGuid);
//                if((result = Shell32.Shell_NotifyIconGetRect(ref identifier, out RECT iconLocation)) != 0) 
//                {
//                    Logger.Log($"failed: {result}");
//                }
//                if (
//                    iconLocation.Left != 0 ||
//                    iconLocation.Top != 0 ||
//                    iconLocation.Right != 0 ||
//                    iconLocation.Bottom != 0
//                ) actuallyInTrayRegKeys.Add(key);
//            }
//            // UID + hWnd
//            else if(key.UID != null)
//            {
//                identifier.UID = (uint)key.UID;
//                string exePath = key.ExecutablePath;
//                string processName = exePath.Split(@"\").Last().Replace(".exe", "");
//                if (processName == "Taskmgr") identifier.UID = 0;
//                var process = runningProcesses.Where(_p => _p.ProcessName == processName).First();
//                GUIProcess guiProcess = allWindows.Where(gui_p => gui_p.name == process.ProcessName).First();
//                foreach (var window in guiProcess.windows)
//                {
//                    identifier.hWnd = window.hWnd;
//                    if((result = Shell32.Shell_NotifyIconGetRect(ref identifier, out RECT iconLocation)) != 0) 
//                    {
//                        Logger.Log($"failed: {result}");
//                    }
//                    if (
//                        iconLocation.Left != 0 ||
//                        iconLocation.Top != 0 ||
//                        iconLocation.Right != 0 ||
//                        iconLocation.Bottom != 0
//                    )
//                    {
//                        actuallyInTrayRegKeys.Add(key);
//                        break;
//                    }
//                }
//                //nint hWnd = Utils.GetHWNDFromPID(process.Id);
//                Logger.Log($"{key.ExecutablePath}, UID: {key.UID}, handle: {identifier.hWnd}, parent: {key.parentKey}");
//            }
//            else
//            {
//                Logger.Log($"BOTH NULL, path: {key.ExecutablePath}, UID: {key.UID}, parent: {key.parentKey}");
//            }
//        }
//
//        actuallyInTrayRegKeys.ForEach(key => Logger.Log("regInTrayFound: " + key.ExecutablePath));
//        */
//        // Enumerate TrayOverflowMenu
//        //IntPtr hWnd_Overflow = User32.FindWindow("TopLevelWindowForOverflowXamlIsland", null);
//        //IntPtr hWnd_IconContainer = User32.FindWindowEx(hWnd_Overflow, IntPtr.Zero, "Windows.UI.Composition.DesktopWindowContentBridge", null);
//
//        //var innerIconContainer = ui.ElementFromHandle(hWnd_IconContainer);
//        //var icons = innerIconContainer.FindAll(TreeScope.TreeScope_Children, ui.CreateTrueCondition());
//        //
//        //for(int i = 0; i < icons.Length; i++) {
//        //    var icon = icons.GetElement(i);
//        //    TrayIcon trayIcon = new(i, hWnd_Overflow, icon as IUIAutomationElement3);
//        //    trayIcon.szTip = icon.CurrentName;
//        //    trayIcons.Add(trayIcon);
//        //}
//
//        //trayIcons.ForEach(icon => Logger.Log($"trayIcon, {icon.element.FindAll(TreeScope.TreeScope_Children, ui.CreateTrueCondition()).GetElement(0).CurrentClassName}") );
//        
//        
//        // icons obtained through registry enumeration must be same as that obtained
//        // from IUIAutomation
//        //Trace.Assert(actuallyInTrayRegKeys.Count == icons.Length);
//
//
//    }
//
//   
//}

//public class TrayIconRegKey
//{
//    public string parentKey;
//    public string ExecutablePath;
//    public string IconGuid;
//    public byte[] IconSnapshot;
//    public uint? UID;
//}



