using LeavePlanner.Application.Common;
using LeavePlanner.Data;
using LeavePlanner.Domain;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeavePlanner.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
	private readonly LeavePlannerContext _context;
	private readonly IDomainEventDispatcher _dispatcher;
	private IDbContextTransaction? _transaction;

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

	public async Task BeginTransactionAsync(CancellationToken cancellationToken) =>
		_transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

	public async Task CommitTransactionAsync(CancellationToken cancellationToken)
	{
		if (_transaction == null)
		{
			return;
		}

		await _transaction.CommitAsync(cancellationToken);
		await _transaction.DisposeAsync();
		_transaction = null;
	}

	public async Task RollbackTransactionAsync(CancellationToken _)
	{
		if (_transaction == null)
		{
			return;
		}

		await _transaction.RollbackAsync(CancellationToken.None);
		await _transaction.DisposeAsync();
		_transaction = null;
	}

	public async ValueTask DisposeAsync()
	{
		if (_transaction != null)
		{
			await _transaction.DisposeAsync();
			_transaction = null;
		}
	}
}
