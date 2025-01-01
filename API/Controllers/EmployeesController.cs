using LeavePlanner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("employee")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeesService _employeesService;

    public EmployeesController(EmployeesService employeesService)
    {
        _employeesService = employeesService;

    }

    [HttpPost]
    [Authorize]
    public async Task<IResult> CreateEmployee([FromBody] EmployeeCreateDTO model)
    {

        var result = await _employeesService.CreateEmployee(model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }

    [HttpGet("{email}")]
    [Authorize]
    public async Task<IResult> GetEmployee(string email)
    {
        var result = await _employeesService.GetEmployeeByEmail(email);
        if (!result.IsSuccess)
            return Results.NotFound(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IResult> UpdateEmployee(string id, [FromBody] EmployeeUpdateDTO model)
    {
        var result = await _employeesService.UpdateEmployee(id, model);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IResult> DeleteEmployee(string id)
    {
        var result = await _employeesService.DeleteEmployee(id);
        if (!result.IsSuccess)
            return Results.BadRequest(result.ErrorMessage);

        return Results.Ok(result.Employee);
    }
}
