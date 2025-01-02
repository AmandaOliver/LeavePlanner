using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.IdentityModel.Tokens;

public class PaginatedRequestsResult
{
	public int TotalCount { get; set; }
	public List<LeaveDTO>? Requests { get; set; }
}
public class RequestsService
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly LeavesService _leavesService;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly string _leavePlannerUrl;

	public RequestsService(LeavePlannerContext context, EmployeesService employeesService, EmailService emailService, IConfiguration configuration, LeavesService leavesService)
	{
		_context = context;
		_employeesService = employeesService;
		_leavesService = leavesService;
		_emailService = emailService;
		_configuration = configuration;
		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedRequestsResult? requests)> GetReviewedRequestsOfAManager(string id, int page, int pageSize)
	{
		var manager = await _context.Employees.FindAsync(int.Parse(id));
		if (manager == null)
		{
			return (false, "employee not found", null);
		}
		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return (false, "employee is not a manager", null);
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

		return (true, null, new PaginatedRequestsResult
		{
			TotalCount = requests.Count,
			Requests = paginatedRequests
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Leave? request)> ApproveRequest(string id, string employeeId)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(employeeId))
		{
			return (false, "employee and request id can't be empty", null);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return (false, "request not found", null);

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
			return (true, null, request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Leave? request)> RejectRequest(string id, string employeeId)
	{
		if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(employeeId))
		{
			return (false, "Employee and request id can't be empty", null);
		}
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var request = await _context.Leaves.FindAsync(int.Parse(id));
			if (request == null)
			{
				return (false, "request not found", null);

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
			return (true, null, request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, PaginatedRequestsResult? requests)> GetRequestsOfAManager(string id, int page, int pageSize)
	{
		var manager = await _context.Employees.FindAsync(int.Parse(id));
		if (manager == null)
		{
			return (false, "employee not found", null);
		}
		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return (false, "employee is not a manager", null);
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

		return (true, null, new PaginatedRequestsResult
		{
			TotalCount = requests.Count,
			Requests = paginatedRequests
		});
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, LeaveDTO? RequestWithDynamicInfo)> GetRequest(string id)
	{
		var leave = await _context.Leaves.FindAsync(int.Parse(id));
		if (leave == null)
		{
			return (false, "Request not found", null);
		}
		var leaveDTO = await _leavesService.GetLeaveDynamicInfo(leave, true);
		return (true, null, leaveDTO);
	}
}