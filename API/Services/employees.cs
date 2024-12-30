using System.Text.RegularExpressions;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;


public class EmployeesService
{
	private readonly LeavePlannerContext _context;
	private readonly BankHolidayService _bankHolidayService;
	private readonly EmailService _emailService;
	private readonly IConfiguration _configuration;
	private readonly string _leavePlannerUrl;


	private static readonly Regex _emailRegex = new Regex(
		 @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
		 RegexOptions.Compiled | RegexOptions.IgnoreCase);
	public EmployeesService(LeavePlannerContext context, BankHolidayService bankHolidayService, IConfiguration configuration, EmailService emailService)
	{
		_context = context;
		_bankHolidayService = bankHolidayService;
		_configuration = configuration;
		_emailService = emailService;
		_leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

	}
	public async Task<int> GetPaidTimeOffLeft(int employeeId, int year, int? leaveId)
	{
		// Fetch employee details
		var employee = await _context.Employees.FindAsync(employeeId);
		if (employee == null)
		{
			throw new Exception("Employee not found.");
		}

		// Fetch paidTimeOff leaves for the current year excluding the leave being updated/created
		var leavesThisYear = await _context.Leaves
			.Where(l => l.Owner == employeeId && l.Id != leaveId && l.Type == "paidTimeOff" && l.ApprovedBy != null && (l.DateStart.Year == year || l.DateEnd.Year == year))
			.ToListAsync();


		// Calculate total days taken 
		int totalDaysTaken = 0;
		foreach (var leave in leavesThisYear)
		{
			int daysRequested = await GetDaysRequested(leave.DateStart, leave.DateEnd, employeeId, year, leave.Id);
			totalDaysTaken = totalDaysTaken + daysRequested;
		}

		// Return remaining paid time off
		return employee.PaidTimeOff - totalDaysTaken;
	}

	public async Task<int> GetDaysRequested(DateTime start, DateTime end, int owner, int year, int? leaveId)
	{
		var leaveCreationDate = DateTime.UtcNow;
		if (leaveId != null)
		{
			var currentLeave = await _context.Leaves.FindAsync(leaveId);
			if (currentLeave == null)
			{
				throw new Exception("leave not found");
			}
			leaveCreationDate = currentLeave.CreatedAt;
			_context.Leaves.Remove(currentLeave);
		}
		// Check for conflicting leaves
		var conflictingLeaves = await _context.Leaves
			.Where(leave =>
				leave.Owner == owner && // is a leave of this employee
				(leave.ApprovedBy != null || leave.Type == "bankHoliday") && // we only take in account approved leaves and bank holidays
				(leaveId != null ? leave.Id != leaveId : true) && // if we are updating, do not take in account the previous version
				leave.CreatedAt < leaveCreationDate && // we only take in account leaves created previously (this is important when retrieving days dynamically on get calls)
				(
					(start >= leave.DateStart && start < leave.DateEnd) ||   // Start date is within an existing leave (excluding an exact match on end date)
					(end > leave.DateStart && end <= leave.DateEnd) ||       // End date is within an existing leave (excluding an exact match on start date)
					(start < leave.DateStart && end > leave.DateEnd)         // The requested leave fully contains an existing leave
				))
			.ToListAsync();


		int totalDays = 0;
		var employee = await _context.Employees.FindAsync(owner);
		if (employee == null)
		{
			throw new Exception("Owner not found");
		}
		var organization = await _context.Organizations.FindAsync(employee.Organization);
		if (organization == null)
		{
			throw new Exception("Organization not found");
		}
		// Loop through all days between start and end date
		for (DateTime date = start; date <= end.AddDays(-1); date = date.AddDays(1))
		{
			var dayOfWeek = date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;
			// Only count working days
			if (organization.WorkingDays.Contains(dayOfWeek) &&
			//only count days that are not within another leave already
			!conflictingLeaves.Any(leave => date >= leave.DateStart && date < leave.DateEnd) &&
			// only count days on the year we are interested in
			date.Year == year)
			{
				totalDays++;
			}
		}

		return totalDays;
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Employee? Employee)> DeleteEmployee(string id)
	{
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var employee = await _context.Employees.FindAsync(int.Parse(id));

			if (employee == null)
			{
				return (false, "Employee not found.", null);
			}

			// Check if the employee is the head of the organization (ManagedBy is null)
			if (employee.ManagedBy == null)
			{
				// Check if the head manages any subordinates
				var subordinates = await _context.Employees
												.Where(e => e.ManagedBy == employee.Id)
												.ToListAsync();

				if (subordinates.Any())
				{
					return (false, "Cannot delete the head of the organization because they manage other employees.", null);
				}
			}
			else
			{

				// Reassign subordinates to the manager above
				var subordinates = await _context.Employees
												.Where(e => e.ManagedBy == employee.Id)
												.ToListAsync();

				foreach (var subordinate in subordinates)
				{
					subordinate.ManagedBy = employee.ManagedBy;
				}

				_context.Employees.UpdateRange(subordinates);
			}
			// Remove leaves associated with the employee
			var leaves = await _context.Leaves.Where(l => l.Owner == employee.Id).ToListAsync();
			_context.Leaves.RemoveRange(leaves);

			// if employee is org owner, can't be deleted completely
			if (employee.IsOrgOwner == true)
			{
				employee.Country = null;
				employee.ManagedBy = null;
				employee.PaidTimeOff = 0;
				employee.Title = null;

				_context.Employees.Update(employee);
			}
			else
			{
				_context.Employees.Remove(employee);
			}
			await _context.SaveChangesAsync();
			await transaction.CommitAsync();

			return (true, null, employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Employee? Employee)> UpdateEmployee(string id, EmployeeUpdateDTO model)
	{
		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var employee = await _context.Employees.FindAsync(int.Parse(id));

			if (employee == null)
			{
				return (false, "Employee not found.", null);
			}

			// if country changes we need to update the leaves
			if (employee.Country != model.Country)
			{
				var leaves = await _context.Leaves.Where(l => l.Owner == employee.Id && l.Type == "bankHoliday").ToListAsync();
				_context.Leaves.RemoveRange(leaves);
				employee.Country = model.Country;
				await _bankHolidayService.GenerateEmployeeBankHolidays(employee);
			}
			employee.PaidTimeOff = model.PaidTimeOff != 0 ? model.PaidTimeOff : employee.PaidTimeOff;
			employee.Title = model.Title ?? employee.Title;
			employee.Name = model.Name ?? employee.Name;
			employee.Email = model.Email ?? employee.Email;
			if (employee.IsOrgOwner == true && model.IsOrgOwner == false)
			{
				var anotherOwner = await _context.Employees.FirstOrDefaultAsync(e => e.IsOrgOwner == true && employee.Email != e.Email);
				if (anotherOwner == null)
				{
					await transaction.RollbackAsync();
					return (false, "You can't leave the organization without admins", null);
				}
			}
			employee.IsOrgOwner = model.IsOrgOwner;

			await transaction.CommitAsync();
			await _context.SaveChangesAsync();
			return (true, null, employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, EmployeeWithSubordinatesDTO? Employee)> GetEmployeeByEmail(string email)
	{
		var employee = await _context.Employees
								   .FirstOrDefaultAsync(e => e.Email == email);

		if (employee == null)
		{
			return (false, "User is not an employee.", null);
		}

		// Fetch subordinates recursively
		var employeeWithSubordinates = await GetEmployeeWithSubordinates(employee);
		return (true, null, employeeWithSubordinates);
	}
	public async Task<(bool IsSuccess, string? ErrorMessage, Employee? Employee)> CreateEmployee(EmployeeCreateDTO model)
	{
		var validationResult = await ValidateEmployee(model);
		if (validationResult != "success")
			return (false, validationResult, null);

		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			Employee employee;
			var existingEmployee = await _context.Employees.FirstOrDefaultAsync(employee => employee.Email == model.Email);
			if (existingEmployee != null)
			{
				existingEmployee.Country = model.Country;
				existingEmployee.Organization = model.Organization;
				existingEmployee.ManagedBy = model.ManagedBy;
				existingEmployee.PaidTimeOff = model.PaidTimeOff;
				existingEmployee.Title = model.Title;
				existingEmployee.Name = model.Name;

				_context.Employees.Update(existingEmployee);
				var leaves = await _context.Leaves.Where(l => l.Owner == existingEmployee.Id && l.Type == "bankHoliday").ToListAsync();
				_context.Leaves.RemoveRange(leaves);
				employee = existingEmployee;
			}
			else
			{
				employee = new Employee
				{
					Email = model.Email,
					Country = model.Country,
					Organization = model.Organization,
					ManagedBy = model.ManagedBy,
					IsOrgOwner = model.IsOrgOwner,
					PaidTimeOff = model.PaidTimeOff,
					Title = model.Title,
					Name = model.Name,
				};

				_context.Employees.Add(employee);
			}
			await _context.SaveChangesAsync();
			await _bankHolidayService.GenerateEmployeeBankHolidays(employee);
			await transaction.CommitAsync();
			var organization = await _context.Organizations.FindAsync(employee.Organization);
			if (organization != null)
			{
				string emailBody = $@"
Hello {employee.Name}, 
	You have been added as an Employee of {organization.Name} organization in LeavePlanner App. 
    Please log in with this email in {_leavePlannerUrl} to see your dashboard.";
				await _emailService.SendEmail(employee.Email, $"You have been added to LeavePlanner", emailBody);
			}
			return (true, null, employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}

	}
	public async Task<(bool IsSuccess, string? ErrorMessage, int? OrganizationId)> CreateEmployeeAndOrganization(EmployeeOrganizationCreateDTO model)
	{
		if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OrganizationName))
			return (false, "Invalid data.", null);

		using var transaction = await _context.Database.BeginTransactionAsync();
		try
		{
			var organization = new Organization { Name = model.OrganizationName };
			_context.Organizations.Add(organization);
			await _context.SaveChangesAsync();

			var employee = new Employee
			{
				Email = model.Email,
				IsOrgOwner = true,
				Organization = organization.Id
			};

			_context.Employees.Add(employee);
			await transaction.CommitAsync();
			await _context.SaveChangesAsync();
			return (true, null, organization.Id);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			return (false, ex.Message, null);
		}
	}
	public async Task<string> ValidateEmployee(EmployeeCreateDTO employee)
	{
		var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email);
		var emailRegex = new Regex(
	  @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
	  RegexOptions.Compiled | RegexOptions.IgnoreCase);
		if (string.IsNullOrWhiteSpace(employee.Email))
			return "Email can't be empty";

		if (!_emailRegex.IsMatch(employee.Email))
		{
			return "Email must be valid.";
		}
		if (string.IsNullOrWhiteSpace(employee.Name))
		{
			return "Name can't be empty";
		}
		if (string.IsNullOrWhiteSpace(employee.Title))
		{
			return "Title can't be empty";
		}
		if (employee.ManagedBy != null)
		{
			var manager = await _context.Employees.FindAsync(employee.ManagedBy);
			if (manager == null)
			{
				return "Manager must exist.";
			}
			if (manager.Email == employee.Email)
			{
				return "An employee can't be managed by himself";
			}
		}
		if (string.IsNullOrWhiteSpace(employee.Country))
		{
			return "Country can't be empty.";
		}
		try
		{
			var Country = await _context.Countries.FirstAsync(country => country.Name == employee.Country);
		}
		catch (Exception ex)
		{
			return "Country is not within the list.";
		}
		if (employee.PaidTimeOff < 1)
		{
			return "Paid time off needs to be higher than 1.";
		}
		return "success";
	}
	public async Task<EmployeeWithSubordinatesDTO> GetEmployeeWithSubordinates(Employee employee)
	{
		int paidTimeOffLeft = await GetPaidTimeOffLeft(employee.Id, DateTime.UtcNow.Year, null);
		int paidTimeOffLeftNextYear = await GetPaidTimeOffLeft(employee.Id, DateTime.UtcNow.Year + 1, null);
		var manager = await _context.Employees.FindAsync(employee.ManagedBy);
		var employeeWithSubordinates = new EmployeeWithSubordinatesDTO
		{
			Id = employee.Id,
			Email = employee.Email,
			Name = employee.Name,
			Country = employee.Country,
			Organization = employee.Organization,
			ManagerName = manager != null ? manager.Name : null,
			ManagedBy = employee.ManagedBy,
			IsOrgOwner = employee.IsOrgOwner,
			PaidTimeOff = employee.PaidTimeOff,
			Title = employee.Title,
			PaidTimeOffLeft = paidTimeOffLeft,
			PaidTimeOffLeftNextYear = paidTimeOffLeftNextYear,
			Subordinates = new List<EmployeeWithSubordinatesDTO>()
		};

		var subordinates = await _context.Employees
										.Where(e => e.ManagedBy == employee.Id)
										.ToListAsync();
		if (subordinates != null)
		{
			var pendingRequests = 0;
			foreach (var subordinate in subordinates)
			{
				pendingRequests += await _context.Leaves
					  .Where(leave => leave.Owner == subordinate.Id &&
									  leave.ApprovedBy == null && leave.RejectedBy == null)
					  .CountAsync();
				var subordinateWithSubordinates = await GetEmployeeWithSubordinates(subordinate);
				employeeWithSubordinates.Subordinates.Add(subordinateWithSubordinates);
			}
			employeeWithSubordinates.PendingRequests = pendingRequests;
		}

		return employeeWithSubordinates;

	}
	public async Task DeleteEmployeeWithSubordinates(int id)
	{
		var employee = await _context.Employees.FindAsync(id);
		if (employee == null) return;

		var subordinates = await _context.Employees
			.Where(e => e.ManagedBy == id)
			.ToListAsync();

		foreach (var subordinate in subordinates)
		{
			await DeleteEmployeeWithSubordinates(subordinate.Id);
		}

		var leaves = await _context.Leaves
			.Where(l => l.Owner == employee.Id)
			.ToListAsync();

		_context.Leaves.RemoveRange(leaves);

		_context.Employees.Remove(employee);
	}


}