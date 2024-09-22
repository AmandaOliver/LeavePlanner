namespace LeavePlanner.Models;

public partial class RequestDTO
{
	public int Id { get; set; }
	public string? Type { get; set; }
	public DateTime DateStart { get; set; }
	public DateTime DateEnd { get; set; }
	public string? Description { get; set; }
	public string? OwnerName { get; set; }
}
