
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
public static class EmployeesController
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", GetEmployee).RequireAuthorization();
        endpoints.MapPost("/employee", CreateEmployee).RequireAuthorization();
        endpoints.MapPut("/employee/{email}", UpdateEmployee).RequireAuthorization();


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
            var employee = new Employee
            {
                Email = model.Email,
                Country = model.Country,
                Organization = model.Organization,
                ManagedBy = model.ManagedBy,
                IsOrgOwner = false,
                PaidTimeOff = model.PaidTimeOff,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add employee to context
            context.Employees.Add(employee);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            // Log the exception (ex) as needed
            return Results.Problem("An error occurred while creating the employee.");
        }
    }

    public static async Task<IResult> GetEmployee(string email, LeavePlannerContext context)
    {

        var employee = await context.Employees
                                    .FirstOrDefaultAsync(e => e.Email == email);

        if (employee != null)
        {
            return Results.Ok(employee);
        }
        else
        {
            return Results.NotFound("User is not an employee.");
        }
    }

    public static async Task<IResult> UpdateEmployee(string email, EmployeeUpdateModel model, LeavePlannerContext context)
    {
        // Find the existing employee by email
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("Employee not found.");
        }

        // Update employee fields with provided data
        employee.Country = model.Country ?? employee.Country;
        employee.PaidTimeOff = model.PaidTimeOff != 0 ? model.PaidTimeOff : employee.PaidTimeOff;
        employee.UpdatedAt = DateTime.UtcNow;

        try
        {
            // Save changes to the database
            await context.SaveChangesAsync();
            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            // Log the exception (ex) as needed
            return Results.Problem("An error occurred while updating the employee.");
        }
    }
}

