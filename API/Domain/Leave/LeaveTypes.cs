namespace LeavePlanner.Domain;

public static class LeaveTypes
{
	public const string PaidTimeOff = "paidTimeOff";

	/// <summary>
	/// Public holiday. The wire value (JSON and the MySQL ENUM) is <c>bankHoliday</c>.
	/// </summary>
	public const string BankHoliday = "bankHoliday";

	public const string StatutoryLeave = "statutoryLeave";
}
