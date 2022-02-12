using Distribt.Services.Orders.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Orders.Controllers;
[ApiController]
[Route("[controller]")]
public class OrderController
{
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    
    public OrderController(IDomainMessagePublisher domainMessagePublisher)
    {
        _domainMessagePublisher = domainMessagePublisher;
    }

    [HttpGet("{orderId}")]
    public Task<Order> GetOrder(Guid orderId)
    {
        throw new NotImplementedException();
    }

    [HttpGet("getorderstatus/{orderId}")]
    public Task<Order> GetOrderStatus(Guid orderId)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost(Name = "createorder")]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderDto createOrder)
    {
        Order order = new Order(Guid.NewGuid(), createOrder.orderAddress, createOrder.PersonalDetails,
            createOrder.Products);
        await _domainMessagePublisher.Publish(order, routingKey: "order");
        return new AcceptedResult($"getorderstatus/{order.orderId}", order.orderId);
    }
}

