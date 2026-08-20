using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record CreateEmployeeCommand(EmployeeCreateDTO Employee) : ICommand<Result<Employee>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly ICountryRepository _countries;
	private readonly ILeaveRepository _leaves;
	private readonly PublicHolidayGenerator _holidays;
	private readonly IUnitOfWork _unitOfWork;

	public CreateEmployeeCommandHandler(
		LeavePlannerContext context,
		IEmployeeRepository employees,
		ICountryRepository countries,
		ILeaveRepository leaves,
		PublicHolidayGenerator holidays,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_employees = employees;
		_countries = countries;
		_leaves = leaves;
		_holidays = holidays;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Employee>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
	{
		var model = request.Employee;
		try
		{
			await AssertCanHire(model, cancellationToken);
		}
		catch (DomainException ex)
		{
			return Result<Employee>.Invalid(ex.Message);
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var existing = await _employees.GetByEmailAsync(model.Email, cancellationToken);
			Employee employee;
			if (existing != null)
			{
				_leaves.RemoveRange(await _leaves.GetPublicHolidaysOwnedByAsync(existing.Id, cancellationToken));
				existing.Reactivate(model.Name, model.Title, model.Country, model.Organization, model.ManagedBy, model.PaidTimeOff);
				employee = existing;
			}
			else
			{
				employee = Employee.Hire(
					model.Email, model.Name, model.Title, model.Country, model.Organization, model.ManagedBy, model.PaidTimeOff, model.IsOrgOwner);
				_employees.Add(employee);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _holidays.GenerateFor(employee, cancellationToken);
			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<Employee>.Success(employee);
		}
		catch (DomainException ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Employee>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Employee>.Invalid(ex.Message);
		}
	}

	private async Task AssertCanHire(EmployeeCreateDTO model, CancellationToken cancellationToken)
	{
		var existing = await _employees.GetByEmailAsync(model.Email, cancellationToken);
		Employee? manager = null;
		if (model.ManagedBy != null)
		{
			manager = await _employees.GetByIdAsync(model.ManagedBy.Value, cancellationToken);
			EmployeePolicy.AssertManagerExists(manager, true);
		}

		var country = await _countries.GetByNameAsync(model.Country, cancellationToken);
		EmployeePolicy.AssertCanHire(
			model.Email, model.Name, model.Title, model.Country, model.PaidTimeOff, existing, manager, country != null);
	}
}
