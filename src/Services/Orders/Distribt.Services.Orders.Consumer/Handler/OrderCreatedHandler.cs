using Distribt.Services.Orders.Dto;

namespace Distribt.Services.Orders.Consumer.Handler;

public class OrderCreatedHandler : IDomainMessageHandler<Order>
{
    public Task Handle(DomainMessage<Order> message, CancellationToken cancelToken = default(CancellationToken))
    {
        Console.WriteLine($"Order {message.Content.orderId} created");
        //TODO: create order flow
        return Task.CompletedTask;
    }
}