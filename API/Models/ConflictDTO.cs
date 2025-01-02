namespace LeavePlanner.Models;

public class ConflictDTO
{
	public string? EmployeeName { get; set; }
	public int? EmployeeId { get; set; }
	public required List<Leave> ConflictingLeaves { get; set; }
}