using LeavePlanner.Domain;

namespace LeavePlanner.Application.Calendar;

public class PublicHolidayGenerator
{
	private readonly ICountryRepository _countries;
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly IPublicHolidayCalendar _calendar;
	private readonly IClock _clock;

	public PublicHolidayGenerator(
		ICountryRepository countries,
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		IPublicHolidayCalendar calendar,
		IClock clock)
	{
		_countries = countries;
		_employees = employees;
		_leaves = leaves;
		_calendar = calendar;
		_clock = clock;
	}

	public async Task GenerateFor(Employee employee, CancellationToken cancellationToken)
	{
		if (employee.Country == null)
		{
			throw new DomainException("Employee has no country set");
		}

		var country = await _countries.GetByNameAsync(employee.Country, cancellationToken);
		if (country?.Code == null)
		{
			throw new DomainException("Employee country doesnt exists in the DB");
		}

		var holidays = await _calendar.GetUpcomingAsync(country.Code, cancellationToken);
		var system = await _employees.GetSystemAsync(cancellationToken);
		foreach (var holiday in holidays)
		{
			_leaves.Add(Leave.RecordPublicHoliday(employee, holiday, system.Id, _clock.UtcNow));
		}
	}

	public async Task ReplaceFor(Employee employee, CancellationToken cancellationToken)
	{
		_leaves.RemoveRange(await _leaves.GetPublicHolidaysOwnedByAsync(employee.Id, cancellationToken));
		await GenerateFor(employee, cancellationToken);
	}
}
