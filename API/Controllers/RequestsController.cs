using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;



public static class RequestsEndpointsExtensions
{
	public static void MapRequestsEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/requests/{id}", async (RequestsController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetRequestsOfAManager(id, page, pageSize)).RequireAuthorization();
		endpoints.MapGet("/request/{id}", async (RequestsController controller, string id) => await controller.GetRequestConflicts(id)).RequireAuthorization();
		endpoints.MapGet("/requests/reviewed/{id}", async (RequestsController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetReviewedRequestsOfAManager(id, page, pageSize)).RequireAuthorization();
		endpoints.MapPost("/requests/{employeeId}/approve/{id}", async (RequestsController controller, string employeeId, string id) => await controller.ApproveRequest(id, employeeId)).RequireAuthorization();
		endpoints.MapPost("/requests/{employeeId}/reject/{id}", async (RequestsController controller, string employeeId, string id) => await controller.RejectRequest(id, employeeId)).RequireAuthorization();

	}
}
public class RequestsController
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly LeavesService _leavesService;
	private readonly EmailService _emailService;


	public RequestsController(LeavePlannerContext context, EmployeesService employeesService, LeavesService leavesService, EmailService emailService)
	{
		_context = context;
		_employeesService = employeesService;
		_leavesService = leavesService;
		_emailService = emailService;
	}
	public async Task<IResult> GetRequestConflicts(string id)
	{
		var leave = await _context.Leaves.FindAsync(int.Parse(id));
		if (leave == null)
		{
			return Results.NotFound("Request not found");
		}
		var leaveDTO = await _leavesService.GetLeaveDynamicInfo(leave, true);
		return Results.Ok(leaveDTO);
	}
	public async Task<IResult> GetRequestsOfAManager(string id, int page, int pageSize)
	{
		var manager = await _context.Employees.FindAsync(int.Parse(id));
		if (manager == null)
		{
			return Results.NotFound("employee not found");
		}
		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return Results.NotFound("employee is not a manager");
		}
		var requests = new List<LeaveDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			var subordinateRequests = await _leavesService.GetLeaveRequests(subordinate);
			requests.AddRange(subordinateRequests);
		}

		// Apply pagination
		var paginatedRequests = requests
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return Results.Ok(new
		{
			TotalCount = requests.Count,
			Requests = paginatedRequests
		});
	}
	public async Task<IResult> GetReviewedRequestsOfAManager(string id, int page, int pageSize)
	{
		var manager = await _context.Employees.FindAsync(int.Parse(id));
		if (manager == null)
		{
			return Results.NotFound("employee not found");
		}
		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return Results.NotFound("employee is not a manager");
		}
		var requests = new List<LeaveDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates)
		{
			var subordinateRequests = await _leavesService.GetReviewedRequests(subordinate);
			requests.AddRange(subordinateRequests);
		}

		// Apply pagination
		var paginatedRequests = requests
			.Skip((page - 1) * pageSize)
			.Take(pageSize)
			.ToList();

		return Results.Ok(new
		{
			TotalCount = requests.Count,
			Requests = paginatedRequests
		});
	}

	public async Task<IResult> ApproveRequest(string id, string employeeId)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(employeeId))
		{
			return Results.BadRequest("employee and request id can't be empty");
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return Results.NotFound("request not found");

			}
			request.ApprovedBy = int.Parse(employeeId);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			var employee = await _context.Employees.FindAsync(request.Owner);
			if (employee != null)
			{
				string emailBody = $@"
Hello {employee.Name}, 
	Your leave request from {request.DateStart.ToShortDateString()} to {request.DateEnd.ToShortDateString()} 
	has been approved. 
	Enjoy your time off!.";
				await _emailService.SendEmail(employee.Email, $"Leave Request approved", emailBody);
			}
			return Results.Ok(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);
		}
	}
	public async Task<IResult> RejectRequest(string id, string employeeId)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(employeeId))
		{
			return Results.BadRequest("Employee and request id can't be empty");
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return Results.NotFound("request not found");

			}
			request.RejectedBy = int.Parse(employeeId);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			var employee = await _context.Employees.FindAsync(request.Owner);
			if (employee != null)
			{
				string emailBody = $@"
Hello {employee.Name}, 
	Your leave request from {request.DateStart.ToShortDateString()} to {request.DateEnd.ToShortDateString()} 
	has been rejected. ";
				await _emailService.SendEmail(employee.Email, $"Leave Request rejected", emailBody);
			}
			return Results.Ok(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return Results.Problem(ex.Message);
		}
	}

}