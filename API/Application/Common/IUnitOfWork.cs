using LeavePlanner.Domain;

namespace LeavePlanner.Application.Common;

public interface IUnitOfWork
{
	IReadOnlyList<IDomainEvent> CollectEvents();
	Task SaveChangesAsync(CancellationToken cancellationToken);
	Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken);
	Task BeginTransactionAsync(CancellationToken cancellationToken);
	Task CommitTransactionAsync(CancellationToken cancellationToken);
	Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
