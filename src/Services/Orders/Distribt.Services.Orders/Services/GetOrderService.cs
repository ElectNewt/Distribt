using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Dto;
using Distribt.Shared.Setup.Extensions;

namespace Distribt.Services.Orders.Services;

public interface IGetOrderService
{
    Task<Result<OrderResponse>> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
}

public class GetOrderService : IGetOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductNameService _productNameService;

    public GetOrderService(IOrderRepository orderRepository, IProductNameService productNameService)
    {
        _orderRepository = orderRepository;
        _productNameService = productNameService;
    }


    public async Task<Result<OrderResponse>> Execute(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await GetOrderDetails(orderId, cancellationToken)
            .Bind(x => MapToOrderResponse(x, cancellationToken));
    }

    private async Task<Result<OrderDetails>> GetOrderDetails(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        OrderDetails? orderDetails = await _orderRepository.GetByIdOrDefault(orderId, cancellationToken);
        if (orderDetails == null)
            return Result.NotFound<OrderDetails>($"Order {orderId} not found");

        return orderDetails;
    }

    private async Task<Result<OrderResponse>> MapToOrderResponse(OrderDetails orderDetails,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        //SelectAsync is a custom method, go to the source code to check it out 
        var products = await orderDetails.Products
            .SelectAsync(async p => new ProductQuantityName(p.ProductId, p.Quantity,
                await _productNameService.GetProductName(p.ProductId, cancellationToken)));

        return new OrderResponse(orderDetails.Id, orderDetails.Status.ToString(), orderDetails.Delivery,
            orderDetails.PaymentInformation, products.ToList());
    }
}