using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record DeleteLeaveCommand(int LeaveId) : ICommand<Result<LeaveDTO>>;

public class DeleteLeaveCommandHandler : IRequestHandler<DeleteLeaveCommand, Result<LeaveDTO>>
{
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public DeleteLeaveCommandHandler(
		ILeaveRepository leaves,
		IUnitOfWork unitOfWork,
		IClock clock)
	{
		_leaves = leaves;
		_unitOfWork = unitOfWork;
		_clock = clock;
	}

	public async Task<Result<LeaveDTO>> Handle(DeleteLeaveCommand command, CancellationToken cancellationToken)
	{
		var leave = await _leaves.GetByIdAsync(command.LeaveId, cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.Invalid("Leave not found");
		}

		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			leave.Cancel(_clock.UtcNow);
			_leaves.Remove(leave);

			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<LeaveDTO>.Success(leave.ToLeaveDto());
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
