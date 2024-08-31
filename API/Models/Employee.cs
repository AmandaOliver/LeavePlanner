using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeavePlanner.Models;

public partial class Employee
{
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public int? Organization { get; set; }
    public string? ManagedBy { get; set; }
    public string? Country { get; set; }
    public bool IsOrgOwner { get; set; } = false;
    public int? PaidTimeOff { get; set; }
    public string? Title { get; set; }
    [JsonIgnore]
    public virtual ICollection<Employee> InverseManagedByNavigation { get; set; } = new List<Employee>();
    [JsonIgnore]
    public virtual ICollection<Leave> LeaveApprovedByNavigations { get; set; } = new List<Leave>();
    [JsonIgnore]
    public virtual ICollection<Leave> LeaveOwnerNavigations { get; set; } = new List<Leave>();
    [JsonIgnore]
    public virtual Employee? ManagedByNavigation { get; set; }
    [JsonIgnore]
    public virtual ICollection<Notification> NotificationCreatorNavigations { get; set; } = new List<Notification>();
    [JsonIgnore]
    public virtual ICollection<Notification> NotificationRecipientNavigations { get; set; } = new List<Notification>();
    [JsonIgnore]
    public virtual Organization? OrganizationNavigation { get; set; }
}
