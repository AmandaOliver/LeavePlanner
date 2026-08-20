using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetAllLeavesQuery(string OrganizationId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetAllLeavesQueryHandler : IRequestHandler<GetAllLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly LeavePlannerContext _context;

	public GetAllLeavesQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<List<LeaveDTO>>> Handle(GetAllLeavesQuery request, CancellationToken cancellationToken)
	{
		var organizationId = int.Parse(request.OrganizationId);

		var leaves = await _context.Leaves
			.Where(leave => leave.ApprovedBy != null && leave.OwnerNavigation.Organization == organizationId)
			.ToListAsync(cancellationToken);

		if (leaves.Count == 0)
		{
			return Result<List<LeaveDTO>>.Success(new List<LeaveDTO>());
		}

		if (request.Start == null || request.End == null)
		{
			return Result<List<LeaveDTO>>.Invalid("You need to specify start and end");
		}

		var start = DateTime.Parse(request.Start);
		var end = DateTime.Parse(request.End);

		var leaveDTOs = new List<LeaveDTO>();
		foreach (var leave in leaves.Where(leave => leave.DateEnd >= start && leave.DateStart <= end))
		{
			var employee = await _context.Employees.FindAsync(new object?[] { leave.Owner }, cancellationToken);
			if (employee == null)
			{
				return Result<List<LeaveDTO>>.Invalid("employee not found");
			}

			leaveDTOs.Add(new LeaveDTO
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
			});
		}

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}
}
