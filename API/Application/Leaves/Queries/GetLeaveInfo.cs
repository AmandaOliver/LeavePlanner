using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetLeaveInfoQuery(string LeaveId) : IQuery<Result<LeaveDTO>>;

public class GetLeaveInfoQueryHandler : IRequestHandler<GetLeaveInfoQuery, Result<LeaveDTO>>
{
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;

	public GetLeaveInfoQueryHandler(ILeaveRepository leaves, LeaveEvaluator evaluator)
	{
		_leaves = leaves;
		_evaluator = evaluator;
	}

	public async Task<Result<LeaveDTO>> Handle(GetLeaveInfoQuery request, CancellationToken cancellationToken)
	{
		if (!int.TryParse(request.LeaveId, out var leaveId))
		{
			return Result<LeaveDTO>.Invalid("Invalid leave id.");
		}

		var leave = await _leaves.GetByIdAsync(leaveId, cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.NotFound("leave not found");
		}

		try
		{
			return Result<LeaveDTO>.Success(await _evaluator.ComposeDto(leave, false, cancellationToken));
		}
		catch (DomainException ex)
		{
			return Result<LeaveDTO>.Invalid(ex.Message);
		}
	}
}
