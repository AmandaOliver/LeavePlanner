namespace LeavePlanner.Models
{
	public class LeaveDTO
	{
		public int Id { get; set; }

		public string? Type { get; set; }

		public DateTime DateStart { get; set; }

		public DateTime DateEnd { get; set; }
		public int? Owner { get; set; }
		public string? OwnerName { get; set; }
		public string? Description { get; set; }

		public int? ApprovedBy { get; set; }
		public int? RejectedBy { get; set; }

		public int DaysRequested { get; set; }
		public int DaysLeftThisYear { get; set; }
		public int DaysLeftNextYear { get; set; }
		public List<ConflictDTO>? Conflicts { get; set; }


	}
}