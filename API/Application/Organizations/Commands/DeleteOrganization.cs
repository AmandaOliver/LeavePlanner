using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record DeleteOrganizationCommand(string OrganizationId) : ICommand<Result<Organization>>;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<Organization>>
{
	private readonly LeavePlannerContext _context;
	private readonly IOrganizationRepository _organizations;
	private readonly IEmployeeRepository _employees;
	private readonly EmployeeHierarchy _hierarchy;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteOrganizationCommandHandler(
		LeavePlannerContext context,
		IOrganizationRepository organizations,
		IEmployeeRepository employees,
		EmployeeHierarchy hierarchy,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_organizations = organizations;
		_employees = employees;
		_hierarchy = hierarchy;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Organization>> Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
	{
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var organization = await _organizations.GetByIdAsync(int.Parse(command.OrganizationId), cancellationToken);
			if (organization == null)
			{
				return Result<Organization>.Invalid("Organization not found.");
			}

			var employees = await _employees.GetByOrganizationAsync(organization.Id, cancellationToken);
			foreach (var employee in employees)
			{
				await _hierarchy.DeleteWithSubordinates(employee.Id, cancellationToken);
			}

			_organizations.Remove(organization);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
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
