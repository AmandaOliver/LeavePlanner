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
			return Results.Ok(new List<Leave>());
		}
		var leavesWithDaysRequested = await GetLeavesWithDaysRequested(leaves, email);
		return Results.Ok(leavesWithDaysRequested);
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
		var leavesWithDaysRequested = await GetLeavesWithDaysRequested(leaves, email);

		return Results.Ok(leavesWithDaysRequested);
	}
	public async Task<IResult> GetLeavesAwaitingApproval(string email)
	{
		var leaves = await GetLeaveRequests(email);

		if (leaves == null || leaves.Count == 0)
		{
			return Results.Ok(new List<Leave>());
		}
		var leavesWithDaysRequested = await GetLeavesWithDaysRequested(leaves, email);

		return Results.Ok(leavesWithDaysRequested);
	}
	public async Task<List<Leave>> GetLeaveRequests(string email)
	{
		return await _context.Leaves
						   .Where(leave => leave.Owner == email &&
										   leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
						   .ToListAsync();
	}
	private async Task<string> ValidateLeave(DateTime dateStart, DateTime dateEnd, string owner, int? leaveId)
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



	public async Task<IResult> CreateLeave(LeaveCreateDTO model)
	{
		var validationResult = await ValidateLeave(model.DateStart, model.DateEnd, model.Owner, null);
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
		var validationResult = await ValidateLeave(leaveUpdate.DateStart, leaveUpdate.DateEnd, leaveUpdate.Owner, leaveUpdate.Id);
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
	private async Task<List<LeaveWithDaysDTO>> GetLeavesWithDaysRequested(List<Leave> leaves, string email)
	{
		// Dynamically calculate requested days
		var leavesWithDays = new List<LeaveWithDaysDTO>();
		foreach (var leave in leaves)
		{
			int requestedDaysThisYear = await _paidTimeOffLeft.GetDaysRequested(leave.DateStart, leave.DateEnd, email, DateTime.UtcNow.Year, leave.Id);
			int requestedDaysNextYear = await _paidTimeOffLeft.GetDaysRequested(leave.DateStart, leave.DateEnd, email, DateTime.UtcNow.Year + 1, leave.Id);

			leavesWithDays.Add(new LeaveWithDaysDTO
			{
				Id = leave.Id,
				Type = leave.Type,
				DateStart = leave.DateStart,
				DateEnd = leave.DateEnd,
				Description = leave.Description,
				ApprovedBy = leave.ApprovedBy,
				RejectedBy = leave.RejectedBy,
				DaysRequested = requestedDaysThisYear + requestedDaysNextYear,
			});
		}

		return leavesWithDays;
	}
}
