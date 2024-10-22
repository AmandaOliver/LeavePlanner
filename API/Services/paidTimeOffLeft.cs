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

		// Fetch paidTimeOff leaves for the current year excluding the leave being updated/created
		var leavesThisYear = await _context.Leaves
			.Where(l => l.Owner == employeeEmail && l.Id != leaveId && l.Type == "paidTimeOff" && l.ApprovedBy != null && (l.DateStart.Year == year || l.DateEnd.Year == year))
			.ToListAsync();


		// Calculate total days taken 
		int totalDaysTaken = 0;
		foreach (var leave in leavesThisYear)
		{
			int daysRequested = await GetDaysRequested(leave.DateStart, leave.DateEnd, employeeEmail, year, leave.Id);
			totalDaysTaken = totalDaysTaken + daysRequested;
		}

		// Return remaining paid time off
		return employee.PaidTimeOff - totalDaysTaken;
	}

	async public Task<int> GetDaysRequested(DateTime start, DateTime end, string owner, int year, int? leaveId)
	{
		var leaveCreationDate = DateTime.UtcNow;
		if (leaveId != null)
		{
			var currentLeave = await _context.Leaves.FindAsync(leaveId);
			if (currentLeave == null)
			{
				throw new Exception("leave not found");
			}
			leaveCreationDate = currentLeave.CreatedAt;
			_context.Leaves.Remove(currentLeave);
		}
		// Check for conflicting leaves
		var conflictingLeaves = await _context.Leaves
			.Where(leave =>
				leave.Owner == owner && // is a leave of this employee
				(leaveId != null ? leave.Id != leaveId : true) && // if we are updating, do not take in account the previous version
				leave.CreatedAt < leaveCreationDate && // we only take in account leaves created previously (this is important when retrieving days dynamically on get calls)
				(
					(start >= leave.DateStart && start < leave.DateEnd) ||   // Start date is within an existing leave (excluding an exact match on end date)
					(end > leave.DateStart && end <= leave.DateEnd) ||       // End date is within an existing leave (excluding an exact match on start date)
					(start < leave.DateStart && end > leave.DateEnd)         // The requested leave fully contains an existing leave
				))
			.ToListAsync();


		int totalDays = 0;

		// Loop through all days between start and end date
		for (DateTime date = start; date <= end.AddDays(-1); date = date.AddDays(1))
		{
			// Only count weekdays (Monday to Friday)
			if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday &&
			//only count days that are not within another leave already
			!conflictingLeaves.Any(leave => date >= leave.DateStart && date < leave.DateEnd) &&
			// only count days on the year we are interested in
			date.Year == year)
			{
				totalDays++;
			}
		}

		return totalDays;
	}

}
