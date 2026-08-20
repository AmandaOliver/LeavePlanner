using LeavePlanner.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

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
		var organizationId = context.RouteData.Values["organizationId"]?.ToString();
		var employeeId = context.RouteData.Values["id"]?.ToString();
		if (organizationId == null && employeeId == null)
		{
			organizationId = await OrganizationIdFromBody(context.HttpContext.Request, context.HttpContext.RequestAborted);
		}

		AccessResult.Apply(
			context,
			await checker.EnsureAdmin(AccessResult.Email(context), organizationId, employeeId, context.HttpContext.RequestAborted));
	}

	private static async Task<string?> OrganizationIdFromBody(HttpRequest request, CancellationToken cancellationToken)
	{
		request.EnableBuffering();
		using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
		var body = await reader.ReadToEndAsync(cancellationToken);
		request.Body.Position = 0;
		if (string.IsNullOrWhiteSpace(body))
		{
			return null;
		}

		using var document = JsonDocument.Parse(body);
		if (!document.RootElement.TryGetProperty("organization", out var organization))
		{
			return null;
		}

		return organization.ValueKind == JsonValueKind.Number
			? organization.GetInt32().ToString()
			: organization.GetString();
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
		var employeeId = context.RouteData.Values["employeeId"]?.ToString();
		AccessResult.Apply(
			context,
			await checker.EnsureManagerOfRequest(AccessResult.Email(context), requestId, employeeId, context.HttpContext.RequestAborted));
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
