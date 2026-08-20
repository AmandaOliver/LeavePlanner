using System.Text.Json.Serialization;

namespace LeavePlanner.Domain;

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

	[JsonIgnore]
	public virtual Employee? ApprovedByNavigation { get; set; }

	[JsonIgnore]
	public virtual Employee? RejectedByNavigation { get; set; }

	[JsonIgnore]
	public virtual Employee? OwnerNavigation { get; set; }

	[JsonIgnore]
	public DateRange Period => new(DateStart, DateEnd);

	[JsonIgnore]
	public bool IsApproved => ApprovedBy != null;

	[JsonIgnore]
	public bool IsRejected => RejectedBy != null;

	[JsonIgnore]
	public bool IsPending => ApprovedBy == null && RejectedBy == null;

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
