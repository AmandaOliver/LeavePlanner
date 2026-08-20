using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record UpdateOrganizationCommand(int OrganizationId, OrganizationUpdateDTO Organization) : ICommand<Result<OrganizationDTO>>;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<OrganizationDTO>>
{
	private readonly IOrganizationRepository _organizations;
	private readonly IUnitOfWork _unitOfWork;

	public UpdateOrganizationCommandHandler(
		IOrganizationRepository organizations,
		IUnitOfWork unitOfWork)
	{
		_organizations = organizations;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result<OrganizationDTO>> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
	{
		var update = command.Organization;
		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			var organization = await _organizations.GetByIdAsync(command.OrganizationId, cancellationToken);
			if (organization == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<OrganizationDTO>.Invalid("Organization not found with that Id");
			}

			if (update.Name == null && update.WorkingDays == null)
			{
				await _unitOfWork.RollbackTransactionAsync(cancellationToken);
				return Result<OrganizationDTO>.Invalid("name or working days needs to be specified");
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
