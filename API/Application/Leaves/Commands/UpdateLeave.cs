using LeavePlanner.Application.Common;
using LeavePlanner.Configuration;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace LeavePlanner.Application.Leaves.Commands;

public record UpdateLeaveCommand(int LeaveId, LeaveUpdateDTO Leave) : ICommand<Result<Leave>>;

public class UpdateLeaveCommandHandler : IRequestHandler<UpdateLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly LeavesService _leavesService;
	private readonly EmailService _emailService;
	private readonly string _leavePlannerUrl;

	public UpdateLeaveCommandHandler(LeavePlannerContext context, LeavesService leavesService,
		EmailService emailService, IOptions<AppOptions> appOptions)
	{
		_context = context;
		_leavesService = leavesService;
		_emailService = emailService;
		_leavePlannerUrl = appOptions.Value.FrontendUrl;
	}

	public async Task<Result<Leave>> Handle(UpdateLeaveCommand command, CancellationToken cancellationToken)
	{
		var update = command.Leave;

		var validationResult = await _leavesService.ValidateLeave(
			update.DateStart, update.DateEnd, update.Owner, update.Id, update.Type);
		if (validationResult != "success")
		{
			return Result<Leave>.Invalid(validationResult);
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

		var leave = await _context.Leaves.FindAsync(new object?[] { command.LeaveId }, cancellationToken);
		if (leave == null)
		{
			return Result<Leave>.Invalid("Leave not found with that Id");
		}

		try
		{
			leave.Description = update.Description;
			leave.DateStart = update.DateStart;
			leave.DateEnd = update.DateEnd;
			leave.CreatedAt = DateTime.UtcNow;
			leave.ApprovedBy = null;
			leave.RejectedBy = null;

			_context.Leaves.Update(leave);
			await _context.SaveChangesAsync(cancellationToken);

			var employee = await _context.Employees.FindAsync(new object?[] { leave.Owner }, cancellationToken);
			if (employee == null)
			{
				return Result<Leave>.Invalid("Employee not found.");
			}

			if (employee.ManagedBy != null)
			{
				var manager = await _context.Employees.FindAsync(new object?[] { employee.ManagedBy }, cancellationToken);
				if (manager != null)
				{
					var leaveWithDynamicInfo = await _leavesService.GetLeaveDynamicInfo(leave, true);
					string emailBody = $@"
Hello {manager.Name}, 
	{employee.Name} has updated an existing leave request.
	Number of days requested: {leaveWithDynamicInfo.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{LeavesService.DescribeConflicts(leaveWithDynamicInfo)}
	To review go to {_leavePlannerUrl}/requests/{manager.Email}";
					await _emailService.SendEmail(manager.Email, $"Leave Request Updated by {employee.Name}", emailBody);
				}
			}
			else
			{
				leave.ApprovedBy = 1;
			}

			_context.Leaves.Update(leave);

			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			return Result<Leave>.Success(leave);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
	}
}
