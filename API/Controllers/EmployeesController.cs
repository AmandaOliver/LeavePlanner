
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
public static class EmployeesController
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{id}", GetEmployee).RequireAuthorization();
    }
	public static async Task<IResult> GetEmployee(string id, LeavePlannerContext context)
    {

        var employee = await context.Employees
                                    .FirstOrDefaultAsync(e => e.Id == id);

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

