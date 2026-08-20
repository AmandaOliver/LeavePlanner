using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using MediatR;

namespace LeavePlanner.Application.Employees.Commands;

public record DeleteEmployeeCommand(string EmployeeId) : ICommand<Result<Employee>>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteEmployeeCommandHandler(
		LeavePlannerContext context,
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_employees = employees;
		_leaves = leaves;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Employee>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _employees.GetByIdAsync(int.Parse(request.EmployeeId), cancellationToken);
			if (employee == null)
			{
				return Result<Employee>.NotFound("Employee not found.");
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
