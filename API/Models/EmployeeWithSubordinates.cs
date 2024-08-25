namespace LeavePlanner.Models
{
	public class EmployeeWithSubordinates
	{
		public string Email { get; set; } = null!;
		public string? Name { get; set; }
		public string? Country { get; set; }
		public int? Organization { get; set; }
		public string? ManagedBy { get; set; }
		public bool IsOrgOwner { get; set; } = false;
		public int? PaidTimeOff { get; set; }
		public string? Picture { get; set; }
		public DateTime? CreatedAt { get; set; }
		public List<EmployeeWithSubordinates> Subordinates { get; set; } = new List<EmployeeWithSubordinates>();
	}
}