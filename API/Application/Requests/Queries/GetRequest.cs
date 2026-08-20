using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Requests.Queries;

public record GetRequestQuery(string RequestId) : IQuery<Result<LeaveDTO>>;

public class GetRequestQueryHandler : IRequestHandler<GetRequestQuery, Result<LeaveDTO>>
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;

	public GetRequestQueryHandler(LeavePlannerContext context, LeavesService leavesService)
	{
		_context = context;
		_leavesService = leavesService;
	}

	public async Task<Result<LeaveDTO>> Handle(GetRequestQuery request, CancellationToken cancellationToken)
	{
		var leave = await _context.Leaves.FindAsync(new object?[] { int.Parse(request.RequestId) }, cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.NotFound("Request not found");
		}

		var leaveDTO = await _leavesService.GetLeaveDynamicInfo(leave, true);
		return Result<LeaveDTO>.Success(leaveDTO);
	}
}
