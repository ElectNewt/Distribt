using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Events;

namespace Distribt.Services.Orders.Services;

public interface ICancelOrderService
{
    Task<Result<bool>> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class CancelOrderService : ICancelOrderService
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<bool>> Execute(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await GetOrderDetails(orderId, cancellationToken)
            .Bind(EnsureCanBeCancelled)
            .Bind(orderDetails => CancelOrder(orderDetails, cancellationToken));
    }

    private async Task<Result<OrderDetails>> GetOrderDetails(Guid orderId,
        CancellationToken cancellationToken)
    {
        OrderDetails? orderDetails = await _orderRepository.GetByIdOrDefault(orderId, cancellationToken);
        if (orderDetails == null)
            return Result.NotFound<OrderDetails>($"Order {orderId} not found");

        return orderDetails;
    }

    private Result<OrderDetails> EnsureCanBeCancelled(OrderDetails orderDetails)
    {
        if (orderDetails.Status is OrderStatus.Dispatched or OrderStatus.Completed)
        {
            string statusDescription = orderDetails.Status == OrderStatus.Completed
                ? "delivered"
                : "dispatched";
            return Result.Conflict<OrderDetails>(
                $"Order {orderDetails.Id} cannot be cancelled because it has already been {statusDescription}");
        }

        if (orderDetails.Status == OrderStatus.Cancelled)
            return Result.Conflict<OrderDetails>($"Order {orderDetails.Id} is already cancelled");

        return orderDetails;
    }

    private async Task<Result<bool>> CancelOrder(OrderDetails orderDetails, CancellationToken cancellationToken)
    {
        orderDetails.Apply(new OrderCancelled());
        await _orderRepository.Save(orderDetails, cancellationToken);
        return true;
    }
}
