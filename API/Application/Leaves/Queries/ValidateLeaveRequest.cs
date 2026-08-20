using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record ValidateLeaveRequestQuery(string EmployeeId, LeaveValidateDTO Leave) : IQuery<Result<LeaveDTO>>;

public class ValidateLeaveRequestQueryHandler : IRequestHandler<ValidateLeaveRequestQuery, Result<LeaveDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly LeaveEvaluator _evaluator;

	public ValidateLeaveRequestQueryHandler(IEmployeeRepository employees, LeaveEvaluator evaluator)
	{
		_employees = employees;
		_evaluator = evaluator;
	}

	public async Task<Result<LeaveDTO>> Handle(ValidateLeaveRequestQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.EmployeeId, out var employeeId))
		{
			return Result<LeaveDTO>.Invalid("Invalid employee id.");
		}

		try
		{
			var toValidate = request.Leave;

			var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
			if (employee == null)
			{
				return Result<LeaveDTO>.Invalid("Employee not found.");
			}

			await _evaluator.AssertCanRequest(
				toValidate.DateStart, toValidate.DateEnd, employeeId, toValidate.Id, toValidate.Type, cancellationToken);

			var leaveRequest = Leave.Preview(employee, toValidate.Id, toValidate.Type, toValidate.DateStart, toValidate.DateEnd);
			return Result<LeaveDTO>.Success(await _evaluator.ComposeDto(leaveRequest, false, cancellationToken));
		}
		catch (DomainException ex)
		{
			return Result<LeaveDTO>.Invalid(ex.Message);
		}
	}
}
