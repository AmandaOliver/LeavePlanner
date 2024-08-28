namespace LeavePlanner.Models
{
	public class HolidayModel
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string? Summary { get; set; }
	}
	public class GoogleCalendarResponse
	{
		public List<CalendarEvent>? Items { get; set; }
	}

	public class CalendarEvent
	{
		public CalendarEventDate? Start { get; set; }
		public CalendarEventDate? End { get; set; }
		public string? Summary { get; set; }
	}

	public class CalendarEventDate
	{
		public string? Date { get; set; }
	}

}