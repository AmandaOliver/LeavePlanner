namespace LeavePlanner.Models;

public class OrganizationDTO
{
	public int Id { get; set; }
	public string Name { get; set; } = null!;
	public int[] WorkingDays { get; set; } = [];
}
