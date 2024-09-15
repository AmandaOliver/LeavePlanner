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
		var employee = await _context.Employees
								  .FindAsync(employeeEmail);
		if (employee == null)
		{
			throw new Exception("Employee not found.");
		}
		var leavesThisYear = await _context.Leaves
			.Where(l => l.Owner == employeeEmail && l.Id != leaveId && l.Type == "paidTimeOff" && l.ApprovedBy != null && l.DateStart.Year == year)
			.ToListAsync();

		int totalDaysTaken = leavesThisYear.Sum(leave => (leave.DateEnd - leave.DateStart).Days + 1);
		return employee.PaidTimeOff - totalDaysTaken;
	}
}