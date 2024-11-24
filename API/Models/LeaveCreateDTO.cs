
namespace LeavePlanner.Models
{
	public class LeaveCreateDTO
	{
		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public string? Description { get; set; }
		public required string Type { get; set; }
		public required int Owner { get; set; }

	}
}