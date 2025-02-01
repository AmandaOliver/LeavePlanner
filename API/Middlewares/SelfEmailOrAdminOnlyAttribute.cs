using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using LeavePlanner.Data;
using System.Security.Claims;

public class SelfEmailOrAdminOnlyAttribute : Attribute, IAuthorizationFilter
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
        // if its an admin has access to everything
        if (employee != null && employee.IsOrgOwner)
        {
            return;
        }

        // Extract the requested email from the route
        var routeData = context.RouteData.Values["email"]?.ToString();
        if (routeData == null)
        {
            context.Result = new BadRequestResult();
            return;
        }

        // Allow if the user is requesting their own data or they are an admin
        if (!string.Equals(userEmail, routeData, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();
        }
    }
}
