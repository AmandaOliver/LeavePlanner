using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class LeavesService
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;

	public LeavesService(LeavePlannerContext context, EmployeesService employeesService)
	{
		_context = context;
		_employeesService = employeesService;
	}

	public static string DescribeConflicts(LeaveDTO leave)
	{
		if (leave.Conflicts == null || leave.Conflicts.IsNullOrEmpty())
		{
			return "There are no other team members on leave during this time.";
		}

		var description = "This leave conflicts with other team members: \n";
		foreach (var conflict in leave.Conflicts)
		{
			description += $"\n\t- {conflict.EmployeeName}:\n";
			foreach (var conflictingLeave in conflict.ConflictingLeaves)
			{
				description += $"\t\tStart Date: {conflictingLeave.DateStart.ToShortDateString()}, End Date: {conflictingLeave.DateEnd.ToShortDateString()}\n";
				description += $"\t\tDescription: {conflictingLeave.Description}\n";
			}
		}

		return description;
	}


	public async Task<List<LeaveDTO>> GetLeaveRequests(EmployeeWithSubordinatesDTO employee)
	{
		var leaves = await _context.Leaves
					   .Where(leave => leave.Owner == employee.Id &&
									   leave.ApprovedBy == null && leave.RejectedBy == null)
					   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return new List<LeaveDTO>();
		}
		var leaveRequests = await GetLeavesDynamicInfo(leaves);

		return leaveRequests;
	}
	public async Task<List<LeaveDTO>> GetReviewedRequests(EmployeeWithSubordinatesDTO employee)
	{
		var systemEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == "system");
		if (systemEmployee == null)
		{
			throw new Exception("System employee not found");
		}
		var leaves = await _context.Leaves
					   .Where(leave => leave.Owner == employee.Id &&
									   ((leave.ApprovedBy != null && leave.ApprovedBy != systemEmployee.Id) || leave.RejectedBy != null))
					   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return new List<LeaveDTO>();
		}
		var leaveRequests = await GetLeavesDynamicInfo(leaves);

		return leaveRequests;
	}
	public async Task<LeaveDTO> GetLeaveDynamicInfo(Leave leave, bool withConflicts = false)
	{

		var employee = await _context.Employees.FindAsync(leave.Owner);
		if (employee == null)
		{
			throw new Exception("employee not found");
		}
		int requestedDaysThisYear = await _employeesService.GetDaysRequested(leave.DateStart, leave.DateEnd, leave.Owner, DateTime.UtcNow.Year, leave.Id != 0 ? leave.Id : null);
		int requestedDaysNextYear = await _employeesService.GetDaysRequested(leave.DateStart, leave.DateEnd, leave.Owner, DateTime.UtcNow.Year + 1, leave.Id != 0 ? leave.Id : null);
		int leftDaysThisYear = await _employeesService.GetPaidTimeOffLeft(leave.Owner, leave.DateStart.Year, leave.Id != 0 ? leave.Id : null);
		int leftDaysNextYear = await _employeesService.GetPaidTimeOffLeft(leave.Owner, leave.DateStart.Year + 1, leave.Id != 0 ? leave.Id : null);
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
			DaysLeftThisYear = leftDaysThisYear - requestedDaysThisYear,
			DaysLeftNextYear = leftDaysNextYear - requestedDaysNextYear
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
	public async Task<string> ValidateLeave(DateTime dateStart, DateTime dateEnd, int owner, int? leaveId, string type)
	{
		// Leave update validation checks
		if (leaveId != null)
		{
			var leave = await _context.Leaves.FindAsync(leaveId);
			if (leave == null)
			{
				return "Leave not found.";
			}

			if (leave.ApprovedBy != null && dateStart < DateTime.UtcNow.Date)
			{
				return "You cannot update an already taken leave";
			}
			if (type == "bankHoliday")
			{
				var days = (dateEnd - dateStart).Days;
				if (days > 1)
				{
					return "You cannot request more than 1 day of public holidays";
				}
			}
		}
		else
		{
			if (type == "bankHoliday")
			{
				return "You can't request a new public holiday";
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


		if (type == "paidTimeOff")
		{
			// If leave crosses over into the next year
			if (dateStart.Year != dateEnd.Year)
			{
				var endOfYear = new DateTime(dateStart.Year + 1, 1, 1);
				var daysInCurrentYear = await _employeesService.GetDaysRequested(dateStart, endOfYear, owner, dateStart.Year, leaveId);
				var startOfNextYear = new DateTime(dateEnd.Year, 1, 1);
				var daysInNextYear = await _employeesService.GetDaysRequested(startOfNextYear, dateEnd, owner, dateEnd.Year, leaveId);

				// Check for enough paid time off in current year
				var paidTimeOffLeftForCurrentYear = await _employeesService.GetPaidTimeOffLeft(employee.Id, dateStart.Year, leaveId);
				if (daysInCurrentYear > paidTimeOffLeftForCurrentYear)
				{
					return $"You cannot request more days than you have left.\nDays requested: {daysInCurrentYear}.\nDays left for the year {dateStart.Year}: {paidTimeOffLeftForCurrentYear}.";
				}

				// Check for enough paid time off in next year
				var paidTimeOffLeftForNextYear = await _employeesService.GetPaidTimeOffLeft(employee.Id, dateEnd.Year, leaveId);
				if (daysInNextYear > paidTimeOffLeftForNextYear)
				{
					return $"You cannot request more days than you have left.\nDays requested: {daysInNextYear}.\nDays left for the year {dateEnd.Year}: {paidTimeOffLeftForNextYear}.";
				}
			}
			else
			{
				int totalWeekdaysRequested = await _employeesService.GetDaysRequested(dateStart, dateEnd, owner, dateStart.Year, leaveId);
				var paidTimeOffLeft = await _employeesService.GetPaidTimeOffLeft(employee.Id, dateStart.Year, leaveId);

				if (totalWeekdaysRequested > paidTimeOffLeft)
				{
					return $"You cannot request more days than you have left.\nDays requested: {totalWeekdaysRequested}.\nDays left for {dateStart.Year}: {paidTimeOffLeft}.";
				}
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
		var manager = await _context.Employees.FindAsync(employee.ManagedBy);
		if (manager == null)
		{
			throw new Exception("Manager not found");
		}
		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		var conflicts = new List<ConflictDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			// do not take in account leaves of the same employee
			if (subordinate.Id != leaveRequest.Owner)
			{
				var conflictingLeaves = await _context.Leaves
					.Where(leave =>
						leave.Owner == subordinate.Id && // is a leave of this employee
						(leave.ApprovedBy != null) &&
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
						EmployeeId = subordinate.Id,
						EmployeeName = subordinate.Name,
						ConflictingLeaves = conflictingLeaves
					});
				}
			}
		}
		return conflicts;
	}
}