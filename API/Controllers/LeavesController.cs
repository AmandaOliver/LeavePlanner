using LeavePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("leaves")]
public class LeavesController : ControllerBase
{
	private readonly LeavesService _leavesService;

	public LeavesController(LeavesService leavesService)
	{
		_leavesService = leavesService;
	}

	[HttpGet("{leaveId}")]
	[Authorize]
	public async Task<IResult> GetLeaveInfo(string leaveId)
	{
		var result = await _leavesService.GetLeaveInfo(leaveId);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.LeaveWithDynamicInfo);
	}

	[HttpGet("myleaves/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetMyLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("circle/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetMyCircleLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyCircleLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("all/{organizationId}")]
	[Authorize]
	public async Task<IResult> GetAllLeaves(string organizationId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetAllLeaves(organizationId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("approved/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetLeavesApproved(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesApproved(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("past/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetPastLeaves(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetPastLeaves(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("rejected/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetLeavesRejected(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesRejected(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpGet("pending/{employeeId}")]
	[Authorize]
	public async Task<IResult> GetLeavesPending(string employeeId, [FromQuery] int page, [FromQuery] int pageSize)
	{
		var result = await _leavesService.GetLeavesPending(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}

	[HttpPost("validate")]
	[Authorize]
	public async Task<IResult> ValidateLeaveRequest([FromBody] LeaveValidateDTO leaveToValidate)
	{
		var result = await _leavesService.ValidateLeaveRequest(leaveToValidate);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[HttpPost]
	[Authorize]
	public async Task<IResult> CreateLeave([FromBody] LeaveCreateDTO model)
	{
		var result = await _leavesService.CreateLeave(model);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[HttpPut("{leaveId}")]
	[Authorize]
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate)
	{
		var result = await _leavesService.UpdateLeave(leaveId, leaveUpdate);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}

	[HttpDelete("{leaveId}")]
	[Authorize]
	public async Task<IResult> DeleteLeave(int leaveId)
	{
		var result = await _leavesService.DeleteLeave(leaveId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}
}
