namespace LeavePlanner.Models
{
	public class EmployeeCreateDTO
	{
		public required string Email { get; set; }
		public int Organization { get; set; }
		public string? ManagedBy { get; set; }
		public required string Country { get; set; }
		public int PaidTimeOff { get; set; }
		public required string Title { get; set; }
		public required string Name { get; set; }
		public required bool IsOrgOwner { get; set; }
	}
}