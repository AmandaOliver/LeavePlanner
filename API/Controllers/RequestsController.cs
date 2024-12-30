using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;



public static class RequestsEndpointsExtensions
{
	public static void MapRequestsEndpoints(this IEndpointRouteBuilder endpoints)
	{

		endpoints.MapGet("/requests/{id}", async (RequestsController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetRequestsOfAManager(id, page, pageSize)).RequireAuthorization();
		endpoints.MapGet("/request/{id}", async (RequestsController controller, string id) => await controller.GetRequestConflicts(id)).RequireAuthorization();
		endpoints.MapGet("/requests/reviewed/{id}", async (RequestsController controller, string id, [FromQuery] int page, [FromQuery] int pageSize) => await controller.GetReviewedRequestsOfAManager(id, page, pageSize)).RequireAuthorization();
		endpoints.MapPost("/requests/{employeeId}/approve/{id}", async (RequestsController controller, string employeeId, string id) => await controller.ApproveRequest(id, employeeId)).RequireAuthorization();
		endpoints.MapPost("/requests/{employeeId}/reject/{id}", async (RequestsController controller, string employeeId, string id) => await controller.RejectRequest(id, employeeId)).RequireAuthorization();

	}
}
public class RequestsController
{
	private readonly EmployeesService _employeesService;
	private readonly RequestsService _requestsService;
	private readonly EmailService _emailService;


	public RequestsController(EmployeesService employeesService, RequestsService requestsService, EmailService emailService)
	{
		_employeesService = employeesService;
		_requestsService = requestsService;
		_emailService = emailService;
	}
	public async Task<IResult> GetRequestConflicts(string id)
	{
		var result = await _requestsService.GetRequestConflicts(id);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.LeaveWithDynamicInfo);
	}
	public async Task<IResult> GetRequestsOfAManager(string id, int page, int pageSize)
	{
		var result = await _requestsService.GetRequestsOfAManager(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}
	public async Task<IResult> GetReviewedRequestsOfAManager(string id, int page, int pageSize)
	{
		var result = await _requestsService.GetReviewedRequestsOfAManager(id, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}

	public async Task<IResult> ApproveRequest(string id, string employeeId)
	{
		var result = await _requestsService.ApproveRequest(id, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}
	public async Task<IResult> RejectRequest(string id, string employeeId)
	{
		var result = await _requestsService.RejectRequest(id, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}

}