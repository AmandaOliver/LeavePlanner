namespace LeavePlanner.Models;

public class EmployeeDTO
{
	public int Id { get; set; }
	public string Email { get; set; } = null!;
	public string? Name { get; set; }
	public int? Organization { get; set; }
	public int? ManagedBy { get; set; }
	public string? Country { get; set; }
	public bool IsOrgOwner { get; set; }
	public int PaidTimeOff { get; set; }
	public string? Title { get; set; }
}
