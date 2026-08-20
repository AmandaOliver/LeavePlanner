using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record CreateEmployeeCommand(EmployeeCreateDTO Employee) : ICommand<Result<EmployeeDTO>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ICountryRepository _countries;
	private readonly ILeaveRepository _leaves;
	private readonly PublicHolidayGenerator _holidays;
	private readonly IUnitOfWork _unitOfWork;

	public CreateEmployeeCommandHandler(
		IEmployeeRepository employees,
		ICountryRepository countries,
		ILeaveRepository leaves,
		PublicHolidayGenerator holidays,
		IUnitOfWork unitOfWork)
	{
		_employees = employees;
		_countries = countries;
		_leaves = leaves;
		_holidays = holidays;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EmployeeDTO>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
	{
		var model = request.Employee;
		try
		{
			await AssertCanHire(model, cancellationToken);
		}
		catch (DomainException ex)
		{
			return Result<EmployeeDTO>.Invalid(ex.Message);
		}

		await _unitOfWork.BeginTransactionAsync(cancellationToken);
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
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result<EmployeeDTO>.Success(employee.ToEmployeeDto());
		}
		catch (DomainException ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<EmployeeDTO>.Invalid(ex.Message);
		}
		catch
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			throw;
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
			model.Email, model.Name, model.Title, model.Country, model.PaidTimeOff, existing, manager, country != null, model.Organization);
	}
}
