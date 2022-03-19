using Distribt.Services.Orders.Dto;
using Distribt.Services.Products.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Orders.Controllers;
[ApiController]
[Route("[controller]")]
public class OrderController
{
    private readonly IDomainMessagePublisher _domainMessagePublisher;
    private readonly ILogger<OrderController>  _logger;
    
    public OrderController(ILogger<OrderController> logger,  IDomainMessagePublisher domainMessagePublisher)
    {
        _domainMessagePublisher = domainMessagePublisher;
        _logger = logger;
    }

    [HttpGet("{orderId}")]
    public Task<OrderDto> GetOrder(Guid orderId)
    {
        _logger.LogError($"esto es un mensaje de ejemplo con el order {orderId}");

        //Todo, change for a real one as this one is only to test the logger.
        return Task.FromResult(new OrderDto(orderId, new OrderAddress("stree1", "postalCode"), 
            new PersonalDetails("name", "surname"), new List<FullProductResponse>()));
    }

    [HttpGet("getorderstatus/{orderId}")]
    public Task<OrderDto> GetOrderStatus(Guid orderId)
    {
        throw new NotImplementedException();
    }
    
    [HttpPost(Name = "createorder")]
    public async Task<ActionResult<Guid>> CreateOrder(CreateOrderDto createOrder)
    {
        OrderDto orderDto = new OrderDto(Guid.NewGuid(), createOrder.orderAddress, createOrder.PersonalDetails,
            createOrder.Products);
        await _domainMessagePublisher.Publish(orderDto, routingKey: "order");
        return new AcceptedResult($"getorderstatus/{orderDto.orderId}", orderDto.orderId);
    }
}

