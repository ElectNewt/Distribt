using System.Net;
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
    [ProducesResponseType(typeof(ResultDto<bool>), (int)HttpStatusCode.Accepted)]
    public async Task<IActionResult> Subscribe(SubscriptionDto subscription)
    {
        await _integrationMessagePublisher.Publish(subscription);
        return true.Success().ToActionResult();
    }

    [HttpDelete(Name = "unsubscribe")]
    [ProducesResponseType(typeof(ResultDto<bool>), (int)HttpStatusCode.Accepted)]
    public Task<IActionResult> Unsubscribe(SubscriptionDto subscription)
    {
        return true.Success().Async().ToActionResult();
    }
}

