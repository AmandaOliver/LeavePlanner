using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetLeavesPendingQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedLeavesResult>>;

public class GetLeavesPendingQueryHandler : IRequestHandler<GetLeavesPendingQuery, Result<PaginatedLeavesResult>>
{
	private readonly LeavePlannerContext _context;

	public GetLeavesPendingQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<PaginatedLeavesResult>> Handle(GetLeavesPendingQuery request, CancellationToken cancellationToken)
	{
		var employeeId = int.Parse(request.EmployeeId);

		var leaves = await _context.Leaves
			.Where(leave => leave.Owner == employeeId && leave.ApprovedBy == null && leave.RejectedBy == null)
			.ToListAsync(cancellationToken);

		return Result<PaginatedLeavesResult>.Success(new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = leaves.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
		});
	}
}
