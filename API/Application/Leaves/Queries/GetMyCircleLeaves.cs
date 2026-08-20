using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetMyCircleLeavesQuery(string EmployeeId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetMyCircleLeavesQueryHandler : IRequestHandler<GetMyCircleLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly EmployeeHierarchy _hierarchy;

	public GetMyCircleLeavesQueryHandler(
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		EmployeeHierarchy hierarchy)
	{
		_employees = employees;
		_leaves = leaves;
		_hierarchy = hierarchy;
	}

	public async Task<Result<List<LeaveDTO>>> Handle(GetMyCircleLeavesQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.EmployeeId, out var employeeId))
		{
			return Result<List<LeaveDTO>>.Invalid("Invalid employee id.");
		}

		if (!DateTime.TryParse(request.Start, out var start) || !DateTime.TryParse(request.End, out var end))
		{
			return Result<List<LeaveDTO>>.Invalid("You need to specify start and end");
		}
		var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
		if (employee == null)
		{
			return Result<List<LeaveDTO>>.Invalid("employee not found");
		}

		var allLeaves = new List<Leave>();
		var manager = employee.ManagedBy == null
			? null
			: await _employees.GetByIdAsync(employee.ManagedBy.Value, cancellationToken);

		if (manager == null)
		{
			allLeaves.AddRange(await _leaves.GetApprovedByOwnerAsync(employeeId, cancellationToken));
		}
		else
		{
			allLeaves.AddRange(await _leaves.GetApprovedByOwnerAsync(manager.Id, cancellationToken));

			var managerWithSubordinates = await _hierarchy.GetWithSubordinates(manager, cancellationToken);
			foreach (var subordinate in managerWithSubordinates.Subordinates!)
			{
				allLeaves.AddRange(await _leaves.GetApprovedByOwnerAsync(subordinate.Id, cancellationToken));
			}
		}

		var employeeWithSubordinates = await _hierarchy.GetWithSubordinates(employee, cancellationToken);
		foreach (var subordinate in employeeWithSubordinates.Subordinates!)
		{
			allLeaves.AddRange(await _leaves.GetApprovedByOwnerAsync(subordinate.Id, cancellationToken));
		}

		if (allLeaves.Count == 0)
		{
			return Result<List<LeaveDTO>>.Success([]);
		}

		var leaveDTOs = new List<LeaveDTO>();
		foreach (var leave in allLeaves.Where(leave => leave.DateEnd >= start && leave.DateStart <= end))
		{
			var leaveOwner = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
			if (leaveOwner == null)
			{
				return Result<List<LeaveDTO>>.Invalid("error getting owner");
			}

			leaveDTOs.Add(leave.ToLeaveDto(leaveOwner.Name));
		}

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}
}
