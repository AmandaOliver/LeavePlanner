using LeavePlanner.Configuration;
using LeavePlanner.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LeavePlanner.Infrastructure.Calendar;

public class GooglePublicHolidayCalendar : IPublicHolidayCalendar
{
	private readonly HttpClient _httpClient;
	private readonly string _apiKey;

	public GooglePublicHolidayCalendar(HttpClient httpClient, IOptions<GoogleCalendarOptions> options)
	{
		_httpClient = httpClient;
		_apiKey = options.Value.ApiKey;
	}

	public async Task<List<PublicHoliday>> GetUpcomingAsync(string countryCode, CancellationToken cancellationToken)
	{
		try
		{
			var url = $"https://www.googleapis.com/calendar/v3/calendars/en.{countryCode}.official%23holiday%40group.v.calendar.google.com/events?key={Uri.EscapeDataString(_apiKey)}";
			var response = await _httpClient.GetAsync(url, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				return [];
			}

			var data = await response.Content.ReadAsStringAsync(cancellationToken);
			var json = JsonConvert.DeserializeObject<GoogleCalendarResponse>(data);
			if (json?.Items == null)
			{
				throw new Exception("response from google doesn't contain items");
			}

			var holidays = new List<PublicHoliday>();
			foreach (var item in json.Items)
			{
				if (item.Start?.Date == null || item.End?.Date == null)
				{
					throw new Exception("holiday returned by google is missing date");
				}

				var start = DateTime.Parse(item.Start.Date);
				if (start > DateTime.UtcNow)
				{
					holidays.Add(new PublicHoliday(start, DateTime.Parse(item.End.Date), item.Summary));
				}
			}

			return holidays;
		}
		catch (Exception ex) when (ex is not DomainException)
		{
			throw new Exception("Error when fetching holidays", ex);
		}
	}

	private sealed class GoogleCalendarResponse
	{
		public List<CalendarEvent>? Items { get; set; }
	}

	private sealed class CalendarEvent
	{
		public CalendarEventDate? Start { get; set; }
		public CalendarEventDate? End { get; set; }
		public string? Summary { get; set; }
	}

	private sealed class CalendarEventDate
	{
		public string? Date { get; set; }
	}
}
