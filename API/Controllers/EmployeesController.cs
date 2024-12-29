using LeavePlanner.Models;
public static class EmployeesEndpointsExtensions
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", async (EmployeesController controller, string email) => await controller.GetEmployee(email))
                 .RequireAuthorization();
        endpoints.MapPost("/employee", async (EmployeesController controller, EmployeeCreateDTO model) => await controller.CreateEmployee(model))
                 .RequireAuthorization();
        endpoints.MapPut("/employee/{id}", async (EmployeesController controller, string id, EmployeeUpdateDTO model) => await controller.UpdateEmployee(id, model))
                 .RequireAuthorization();
        endpoints.MapDelete("/employee/{id}", async (EmployeesController controller, string id) => await controller.DeleteEmployee(id))
                 .RequireAuthorization();
        endpoints.MapPost("/create-employee-organization", async (EmployeesController controller, EmployeeOrganizationCreateDTO model) => await controller.CreateEmployeeAndOrganization(model)).RequireAuthorization();
    }
}
public class EmployeesController
{
    private readonly EmployeesService _employeesService;

    public EmployeesController(EmployeesService employeesService)
    {
        _employeesService = employeesService;

    }
    public async Task<IResult> CreateEmployeeAndOrganization(EmployeeOrganizationCreateDTO model)
    {
        var result = await _employeesService.CreateEmployeeAndOrganization(model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(new { OrganizationId = result.OrganizationId });
    }
    public async Task<IResult> CreateEmployee(EmployeeCreateDTO model)
    {

        var result = await _employeesService.CreateEmployee(model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
    public async Task<IResult> GetEmployee(string email)
    {
        var result = await _employeesService.GetEmployeeByEmail(email);
        if (!result.IsSuccess)
            return Results.NotFound(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
    public async Task<IResult> UpdateEmployee(string id, EmployeeUpdateDTO model)
    {
        var result = await _employeesService.UpdateEmployee(id, model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
    public async Task<IResult> DeleteEmployee(string id)
    {
        var result = await _employeesService.DeleteEmployee(id);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
}
