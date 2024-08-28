
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Newtonsoft.Json;

public static class EmployeesController
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", GetEmployee).RequireAuthorization();
        endpoints.MapPost("/employee", CreateEmployee).RequireAuthorization();
        endpoints.MapPut("/employee/{email}", UpdateEmployee).RequireAuthorization();
        endpoints.MapDelete("/employee/{email}", DeleteEmployee).RequireAuthorization();
    }
    public static async Task<IResult> CreateEmployee(EmployeeCreateModel model, LeavePlannerContext context)
    {
        // Check for invalid data
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Country) || model.Organization == 0 || model.PaidTimeOff == 0)
        {
            return Results.BadRequest("Invalid data.");
        }

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            Employee employee;
            var existingEmployee = await context.Employees
                                           .FirstOrDefaultAsync(e => e.Email == model.Email);
            if (existingEmployee != null)
            {
                existingEmployee.Country = model.Country;
                existingEmployee.Organization = model.Organization;
                existingEmployee.ManagedBy = model.ManagedBy;
                existingEmployee.PaidTimeOff = model.PaidTimeOff;
                existingEmployee.Title = model.Title;
                existingEmployee.Name = model.Name;

                context.Employees.Update(existingEmployee);
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
                context.Employees.Add(employee);
            }
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.ToString());
        }
    }

    public static async Task<IResult> GetEmployee(string email, LeavePlannerContext context)
    {
        var employee = await context.Employees
                                    .FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("User is not an employee.");
        }

        // Fetch subordinates recursively
        var employeeWithSubordinates = await GetEmployeeWithSubordinates(employee.Email, context);

        return Results.Ok(employeeWithSubordinates);
    }

    private static async Task<EmployeeWithSubordinates> GetEmployeeWithSubordinates(string employeeEmail, LeavePlannerContext context)
    {
        var employee = await context.Employees
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

        var subordinates = await context.Employees
                                        .Where(e => e.ManagedBy == employeeEmail)
                                        .ToListAsync();

        foreach (var subordinate in subordinates)
        {
            var subordinateWithSubordinates = await GetEmployeeWithSubordinates(subordinate.Email, context);
            employee.Subordinates.Add(subordinateWithSubordinates);
        }

        return employee;

    }


    public static async Task<IResult> UpdateEmployee(string email, EmployeeUpdateModel model, LeavePlannerContext context)
    {
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("Employee not found.");
        }

        employee.Country = model.Country ?? employee.Country;
        employee.PaidTimeOff = model.PaidTimeOff != 0 ? model.PaidTimeOff : employee.PaidTimeOff;
        employee.Title = model.Title ?? employee.Title;
        employee.Name = model.Name ?? employee.Name;

        try
        {
            await context.SaveChangesAsync();
            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            return Results.Problem("An error occurred while updating the employee.");
        }
    }
    public static async Task<IResult> DeleteEmployee(string email, LeavePlannerContext context)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email == email);

            if (employee == null)
            {
                return Results.NotFound("Employee not found.");
            }

            // Check if the employee is the head of the organization (ManagedBy is null)
            if (employee.ManagedBy == null)
            {
                // Check if the head manages any subordinates
                var subordinates = await context.Employees
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
                var subordinates = await context.Employees
                                                .Where(e => e.ManagedBy == employee.Email)
                                                .ToListAsync();

                foreach (var subordinate in subordinates)
                {
                    subordinate.ManagedBy = employee.ManagedBy; // Reassign to the manager above
                }

                context.Employees.UpdateRange(subordinates);
            }
            // if employee is org owner, can't be deleted completely
            if (employee.IsOrgOwner == true)
            {
                employee.Country = null;
                employee.ManagedBy = null;
                employee.PaidTimeOff = null;
                employee.Title = null;

                context.Employees.Update(employee);


            }
            else
            {
                // Remove the employee from the context
                context.Employees.Remove(employee);
            }
            await context.SaveChangesAsync();
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
