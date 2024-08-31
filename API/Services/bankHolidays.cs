using LeavePlanner.Models;
using LeavePlanner.Data;
using Newtonsoft.Json;

public interface IBankHolidayService
{
	Task<List<HolidayModel>> FetchBankHolidays(string countryCode);
	Task GenerateEmployeeBankHolidays(Employee employee);
}

public class BankHolidayService : IBankHolidayService
{
	private readonly LeavePlannerContext _context;
	public BankHolidayService(LeavePlannerContext context)
	{
		_context = context;
	}
	public async Task<List<HolidayModel>> FetchBankHolidays(string countryCode)
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
							if (item.Start != null && item.Start.Date != null
							&& item.End != null && item.End.Date != null)
							{
								if (DateTime.Parse(item.Start.Date) > DateTime.UtcNow)
								{
									holidays.Add(new HolidayModel
									{
										StartDate = DateTime.Parse(item.Start.Date),
										EndDate = DateTime.Parse(item.End.Date),
										Summary = item.Summary
									});
								}
							}
							else
							{
								throw new Exception("holiday returned by google is missing date");
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
		return holidays;
	}
	public async Task GenerateEmployeeBankHolidays(Employee employee)
	{
		if (employee.Country != null)
		{
			var holidaysForEmployeeCountry = await FetchBankHolidays(employee.Country);
			foreach (var holiday in holidaysForEmployeeCountry)
			{
				var leave = new Leave
				{
					Type = "bankHoliday",
					DateStart = holiday.StartDate,
					DateEnd = holiday.EndDate,
					Owner = employee.Email,
					Description = holiday.Summary,
					ApprovedBy = null // Bank holiday doesn't need approval
				};
				_context.Leaves.Add(leave);
			}

			await _context.SaveChangesAsync();
		}
		else
		{
			throw new Exception("Employee has no country set");
		}
	}
}
