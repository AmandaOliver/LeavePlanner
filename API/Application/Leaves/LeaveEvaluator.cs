using LeavePlanner.Domain;
using LeavePlanner.Models;

namespace LeavePlanner.Application.Leaves;

public class LeaveEvaluator
{
	private readonly ILeaveRepository _leaves;
	private readonly IEmployeeRepository _employees;
	private readonly IOrganizationRepository _organizations;
	private readonly IClock _clock;

	public LeaveEvaluator(
		ILeaveRepository leaves,
		IEmployeeRepository employees,
		IOrganizationRepository organizations,
		IClock clock)
	{
		_leaves = leaves;
		_employees = employees;
		_organizations = organizations;
		_clock = clock;
	}

	public async Task AssertCanRequest(DateTime dateStart, DateTime dateEnd, int ownerId, int? leaveId, string type, CancellationToken cancellationToken)
	{
		Leave? existing = null;
		if (leaveId != null)
		{
			existing = await _leaves.GetByIdAsync(leaveId.Value, cancellationToken);
			if (existing == null)
			{
				throw new DomainException("Leave not found.");
			}
		}

		var employee = await _employees.GetByIdAsync(ownerId, cancellationToken);
		if (employee == null)
		{
			throw new DomainException("Employee not found.");
		}

		var daysThisYear = 0;
		var remainingThisYear = 0;
		var daysNextYear = 0;
		var remainingNextYear = 0;

		if (type == LeaveTypes.PaidTimeOff)
		{
			if (dateStart.Year != dateEnd.Year)
			{
				var endOfYear = new DateTime(dateStart.Year + 1, 1, 1);
				var startOfNextYear = new DateTime(dateEnd.Year, 1, 1);
				daysThisYear = await CountRequestedDays(dateStart, endOfYear, ownerId, dateStart.Year, leaveId, cancellationToken);
				daysNextYear = await CountRequestedDays(startOfNextYear, dateEnd, ownerId, dateEnd.Year, leaveId, cancellationToken);
				remainingThisYear = await RemainingPaidTimeOff(employee.Id, dateStart.Year, leaveId, cancellationToken);
				remainingNextYear = await RemainingPaidTimeOff(employee.Id, dateEnd.Year, leaveId, cancellationToken);
			}
			else
			{
				daysThisYear = await CountRequestedDays(dateStart, dateEnd, ownerId, dateStart.Year, leaveId, cancellationToken);
				remainingThisYear = await RemainingPaidTimeOff(employee.Id, dateStart.Year, leaveId, cancellationToken);
			}
		}

		LeavePolicy.AssertCanRequest(
			type,
			dateStart,
			dateEnd,
			_clock.UtcNow.Date,
			existing,
			daysThisYear,
			remainingThisYear,
			daysNextYear,
			remainingNextYear);
	}

	public async Task<LeaveDTO> ComposeDto(Leave leave, bool withConflicts, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(leave.Owner, cancellationToken);
		if (employee == null)
		{
			throw new DomainException("employee not found");
		}

		var excludeId = leave.Id != 0 ? leave.Id : (int?)null;
		var requestedDaysThisYear = await CountRequestedDays(leave.DateStart, leave.DateEnd, leave.Owner, _clock.UtcNow.Year, excludeId, cancellationToken);
		var requestedDaysNextYear = await CountRequestedDays(leave.DateStart, leave.DateEnd, leave.Owner, _clock.UtcNow.Year + 1, excludeId, cancellationToken);
		var leftDaysThisYear = await RemainingPaidTimeOff(leave.Owner, leave.DateStart.Year, excludeId, cancellationToken);
		var leftDaysNextYear = await RemainingPaidTimeOff(leave.Owner, leave.DateStart.Year + 1, excludeId, cancellationToken);

		var dto = leave.ToLeaveDto(employee.Name);
		dto.DaysRequested = requestedDaysThisYear + requestedDaysNextYear;
		dto.DaysLeftThisYear = leftDaysThisYear - requestedDaysThisYear;
		dto.DaysLeftNextYear = leftDaysNextYear - requestedDaysNextYear;

		if (withConflicts)
		{
			dto.Conflicts = await GetConflicts(leave, cancellationToken);
		}

		return dto;
	}

	public async Task<List<LeaveDTO>> ComposeDtos(IEnumerable<Leave> leaves, bool withConflicts, CancellationToken cancellationToken)
	{
		var dtos = new List<LeaveDTO>();
		foreach (var leave in leaves)
		{
			dtos.Add(await ComposeDto(leave, withConflicts, cancellationToken));
		}

		return dtos;
	}

	public async Task<List<LeaveDTO>> GetPendingRequests(int employeeId, CancellationToken cancellationToken)
	{
		var leaves = await _leaves.GetPendingByOwnerAsync(employeeId, cancellationToken);
		return await ComposeDtos(leaves, false, cancellationToken);
	}

	public async Task<List<LeaveDTO>> GetReviewedRequests(int employeeId, CancellationToken cancellationToken)
	{
		var system = await _employees.GetSystemAsync(cancellationToken);
		var leaves = await _leaves.GetReviewedByOwnerAsync(employeeId, system.Id, cancellationToken);
		return await ComposeDtos(leaves, false, cancellationToken);
	}

	public async Task<List<ConflictDTO>> GetConflicts(Leave leaveRequest, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(leaveRequest.Owner, cancellationToken);
		if (employee == null)
		{
			throw new DomainException("Employee not found");
		}

		if (employee.ManagedBy == null)
		{
			return [];
		}

		var manager = await _employees.GetByIdAsync(employee.ManagedBy.Value, cancellationToken);
		if (manager == null)
		{
			throw new DomainException("Manager not found");
		}

		var teammates = await _employees.GetDirectReportsAsync(manager.Id, cancellationToken);
		var teammateLeaves = new List<(int Id, string? Name, List<Leave> ApprovedLeaves)>();
		foreach (var teammate in teammates)
		{
			var approved = await _leaves.GetApprovedByOwnerAsync(teammate.Id, cancellationToken);
			teammateLeaves.Add((teammate.Id, teammate.Name, approved));
		}

		return TeamConflictDetector.Find(leaveRequest, teammateLeaves)
			.Select(conflict => new ConflictDTO
			{
				EmployeeId = conflict.EmployeeId,
				EmployeeName = conflict.Name,
				ConflictingLeaves = conflict.Leaves.Select(leave => leave.ToLeaveDto(conflict.Name)).ToList()
			})
			.ToList();
	}

	public static string DescribeConflicts(LeaveDTO leave)
	{
		if (leave.Conflicts == null || leave.Conflicts.Count == 0)
		{
			return "There are no other team members on leave during this time.";
		}

		var description = "This leave conflicts with other team members: \n";
		foreach (var conflict in leave.Conflicts)
		{
			description += $"\n\t- {conflict.EmployeeName}:\n";
			foreach (var conflictingLeave in conflict.ConflictingLeaves)
			{
				description += $"\t\tStart Date: {conflictingLeave.DateStart.ToShortDateString()}, End Date: {conflictingLeave.DateEnd.ToShortDateString()}\n";
				description += $"\t\tDescription: {conflictingLeave.Description}\n";
			}
		}

		return description;
	}

	public async Task<int> RemainingPaidTimeOff(int employeeId, int year, int? leaveId, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
		if (employee == null)
		{
			throw new DomainException("Employee not found.");
		}

		var leavesThisYear = await _leaves.GetApprovedPaidTimeOffInYearAsync(employeeId, year, leaveId, cancellationToken);
		var totalDaysTaken = 0;
		foreach (var leave in leavesThisYear)
		{
			totalDaysTaken += await CountRequestedDays(leave.DateStart, leave.DateEnd, employeeId, year, leave.Id, cancellationToken);
		}

		return PaidTimeOffCalculator.Remaining(employee.PaidTimeOff, totalDaysTaken);
	}

	public async Task<int> CountRequestedDays(DateTime start, DateTime end, int owner, int year, int? leaveId, CancellationToken cancellationToken)
	{
		var asOf = _clock.UtcNow;
		if (leaveId != null)
		{
			var currentLeave = await _leaves.GetByIdAsync(leaveId.Value, cancellationToken);
			if (currentLeave == null)
			{
				throw new DomainException("leave not found");
			}

			asOf = currentLeave.CreatedAt;
		}

		var blockingLeaves = await _leaves.GetBlockingLeavesAsync(owner, start, end, asOf, leaveId, cancellationToken);
		var employee = await _employees.GetByIdAsync(owner, cancellationToken);
		if (employee == null)
		{
			throw new DomainException("Owner not found");
		}

		if (employee.Organization == null)
		{
			throw new DomainException("Organization not found");
		}

		var organization = await _organizations.GetByIdAsync(employee.Organization.Value, cancellationToken);
		if (organization == null)
		{
			throw new DomainException("Organization not found");
		}

		return WorkingDayCalculator.Count(start, end, year, organization.WorkingDays, blockingLeaves);
	}
}
