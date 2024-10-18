using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;



public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeavesApproved(email)).RequireAuthorization();
		endpoints.MapGet("/leaves/pending/{email}", async (LeavesController controller, string email) => await controller.GetLeavesAwaitingApproval(email)).RequireAuthorization();
		endpoints.MapGet("/leaves/rejected/{email}", async (LeavesController controller, string email) => await controller.GetLeavesRejected(email)).RequireAuthorization();
		endpoints.MapPost("/leaves", async (LeavesController controller, LeaveCreateDTO model) => await controller.CreateLeave(model)).RequireAuthorization();
		endpoints.MapPut("/leaves/{leaveId}", async (LeavesController controller, int leaveId, LeaveUpdateDTO leaveUpdate) => await controller.UpdateLeave(leaveId, leaveUpdate)).RequireAuthorization();
		endpoints.MapDelete("/leaves/{leaveId}", async (LeavesController controller, int leaveId) => await controller.DeleteLeave(leaveId)).RequireAuthorization();
	}
}
public class LeavesController
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;

	public LeavesController(LeavePlannerContext context, PaidTimeOffLeft paidTimeOffLeft, EmployeesController employeesController, LeavesService leavesService)
	{
		_context = context;
		_leavesService = leavesService;
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
		var leaveRequests = await _leavesService.GetLeavesDynamicInfo(leaves);
		return Results.Ok(leaveRequests);
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
		var leaveRequests = await _leavesService.GetLeaveRequests(email);

		return Results.Ok(leaveRequests);
	}
	public async Task<IResult> ValidateLeaveRequest(DateTime dateStart, DateTime dateEnd, string owner, int? leaveId)
	{
		var validationResult = await _leavesService.ValidateLeave(dateStart, dateEnd, owner, leaveId);
		if (validationResult != "success")
		{
			return Results.BadRequest(validationResult);
		}
		var employee = await _context.Employees.FindAsync(owner);
		if (employee == null)
		{
			return Results.NotFound("Employee not found.");
		}
		Leave leaveRequest = new Leave
		{
			DateStart = dateStart,
			DateEnd = dateEnd,
			Type = "paidTimeOff",
			Owner = owner,
			OwnerNavigation = employee,
		};
		var leave = await _leavesService.GetLeaveDynamicInfo(leaveRequest);
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

			_context.Leaves.Update(leave);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
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
			return Results.Ok(leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);

		}

	}

}
