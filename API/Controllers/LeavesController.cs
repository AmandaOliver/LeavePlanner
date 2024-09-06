using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;

// https://www.googleapis.com/calendar/v3/calendars/en.italian%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c


public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeaves(email)).RequireAuthorization();
		endpoints.MapPost("/leaves", async (LeavesController controller, LeaveCreateModel model) => await controller.CreateLeave(model)).RequireAuthorization();
	}
}
public class LeavesController
{
	private readonly LeavePlannerContext _context;


	public LeavesController(LeavePlannerContext context)
	{
		_context = context;
	}
	public async Task<IResult> GetLeaves(string email)
	{
		var leaves = await _context.Leaves
						  .Where(leave => leave.Owner == email).ToListAsync();

		if (leaves == null || leaves.Count == 0)
		{
			return Results.NotFound("No leaves found for this employee.");
		}

		// Return the leaves
		return Results.Ok(leaves);
	}

	public async Task<IResult> CreateLeave(LeaveCreateModel model)
	{
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
			return Results.Problem(ex.ToString());
		}
	}

}
