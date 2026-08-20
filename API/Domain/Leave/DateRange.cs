namespace LeavePlanner.Domain;

public readonly record struct DateRange(DateTime Start, DateTime End)
{
	public static DateRange Between(DateTime start, DateTime end)
	{
		if (end < start)
		{
			throw new DomainException("The end date cannot be before the start date.");
		}

		return new DateRange(start, end);
	}

	public bool Overlaps(DateRange other) =>
		(Start >= other.Start && Start < other.End) ||
		(End > other.Start && End <= other.End) ||
		(Start < other.Start && End > other.End);

	public bool Contains(DateTime date) => date >= Start && date < End;

	public int DaySpan => (End - Start).Days;

	public bool CrossesYearBoundary => Start.Year != End.Year;
}
