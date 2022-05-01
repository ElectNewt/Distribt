using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Dto;
using Distribt.Shared.Setup.Extensions;

namespace Distribt.Services.Orders.Services;

public interface IGetOrderService
{
    Task<OrderResponse> Execute(Guid orderId, CancellationToken cancellationToken = default(CancellationToken));
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


    public async Task<OrderResponse> Execute(Guid orderId,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        OrderDetails orderDetails = await _orderRepository.GetById(orderId, cancellationToken);
        //on a real scenario this implementation will be much bigger.
        
        //SelectAsync is a custom method, go to the source code to check it out 
        var products = await orderDetails.Products
            .SelectAsync(async p => new ProductQuantityName(p.ProductId, p.Quantity,
                await _productNameService.GetProductName(p.ProductId, cancellationToken)));
        
        return new OrderResponse(orderDetails.Id, orderDetails.Status.ToString(), orderDetails.Delivery,
            orderDetails.PaymentInformation, products.ToList());
    }
}