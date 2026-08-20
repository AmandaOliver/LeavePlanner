using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record UpdateOrganizationCommand(int OrganizationId, OrganizationUpdateDTO Organization) : ICommand<Result<Organization>>;

public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, Result<Organization>>
{
	private readonly LeavePlannerContext _context;

	public UpdateOrganizationCommandHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<Organization>> Handle(UpdateOrganizationCommand command, CancellationToken cancellationToken)
	{
		var update = command.Organization;

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

		var organization = await _context.Organizations.FindAsync(new object?[] { command.OrganizationId }, cancellationToken);
		if (organization == null)
		{
			return Result<Organization>.Invalid("Organization not found with that Id");
		}

		try
		{
			if (update.Name == null && update.WorkingDays == null)
			{
				return Result<Organization>.Invalid("name or working days needs to be specified");
			}

			if (update.Name != null)
			{
				organization.Name = update.Name;
			}

			if (update.WorkingDays != null)
			{
				if (update.WorkingDays.Length < 1 || !update.WorkingDays.All(day => day >= 1 && day <= 7))
				{
					return Result<Organization>.Invalid("Working days must be defined.");
				}

				organization.WorkingDays = update.WorkingDays;
			}

			_context.Organizations.Update(organization);
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
