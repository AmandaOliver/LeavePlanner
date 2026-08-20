using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetPastLeavesQuery(string EmployeeId, int Page, int PageSize) : IQuery<Result<PaginatedLeavesResult>>;

public class GetPastLeavesQueryHandler : IRequestHandler<GetPastLeavesQuery, Result<PaginatedLeavesResult>>
{
	private readonly LeavePlannerContext _context;

	public GetPastLeavesQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<PaginatedLeavesResult>> Handle(GetPastLeavesQuery request, CancellationToken cancellationToken)
	{
		var employeeId = int.Parse(request.EmployeeId);

		var leaves = await _context.Leaves
			.Where(leave => leave.Owner == employeeId && leave.ApprovedBy != null && leave.DateStart < DateTime.UtcNow)
			.OrderByDescending(leave => leave.DateStart)
			.ToListAsync(cancellationToken);

		return Result<PaginatedLeavesResult>.Success(new PaginatedLeavesResult
		{
			TotalCount = leaves.Count,
			Leaves = leaves.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList()
		});
	}
}
