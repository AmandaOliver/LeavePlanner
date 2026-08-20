using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record ValidateLeaveRequestQuery(string EmployeeId, LeaveValidateDTO Leave) : IQuery<Result<LeaveDTO>>;

public class ValidateLeaveRequestQueryHandler : IRequestHandler<ValidateLeaveRequestQuery, Result<LeaveDTO>>
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;

	public ValidateLeaveRequestQueryHandler(LeavePlannerContext context, LeavesService leavesService)
	{
		_context = context;
		_leavesService = leavesService;
	}

	public async Task<Result<LeaveDTO>> Handle(ValidateLeaveRequestQuery request, CancellationToken cancellationToken)
	{
		var employeeId = int.Parse(request.EmployeeId);
		var toValidate = request.Leave;

		var employee = await _context.Employees.FindAsync(new object?[] { employeeId }, cancellationToken);
		if (employee == null)
		{
			return Result<LeaveDTO>.Invalid("Employee not found.");
		}

		var validationResult = await _leavesService.ValidateLeave(
			toValidate.DateStart, toValidate.DateEnd, employeeId, toValidate.Id, toValidate.Type);
		if (validationResult != "success")
		{
			return Result<LeaveDTO>.Invalid(validationResult);
		}

		var leaveRequest = new Leave
		{
			Id = toValidate.Id ?? 0,
			DateStart = toValidate.DateStart,
			DateEnd = toValidate.DateEnd,
			Type = toValidate.Type,
			Owner = employeeId,
			OwnerNavigation = employee,
		};

		return Result<LeaveDTO>.Success(await _leavesService.GetLeaveDynamicInfo(leaveRequest));
	}
}
