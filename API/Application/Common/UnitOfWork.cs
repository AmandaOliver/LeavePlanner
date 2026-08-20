using LeavePlanner.Data;
using LeavePlanner.Domain;

namespace LeavePlanner.Application.Common;

public interface IUnitOfWork
{
	IReadOnlyList<IDomainEvent> CollectEvents();
	Task SaveChangesAsync(CancellationToken cancellationToken);
	Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
}

public class UnitOfWork : IUnitOfWork
{
	private readonly LeavePlannerContext _context;
	private readonly IDomainEventDispatcher _dispatcher;

	public UnitOfWork(LeavePlannerContext context, IDomainEventDispatcher dispatcher)
	{
		_context = context;
		_dispatcher = dispatcher;
	}

	public IReadOnlyList<IDomainEvent> CollectEvents()
	{
		var aggregates = _context.TrackedAggregates().ToList();
		var events = aggregates.SelectMany(aggregate => aggregate.DomainEvents).ToList();
		foreach (var aggregate in aggregates)
		{
			aggregate.ClearDomainEvents();
		}

		return events;
	}

	public Task SaveChangesAsync(CancellationToken cancellationToken) =>
		_context.SaveChangesAsync(cancellationToken);

	public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken) =>
		_dispatcher.DispatchAsync(events, cancellationToken);
}
