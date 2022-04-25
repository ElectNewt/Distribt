using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Events;

namespace Distribt.Services.Orders.Services;

public interface IOrderPaidService
{
    Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class OrderPaidService : IOrderPaidService
{
    private readonly IOrderRepository _orderRepository;

    public OrderPaidService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
    {
        OrderDetails orderDetails = await _orderRepository.GetById(orderId, cancellationToken);
        orderDetails.Apply(new OrderPaid());
        await _orderRepository.Save(orderDetails, cancellationToken);
        return true;
    }
}