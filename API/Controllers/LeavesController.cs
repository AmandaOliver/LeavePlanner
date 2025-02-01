using LeavePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
[Authorize]
[ApiController]
[Route("leaves")]
public class LeavesController : ControllerBase
{
	private readonly LeavesService _leavesService;

	public LeavesController(LeavesService leavesService)
	{
		_leavesService = leavesService;
	}

	[LeaveOwnerOrManager]
	[HttpGet("{leaveId}")]
	public async Task<IResult> GetLeaveInfo(string leaveId)
	{
		var result = await _leavesService.GetLeaveInfo(leaveId);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.LeaveWithDynamicInfo);
	}

	[SelfAccessOnly]
	[HttpGet("myleaves/{employeeId}")]
	public async Task<IResult> GetMyLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpGet("circle/{employeeId}")]
	public async Task<IResult> GetMyCircleLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyCircleLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[OrganizationMemberOnly]
	[HttpGet("all/{organizationId}")]
	public async Task<IResult> GetAllLeaves(string organizationId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetAllLeaves(organizationId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpGet("approved/{employeeId}")]
	public async Task<IResult> GetLeavesApproved(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesApproved(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpGet("past/{employeeId}")]
	public async Task<IResult> GetPastLeaves(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetPastLeaves(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpGet("rejected/{employeeId}")]
	public async Task<IResult> GetLeavesRejected(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesRejected(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpGet("pending/{employeeId}")]
	public async Task<IResult> GetLeavesPending(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesPending(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[SelfAccessOnly]
	[HttpPost("validate/{employeeId}")]
	public async Task<IResult> ValidateLeaveRequest(string employeeId, [FromBody] LeaveValidateDTO leaveToValidate)
	{
		var result = await _leavesService.ValidateLeaveRequest(employeeId, leaveToValidate);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[SelfAccessOnly]
	[HttpPost("{employeeId}")]
	public async Task<IResult> CreateLeave(string employeeId, [FromBody] LeaveCreateDTO model)
	{
		var result = await _leavesService.CreateLeave(employeeId, model);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[LeaveOwnerOrManager]
	[HttpPut("{leaveId}")]
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate)
	{
		var result = await _leavesService.UpdateLeave(leaveId, leaveUpdate);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[LeaveOwnerOrManager]
	[HttpDelete("{leaveId}")]
	public async Task<IResult> DeleteLeave(int leaveId)
	{
		var result = await _leavesService.DeleteLeave(leaveId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}
}
