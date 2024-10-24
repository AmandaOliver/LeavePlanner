using LeavePlanner.Data;
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Models;
public static class OrganizationEndpointsExtensions
{
    public static void MapOrganizationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/organization/{id}", async (OrganizationsController controller, string id) => await controller.GetOrganization(id)).RequireAuthorization();
        endpoints.MapPut("/organization/{id}", async (OrganizationsController controller, int id, OrganizationUpdateDTO organizationUpdate) => await controller.UpdateOrganization(id, organizationUpdate)).RequireAuthorization();
        endpoints.MapDelete("/organization/{id}", async (OrganizationsController controller, string id) => await controller.DeleteOrganization(id)).RequireAuthorization();
    }
}
public class OrganizationsController
{
    private readonly LeavePlannerContext _context;

    public OrganizationsController(LeavePlannerContext context)
    {
        _context = context;
    }
    public async Task<IResult> GetOrganization(string id)
    {
        try
        {
            var organization = await _context.Organizations.Include(o => o.Employees)
                                        .FirstOrDefaultAsync(e => e.Id.ToString() == id);
            if (organization != null)
            {
                var employeeTree = BuildEmployeeHierarchy(organization.Employees.Where(e => e.ManagedBy == null && e.Country != null).ToList(), organization.Employees.ToList());
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
        catch (Exception ex)
        {
            return Results.NotFound("Organization does not exists.");
        }

    }
    public async Task<IResult> UpdateOrganization(int organizationId, OrganizationUpdateDTO organizationUpdate)
    {

        using var transaction = await _context.Database.BeginTransactionAsync();
        var organization = await _context.Organizations.FindAsync(organizationId);
        if (organization == null)
        {
            return Results.NotFound("Organization not found with that Id");
        }
        try
        {

            organization.Name = organizationUpdate.Name;


            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(organization);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.Message);

        }

    }
    public async Task<IResult> DeleteOrganization(string id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var organization = await _context.Organizations.FindAsync(int.Parse(id));
            if (organization == null)
            {
                return Results.NotFound("Organization not found.");
            }

            var employees = await _context.Employees
                .Where(e => e.Organization.ToString() == id)
                .ToListAsync();

            foreach (var employee in employees)
            {
                await DeleteEmployeeWithSubordinates(employee.Email);
            }

            _context.Organizations.Remove(organization);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok("Organization and related data deleted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem($"An error occurred: {ex.Message}");
        }
    }

    private async Task DeleteEmployeeWithSubordinates(string email)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
        if (employee == null) return;

        var subordinates = await _context.Employees
            .Where(e => e.ManagedBy == email)
            .ToListAsync();

        foreach (var subordinate in subordinates)
        {
            await DeleteEmployeeWithSubordinates(subordinate.Email);
        }

        var leaves = await _context.Leaves
            .Where(l => l.Owner == employee.Email)
            .ToListAsync();

        _context.Leaves.RemoveRange(leaves);

        _context.Employees.Remove(employee);
    }



    private List<EmployeeWithSubordinatesDTO> BuildEmployeeHierarchy(List<Employee> managers, List<Employee> allEmployees)
    {
        var result = new List<EmployeeWithSubordinatesDTO>();

        foreach (var manager in managers)
        {
            var subordinates = allEmployees.Where(e => e.ManagedBy == manager.Email).ToList();
            var employeeDto = new EmployeeWithSubordinatesDTO
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