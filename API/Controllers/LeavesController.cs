using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;



public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeaves(email)).RequireAuthorization();
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

		return Results.Ok(leaves);
	}
	public async Task<IResult> GetLeavesRejected(string email)
	{
		var leaves = await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   leave.RejectedBy != null && leave.Type != "bankHoliday")
						   .ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.NotFound("No leaves found for this employee.");
		}

		return Results.Ok(leaves);
	}
	public async Task<IResult> GetLeavesAwaitingApproval(string email)
	{
		var leaves = await GetLeaveRequests(email);

		if (leaves == null || leaves.Count == 0)
		{
			return Results.NotFound("No leaves found for this employee.");
		}

		return Results.Ok(leaves);
	}
	public async Task<List<Leave>> GetLeaveRequests(string email)
	{
		return await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
						   .ToListAsync();
	}
	private async Task<IResult> ValidateLeave(DateTime dateStart, DateTime dateEnd, string owner, int? leaveId)
	{
		// Leave validation checks
		if (leaveId != null)
		{
			var leave = await _context.Leaves.FindAsync(leaveId);
			if (leave == null)
			{
				return Results.NotFound("Leave not found.");
			}
			if (leave.Type == "bankHoliday")
			{
				return Results.BadRequest("You cannot update bank holidays.");
			}
			if (leave.RejectedBy != null)
			{
				return Results.BadRequest("You cannot update a rejected leave.");
			}
		}

		// Date validation checks
		if (dateStart < DateTime.UtcNow.Date || dateEnd < DateTime.UtcNow.Date)
		{
			return Results.BadRequest("You cannot request leave for dates in the past.");
		}

		if (dateEnd < dateStart)
		{
			return Results.BadRequest("The end date cannot be before the start date.");
		}

		var employee = await _context.Employees.FindAsync(owner);
		if (employee == null)
		{
			return Results.NotFound("Employee not found.");
		}

		// Exclude weekends when calculating days off
		int totalWeekdaysRequested = _paidTimeOffLeft.GetWeekdaysBetween(dateStart, dateEnd.AddDays(-1));  // Adjusted to exclude dateEnd

		// If leave crosses over into the next year
		if (dateStart.Year != dateEnd.Year)
		{
			var endOfYear = new DateTime(dateStart.Year, 12, 31);
			var daysInCurrentYear = _paidTimeOffLeft.GetWeekdaysBetween(dateStart, endOfYear);
			var startOfNextYear = new DateTime(dateEnd.Year, 1, 1);
			var daysInNextYear = _paidTimeOffLeft.GetWeekdaysBetween(startOfNextYear, dateEnd.AddDays(-1));

			// Check for enough paid time off in current year
			var paidTimeOffLeftForCurrentYear = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateStart.Year, leaveId);
			if (daysInCurrentYear > paidTimeOffLeftForCurrentYear)
			{
				return Results.BadRequest($"You cannot request more days than you have left for the year {dateStart.Year}.");
			}

			// Check for enough paid time off in next year
			var paidTimeOffLeftForNextYear = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateEnd.Year, leaveId);
			if (daysInNextYear > paidTimeOffLeftForNextYear)
			{
				return Results.BadRequest($"You cannot request more days than you have left for the year {dateEnd.Year}.");
			}
		}
		else
		{
			var paidTimeOffLeft = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, dateStart.Year, leaveId);

			if (totalWeekdaysRequested > paidTimeOffLeft)
			{
				return Results.BadRequest("You cannot request more days than you have left.");
			}
		}

		// Check for conflicting leaves
		var conflictingLeaves = await _context.Leaves
			.Where(leave => leave.Owner == owner && leave.Id != leaveId &&
							((dateStart >= leave.DateStart && dateStart < leave.DateEnd.AddDays(-1)) ||
							 (dateEnd >= leave.DateStart && dateEnd < leave.DateEnd.AddDays(-1))))
			.ToListAsync();

		if (conflictingLeaves.Any())
		{
			return Results.BadRequest("You cannot request leave for the same day(s) you already have requested.");
		}

		return Results.Ok();
	}



	public async Task<IResult> CreateLeave(LeaveCreateDTO model)
	{
		var validationResult = await ValidateLeave(model.DateStart, model.DateEnd, model.Owner, null);
		if (!validationResult.Equals(Results.Ok()))
		{
			return validationResult;
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
				OwnerNavigation = employee
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
		var validationResult = await ValidateLeave(leaveUpdate.DateStart, leaveUpdate.DateEnd, leaveUpdate.Owner, leaveUpdate.Id);
		if (!validationResult.Equals(Results.Ok()))
		{
			return validationResult;
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
