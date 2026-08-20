using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Queries;

public record GetLeaveInfoQuery(string LeaveId) : IQuery<Result<LeaveDTO>>;

public class GetLeaveInfoQueryHandler : IRequestHandler<GetLeaveInfoQuery, Result<LeaveDTO>>
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;

	public GetLeaveInfoQueryHandler(LeavePlannerContext context, LeavesService leavesService)
	{
		_context = context;
		_leavesService = leavesService;
	}

	public async Task<Result<LeaveDTO>> Handle(GetLeaveInfoQuery request, CancellationToken cancellationToken)
	{
		var leave = await _context.Leaves.FindAsync(new object?[] { int.Parse(request.LeaveId) }, cancellationToken);
		if (leave == null)
		{
			return Result<LeaveDTO>.NotFound("leave not found");
		}

		return Result<LeaveDTO>.Success(await _leavesService.GetLeaveDynamicInfo(leave));
	}
}
