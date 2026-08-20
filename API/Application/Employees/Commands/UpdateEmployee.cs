using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Employees.Commands;

public record UpdateEmployeeCommand(string EmployeeId, EmployeeUpdateDTO Employee) : ICommand<Result<Employee>>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;
	private readonly CountriesService _countriesService;

	public UpdateEmployeeCommandHandler(LeavePlannerContext context, CountriesService countriesService)
	{
		_context = context;
		_countriesService = countriesService;
	}

	public async Task<Result<Employee>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
	{
		var model = request.Employee;

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _context.Employees.FindAsync(new object?[] { int.Parse(request.EmployeeId) }, cancellationToken);

			if (employee == null)
			{
				return Result<Employee>.NotFound("Employee not found.");
			}

			if (employee.Country != model.Country)
			{
				var leaves = await _context.Leaves
					.Where(l => l.Owner == employee.Id && l.Type == "bankHoliday")
					.ToListAsync(cancellationToken);
				_context.Leaves.RemoveRange(leaves);
				employee.Country = model.Country;
				await _countriesService.GenerateEmployeeBankHolidays(employee);
			}

			employee.PaidTimeOff = model.PaidTimeOff != 0 ? model.PaidTimeOff : employee.PaidTimeOff;
			employee.Title = model.Title ?? employee.Title;
			employee.Name = model.Name ?? employee.Name;
			employee.Email = model.Email ?? employee.Email;

			if (employee.IsOrgOwner == true && model.IsOrgOwner == false)
			{
				var anotherOwner = await _context.Employees.FirstOrDefaultAsync(
					e => e.IsOrgOwner == true && employee.Organization == e.Organization && employee.Email != e.Email,
					cancellationToken);
				if (anotherOwner == null)
				{
					await transaction.RollbackAsync(cancellationToken);
					return Result<Employee>.Invalid("You can't leave the organization without admins");
				}
			}

			employee.IsOrgOwner = model.IsOrgOwner;

			await transaction.CommitAsync(cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
			return Result<Employee>.Success(employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Employee>.Invalid(ex.Message);
		}
	}
}
