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
        endpoints.MapPost("/organization/import/{id}", async (OrganizationsController controller, string id, IFormFile file) => await controller.ImportOrganization(id, file)).DisableAntiforgery().RequireAuthorization();
    }
}
public class OrganizationsController
{
    private readonly LeavePlannerContext _context;
    private readonly OrganizationImportService _importService;

    public OrganizationsController(LeavePlannerContext context, OrganizationImportService importService)
    {
        _context = context;
        _importService = importService;
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
            await _importService.ImportOrganizationHierarchy(organizationId, stream);
            return Results.Ok("Organization tree imported successfully.");
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
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
                    organization.WorkingDays,
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
            if (organizationUpdate.Name == null && organizationUpdate.WorkingDays == null)
            {
                return Results.BadRequest("name or working days needs to be specified");
            }
            if (organizationUpdate.Name != null)
            {
                organization.Name = organizationUpdate.Name;
            }
            if (organizationUpdate.WorkingDays != null)
            {
                if (organizationUpdate.WorkingDays is not IEnumerable<int> || organizationUpdate.WorkingDays.Length < 1 || !organizationUpdate.WorkingDays.All(day => day >= 1 && day <= 7))
                {
                    return Results.BadRequest("Working days must be defined.");
                }
                organization.WorkingDays = organizationUpdate.WorkingDays;
            }


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
                await DeleteEmployeeWithSubordinates(employee.Id);
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

    private async Task DeleteEmployeeWithSubordinates(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return;

        var subordinates = await _context.Employees
            .Where(e => e.ManagedBy == id)
            .ToListAsync();

        foreach (var subordinate in subordinates)
        {
            await DeleteEmployeeWithSubordinates(subordinate.Id);
        }

        var leaves = await _context.Leaves
            .Where(l => l.Owner == employee.Id)
            .ToListAsync();

        _context.Leaves.RemoveRange(leaves);

        _context.Employees.Remove(employee);
    }



    private List<EmployeeWithSubordinatesDTO> BuildEmployeeHierarchy(List<Employee> managers, List<Employee> allEmployees)
    {
        var result = new List<EmployeeWithSubordinatesDTO>();

        foreach (var manager in managers)
        {
            var subordinates = allEmployees.Where(e => e.ManagedBy == manager.Id).ToList();
            var employeeDto = new EmployeeWithSubordinatesDTO
            {
                Id = manager.Id,
                Name = manager.Name,
                Email = manager.Email,
                Country = manager.Country,
                PaidTimeOff = manager.PaidTimeOff,
                ManagedBy = manager.ManagedBy,
                Title = manager.Title,
                IsOrgOwner = manager.IsOrgOwner,
                Subordinates = BuildEmployeeHierarchy(subordinates, allEmployees)
            };
            result.Add(employeeDto);
        }

        return result;
    }
}