using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using LeavePlanner.Data;

public class OrganizationMemberOnlyAttribute : Attribute, IAuthorizationFilter
{
	public void OnAuthorization(AuthorizationFilterContext context)
	{
		var userEmail = context.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
		if (userEmail == null)
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		using var scope = context.HttpContext.RequestServices.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<LeavePlannerContext>();

		// Get the employee associated with the JWT email
		var employee = dbContext.Employees.FirstOrDefault(e => e.Email == userEmail);
		if (employee == null)
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		// Extract organizationId from the route
		var routeData = context.RouteData.Values["organizationId"]?.ToString();
		if (routeData == null || !int.TryParse(routeData, out int requestedOrganizationId))
		{
			context.Result = new BadRequestResult();
			return;
		}

		// Ensure that the employee belongs to the requested organization
		if (employee.Organization != requestedOrganizationId)
		{
			context.Result = new ForbidResult(); // 403 Forbidden
		}
	}
}
