namespace sambar;

public partial class Api
{
	public delegate void ClockTickedEventHandler(Time time);
	public event ClockTickedEventHandler CLOCK_TICKED = (time) => { };
	private void ClockInit()
	{
		Task.Run(async () =>
		{
			while (true)
			{
				Time now = new();
				CLOCK_TICKED(now);
				await Task.Delay(1000);
			}
		});
	}

	// Api Endpoint
	// Set polling frequence / event firing rate
}

public class Time
{
	public int seconds = DateTime.Now.Second;
	public int minutes = DateTime.Now.Minute;
	public int hours = DateTime.Now.Hour;
	public int day = DateTime.Now.Day;
	public int month = DateTime.Now.Month;
	public int year = DateTime.Now.Year;
	public DayOfWeek dayName = DateTime.Now.DayOfWeek;
	public string monthName { get { return months[month]; } }

	List<string> months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
}

