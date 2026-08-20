using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Employees.Queries;

public record GetEmployeeByEmailQuery(string Email) : IQuery<Result<EmployeeWithSubordinatesDTO>>;

public class GetEmployeeByEmailQueryHandler : IRequestHandler<GetEmployeeByEmailQuery, Result<EmployeeWithSubordinatesDTO>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;

	public GetEmployeeByEmailQueryHandler(LeavePlannerContext context, EmployeesService employeesService)
	{
		_context = context;
		_employeesService = employeesService;
	}

	public async Task<Result<EmployeeWithSubordinatesDTO>> Handle(GetEmployeeByEmailQuery request, CancellationToken cancellationToken)
	{
		var employee = await _context.Employees
			.FirstOrDefaultAsync(e => e.Email == request.Email, cancellationToken);

		if (employee == null)
		{
			return Result<EmployeeWithSubordinatesDTO>.NotFound("User is not an employee.");
		}

		var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(employee);
		return Result<EmployeeWithSubordinatesDTO>.Success(employeeWithSubordinates);
	}
}
