using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class WorkingDayCalculatorTests
{
	private static readonly int[] Weekdays = [1, 2, 3, 4, 5];

	[Fact]
	public void Counts_weekdays_in_the_requested_year_and_skips_weekends()
	{
		// Monday 10 Aug 2026 through Friday 14 Aug 2026, exclusive end on Saturday.
		var start = new DateTime(2026, 8, 10);
		var end = new DateTime(2026, 8, 15);

		Assert.Equal(5, WorkingDayCalculator.Count(start, end, 2026, Weekdays, []));
	}

	[Fact]
	public void Skips_days_covered_by_blocking_leaves()
	{
		var start = new DateTime(2026, 8, 10);
		var end = new DateTime(2026, 8, 15);
		var publicHoliday = Leave.Rehydrate(
			1,
			LeaveTypes.BankHoliday,
			new DateTime(2026, 8, 12),
			new DateTime(2026, 8, 13),
			owner: 2,
			approvedBy: 1);

		Assert.Equal(4, WorkingDayCalculator.Count(start, end, 2026, Weekdays, [publicHoliday]));
	}

	[Fact]
	public void Counts_only_days_in_the_requested_year()
	{
		var start = new DateTime(2026, 12, 30);
		var end = new DateTime(2027, 1, 3);

		Assert.Equal(2, WorkingDayCalculator.Count(start, end, 2026, Weekdays, []));
		Assert.Equal(1, WorkingDayCalculator.Count(start, end, 2027, Weekdays, []));
	}

	[Fact]
	public void Overlaps_when_the_requested_range_intersects_another_leave()
	{
		var other = Leave.Rehydrate(1, LeaveTypes.PaidTimeOff, new DateTime(2026, 8, 12), new DateTime(2026, 8, 16), 2);
		Assert.True(WorkingDayCalculator.Overlaps(new DateTime(2026, 8, 10), new DateTime(2026, 8, 13), other));
		Assert.False(WorkingDayCalculator.Overlaps(new DateTime(2026, 8, 16), new DateTime(2026, 8, 18), other));
	}
}

public class PaidTimeOffCalculatorTests
{
	[Fact]
	public void Remaining_is_allowance_minus_days_already_taken()
	{
		Assert.Equal(18, PaidTimeOffCalculator.Remaining(25, 7));
	}
}
