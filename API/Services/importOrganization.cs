using CsvHelper;
using System.Globalization;
using LeavePlanner.Data;
using LeavePlanner.Models;

public class OrganizationImportService
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly BankHolidayService _bankholidayService;

	private readonly string _leavePlannerUrl;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;


	public OrganizationImportService(LeavePlannerContext context, EmployeesService employeesService, BankHolidayService bankholidayService, IConfiguration configuration, EmailService emailService)
	{
		_context = context;
		_employeesService = employeesService;
		_bankholidayService = bankholidayService;
		_emailService = emailService;
		_configuration = configuration;

		// Access the LeavePlannerUrl from appsettings.json
		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

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
					ManagedBy = employee.ManagerEmail,
					Organization = int.Parse(organizationId)
				});
				if (validationResult != "success")
				{
					throw new Exception("Error in Employee " + employee.Email + ": " + validationResult);

				}
				var existingEmployee = await _context.Employees.FindAsync(employee.Email);
				if (existingEmployee == null)
				{
					var newEmployee = new Employee
					{
						Email = employee.Email,
						Name = employee.Name,
						Title = employee.Title,
						Country = employee.Country,
						PaidTimeOff = employee.PaidTimeOff,
						Organization = int.Parse(organizationId)
					};
					_context.Employees.Add(newEmployee);
					await _bankholidayService.GenerateEmployeeBankHolidays(newEmployee);

				}
				else
				{
					existingEmployee.Country = employee.Country;
					existingEmployee.Organization = int.Parse(organizationId);
					existingEmployee.PaidTimeOff = employee.PaidTimeOff;
					existingEmployee.Title = employee.Title;
					existingEmployee.Name = employee.Name;

					_context.Employees.Update(existingEmployee);
					await _bankholidayService.GenerateEmployeeBankHolidays(existingEmployee);

				}
			}
			await _context.SaveChangesAsync();

			// Step 2: Update managedBy relationships
			foreach (var employee in employees)
			{

				if (!string.IsNullOrEmpty(employee.ManagerEmail))
				{
					var employeeToUpdate = await _context.Employees.FindAsync(employee.Email);
					if (employeeToUpdate != null)
					{
						employeeToUpdate.ManagedBy = employee.ManagerEmail;
						_context.Employees.Update(employeeToUpdate);
					}
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