using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class LeavePolicyTests
{
	private static readonly DateTime Today = new(2026, 8, 20);

	[Fact]
	public void Rejects_a_new_public_holiday_request()
	{
		var error = Assert.Throws<DomainException>(() =>
			LeavePolicy.AssertCanRequest(
				LeaveTypes.BankHoliday,
				Today.AddDays(1),
				Today.AddDays(2),
				Today,
				existing: null,
				daysThisYear: 0,
				remainingThisYear: 25,
				daysNextYear: 0,
				remainingNextYear: 25));

		Assert.Equal("You can't request a new public holiday", error.Message);
	}

	[Fact]
	public void Rejects_dates_in_the_past()
	{
		var error = Assert.Throws<DomainException>(() =>
			LeavePolicy.AssertCanRequest(
				LeaveTypes.PaidTimeOff,
				Today.AddDays(-2),
				Today.AddDays(-1),
				Today,
				existing: null,
				daysThisYear: 1,
				remainingThisYear: 25,
				daysNextYear: 0,
				remainingNextYear: 25));

		Assert.Equal("You cannot request leave for dates in the past.", error.Message);
	}

	[Fact]
	public void Rejects_an_end_date_before_the_start()
	{
		Assert.Throws<DomainException>(() =>
			LeavePolicy.AssertCanRequest(
				LeaveTypes.PaidTimeOff,
				Today.AddDays(5),
				Today.AddDays(2),
				Today,
				existing: null,
				daysThisYear: 0,
				remainingThisYear: 25,
				daysNextYear: 0,
				remainingNextYear: 25));
	}

	[Fact]
	public void Rejects_more_paid_time_off_than_remaining()
	{
		var error = Assert.Throws<DomainException>(() =>
			LeavePolicy.AssertCanRequest(
				LeaveTypes.PaidTimeOff,
				Today.AddDays(1),
				Today.AddDays(6),
				Today,
				existing: null,
				daysThisYear: 5,
				remainingThisYear: 2,
				daysNextYear: 0,
				remainingNextYear: 25));

		Assert.Contains("You cannot request more days than you have left", error.Message);
	}

	[Fact]
	public void Allows_paid_time_off_within_the_remaining_balance()
	{
		LeavePolicy.AssertCanRequest(
			LeaveTypes.PaidTimeOff,
			Today.AddDays(1),
			Today.AddDays(3),
			Today,
			existing: null,
			daysThisYear: 2,
			remainingThisYear: 25,
			daysNextYear: 0,
			remainingNextYear: 25);
	}

	[Fact]
	public void Rejects_amending_an_already_taken_leave()
	{
		var existing = Leave.Rehydrate(
			1,
			LeaveTypes.PaidTimeOff,
			Today.AddDays(-3),
			Today.AddDays(-1),
			owner: 2,
			approvedBy: 1);

		Assert.Throws<DomainException>(() =>
			LeavePolicy.AssertCanRequest(
				LeaveTypes.PaidTimeOff,
				Today.AddDays(-1),
				Today.AddDays(3),
				Today,
				existing,
				daysThisYear: 2,
				remainingThisYear: 25,
				daysNextYear: 0,
				remainingNextYear: 25));
	}
}
