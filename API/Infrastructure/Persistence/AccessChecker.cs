using LeavePlanner.Application.Common;
using LeavePlanner.Domain;

namespace LeavePlanner.Infrastructure.Persistence;

public class AccessChecker : IAccessChecker
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;

	public AccessChecker(IEmployeeRepository employees, ILeaveRepository leaves)
	{
		_employees = employees;
		_leaves = leaves;
	}

	public async Task<AccessDenial> EnsureAdmin(string? email, string? organizationId, string? employeeId, CancellationToken cancellationToken)
	{
		var caller = await CallerOrDenial(email, cancellationToken);
		if (caller.Denial != AccessDenial.None)
		{
			return caller.Denial;
		}

		if (!caller.Employee!.IsOrgOwner)
		{
			return AccessDenial.Forbidden;
		}

		int? targetOrganization = null;
		if (organizationId != null)
		{
			if (!int.TryParse(organizationId, out var parsedOrganizationId))
			{
				return AccessDenial.BadRequest;
			}

			targetOrganization = parsedOrganizationId;
		}
		else if (employeeId != null)
		{
			if (!int.TryParse(employeeId, out var parsedEmployeeId))
			{
				return AccessDenial.BadRequest;
			}

			var target = await _employees.GetByIdAsync(parsedEmployeeId, cancellationToken);
			if (target == null)
			{
				return AccessDenial.NotFound;
			}

			targetOrganization = target.Organization;
		}
		else
		{
			return AccessDenial.BadRequest;
		}

		return AccessPolicy.CanAdministerOrganization(caller.Employee, targetOrganization)
			? AccessDenial.None
			: AccessDenial.Forbidden;
	}

	public async Task<AccessDenial> EnsureSelf(string? email, string? employeeId, CancellationToken cancellationToken)
	{
		var caller = await CallerOrDenial(email, cancellationToken);
		if (caller.Denial != AccessDenial.None)
		{
			return caller.Denial;
		}

		if (employeeId == null || !int.TryParse(employeeId, out var requestedEmployeeId))
		{
			return AccessDenial.BadRequest;
		}

		return caller.Employee!.Id != requestedEmployeeId ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureSelfEmailOrAdmin(string? email, string? requestedEmail, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		if (requestedEmail == null)
		{
			return AccessDenial.BadRequest;
		}

		if (string.Equals(email, requestedEmail, StringComparison.OrdinalIgnoreCase))
		{
			return AccessDenial.None;
		}

		var caller = await _employees.GetByEmailAsync(email, cancellationToken);
		if (caller == null || !caller.IsOrgOwner)
		{
			return AccessDenial.Forbidden;
		}

		var target = await _employees.GetByEmailAsync(requestedEmail, cancellationToken);
		if (target == null)
		{
			return AccessDenial.NotFound;
		}

		return AccessPolicy.CanViewEmployee(caller, target) ? AccessDenial.None : AccessDenial.Forbidden;
	}

	public async Task<AccessDenial> EnsureOrganizationMember(string? email, string? organizationId, CancellationToken cancellationToken)
	{
		var caller = await CallerOrDenial(email, cancellationToken);
		if (caller.Denial != AccessDenial.None)
		{
			return caller.Denial;
		}

		if (organizationId == null || !int.TryParse(organizationId, out var requestedOrganizationId))
		{
			return AccessDenial.BadRequest;
		}

		return caller.Employee!.Organization != requestedOrganizationId ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureManagerOfRequest(string? email, string? requestId, string? employeeId, CancellationToken cancellationToken)
	{
		var caller = await CallerOrDenial(email, cancellationToken);
		if (caller.Denial != AccessDenial.None)
		{
			return caller.Denial;
		}

		if (requestId == null || !int.TryParse(requestId, out var id))
		{
			return AccessDenial.BadRequest;
		}

		if (employeeId != null)
		{
			if (!int.TryParse(employeeId, out var actingEmployeeId) || actingEmployeeId != caller.Employee!.Id)
			{
				return AccessDenial.Forbidden;
			}
		}

		var leaveRequest = await _leaves.GetByIdAsync(id, cancellationToken);
		if (leaveRequest == null)
		{
			return AccessDenial.NotFound;
		}

		var leaveOwner = await _employees.GetByIdAsync(leaveRequest.Owner, cancellationToken);
		if (leaveOwner == null)
		{
			return AccessDenial.NotFound;
		}

		return AccessPolicy.CanReviewAsManager(caller.Employee!, leaveOwner)
			? AccessDenial.None
			: AccessDenial.Forbidden;
	}

	public async Task<AccessDenial> EnsureLeaveOwnerOrManager(string? email, string? leaveId, CancellationToken cancellationToken)
	{
		var caller = await CallerOrDenial(email, cancellationToken);
		if (caller.Denial != AccessDenial.None)
		{
			return caller.Denial;
		}

		if (leaveId == null || !int.TryParse(leaveId, out var id))
		{
			return AccessDenial.BadRequest;
		}

		var leave = await _leaves.GetByIdAsync(id, cancellationToken);
		if (leave == null)
		{
			return AccessDenial.NotFound;
		}

		var owner = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
		if (owner == null)
		{
			return AccessDenial.NotFound;
		}

		return AccessPolicy.CanManageLeave(caller.Employee!, owner)
			? AccessDenial.None
			: AccessDenial.Forbidden;
	}

	private async Task<(Employee? Employee, AccessDenial Denial)> CallerOrDenial(string? email, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return (null, AccessDenial.Unauthorized);
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		return employee == null
			? (null, AccessDenial.Unauthorized)
			: (employee, AccessDenial.None);
	}
}
