using System.ComponentModel.DataAnnotations;

namespace LeavePlanner.Configuration;

public class GoogleCalendarOptions
{
	public const string SectionName = "GoogleCalendar";

	[Required(ErrorMessage = "GoogleCalendar:ApiKey is required. See README.md > Configuration.")]
	public string ApiKey { get; set; } = default!;
}
