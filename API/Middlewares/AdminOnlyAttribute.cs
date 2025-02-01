using Microsoft.AspNetCore.Mvc;
using LeavePlanner.Data;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Security.Claims;

public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
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

		var employee = dbContext.Employees.FirstOrDefault(e => e.Email == userEmail);
		if (employee == null || !employee.IsOrgOwner)
		{
			context.Result = new ForbidResult();
		}
	}
}
