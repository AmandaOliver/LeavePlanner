using LeavePlanner.Data;
using LeavePlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Infrastructure.Persistence;

public class LeaveRepository : ILeaveRepository
{
	private readonly LeavePlannerContext _context;

	public LeaveRepository(LeavePlannerContext context) => _context = context;

	public async Task<Leave?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
		await _context.Leaves.FindAsync(new object[] { id }, cancellationToken);

	public Task<List<Leave>> GetOwnedByAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves.Where(leave => leave.Owner == ownerId).ToListAsync(cancellationToken);

	public Task<List<Leave>> GetPublicHolidaysOwnedByAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.Type == LeaveTypes.BankHoliday)
			.ToListAsync(cancellationToken);

	public void Add(Leave leave) => _context.Leaves.Add(leave);

	public void Remove(Leave leave) => _context.Leaves.Remove(leave);

	public void RemoveRange(IEnumerable<Leave> leaves) => _context.Leaves.RemoveRange(leaves);
}

public class EmployeeRepository : IEmployeeRepository
{
	private readonly LeavePlannerContext _context;

	public EmployeeRepository(LeavePlannerContext context) => _context = context;

	public async Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
		await _context.Employees.FindAsync(new object[] { id }, cancellationToken);

	public Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
		_context.Employees.FirstOrDefaultAsync(e => e.Email == email, cancellationToken);

	public async Task<Employee> GetSystemAsync(CancellationToken cancellationToken)
	{
		var system = await _context.Employees.FirstOrDefaultAsync(e => e.Email == SystemActor.Email, cancellationToken);
		if (system == null)
		{
			throw new DomainException("System employee not found");
		}

		return system;
	}

	public Task<List<Employee>> GetByOrganizationAsync(int organizationId, CancellationToken cancellationToken) =>
		_context.Employees.Where(e => e.Organization == organizationId).ToListAsync(cancellationToken);

	public Task<List<Employee>> GetDirectReportsAsync(int managerId, CancellationToken cancellationToken) =>
		_context.Employees.Where(e => e.ManagedBy == managerId).ToListAsync(cancellationToken);

	public Task<bool> AnotherOwnerExistsAsync(Employee employee, CancellationToken cancellationToken) =>
		_context.Employees.AnyAsync(
			e => e.IsOrgOwner && employee.Organization == e.Organization && employee.Email != e.Email,
			cancellationToken);

	public void Add(Employee employee) => _context.Employees.Add(employee);

	public void Remove(Employee employee) => _context.Employees.Remove(employee);
}

public class OrganizationRepository : IOrganizationRepository
{
	private readonly LeavePlannerContext _context;

	public OrganizationRepository(LeavePlannerContext context) => _context = context;

	public async Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
		await _context.Organizations.FindAsync(new object[] { id }, cancellationToken);

	public void Add(Organization organization) => _context.Organizations.Add(organization);

	public void Remove(Organization organization) => _context.Organizations.Remove(organization);
}

public class CountryRepository : ICountryRepository
{
	private readonly LeavePlannerContext _context;

	public CountryRepository(LeavePlannerContext context) => _context = context;

	public Task<Country?> GetByNameAsync(string name, CancellationToken cancellationToken) =>
		_context.Countries.FirstOrDefaultAsync(country => country.Name == name, cancellationToken);

	public Task<List<Country>> GetAllAsync(CancellationToken cancellationToken) =>
		_context.Countries.ToListAsync(cancellationToken);
}
