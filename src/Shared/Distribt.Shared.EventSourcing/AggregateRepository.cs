using System.Runtime.Serialization;
using Distribt.Shared.EventSourcing.EventStores;

namespace Distribt.Shared.EventSourcing;

public interface IAggregateRepository<TAggregate>
{
    Task<TAggregate> GetByIdAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    Task<TAggregate?> GetByIdOrDefaultAsync(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken));
}

public class AggregateRepository<TAggregate> : IAggregateRepository<TAggregate>
    where TAggregate : Aggregate
{
    private readonly IEventStore _eventStore;
    private string AggregateName => typeof(TAggregate).Name;

    public AggregateRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<TAggregate> GetByIdAsync(Guid id,
        CancellationToken cancellationToken = default(CancellationToken))
        => await GetByIdOrDefaultAsync(id, cancellationToken) ??
           throw new ApplicationException(
               "seems that the aggregate cannot be found, please use GetByIdOrDefaultAsync");


    public async Task<TAggregate?> GetByIdOrDefaultAsync(Guid id,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var events =
            (await _eventStore.GetEventsForAggregate(AggregateName, id, cancellationToken)).ToList();
        if (!events.Any())
            return null;

#pragma warning disable SYSLIB0050
        //TODO: #39 remove the obsolete class.
        var obj = (TAggregate)FormatterServices.GetUninitializedObject(typeof(TAggregate));
#pragma warning restore SYSLIB0050
        obj.Initialize(id);
        obj.LoadFromHistory(events);
        return obj;
    }

    public async Task SaveAsync(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
    {
        var uncommittedEvents = aggregate.GetUncommittedChanges();
        if (uncommittedEvents.Count == 0) return;

        await _eventStore.SaveEvents(
            AggregateName,
            aggregate.Id,
            uncommittedEvents,
            aggregate.Version, cancellationToken);
        aggregate.MarkChangesAsCommitted();
    }
}