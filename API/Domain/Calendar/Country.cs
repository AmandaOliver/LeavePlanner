namespace LeavePlanner.Domain;

public class Country
{
	public string Code { get; set; } = null!;

	public string Name { get; set; } = null!;
}

public record PublicHoliday(DateTime Start, DateTime End, string? Summary);
