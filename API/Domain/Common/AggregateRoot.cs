namespace LeavePlanner.Domain;

public abstract class AggregateRoot
{
	private readonly List<IDomainEvent> _domainEvents = [];

	public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

	protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

	public void ClearDomainEvents() => _domainEvents.Clear();
}
