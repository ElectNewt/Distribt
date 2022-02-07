using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Orders.Controllers;
[ApiController]
[Route("[controller]")]
public class OrderController
{
    private readonly IIntegrationMessagePublisher _integrationMessagePublisher;

    public OrderController(IIntegrationMessagePublisher integrationMessagePublisher)
    {
        _integrationMessagePublisher = integrationMessagePublisher;
    }

    [HttpGet("{orderId}")]
    public async Task<OrderDto> GetOrder(Guid orderId)
    {
        OrderDto orderDto = new OrderDto(orderId);
        await _integrationMessagePublisher.Publish(orderDto);
        //TODO: logic
        return orderDto;
    }

    [HttpPost(Name = "addorder")]
    public Task<Guid> AddOrder(OrderDto order)
    {
        //TODO: logic
        return Task.FromResult(Guid.NewGuid());
    }


    //TODO: finish the dto
    //TODO: move
    public record OrderDto(Guid orderId);
}

