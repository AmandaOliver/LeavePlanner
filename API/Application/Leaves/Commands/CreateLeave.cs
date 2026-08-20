using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record CreateLeaveCommand(string EmployeeId, LeaveCreateDTO Leave) : ICommand<Result<Leave>>;

public class CreateLeaveCommandHandler : IRequestHandler<CreateLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public CreateLeaveCommandHandler(
		LeavePlannerContext context,
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		LeaveEvaluator evaluator,
		IUnitOfWork unitOfWork,
		IClock clock)
	{
		_context = context;
		_employees = employees;
		_leaves = leaves;
		_evaluator = evaluator;
		_unitOfWork = unitOfWork;
		_clock = clock;
	}

	public async Task<Result<Leave>> Handle(CreateLeaveCommand command, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employeeId = int.Parse(command.EmployeeId);
			var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
			if (employee == null)
			{
				return Result<Leave>.Invalid("Employee not found.");
			}

			await _evaluator.AssertCanRequest(
				command.Leave.DateStart, command.Leave.DateEnd, employeeId, null, command.Leave.Type, cancellationToken);

			var leave = Leave.Submit(
				employee, command.Leave.Type, command.Leave.DateStart, command.Leave.DateEnd, command.Leave.Description, _clock.UtcNow);
			_leaves.Add(leave);

			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<Leave>.Success(leave);
		}
		catch (DomainException ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
	}
}
