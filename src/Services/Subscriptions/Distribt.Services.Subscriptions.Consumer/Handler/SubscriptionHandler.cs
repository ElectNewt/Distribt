using Distribt.Services.Subscriptions.Dtos;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;

namespace Distribt.Services.Subscriptions.Consumer.Handler;

public class SubscriptionHandler : IIntegrationMessageHandler<SubscriptionDto>
{
    public Task Handle(IntegrationMessage<SubscriptionDto> message, CancellationToken cancelToken = default(CancellationToken))
    {
       Console.WriteLine($"Email {message.Content.Email} successfully subscribed.");
       //TODO: Full use case
       return Task.CompletedTask;
    }
}