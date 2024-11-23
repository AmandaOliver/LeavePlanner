public class EmployeeCsvModel
{
	public required string Email { get; set; }
	public required string Name { get; set; }
	public required string Title { get; set; }
	public string? ManagerEmail { get; set; }
	public required string Country { get; set; }
	public required int PaidTimeOff { get; set; }
	public required bool IsAdmin { get; set; }
}
