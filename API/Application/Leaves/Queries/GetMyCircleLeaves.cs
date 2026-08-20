using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetMyCircleLeavesQuery(string EmployeeId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetMyCircleLeavesQueryHandler : IRequestHandler<GetMyCircleLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly EmployeeHierarchy _hierarchy;

	public GetMyCircleLeavesQueryHandler(
		LeavePlannerContext context,
		IEmployeeRepository employees,
		EmployeeHierarchy hierarchy)
	{
		_context = context;
		_employees = employees;
		_hierarchy = hierarchy;
	}

	public async Task<Result<List<LeaveDTO>>> Handle(GetMyCircleLeavesQuery request, CancellationToken cancellationToken)
	{
		if (request.Start == null || request.End == null)
		{
			return Result<List<LeaveDTO>>.Invalid("You need to specify start and end");
		}

		var employeeId = int.Parse(request.EmployeeId);
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
			allLeaves.AddRange(await ApprovedLeavesOf(employeeId, cancellationToken));
		}
		else
		{
			allLeaves.AddRange(await ApprovedLeavesOf(employee.ManagedBy, cancellationToken));

			var managerWithSubordinates = await _hierarchy.GetWithSubordinates(manager, cancellationToken);
			foreach (var subordinate in managerWithSubordinates.Subordinates!)
			{
				allLeaves.AddRange(await ApprovedLeavesOf(subordinate.Id, cancellationToken));
			}
		}

		var employeeWithSubordinates = await _hierarchy.GetWithSubordinates(employee, cancellationToken);
		foreach (var subordinate in employeeWithSubordinates.Subordinates!)
		{
			allLeaves.AddRange(await ApprovedLeavesOf(subordinate.Id, cancellationToken));
		}

		if (allLeaves.Count == 0)
		{
			return Result<List<LeaveDTO>>.Success([]);
		}

		var start = DateTime.Parse(request.Start);
		var end = DateTime.Parse(request.End);

		var leaveDTOs = new List<LeaveDTO>();
		foreach (var leave in allLeaves.Where(leave => leave.DateEnd >= start && leave.DateStart <= end))
		{
			var leaveOwner = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
			if (leaveOwner == null)
			{
				return Result<List<LeaveDTO>>.Invalid("error getting owner");
			}

			leaveDTOs.Add(new LeaveDTO
			{
				Id = leave.Id,
				Type = leave.Type,
				Owner = leave.Owner,
				OwnerName = leaveOwner.Name,
				DateStart = leave.DateStart,
				DateEnd = leave.DateEnd,
				Description = leave.Description,
				ApprovedBy = leave.ApprovedBy,
				RejectedBy = leave.RejectedBy,
			});
		}

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}

	private Task<List<Leave>> ApprovedLeavesOf(int? ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.ApprovedBy != null)
			.ToListAsync(cancellationToken);
}
