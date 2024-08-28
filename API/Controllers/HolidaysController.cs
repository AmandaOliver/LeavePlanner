using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Newtonsoft.Json;

public static class HolidaysController
{
	public static void MapHolidaysEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/holidays/{countryCode}", FetchPublicHolidays);
	}
	private static async Task<IResult> FetchPublicHolidays(string countryCode)
	{
		var holidays = new List<HolidayModel>();
		try
		{
			using (var httpClient = new HttpClient())
			{
				var url = $"https://www.googleapis.com/calendar/v3/calendars/en.{countryCode}.official%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c";
				var response = await httpClient.GetAsync(url);
				if (response.IsSuccessStatusCode)
				{
					var data = await response.Content.ReadAsStringAsync();
					var json = JsonConvert.DeserializeObject<GoogleCalendarResponse>(data);
					if (json != null && json.Items != null)
					{
						foreach (var item in json.Items)
						{
							if (item.Start != null && item.Start.Date != null && item.End != null && item.End.Date != null)
							{
								holidays.Add(new HolidayModel
								{
									StartDate = DateTime.Parse(item.Start.Date),
									EndDate = DateTime.Parse(item.End.Date),
									Summary = item.Summary
								});
							}
							else
							{
								throw new Exception("holiday missing date");
							}
						}
					}
					else
					{
						throw new Exception("response from google doesn't contain items");
					}
				}
			}
		}
		catch (Exception ex)
		{
			throw new Exception("Error when fetching holidays", ex);
		}
		return Results.Ok(holidays);
	}

}