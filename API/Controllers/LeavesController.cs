using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;



public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeaves(email)).RequireAuthorization();
		endpoints.MapGet("/leaves/pending/{email}", async (LeavesController controller, string email) => await controller.GetLeavesAwaitingApproval(email)).RequireAuthorization();
		endpoints.MapPost("/leaves", async (LeavesController controller, LeaveCreateDTO model) => await controller.CreateLeave(model)).RequireAuthorization();
		endpoints.MapPut("/leaves/{leaveId}", async (LeavesController controller, int leaveId, LeaveUpdateDTO leaveUpdate) => await controller.UpdateLeave(leaveId, leaveUpdate)).RequireAuthorization();
		endpoints.MapDelete("/leaves/{leaveId}", async (LeavesController controller, int leaveId) => await controller.DeleteLeave(leaveId)).RequireAuthorization();
	}
}
public class LeavesController
{
	private readonly LeavePlannerContext _context;
	private readonly PaidTimeOffLeft _paidTimeOffLeft;

	public LeavesController(LeavePlannerContext context, PaidTimeOffLeft paidTimeOffLeft)
	{
		_context = context;
		_paidTimeOffLeft = paidTimeOffLeft;
	}
	public async Task<IResult> GetLeaves(string email)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   (leave.ApprovedBy != null || leave.Type == "bankHoliday"))
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.NotFound("No leaves found for this employee.");
		}

		// Return the leaves
		return Results.Ok(leaves);
	}
	public async Task<IResult> GetLeavesAwaitingApproval(string email)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.NotFound("No leaves found for this employee.");
		}

		// Return the leaves
		return Results.Ok(leaves);
	}
	public async Task<IResult> CreateLeave(LeaveCreateDTO model)
	{
		// Check if the requested dates are in the past
		if (model.DateStart < DateTime.UtcNow || model.DateEnd < DateTime.UtcNow)
		{
			return Results.BadRequest("You cannot request leave for dates in the past.");
		}

		// Check if the end date is before the start date
		if (model.DateEnd < model.DateStart)
		{
			return Results.BadRequest("The end date cannot be before the start date.");
		}

		var employee = await _context.Employees
									 .Include(e => e.LeaveOwnerNavigations)
									 .FirstOrDefaultAsync(e => e.Email == model.Owner);

		if (employee == null)
		{
			return Results.NotFound("Employee not found.");
		}

		var paidTimeOffLeft = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, DateTime.UtcNow.Year);
		var totalDaysRequested = (model.DateEnd - model.DateStart).TotalDays + 1;

		if (totalDaysRequested > employee.PaidTimeOff - paidTimeOffLeft)
		{
			return Results.BadRequest("You cannot request more days than you have left.");
		}

		// Check if the employee already has a leave on the requested days
		var conflictingLeaves = await _context.Leaves
											  .Where(leave => leave.Owner == model.Owner &&
															  ((model.DateStart >= leave.DateStart && model.DateStart <= leave.DateEnd) ||
															   (model.DateEnd >= leave.DateStart && model.DateEnd <= leave.DateEnd)))
											  .ToListAsync();

		if (conflictingLeaves.Any())
		{
			return Results.BadRequest("You cannot request leave for the same day(s) you already have.");
		}

		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			Leave leave = new Leave
			{
				Description = model.Description,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Type = model.Type,
				Owner = model.Owner
			};

			// Add employee to context
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
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var leave = await _context.Leaves.FindAsync(leaveId);
			if (leave == null)
			{
				return Results.NotFound("Leave not found with that id");
			}

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
