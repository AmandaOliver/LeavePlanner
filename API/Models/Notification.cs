using System;
using System.Collections.Generic;

namespace LeavePlanner.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int Creator { get; set; }

    public int? LeaveId { get; set; }

    public int Recipient { get; set; }

    public virtual Employee CreatorNavigation { get; set; } = null!;

    public virtual Leave? Leave { get; set; }

    public virtual Employee RecipientNavigation { get; set; } = null!;
}
