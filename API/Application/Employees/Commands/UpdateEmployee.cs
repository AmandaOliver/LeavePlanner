using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record UpdateEmployeeCommand(string EmployeeId, EmployeeUpdateDTO Employee) : ICommand<Result<Employee>>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly PublicHolidayGenerator _holidays;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateEmployeeCommandHandler(
		LeavePlannerContext context,
		IEmployeeRepository employees,
		PublicHolidayGenerator holidays,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_employees = employees;
		_holidays = holidays;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Employee>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _employees.GetByIdAsync(int.Parse(request.EmployeeId), cancellationToken);
			if (employee == null)
			{
				return Result<Employee>.NotFound("Employee not found.");
			}

			var model = request.Employee;
			if (employee.ChangeCountry(model.Country))
			{
				await _holidays.ReplaceFor(employee, cancellationToken);
			}

			if (employee.IsOrgOwner && !model.IsOrgOwner)
			{
				employee.EnsureOrganizationKeepsAnAdmin(
					await _employees.AnotherOwnerExistsAsync(employee, cancellationToken));
			}

			employee.UpdateDetails(model.Email, model.Name, model.Title, model.PaidTimeOff, model.IsOrgOwner);

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

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
}
