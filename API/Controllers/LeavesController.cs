using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;

// https://www.googleapis.com/calendar/v3/calendars/en.italian%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c


public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/leaves/{email}", async (LeavesController controller, string email) => await controller.GetLeaves(email))
				 .RequireAuthorization();
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

}
