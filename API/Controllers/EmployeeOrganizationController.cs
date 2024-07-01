using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;

public static class EmployeeOrganizationController
{
    public static void MapEmployeeOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/create-employee-organization", CreateEmployeeAndOrganization);
    }

    public static async Task<IResult> CreateEmployeeAndOrganization(EmployeeOrganizationCreateModel model, LeavePlannerContext context)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.OrganizationName))
        {
            return Results.BadRequest("Invalid data.");
        }

        // Create new organization
        var organization = new Organization
        {
            Name = model.OrganizationName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        // Create new employee and set the IsOrgOwner property to true
        var employee = new Employee
        {
            Picture = model.Picture,
            Email = model.Email,
            Name = model.Name,
            Country = model.Country,
            IsOrgOwner = true,
            Organization = organization.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return Results.Ok(new { OrganizationId = organization.Id });

    }
}