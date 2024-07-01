using LeavePlanner.Data;
using Microsoft.EntityFrameworkCore;

public static class OrganizationsController
{
    public static void MapOrganizationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
                endpoints.MapGet("/organization/{id}", GetOrganization).RequireAuthorization();

    }
	public static async Task<IResult> GetOrganization(string id, LeavePlannerContext context)
    {

        var organization = await context.Organizations
                                    .FirstOrDefaultAsync(e => e.Id.ToString() == id);

        if (organization != null)
        {
            return Results.Ok(organization);
        }
        else
        {
            return Results.NotFound("Organization does not exists.");
        }
	}
}