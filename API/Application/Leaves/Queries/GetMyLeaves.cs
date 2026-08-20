using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetMyLeavesQuery(string EmployeeId, string? Start, string? End) : IQuery<Result<List<LeaveDTO>>>;

public class GetMyLeavesQueryHandler : IRequestHandler<GetMyLeavesQuery, Result<List<LeaveDTO>>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;

	public GetMyLeavesQueryHandler(IEmployeeRepository employees, ILeaveRepository leaves)
	{
		_employees = employees;
		_leaves = leaves;
	}

	public async Task<Result<List<LeaveDTO>>> Handle(GetMyLeavesQuery request, CancellationToken cancellationToken)
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

		var leaves = await _leaves.GetNotRejectedByOwnerAsync(employeeId, cancellationToken);
		var start = DateTime.Parse(request.Start);
		var end = DateTime.Parse(request.End);

		var leaveDTOs = leaves
			.Where(leave => leave.DateEnd >= start && leave.DateStart <= end)
			.Select(leave => leave.ToLeaveDto(employee.Name))
			.ToList();

		return Result<List<LeaveDTO>>.Success(leaveDTOs);
	}
}
