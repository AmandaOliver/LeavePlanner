using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using LeavePlanner.Data;


public class ManagerOnlyAttribute : Attribute, IAuthorizationFilter
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

		// Get the authenticated manager (the one making the request)
		var manager = dbContext.Employees.FirstOrDefault(e => e.Email == userEmail);
		if (manager == null)
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		// Extract the requestId from the route
		var requestData = context.RouteData.Values["requestId"]?.ToString();
		if (requestData == null || !int.TryParse(requestData, out int requestId))
		{
			context.Result = new BadRequestResult();
			return;
		}

		// Get the leave request and include the Owner (employee who made the request)
		var leaveRequest = dbContext.Leaves
			.FirstOrDefault(l => l.Id == requestId);

		if (leaveRequest == null)
		{
			context.Result = new NotFoundResult();
			return;
		}

		// Get the employee who requested the leave
		var leaveOwner = dbContext.Employees.FirstOrDefault(e => e.Id == leaveRequest.Owner);
		if (leaveOwner == null)
		{
			context.Result = new NotFoundResult();
			return;
		}

		// Ensure the leave request belongs to one of the manager's subordinates
		if (leaveOwner.ManagedBy != manager.Id)
		{
			context.Result = new ForbidResult(); // 403 Forbidden
		}
	}
}
