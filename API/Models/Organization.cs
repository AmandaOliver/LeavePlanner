using System;
using System.Collections.Generic;

namespace LeavePlanner.Models;

public partial class Organization
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public int[] WorkingDays { get; set; } = [1, 2, 3, 4, 5];

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
