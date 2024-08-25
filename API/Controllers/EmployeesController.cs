
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
public static class EmployeesController
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", GetEmployee).RequireAuthorization();
        endpoints.MapPost("/employee", CreateEmployee).RequireAuthorization();

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
                IsManager = model.IsManager,
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
}

