using System;
using System.Collections.Generic;

namespace LeavePlanner.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string? Picture { get; set; }

    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int? Organization { get; set; }

    public int? ManagedBy { get; set; }

    public string? Country { get; set; }

    public bool? IsManager { get; set; }

    public bool? IsOrgOwner { get; set; }

    public int? PaidTimeOff { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Employee> InverseManagedByNavigation { get; set; } = new List<Employee>();

    public virtual ICollection<Leave> LeafApprovedByNavigations { get; set; } = new List<Leave>();

    public virtual ICollection<Leave> LeafOwnerNavigations { get; set; } = new List<Leave>();

    public virtual Employee? ManagedByNavigation { get; set; }

    public virtual ICollection<Notification> NotificationCreatorNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationRecipientNavigations { get; set; } = new List<Notification>();

    public virtual Organization? OrganizationNavigation { get; set; }

    public virtual ICollection<Organization> Organizations { get; set; } = new List<Organization>();
}
