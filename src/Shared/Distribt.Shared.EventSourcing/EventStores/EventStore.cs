namespace Distribt.Shared.EventSourcing.EventStores;

public interface IEventStore
{
    Task SaveEvents(string aggregateType, Guid aggregateId, IList<AggregateChange> events, int expectedVersion,
        CancellationToken cancellationToken = default(CancellationToken));

    Task<IEnumerable<AggregateChange>> GetEventsForAggregate(string aggregateType, Guid aggregateId,
        CancellationToken cancellationToken = default(CancellationToken));
}

public class EventStore : IEventStore
{
    private readonly IEventStoreManager _eventStoreManager;

    public EventStore(IEventStoreManager eventStoreManager)
    {
        _eventStoreManager = eventStoreManager;
    }

    public async Task SaveEvents(string aggregateType, Guid aggregateId, IList<AggregateChange> events,
        int expectedVersion, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _eventStoreManager.SaveEvents(aggregateId, aggregateType, events, expectedVersion, cancellationToken);
    }

    public Task<IEnumerable<AggregateChange>> GetEventsForAggregate(string aggregateType, Guid aggregateId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return _eventStoreManager.GetEventsForAggregate(aggregateType, aggregateId, cancellationToken);
    }
}