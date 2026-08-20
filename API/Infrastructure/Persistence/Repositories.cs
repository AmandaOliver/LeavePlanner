using LeavePlanner.Data;
using LeavePlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace LeavePlanner.Infrastructure.Persistence;

public class LeaveRepository : ILeaveRepository
{
	private readonly LeavePlannerContext _context;

	public LeaveRepository(LeavePlannerContext context) => _context = context;

	public async Task<Leave?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
		await _context.Leaves.FindAsync([id], cancellationToken);

	public Task<List<Leave>> GetOwnedByAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves.Where(leave => leave.Owner == ownerId).ToListAsync(cancellationToken);

	public Task<List<Leave>> GetPublicHolidaysOwnedByAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.Type == LeaveTypes.BankHoliday)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetNotRejectedByOwnerAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.RejectedBy == null)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetRejectedByOwnerAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.RejectedBy != null)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetPendingByOwnerAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.ApprovedBy == null && leave.RejectedBy == null)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetReviewedByOwnerAsync(int ownerId, int systemEmployeeId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId &&
							((leave.ApprovedBy != null && leave.ApprovedBy != systemEmployeeId) || leave.RejectedBy != null))
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetApprovedByOwnerAsync(int ownerId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.ApprovedBy != null)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetApprovedUpcomingByOwnerAsync(int ownerId, DateTime asOf, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.ApprovedBy != null && leave.DateStart >= asOf)
			.OrderBy(leave => leave.DateStart)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetApprovedPastByOwnerAsync(int ownerId, DateTime asOf, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.Owner == ownerId && leave.ApprovedBy != null && leave.DateStart < asOf)
			.OrderByDescending(leave => leave.DateStart)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetApprovedInOrganizationAsync(int organizationId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave => leave.ApprovedBy != null && leave.OwnerNavigation!.Organization == organizationId)
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetApprovedPaidTimeOffInYearAsync(int ownerId, int year, int? excludeLeaveId, CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave =>
				leave.Owner == ownerId &&
				leave.Id != excludeLeaveId &&
				leave.Type == LeaveTypes.PaidTimeOff &&
				leave.ApprovedBy != null &&
				(leave.DateStart.Year == year || leave.DateEnd.Year == year))
			.ToListAsync(cancellationToken);

	public Task<List<Leave>> GetBlockingLeavesAsync(
		int ownerId,
		DateTime start,
		DateTime end,
		DateTime asOf,
		int? excludeLeaveId,
		CancellationToken cancellationToken) =>
		_context.Leaves
			.Where(leave =>
				leave.Owner == ownerId &&
				(leave.ApprovedBy != null || leave.Type == LeaveTypes.BankHoliday) &&
				(excludeLeaveId == null || leave.Id != excludeLeaveId) &&
				leave.CreatedAt < asOf &&
				(
					(start >= leave.DateStart && start < leave.DateEnd) ||
					(end > leave.DateStart && end <= leave.DateEnd) ||
					(start < leave.DateStart && end > leave.DateEnd)
				))
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
		await _context.Employees.FindAsync([id], cancellationToken);

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
		await _context.Organizations.FindAsync([id], cancellationToken);

	public Task<Organization?> GetByIdWithEmployeesAsync(int id, CancellationToken cancellationToken) =>
		_context.Organizations
			.Include(organization => organization.Employees)
			.FirstOrDefaultAsync(organization => organization.Id == id, cancellationToken);

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
