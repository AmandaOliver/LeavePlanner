using LeavePlanner.Domain;

namespace LeavePlanner.Models;

public static class Mappings
{
	public static LeaveDTO ToLeaveDto(this Leave leave, string? ownerName = null) =>
		new()
		{
			Id = leave.Id,
			Type = leave.Type,
			Owner = leave.Owner,
			OwnerName = ownerName,
			DateStart = leave.DateStart,
			DateEnd = leave.DateEnd,
			Description = leave.Description,
			ApprovedBy = leave.ApprovedBy,
			RejectedBy = leave.RejectedBy
		};

	public static EmployeeDTO ToEmployeeDto(this Employee employee) =>
		new()
		{
			Id = employee.Id,
			Email = employee.Email,
			Name = employee.Name,
			Organization = employee.Organization,
			ManagedBy = employee.ManagedBy,
			Country = employee.Country,
			IsOrgOwner = employee.IsOrgOwner,
			PaidTimeOff = employee.PaidTimeOff,
			Title = employee.Title
		};

	public static OrganizationDTO ToOrganizationDto(this Organization organization) =>
		new()
		{
			Id = organization.Id,
			Name = organization.Name,
			WorkingDays = organization.WorkingDays
		};
}
