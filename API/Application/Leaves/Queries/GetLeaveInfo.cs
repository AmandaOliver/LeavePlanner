using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Data;
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
		var leave = await _leaves.GetByIdAsync(int.Parse(request.LeaveId), cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.NotFound("leave not found");
		}

		return Result<LeaveDTO>.Success(await _evaluator.ComposeDto(leave, false, cancellationToken));
	}
}
