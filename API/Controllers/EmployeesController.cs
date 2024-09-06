
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
public static class EmployeesEndpointsExtensions
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", async (EmployeesController controller, string email) => await controller.GetEmployee(email))
                 .RequireAuthorization();
        endpoints.MapPost("/employee", async (EmployeesController controller, EmployeeCreateModel model) => await controller.CreateEmployee(model))
                 .RequireAuthorization();
        endpoints.MapPut("/employee/{email}", async (EmployeesController controller, string email, EmployeeUpdateModel model) => await controller.UpdateEmployee(email, model))
                 .RequireAuthorization();
        endpoints.MapDelete("/employee/{email}", async (EmployeesController controller, string email) => await controller.DeleteEmployee(email))
                 .RequireAuthorization();
    }
}
public class EmployeesController
{
    private readonly LeavePlannerContext _context;
    private readonly BankHolidayService _bankholidayService;


    public EmployeesController(LeavePlannerContext context, BankHolidayService bankHolidayService)
    {
        _context = context;
        _bankholidayService = bankHolidayService;

    }
    public async Task<IResult> CreateEmployee(EmployeeCreateModel model)
    {

        // Check for invalid data
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Country) || model.Organization == 0 || model.PaidTimeOff == 0)
        {
            return Results.BadRequest("Invalid data.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            Employee employee;
            var existingEmployee = await _context.Employees
                                           .FirstOrDefaultAsync(e => e.Email == model.Email);
            if (existingEmployee != null)
            {
                existingEmployee.Country = model.Country;
                existingEmployee.Organization = model.Organization;
                existingEmployee.ManagedBy = model.ManagedBy;
                existingEmployee.PaidTimeOff = model.PaidTimeOff;
                existingEmployee.Title = model.Title;
                existingEmployee.Name = model.Name;

                _context.Employees.Update(existingEmployee);
                employee = existingEmployee;
            }
            else
            {
                employee = new Employee
                {
                    Email = model.Email,
                    Country = model.Country,
                    Organization = model.Organization,
                    ManagedBy = model.ManagedBy,
                    IsOrgOwner = false,
                    PaidTimeOff = model.PaidTimeOff,
                    Title = model.Title,
                    Name = model.Name,
                };

                // Add employee to context
                _context.Employees.Add(employee);
            }
            await _context.SaveChangesAsync();
            await _bankholidayService.GenerateEmployeeBankHolidays(employee);
            await transaction.CommitAsync();

            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.ToString());
        }
    }

    public async Task<IResult> GetEmployee(string email)
    {
        var employee = await _context.Employees
                                    .FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("User is not an employee.");
        }

        // Fetch subordinates recursively
        var employeeWithSubordinates = await GetEmployeeWithSubordinates(employee.Email);

        return Results.Ok(employeeWithSubordinates);
    }

    private async Task<EmployeeWithSubordinates> GetEmployeeWithSubordinates(string employeeEmail)
    {
        var employee = await _context.Employees
                                    .Where(e => e.Email == employeeEmail)
                                    .Select(e => new EmployeeWithSubordinates
                                    {
                                        Email = e.Email,
                                        Name = e.Name,
                                        Country = e.Country,
                                        Organization = e.Organization,
                                        ManagedBy = e.ManagedBy,
                                        IsOrgOwner = e.IsOrgOwner,
                                        PaidTimeOff = e.PaidTimeOff,
                                        Title = e.Title,
                                        Subordinates = new List<EmployeeWithSubordinates>()
                                    })
                                    .FirstOrDefaultAsync();

        if (employee == null)
        {
            throw new Exception("Employee not found.");
        }

        var subordinates = await _context.Employees
                                        .Where(e => e.ManagedBy == employeeEmail)
                                        .ToListAsync();

        foreach (var subordinate in subordinates)
        {
            var subordinateWithSubordinates = await GetEmployeeWithSubordinates(subordinate.Email);
            employee.Subordinates.Add(subordinateWithSubordinates);
        }

        return employee;

    }

    public async Task<IResult> UpdateEmployee(string email, EmployeeUpdateModel model)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("Employee not found.");
        }
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // if country changes we need to update the leaves
            if (employee.Country != model.Country)
            {
                var leaves = await _context.Leaves.Where(l => l.Owner == employee.Email).ToListAsync();
                _context.Leaves.RemoveRange(leaves);
                employee.Country = model.Country;
                await _context.SaveChangesAsync();
                await _bankholidayService.GenerateEmployeeBankHolidays(employee);
            }
            employee.PaidTimeOff = model.PaidTimeOff != 0 ? model.PaidTimeOff : employee.PaidTimeOff;
            employee.Title = model.Title ?? employee.Title;
            employee.Name = model.Name ?? employee.Name;

            await transaction.CommitAsync();
            await _context.SaveChangesAsync();
            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem("An error occurred while updating the employee.");
        }
    }
    public async Task<IResult> DeleteEmployee(string email)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                return Results.NotFound("Employee not found.");
            }

            // Check if the employee is the head of the organization (ManagedBy is null)
            if (employee.ManagedBy == null)
            {
                // Check if the head manages any subordinates
                var subordinates = await _context.Employees
                                                .Where(e => e.ManagedBy == employee.Email)
                                                .ToListAsync();

                if (subordinates.Any())
                {
                    return Results.BadRequest("Cannot delete the head of the organization because they manage other employees.");
                }
            }
            else
            {

                // Reassign subordinates to the manager above
                var subordinates = await _context.Employees
                                                .Where(e => e.ManagedBy == employee.Email)
                                                .ToListAsync();

                foreach (var subordinate in subordinates)
                {
                    subordinate.ManagedBy = employee.ManagedBy; // Reassign to the manager above
                }

                _context.Employees.UpdateRange(subordinates);
            }
            // Remove leaves associated with the employee
            var leaves = await _context.Leaves.Where(l => l.Owner == employee.Email).ToListAsync();
            _context.Leaves.RemoveRange(leaves);

            // if employee is org owner, can't be deleted completely
            if (employee.IsOrgOwner == true)
            {
                employee.Country = null;
                employee.ManagedBy = null;
                employee.PaidTimeOff = null;
                employee.Title = null;

                _context.Employees.Update(employee);


            }
            else
            {
                // Remove the employee from the context
                _context.Employees.Remove(employee);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok("Employee deleted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the exception (ex) as needed
            return Results.Problem("An error occurred while deleting the employee.");
        }
    }
}
