using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Leaves.Commands;

public record DeleteLeaveCommand(int LeaveId) : ICommand<Result<Leave>>;

public class DeleteLeaveCommandHandler : IRequestHandler<DeleteLeaveCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmailService _emailService;

	public DeleteLeaveCommandHandler(LeavePlannerContext context, EmailService emailService)
	{
		_context = context;
		_emailService = emailService;
	}

	public async Task<Result<Leave>> Handle(DeleteLeaveCommand command, CancellationToken cancellationToken)
	{
		var leave = await _context.Leaves.FindAsync(new object?[] { command.LeaveId }, cancellationToken);
		if (leave == null)
		{
			return Result<Leave>.Invalid("Leave not found");
		}

		if (leave.ApprovedBy != null && (leave.DateStart < DateTime.UtcNow || leave.DateEnd < DateTime.UtcNow))
		{
			return Result<Leave>.Invalid("You cannot delete leaves in the past.");
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			_context.Leaves.Remove(leave);
			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

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
					string emailBody = $@"
Hello {manager.Name}, 
	{employee.Name} has deleted a leave request.

	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	Description: {leave.Description}						
";
					await _emailService.SendEmail(manager.Email, $"Leave Request Deleted by {employee.Name}", emailBody);
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
