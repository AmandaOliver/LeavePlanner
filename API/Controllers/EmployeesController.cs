using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees.Commands;
using LeavePlanner.Application.Employees.Queries;
using LeavePlanner.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("employee")]
public class EmployeesController : ControllerBase
{
	private readonly IMediator _mediator;

	public EmployeesController(IMediator mediator) => _mediator = mediator;

	[AdminOnly]
	[HttpPost]
	public async Task<IResult> CreateEmployee([FromBody] EmployeeCreateDTO model) =>
		(await _mediator.Send(new CreateEmployeeCommand(model))).ToHttpResult();

	[SelfEmailOrAdminOnly]
	[HttpGet("{email}")]
	public async Task<IResult> GetEmployee(string email) =>
		(await _mediator.Send(new GetEmployeeByEmailQuery(email))).ToHttpResult();

	[AdminOnly]
	[HttpPut("{id}")]
	public async Task<IResult> UpdateEmployee(string id, [FromBody] EmployeeUpdateDTO model) =>
		(await _mediator.Send(new UpdateEmployeeCommand(id, model))).ToHttpResult();

	[AdminOnly]
	[HttpDelete("{id}")]
	public async Task<IResult> DeleteEmployee(string id) =>
		(await _mediator.Send(new DeleteEmployeeCommand(id))).ToHttpResult();
}
