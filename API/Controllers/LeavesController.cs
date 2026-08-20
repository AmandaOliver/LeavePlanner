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
	public async Task<IResult> GetLeaveInfo(string leaveId) =>
		(await _mediator.Send(new GetLeaveInfoQuery(leaveId))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("myleaves/{employeeId}")]
	public async Task<IResult> GetMyLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end) =>
		(await _mediator.Send(new GetMyLeavesQuery(employeeId, start, end))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("circle/{employeeId}")]
	public async Task<IResult> GetMyCircleLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end) =>
		(await _mediator.Send(new GetMyCircleLeavesQuery(employeeId, start, end))).ToHttpResult();

	[OrganizationMemberOnly]
	[HttpGet("all/{organizationId}")]
	public async Task<IResult> GetAllLeaves(string organizationId, [FromQuery] string? start, [FromQuery] string? end) =>
		(await _mediator.Send(new GetAllLeavesQuery(organizationId, start, end))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("approved/{employeeId}")]
	public async Task<IResult> GetLeavesApproved(string employeeId, [FromQuery] int page, [FromQuery] int pageSize) =>
		(await _mediator.Send(new GetLeavesApprovedQuery(employeeId, page, pageSize))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("past/{employeeId}")]
	public async Task<IResult> GetPastLeaves(string employeeId, [FromQuery] int page, [FromQuery] int pageSize) =>
		(await _mediator.Send(new GetPastLeavesQuery(employeeId, page, pageSize))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("rejected/{employeeId}")]
	public async Task<IResult> GetLeavesRejected(string employeeId, [FromQuery] int page, [FromQuery] int pageSize) =>
		(await _mediator.Send(new GetLeavesRejectedQuery(employeeId, page, pageSize))).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("pending/{employeeId}")]
	public async Task<IResult> GetLeavesPending(string employeeId, [FromQuery] int page, [FromQuery] int pageSize) =>
		(await _mediator.Send(new GetLeavesPendingQuery(employeeId, page, pageSize))).ToHttpResult();

	[SelfAccessOnly]
	[HttpPost("validate/{employeeId}")]
	public async Task<IResult> ValidateLeaveRequest(string employeeId, [FromBody] LeaveValidateDTO leaveToValidate) =>
		(await _mediator.Send(new ValidateLeaveRequestQuery(employeeId, leaveToValidate))).ToHttpResult();

	[SelfAccessOnly]
	[HttpPost("{employeeId}")]
	public async Task<IResult> CreateLeave(string employeeId, [FromBody] LeaveCreateDTO model) =>
		(await _mediator.Send(new CreateLeaveCommand(employeeId, model))).ToHttpResult();

	[LeaveOwnerOrManager]
	[HttpPut("{leaveId}")]
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate) =>
		(await _mediator.Send(new UpdateLeaveCommand(leaveId, leaveUpdate))).ToHttpResult();

	[LeaveOwnerOrManager]
	[HttpDelete("{leaveId}")]
	public async Task<IResult> DeleteLeave(int leaveId) =>
		(await _mediator.Send(new DeleteLeaveCommand(leaveId))).ToHttpResult();
}
