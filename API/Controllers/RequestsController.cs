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
	public async Task<IResult> GetRequestsOfAManager(string email)
	{
		var manager = await _context.Employees.FindAsync(email);
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