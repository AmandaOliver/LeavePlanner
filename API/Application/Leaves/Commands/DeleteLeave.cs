using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record DeleteLeaveCommand(int LeaveId) : ICommand<Result<Leave>>;

public class DeleteLeaveCommandHandler : IRequestHandler<DeleteLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;
	private readonly IClock _clock;

	public DeleteLeaveCommandHandler(
		LeavePlannerContext context,
		ILeaveRepository leaves,
		IUnitOfWork unitOfWork,
		IClock clock)
	{
		_context = context;
		_leaves = leaves;
		_unitOfWork = unitOfWork;
		_clock = clock;
	}

	public async Task<Result<Leave>> Handle(DeleteLeaveCommand command, CancellationToken cancellationToken)
	{
		var leave = await _leaves.GetByIdAsync(command.LeaveId, cancellationToken);
		if (leave == null)
		{
			return Result<Leave>.Invalid("Leave not found");
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			leave.Cancel(_clock.UtcNow);
			_leaves.Remove(leave);

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
