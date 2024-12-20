﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeavePlanner.Models;

public partial class Leave
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public DateTime DateStart { get; set; }

    public DateTime DateEnd { get; set; }
    public required int Owner { get; set; }
    public string? Description { get; set; }

    public int? ApprovedBy { get; set; }
    public int? RejectedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    [JsonIgnore]
    public virtual Employee? ApprovedByNavigation { get; set; }
    [JsonIgnore]
    public virtual Employee? RejectedByNavigation { get; set; }
    public required virtual Employee OwnerNavigation { get; set; }
}
