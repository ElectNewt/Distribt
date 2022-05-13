using System.Net;
using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.BusinessLogic.Services.External;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Dto;
using Distribt.Services.Orders.Events;
using Distribt.Shared.Setup.Extensions;

namespace Distribt.Services.Orders.Services;

public interface ICreateOrderService
{
    Task<Result<CreateOrderResponse>> Execute(CreateOrderRequest createOrder,
        CancellationToken cancellationToken = default(CancellationToken));
}

public class CreateOrderService : ICreateOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly IProductNameService _productNameService;

    public CreateOrderService(IOrderRepository orderRepository, IDomainMessagePublisher domainMessagePublisher,
        IProductNameService productNameService)
    {
        _orderRepository = orderRepository;
        _domainMessagePublisher = domainMessagePublisher;
        _productNameService = productNameService;
    }


    public async Task<Result<CreateOrderResponse>> Execute(CreateOrderRequest createOrder,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        return await CreateOrder(createOrder)
            .Async()
            //On a real scenario:
            //validate orders
            //validate fraud check
            .Bind(x => SaveOrder(x, cancellationToken))
            .Then(x => MapToOrderResponse(x, cancellationToken)
                .Bind(or => PublishDomainEvent(or, cancellationToken)))
            .Map(x => new CreateOrderResponse(x.Id, $"order/getorderstatus/{x.Id}"));
    }

    private Result<OrderDetails> CreateOrder(CreateOrderRequest createOrder)
    {
        Guid createdOrderId = Guid.NewGuid();

        OrderDetails orderDetails = new OrderDetails(createdOrderId);
        orderDetails.Apply(new OrderCreated(createOrder.DeliveryDetails, createOrder.PaymentInformation,
            createOrder.Products));

        return orderDetails;
    }

    private async Task<Result<OrderDetails>> SaveOrder(OrderDetails orderDetails, CancellationToken cancellationToken)
    {
        await _orderRepository.Save(orderDetails, cancellationToken);
        return orderDetails;
    }

    private async Task<Result<OrderResponse>> MapToOrderResponse(OrderDetails orderDetails,
        CancellationToken cancellationToken)
    {
        var products = await orderDetails.Products
            .SelectAsync(async p => new ProductQuantityName(p.ProductId, p.Quantity,
                await _productNameService.GetProductName(p.ProductId, cancellationToken)));


        OrderResponse orderResponse = new OrderResponse(orderDetails.Id, orderDetails.Status.ToString(),
            orderDetails.Delivery, orderDetails.PaymentInformation, products.ToList());
        return orderResponse;
    }

    private async Task<Result<Guid>> PublishDomainEvent(OrderResponse orderResponse,
        CancellationToken cancellationToken)
    {
        await _domainMessagePublisher.Publish(orderResponse, routingKey: "order", cancellationToken: cancellationToken);
        return orderResponse.OrderId;
    }
}