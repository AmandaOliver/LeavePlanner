using LeavePlanner.Models;
using LeavePlanner.Data;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;


public class CountriesService
{
	private readonly LeavePlannerContext _context;
	private readonly HttpClient _httpClient;

	public CountriesService(LeavePlannerContext context, HttpClient httpClient)
	{
		_context = context;
		_httpClient = httpClient;

	}
	public async Task<(bool IsSuccess, string? ErrorMessage, List<Country>? countries)> GetCountries()
	{
		try
		{
			var countries = await _context.Countries.ToListAsync();
			return (true, null, countries);
		}
		catch (Exception ex)
		{
			return (false, ex.Message, null);
		}
	}
	public async Task<List<HolidayModel>> FetchBankHolidays(string countryCode)
	{
		var holidays = new List<HolidayModel>();
		try
		{

			var url = $"https://www.googleapis.com/calendar/v3/calendars/en.{countryCode}.official%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c";
			var response = await _httpClient.GetAsync(url);
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
			var country = await _context.Countries
				.FirstOrDefaultAsync(country => country.Name == employee.Country);

			var countryCode = country?.Code;
			if (countryCode != null)
			{
				var holidaysForEmployeeCountry = await FetchBankHolidays(countryCode);
				foreach (var holiday in holidaysForEmployeeCountry)
				{

					var systemEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == "system");
					if (systemEmployee != null)
					{
						var leave = new Leave
						{
							Type = "bankHoliday",
							DateStart = holiday.StartDate,
							DateEnd = holiday.EndDate,
							Owner = employee.Id,
							Description = holiday.Summary,
							ApprovedBy = systemEmployee.Id,
							RejectedBy = null,
							OwnerNavigation = employee,
							CreatedAt = DateTime.UtcNow
						};

						_context.Leaves.Add(leave);
					}
					else
					{
						throw new Exception("System employee not found");
					}

				}

				await _context.SaveChangesAsync();
			}
			else
			{
				throw new Exception("Employee country doesnt exists in the DB");
			}
		}
		else
		{
			throw new Exception("Employee has no country set");
		}
	}
}
