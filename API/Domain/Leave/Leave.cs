namespace LeavePlanner.Domain;

public enum LeaveStatus
{
	Pending,
	Approved,
	Rejected
}

public class Leave : AggregateRoot
{
	private Leave()
	{
	}

	public int Id { get; private set; }

	public string? Type { get; private set; }

	public DateTime DateStart { get; private set; }

	public DateTime DateEnd { get; private set; }

	public int Owner { get; private set; }

	public string? Description { get; private set; }

	public int? ApprovedBy { get; private set; }

	public int? RejectedBy { get; private set; }

	public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

	public virtual Employee? ApprovedByNavigation { get; set; }

	public virtual Employee? RejectedByNavigation { get; set; }

	public virtual Employee? OwnerNavigation { get; set; }

	public DateRange Period => new(DateStart, DateEnd);

	public LeaveStatus Status =>
		ApprovedBy != null ? LeaveStatus.Approved :
		RejectedBy != null ? LeaveStatus.Rejected :
		LeaveStatus.Pending;

	public bool IsApproved => Status == LeaveStatus.Approved;

	public bool IsRejected => Status == LeaveStatus.Rejected;

	public bool IsPending => Status == LeaveStatus.Pending;

	public static Leave Submit(Employee owner, string type, DateTime start, DateTime end, string? description, DateTime utcNow)
	{
		var leave = Create(owner, type, start, end, description, utcNow);
		if (owner.IsOrgHead)
		{
			leave.ApprovedBy = owner.Id;
		}
		else
		{
			leave.Raise(new LeaveSubmitted(leave));
		}

		return leave;
	}

	public static Leave RecordPublicHoliday(Employee owner, PublicHoliday holiday, int systemApproverId, DateTime utcNow)
	{
		var leave = Create(owner, LeaveTypes.BankHoliday, holiday.Start, holiday.End, holiday.Summary, utcNow);
		leave.ApprovedBy = systemApproverId;
		return leave;
	}

	public static Leave Preview(Employee owner, int? id, string type, DateTime start, DateTime end)
	{
		return new Leave
		{
			Id = id ?? 0,
			Type = type,
			DateStart = start,
			DateEnd = end,
			Owner = owner.Id,
			OwnerNavigation = owner
		};
	}

	public static Leave Rehydrate(
		int id,
		string? type,
		DateTime dateStart,
		DateTime dateEnd,
		int owner,
		string? description = null,
		int? approvedBy = null,
		int? rejectedBy = null,
		DateTime? createdAt = null)
	{
		return new Leave
		{
			Id = id,
			Type = type,
			DateStart = dateStart,
			DateEnd = dateEnd,
			Owner = owner,
			Description = description,
			ApprovedBy = approvedBy,
			RejectedBy = rejectedBy,
			CreatedAt = createdAt ?? DateTime.UnixEpoch
		};
	}

	public void Amend(DateTime start, DateTime end, string? description, DateTime utcNow, bool ownerIsOrgHead, int? systemApproverId)
	{
		Description = description;
		DateStart = start;
		DateEnd = end;
		CreatedAt = utcNow;
		ApprovedBy = null;
		RejectedBy = null;

		if (ownerIsOrgHead)
		{
			ApprovedBy = systemApproverId;
		}
		else
		{
			Raise(new LeaveAmended(this));
		}
	}

	public void Approve(int reviewerId)
	{
		ApprovedBy = reviewerId;
		RejectedBy = null;
		Raise(new LeaveApproved(this));
	}

	public void Reject(int reviewerId)
	{
		RejectedBy = reviewerId;
		Raise(new LeaveRejected(this));
	}

	public void Cancel(DateTime utcNow)
	{
		if (IsApproved && (DateStart < utcNow || DateEnd < utcNow))
		{
			throw new DomainException("You cannot delete leaves in the past.");
		}

		Raise(new LeaveCancelled(Owner, DateStart, DateEnd, Description));
	}

	private static Leave Create(Employee owner, string type, DateTime start, DateTime end, string? description, DateTime utcNow)
	{
		return new Leave
		{
			Type = type,
			DateStart = start,
			DateEnd = end,
			Description = description,
			Owner = owner.Id,
			OwnerNavigation = owner,
			CreatedAt = utcNow
		};
	}
}
