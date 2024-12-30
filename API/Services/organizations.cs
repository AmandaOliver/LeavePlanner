using CsvHelper;
using System.Globalization;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
public class OrganizationTree
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public int[]? WorkingDays { get; set; }
	public object? Tree { get; set; }


}
public class OrganizationsService
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly CountriesService _countriesService;
	private readonly string _leavePlannerUrl;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;


	public OrganizationsService(LeavePlannerContext context, EmployeesService employeesService, CountriesService countriesService, IConfiguration configuration, EmailService emailService)
	{
		_context = context;
		_employeesService = employeesService;
		_countriesService = countriesService;
		_emailService = emailService;
		_configuration = configuration;

		// Access the LeavePlannerUrl from appsettings.json
		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

	}
	private List<EmployeeWithSubordinatesDTO> BuildEmployeeHierarchy(List<Employee> managers, List<Employee> allEmployees)
	{
		var result = new List<EmployeeWithSubordinatesDTO>();

		foreach (var manager in managers)
		{
			var subordinates = allEmployees.Where(e => e.ManagedBy == manager.Id).ToList();
			var employeeDto = new EmployeeWithSubordinatesDTO
			{
				Id = manager.Id,
				Name = manager.Name,
				Email = manager.Email,
				Country = manager.Country,
				PaidTimeOff = manager.PaidTimeOff,
				ManagedBy = manager.ManagedBy,
				Title = manager.Title,
				IsOrgOwner = manager.IsOrgOwner,
				Subordinates = BuildEmployeeHierarchy(subordinates, allEmployees)
			};
			result.Add(employeeDto);
		}

		return result;
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Organization? organization)> UpdateOrganization(int organizationId, OrganizationUpdateDTO organizationUpdate)
	{

		using var transaction = await _context.Database.BeginTransactionAsync();
		var organization = await _context.Organizations.FindAsync(organizationId);
		if (organization == null)
		{
			return (false, "Organization not found with that Id", null);
		}
		try
		{
			if (organizationUpdate.Name == null && organizationUpdate.WorkingDays == null)
			{
				return (false, "name or working days needs to be specified", null);
			}
			if (organizationUpdate.Name != null)
			{
				organization.Name = organizationUpdate.Name;
			}
			if (organizationUpdate.WorkingDays != null)
			{
				if (organizationUpdate.WorkingDays is not IEnumerable<int> || organizationUpdate.WorkingDays.Length < 1 || !organizationUpdate.WorkingDays.All(day => day >= 1 && day <= 7))
				{
					return (false, "Working days must be defined.", null);
				}
				organization.WorkingDays = organizationUpdate.WorkingDays;
			}


			_context.Organizations.Update(organization);
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			return (true, null, organization);

		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, OrganizationTree? organization)> GetOrganization(string id)
	{
		try
		{
			var organization = await _context.Organizations.Include(o => o.Employees)
										.FirstOrDefaultAsync(e => e.Id.ToString() == id);
			if (organization != null)
			{
				var employeeTree = BuildEmployeeHierarchy(organization.Employees.Where(e => e.ManagedBy == null && e.Country != null).ToList(), organization.Employees.ToList());
				var result = new OrganizationTree
				{
					Id = organization.Id,
					Name = organization.Name,
					WorkingDays = organization.WorkingDays,
					Tree = employeeTree
				};
				return (true, null, result);
			}
			else
			{
				return (false, "Organization does not exists.", null);
			}
		}
		catch (Exception ex)
		{
			return (false, "Organization does not exists.", null);
		}

	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Organization? organization)> DeleteOrganization(string id)
	{
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var organization = await _context.Organizations.FindAsync(int.Parse(id));
			if (organization == null)
			{
				return (false, "Organization not found.", null);
			}

			var employees = await _context.Employees
				.Where(e => e.Organization.ToString() == id)
				.ToListAsync();

			foreach (var employee in employees)
			{
				await _employeesService.DeleteEmployeeWithSubordinates(employee.Id);
			}

			_context.Organizations.Remove(organization);

			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			return (false, "Organization and related data deleted successfully.", null);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task ImportOrganizationHierarchy(string organizationId, Stream fileStream)
	{
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var employees = new List<EmployeeCsvModel>();

			try
			{
				using var reader = new StreamReader(fileStream);
				using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
				employees = csv.GetRecords<EmployeeCsvModel>().ToList();
			}
			catch (Exception ex)
			{
				throw new Exception("Error parsing the CSV, verify the structure.");
			}
			var headFound = false;
			// Step 1: Insert employees without setting managedBy
			foreach (var employee in employees)
			{
				if (string.IsNullOrEmpty(employee.ManagerEmail))
				{
					if (headFound == true)
					{
						throw new Exception("Error in organization structure, you can't have two heads (employees without manager)");
					}
					headFound = true;
				}

				var validationResult = await _employeesService.ValidateEmployee(new EmployeeCreateDTO
				{
					Email = employee.Email,
					Name = employee.Name,
					Title = employee.Title,
					Country = employee.Country,
					PaidTimeOff = employee.PaidTimeOff,
					Organization = int.Parse(organizationId),
					IsOrgOwner = employee.IsAdmin
				});
				if (validationResult != "success")
				{
					throw new Exception("Error in Employee " + employee.Email + ": " + validationResult);

				}
				var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
				if (existingEmployee == null)
				{
					var newEmployee = new Employee
					{
						Email = employee.Email,
						Name = employee.Name,
						Title = employee.Title,
						Country = employee.Country,
						PaidTimeOff = employee.PaidTimeOff,
						Organization = int.Parse(organizationId),
						IsOrgOwner = employee.IsAdmin
					};
					_context.Employees.Add(newEmployee);
				}
				else
				{
					existingEmployee.Country = employee.Country;
					existingEmployee.Organization = int.Parse(organizationId);
					existingEmployee.PaidTimeOff = employee.PaidTimeOff;
					existingEmployee.Title = employee.Title;
					existingEmployee.Name = employee.Name;

					_context.Employees.Update(existingEmployee);
				}
			}
			await _context.SaveChangesAsync();

			// Step 2: Update managedBy relationships and bankHolidays
			foreach (var employee in employees)
			{
				var employeeToUpdate = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
				if (employeeToUpdate != null)
				{
					if (!string.IsNullOrEmpty(employee.ManagerEmail))
					{
						var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.ManagerEmail);
						if (manager == null)
						{
							throw new Exception("Error in Employee " + employee.Email + ": manager not found");

						}
						employeeToUpdate.ManagedBy = manager.Id;
						_context.Employees.Update(employeeToUpdate);

					}
					await _countriesService.GenerateEmployeeBankHolidays(employeeToUpdate);
				}

			}
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();
			foreach (var employee in employees)
			{
				var organization = await _context.Organizations.FindAsync(int.Parse(organizationId));
				if (organization != null)
				{
					string emailBody = $@"
Hello {employee.Name}, 
	You have been added as an Employee of {organization.Name} organization in LeavePlanner App. 
    Please log in with this email in {_leavePlannerUrl} to see your dashboard.";
					await _emailService.SendEmail(employee.Email, $"You have been added to LeavePlanner", emailBody);
				}
			}

		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception(ex.Message);
		}
	}
}