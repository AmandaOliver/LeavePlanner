using LeavePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[Authorize]
[ApiController]
[Route("employee")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeesService _employeesService;

    public EmployeesController(EmployeesService employeesService)
    {
        _employeesService = employeesService;

    }

    [AdminOnly]
    [HttpPost]
    public async Task<IResult> CreateEmployee([FromBody] EmployeeCreateDTO model)
    {

        var result = await _employeesService.CreateEmployee(model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }

    [SelfEmailOrAdminOnly]
    [HttpGet("{email}")]
    public async Task<IResult> GetEmployee(string email)
    {
        var result = await _employeesService.GetEmployeeByEmail(email);
        if (!result.IsSuccess)
            return Results.NotFound(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }

    [AdminOnly]
    [HttpPut("{id}")]
    public async Task<IResult> UpdateEmployee(string id, [FromBody] EmployeeUpdateDTO model)
    {
        var result = await _employeesService.UpdateEmployee(id, model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }

    [AdminOnly]
    [HttpDelete("{id}")]
    public async Task<IResult> DeleteEmployee(string id)
    {
        var result = await _employeesService.DeleteEmployee(id);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
}
