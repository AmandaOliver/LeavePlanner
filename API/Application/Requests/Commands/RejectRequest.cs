using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using MediatR;

namespace LeavePlanner.Application.Requests.Commands;

public record RejectRequestCommand(string RequestId, string EmployeeId) : ICommand<Result<Leave>>;

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;

	public RejectRequestCommandHandler(LeavePlannerContext context, ILeaveRepository leaves, IUnitOfWork unitOfWork)
	{
		_context = context;
		_leaves = leaves;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Leave>> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(command.RequestId) || string.IsNullOrEmpty(command.EmployeeId))
		{
			return Result<Leave>.Invalid("Employee and request id can't be empty");
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var request = await _leaves.GetByIdAsync(int.Parse(command.RequestId), cancellationToken);
			if (request == null)
			{
				return Result<Leave>.Invalid("request not found");
			}

			request.Reject(int.Parse(command.EmployeeId));
			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<Leave>.Success(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
	}
}
