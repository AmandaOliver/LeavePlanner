
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
public static class EmployeesController
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", GetEmployee).RequireAuthorization();
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

