using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetLeavesPendingQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedLeavesResult>>;

public class GetLeavesPendingQueryHandler : IRequestHandler<GetLeavesPendingQuery, Result<PaginatedLeavesResult>>
{
	private readonly ILeaveRepository _leaves;

	public GetLeavesPendingQueryHandler(ILeaveRepository leaves) => _leaves = leaves;

	public async Task<Result<PaginatedLeavesResult>> Handle(GetLeavesPendingQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.EmployeeId, out var employeeId))
		{
			return Result<PaginatedLeavesResult>.Invalid("Invalid employee id.");
		}

		var leaves = await _leaves.GetPendingByOwnerAsync(employeeId, cancellationToken);

		return Result<PaginatedLeavesResult>.Success(new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = leaves.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(leave => leave.ToLeaveDto()).ToList()
		});
	}
}
