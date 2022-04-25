using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Events;

namespace Distribt.Services.Orders.Services;

public interface IOrderDeliveredService
{
    Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class OrderDeliveredService : IOrderDeliveredService
{
    private readonly IOrderRepository _orderRepository;

    public OrderDeliveredService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }


    public async Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
    { 
        
        OrderDetails orderDetails =  await _orderRepository.GetById(orderId, cancellationToken);
        orderDetails.Apply(new OrderCompleted());
        await _orderRepository.Save(orderDetails, cancellationToken);
        return true;
    }
}