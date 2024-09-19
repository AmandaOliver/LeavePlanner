using LeavePlanner.Models;
using LeavePlanner.Data;
using Microsoft.EntityFrameworkCore;
public interface IPaidTimeOffLeft
{
	Task<int> GetPaidTimeOffLeft(string employeeEmail, int year, int? leaveId);
}

public class PaidTimeOffLeft : IPaidTimeOffLeft
{
	private readonly LeavePlannerContext _context;
	public PaidTimeOffLeft(LeavePlannerContext context)
	{
		_context = context;
	}

	public async Task<int> GetPaidTimeOffLeft(string employeeEmail, int year, int? leaveId)
	{
		// Fetch employee details
		var employee = await _context.Employees.FindAsync(employeeEmail);
		if (employee == null)
		{
			throw new Exception("Employee not found.");
		}

		// Fetch leaves for the current year excluding the leave being updated/created
		var leavesThisYear = await _context.Leaves
			.Where(l => l.Owner == employeeEmail && l.Id != leaveId && l.Type == "paidTimeOff" && l.ApprovedBy != null && l.DateStart.Year == year)
			.ToListAsync();

		// Calculate total days taken (excluding weekends) 
		int totalDaysTaken = 0;
		foreach (var leave in leavesThisYear)
		{
			// Exclude the end date as a leave day
			totalDaysTaken += GetWeekdaysBetween(leave.DateStart, leave.DateEnd.AddDays(-1));
		}

		// Return remaining paid time off
		return employee.PaidTimeOff - totalDaysTaken;
	}

	public int GetWeekdaysBetween(DateTime start, DateTime end)
	{
		int totalDays = 0;

		// Loop through all days between start and end date
		for (DateTime date = start; date <= end; date = date.AddDays(1))
		{
			// Only count weekdays (Monday to Friday)
			if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
			{
				totalDays++;
			}
		}

		return totalDays;
	}
}
