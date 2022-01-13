using Distribt.Services.Subscriptions.Dtos;
using Distribt.Shared.Communication.Consumer.Handler;
using Distribt.Shared.Communication.Messages;

namespace Distribt.Services.Subscriptions.Consumer.Handler;

public class UnSubscriptionHandler : IIntegrationMessageHandler<UnSubscriptionDto>
{
    public Task Handle(IntegrationMessage<UnSubscriptionDto> message, CancellationToken cancelToken = default(CancellationToken))
    {
        Console.WriteLine($"the email {message.Content.Email} has unsubscribed.");
        //TODO: Full use case
        return Task.CompletedTask;
    }
}