using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Application.Organizations.Queries;

public record GetOrganizationQuery(string OrganizationId) : IQuery<Result<OrganizationTree>>;

public class GetOrganizationQueryHandler : IRequestHandler<GetOrganizationQuery, Result<OrganizationTree>>
{
	private readonly LeavePlannerContext _context;

	public GetOrganizationQueryHandler(LeavePlannerContext context) => _context = context;

	public async Task<Result<OrganizationTree>> Handle(GetOrganizationQuery request, CancellationToken cancellationToken)
	{
		var organization = await _context.Organizations
			.Include(o => o.Employees)
			.FirstOrDefaultAsync(e => e.Id.ToString() == request.OrganizationId, cancellationToken);

		if (organization == null)
		{
			return Result<OrganizationTree>.NotFound("Organization does not exists.");
		}

		var employees = organization.Employees.ToList();
		var roots = employees.Where(e => e.ManagedBy == null && e.Country != null).ToList();

		return Result<OrganizationTree>.Success(new OrganizationTree
		{
			Id = organization.Id,
			Name = organization.Name,
			WorkingDays = organization.WorkingDays,
			Tree = BuildEmployeeHierarchy(roots, employees)
		});
	}

	private static List<EmployeeWithSubordinatesDTO> BuildEmployeeHierarchy(List<Employee> managers, List<Employee> allEmployees) =>
		managers.Select(manager => new EmployeeWithSubordinatesDTO
		{
			Id = manager.Id,
			Name = manager.Name,
			Email = manager.Email,
			Country = manager.Country,
			PaidTimeOff = manager.PaidTimeOff,
			ManagedBy = manager.ManagedBy,
			Title = manager.Title,
			IsOrgOwner = manager.IsOrgOwner,
			Subordinates = BuildEmployeeHierarchy(allEmployees.Where(e => e.ManagedBy == manager.Id).ToList(), allEmployees)
		}).ToList();
}
