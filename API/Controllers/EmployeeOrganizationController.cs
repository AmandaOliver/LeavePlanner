using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using LeavePlanner.Models;
using LeavePlanner.Data;

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
            Id = model.EmployeeId,
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

        return Results.Ok(new { EmployeeId = employee.Id, OrganizationId = organization.Id });
        // return Results.Ok(new { EmployeeId = "something", OrganizationId = 1 });

    }
}