namespace LeavePlanner.Domain;

public interface ILeaveRepository
{
	Task<Leave?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<List<Leave>> GetOwnedByAsync(int ownerId, CancellationToken cancellationToken);
	Task<List<Leave>> GetPublicHolidaysOwnedByAsync(int ownerId, CancellationToken cancellationToken);
	void Add(Leave leave);
	void Remove(Leave leave);
	void RemoveRange(IEnumerable<Leave> leaves);
}

public interface IEmployeeRepository
{
	Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<Employee?> GetByEmailAsync(string email, CancellationToken cancellationToken);
	Task<Employee> GetSystemAsync(CancellationToken cancellationToken);
	Task<List<Employee>> GetByOrganizationAsync(int organizationId, CancellationToken cancellationToken);
	Task<List<Employee>> GetDirectReportsAsync(int managerId, CancellationToken cancellationToken);
	Task<bool> AnotherOwnerExistsAsync(Employee employee, CancellationToken cancellationToken);
	void Add(Employee employee);
	void Remove(Employee employee);
}

public interface IOrganizationRepository
{
	Task<Organization?> GetByIdAsync(int id, CancellationToken cancellationToken);
	void Add(Organization organization);
	void Remove(Organization organization);
}

public interface ICountryRepository
{
	Task<Country?> GetByNameAsync(string name, CancellationToken cancellationToken);
	Task<List<Country>> GetAllAsync(CancellationToken cancellationToken);
}

public interface IEmailSender
{
	Task SendAsync(string toEmail, string subject, string body);
}

public interface IPublicHolidayCalendar
{
	Task<List<PublicHoliday>> GetUpcomingAsync(string countryCode, CancellationToken cancellationToken);
}
