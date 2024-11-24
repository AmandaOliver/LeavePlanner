using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeavePlanner.Models;

public partial class Employee
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public int? Organization { get; set; }
    public int? ManagedBy { get; set; }
    public string? Country { get; set; }
    public bool IsOrgOwner { get; set; }
    public int PaidTimeOff { get; set; }
    public string? Title { get; set; }
    [JsonIgnore]
    public virtual ICollection<Employee> InverseManagedByNavigation { get; set; } = new List<Employee>();
    [JsonIgnore]
    public virtual ICollection<Leave> LeaveApprovedByNavigations { get; set; } = new List<Leave>();
    [JsonIgnore]
    public virtual ICollection<Leave> LeaveRejectedByNavigations { get; set; } = new List<Leave>();
    [JsonIgnore]
    public virtual ICollection<Leave> LeaveOwnerNavigations { get; set; } = new List<Leave>();
    [JsonIgnore]
    public virtual Employee? ManagedByNavigation { get; set; }
    [JsonIgnore]
    public virtual Organization? OrganizationNavigation { get; set; }
}
