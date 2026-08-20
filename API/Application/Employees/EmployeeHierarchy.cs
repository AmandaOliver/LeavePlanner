using LeavePlanner.Domain;
using LeavePlanner.Models;

namespace LeavePlanner.Application.Employees;

public class EmployeeHierarchy
{
	private readonly IEmployeeRepository _employees;
	private readonly ILeaveRepository _leaves;
	private readonly LeavePlanner.Application.Leaves.LeaveEvaluator _leaveEvaluator;
	private readonly IClock _clock;

	public EmployeeHierarchy(
		IEmployeeRepository employees,
		ILeaveRepository leaves,
		LeavePlanner.Application.Leaves.LeaveEvaluator leaveEvaluator,
		IClock clock)
	{
		_employees = employees;
		_leaves = leaves;
		_leaveEvaluator = leaveEvaluator;
		_clock = clock;
	}

	public async Task<EmployeeWithSubordinatesDTO> GetWithSubordinates(Employee employee, CancellationToken cancellationToken)
	{
		var paidTimeOffLeft = await _leaveEvaluator.RemainingPaidTimeOff(employee.Id, _clock.UtcNow.Year, null, cancellationToken);
		var paidTimeOffLeftNextYear = await _leaveEvaluator.RemainingPaidTimeOff(employee.Id, _clock.UtcNow.Year + 1, null, cancellationToken);
		var manager = employee.ManagedBy == null
			? null
			: await _employees.GetByIdAsync(employee.ManagedBy.Value, cancellationToken);

		var dto = new EmployeeWithSubordinatesDTO
		{
			Id = employee.Id,
			Email = employee.Email,
			Name = employee.Name,
			Country = employee.Country,
			Organization = employee.Organization,
			ManagerName = manager?.Name,
			ManagedBy = employee.ManagedBy,
			IsOrgOwner = employee.IsOrgOwner,
			PaidTimeOff = employee.PaidTimeOff,
			Title = employee.Title,
			PaidTimeOffLeft = paidTimeOffLeft,
			PaidTimeOffLeftNextYear = paidTimeOffLeftNextYear,
			Subordinates = []
		};

		var subordinates = await _employees.GetDirectReportsAsync(employee.Id, cancellationToken);
		var pendingRequests = 0;
		foreach (var subordinate in subordinates)
		{
			var pending = await _leaveEvaluator.GetPendingRequests(subordinate.Id, cancellationToken);
			pendingRequests += pending.Count;
			dto.Subordinates.Add(await GetWithSubordinates(subordinate, cancellationToken));
		}

		dto.PendingRequests = pendingRequests;
		return dto;
	}

	public async Task DeleteWithSubordinates(int id, CancellationToken cancellationToken)
	{
		var employee = await _employees.GetByIdAsync(id, cancellationToken);
		if (employee == null)
		{
			return;
		}

		var subordinates = await _employees.GetDirectReportsAsync(id, cancellationToken);
		foreach (var subordinate in subordinates)
		{
			await DeleteWithSubordinates(subordinate.Id, cancellationToken);
		}

		_leaves.RemoveRange(await _leaves.GetOwnedByAsync(employee.Id, cancellationToken));
		_employees.Remove(employee);
	}
}
