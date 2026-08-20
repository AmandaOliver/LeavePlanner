using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record CreateOrganizationCommand(OrganizationCreateDTO Organization) : ICommand<Result<int>>;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<int>>
{
	private readonly LeavePlannerContext _context;
	private readonly IOrganizationRepository _organizations;
	private readonly IEmployeeRepository _employees;
	private readonly IUnitOfWork _unitOfWork;

	public CreateOrganizationCommandHandler(
		LeavePlannerContext context,
		IOrganizationRepository organizations,
		IEmployeeRepository employees,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_organizations = organizations;
		_employees = employees;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<int>> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken)
	{
		var model = command.Organization;
		if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OrganizationName))
		{
			return Result<int>.Invalid("Invalid data.");
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var organization = Organization.Create(model.OrganizationName);
			_organizations.Add(organization);
			await _unitOfWork.SaveChangesAsync(cancellationToken);

			_employees.Add(Employee.CreateOwner(model.Email, organization.Id));
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return Result<int>.Success(organization.Id);
		}
		catch (DomainException ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<int>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<int>.Invalid(ex.Message);
		}
	}
}
