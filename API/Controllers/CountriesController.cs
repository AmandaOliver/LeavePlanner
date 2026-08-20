using LeavePlanner.Application.Common;
using LeavePlanner.Application.Countries.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("countries")]
public class CountriesController : ControllerBase
{
	private readonly IMediator _mediator;

	public CountriesController(IMediator mediator) => _mediator = mediator;

	[HttpGet]
	[Authorize]
	public async Task<IResult> GetCountries(CancellationToken cancellationToken) =>
		(await _mediator.Send(new GetCountriesQuery(), cancellationToken)).ToHttpResult();
}
