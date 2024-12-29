
using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;
using LeavePlanner.Models;
public static class EmployeesEndpointsExtensions
{
    public static void MapEmployeesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/employee/{email}", async (EmployeesController controller, string email) => await controller.GetEmployee(email))
                 .RequireAuthorization();
        endpoints.MapPost("/employee", async (EmployeesController controller, EmployeeCreateDTO model) => await controller.CreateEmployee(model))
                 .RequireAuthorization();
        endpoints.MapPut("/employee/{id}", async (EmployeesController controller, string id, EmployeeUpdateDTO model) => await controller.UpdateEmployee(id, model))
                 .RequireAuthorization();
        endpoints.MapDelete("/employee/{id}", async (EmployeesController controller, string id) => await controller.DeleteEmployee(id))
                 .RequireAuthorization();
        endpoints.MapPost("/create-employee-organization", async (EmployeesController controller, EmployeeOrganizationCreateDTO model) => await controller.CreateEmployeeAndOrganization(model)).RequireAuthorization();

    }
}
public class EmployeesController
{
    private readonly LeavePlannerContext _context;
    private readonly BankHolidayService _bankholidayService;
    private readonly IConfiguration _configuration;
    private readonly string _leavePlannerUrl;
    private readonly EmailService _emailService;
    private readonly EmployeesService _employeesService;



    public EmployeesController(LeavePlannerContext context, BankHolidayService bankHolidayService, IConfiguration configuration, EmailService emailService, EmployeesService employeesService)
    {
        _context = context;
        _bankholidayService = bankHolidayService;
        _configuration = configuration;
        _emailService = emailService;
        _employeesService = employeesService;
        // Access the LeavePlannerUrl from appsettings.json
        _leavePlannerUrl = _configuration.GetValue<string>("ConnectionStrings:LeavePlannerUrl");

    }
    public async Task<IResult> CreateEmployeeAndOrganization(EmployeeOrganizationCreateDTO model)
    {
        if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.OrganizationName))
        {
            return Results.BadRequest("Invalid data.");
        }

        // Create new organization
        var organization = new Organization
        {
            Name = model.OrganizationName,
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        // Create new employee and set the IsOrgOwner property to true
        var employee = new Employee
        {
            Email = model.Email,
            IsOrgOwner = true,
            Organization = organization.Id,
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        return Results.Ok(new { OrganizationId = organization.Id });

    }

    public async Task<IResult> CreateEmployee(EmployeeCreateDTO model)
    {

        var validationResult = await _employeesService.ValidateEmployee(model);
        if (validationResult != "success")
        {
            return Results.BadRequest(validationResult);
        }

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

                // Add employee to context
                _context.Employees.Add(employee);
            }
            await _context.SaveChangesAsync();
            await _bankholidayService.GenerateEmployeeBankHolidays(employee);
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
            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.Message);
        }
    }

    public async Task<IResult> GetEmployee(string email)
    {
        var employee = await _context.Employees
                                    .FirstOrDefaultAsync(e => e.Email == email);

        if (employee == null)
        {
            return Results.NotFound("User is not an employee.");
        }

        // Fetch subordinates recursively
        var employeeWithSubordinates = await _employeesService.GetEmployeeWithSubordinates(employee);
        return Results.Ok(employeeWithSubordinates);
    }

    public async Task<IResult> UpdateEmployee(string id, EmployeeUpdateDTO model)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var employee = await _context.Employees.FindAsync(int.Parse(id));

            if (employee == null)
            {
                return Results.NotFound("Employee not found.");
            }

            // if country changes we need to update the leaves
            if (employee.Country != model.Country)
            {
                var leaves = await _context.Leaves.Where(l => l.Owner == employee.Id && l.Type == "bankHoliday").ToListAsync();
                _context.Leaves.RemoveRange(leaves);
                employee.Country = model.Country;
                await _bankholidayService.GenerateEmployeeBankHolidays(employee);
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
                    return Results.BadRequest("You can't leave the organization without admins");
                }
            }
            employee.IsOrgOwner = model.IsOrgOwner;

            await transaction.CommitAsync();
            await _context.SaveChangesAsync();
            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.Message);
        }
    }
    public async Task<IResult> DeleteEmployee(string id)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var employee = await _context.Employees.FindAsync(int.Parse(id));

            if (employee == null)
            {
                return Results.NotFound("Employee not found.");
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
                    return Results.BadRequest("Cannot delete the head of the organization because they manage other employees.");
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
                // Remove the employee from the context
                _context.Employees.Remove(employee);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Ok(employee);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Results.Problem(ex.Message);
        }
    }
}
