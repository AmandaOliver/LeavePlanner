namespace LeavePlanner.Models;

public class PaginatedLeavesResult
{
	public int TotalCount { get; set; }
	public List<Leave>? Leaves { get; set; }
}

public class PaginatedRequestsResult
{
	public int TotalCount { get; set; }
	public List<LeaveDTO>? Requests { get; set; }
}

public class OrganizationTree
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public int[]? WorkingDays { get; set; }
	public object? Tree { get; set; }
}
