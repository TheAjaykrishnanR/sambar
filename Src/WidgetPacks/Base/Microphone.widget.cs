public class Microphone: Widget {

	public RoundedButton btn = new();
	public string micOnImg = "mic_on.svg";
	public string micOffImg = "mic_off.svg";
	public Theme theme = new();
	string ASSETS_FOLDER = default;

	public Microphone(WidgetEnv ENV): base(ENV) { }
	public override void Init()
	{
		if (Directory.Exists(ENV.ASSETS_FOLDER)) ASSETS_FOLDER = ENV.ASSETS_FOLDER;
		else ASSETS_FOLDER = ENV.IMPORTS_ASSETS_FOLDER;

		btn.CornerRadius = theme.BUTTON_CORNER_RADIUS;
		btn.Margin = theme.BUTTON_MARGIN;
		btn.Height = theme.BUTTON_HEIGHT;
		btn.Width = theme.BUTTON_WIDTH;
		btn.FontFamily = theme.FONT_FAMILY;
		btn.HoverColor = theme.BUTTON_HOVER_COLOR;
		btn.Background = theme.BUTTON_BACKGROUND;
		btn.HoverEffect = false;
		btn.MouseDown += ButtonMouseDown;

		this.Background = theme.WIDGET_BACKGROUND;
		this.Content = btn;

		// mic state
		MicTaskTask = Task.Run(MicTask);
		Task.Run(MicTaskChecker);
	}

	public void ButtonMouseDown(object sender, object args) {
		if((bool)Sambar.api.IsMicMuted()) Sambar.api.UnmuteMic();
		else Sambar.api.MuteMic();
	}	

	public async Task MicTask() {
		while(true)	{
			try{
				this.Thread.Invoke(() => {
					Sambar.api.Print($"[ MIC TASK ]");
					btn.ImageSrc = Sambar.api.IsMicMuted() == false ? Path.Join(ASSETS_FOLDER, micOnImg) : Path.Join(ASSETS_FOLDER, micOffImg);
					btn.IconWidth = 16;
					btn.IconHeight = 16;
				});
			}
			catch(Exception ex) {
				Sambar.api.Print($"[ MIC TASK EXCEPTION ]: {ex.Message}");
			}
			await Task.Delay(100);
		}
	}

	Task MicTaskTask;
	public async Task MicTaskChecker() {
		while(true) {
			Sambar.api.Print($"[ MicTaskChecker ]: {MicTaskTask.Status}");
			await Task.Delay(500);
		}
	}
}