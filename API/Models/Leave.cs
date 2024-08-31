using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeavePlanner.Models;

public partial class Leave
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }
    public string? Owner { get; set; }
    public string? Description { get; set; }

    public string? ApprovedBy { get; set; }


    [JsonIgnore]
    public virtual Employee? ApprovedByNavigation { get; set; }

    [JsonIgnore]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [JsonIgnore]
    public virtual Employee? OwnerNavigation { get; set; }
}
