using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record UpdateOrganizationCommand(int OrganizationId, OrganizationUpdateDTO Organization) : ICommand<Result<Organization>>;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<Organization>>
{
	private readonly LeavePlannerContext _context;
	private readonly IOrganizationRepository _organizations;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateOrganizationCommandHandler(
		LeavePlannerContext context,
		IOrganizationRepository organizations,
		IUnitOfWork unitOfWork)
	{
		_context = context;
		_organizations = organizations;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<Organization>> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
	{
		var update = command.Organization;
		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var organization = await _organizations.GetByIdAsync(command.OrganizationId, cancellationToken);
			if (organization == null)
			{
				return Result<Organization>.Invalid("Organization not found with that Id");
			}

			if (update.Name == null && update.WorkingDays == null)
			{
				return Result<Organization>.Invalid("name or working days needs to be specified");
			}

			if (update.Name != null)
			{
				organization.Rename(update.Name);
			}

			if (update.WorkingDays != null)
			{
				organization.ChangeWorkingDays(update.WorkingDays);
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return Result<Organization>.Success(organization);
		}
		catch (DomainException ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Organization>.Invalid(ex.Message);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Organization>.Invalid(ex.Message);
		}
	}
}
