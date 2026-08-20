using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class EmployeePolicyTests
{
	[Fact]
	public void Rejects_an_active_duplicate_email()
	{
		var existing = Employee.Rehydrate(1, "alex@org.com", country: "GB");

		var error = Assert.Throws<DomainException>(() =>
			EmployeePolicy.AssertCanHire(
				"alex@org.com",
				"Alex",
				"Engineer",
				"GB",
				25,
				existing,
				manager: null,
				countryExists: true));

		Assert.Equal("There is an existing employee with the same email", error.Message);
	}

	[Fact]
	public void Rejects_an_invalid_email()
	{
		Assert.Throws<DomainException>(() =>
			EmployeePolicy.AssertCanHire(
				"not-an-email",
				"Alex",
				"Engineer",
				"GB",
				25,
				existingWithEmail: null,
				manager: null,
				countryExists: true));
	}

	[Fact]
	public void Rejects_being_managed_by_yourself()
	{
		var manager = Employee.Rehydrate(1, "alex@org.com");

		var error = Assert.Throws<DomainException>(() =>
			EmployeePolicy.AssertCanHire(
				"alex@org.com",
				"Alex",
				"Engineer",
				"GB",
				25,
				existingWithEmail: null,
				manager,
				countryExists: true));

		Assert.Equal("An employee can't be managed by himself", error.Message);
	}

	[Fact]
	public void Rejects_an_unknown_country()
	{
		Assert.Throws<DomainException>(() =>
			EmployeePolicy.AssertCanHire(
				"alex@org.com",
				"Alex",
				"Engineer",
				"ZZ",
				25,
				existingWithEmail: null,
				manager: null,
				countryExists: false));
	}

	[Fact]
	public void Rejects_paid_time_off_below_one_day()
	{
		Assert.Throws<DomainException>(() =>
			EmployeePolicy.AssertCanHire(
				"alex@org.com",
				"Alex",
				"Engineer",
				"GB",
				0,
				existingWithEmail: null,
				manager: null,
				countryExists: true));
	}

	[Fact]
	public void AssertManagerExists_when_a_manager_id_was_given_but_not_found()
	{
		Assert.Throws<DomainException>(() => EmployeePolicy.AssertManagerExists(null, managerWasSpecified: true));
	}

	[Fact]
	public void Allows_a_valid_hire()
	{
		EmployeePolicy.AssertCanHire(
			"alex@org.com",
			"Alex",
			"Engineer",
			"GB",
			25,
			existingWithEmail: null,
			manager: null,
			countryExists: true);
	}
}
