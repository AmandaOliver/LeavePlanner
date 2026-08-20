using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Models;
using MediatR;

namespace LeavePlanner.Application.Requests.Commands;

public record RejectRequestCommand(string RequestId, string EmployeeId) : ICommand<Result<Leave>>;

public class RejectRequestCommandHandler : IRequestHandler<RejectRequestCommand, Result<Leave>>
{
	private readonly LeavePlannerContext _context;
	private readonly EmailService _emailService;

	public RejectRequestCommandHandler(LeavePlannerContext context, EmailService emailService)
	{
		_context = context;
		_emailService = emailService;
	}

	public async Task<Result<Leave>> Handle(RejectRequestCommand command, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(command.RequestId) || string.IsNullOrEmpty(command.EmployeeId))
		{
			return Result<Leave>.Invalid("Employee and request id can't be empty");
		}

		using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
		try
		{
			var request = await _context.Leaves.FindAsync(new object?[] { int.Parse(command.RequestId) }, cancellationToken);
			if (request == null)
			{
				return Result<Leave>.Invalid("request not found");
			}

			request.RejectedBy = int.Parse(command.EmployeeId);
			await _context.SaveChangesAsync(cancellationToken);
			await transaction.CommitAsync(cancellationToken);

			var employee = await _context.Employees.FindAsync(new object?[] { request.Owner }, cancellationToken);
			if (employee != null)
			{
				string emailBody = $@"
Hello {employee.Name}, 
	Your leave request from {request.DateStart.ToShortDateString()} to {request.DateEnd.ToShortDateString()} 
	has been rejected. ";
				await _emailService.SendEmail(employee.Email, $"Leave Request rejected", emailBody);
			}

			return Result<Leave>.Success(request);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync(cancellationToken);
			return Result<Leave>.Invalid(ex.Message);
		}
	}
}
