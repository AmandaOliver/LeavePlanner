using System;
using System.Collections.Generic;

namespace LeavePlanner.Models;

public partial class ConflictDTO
{
	public string? EmployeeName { get; set; }
	public string? EmployeeEmail { get; set; }
	public required List<Leave> ConflictingLeaves { get; set; }
}