using System.Text.RegularExpressions;

namespace LeavePlanner.Domain;

public static class LeavePolicy
{
	public static void AssertCanRequest(
		string type,
		DateTime start,
		DateTime end,
		DateTime utcToday,
		Leave? existing,
		int daysThisYear,
		int remainingThisYear,
		int daysNextYear,
		int remainingNextYear)
	{
		if (existing != null)
		{
			if (existing.IsApproved && start < utcToday)
			{
				throw new DomainException("You cannot update an already taken leave");
			}

			if (type == LeaveTypes.BankHoliday && (end - start).Days > 1)
			{
				throw new DomainException("You cannot request more than 1 day of public holidays");
			}
		}
		else if (type == LeaveTypes.BankHoliday)
		{
			throw new DomainException("You can't request a new public holiday");
		}

		if (start < utcToday || end < utcToday)
		{
			throw new DomainException("You cannot request leave for dates in the past.");
		}

		if (end < start)
		{
			throw new DomainException("The end date cannot be before the start date.");
		}

		if (type != LeaveTypes.PaidTimeOff)
		{
			return;
		}

		if (start.Year != end.Year)
		{
			if (daysThisYear > remainingThisYear)
			{
				throw new DomainException(
					$"You cannot request more days than you have left.\nDays requested: {daysThisYear}.\nDays left for the year {start.Year}: {remainingThisYear}.");
			}

			if (daysNextYear > remainingNextYear)
			{
				throw new DomainException(
					$"You cannot request more days than you have left.\nDays requested: {daysNextYear}.\nDays left for the year {end.Year}: {remainingNextYear}.");
			}

			return;
		}

		if (daysThisYear > remainingThisYear)
		{
			throw new DomainException(
				$"You cannot request more days than you have left.\nDays requested: {daysThisYear}.\nDays left for {start.Year}: {remainingThisYear}.");
		}
	}
}

public static class WorkingDayCalculator
{
	public static int Count(DateTime start, DateTime end, int year, int[] workingDays, IReadOnlyList<Leave> blockingLeaves)
	{
		var totalDays = 0;
		for (var date = start; date <= end.AddDays(-1); date = date.AddDays(1))
		{
			var dayOfWeek = date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek;
			if (workingDays.Contains(dayOfWeek)
				&& !blockingLeaves.Any(leave => date >= leave.DateStart && date < leave.DateEnd)
				&& date.Year == year)
			{
				totalDays++;
			}
		}

		return totalDays;
	}

	public static bool Overlaps(DateTime start, DateTime end, Leave other) =>
		(start >= other.DateStart && start < other.DateEnd) ||
		(end > other.DateStart && end <= other.DateEnd) ||
		(start < other.DateStart && end > other.DateEnd);
}

public static class PaidTimeOffCalculator
{
	public static int Remaining(int allowance, int daysTaken) => allowance - daysTaken;
}

public static class TeamConflictDetector
{
	public static List<(int EmployeeId, string? Name, List<Leave> Leaves)> Find(
		Leave request,
		IEnumerable<(int Id, string? Name, List<Leave> ApprovedLeaves)> teammates)
	{
		var conflicts = new List<(int EmployeeId, string? Name, List<Leave> Leaves)>();
		foreach (var teammate in teammates)
		{
			if (teammate.Id == request.Owner)
			{
				continue;
			}

			var overlapping = teammate.ApprovedLeaves
				.Where(leave => WorkingDayCalculator.Overlaps(request.DateStart, request.DateEnd, leave))
				.ToList();
			if (overlapping.Count > 0)
			{
				conflicts.Add((teammate.Id, teammate.Name, overlapping));
			}
		}

		return conflicts;
	}
}

public static class EmployeePolicy
{
	private static readonly Regex EmailRegex = new(
		@"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	public static void AssertCanHire(
		string email,
		string? name,
		string? title,
		string? country,
		int paidTimeOff,
		Employee? existingWithEmail,
		Employee? manager,
		bool countryExists)
	{
		if (existingWithEmail != null && existingWithEmail.Country != null)
		{
			throw new DomainException("There is an existing employee with the same email");
		}

		if (string.IsNullOrWhiteSpace(email))
		{
			throw new DomainException("Email can't be empty");
		}

		if (!EmailRegex.IsMatch(email))
		{
			throw new DomainException("Email must be valid.");
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new DomainException("Name can't be empty");
		}

		if (string.IsNullOrWhiteSpace(title))
		{
			throw new DomainException("Title can't be empty");
		}

		if (manager != null && manager.Email == email)
		{
			throw new DomainException("An employee can't be managed by himself");
		}

		if (string.IsNullOrWhiteSpace(country))
		{
			throw new DomainException("Country can't be empty.");
		}

		if (!countryExists)
		{
			throw new DomainException("Country is not within the list.");
		}

		if (paidTimeOff < 1)
		{
			throw new DomainException("Paid time off needs to be higher than 1.");
		}
	}

	public static void AssertManagerExists(Employee? manager, bool managerWasSpecified)
	{
		if (managerWasSpecified && manager == null)
		{
			throw new DomainException("Manager must exist.");
		}
	}
}
