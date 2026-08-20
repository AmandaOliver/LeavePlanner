namespace LeavePlanner.Domain;

public interface IClock
{
	DateTime UtcNow { get; }
}
