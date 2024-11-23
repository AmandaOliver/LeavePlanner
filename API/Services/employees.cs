using System.Text.RegularExpressions;
using LeavePlanner.Data;
using LeavePlanner.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
public interface IEmployeesService
{
	Task<string> ValidateEmployee(EmployeeCreateDTO employee);
	Task<EmployeeWithSubordinatesDTO> GetEmployeeWithSubordinates(Employee employee);
}

public class EmployeesService : IEmployeesService
{
	private readonly LeavePlannerContext _context;
	private readonly PaidTimeOffLeft _paidTimeOffLeft;

	private static readonly Regex _emailRegex = new Regex(
		 @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
		 RegexOptions.Compiled | RegexOptions.IgnoreCase);
	public EmployeesService(LeavePlannerContext context, PaidTimeOffLeft paidTimeOffLeft)
	{
		_context = context;
		_paidTimeOffLeft = paidTimeOffLeft;
	}

	public async Task<string> ValidateEmployee(EmployeeCreateDTO employee)
	{
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
		if (!string.IsNullOrWhiteSpace(employee.ManagedBy) && !_emailRegex.IsMatch(employee.ManagedBy))
		{
			return "ManagerEmail must be valid.";
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
		int paidTimeOffLeft = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, DateTime.UtcNow.Year, null);
		int paidTimeOffLeftNextYear = await _paidTimeOffLeft.GetPaidTimeOffLeft(employee.Email, DateTime.UtcNow.Year + 1, null);
		var employeeWithSubordinates = new EmployeeWithSubordinatesDTO
		{
			Email = employee.Email,
			Name = employee.Name,
			Country = employee.Country,
			Organization = employee.Organization,
			ManagedBy = employee.ManagedBy,
			IsOrgOwner = employee.IsOrgOwner,
			PaidTimeOff = employee.PaidTimeOff,
			Title = employee.Title,
			PaidTimeOffLeft = paidTimeOffLeft,
			PaidTimeOffLeftNextYear = paidTimeOffLeftNextYear,
			Subordinates = new List<EmployeeWithSubordinatesDTO>()
		};

		var subordinates = await _context.Employees
										.Where(e => e.ManagedBy == employee.Email)
										.ToListAsync();
		if (subordinates != null)
		{
			var pendingRequests = 0;
			foreach (var subordinate in subordinates)
			{
				pendingRequests += await _context.Leaves
					  .Where(leave => leave.Owner == subordinate.Email &&
									  leave.ApprovedBy == null && leave.RejectedBy == null && leave.Type != "bankHoliday")
					  .CountAsync();
				var subordinateWithSubordinates = await GetEmployeeWithSubordinates(subordinate);
				employeeWithSubordinates.Subordinates.Add(subordinateWithSubordinates);
			}
			employeeWithSubordinates.PendingRequests = pendingRequests;
		}

		return employeeWithSubordinates;

	}
}