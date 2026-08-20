using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record UpdateLeaveCommand(int LeaveId, LeaveUpdateDTO Leave) : ICommand<Result<LeaveDTO>>;

public class UpdateLeaveCommandHandler : IRequestHandler<UpdateLeaveCommand, Result<LeaveDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public UpdateLeaveCommandHandler(
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

	public async Task<Result<LeaveDTO>> Handle(UpdateLeaveCommand command, CancellationToken cancellationToken)
	{
		var update = command.Leave;
		try
		{
			await _evaluator.AssertCanRequest(
				update.DateStart, update.DateEnd, update.Owner, update.Id, update.Type, cancellationToken);
		}
		catch (DomainException ex)
		{
			return Result<LeaveDTO>.Invalid(ex.Message);
		}

		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var leave = await _leaves.GetByIdAsync(command.LeaveId, cancellationToken);
			if (leave == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<LeaveDTO>.Invalid("Leave not found with that Id");
			}

			var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
			if (employee == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<LeaveDTO>.Invalid("Employee not found.");
			}

			int? systemApproverId = null;
			if (employee.IsOrgHead)
			{
				systemApproverId = (await _employees.GetSystemAsync(cancellationToken)).Id;
			}

			leave.Amend(update.DateStart, update.DateEnd, update.Description, _clock.UtcNow, employee.IsOrgHead, systemApproverId);

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
		catch
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			throw;
		}
	}
}
