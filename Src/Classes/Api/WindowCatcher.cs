using System.Diagnostics;

namespace sambar;

public partial class Api
{
	public void WindowCatcherInit()
	{
		STRUCTURE_CHANGED_EVENT += CaptureWindows;
	}

	private bool capturing = false;
	public List<StructureChangedMessage> capturedWindows = new();
	public int WINDOW_CAPTURE_DURATION = 50;
	public async void StartTimedWindowCapture()
	{
		capturing = true;
		capturedWindows = new();
		await Task.Delay(WINDOW_CAPTURE_DURATION);
		capturing = false;
	}
	private void CaptureWindows(StructureChangedMessage msg)
	{
		//Logger.Log($"StructureChanged, name: {msg.name}, class: {msg.className}, type: {msg.controlType}");
		if (capturing)
		{
			capturedWindows.Add(msg);
		}
	}
}
