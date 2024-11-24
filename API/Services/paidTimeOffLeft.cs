using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
public interface IPaidTimeOffLeft
{
	Task<int> GetPaidTimeOffLeft(int employeeId, int year, int? leaveId);
}

public class PaidTimeOffLeft : IPaidTimeOffLeft
{
	private readonly LeavePlannerContext _context;
	public PaidTimeOffLeft(LeavePlannerContext context)
	{
		_context = context;
	}

	public async Task<int> GetPaidTimeOffLeft(int employeeId, int year, int? leaveId)
	{
		// Fetch employee details
		var employee = await _context.Employees.FindAsync(employeeId);
		if (employee == null)
		{
			throw new Exception("Employee not found.");
		}

		// Fetch paidTimeOff leaves for the current year excluding the leave being updated/created
		var leavesThisYear = await _context.Leaves
			.Where(l => l.Owner == employeeId && l.Id != leaveId && l.Type == "paidTimeOff" && l.ApprovedBy != null && (l.DateStart.Year == year || l.DateEnd.Year == year))
			.ToListAsync();


		// Calculate total days taken 
		int totalDaysTaken = 0;
		foreach (var leave in leavesThisYear)
		{
			int daysRequested = await GetDaysRequested(leave.DateStart, leave.DateEnd, employeeId, year, leave.Id);
			totalDaysTaken = totalDaysTaken + daysRequested;
		}

		// Return remaining paid time off
		return employee.PaidTimeOff - totalDaysTaken;
	}

	async public Task<int> GetDaysRequested(DateTime start, DateTime end, int owner, int year, int? leaveId)
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
				(leave.ApprovedBy != null || leave.Type == "bankHoliday") && // we only take in account approved leaves and bank holidays
				(leaveId != null ? leave.Id != leaveId : true) && // if we are updating, do not take in account the previous version
				leave.CreatedAt < leaveCreationDate && // we only take in account leaves created previously (this is important when retrieving days dynamically on get calls)
				(
					(start >= leave.DateStart && start < leave.DateEnd) ||   // Start date is within an existing leave (excluding an exact match on end date)
					(end > leave.DateStart && end <= leave.DateEnd) ||       // End date is within an existing leave (excluding an exact match on start date)
					(start < leave.DateStart && end > leave.DateEnd)         // The requested leave fully contains an existing leave
				))
			.ToListAsync();


		int totalDays = 0;
		var employee = await _context.Employees.FindAsync(owner);
		if (employee == null)
		{
			throw new Exception("Owner not found");
		}
		var organization = await _context.Organizations.FindAsync(employee.Organization);
		if (organization == null)
		{
			throw new Exception("Organization not found");
		}
		// Loop through all days between start and end date
		for (DateTime date = start; date <= end.AddDays(-1); date = date.AddDays(1))
		{
			var dayOfWeek = date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;
			// Only count working days
			if (organization.WorkingDays.Contains(dayOfWeek) &&
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
