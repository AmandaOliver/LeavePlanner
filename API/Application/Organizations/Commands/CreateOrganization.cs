using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record CreateOrganizationCommand(OrganizationCreateDTO Organization) : ICommand<Result<int>>;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<int>>
{
	private readonly LeavePlannerContext _context;

	public CreateOrganizationCommandHandler(LeavePlannerContext context) => _context = context;

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
			var organization = new Organization { Name = model.OrganizationName };
			_context.Organizations.Add(organization);
			await _context.SaveChangesAsync(cancellationToken);

			_context.Employees.Add(new Employee
			{
				Email = model.Email,
				IsOrgOwner = true,
				Organization = organization.Id
			});

			await transaction.CommitAsync(cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);

			return Result<int>.Success(organization.Id);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<int>.Invalid(ex.Message);
		}
	}
}
