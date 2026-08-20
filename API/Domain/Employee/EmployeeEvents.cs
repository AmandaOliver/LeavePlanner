namespace LeavePlanner.Domain;

public sealed class EmployeeJoined : IDomainEvent
{
	public EmployeeJoined(Employee employee) => Employee = employee;

	public Employee Employee { get; }
}
