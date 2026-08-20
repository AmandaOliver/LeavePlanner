using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Requests.Queries;

public record GetRequestQuery(string RequestId) : IQuery<Result<LeaveDTO>>;

public class GetRequestQueryHandler : IRequestHandler<GetRequestQuery, Result<LeaveDTO>>
{
	private readonly ILeaveRepository _leaves;
	private readonly LeaveEvaluator _evaluator;

	public GetRequestQueryHandler(ILeaveRepository leaves, LeaveEvaluator evaluator)
	{
		_leaves = leaves;
		_evaluator = evaluator;
	}

	public async Task<Result<LeaveDTO>> Handle(GetRequestQuery request, CancellationToken cancellationToken)
	{
		var leave = await _leaves.GetByIdAsync(int.Parse(request.RequestId), cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.NotFound("Request not found");
		}

		return Result<LeaveDTO>.Success(await _evaluator.ComposeDto(leave, true, cancellationToken));
	}
}
