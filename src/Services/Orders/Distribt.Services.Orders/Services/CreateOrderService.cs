using Distribt.Services.Orders.Aggregates;
using Distribt.Services.Orders.Data;
using Distribt.Services.Orders.Dto;
using Distribt.Services.Orders.Events;

namespace Distribt.Services.Orders.Services;

public interface ICreateOrderService
{
    Task<Guid> Execute(CreateOrderRequest createOrder, CancellationToken cancellationToken = default(CancellationToken));
}

public class CreateOrderService : ICreateOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IDomainMessagePublisher _domainMessagePublisher;

    public CreateOrderService(IOrderRepository orderRepository, IDomainMessagePublisher domainMessagePublisher)
    {
        _orderRepository = orderRepository;
        _domainMessagePublisher = domainMessagePublisher;
    }


    public async Task<Guid> Execute(CreateOrderRequest createOrder, CancellationToken cancellationToken = default(CancellationToken))
    {
        Guid createdOrderId = Guid.NewGuid();

        //On a real scenario:
        //validate orders
        //validate fraud check

        OrderDetails orderDetails = new OrderDetails(createdOrderId);
        orderDetails.Apply(new OrderCreated(createOrder.DeliveryDetails, createOrder.PaymentInformation,
            createOrder.Products));

        await _orderRepository.Save(orderDetails, cancellationToken);

        OrderResponse orderResponse = new OrderResponse(orderDetails.Id, orderDetails.Status.ToString(), orderDetails.Delivery, orderDetails.PaymentInformation,
            orderDetails.Products
                .Select(p => new ProductQuantityName(p.ProductId, p.Quantity, "fakename"))
                .ToList());//the name is temporal, it will be completed in the next episodes.
        
        await _domainMessagePublisher.Publish(orderResponse, routingKey: "order", cancellationToken: cancellationToken);
        return createdOrderId;
    }
}