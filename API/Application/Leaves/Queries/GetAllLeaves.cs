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
		var organizationId = int.Parse(request.OrganizationId);
		var leaves = await _leaves.GetApprovedInOrganizationAsync(organizationId, cancellationToken);

		if (leaves.Count == 0)
		{
			return Result<List<LeaveDTO>>.Success([]);
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
