using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record DeleteEmployeeCommand(string EmployeeId) : ICommand<Result<EmployeeDTO>>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<EmployeeDTO>>
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteEmployeeCommandHandler(
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		IUnitOfWork unitOfWork)
	{
		_employees = employees;
		_leaves = leaves;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<EmployeeDTO>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
	{
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			if (!int.TryParse(request.EmployeeId, out var employeeId))
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<EmployeeDTO>.Invalid("Invalid employee id.");
			}

			var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
			if (employee == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<EmployeeDTO>.NotFound("Employee not found.");
			}

			var subordinates = await _employees.GetDirectReportsAsync(employee.Id, cancellationToken);
			employee.EnsureCanBeDeleted(subordinates.Count > 0);

			foreach (var subordinate in subordinates)
			{
				subordinate.ReassignReportsTo(employee.ManagedBy);
			}

			_leaves.RemoveRange(await _leaves.GetOwnedByAsync(employee.Id, cancellationToken));

			if (employee.IsOrgOwner)
			{
				employee.DeactivateAsOwner();
			}
			else
			{
				_employees.Remove(employee);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);

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
}
