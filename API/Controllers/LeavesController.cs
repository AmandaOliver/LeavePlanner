using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.AspNetCore.Mvc;

public static class LeavesEndpointsExtensions
{
	public static void MapLeavesEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/leave/{id}", async (LeavesController controller, string id) => await controller.GetLeaveInfo(id)).RequireAuthorization();
		endpoints.MapGet("/myleaves/{id}", async (LeavesController controller, string id, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetMyLeaves(id, start, end)).RequireAuthorization();
		endpoints.MapGet("/mycircleleaves/{id}", async (LeavesController controller, string id, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetMyCircleLeaves(id, start, end)).RequireAuthorization();
		endpoints.MapGet("/allleaves", async (LeavesController controller, [FromQuery] string? start, [FromQuery] string? end) => await controller.GetAllLeaves(start, end)).RequireAuthorization();
		endpoints.MapGet("/leaves/{id}", async (LeavesController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetLeavesApproved(id, page, pageSize)).RequireAuthorization();
		endpoints.MapGet("/pastleaves/{id}", async (LeavesController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetPastLeaves(id, page, pageSize)).RequireAuthorization();
		endpoints.MapGet("/leaves/pending/{id}", async (LeavesController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetLeavesAwaitingApproval(id, page, pageSize)).RequireAuthorization();
		endpoints.MapGet("/leaves/rejected/{id}", async (LeavesController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetLeavesRejected(id, page, pageSize)).RequireAuthorization();
		endpoints.MapPost("/leaves/validate", async (LeavesController controller, LeaveValidateDTO model) => await controller.ValidateLeaveRequest(model)).RequireAuthorization();
		endpoints.MapPost("/leaves", async (LeavesController controller, LeaveCreateDTO model) => await controller.CreateLeave(model)).RequireAuthorization();
		endpoints.MapPut("/leaves/{leaveId}", async (LeavesController controller, int leaveId, LeaveUpdateDTO leaveUpdate) => await controller.UpdateLeave(leaveId, leaveUpdate)).RequireAuthorization();
		endpoints.MapDelete("/leaves/{leaveId}", async (LeavesController controller, int leaveId) => await controller.DeleteLeave(leaveId)).RequireAuthorization();
	}
}
public class LeavesController
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;

	public LeavesController(LeavePlannerContext context, LeavesService leavesService)
	{
		_context = context;
		_leavesService = leavesService;
	}
	public async Task<IResult> GetLeaveInfo(string id)
	{
		var result = await _leavesService.GetLeaveInfo(id);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.LeaveWithDynamicInfo);
	}
	public async Task<IResult> GetMyLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetMyCircleLeaves(string employeeId, [FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetMyCircleLeaves(employeeId, start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetAllLeaves([FromQuery] string? start, [FromQuery] string? end)
	{
		var result = await _leavesService.GetAllLeaves(start, end);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetLeavesApproved(string id, int page, int pageSize)
	{
		var result = await _leavesService.GetLeavesApproved(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetPastLeaves(string id, int page, int pageSize)
	{
		var result = await _leavesService.GetPastLeaves(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetLeavesRejected(string id, int page, int pageSize)
	{
		var result = await _leavesService.GetLeavesRejected(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> GetLeavesAwaitingApproval(string id, int page, int pageSize)
	{
		var result = await _leavesService.GetLeavesAwaitingApproval(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leaves);
	}
	public async Task<IResult> ValidateLeaveRequest(LeaveValidateDTO leaveToValidate)
	{
		var validationResult = await _leavesService.ValidateLeave(leaveToValidate.DateStart, leaveToValidate.DateEnd, leaveToValidate.Owner, leaveToValidate.Id, leaveToValidate.Type);
		if (validationResult != "success")
		{
			return Results.BadRequest(validationResult);
		}
		var employee = await _context.Employees.FindAsync(leaveToValidate.Owner);
		if (employee == null)
		{
			return Results.NotFound("Employee not found.");
		}
		Leave leaveRequest = new Leave
		{
			Id = leaveToValidate.Id ?? 0,
			DateStart = leaveToValidate.DateStart,
			DateEnd = leaveToValidate.DateEnd,
			Type = "paidTimeOff",
			Owner = leaveToValidate.Owner,
			OwnerNavigation = employee,
		};
		var leave = await _leavesService.GetLeaveDynamicInfo(leaveRequest);
		return Results.Ok(leave);

	}
	public async Task<IResult> CreateLeave(LeaveCreateDTO model)
	{
		var result = await _leavesService.CreateLeave(model);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}
	public async Task<IResult> UpdateLeave(int leaveId, LeaveUpdateDTO leaveUpdate)
	{
		var result = await _leavesService.UpdateLeave(leaveId, leaveUpdate);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}
	public async Task<IResult> DeleteLeave(int leaveId)
	{
		var result = await _leavesService.DeleteLeave(leaveId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.leave);
	}
}
