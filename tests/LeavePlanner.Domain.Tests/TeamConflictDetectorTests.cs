using LeavePlanner.Domain;

namespace LeavePlanner.Domain.Tests;

public class TeamConflictDetectorTests
{
	[Fact]
	public void Finds_overlapping_approved_leave_for_teammates()
	{
		var request = Leave.Rehydrate(10, LeaveTypes.PaidTimeOff, new DateTime(2026, 8, 10), new DateTime(2026, 8, 14), owner: 1);
		var overlapping = Leave.Rehydrate(11, LeaveTypes.PaidTimeOff, new DateTime(2026, 8, 12), new DateTime(2026, 8, 16), owner: 2, approvedBy: 9);
		var later = Leave.Rehydrate(12, LeaveTypes.PaidTimeOff, new DateTime(2026, 9, 1), new DateTime(2026, 9, 5), owner: 3, approvedBy: 9);

		var conflicts = TeamConflictDetector.Find(request, [
			(2, "Alex", [overlapping]),
			(3, "Sam", [later])
		]);

		var conflict = Assert.Single(conflicts);
		Assert.Equal(2, conflict.EmployeeId);
		Assert.Equal("Alex", conflict.Name);
		Assert.Equal(11, Assert.Single(conflict.Leaves).Id);
	}

	[Fact]
	public void Ignores_the_requester_when_they_appear_in_the_team_list()
	{
		var request = Leave.Rehydrate(10, LeaveTypes.PaidTimeOff, new DateTime(2026, 8, 10), new DateTime(2026, 8, 14), owner: 1);
		var ownLeave = Leave.Rehydrate(11, LeaveTypes.PaidTimeOff, new DateTime(2026, 8, 10), new DateTime(2026, 8, 14), owner: 1, approvedBy: 9);

		Assert.Empty(TeamConflictDetector.Find(request, [(1, "Self", [ownLeave])]));
	}
}
