using LeavePlanner.Application.Common;
using LeavePlanner.Application.Requests.Commands;
using LeavePlanner.Application.Requests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("requests")]
public class RequestsController : ControllerBase
{
	private readonly IMediator _mediator;

	public RequestsController(IMediator mediator) => _mediator = mediator;

	[ManagerOnly]
	[HttpGet("{requestId}")]
	public async Task<IResult> GetRequest(string requestId, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetRequestQuery(requestId), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("review/{employeeId}")]
	public async Task<IResult> GetRequestsOfAManager(string employeeId, int page, int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetRequestsOfAManagerQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[SelfAccessOnly]
	[HttpGet("reviewed/{employeeId}")]
	public async Task<IResult> GetReviewedRequestsOfAManager(string employeeId, int page, int pageSize, CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetReviewedRequestsOfAManagerQuery(employeeId, page, pageSize), cancellationToken)).ToHttpResult();

	[ManagerOnly]
	[HttpPost("{employeeId}/approve/{requestId}")]
	public async Task<IResult> ApproveRequest(string requestId, string employeeId, CancellationToken cancellationToken) =>
		(await _mediator.Send(new ApproveRequestCommand(requestId, employeeId), cancellationToken)).ToHttpResult();

	[ManagerOnly]
	[HttpPost("{employeeId}/reject/{requestId}")]
	public async Task<IResult> RejectRequest(string requestId, string employeeId, CancellationToken cancellationToken) =>
		(await _mediator.Send(new RejectRequestCommand(requestId, employeeId), cancellationToken)).ToHttpResult();
}
