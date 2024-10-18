using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
public interface ILeavesService
{
	Task<List<LeaveDTO>> GetLeaveRequests(string email);
	Task<LeaveDTO> GetLeaveDynamicInfo(Leave leave, bool withConflicts);
	Task<List<LeaveDTO>> GetLeavesDynamicInfo(List<Leave> leaves, bool withConflicts);
	Task<string> ValidateLeave(DateTime dateStart, DateTime dateEnd, string owner, int? leaveId);
	Task<List<ConflictDTO>> GetConflicts(Leave leaveRequest);
}

public class LeavesService : ILeavesService
{
	private readonly LeavePlannerContext _context;
	private readonly PaidTimeOffLeft _paidTimeOffLeft;
	private readonly EmployeesController _employeesController;

	public LeavesService(LeavePlannerContext context, PaidTimeOffLeft paidTimeOffLeft, EmployeesController employeesController)
	{
		_context = context;
		_paidTimeOffLeft = paidTimeOffLeft;
		_employeesController = employeesController;
	}
	public async Task<List<LeaveDTO>> GetLeaveRequests(string email)
	{
		var leaves = await _context.Leaves
					   .Where(leave => leave.Owner == email &&
									   leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
					   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return new List<LeaveDTO>();
		}
		var leaveRequests = await GetLeavesDynamicInfo(leaves, true);

		return leaveRequests;
	}
	public async Task<LeaveDTO> GetLeaveDynamicInfo(Leave leave, bool withConflicts = false)
	{
		int requestedDaysThisYear = await _paidTimeOffLeft.GetDaysRequested(leave.DateStart, leave.DateEnd, leave.Owner, DateTime.UtcNow.Year, leave.Id);
		int requestedDaysNextYear = await _paidTimeOffLeft.GetDaysRequested(leave.DateStart, leave.DateEnd, leave.Owner, DateTime.UtcNow.Year + 1, leave.Id);
		var employee = await _context.Employees.FindAsync(leave.Owner);
		if (employee == null)
		{
			throw new Exception("employee not found");
		}
		LeaveDTO leaveWithDynamicInfo = new LeaveDTO
		{
			Id = leave.Id,
			Type = leave.Type,
			Owner = leave.Owner,
			OwnerName = employee.Name,
			DateStart = leave.DateStart,
			DateEnd = leave.DateEnd,
			Description = leave.Description,
			ApprovedBy = leave.ApprovedBy,
			RejectedBy = leave.RejectedBy,
			DaysRequested = requestedDaysThisYear + requestedDaysNextYear,
		};
		if (withConflicts == true)
		{
			List<ConflictDTO> conflicts = await GetConflicts(leave);
			leaveWithDynamicInfo.Conflicts = conflicts;
		}
		return leaveWithDynamicInfo;
	}
	public async Task<List<LeaveDTO>> GetLeavesDynamicInfo(List<Leave> leaves, bool withConflicts = false)
	{
		var leaveDTOs = new List<LeaveDTO>();
		foreach (var leave in leaves)
		{
			leaveDTOs.Add(await GetLeaveDynamicInfo(leave, withConflicts));
		}

		return leaveDTOs;
	}
	public async Task<string> ValidateLeave(DateTime dateStart, DateTime dateEnd, string owner, int? leaveId)
	{
		// Leave update validation checks
		if (leaveId != null)
		{
			var leave = await _context.Leaves.FindAsync(leaveId);
			if (leave == null)
			{
				return "Leave not found.";
			}
			if (leave.Type == "bankHoliday")
			{
				return "You cannot update bank holidays.";
			}
			if (leave.RejectedBy != null)
			{
				return "You cannot update a rejected leave.";
			}
		}

		// Date validation checks
		if (dateStart < DateTime.UtcNow.Date || dateEnd < DateTime.UtcNow.Date)
		{
			return "You cannot request leave for dates in the past.";
		}

		if (dateEnd < dateStart)
		{
			return "The end date cannot be before the start date.";
		}

		var employee = await _context.Employees.FindAsync(owner);
		if (employee == null)
		{
			return "Employee not found.";
		}



		// If leave crosses over into the next year
		if (dateStart.Year != dateEnd.Year)
		{
			var endOfYear = new DateTime(dateStart.Year + 1, 1, 1);
			var daysInCurrentYear = await _paidTimeOffLeft.GetDaysRequested(dateStart, endOfYear, owner, dateStart.Year, leaveId);
			var startOfNextYear = new DateTime(dateEnd.Year, 1, 1);
			var daysInNextYear = await _paidTimeOffLeft.GetDaysRequested(startOfNextYear, dateEnd, owner, dateEnd.Year, leaveId);

			// Check for enough paid time off in current year
			var paidTimeOffLeftForCurrentYear = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateStart.Year, leaveId);
			if (daysInCurrentYear > paidTimeOffLeftForCurrentYear)
			{
				return $"You cannot request more days than you have left for the year {dateStart.Year}.";
			}

			// Check for enough paid time off in next year
			var paidTimeOffLeftForNextYear = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateEnd.Year, leaveId);
			if (daysInNextYear > paidTimeOffLeftForNextYear)
			{
				return $"You cannot request more days than you have left for the year {dateEnd.Year}.";
			}
		}
		else
		{
			int totalWeekdaysRequested = await _paidTimeOffLeft.GetDaysRequested(dateStart, dateEnd, owner, dateStart.Year, leaveId);
			var paidTimeOffLeft = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateStart.Year, leaveId);

			if (totalWeekdaysRequested > paidTimeOffLeft)
			{
				return "You cannot request more days than you have left.";
			}
		}

		return "success";
	}
	public async Task<List<ConflictDTO>> GetConflicts(Leave leaveRequest)
	{

		var employee = await _context.Employees.FindAsync(leaveRequest.Owner);
		if (employee == null)
		{
			throw new Exception("Employee not found");
		}
		if (employee.ManagedBy == null)
		{
			return new List<ConflictDTO>(); // head of org doesn't have conflicts
		}
		var employeeWithSubordinates = await _employeesController.GetEmployeeWithSubordinates(employee.ManagedBy);
		var conflicts = new List<ConflictDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			// do not take in account leaves of the same employee
			if (subordinate.Email != leaveRequest.Owner)
			{
				var conflictingLeaves = await _context.Leaves
					.Where(leave =>
						leave.Owner == subordinate.Email && // is a leave of this employee
						leaveRequest.Id != leave.Id && // do not take in account leaves of the same employee
						(
							(leaveRequest.DateStart >= leave.DateStart && leaveRequest.DateStart < leave.DateEnd) ||   // Start date is within an existing leave (excluding an exact match on end date)
							(leaveRequest.DateEnd > leave.DateStart && leaveRequest.DateEnd <= leave.DateEnd) ||       // End date is within an existing leave (excluding an exact match on start date)
							(leaveRequest.DateStart < leave.DateStart && leaveRequest.DateEnd > leave.DateEnd)         // The requested leave fully contains an existing leave
						))
					.ToListAsync();
				if (!conflictingLeaves.IsNullOrEmpty())
				{
					conflicts.Add(new ConflictDTO
					{
						EmployeeEmail = subordinate.Email,
						EmployeeName = subordinate.Name,
						ConflictingLeaves = conflictingLeaves
					});
				}
			}
		}
		return conflicts;
	}
}