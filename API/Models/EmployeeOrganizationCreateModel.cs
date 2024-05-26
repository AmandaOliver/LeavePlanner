namespace LeavePlanner.Models
{
    public class EmployeeOrganizationCreateModel
    {
        public string EmployeeId { get; set; }
        public string? Picture { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Country { get; set; }
        public string OrganizationName { get; set; } = null!;
    }
}