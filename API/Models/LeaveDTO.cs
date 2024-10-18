namespace LeavePlanner.Models
{
	public class LeaveDTO
	{
		public int Id { get; set; }

		public string? Type { get; set; }

		public DateTime DateStart { get; set; }

		public DateTime DateEnd { get; set; }
		public string? Owner { get; set; }
		public string? OwnerName { get; set; }
		public string? Description { get; set; }

		public string? ApprovedBy { get; set; }
		public string? RejectedBy { get; set; }

		public int DaysRequested { get; set; }
		public List<ConflictDTO>? Conflicts { get; set; }


	}
}