namespace LeavePlanner.Domain;

public static class AccessPolicy
{
	public static bool CanAdministerOrganization(Employee caller, int? organizationId) =>
		caller.IsOrgOwner && organizationId != null && caller.Organization == organizationId;

	public static bool CanAdministerEmployee(Employee caller, Employee target) =>
		CanAdministerOrganization(caller, target.Organization);

	public static bool CanViewEmployee(Employee caller, Employee target) =>
		string.Equals(caller.Email, target.Email, StringComparison.OrdinalIgnoreCase)
		|| CanAdministerEmployee(caller, target);

	public static bool CanManageLeave(Employee caller, Employee owner) =>
		caller.Id == owner.Id
		|| (owner.ManagedBy == caller.Id && caller.Organization == owner.Organization);

	public static bool CanReviewAsManager(Employee manager, Employee owner) =>
		owner.ManagedBy == manager.Id && manager.Organization == owner.Organization;
}
