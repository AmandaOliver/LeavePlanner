using LeavePlanner.Application.Common;
using LeavePlanner.Application.Leaves.Commands;
using LeavePlanner.Application.Leaves.Queries;
using LeavePlanner.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("leaves")]
public class LeavesController : ControllerBase
{
	private readonly IMediator _mediator;

	public LeavesController(IMediator mediator) => _mediator = mediator;

	[LeaveOwnerOrManager]
	[HttpGet("{leaveId}")]
	public async Task<IResult> GetLeaveInfo(string leaveId, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetLeaveInfoQuery(leaveId), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("myleaves/{employeeId}")]
	public async Task<IResult> GetMyLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetMyLeavesQuery(employeeId, start, end), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("circle/{employeeId}")]
	public async Task<IResult> GetMyCircleLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetMyCircleLeavesQuery(employeeId, start, end), cancellationToken)).ToHttpResult();

	[OrganizationMemberOnly]
	[HttpGet("all/{organizationId}")]
	public async Task<IResult> GetAllLeaves(string organizationId, [FromQuery] string? start, [FromQuery] string? end, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetAllLeavesQuery(organizationId, start, end), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("approved/{employeeId}")]
	public async Task<IResult> GetLeavesApproved(string employeeId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetLeavesApprovedQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("past/{employeeId}")]
	public async Task<IResult> GetPastLeaves(string employeeId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetPastLeavesQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("rejected/{employeeId}")]
	public async Task<IResult> GetLeavesRejected(string employeeId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetLeavesRejectedQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("pending/{employeeId}")]
	public async Task<IResult> GetLeavesPending(string employeeId, [FromQuery] int page, [FromQuery] int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetLeavesPendingQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpPost("validate/{employeeId}")]
	public async Task<IResult> ValidateLeaveRequest(string employeeId, [FromBody] LeaveValidateDTO leaveToValidate, CancellationToken cancellationToken) =>
		(await _mediator.Send(new ValidateLeaveRequestQuery(employeeId, leaveToValidate), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpPost("{employeeId}")]
	public async Task<IResult> CreateLeave(string employeeId, [FromBody] LeaveCreateDTO model, CancellationToken cancellationToken) =>
		(await _mediator.Send(new CreateLeaveCommand(employeeId, model), cancellationToken)).ToHttpResult();

	[LeaveOwnerOrManager]
	[HttpPut("{leaveId}")]
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate, CancellationToken cancellationToken) =>
		(await _mediator.Send(new UpdateLeaveCommand(leaveId, leaveUpdate), cancellationToken)).ToHttpResult();

	[LeaveOwnerOrManager]
	[HttpDelete("{leaveId}")]
	public async Task<IResult> DeleteLeave(int leaveId, CancellationToken cancellationToken) =>
		(await _mediator.Send(new DeleteLeaveCommand(leaveId), cancellationToken)).ToHttpResult();
}
