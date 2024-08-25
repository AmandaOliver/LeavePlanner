namespace LeavePlanner.Models
{
	public class EmployeeCreateModel
	{

		public string Email { get; set; } = null!;

		public int Organization { get; set; } = 0;

		public string? ManagedBy { get; set; }

		public string Country { get; set; } = null!;

		public int PaidTimeOff { get; set; } = 0;
	}
}