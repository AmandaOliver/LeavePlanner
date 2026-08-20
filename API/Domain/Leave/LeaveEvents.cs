namespace LeavePlanner.Domain;

public sealed class LeaveSubmitted : IDomainEvent
{
	public LeaveSubmitted(Leave leave) => Leave = leave;

	public Leave Leave { get; }
}

public sealed class LeaveAmended : IDomainEvent
{
	public LeaveAmended(Leave leave) => Leave = leave;

	public Leave Leave { get; }
}

public sealed class LeaveCancelled : IDomainEvent
{
	public LeaveCancelled(int ownerId, DateTime dateStart, DateTime dateEnd, string? description)
	{
		OwnerId = ownerId;
		DateStart = dateStart;
		DateEnd = dateEnd;
		Description = description;
	}

	public int OwnerId { get; }
	public DateTime DateStart { get; }
	public DateTime DateEnd { get; }
	public string? Description { get; }
}

public sealed class LeaveApproved : IDomainEvent
{
	public LeaveApproved(Leave leave) => Leave = leave;

	public Leave Leave { get; }
}

public sealed class LeaveRejected : IDomainEvent
{
	public LeaveRejected(Leave leave) => Leave = leave;

	public Leave Leave { get; }
}
