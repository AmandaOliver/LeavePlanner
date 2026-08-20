using System.Globalization;
using CsvHelper;
using LeavePlanner.Application.Common;
using LeavePlanner.Configuration;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LeavePlanner.Application.Organizations.Commands;

public record ImportOrganizationCommand(string OrganizationId, Stream File) : ICommand<Result>;

public class ImportOrganizationCommandHandler : IRequestHandler<ImportOrganizationCommand, Result>
{
	private readonly LeavePlannerContext _context;
	private readonly EmployeesService _employeesService;
	private readonly CountriesService _countriesService;
	private readonly EmailService _emailService;
	private readonly string _leavePlannerUrl;

	public ImportOrganizationCommandHandler(LeavePlannerContext context, EmployeesService employeesService,
		CountriesService countriesService, EmailService emailService, IOptions<AppOptions> appOptions)
	{
		_context = context;
		_employeesService = employeesService;
		_countriesService = countriesService;
		_emailService = emailService;
		_leavePlannerUrl = appOptions.Value.FrontendUrl;
	}

	public async Task<Result> Handle(ImportOrganizationCommand command, CancellationToken cancellationToken)
	{
		var organizationId = int.Parse(command.OrganizationId);

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			List<EmployeeCsvDTO> employees;
			try
			{
				using var reader = new StreamReader(command.File);
				using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
				employees = csv.GetRecords<EmployeeCsvDTO>().ToList();
			}
			catch
			{
				throw new InvalidOperationException("Error parsing the CSV, verify the structure.");
			}

			var headFound = false;
			foreach (var employee in employees)
			{
				if (string.IsNullOrEmpty(employee.ManagerEmail))
				{
					if (headFound)
					{
						throw new InvalidOperationException("Error in organization structure, you can't have two heads (employees without manager)");
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
					Organization = organizationId,
					IsOrgOwner = employee.IsAdmin
				});

				if (validationResult != "success")
				{
					throw new InvalidOperationException("Error in Employee " + employee.Email + ": " + validationResult);
				}

				var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email, cancellationToken);
				if (existingEmployee == null)
				{
					_context.Employees.Add(new Employee
					{
						Email = employee.Email,
						Name = employee.Name,
						Title = employee.Title,
						Country = employee.Country,
						PaidTimeOff = employee.PaidTimeOff,
						Organization = organizationId,
						IsOrgOwner = employee.IsAdmin
					});
				}
				else
				{
					existingEmployee.Country = employee.Country;
					existingEmployee.Organization = organizationId;
					existingEmployee.PaidTimeOff = employee.PaidTimeOff;
					existingEmployee.Title = employee.Title;
					existingEmployee.Name = employee.Name;

					_context.Employees.Update(existingEmployee);
				}
			}

			await _context.SaveChangesAsync(cancellationToken);

			foreach (var employee in employees)
			{
				var employeeToUpdate = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.Email, cancellationToken);
				if (employeeToUpdate == null)
				{
					continue;
				}

				if (!string.IsNullOrEmpty(employee.ManagerEmail))
				{
					var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == employee.ManagerEmail, cancellationToken);
					if (manager == null)
					{
						throw new InvalidOperationException("Error in Employee " + employee.Email + ": manager not found");
					}

					employeeToUpdate.ManagedBy = manager.Id;
					_context.Employees.Update(employeeToUpdate);
				}

				await _countriesService.GenerateEmployeeBankHolidays(employeeToUpdate);
			}

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			var organization = await _context.Organizations.FindAsync(new object?[] { organizationId }, cancellationToken);
			if (organization != null)
			{
				foreach (var employee in employees)
				{
					string emailBody = $@"
Hello {employee.Name}, 
	You have been added as an Employee of {organization.Name} organization in LeavePlanner App. 
    Please log in with this email in {_leavePlannerUrl} to see your dashboard.";
					await _emailService.SendEmail(employee.Email, $"You have been added to LeavePlanner", emailBody);
				}
			}

			return Result.Success();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result.Invalid(ex.Message);
		}
	}
}
