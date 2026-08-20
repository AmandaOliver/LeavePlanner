using LeavePlanner.Application.Leaves;
using LeavePlanner.Configuration;
using LeavePlanner.Domain;
using Microsoft.Extensions.Options;

namespace LeavePlanner.Application.Common;

public interface IDomainEventDispatcher
{
	Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
	private readonly IEmployeeRepository _employees;
	private readonly IOrganizationRepository _organizations;
	private readonly IEmailSender _email;
	private readonly LeaveEvaluator _leaveEvaluator;
	private readonly string _leavePlannerUrl;

	public DomainEventDispatcher(
		IEmployeeRepository employees,
		IOrganizationRepository organizations,
		IEmailSender email,
		LeaveEvaluator leaveEvaluator,
		IOptions<AppOptions> appOptions)
	{
		_employees = employees;
		_organizations = organizations;
		_email = email;
		_leaveEvaluator = leaveEvaluator;
		_leavePlannerUrl = appOptions.Value.FrontendUrl;
	}

	public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken)
	{
		foreach (var domainEvent in events)
		{
			switch (domainEvent)
			{
				case LeaveSubmitted submitted:
					await NotifyManagerOfRequest(submitted.Leave, isUpdate: false, cancellationToken);
					break;
				case LeaveAmended amended:
					await NotifyManagerOfRequest(amended.Leave, isUpdate: true, cancellationToken);
					break;
				case LeaveCancelled cancelled:
					await NotifyManagerOfCancellation(cancelled, cancellationToken);
					break;
				case LeaveApproved approved:
					await NotifyOwnerOfDecision(approved.Leave, approved: true, cancellationToken);
					break;
				case LeaveRejected rejected:
					await NotifyOwnerOfDecision(rejected.Leave, approved: false, cancellationToken);
					break;
				case EmployeeJoined joined:
					await NotifyEmployeeJoined(joined.Employee, cancellationToken);
					break;
			}
		}
	}

	private async Task NotifyManagerOfRequest(Leave leave, bool isUpdate, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
		if (employee?.ManagedBy == null)
		{
			return;
		}

		var manager = await _employees.GetByIdAsync(employee.ManagedBy.Value, cancellationToken);
		if (manager == null)
		{
			return;
		}

		var info = await _leaveEvaluator.ComposeDto(leave, withConflicts: true, cancellationToken);
		var action = isUpdate
			? $"{employee.Name} has updated an existing leave request."
			: $"You have a new leave request from {employee.Name}.";
		var subject = isUpdate
			? $"Leave Request Updated by {employee.Name}"
			: $"New Leave Request from {employee.Name}";

		var body = $@"
Hello {manager.Name}, 
	{action}
	Number of days requested: {info.DaysRequested} days.
	Description: {leave.Description}						
	Start Date: {leave.DateStart.ToShortDateString()}
	End Date: {leave.DateEnd.ToShortDateString()}
	{LeaveEvaluator.DescribeConflicts(info)}
	To review go to {_leavePlannerUrl}/requests/{manager.Email}";

		await _email.SendAsync(manager.Email, subject, body, cancellationToken);
	}

	private async Task NotifyManagerOfCancellation(LeaveCancelled cancelled, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(cancelled.OwnerId, cancellationToken);
		if (employee?.ManagedBy == null)
		{
			return;
		}

		var manager = await _employees.GetByIdAsync(employee.ManagedBy.Value, cancellationToken);
		if (manager == null)
		{
			return;
		}

		var body = $@"
Hello {manager.Name}, 
	{employee.Name} has deleted a leave request.

	Start Date: {cancelled.DateStart.ToShortDateString()}
	End Date: {cancelled.DateEnd.ToShortDateString()}
	Description: {cancelled.Description}						
";
		await _email.SendAsync(manager.Email, $"Leave Request Deleted by {employee.Name}", body, cancellationToken);
	}

	private async Task NotifyOwnerOfDecision(Leave leave, bool approved, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
		if (employee == null)
		{
			return;
		}

		if (approved)
		{
			var body = $@"
Hello {employee.Name}, 
	Your leave request from {leave.DateStart.ToShortDateString()} to {leave.DateEnd.ToShortDateString()} 
	has been approved. 
	Enjoy your time off!.";
			await _email.SendAsync(employee.Email, "Leave Request approved", body, cancellationToken);
		}
		else
		{
			var body = $@"
Hello {employee.Name}, 
	Your leave request from {leave.DateStart.ToShortDateString()} to {leave.DateEnd.ToShortDateString()} 
	has been rejected. ";
			await _email.SendAsync(employee.Email, "Leave Request rejected", body, cancellationToken);
		}
	}

	private async Task NotifyEmployeeJoined(Employee employee, CancellationToken cancellationToken)
	{
		if (employee.Organization == null)
		{
			return;
		}

		var organization = await _organizations.GetByIdAsync(employee.Organization.Value, cancellationToken);
		if (organization == null)
		{
			return;
		}

		var body = $@"
Hello {employee.Name}, 
	You have been added as an Employee of {organization.Name} organization in LeavePlanner App. 
    Please log in with this email in {_leavePlannerUrl} to see your dashboard.";
		await _email.SendAsync(employee.Email, "You have been added to LeavePlanner", body, cancellationToken);
	}
}
