using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class LeaveTests
{
	private static readonly DateTime Now = new(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);
	private static readonly DateTime Start = new(2026, 8, 24);
	private static readonly DateTime End = new(2026, 8, 28);

	[Fact]
	public void Submit_auto_approves_when_the_owner_is_the_organization_head()
	{
		var head = Employee.Rehydrate(1, "head@org.com", name: "Head", organization: 8, managedBy: null);
		var leave = Leave.Submit(head, LeaveTypes.PaidTimeOff, Start, End, "summer", Now);

		Assert.Equal(1, leave.ApprovedBy);
		Assert.Equal(LeaveStatus.Approved, leave.Status);
		Assert.Empty(leave.DomainEvents);
	}

	[Fact]
	public void Submit_raises_LeaveSubmitted_when_the_owner_has_a_manager()
	{
		var employee = Employee.Rehydrate(2, "alex@org.com", name: "Alex", organization: 8, managedBy: 1);
		var leave = Leave.Submit(employee, LeaveTypes.PaidTimeOff, Start, End, "summer", Now);

		Assert.Null(leave.ApprovedBy);
		Assert.Equal(LeaveStatus.Pending, leave.Status);
		Assert.Contains(leave.DomainEvents, domainEvent => domainEvent is LeaveSubmitted);
	}

	[Fact]
	public void Amend_resets_review_and_auto_approves_for_the_head()
	{
		var leave = Leave.Rehydrate(5, LeaveTypes.PaidTimeOff, Start, End, owner: 1, approvedBy: 1);
		leave.Amend(Start.AddDays(1), End.AddDays(1), "moved", Now, ownerIsOrgHead: true, systemApproverId: 99);

		Assert.Equal(99, leave.ApprovedBy);
		Assert.Null(leave.RejectedBy);
		Assert.Empty(leave.DomainEvents);
	}

	[Fact]
	public void Amend_raises_LeaveAmended_when_the_owner_is_not_the_head()
	{
		var leave = Leave.Rehydrate(5, LeaveTypes.PaidTimeOff, Start, End, owner: 2);
		leave.Amend(Start.AddDays(1), End.AddDays(1), "moved", Now, ownerIsOrgHead: false, systemApproverId: null);

		Assert.Null(leave.ApprovedBy);
		Assert.Contains(leave.DomainEvents, domainEvent => domainEvent is LeaveAmended);
	}

	[Fact]
	public void Approve_sets_status_and_raises_LeaveApproved()
	{
		var leave = Leave.Rehydrate(5, LeaveTypes.PaidTimeOff, Start, End, owner: 2);
		leave.Approve(1);
		Assert.Equal(LeaveStatus.Approved, leave.Status);
		Assert.Contains(leave.DomainEvents, domainEvent => domainEvent is LeaveApproved);
	}

	[Fact]
	public void Reject_sets_status_and_raises_LeaveRejected()
	{
		var leave = Leave.Rehydrate(5, LeaveTypes.PaidTimeOff, Start, End, owner: 2);
		leave.Reject(1);
		Assert.Equal(LeaveStatus.Rejected, leave.Status);
		Assert.Contains(leave.DomainEvents, domainEvent => domainEvent is LeaveRejected);
	}

	[Fact]
	public void Cancel_rejects_an_approved_leave_that_has_already_started()
	{
		var leave = Leave.Rehydrate(
			5,
			LeaveTypes.PaidTimeOff,
			Now.AddDays(-2),
			Now.AddDays(2),
			owner: 2,
			approvedBy: 1);

		Assert.Throws<DomainException>(() => leave.Cancel(Now));
	}

	[Fact]
	public void Cancel_raises_LeaveCancelled_for_a_future_leave()
	{
		var leave = Leave.Rehydrate(5, LeaveTypes.PaidTimeOff, Start, End, owner: 2, approvedBy: 1);
		leave.Cancel(Now);
		Assert.Contains(leave.DomainEvents, domainEvent => domainEvent is LeaveCancelled);
	}
}
