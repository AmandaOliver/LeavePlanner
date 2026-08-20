using LeavePlanner.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

internal static class AccessResult
{
	public static void Apply(AuthorizationFilterContext context, AccessDenial denial)
	{
		context.Result = denial switch
		{
			AccessDenial.Unauthorized => new UnauthorizedResult(),
			AccessDenial.Forbidden => new ForbidResult(),
			AccessDenial.NotFound => new NotFoundResult(),
			AccessDenial.BadRequest => new BadRequestResult(),
			_ => null
		};
	}

	public static string? Email(AuthorizationFilterContext context) =>
		context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
}

public class AdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		AccessResult.Apply(context, await checker.EnsureAdmin(AccessResult.Email(context), context.HttpContext.RequestAborted));
	}
}

public class SelfAccessOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		var employeeId = context.RouteData.Values["employeeId"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureSelf(AccessResult.Email(context), employeeId, context.HttpContext.RequestAborted));
	}
}

public class SelfEmailOrAdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		var email = context.RouteData.Values["email"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureSelfEmailOrAdmin(AccessResult.Email(context), email, context.HttpContext.RequestAborted));
	}
}

public class OrganizationMemberOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		var organizationId = context.RouteData.Values["organizationId"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureOrganizationMember(AccessResult.Email(context), organizationId, context.HttpContext.RequestAborted));
	}
}

public class ManagerOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		var requestId = context.RouteData.Values["requestId"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureManagerOfRequest(AccessResult.Email(context), requestId, context.HttpContext.RequestAborted));
	}
}

public class LeaveOwnerOrManagerAttribute : Attribute, IAsyncAuthorizationFilter
{
	public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		var checker = context.HttpContext.RequestServices.GetRequiredService<IAccessChecker>();
		var leaveId = context.RouteData.Values["leaveId"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureLeaveOwnerOrManager(AccessResult.Email(context), leaveId, context.HttpContext.RequestAborted));
	}
}
