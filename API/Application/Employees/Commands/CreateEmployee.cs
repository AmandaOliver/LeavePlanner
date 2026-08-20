using LeavePlanner.Application.Common;
using LeavePlanner.Configuration;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LeavePlanner.Application.Employees.Commands;

public record CreateEmployeeCommand(EmployeeCreateDTO Employee) : ICommand<Result<Employee>>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Employee>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly CountriesService _countriesService;
	private readonly EmailService _emailService;
	private readonly string _leavePlannerUrl;

	public CreateEmployeeCommandHandler(LeavePlannerContext context, EmployeesService employeesService,
		CountriesService countriesService, EmailService emailService, IOptions<AppOptions> appOptions)
	{
		_context = context;
		_employeesService = employeesService;
		_countriesService = countriesService;
		_emailService = emailService;
		_leavePlannerUrl = appOptions.Value.FrontendUrl;
	}

	public async Task<Result<Employee>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
	{
		var model = request.Employee;

		var validationResult = await _employeesService.ValidateEmployee(model);
		if (validationResult != "success")
		{
			return Result<Employee>.Invalid(validationResult);
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			Employee employee;
			var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == model.Email, cancellationToken);
			if (existingEmployee != null)
			{
				existingEmployee.Country = model.Country;
				existingEmployee.Organization = model.Organization;
				existingEmployee.ManagedBy = model.ManagedBy;
				existingEmployee.PaidTimeOff = model.PaidTimeOff;
				existingEmployee.Title = model.Title;
				existingEmployee.Name = model.Name;

				_context.Employees.Update(existingEmployee);
				var leaves = await _context.Leaves
					.Where(l => l.Owner == existingEmployee.Id && l.Type == "bankHoliday")
					.ToListAsync(cancellationToken);
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

			await _context.SaveChangesAsync(cancellationToken);
			await _countriesService.GenerateEmployeeBankHolidays(employee);
			await transaction.CommitAsync(cancellationToken);

			var organization = await _context.Organizations.FindAsync(new object?[] { employee.Organization }, cancellationToken);
			if (organization != null)
			{
				string emailBody = $@"
Hello {employee.Name}, 
	You have been added as an Employee of {organization.Name} organization in LeavePlanner App. 
    Please log in with this email in {_leavePlannerUrl} to see your dashboard.";
				await _emailService.SendEmail(employee.Email, $"You have been added to LeavePlanner", emailBody);
			}

			return Result<Employee>.Success(employee);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Employee>.Invalid(ex.Message);
		}
	}
}
