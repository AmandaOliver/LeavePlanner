using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.IdentityModel.Tokens;



public static class RequestsEndpointsExtensions
{
	public static void MapRequestsEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/requests/{email}", async (RequestsController controller, string email) => await controller.GetRequestsOfAManager(email)).RequireAuthorization();
		endpoints.MapPost("/requests/{email}/approve/{id}", async (RequestsController controller, string email, string id) => await controller.ApproveRequest(id, email)).RequireAuthorization();
		endpoints.MapPost("/requests/{email}/reject/{id}", async (RequestsController controller, string email, string id) => await controller.RejectRequest(id, email)).RequireAuthorization();

	}
}
public class RequestsController
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesController _employeesController;
	private readonly LeavesService _leavesService;


	public RequestsController(LeavePlannerContext context, EmployeesController employeesController, LeavesService leavesService)
	{
		_context = context;
		_employeesController = employeesController;
		_leavesService = leavesService;
	}
	public async Task<IResult> GetRequestsOfAManager(string email)
	{
		var employeeWithSubordinates = await _employeesController.GetEmployeeWithSubordinates(email);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			Results.NotFound("employee is not a manager");
		}
		var requests = new List<LeaveDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			var subordinateRequests = await _leavesService.GetLeaveRequests(subordinate.Email);
			requests.AddRange(subordinateRequests);
		}

		return Results.Ok(requests);
	}
	public async Task<IResult> ApproveRequest(string id, string email)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
		{
			return Results.BadRequest("email and request id can't be empty");
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return Results.NotFound("request not found");

			}
			request.ApprovedBy = email;
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			return Results.Ok(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);
		}
	}
	public async Task<IResult> RejectRequest(string id, string email)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
		{
			return Results.BadRequest("email and request id can't be empty");
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return Results.NotFound("request not found");

			}
			request.RejectedBy = email;
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			return Results.Ok(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);
		}
	}

}