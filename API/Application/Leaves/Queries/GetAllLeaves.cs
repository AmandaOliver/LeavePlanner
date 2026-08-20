using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetAllLeavesQuery(string OrganizationId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetAllLeavesQueryHandler : IRequestHandler<GetAllLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;

	public GetAllLeavesQueryHandler(IEmployeeRepository employees, ILeaveRepository leaves)
	{
		_employees = employees;
		_leaves = leaves;
	}

	public async Task<Result<List<LeaveDTO>>> Handle(GetAllLeavesQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.OrganizationId, out var organizationId))
		{
			return Result<List<LeaveDTO>>.Invalid("Invalid organization id.");
		}

		var leaves = await _leaves.GetApprovedInOrganizationAsync(organizationId, cancellationToken);

		if (leaves.Count == 0)
		{
			return Result<List<LeaveDTO>>.Success([]);
		}

		if (!DateTime.TryParse(request.Start, out var start) || !DateTime.TryParse(request.End, out var end))
		{
			return Result<List<LeaveDTO>>.Invalid("You need to specify start and end");
		}

		var leaveDTOs = new List<LeaveDTO>();
		foreach (var leave in leaves.Where(leave => leave.DateEnd >= start && leave.DateStart <= end))
		{
			var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
			if (employee == null)
			{
				return Result<List<LeaveDTO>>.Invalid("employee not found");
			}

			leaveDTOs.Add(leave.ToLeaveDto(employee.Name));
		}

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}
}
