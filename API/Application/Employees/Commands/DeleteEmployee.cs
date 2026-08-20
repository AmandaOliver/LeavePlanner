using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Employees.Commands;

public record DeleteEmployeeCommand(string EmployeeId) : ICommand<Result<Employee>>;

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;

	public DeleteEmployeeCommandHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<Employee>> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _context.Employees.FindAsync(new object?[] { int.Parse(request.EmployeeId) }, cancellationToken);

			if (employee == null)
			{
				return Result<Employee>.NotFound("Employee not found.");
			}

			var subordinates = await _context.Employees
				.Where(e => e.ManagedBy == employee.Id)
				.ToListAsync(cancellationToken);

			if (employee.ManagedBy == null)
			{
				if (subordinates.Any())
				{
					return Result<Employee>.Invalid("Cannot delete the head of the organization because they manage other employees.");
				}
			}
			else
			{
				foreach (var subordinate in subordinates)
				{
					subordinate.ManagedBy = employee.ManagedBy;
				}

				_context.Employees.UpdateRange(subordinates);
			}

			var leaves = await _context.Leaves.Where(l => l.Owner == employee.Id).ToListAsync(cancellationToken);
			_context.Leaves.RemoveRange(leaves);

			if (employee.IsOrgOwner == true)
			{
				employee.Country = null;
				employee.ManagedBy = null;
				employee.PaidTimeOff = 0;
				employee.Title = null;

				_context.Employees.Update(employee);
			}
			else
			{
				_context.Employees.Remove(employee);
			}

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return Result<Employee>.Success(employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Employee>.Invalid(ex.Message);
		}
	}
}
