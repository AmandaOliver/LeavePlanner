using LeavePlanner.Domain;

namespace LeavePlanner.Infrastructure.Time;

public class SystemClock : IClock
{
	public DateTime UtcNow => DateTime.UtcNow;
}
