using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Dto;

namespace Distribt.Services.Orders.Services;

public interface IGetOrderService
{
    Task<OrderResponse> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class GetOrderService : IGetOrderService
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }


    public async Task<OrderResponse> Execute(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        OrderDetails orderDetails = await _orderRepository.GetById(orderId, cancellationToken);
        //on a real scenario this implementation will be much bigger.
        return new OrderResponse(orderDetails.Id, orderDetails.Status.ToString(), orderDetails.Delivery, orderDetails.PaymentInformation,
            orderDetails.Products
                .Select(p => new ProductQuantityName(p.ProductId, p.Quantity, "fakeNAme"))
                .ToList());
    }
}