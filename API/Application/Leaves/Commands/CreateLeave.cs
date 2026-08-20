using LeavePlanner.Application.Common;
using LeavePlanner.Configuration;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace LeavePlanner.Application.Leaves.Commands;

public record CreateLeaveCommand(string EmployeeId, LeaveCreateDTO Leave) : ICommand<Result<Leave>>;

public class CreateLeaveCommandHandler : IRequestHandler<CreateLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;
	private readonly EmailService _emailService;
	private readonly string _leavePlannerUrl;

	public CreateLeaveCommandHandler(LeavePlannerContext context, LeavesService leavesService,
		EmailService emailService, IOptions<AppOptions> appOptions)
	{
		_context = context;
		_leavesService = leavesService;
		_emailService = emailService;
		_leavePlannerUrl = appOptions.Value.FrontendUrl;
	}

	public async Task<Result<Leave>> Handle(CreateLeaveCommand command, CancellationToken cancellationToken)
	{
		var employeeId = int.Parse(command.EmployeeId);
		var model = command.Leave;

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var employee = await _context.Employees.FindAsync(new object?[] { employeeId }, cancellationToken);
			if (employee == null)
			{
				return Result<Leave>.Invalid("Employee not found.");
			}

			var validationResult = await _leavesService.ValidateLeave(model.DateStart, model.DateEnd, employeeId, null, model.Type);
			if (validationResult != "success")
			{
				return Result<Leave>.Invalid(validationResult);
			}

			var leave = new Leave
			{
				Description = model.Description,
				DateStart = model.DateStart,
				DateEnd = model.DateEnd,
				Type = model.Type,
				Owner = employeeId,
				OwnerNavigation = employee,
				CreatedAt = DateTime.UtcNow
			};

			if (employee.ManagedBy == null)
			{
				leave.ApprovedBy = employeeId;
			}

			_context.Leaves.Add(leave);

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(new object?[] { employee.ManagedBy }, cancellationToken);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await _leavesService.GetLeaveDynamicInfo(leave, true);
					string emailBody = $@"
Hello {manager.Name}, 
	You have a new leave request from {employee.Name}.
	Number of days requested: {leaveWithDynamicInfo.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{LeavesService.DescribeConflicts(leaveWithDynamicInfo)}
	To review go to {_leavePlannerUrl}/requests/{manager.Email}

						";
					await _emailService.SendEmail(manager.Email, $"New Leave Request from {employee.Name}", emailBody);
				}
			}

			return Result<Leave>.Success(leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
	}
}
