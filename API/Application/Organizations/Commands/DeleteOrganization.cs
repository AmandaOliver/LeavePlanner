using LeavePlanner.Application.Common;
using LeavePlanner.Application.Employees;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record DeleteOrganizationCommand(string OrganizationId) : ICommand<Result<OrganizationDTO>>;

public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Result<OrganizationDTO>>
{
	private readonly IOrganizationRepository _organizations;
	private readonly IEmployeeRepository _employees;
	private readonly EmployeeHierarchy _hierarchy;
	private readonly IUnitOfWork _unitOfWork;

	public DeleteOrganizationCommandHandler(
		IOrganizationRepository organizations,
		IEmployeeRepository employees,
		EmployeeHierarchy hierarchy,
		IUnitOfWork unitOfWork)
	{
		_organizations = organizations;
		_employees = employees;
		_hierarchy = hierarchy;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<OrganizationDTO>> Handle(DeleteOrganizationCommand command, CancellationToken cancellationToken)
	{
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			if (!int.TryParse(command.OrganizationId, out var organizationId))
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<OrganizationDTO>.Invalid("Invalid organization id.");
			}

			var organization = await _organizations.GetByIdAsync(organizationId, cancellationToken);
			if (organization == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<OrganizationDTO>.Invalid("Organization not found.");
			}

			var employees = await _employees.GetByOrganizationAsync(organization.Id, cancellationToken);
			foreach (var employee in employees)
			{
				await _hierarchy.DeleteWithSubordinates(employee.Id, cancellationToken);
			}

			_organizations.Remove(organization);
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);

			return Result<OrganizationDTO>.Success(organization.ToOrganizationDto());
		}
		catch (DomainException ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result<OrganizationDTO>.Invalid(ex.Message);
		}
		catch
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			throw;
		}
	}
}
