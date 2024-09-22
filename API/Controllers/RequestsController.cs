using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.IdentityModel.Tokens;



public static class RequestsEndpointsExtensions
{
	public static void MapRequestsEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/requests/{email}", async (RequestsController controller, string email) => await controller.GetRequests(email)).RequireAuthorization();

	}
}
public class RequestsController
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesController _employeesController;
	private readonly LeavesController _leavesController;

	public RequestsController(LeavePlannerContext context, EmployeesController employeesController, LeavesController leavesController)
	{
		_context = context;
		_employeesController = employeesController;
		_leavesController = leavesController;
	}
	public async Task<IResult> GetRequests(string email)
	{
		var employeeWithSubordinates = await _employeesController.GetEmployeeWithSubordinates(email);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			Results.NotFound("employee is not a manager");
		}
		var requests = new List<RequestDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			var subordinateRequests = await _leavesController.GetLeaveRequests(subordinate.Email);
			foreach (var leaveRequest in subordinateRequests)
			{
				var request = new RequestDTO
				{
					Id = leaveRequest.Id,
					Type = leaveRequest.Type,
					DateStart = leaveRequest.DateStart,
					DateEnd = leaveRequest.DateEnd,
					Description = leaveRequest.Description,
					OwnerName = leaveRequest.OwnerNavigation.Name,
				};
				requests.Add(request);
			}

		}
		return Results.Ok(requests);
	}
}