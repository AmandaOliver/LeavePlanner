using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class PaginatedLeavesResult
{
	public int TotalCount { get; set; }
	public List<Leave>? Leaves { get; set; }
}
public class LeavesService
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly string _leavePlannerUrl;

	public LeavesService(LeavePlannerContext context, EmployeesService employeesService, EmailService emailService, IConfiguration configuration)
	{
		_context = context;
		_employeesService = employeesService;
		_emailService = emailService;
		_configuration = configuration;
		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

	}

	public async Task<(bool IsSuccess, string? ErrorMessage, LeaveDTO? LeaveWithDynamicInfo)> GetLeaveInfo(string id)
	{
		var leave = await _context.Leaves.FindAsync(int.Parse(id));
		if (leave == null)
		{
			return (false, "leave not found", null);
		}
		var leaveWithDynamicInfo = await GetLeaveDynamicInfo(leave);
		return (true, null, leaveWithDynamicInfo);
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, List<LeaveDTO>? leaves)> GetMyLeaves(string employeeId, string? start, string? end)
	{
		if (start != null && end != null)
		{
			var employee = await _context.Employees.FindAsync(int.Parse(employeeId));
			if (employee == null)
			{
				return (false, "employee not found", null);
			}
			var leaves = await _context.Leaves
							   .Where(leave => leave.Owner == int.Parse(employeeId) &&
											   (leave.RejectedBy == null))
							   .ToListAsync();
			if (leaves.IsNullOrEmpty() || leaves == null || leaves.Count == 0)
			{
				return (true, null, new List<LeaveDTO>());
			}

			var leavesWithinRange = leaves.Where(leave =>
				leave.DateEnd >= DateTime.Parse(start) && leave.DateStart <= DateTime.Parse(end)
			).ToList();
			var leaveDTOs = new List<LeaveDTO>();

			foreach (var leave in leavesWithinRange)
			{

				leaveDTOs.Add(new LeaveDTO
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
				});
			}
			return (true, null, leaveDTOs);
		}
		else
		{
			return (false, "You need to specify start and end", null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, List<LeaveDTO>? leaves)> GetMyCircleLeaves(string employeeId, string? start, string? end)
	{
		if (start != null && end != null)
		{
			var employee = await _context.Employees.FindAsync(int.Parse(employeeId));
			if (employee == null)
			{
				return (false, "employee not found", null);
			}
			var allLeaves = new List<Leave>();
			var manager = await _context.Employees.FindAsync(employee.ManagedBy);

			if (manager == null)
			{
				// it's head, we don't check teammates
				var employeeLeaves = await _context.Leaves
							   .Where(leave => leave.Owner == int.Parse(employeeId) &&
											   (leave.ApprovedBy != null))
							   .ToListAsync();
				allLeaves.AddRange(employeeLeaves);
			}
			else
			{
				// get leaves from the manager
				var managerLeaves = await _context.Leaves
					   .Where(leave => leave.Owner == employee.ManagedBy &&
									   (leave.ApprovedBy != null))
					   .ToListAsync();
				allLeaves.AddRange(managerLeaves);
				// has a team, get leaves from the team
				var managerWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);

				foreach (var subordinate in managerWithSubordinates.Subordinates)
				{
					var subordinateLeaves = await _context.Leaves
								   .Where(leave => leave.Owner == subordinate.Id &&
												   (leave.ApprovedBy != null))
								   .ToListAsync();
					allLeaves.AddRange(subordinateLeaves);
				}

			}

			// get leaves from subordinates
			var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(employee);
			if (employeeWithSubordinates == null)
			{
				return (false, "employee with subordinates not found", null);
			}
			foreach (var subordinate in employeeWithSubordinates.Subordinates)
			{
				var subordinateLeaves = await _context.Leaves
							   .Where(leave => leave.Owner == subordinate.Id &&
											   (leave.ApprovedBy != null))
							   .ToListAsync();
				allLeaves.AddRange(subordinateLeaves);
			}
			if (allLeaves == null || allLeaves.Count == 0)
			{
				return (true, null, new List<LeaveDTO>());
			}

			var leavesWithinRange = allLeaves.Where(leave =>
				leave.DateEnd >= DateTime.Parse(start) && leave.DateStart <= DateTime.Parse(end)
			).ToList();

			var leaveDTOs = new List<LeaveDTO>();

			foreach (var leave in leavesWithinRange)
			{
				var leaveOwner = await _context.Employees.FindAsync(leave.Owner);
				if (leaveOwner == null)
				{
					return (false, "error getting owner", null);
				}
				leaveDTOs.Add(new LeaveDTO
				{
					Id = leave.Id,
					Type = leave.Type,
					Owner = leave.Owner,
					OwnerName = leaveOwner.Name,
					DateStart = leave.DateStart,
					DateEnd = leave.DateEnd,
					Description = leave.Description,
					ApprovedBy = leave.ApprovedBy,
					RejectedBy = leave.RejectedBy,
				});
			}
			return (true, null, leaveDTOs);
		}
		else
		{
			return (false, "You need to specify start and end", null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, List<LeaveDTO>? leaves)> GetAllLeaves(string organizationId, string? start, string? end)
	{

		var leaves = await _context.Leaves
						   .Where(leave => leave.ApprovedBy != null && leave.OwnerNavigation.Organization == int.Parse(organizationId))
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return (true, null, new List<LeaveDTO>());
		}

		if (start != null && end != null)
		{

			var leavesWithinRange = leaves.Where(leave =>
				leave.DateEnd >= DateTime.Parse(start) && leave.DateStart <= DateTime.Parse(end)
			).ToList();
			var leaveDTOs = new List<LeaveDTO>();

			foreach (var leave in leavesWithinRange)
			{
				var employee = await _context.Employees.FindAsync(leave.Owner);
				if (employee == null)
				{
					return (false, "employee not found", null);
				}
				leaveDTOs.Add(new LeaveDTO
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
				});
			}
			return (true, null, leaveDTOs);
		}
		else
		{
			return (false, "You need to specify start and end", null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedLeavesResult? leaves)> GetLeavesApproved(string id, int page, int pageSize)
	{
		var leaves = await _context.Leaves
						   	.Where(leave => leave.Owner == int.Parse(id) &&
									leave.ApprovedBy != null &&
									leave.DateStart >= DateTime.UtcNow)
							.OrderBy(leave => leave.DateStart)
						   	.ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return (true, null, new PaginatedLeavesResult
			{
				TotalCount = 0,
				Leaves = new List<Leave>()
			});
		}

		var paginatedLeaves = leaves
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return (true, null, new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = paginatedLeaves
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedLeavesResult? leaves)> GetPastLeaves(string id, int page, int pageSize)
	{
		var leaves = await _context.Leaves
						   	.Where(leave => leave.Owner == int.Parse(id) &&
									leave.ApprovedBy != null &&
									leave.DateStart < DateTime.UtcNow)
							.OrderByDescending(leave => leave.DateStart)
						   	.ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return (true, null, new PaginatedLeavesResult
			{
				TotalCount = 0,
				Leaves = new List<Leave>()
			});
		}

		// Apply pagination
		var paginatedLeaves = leaves
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return (true, null, new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = paginatedLeaves
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedLeavesResult? leaves)> GetLeavesRejected(string id, int page, int pageSize)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == int.Parse(id) &&
										   leave.RejectedBy != null)
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return (true, null, new PaginatedLeavesResult
			{
				TotalCount = 0,
				Leaves = new List<Leave>()
			});
		}
		var paginatedLeaves = leaves
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return (true, null, new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = paginatedLeaves
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Leave? leave)> CreateLeave(string employeeId, LeaveCreateDTO model)
	{
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var employee = await _context.Employees.FindAsync(int.Parse(employeeId));

			if (employee == null)
			{
				return (false, "Employee not found.", null);
			}
			var validationResult = await ValidateLeave(model.DateStart, model.DateEnd, int.Parse(employeeId), null, model.Type);
			if (validationResult != "success")
			{
				return (false, validationResult, null);
			}


			Leave leave = new Leave
			{
				Description = model.Description,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Type = model.Type,
				Owner = int.Parse(employeeId),
				OwnerNavigation = employee,
				CreatedAt = DateTime.UtcNow
			};

			if (employee.ManagedBy == null)
			{
				leave.ApprovedBy = int.Parse(employeeId);
			}
			_context.Leaves.Add(leave);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(employee.ManagedBy);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await GetLeaveDynamicInfo(leave, true);
					if (leaveWithDynamicInfo != null)
					{
						var conflictsInfo = "There are no other team members on leave during this time.";
						if (leaveWithDynamicInfo.Conflicts != null && !leaveWithDynamicInfo.Conflicts.IsNullOrEmpty())
						{
							conflictsInfo = "This leave conflicts with other team members: \n";

							foreach (var conflict in leaveWithDynamicInfo.Conflicts)
							{
								conflictsInfo += $"\n\t- {conflict.EmployeeName}:\n";
								foreach (var conflictingLeave in conflict.ConflictingLeaves)
								{
									conflictsInfo += $"\t\tStart Date: {conflictingLeave.DateStart.ToShortDateString()}, End Date: {conflictingLeave.DateEnd.ToShortDateString()}\n";
									conflictsInfo += $"\t\tDescription: {conflictingLeave.Description}\n";
								}
							}
						}
						var managerEmployee = await _context.Employees.FindAsync(employee.ManagedBy);

						string emailBody = $@"
Hello {manager.Name}, 
	You have a new leave request from {employee.Name}.
	Number of days requested: {leaveWithDynamicInfo.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{conflictsInfo}
	To review go to {_leavePlannerUrl}/requests/{manager.Email}

						";
						await _emailService.SendEmail(manager.Email, $"New Leave Request from {employee.Name}", emailBody);
					}

				}

			}
			return (true, null, leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Leave? leave)> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate)
	{
		var validationResult = await ValidateLeave(leaveUpdate.DateStart, leaveUpdate.DateEnd, leaveUpdate.Owner, leaveUpdate.Id, leaveUpdate.Type);
		if (validationResult != "success")
		{
			return (false, validationResult, null);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		var leave = await _context.Leaves.FindAsync(leaveId);
		if (leave == null)
		{
			return (false, "Leave not found with that Id", null);

		}
		try
		{

			leave.Description = leaveUpdate.Description;
			leave.DateStart = leaveUpdate.DateStart;
			leave.DateEnd = leaveUpdate.DateEnd;
			leave.CreatedAt = DateTime.UtcNow;
			leave.ApprovedBy = null;
			leave.RejectedBy = null;

			_context.Leaves.Update(leave);
			await _context.SaveChangesAsync();
			var employee = await _context.Employees.FindAsync(leave.Owner);

			if (employee == null)
			{
				return (false, "Employee not found.", null);
			}
			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(employee.ManagedBy);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await GetLeaveDynamicInfo(leave, true);
					if (leaveWithDynamicInfo != null)
					{
						var conflictsInfo = "There are no other team members on leave during this time.";
						if (leaveWithDynamicInfo.Conflicts != null && !leaveWithDynamicInfo.Conflicts.IsNullOrEmpty())
						{
							conflictsInfo = "This leave conflicts with other team members: \n";

							foreach (var conflict in leaveWithDynamicInfo.Conflicts)
							{
								conflictsInfo += $"\n\t- {conflict.EmployeeName}:\n";
								foreach (var conflictingLeave in conflict.ConflictingLeaves)
								{
									conflictsInfo += $"\t\tStart Date: {conflictingLeave.DateStart.ToShortDateString()}, End Date: {conflictingLeave.DateEnd.ToShortDateString()}\n";
									conflictsInfo += $"\t\tDescription: {conflictingLeave.Description}\n";
								}
							}
						}
						string emailBody = $@"
Hello {manager.Name}, 
	{employee.Name} has updated an existing leave request.
	Number of days requested: {leaveWithDynamicInfo.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{conflictsInfo}
	To review go to {_leavePlannerUrl ?? "https://localhost:3000"}/requests/{manager.Email}";
						await _emailService.SendEmail(manager.Email, $"Leave Request Updated by {employee.Name}", emailBody);
					}

				}
			}
			else
			{
				leave.ApprovedBy = 1;

			}
			_context.Leaves.Update(leave);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			return (true, null, leave);

		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Leave? leave)> DeleteLeave(int leaveId)
	{
		var leave = await _context.Leaves.FindAsync(leaveId);
		if (leave == null)
		{
			return (false, "Leave not found", null);
		}

		if (leave.ApprovedBy != null && (leave.DateStart < DateTime.UtcNow || leave.DateEnd < DateTime.UtcNow))
		{
			return (false, "You cannot delete leaves in the past.", null);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			_context.Leaves.Remove(leave);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			var employee = await _context.Employees.FindAsync(leave.Owner);

			if (employee == null)
			{
				return (false, "Employee not found.", null);
			}
			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(employee.ManagedBy);
				if (manager != null)
				{
					string emailBody = $@"
Hello {manager.Name}, 
	{employee.Name} has deleted a leave request.

	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	Description: {leave.Description}						
";
					await _emailService.SendEmail(manager.Email, $"Leave Request Deleted by {employee.Name}", emailBody);
				}
			}
			return (true, null, leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedLeavesResult? leaves)> GetLeavesPending(string id, int page, int pageSize)
	{
		var leaves = await _context.Leaves
					   .Where(leave => leave.Owner == int.Parse(id) &&
									   leave.ApprovedBy == null && leave.RejectedBy == null)
					   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return (true, null, new PaginatedLeavesResult
			{
				TotalCount = 0,
				Leaves = new List<Leave>()
			});
		}

		// Apply pagination
		var paginatedLeaves = leaves
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return (true, null, new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = paginatedLeaves
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, LeaveDTO? leave)> ValidateLeaveRequest(string employeeId, LeaveValidateDTO leaveToValidate)
	{
		var employee = await _context.Employees.FindAsync(int.Parse(employeeId));
		if (employee == null)
		{
			return (false, "Employee not found.", null);
		}
		var validationResult = await ValidateLeave(leaveToValidate.DateStart, leaveToValidate.DateEnd, int.Parse(employeeId), leaveToValidate.Id, leaveToValidate.Type);
		if (validationResult != "success")
		{
			return (false, validationResult, null);
		}

		Leave leaveRequest = new Leave
		{
			Id = leaveToValidate.Id ?? 0,
			DateStart = leaveToValidate.DateStart,
			DateEnd = leaveToValidate.DateEnd,
			Type = leaveToValidate.Type,
			Owner = int.Parse(employeeId),
			OwnerNavigation = employee,
		};
		var leave = await GetLeaveDynamicInfo(leaveRequest);
		return (true, null, leave);

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