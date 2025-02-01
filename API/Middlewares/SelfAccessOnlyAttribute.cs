using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using LeavePlanner.Data;
using System.Security.Claims;

public class SelfAccessOnlyAttribute : Attribute, IAuthorizationFilter
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

		// Retrieve the employee associated with the email from the JWT
		var employee = dbContext.Employees.FirstOrDefault(e => e.Email == userEmail);
		if (employee == null)
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		// Extract the employeeId from the route data
		var routeData = context.RouteData.Values["employeeId"]?.ToString();
		if (routeData == null || !int.TryParse(routeData, out int requestedEmployeeId))
		{
			context.Result = new BadRequestResult();
			return;
		}

		// Ensure that the authenticated employee is accessing their own data
		if (employee.Id != requestedEmployeeId)
		{
			context.Result = new ForbidResult();
		}
	}
}
