
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("requests")]
public class RequestsController
{
	private readonly RequestsService _requestsService;


	public RequestsController(RequestsService requestsService)
	{
		_requestsService = requestsService;
	}
	[HttpGet("{requestId}")]
	[Authorize]
	public async Task<IResult> GetRequest(string requestId)
	{
		var result = await _requestsService.GetRequest(requestId);
		if (!result.IsSuccess)
			return Results.NotFound(result.ErrorMessage);

		return Results.Ok(result.RequestWithDynamicInfo);
	}
	[HttpGet("review/{requestId}")]
	[Authorize]
	public async Task<IResult> GetRequestsOfAManager(string requestId, int page, int pageSize)
	{
		var result = await _requestsService.GetRequestsOfAManager(requestId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}

	[HttpGet("reviewed/{requestId}")]
	[Authorize]
	public async Task<IResult> GetReviewedRequestsOfAManager(string requestId, int page, int pageSize)
	{
		var result = await _requestsService.GetReviewedRequestsOfAManager(requestId, page, pageSize);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.requests);
	}

	[HttpPost("{employeeId}/approve/{requestId}")]
	[Authorize]
	public async Task<IResult> ApproveRequest(string requestId, string employeeId)
	{
		var result = await _requestsService.ApproveRequest(requestId, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}

	[HttpPost("{employeeId}/reject/{requestId}")]
	[Authorize]
	public async Task<IResult> RejectRequest(string requestId, string employeeId)
	{
		var result = await _requestsService.RejectRequest(requestId, employeeId);
		if (!result.IsSuccess)
			return Results.BadRequest(result.ErrorMessage);

		return Results.Ok(result.request);
	}

}