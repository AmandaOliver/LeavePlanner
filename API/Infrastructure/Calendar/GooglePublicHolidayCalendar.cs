using LeavePlanner.Configuration;
using LeavePlanner.Domain;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace LeavePlanner.Infrastructure.Calendar;

public class GooglePublicHolidayCalendar : IPublicHolidayCalendar
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<GooglePublicHolidayCalendar> _logger;
	private readonly string _apiKey;

	public GooglePublicHolidayCalendar(
		HttpClient httpClient,
		IOptions<GoogleCalendarOptions> options,
		ILogger<GooglePublicHolidayCalendar> logger)
	{
		_httpClient = httpClient;
		_logger = logger;
		_apiKey = options.Value.ApiKey;
	}

	public async Task<List<PublicHoliday>> GetUpcomingAsync(string countryCode, CancellationToken cancellationToken)
	{
		try
		{
			var now = DateTime.UtcNow;
			var query = new QueryString()
				.Add("key", _apiKey)
				.Add("singleEvents", "true")
				.Add("orderBy", "startTime")
				.Add("timeMin", now.ToString("o"))
				.Add("timeMax", now.AddYears(2).ToString("o"));
			var calendarId = Uri.EscapeDataString($"en.{countryCode}.official#holiday@group.v.calendar.google.com");
			var url = $"https://www.googleapis.com/calendar/v3/calendars/{calendarId}/events{query}";

			var response = await _httpClient.GetAsync(url, cancellationToken);
			if (!response.IsSuccessStatusCode)
			{
				_logger.LogWarning(
					"Google holiday calendar returned {StatusCode} for {CountryCode}",
					(int)response.StatusCode,
					countryCode);
				throw new DomainException("Could not load public holidays for this country.");
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
					continue;
				}

				var start = DateTime.Parse(item.Start.Date);
				if (start >= now.Date)
				{
					holidays.Add(new PublicHoliday(start, DateTime.Parse(item.End.Date), item.Summary));
				}
			}

			return holidays;
		}
		catch (Exception ex) when (ex is not DomainException and not OperationCanceledException)
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
