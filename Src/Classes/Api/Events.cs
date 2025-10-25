using System.Diagnostics;
using Windows.Devices.SmartCards;
using Interop.UIAutomationClient;

namespace sambar;

public partial class Api
{
	public void EventsInit()
	{
		FOCUS_CHANGED_EVENT = (msg) => { };
		FocusChangedEventHandlerClass focusChangedHandlerObject = new();
		ui.AddFocusChangedEventHandler(null, focusChangedHandlerObject);

		STRUCTURE_CHANGED_EVENT = (msg) => { };
		StructureChangedEventHandlerClass structureChangedHandlerObject = new();
		ui.AddStructureChangedEventHandler(ui.GetRootElement(), TreeScope.TreeScope_Children, null, structureChangedHandlerObject);
	}

	public delegate void FocusChangedEventHandler(FocusChangedMessage msg);
	public static event FocusChangedEventHandler FOCUS_CHANGED_EVENT = (msg) => { };

	class FocusChangedEventHandlerClass : IUIAutomationFocusChangedEventHandler
	{
		public void HandleFocusChangedEvent(IUIAutomationElement sender)
		{
			//Logger.Log($"focusChangedEvent, name: {sender.CurrentName}, windowClass: {sender.CurrentClassName}, type: {sender.CurrentItemType}");
			try
			{
				FocusChangedMessage msg = new();
				msg.className = sender.CurrentClassName;
				msg.type = sender.CurrentItemType;
				msg.name = sender.CurrentName;
				msg.hWnd = sender.CurrentNativeWindowHandle;
				msg.controlType = (ControlType)sender.CurrentControlType;
				msg.sender = sender;
				FOCUS_CHANGED_EVENT(msg);
			}
			catch (Exception ex)
			{
				Logger.Log($"UIAutomation, HandleFocusChangedEvent: {ex.Message}");
			}
		}
	}

	public delegate void StructureChangedEventHandler(StructureChangedMessage msg);
	public static event StructureChangedEventHandler STRUCTURE_CHANGED_EVENT = (msg) => { };

	class StructureChangedEventHandlerClass : IUIAutomationStructureChangedEventHandler
	{
		public void HandleStructureChangedEvent(IUIAutomationElement sender, StructureChangeType changeType, int[] runtimeId)
		{
			//Logger.Log($"structureChangedEvent, senderName: {sender.CurrentName}, class: {sender.CurrentClassName}, hWnd: {sender.CurrentNativeWindowHandle}, changeType: {changeType}");
			try
			{
				StructureChangedMessage msg = new();
				msg.className = sender.CurrentClassName;
				msg.type = changeType;
				msg.name = sender.CurrentName;
				msg.hWnd = sender.CurrentNativeWindowHandle;
				msg.controlType = (ControlType)sender.CurrentControlType;
				STRUCTURE_CHANGED_EVENT(msg);
			}
			catch (Exception ex)
			{
				Logger.Log($"UIAutomation, StructureChangedEventHandler: {ex.Message}");
			}
		}
	}

	// frequencyAndWeights := (freq, weight)
	public delegate void AudioPlayedEventHandler(float[] amplitudes, (float, float)[] frequencyAndWeights);
	public static event AudioPlayedEventHandler AUDIO_PLAYED_EVENT = (a, fw) => { };
}

public class FocusChangedMessage
{
	public string? name;
	public string? className;
	public nint hWnd;
	public string? type;
	public ControlType controlType;
	public IUIAutomationElement? sender;
}

public class StructureChangedMessage
{
	public string? name;
	public string? className;
	public nint hWnd;
	public StructureChangeType type;
	public ControlType controlType;
}

/// <summary>
/// IUIAutomation ControlTypes
/// </summary>

public enum ControlType : int
{
	APPBAR = 50040,
	BUTTON = 50000,
	CALENDAR = 50001,
	CHECKBOX = 50002,
	COMBOBOX = 50003,
	CUSTOM = 50025,
	DATAGRID = 50028,
	DATAITEM = 50029,
	DOCUMENT = 50030,
	EDIT = 50004,
	GROUP = 50026,
	HEADER = 50034,
	HEADERITEM = 50035,
	HYPERLINK = 50005,
	IMAGE = 50006,
	LIST = 50008,
	LISTITEM = 50007,
	MENUBAR = 50010,
	MENU = 50009,
	MENUITEM = 50011,
	PANE = 50033,
	PROGRESSBAR = 50012,
	RADIOBUTTON = 50013,
	SCROLLBAR = 50014,
	SEMANTICZOOM = 50039,
	SEPARATOR = 50038,
	SLIDER = 50015,
	SPINNER = 50016,
	SPLITBUTTON = 50031,
	STATUSBAR = 50017,
	TAB = 50018,
	TABITEM = 50019,
	TABLE = 50036,
	TEXT = 50020,
	THUMB = 50027,
	TITLEBAR = 50037,
	TOOLBAR = 50021,
	TOOLTIP = 50022,
	TREE = 50023,
	TREEITEM = 50024,
	WINDOW = 50032
}
