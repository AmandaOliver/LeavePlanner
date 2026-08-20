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

	public async Task<AccessDenial> EnsureAdmin(string? email, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		return employee == null || !employee.IsOrgOwner ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureSelf(string? email, string? employeeId, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		if (employee == null)
		{
			return AccessDenial.Unauthorized;
		}

		if (employeeId == null || !int.TryParse(employeeId, out var requestedEmployeeId))
		{
			return AccessDenial.BadRequest;
		}

		return employee.Id != requestedEmployeeId ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureSelfEmailOrAdmin(string? email, string? requestedEmail, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		if (employee != null && employee.IsOrgOwner)
		{
			return AccessDenial.None;
		}

		if (requestedEmail == null)
		{
			return AccessDenial.BadRequest;
		}

		return string.Equals(email, requestedEmail, StringComparison.OrdinalIgnoreCase)
			? AccessDenial.None
			: AccessDenial.Forbidden;
	}

	public async Task<AccessDenial> EnsureOrganizationMember(string? email, string? organizationId, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		if (employee == null)
		{
			return AccessDenial.Unauthorized;
		}

		if (organizationId == null || !int.TryParse(organizationId, out var requestedOrganizationId))
		{
			return AccessDenial.BadRequest;
		}

		return employee.Organization != requestedOrganizationId ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureManagerOfRequest(string? email, string? requestId, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var manager = await _employees.GetByEmailAsync(email, cancellationToken);
		if (manager == null)
		{
			return AccessDenial.Unauthorized;
		}

		if (requestId == null || !int.TryParse(requestId, out var id))
		{
			return AccessDenial.BadRequest;
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

		return leaveOwner.ManagedBy != manager.Id ? AccessDenial.Forbidden : AccessDenial.None;
	}

	public async Task<AccessDenial> EnsureLeaveOwnerOrManager(string? email, string? leaveId, CancellationToken cancellationToken)
	{
		if (email == null)
		{
			return AccessDenial.Unauthorized;
		}

		var employee = await _employees.GetByEmailAsync(email, cancellationToken);
		if (employee == null)
		{
			return AccessDenial.Unauthorized;
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

		if (leave.Owner == employee.Id)
		{
			return AccessDenial.None;
		}

		var owner = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
		return owner?.ManagedBy == employee.Id ? AccessDenial.None : AccessDenial.Forbidden;
	}
}
