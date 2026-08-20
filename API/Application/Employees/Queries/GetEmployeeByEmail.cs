using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Queries;

public record GetEmployeeByEmailQuery(string Email) : IQuery<Result<EmployeeWithSubordinatesDTO>>;

public class GetEmployeeByEmailQueryHandler : IRequestHandler<GetEmployeeByEmailQuery, Result<EmployeeWithSubordinatesDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly EmployeeHierarchy _hierarchy;

	public GetEmployeeByEmailQueryHandler(IEmployeeRepository employees, EmployeeHierarchy hierarchy)
	{
		_employees = employees;
		_hierarchy = hierarchy;
	}

	public async Task<Result<EmployeeWithSubordinatesDTO>> Handle(GetEmployeeByEmailQuery request, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByEmailAsync(request.Email, cancellationToken);
		if (employee == null)
		{
			return Result<EmployeeWithSubordinatesDTO>.NotFound("User is not an employee.");
		}

		return Result<EmployeeWithSubordinatesDTO>.Success(await _hierarchy.GetWithSubordinates(employee, cancellationToken));
	}
}
