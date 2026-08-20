using System.Text.Json.Serialization;

namespace LeavePlanner.Domain;

public class Employee : AggregateRoot
{
	private Employee()
	{
	}

	public int Id { get; private set; }
	public string Email { get; private set; } = null!;
	public string? Name { get; private set; }
	public int? Organization { get; private set; }
	public int? ManagedBy { get; private set; }
	public string? Country { get; private set; }
	public bool IsOrgOwner { get; private set; }
	public int PaidTimeOff { get; private set; }
	public string? Title { get; private set; }

	[JsonIgnore]
	public virtual ICollection<Employee> InverseManagedByNavigation { get; set; } = new List<Employee>();

	[JsonIgnore]
	public virtual ICollection<Leave> LeaveApprovedByNavigations { get; set; } = new List<Leave>();

	[JsonIgnore]
	public virtual ICollection<Leave> LeaveRejectedByNavigations { get; set; } = new List<Leave>();

	[JsonIgnore]
	public virtual ICollection<Leave> LeaveOwnerNavigations { get; set; } = new List<Leave>();

	[JsonIgnore]
	public virtual Employee? ManagedByNavigation { get; set; }

	[JsonIgnore]
	public virtual Organization? OrganizationNavigation { get; set; }

	[JsonIgnore]
	public bool IsOrgHead => ManagedBy == null;

	[JsonIgnore]
	public bool IsActive => Country != null;

	public static Employee CreateOwner(string email, int organizationId)
	{
		return new Employee
		{
			Email = email,
			IsOrgOwner = true,
			Organization = organizationId
		};
	}

	public static Employee Hire(
		string email,
		string name,
		string title,
		string country,
		int organizationId,
		int? managedBy,
		int paidTimeOff,
		bool isOrgOwner)
	{
		var employee = new Employee
		{
			Email = email,
			Name = name,
			Title = title,
			Country = country,
			Organization = organizationId,
			ManagedBy = managedBy,
			PaidTimeOff = paidTimeOff,
			IsOrgOwner = isOrgOwner
		};
		employee.Raise(new EmployeeJoined(employee));
		return employee;
	}

	public void Reactivate(
		string name,
		string title,
		string country,
		int organizationId,
		int? managedBy,
		int paidTimeOff)
	{
		Name = name;
		Title = title;
		Country = country;
		Organization = organizationId;
		ManagedBy = managedBy;
		PaidTimeOff = paidTimeOff;
		Raise(new EmployeeJoined(this));
	}

	public void UpdateDetails(string? email, string? name, string? title, int paidTimeOff, bool isOrgOwner)
	{
		if (email != null)
		{
			Email = email;
		}

		if (name != null)
		{
			Name = name;
		}

		if (title != null)
		{
			Title = title;
		}

		if (paidTimeOff != 0)
		{
			PaidTimeOff = paidTimeOff;
		}

		IsOrgOwner = isOrgOwner;
	}

	public bool ChangeCountry(string? country)
	{
		if (Country == country)
		{
			return false;
		}

		Country = country;
		return true;
	}

	public void NotifyJoined() => Raise(new EmployeeJoined(this));

	public void AssignManager(int? managerId) => ManagedBy = managerId;

	public void ReassignReportsTo(int? newManagerId) => ManagedBy = newManagerId;

	public void EnsureCanBeDeleted(bool hasSubordinates)
	{
		if (IsOrgHead && hasSubordinates)
		{
			throw new DomainException("Cannot delete the head of the organization because they manage other employees.");
		}
	}

	public void DeactivateAsOwner()
	{
		Country = null;
		ManagedBy = null;
		PaidTimeOff = 0;
		Title = null;
	}

	public void EnsureOrganizationKeepsAnAdmin(bool anotherOwnerExists)
	{
		if (IsOrgOwner && !anotherOwnerExists)
		{
			throw new DomainException("You can't leave the organization without admins");
		}
	}
}
