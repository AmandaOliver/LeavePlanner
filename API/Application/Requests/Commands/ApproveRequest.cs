using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Requests.Commands;

public record ApproveRequestCommand(string RequestId, string EmployeeId) : ICommand<Result<LeaveDTO>>;

public class ApproveRequestCommandHandler : IRequestHandler<ApproveRequestCommand, Result<LeaveDTO>>
{
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;

	public ApproveRequestCommandHandler(ILeaveRepository leaves, IUnitOfWork unitOfWork)
	{
		_leaves = leaves;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<LeaveDTO>> Handle(ApproveRequestCommand command, CancellationToken cancellationToken)
	{
		if (!int.TryParse(command.RequestId, out var requestId) || !int.TryParse(command.EmployeeId, out var employeeId))
		{
			return Result<LeaveDTO>.Invalid("employee and request id can't be empty");
		}

		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var request = await _leaves.GetByIdAsync(requestId, cancellationToken);
			if (request == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<LeaveDTO>.Invalid("request not found");
			}

			request.Approve(employeeId);
			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<LeaveDTO>.Success(request.ToLeaveDto());
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
