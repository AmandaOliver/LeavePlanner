using LeavePlanner.Application.Common;
using LeavePlanner.Application.Organizations.Commands;
using LeavePlanner.Application.Organizations.Queries;
using LeavePlanner.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("organization")]
public class OrganizationsController : ControllerBase
{
	private readonly IMediator _mediator;

	public OrganizationsController(IMediator mediator) => _mediator = mediator;

	[HttpPost]
	public async Task<IResult> CreateOrganization([FromBody] OrganizationCreateDTO model)
	{
		var result = await _mediator.Send(new CreateOrganizationCommand(model));
		return result.IsSuccess
			? Results.Ok(new { OrganizationId = result.Value })
			: result.ToHttpResult();
	}

	[AdminOnly]
	[HttpPost("import/{organizationId}")]
	public async Task<IResult> ImportOrganization(string organizationId, [FromForm] IFormFile file)
	{
		if (string.IsNullOrEmpty(organizationId))
		{
			return Results.BadRequest("Organization ID is missing.");
		}

		if (file == null || file.Length == 0)
		{
			return Results.BadRequest("File is empty");
		}

		using var stream = file.OpenReadStream();
		var result = await _mediator.Send(new ImportOrganizationCommand(organizationId, stream));
		return result.ToHttpResult("Organization tree imported successfully.");
	}

	[OrganizationMemberOnly]
	[HttpGet("{organizationId}")]
	public async Task<IResult> GetOrganization(string organizationId) =>
		(await _mediator.Send(new GetOrganizationQuery(organizationId))).ToHttpResult();

	[AdminOnly]
	[HttpPut("{organizationId}")]
	public async Task<IResult> UpdateOrganization(int organizationId, [FromBody] OrganizationUpdateDTO organizationUpdate) =>
		(await _mediator.Send(new UpdateOrganizationCommand(organizationId, organizationUpdate))).ToHttpResult();

	[AdminOnly]
	[HttpDelete("{organizationId}")]
	public async Task<IResult> DeleteOrganization(string organizationId) =>
		(await _mediator.Send(new DeleteOrganizationCommand(organizationId))).ToHttpResult();
}
