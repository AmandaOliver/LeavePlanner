using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LeavePlanner.Data;
using Microsoft.EntityFrameworkCore;


public class LeaveOwnerOrManagerAttribute : Attribute, IAuthorizationFilter
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

		// Get the authenticated user
		var employee = dbContext.Employees.FirstOrDefault(e => e.Email == userEmail);
		if (employee == null)
		{
			context.Result = new UnauthorizedResult();
			return;
		}

		// Extract leaveId from route
		var routeData = context.RouteData.Values["leaveId"]?.ToString();
		if (routeData == null || !int.TryParse(routeData, out int leaveId))
		{
			context.Result = new BadRequestResult();
			return;
		}

		// Find the leave entry
		var leave = dbContext.Leaves
		   .Include(l => l.OwnerNavigation)
		   .FirstOrDefault(l => l.Id == leaveId);
		if (leave == null)
		{
			context.Result = new NotFoundResult();
			return;
		}

		// Check if the user is:
		// - The owner of the leave
		// - The manager of the leave owner
		if (leave.Owner == employee.Id || employee.Id == leave.OwnerNavigation?.ManagedBy)
		{
			return; // Authorized, continue to the controller
		}

		// If none of the conditions match, forbid access
		context.Result = new ForbidResult();
	}
}
