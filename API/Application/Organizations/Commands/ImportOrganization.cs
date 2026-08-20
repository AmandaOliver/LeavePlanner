using System.Globalization;
using CsvHelper;
using LeavePlanner.Application.Calendar;
using LeavePlanner.Application.Common;
using LeavePlanner.Domain;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Organizations.Commands;

public record ImportOrganizationCommand(string OrganizationId, Stream File) : ICommand<Result>;

public class ImportOrganizationCommandHandler : IRequestHandler<ImportOrganizationCommand, Result>
{
	private readonly IEmployeeRepository _employees;
	private readonly ICountryRepository _countries;
	private readonly PublicHolidayGenerator _holidays;
	private readonly IUnitOfWork _unitOfWork;

	public ImportOrganizationCommandHandler(
		IEmployeeRepository employees,
		ICountryRepository countries,
		PublicHolidayGenerator holidays,
		IUnitOfWork unitOfWork)
	{
		_employees = employees;
		_countries = countries;
		_holidays = holidays;
		_unitOfWork = unitOfWork;
	}

	public async Task<Result> Handle(ImportOrganizationCommand command, CancellationToken cancellationToken)
	{
		var organizationId = int.Parse(command.OrganizationId);

		await _unitOfWork.BeginTransactionAsync(cancellationToken);
		try
		{
			List<EmployeeCsvDTO> rows;
			try
			{
				using var reader = new StreamReader(command.File);
				using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
				rows = csv.GetRecords<EmployeeCsvDTO>().ToList();
			}
			catch
			{
				throw new InvalidOperationException("Error parsing the CSV, verify the structure.");
			}

			var headFound = false;
			foreach (var row in rows)
			{
				if (string.IsNullOrEmpty(row.ManagerEmail))
				{
					if (headFound)
					{
						throw new InvalidOperationException("Error in organization structure, you can't have two heads (employees without manager)");
					}

					headFound = true;
				}

				var existing = await _employees.GetByEmailAsync(row.Email, cancellationToken);
				var country = await _countries.GetByNameAsync(row.Country, cancellationToken);
				try
				{
					EmployeePolicy.AssertCanHire(
						row.Email, row.Name, row.Title, row.Country, row.PaidTimeOff, existing, manager: null, country != null, organizationId);
				}
				catch (DomainException ex)
				{
					throw new InvalidOperationException("Error in Employee " + row.Email + ": " + ex.Message);
				}

				if (existing == null)
				{
					_employees.Add(Employee.Hire(
						row.Email, row.Name, row.Title, row.Country, organizationId, null, row.PaidTimeOff, row.IsAdmin));
				}
				else
				{
					existing.Reactivate(row.Name, row.Title, row.Country, organizationId, null, row.PaidTimeOff);
					if (!existing.DomainEvents.OfType<EmployeeJoined>().Any())
					{
						existing.NotifyJoined();
					}
				}
			}

			await _unitOfWork.SaveChangesAsync(cancellationToken);

			foreach (var row in rows)
			{
				var employee = await _employees.GetByEmailAsync(row.Email, cancellationToken);
				if (employee == null)
				{
					continue;
				}

				if (!string.IsNullOrEmpty(row.ManagerEmail))
				{
					var manager = await _employees.GetByEmailAsync(row.ManagerEmail, cancellationToken);
					if (manager == null)
					{
						throw new InvalidOperationException("Error in Employee " + row.Email + ": manager not found");
					}

					if (manager.Organization != organizationId)
					{
						throw new InvalidOperationException("Error in Employee " + row.Email + ": manager must belong to the same organization");
					}

					employee.AssignManager(manager.Id);
				}

				await _holidays.GenerateFor(employee, cancellationToken);
			}

			var events = _unitOfWork.CollectEvents();
			await _unitOfWork.SaveChangesAsync(cancellationToken);
			await _unitOfWork.CommitTransactionAsync(cancellationToken);
			await _unitOfWork.DispatchAsync(events, cancellationToken);

			return Result.Success();
		}
		catch (Exception ex)
		{
			await _unitOfWork.RollbackTransactionAsync(cancellationToken);
			return Result.Invalid(ex.Message);
		}
	}
}
