using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record UpdateEmployeeCommand(string EmployeeId, EmployeeUpdateDTO Employee) : ICommand<Result<EmployeeDTO>>;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly PublicHolidayGenerator _holidays;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateEmployeeCommandHandler(
		IEmployeeRepository employees,
		PublicHolidayGenerator holidays,
		IUnitOfWork unitOfWork)
	{
		_employees = employees;
		_holidays = holidays;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EmployeeDTO>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
	{
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _employees.GetByIdAsync(int.Parse(request.EmployeeId), cancellationToken);
			if (employee == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<EmployeeDTO>.NotFound("Employee not found.");
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
			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			return Result<EmployeeDTO>.Success(employee.ToEmployeeDto());
		}
		catch (DomainException ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<EmployeeDTO>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<EmployeeDTO>.Invalid(ex.Message);
		}
	}
}
