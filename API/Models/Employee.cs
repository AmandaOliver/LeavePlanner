using System;
using System.Collections.Generic;

namespace LeavePlanner.Models;

public partial class Employee
{
    public string? Picture { get; set; }

    public string Email { get; set; } = null!;

    public string? Name { get; set; }

    public int? Organization { get; set; }

    public string? ManagedBy { get; set; }

    public string? Country { get; set; }

    public bool IsOrgOwner { get; set; } = false;

    public int? PaidTimeOff { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Employee> InverseManagedByNavigation { get; set; } = new List<Employee>();

    public virtual ICollection<Leave> LeaveApprovedByNavigations { get; set; } = new List<Leave>();

    public virtual ICollection<Leave> LeaveOwnerNavigations { get; set; } = new List<Leave>();

    public virtual Employee? ManagedByNavigation { get; set; }

    public virtual ICollection<Notification> NotificationCreatorNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationRecipientNavigations { get; set; } = new List<Notification>();

    public virtual Organization? OrganizationNavigation { get; set; }
}
