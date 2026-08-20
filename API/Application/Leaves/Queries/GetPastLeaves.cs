using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetPastLeavesQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedLeavesResult>>;

public class GetPastLeavesQueryHandler : IRequestHandler<GetPastLeavesQuery, Result<PaginatedLeavesResult>>
{
	private readonly ILeaveRepository _leaves;
	private readonly IClock _clock;

	public GetPastLeavesQueryHandler(ILeaveRepository leaves, IClock clock)
	{
		_leaves = leaves;
		_clock = clock;
	}

	public async Task<Result<PaginatedLeavesResult>> Handle(GetPastLeavesQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.EmployeeId, out var employeeId))
		{
			return Result<PaginatedLeavesResult>.Invalid("Invalid employee id.");
		}

		var leaves = await _leaves.GetApprovedPastByOwnerAsync(employeeId, _clock.UtcNow, cancellationToken);

		return Result<PaginatedLeavesResult>.Success(new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = leaves.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Select(leave => leave.ToLeaveDto()).ToList()
		});
	}
}
