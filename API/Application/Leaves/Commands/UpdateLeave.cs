using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record UpdateLeaveCommand(int LeaveId, LeaveUpdateDTO Leave) : ICommand<Result<Leave>>;

public class UpdateLeaveCommandHandler : IRequestHandler<UpdateLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public UpdateLeaveCommandHandler(
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

	public async Task<Result<Leave>> Handle(UpdateLeaveCommand command, CancellationToken cancellationToken)
	{
		var update = command.Leave;
		try
		{
			await _evaluator.AssertCanRequest(
				update.DateStart, update.DateEnd, update.Owner, update.Id, update.Type, cancellationToken);
		}
		catch (DomainException ex)
		{
			return Result<Leave>.Invalid(ex.Message);
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var leave = await _leaves.GetByIdAsync(command.LeaveId, cancellationToken);
			if (leave == null)
			{
				return Result<Leave>.Invalid("Leave not found with that Id");
			}

			var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
			if (employee == null)
			{
				return Result<Leave>.Invalid("Employee not found.");
			}

			int? systemApproverId = null;
			if (employee.IsOrgHead)
			{
				systemApproverId = (await _employees.GetSystemAsync(cancellationToken)).Id;
			}

			leave.Amend(update.DateStart, update.DateEnd, update.Description, _clock.UtcNow, employee.IsOrgHead, systemApproverId);

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
