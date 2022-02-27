using Distribt.Services.Orders.Dto;

namespace Distribt.Services.Orders.Consumer.Handler;

public class OrderCreatedHandler : IDomainMessageHandler<OrderDto>
{
    public Task Handle(DomainMessage<OrderDto> message, CancellationToken cancelToken = default(CancellationToken))
    {
        Console.WriteLine($"Order {message.Content.orderId} created");
        //TODO: create order flow
        return Task.CompletedTask;
    }
}