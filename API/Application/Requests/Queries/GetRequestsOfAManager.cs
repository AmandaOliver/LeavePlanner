using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace LeavePlanner.Application.Requests.Queries;

public record GetRequestsOfAManagerQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedRequestsResult>>;

public class GetRequestsOfAManagerQueryHandler : IRequestHandler<GetRequestsOfAManagerQuery, Result<PaginatedRequestsResult>>
{
	private readonly IEmployeeRepository _employees;
	private readonly EmployeeHierarchy _hierarchy;
	private readonly LeaveEvaluator _evaluator;

	public GetRequestsOfAManagerQueryHandler(
		IEmployeeRepository employees,
		EmployeeHierarchy hierarchy,
		LeaveEvaluator evaluator)
	{
		_employees = employees;
		_hierarchy = hierarchy;
		_evaluator = evaluator;
	}

	public async Task<Result<PaginatedRequestsResult>> Handle(GetRequestsOfAManagerQuery request, CancellationToken cancellationToken)
	{
		var manager = await _employees.GetByIdAsync(int.Parse(request.EmployeeId), cancellationToken);
		if (manager == null)
		{
			return Result<PaginatedRequestsResult>.Invalid("employee not found");
		}

		var employeeWithSubordinates = await _hierarchy.GetWithSubordinates(manager, cancellationToken);
		if (employeeWithSubordinates.Subordinates.IsNullOrEmpty())
		{
			return Result<PaginatedRequestsResult>.Invalid("employee is not a manager");
		}

		var requests = new List<LeaveDTO>();
		foreach (var subordinate in employeeWithSubordinates.Subordinates!)
		{
			requests.AddRange(await _evaluator.GetPendingRequests(subordinate.Id, cancellationToken));
		}

		return Result<PaginatedRequestsResult>.Success(new PaginatedRequestsResult
		{
			TotalCount = requests.Count,
			Requests = requests.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
		});
	}
}
