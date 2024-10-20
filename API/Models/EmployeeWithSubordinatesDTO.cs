namespace LeavePlanner.Models
{
	public class EmployeeWithSubordinatesDTO
	{
		public string Email { get; set; } = null!;
		public string? Name { get; set; }
		public string? Country { get; set; }
		public int? Organization { get; set; }
		public string? ManagedBy { get; set; }
		public bool IsOrgOwner { get; set; } = false;
		public int PaidTimeOff { get; set; }
		public int PaidTimeOffLeft { get; set; }
		public int PaidTimeOffLeftNextYear { get; set; }
		public string? Title { get; set; }
		public int? PendingRequests { get; set; }
		public List<EmployeeWithSubordinatesDTO> Subordinates { get; set; } = new List<EmployeeWithSubordinatesDTO>();
	}
}