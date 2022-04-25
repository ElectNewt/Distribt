using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Events;

namespace Distribt.Services.Orders.Services;

public interface IOrderDispatchedService
{
    Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class OrderDispatchedService : IOrderDispatchedService
{
    private readonly IOrderRepository _orderRepository;

    public OrderDispatchedService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }


    public async Task<bool> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken))
    { 
        
        OrderDetails orderDetails =  await _orderRepository.GetById(orderId, cancellationToken);
        orderDetails.Apply(new OrderDispatched());
        await _orderRepository.Save(orderDetails, cancellationToken);
        return true;
    }
}