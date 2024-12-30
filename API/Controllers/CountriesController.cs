// https://www.googleapis.com/calendar/v3/calendars/en.italian%23holiday%40group.v.calendar.google.com/events?key=AIzaSyD8hdrcLyIKD6lXD-0nGAPWJerZz1c3n5c
public static class CountriesEndpointsExtensions
{
	public static void MapCountriesEndpoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/countries", async (CountriesController controller) => await controller.GetCountries()).RequireAuthorization();
	}
}
public class CountriesController
{
	private readonly CountriesService _countriesService;

	public CountriesController(CountriesService countriesService)
	{
		_countriesService = countriesService;
	}

	public async Task<IResult> GetCountries()
	{
		var result = await _countriesService.GetCountries();
		if (!result.IsSuccess)
		{
			return Results.NotFound(result.ErrorMessage);
		}
		return Results.Ok(result.countries);
	}
}