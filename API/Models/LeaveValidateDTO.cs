
namespace LeavePlanner.Models
{
	public class LeaveValidateDTO
	{
		public DateTime DateStart { get; set; }
		public DateTime DateEnd { get; set; }
		public required string Owner { get; set; }
		public int? Id { get; set; }

	}
}