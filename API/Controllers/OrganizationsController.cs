using LeavePlanner.Models;
public static class OrganizationEndpointsExtensions
{
    public static void MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/organization/{id}", async (OrganizationsController controller, string id) => await controller.GetOrganization(id)).RequireAuthorization();
        endpoints.MapPut("/organization/{id}", async (OrganizationsController controller, int id, OrganizationUpdateDTO organizationUpdate) => await controller.UpdateOrganization(id, organizationUpdate)).RequireAuthorization();
        endpoints.MapDelete("/organization/{id}", async (OrganizationsController controller, string id) => await controller.DeleteOrganization(id)).RequireAuthorization();
        endpoints.MapPost("/organization/import/{id}", async (OrganizationsController controller, string id, IFormFile file) => await controller.ImportOrganization(id, file)).DisableAntiforgery().RequireAuthorization();
    }
}
public class OrganizationsController
{
    private readonly OrganizationsService _organizationsService;

    public OrganizationsController(OrganizationsService organizationsService)
    {
        _organizationsService = organizationsService;
    }
    public async Task<IResult> ImportOrganization(string organizationId, IFormFile file)
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
    public async Task<IResult> GetOrganization(string id)
    {
        var result = await _organizationsService.GetOrganization(id);
        if (!result.IsSuccess)
            return Results.NotFound(result.ErrorMessage);

        return Results.Ok(result.organization);

    }
    public async Task<IResult> UpdateOrganization(int organizationId, OrganizationUpdateDTO organizationUpdate)
    {
        var result = await _organizationsService.UpdateOrganization(organizationId, organizationUpdate);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.organization);
    }
    public async Task<IResult> DeleteOrganization(string id)
    {
        var result = await _organizationsService.DeleteOrganization(id);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.organization);
    }
}