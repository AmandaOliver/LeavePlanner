
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("requests")]
public class RequestsController
{
	private readonly RequestsService _requestsService;


	public RequestsController(RequestsService requestsService)
	{
		_requestsService = requestsService;
	}

	[ManagerOnly]
	[HttpGet("{requestId}")]
	public async Task<IResult> GetRequest(string requestId)
	{
		var result = await _requestsService.GetRequest(requestId);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.RequestWithDynamicInfo);
	}

	[SelfAccessOnly]
	[HttpGet("review/{employeeId}")]
	public async Task<IResult> GetRequestsOfAManager(string employeeId, int page, int pageSize)
	{
		var result = await _requestsService.GetRequestsOfAManager(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}

	[SelfAccessOnly]
	[HttpGet("reviewed/{employeeId}")]
	public async Task<IResult> GetReviewedRequestsOfAManager(string employeeId, int page, int pageSize)
	{
		var result = await _requestsService.GetReviewedRequestsOfAManager(employeeId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}

	[ManagerOnly]
	[HttpPost("{employeeId}/approve/{requestId}")]
	public async Task<IResult> ApproveRequest(string requestId, string employeeId)
	{
		var result = await _requestsService.ApproveRequest(requestId, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}

	[ManagerOnly]
	[HttpPost("{employeeId}/reject/{requestId}")]
	public async Task<IResult> RejectRequest(string requestId, string employeeId)
	{
		var result = await _requestsService.RejectRequest(requestId, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}

}