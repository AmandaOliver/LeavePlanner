using LeavePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("organization")]
public class OrganizationsController : ControllerBase
{
    private readonly OrganizationsService _organizationsService;

    public OrganizationsController(OrganizationsService organizationsService)
    {
        _organizationsService = organizationsService;
    }
    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateOrganization([FromBody] EmployeeOrganizationCreateDTO model)
    {
        var result = await _organizationsService.CreateEmployeeAndOrganization(model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(new { OrganizationId = result.OrganizationId });
    }

    [HttpPost("import/{organizationId}")]
    [Authorize]
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

        try
        {
            using var stream = file.OpenReadStream();
            await _organizationsService.ImportOrganizationHierarchy(organizationId, stream);
            return Results.Ok("Organization tree imported successfully.");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    }

    [HttpGet("{organizationId}")]
    [Authorize]
    public async Task<IResult> GetOrganization(string organizationId)
    {
        var result = await _organizationsService.GetOrganization(organizationId);
        if (!result.IsSuccess)
            return Results.NotFound(result.ErrorMessage);

        return Results.Ok(result.organization);

    }

    [HttpPut("{organizationId}")]
    [Authorize]
    public async Task<IResult> UpdateOrganization(int organizationId, [FromBody] OrganizationUpdateDTO organizationUpdate)
    {
        var result = await _organizationsService.UpdateOrganization(organizationId, organizationUpdate);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.organization);
    }

    [HttpDelete("{organizationId}")]
    [Authorize]
    public async Task<IResult> DeleteOrganization(string organizationId)
    {
        var result = await _organizationsService.DeleteOrganization(organizationId);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.organization);
    }
}