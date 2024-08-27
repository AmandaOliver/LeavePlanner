using LeavePlanner.Data;
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Models;

public static class OrganizationsController
{
    public static void MapOrganizationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/organization/{id}", GetOrganization).RequireAuthorization();

    }
    public static async Task<IResult> GetOrganization(string id, LeavePlannerContext context)
    {

        var organization = await context.Organizations.Include(o => o.Employees)
                                    .FirstOrDefaultAsync(e => e.Id.ToString() == id);

        if (organization != null)
        {
            var employeeTree = BuildEmployeeHierarchy(organization.Employees.Where(e => e.ManagedBy == null && e.IsOrgOwner != true).ToList(), organization.Employees.ToList());
            var result = new
            {
                organization.Id,
                organization.Name,
                Tree = employeeTree
            };
            return Results.Ok(result);
        }
        else
        {
            return Results.NotFound("Organization does not exists.");
        }
    }
    private static List<EmployeeWithSubordinates> BuildEmployeeHierarchy(List<Employee> managers, List<Employee> allEmployees)
    {
        var result = new List<EmployeeWithSubordinates>();

        foreach (var manager in managers)
        {
            var subordinates = allEmployees.Where(e => e.ManagedBy == manager.Email).ToList();
            var employeeDto = new EmployeeWithSubordinates
            {
                Name = manager.Name,
                Email = manager.Email,
                Country = manager.Country,
                PaidTimeOff = manager.PaidTimeOff,
                ManagedBy = manager.ManagedBy,
                Title = manager.Title,
                Subordinates = BuildEmployeeHierarchy(subordinates, allEmployees)
            };
            result.Add(employeeDto);
        }

        return result;
    }
}