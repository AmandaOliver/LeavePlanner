using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class AccessPolicyTests
{
	[Fact]
	public void Org_admin_cannot_administer_another_organization()
	{
		var admin = Employee.Rehydrate(1, "admin@acme.com", organization: 10, isOrgOwner: true);

		Assert.True(AccessPolicy.CanAdministerOrganization(admin, 10));
		Assert.False(AccessPolicy.CanAdministerOrganization(admin, 99));
	}

	[Fact]
	public void Non_admin_cannot_administer_their_own_organization()
	{
		var employee = Employee.Rehydrate(2, "alex@acme.com", organization: 10, isOrgOwner: false);
		Assert.False(AccessPolicy.CanAdministerOrganization(employee, 10));
	}

	[Fact]
	public void Org_admin_cannot_view_or_edit_an_employee_in_another_org()
	{
		var admin = Employee.Rehydrate(1, "admin@acme.com", organization: 10, isOrgOwner: true);
		var other = Employee.Rehydrate(3, "sam@other.com", organization: 99, isOrgOwner: false);

		Assert.False(AccessPolicy.CanAdministerEmployee(admin, other));
		Assert.False(AccessPolicy.CanViewEmployee(admin, other));
	}

	[Fact]
	public void Org_admin_can_view_an_employee_in_their_own_org()
	{
		var admin = Employee.Rehydrate(1, "admin@acme.com", organization: 10, isOrgOwner: true);
		var teammate = Employee.Rehydrate(2, "alex@acme.com", organization: 10, isOrgOwner: false);

		Assert.True(AccessPolicy.CanViewEmployee(admin, teammate));
		Assert.True(AccessPolicy.CanAdministerEmployee(admin, teammate));
	}

	[Fact]
	public void Manager_cannot_review_a_request_from_another_organization()
	{
		var manager = Employee.Rehydrate(1, "manager@acme.com", organization: 10);
		var otherOrgReport = Employee.Rehydrate(4, "sam@other.com", organization: 99, managedBy: 1);

		Assert.False(AccessPolicy.CanReviewAsManager(manager, otherOrgReport));
		Assert.False(AccessPolicy.CanManageLeave(manager, otherOrgReport));
	}

	[Fact]
	public void Manager_can_review_a_direct_report_in_the_same_organization()
	{
		var manager = Employee.Rehydrate(1, "manager@acme.com", organization: 10);
		var report = Employee.Rehydrate(2, "alex@acme.com", organization: 10, managedBy: 1);

		Assert.True(AccessPolicy.CanReviewAsManager(manager, report));
		Assert.True(AccessPolicy.CanManageLeave(manager, report));
	}
}
