using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Prng;
using Microsoft.AspNetCore.Mvc;



public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/leave/{id}", async (LeavesController controller, string id) => await controller.GetLeaveInfo(id)).RequireAuthorization();
		endpoints.MapGet("/myleaves/{email}", async (LeavesController controller, string email, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetMyLeaves(email, start, end)).RequireAuthorization();
		endpoints.MapGet("/mycircleleaves/{email}", async (LeavesController controller, string email, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetMyCircleLeaves(email, start, end)).RequireAuthorization();
		endpoints.MapGet("/allleaves", async (LeavesController controller, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetAllLeaves(start, end)).RequireAuthorization();
		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeavesApproved(email)).RequireAuthorization();
		endpoints.MapGet("/leaves/pending/{email}", async (LeavesController controller, string email) => await controller.GetLeavesAwaitingApproval(email)).RequireAuthorization();
		endpoints.MapGet("/leaves/rejected/{email}", async (LeavesController controller, string email) => await controller.GetLeavesRejected(email)).RequireAuthorization();
		endpoints.MapPost("/leaves/validate", async (LeavesController controller, LeaveValidateDTO model) => await controller.ValidateLeaveRequest(model)).RequireAuthorization();
		endpoints.MapPost("/leaves", async (LeavesController controller, LeaveCreateDTO model) => await controller.CreateLeave(model)).RequireAuthorization();
		endpoints.MapPut("/leaves/{leaveId}", async (LeavesController controller, int leaveId, LeaveUpdateDTO leaveUpdate) => await controller.UpdateLeave(leaveId, leaveUpdate)).RequireAuthorization();
		endpoints.MapDelete("/leaves/{leaveId}", async (LeavesController controller, int leaveId) => await controller.DeleteLeave(leaveId)).RequireAuthorization();
	}
}
public class LeavesController
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly EmployeesController _employeesController;

	private readonly string _leavePlannerUrl;


	public LeavesController(LeavePlannerContext context, LeavesService leavesService, EmailService emailService, IConfiguration configuration, EmployeesController employeesController)
	{
		_context = context;
		_leavesService = leavesService;
		_emailService = emailService;
		_configuration = configuration;
		_employeesController = employeesController;

		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

	}
	public async Task<IResult> GetLeaveInfo(string id)
	{
		var leave = await _context.Leaves.FindAsync(int.Parse(id));
		if (leave == null)
		{
			return Results.NotFound("leave not found");
		}
		var leaveWithDynamicInfo = await _leavesService.GetLeaveDynamicInfo(leave, true);
		return Results.Ok(leaveWithDynamicInfo);
	}
	public async Task<IResult> GetMyLeaves(string email, [FromQuery] string? start, [FromQuery] string? end)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   (leave.RejectedBy == null))
						   .ToListAsync();
		var employee = await _context.Employees.FindAsync(leaves[0].Owner);
		if (employee == null)
		{
			return Results.BadRequest("employee not found");
		}
		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<Leave>());
		}

		if (start != null && end != null)
		{

			var leavesWithinRange = leaves.Where(leave =>
				{
					return leave.DateStart > DateTime.Parse(start) && leave.DateEnd <= DateTime.Parse(end);

				}).ToList();
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
			return Results.Ok(leaveDTOs);
		}
		else
		{
			return Results.BadRequest("You need to specify start and end");
		}
	}
	public async Task<IResult> GetMyCircleLeaves(string email, [FromQuery] string? start, [FromQuery] string? end)
	{
		if (start != null && end != null)
		{
			var employee = await _context.Employees.FindAsync(email);
			if (employee == null)
			{
				return Results.BadRequest("employee not found");
			}
			var allLeaves = new List<Leave>();
			var manager = await _context.Employees.FindAsync(employee.ManagedBy);

			if (manager == null)
			{
				// it's head, we don't check teammates
				var employeeLeaves = await _context.Leaves
							   .Where(leave => leave.Owner == email &&
											   (leave.Type == "bankHoliday" || leave.ApprovedBy != null))
							   .ToListAsync();
				allLeaves.AddRange(employeeLeaves);
			}
			else
			{
				// get leaves from the manager
				var managerLeaves = await _context.Leaves
					   .Where(leave => leave.Owner == employee.ManagedBy &&
									   (leave.Type == "bankHoliday" || leave.ApprovedBy != null))
					   .ToListAsync();
				allLeaves.AddRange(managerLeaves);
				// has a team, get leaves from the team
				var managerWithSubordinates = await _employeesController.GetEmployeeWithSubordinates(manager);

				foreach (var subordinate in managerWithSubordinates.Subordinates)
				{
					var subordinateLeaves = await _context.Leaves
								   .Where(leave => leave.Owner == subordinate.Email &&
												   (leave.Type == "bankHoliday" || leave.ApprovedBy != null))
								   .ToListAsync();
					allLeaves.AddRange(subordinateLeaves);
				}

			}

			// get leaves from subordinates
			var employeeWithSubordinates = await _employeesController.GetEmployeeWithSubordinates(employee);
			if (employeeWithSubordinates == null)
			{
				return Results.BadRequest("employee with subordinates not found");
			}
			foreach (var subordinate in employeeWithSubordinates.Subordinates)
			{
				var subordinateLeaves = await _context.Leaves
							   .Where(leave => leave.Owner == subordinate.Email &&
											   (leave.Type == "bankHoliday" || leave.ApprovedBy != null))
							   .ToListAsync();
				allLeaves.AddRange(subordinateLeaves);
			}
			if (allLeaves == null || allLeaves.Count == 0)
			{
				return Results.Ok(new List<Leave>());
			}

			var leavesWithinRange = allLeaves.Where(leave =>
				{
					return leave.DateStart > DateTime.Parse(start) && leave.DateEnd <= DateTime.Parse(end);

				}).ToList();
			var leaveDTOs = new List<LeaveDTO>();

			foreach (var leave in leavesWithinRange)
			{
				var leaveOwner = await _context.Employees.FindAsync(leave.Owner);
				if (leaveOwner == null)
				{
					return Results.BadRequest("error getting owner");
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
			return Results.Ok(leaveDTOs);
		}
		else
		{
			return Results.BadRequest("You need to specify start and end");
		}
	}
	public async Task<IResult> GetAllLeaves([FromQuery] string? start, [FromQuery] string? end)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Type == "bankHoliday" || leave.ApprovedBy != null)
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<Leave>());
		}

		if (start != null && end != null)
		{

			var leavesWithinRange = leaves.Where(leave =>
				{
					return leave.DateStart > DateTime.Parse(start) && leave.DateEnd <= DateTime.Parse(end);

				}).ToList();
			var leaveDTOs = new List<LeaveDTO>();

			foreach (var leave in leavesWithinRange)
			{
				var employee = await _context.Employees.FindAsync(leave.Owner);
				if (employee == null)
				{
					return Results.BadRequest("employee not found");
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
			return Results.Ok(leaveDTOs);
		}
		else
		{
			return Results.BadRequest("You need to specify start and end");
		}
	}

	public async Task<IResult> GetLeavesApproved(string email)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   (leave.ApprovedBy != null || leave.Type == "bankHoliday"))
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<Leave>());
		}

		var leavesWithinNext6Months = leaves.Where(leave =>
		{
			if (leave.Type == "bankHoliday")
			{
				return leave.DateStart < DateTime.UtcNow.Date.AddMonths(6);
			}
			return true;
		}).ToList();
		return Results.Ok(leavesWithinNext6Months);


	}
	public async Task<IResult> GetLeavesRejected(string email)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   leave.RejectedBy != null && leave.Type != "bankHoliday")
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<Leave>());
		}
		var leaveRequests = await _leavesService.GetLeavesDynamicInfo(leaves);

		return Results.Ok(leaveRequests);
	}
	public async Task<IResult> GetLeavesAwaitingApproval(string email)
	{
		var leaves = await _context.Leaves
					   .Where(leave => leave.Owner == email &&
									   leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
					   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<LeaveDTO>());
		}
		var leaveRequests = await _leavesService.GetLeavesDynamicInfo(leaves);

		return Results.Ok(leaveRequests);
	}
	public async Task<IResult> ValidateLeaveRequest(LeaveValidateDTO leaveToValidate)
	{
		var validationResult = await _leavesService.ValidateLeave(leaveToValidate.DateStart, leaveToValidate.DateEnd, leaveToValidate.Owner, leaveToValidate.Id);
		if (validationResult != "success")
		{
			return Results.BadRequest(validationResult);
		}
		var employee = await _context.Employees.FindAsync(leaveToValidate.Owner);
		if (employee == null)
		{
			return Results.NotFound("Employee not found.");
		}
		Leave leaveRequest = new Leave
		{
			Id = leaveToValidate.Id ?? 0,
			DateStart = leaveToValidate.DateStart,
			DateEnd = leaveToValidate.DateEnd,
			Type = "paidTimeOff",
			Owner = leaveToValidate.Owner,
			OwnerNavigation = employee,
		};
		var leave = await _leavesService.GetLeaveDynamicInfo(leaveRequest, true);
		return Results.Ok(leave);

	}
	public async Task<IResult> CreateLeave(LeaveCreateDTO model)
	{
		var validationResult = await _leavesService.ValidateLeave(model.DateStart, model.DateEnd, model.Owner, null);
		if (validationResult != "success")
		{
			return Results.BadRequest(validationResult);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var employee = await _context.Employees.FindAsync(model.Owner);

			if (employee == null)
			{
				return Results.NotFound("Employee not found.");
			}
			Leave leave = new Leave
			{
				Description = model.Description,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Type = model.Type,
				Owner = model.Owner,
				OwnerNavigation = employee,
				CreatedAt = DateTime.UtcNow
			};

			if (employee.ManagedBy == null)
			{
				leave.ApprovedBy = model.Owner;
			}
			_context.Leaves.Add(leave);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(employee.ManagedBy);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await _leavesService.GetLeaveDynamicInfo(leave, true);
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
	You have a new leave request from {employee.Name}.
	Number of days requested: {leaveWithDynamicInfo.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{conflictsInfo}
	To review go to {_leavePlannerUrl}/requests/{manager.Email}

						";
						await _emailService.SendEmail(employee.ManagedBy, $"New Leave Request from {employee.Name}", emailBody);
					}
				}

			}
			return Results.Ok(leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);
		}
	}
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate)
	{
		var validationResult = await _leavesService.ValidateLeave(leaveUpdate.DateStart, leaveUpdate.DateEnd, leaveUpdate.Owner, leaveUpdate.Id);
		if (validationResult != "success")
		{
			return Results.BadRequest(validationResult);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		var leave = await _context.Leaves.FindAsync(leaveId);
		if (leave == null)
		{
			return Results.NotFound("Leave not found with that Id");

		}
		try
		{

			leave.Description = leaveUpdate.Description;
			leave.DateStart = leaveUpdate.DateStart;
			leave.DateEnd = leaveUpdate.DateEnd;
			leave.Type = leaveUpdate.Type;
			leave.CreatedAt = DateTime.UtcNow;
			leave.ApprovedBy = null;
			leave.RejectedBy = null;

			_context.Leaves.Update(leave);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			var employee = await _context.Employees.FindAsync(leave.Owner);

			if (employee == null)
			{
				return Results.NotFound("Employee not found.");
			}
			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(employee.ManagedBy);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await _leavesService.GetLeaveDynamicInfo(leave, true);
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
	To review go to {_leavePlannerUrl}/requests/{manager.Email}
						";
						await _emailService.SendEmail(employee.ManagedBy, $"Leave Request Updated by {employee.Name}", emailBody);
					}
				}
			}
			return Results.Ok(leave);

		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);

		}

	}
	public async Task<IResult> DeleteLeave(int leaveId)
	{
		var leave = await _context.Leaves.FindAsync(leaveId);
		if (leave == null)
		{
			return Results.NotFound("Leave not found");
		}
		if (leave.Type == "bankHoliday")
		{
			return Results.BadRequest("You cannot delete bank holidays.");
		}
		if (leave.ApprovedBy != null && (leave.DateStart < DateTime.UtcNow || leave.DateEnd < DateTime.UtcNow))
		{
			return Results.BadRequest("You cannot delete leaves in the past.");
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
				return Results.NotFound("Employee not found.");
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
					await _emailService.SendEmail(employee.ManagedBy, $"Leave Request Deleted by {employee.Name}", emailBody);
				}
			}
			return Results.Ok(leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);

		}

	}

}
