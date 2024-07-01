using Microsoft.EntityFrameworkCore;
using LeavePlanner.Data;

// https://www.googleapis.com/calendar/v3/calendars/en.italian%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c

public static class CountriesController
{
    public static void MapCountriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/countries", GetCountries).RequireAuthorization();
    }

    public static async Task<IResult> GetCountries(LeavePlannerContext context)
    {
		try
    	{
        	return Results.Ok(await context.Countries.ToListAsync());
		}
		catch (Exception ex)
		{
			return Results.Problem(ex.Message);  
		}
    }
}