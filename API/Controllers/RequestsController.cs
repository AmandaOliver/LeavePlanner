using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using MySqlX.XDevAPI.Common;



public static class RequestsEndpointsExtensions
{
	public static void MapRequestsEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/requests/{email}", async (RequestsController controller, string email) => await controller.GetRequests(email)).RequireAuthorization();
		endpoints.MapPost("/requests/{email}/approve/{id}", async (RequestsController controller, string email, string id) => await controller.ApproveRequest(id, email)).RequireAuthorization();
		endpoints.MapPost("/requests/{email}/reject/{id}", async (RequestsController controller, string email, string id) => await controller.RejectRequest(id, email)).RequireAuthorization();

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