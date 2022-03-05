using Distribt.Services.Subscriptions.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Distribt.Services.Subscriptions.Controllers;

[ApiController]
[Route("[controller]")]
public class SubscriptionController
{
    private readonly IIntegrationMessagePublisher _integrationMessagePublisher;

    public SubscriptionController(IIntegrationMessagePublisher integrationMessagePublisher)
    {
        _integrationMessagePublisher = integrationMessagePublisher;
    }

    [HttpPost(Name = "subscribe")]
    public async Task<bool> Subscribe(SubscriptionDto subscription)
    {
        await _integrationMessagePublisher.Publish(subscription);
        return true;
    }

    [HttpDelete(Name = "unsubscribe")]
    public Task<bool> Unsubscribe(SubscriptionDto subscription)
    {
        //TODO: logic 
        return Task.FromResult(true);
    }
}

