using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace LeavePlanner.Application.Requests.Queries;

public record GetRequestsOfAManagerQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedRequestsResult>>;

public class GetRequestsOfAManagerQueryHandler : IRequestHandler<GetRequestsOfAManagerQuery, Result<PaginatedRequestsResult>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly LeavesService _leavesService;

	public GetRequestsOfAManagerQueryHandler(LeavePlannerContext context, EmployeesService employeesService, LeavesService leavesService)
	{
		_context = context;
		_employeesService = employeesService;
		_leavesService = leavesService;
	}

	public async Task<Result<PaginatedRequestsResult>> Handle(GetRequestsOfAManagerQuery request, CancellationToken cancellationToken)
	{
		var manager = await _context.Employees.FindAsync(new object?[] { int.Parse(request.EmployeeId) }, cancellationToken);
		if (manager == null)
		{
			return Result<PaginatedRequestsResult>.Invalid("employee not found");
		}

		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(manager);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return Result<PaginatedRequestsResult>.Invalid("employee is not a manager");
		}

		var requests = new List<LeaveDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates!)
		{
			requests.AddRange(await _leavesService.GetLeaveRequests(subordinate));
		}

		return Result<PaginatedRequestsResult>.Success(new PaginatedRequestsResult
		{
			TotalCount = requests.Count,
			Requests = requests.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
		});
	}
}
