namespace Distribt.Shared.EventSourcing.EventStores;

public interface IEventStoreManager
{
    Task SaveEvents(Guid id, string aggregateType, IEnumerable<AggregateChange> events, int expectedVersion, CancellationToken cancellationToken = default(CancellationToken));
    Task<IEnumerable<AggregateChange>> GetEventsForAggregate(string aggregateType, Guid id, CancellationToken cancellationToken = default(CancellationToken));
}