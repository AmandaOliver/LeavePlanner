
namespace LeavePlanner.Models
{
	public class LeaveUpdateDTO
	{
		public int Id { get; set; }
		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public required int Owner { get; set; }
		public string? Description { get; set; }
		public required string Type { get; set; }

	}
}