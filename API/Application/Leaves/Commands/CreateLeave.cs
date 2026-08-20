using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record CreateLeaveCommand(string EmployeeId, LeaveCreateDTO Leave) : ICommand<Result<LeaveDTO>>;

public class CreateLeaveCommandHandler : IRequestHandler<CreateLeaveCommand, Result<LeaveDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public CreateLeaveCommandHandler(
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		LeaveEvaluator evaluator,
		IUnitOfWork unitOfWork,
		IClock clock)
	{
		_employees = employees;
		_leaves = leaves;
		_evaluator = evaluator;
		_unitOfWork = unitOfWork;
		_clock = clock;
	}

	public async Task<Result<LeaveDTO>> Handle(CreateLeaveCommand command, CancellationToken cancellationToken)
	{
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var employeeId = int.Parse(command.EmployeeId);
			var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
			if (employee == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<LeaveDTO>.Invalid("Employee not found.");
			}

			await _evaluator.AssertCanRequest(
				command.Leave.DateStart, command.Leave.DateEnd, employeeId, null, command.Leave.Type, cancellationToken);

			var leave = Leave.Submit(
				employee, command.Leave.Type, command.Leave.DateStart, command.Leave.DateEnd, command.Leave.Description, _clock.UtcNow);
			_leaves.Add(leave);

			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<LeaveDTO>.Success(leave.ToLeaveDto(employee.Name));
		}
		catch (DomainException ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<LeaveDTO>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<LeaveDTO>.Invalid(ex.Message);
		}
	}
}
