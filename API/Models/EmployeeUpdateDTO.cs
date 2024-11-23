namespace LeavePlanner.Models
{
	public class EmployeeUpdateDTO
	{
		public string? Country { get; set; }
		public int PaidTimeOff { get; set; }
		public string? Title { get; set; }
		public string? Name { get; set; }
		public required bool IsOrgOwner { get; set; }

	}
}