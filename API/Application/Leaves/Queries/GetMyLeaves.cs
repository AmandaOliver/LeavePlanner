using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetMyLeavesQuery(string EmployeeId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetMyLeavesQueryHandler : IRequestHandler<GetMyLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly LeavePlannerContext _context;

	public GetMyLeavesQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<List<LeaveDTO>>> Handle(GetMyLeavesQuery request, CancellationToken cancellationToken)
	{
		if (request.Start == null || request.End == null)
		{
			return Result<List<LeaveDTO>>.Invalid("You need to specify start and end");
		}

		var employeeId = int.Parse(request.EmployeeId);
		var employee = await _context.Employees.FindAsync(new object?[] { employeeId }, cancellationToken);
		if (employee == null)
		{
			return Result<List<LeaveDTO>>.Invalid("employee not found");
		}

		var leaves = await _context.Leaves
			.Where(leave => leave.Owner == employeeId && leave.RejectedBy == null)
			.ToListAsync(cancellationToken);

		var start = DateTime.Parse(request.Start);
		var end = DateTime.Parse(request.End);

		var leaveDTOs = leaves
			.Where(leave => leave.DateEnd >= start && leave.DateStart <= end)
			.Select(leave => new LeaveDTO
			{
				Id = leave.Id,
				Type = leave.Type,
				Owner = leave.Owner,
				OwnerName = employee.Name,
				DateStart = leave.DateStart,
				DateEnd = leave.DateEnd,
				Description = leave.Description,
				ApprovedBy = leave.ApprovedBy,
				RejectedBy = leave.RejectedBy,
			})
			.ToList();

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}
}
