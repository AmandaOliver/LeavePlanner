using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class OrganizationTests
{
	[Fact]
	public void Create_rejects_an_empty_name()
	{
		Assert.Throws<DomainException>(() => Organization.Create(""));
	}

	[Fact]
	public void ChangeWorkingDays_rejects_an_empty_or_invalid_set()
	{
		var organization = Organization.Create("Acme");
		Assert.Throws<DomainException>(() => organization.ChangeWorkingDays([]));
		Assert.Throws<DomainException>(() => organization.ChangeWorkingDays([0, 1, 2]));
		Assert.Throws<DomainException>(() => organization.ChangeWorkingDays([1, 8]));
	}

	[Fact]
	public void ChangeWorkingDays_accepts_a_valid_set()
	{
		var organization = Organization.Create("Acme");
		organization.ChangeWorkingDays([1, 2, 3, 4]);
		Assert.Equal([1, 2, 3, 4], organization.WorkingDays);
	}
}

public class EmployeeTests
{
	[Fact]
	public void EnsureCanBeDeleted_rejects_a_head_who_still_manages_people()
	{
		var head = Employee.Rehydrate(1, "head@org.com", managedBy: null);
		Assert.Throws<DomainException>(() => head.EnsureCanBeDeleted(hasSubordinates: true));
	}

	[Fact]
	public void EnsureOrganizationKeepsAnAdmin_rejects_removing_the_last_owner()
	{
		var owner = Employee.Rehydrate(1, "owner@org.com", isOrgOwner: true);
		Assert.Throws<DomainException>(() => owner.EnsureOrganizationKeepsAnAdmin(anotherOwnerExists: false));
	}

	[Fact]
	public void Hire_raises_EmployeeJoined()
	{
		var employee = Employee.Hire("alex@org.com", "Alex", "Engineer", "GB", 8, 1, 25, false);
		Assert.Contains(employee.DomainEvents, domainEvent => domainEvent is EmployeeJoined);
	}
}
