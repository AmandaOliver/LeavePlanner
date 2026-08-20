using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Organizations.Commands;

public record DeleteOrganizationCommand(string OrganizationId) : ICommand<Result<Organization>>;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<Organization>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;

	public DeleteOrganizationCommandHandler(LeavePlannerContext context, EmployeesService employeesService)
	{
		_context = context;
		_employeesService = employeesService;
	}

	public async Task<Result<Organization>> Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var organization = await _context.Organizations.FindAsync(new object?[] { int.Parse(command.OrganizationId) }, cancellationToken);
			if (organization == null)
			{
				return Result<Organization>.Invalid("Organization not found.");
			}

			var employees = await _context.Employees
				.Where(e => e.Organization.ToString() == command.OrganizationId)
				.ToListAsync(cancellationToken);

			foreach (var employee in employees)
			{
				await _employeesService.DeleteEmployeeWithSubordinates(employee.Id);
			}

			_context.Organizations.Remove(organization);

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return Result<Organization>.Success(organization);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Organization>.Invalid(ex.Message);
		}
	}
}
