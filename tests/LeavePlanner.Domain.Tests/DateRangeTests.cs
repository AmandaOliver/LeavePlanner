using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class DateRangeTests
{
	[Fact]
	public void Between_rejects_an_end_before_the_start()
	{
		var start = new DateTime(2026, 8, 20);
		Assert.Throws<DomainException>(() => DateRange.Between(start, start.AddDays(-1)));
	}

	[Fact]
	public void Overlaps_when_ranges_share_any_day()
	{
		var first = DateRange.Between(new DateTime(2026, 8, 10), new DateTime(2026, 8, 15));
		var second = DateRange.Between(new DateTime(2026, 8, 14), new DateTime(2026, 8, 20));

		Assert.True(first.Overlaps(second));
	}

	[Fact]
	public void Does_not_overlap_adjacent_ranges()
	{
		var first = DateRange.Between(new DateTime(2026, 8, 10), new DateTime(2026, 8, 15));
		var second = DateRange.Between(new DateTime(2026, 8, 15), new DateTime(2026, 8, 20));

		Assert.False(first.Overlaps(second));
	}

	[Fact]
	public void CrossesYearBoundary_when_start_and_end_are_in_different_years()
	{
		var range = DateRange.Between(new DateTime(2026, 12, 28), new DateTime(2027, 1, 4));
		Assert.True(range.CrossesYearBoundary);
		Assert.Equal(7, range.DaySpan);
	}
}
