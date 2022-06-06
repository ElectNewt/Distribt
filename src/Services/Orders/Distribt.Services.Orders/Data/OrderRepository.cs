using Distribt.Services.Orders.Aggregates;
using Distribt.Shared.EventSourcing;
using Distribt.Shared.EventSourcing.EventStores;

namespace Distribt.Services.Orders.Data;

public interface IOrderRepository
{
    Task<OrderDetails> GetById(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    Task<OrderDetails?> GetByIdOrDefault(Guid id, CancellationToken cancellationToken = default(CancellationToken));
    Task Save(OrderDetails orderDetails, CancellationToken cancellationToken = default(CancellationToken));
}

public class OrderRepository : AggregateRepository<OrderDetails>, IOrderRepository
{
    public OrderRepository(IEventStore eventStore) : base(eventStore)
    {
    }

    public async Task<OrderDetails> GetById(Guid id, CancellationToken cancellationToken = default(CancellationToken))
        => await GetByIdAsync(id, cancellationToken);

    public async Task<OrderDetails?> GetByIdOrDefault(Guid id,
        CancellationToken cancellationToken = default(CancellationToken))
        => await GetByIdOrDefaultAsync(id, cancellationToken);

    public async Task Save(OrderDetails orderDetails, CancellationToken cancellationToken = default(CancellationToken))
        => await SaveAsync(orderDetails, cancellationToken);
}