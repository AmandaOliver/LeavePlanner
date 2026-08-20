namespace LeavePlanner.Application.Common;

public enum AccessDenial
{
	None,
	Unauthorized,
	Forbidden,
	NotFound,
	BadRequest
}

public interface IAccessChecker
{
	Task<AccessDenial> EnsureAdmin(string? email, string? organizationId, string? employeeId, CancellationToken cancellationToken);
	Task<AccessDenial> EnsureSelf(string? email, string? employeeId, CancellationToken cancellationToken);
	Task<AccessDenial> EnsureSelfEmailOrAdmin(string? email, string? requestedEmail, CancellationToken cancellationToken);
	Task<AccessDenial> EnsureOrganizationMember(string? email, string? organizationId, CancellationToken cancellationToken);
	Task<AccessDenial> EnsureManagerOfRequest(string? email, string? requestId, string? employeeId, CancellationToken cancellationToken);
	Task<AccessDenial> EnsureLeaveOwnerOrManager(string? email, string? leaveId, CancellationToken cancellationToken);
}
